using UnityEngine;

public class SoccerBullet : Bullet
{
    public float damage = 20f;             // 타격 데미지
    public int maxBounceCount = 5;         // 최대 반사 횟수
    public float lifeTime = 5.0f;          // 최대 생존 시간
    public float rotateSpeed = 360f;       // 축구공 회전 각속도

    [Header("충돌 및 위치 보정 설정")]
    public float bulletRadius = 0.2f;      // 축구공 반지름 (벽 내부 갇힘 방지용)
    public float skinWidth = 0.05f;        // 추가 보정 여유값

    [Header("디버그 시각화 설정")]
    public float debugRayDuration = 1.5f;  // 레이 표시 지속 시간 (초)
    public float rayLength = 2.0f;         // 입사/반사 화살표 길이

    private int currentBounceCount = 0;
    private Vector2 lastVelocity;          // 충돌 직전 정상 물리 속도 저장

    // Gizmos 디버그용 필드
    private Vector2 debugHitPoint;
    private Vector2 debugNormalVector;
    private Vector2 debugReflectVector;
    private bool showGizmos = false;

    public override void Init(Vector2 direction, Player master, float customSpeed = -1f)
    {
        base.Init(direction, master, customSpeed);
        currentBounceCount = 0;

        // [수정 1] Continuous 설정으로 고속 이동 시 터널링(벽 뚫기) 방지
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        Destroy(gameObject, lifeTime);
        Debug.Log($"[SoccerBullet] 축구공 발사 초기화 완료 | 위치: {transform.position}");
    }

    protected override void Start()
    {
        base.Start();

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        if (lifeTime > 0f)
        {
            Destroy(gameObject, lifeTime);
        }
    }

    protected override void Update()
    {
        // 비행 중 회전 효과
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        base.Update();
    }

    private void FixedUpdate()
    {
        // [수정 2] 물리 연산 전의 실제 이동 속도를 안정적으로 기록
        // (OnCollisionEnter2D 시점에 속도가 꺾이거나 0이 되는 문제 방지)
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            lastVelocity = rb.linearVelocity;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        currentBounceCount++;

        // 1. 충돌 접점 및 충돌 표면 법선(Normal) 추출
        ContactPoint2D contact = collision.contacts[0];
        debugHitPoint = contact.point;
        debugNormalVector = contact.normal;

        // 2. [수정 3 - 핵심] 벽 내부로 파고든 공의 위치를 법선 방향(벽 표면 밖)으로 즉시 강제 이동
        Vector2 correctedPosition = contact.point + (contact.normal * (bulletRadius + skinWidth));
        transform.position = correctedPosition;
        if (rb != null)
        {
            rb.position = correctedPosition;
        }

        // 3. 입사각에 따른 반사 벡터 계산
        Vector2 inDirection = lastVelocity.sqrMagnitude > 0.01f ? lastVelocity.normalized : transform.up;
        debugReflectVector = Vector2.Reflect(inDirection, debugNormalVector);

        // 4. 반사 속도 적용
        float currentSpeed = lastVelocity.magnitude > 0.1f ? lastVelocity.magnitude : this.speed;
        if (rb != null)
        {
            rb.linearVelocity = debugReflectVector * currentSpeed;
        }

        showGizmos = true;

        // 디버그 레이 드로우
        Debug.DrawLine(debugHitPoint - (inDirection * rayLength), debugHitPoint, Color.blue, debugRayDuration);
        Debug.DrawLine(debugHitPoint, debugHitPoint + (debugNormalVector * rayLength), Color.red, debugRayDuration);
        Debug.DrawLine(debugHitPoint, debugHitPoint + (debugReflectVector * rayLength), Color.green, debugRayDuration);

        // 로그 출력
        Debug.Log($"<color=yellow>[SoccerBullet Bounce]</color> " +
                  $"충돌 대상: {collision.gameObject.name} | " +
                  $"충돌 위치: {debugHitPoint} | " +
                  $"법선(Normal): {debugNormalVector} | " +
                  $"반사(Reflect): {debugReflectVector}");

        // 몬스터 충돌 시 데미지 처리
        if (collision.gameObject.CompareTag("Monster"))
        {
            Player target = collision.gameObject.GetComponent<Player>();
            if (target != null)
            {
                OnHitMonster(target);
            }
        }

        // 최대 바운스 횟수 도달 시 파괴
        if (currentBounceCount >= maxBounceCount)
        {
            Debug.Log("[SoccerBullet] 최대 반사 횟수 도달로 인한 파괴");
            Destroy(gameObject);
        }
    }

    // 에디터 씬 뷰 시각화
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // 1. 충돌 지점 표시 (노란 구)
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(debugHitPoint, 0.15f);

        // 2. 법선 벡터 표시 (빨간 선)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(debugHitPoint, debugHitPoint + (debugNormalVector * rayLength));

        // 3. 반사각 벡터 표시 (초록 선)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(debugHitPoint, debugHitPoint + (debugReflectVector * rayLength));
    }
}
