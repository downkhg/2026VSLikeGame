using UnityEngine;

public class SoccerBullet : Bullet
{
    public float damage = 20f;             // 타격 데미지
    public int maxBounceCount = 5;         // 최대 반사 횟수
    public float lifeTime = 5.0f;          // 최대 생존 시간
    public float rotateSpeed = 360f;       // 축구공 회전 각속도

    [Header("디버그 시각화 설정")]
    public float debugRayDuration = 1.5f;  // 레이 표시 지속 시간 (초)
    public float rayLength = 2.0f;         // 입사/반사 화살표 길이

    private int currentBounceCount = 0; 
    private Vector2 lastVelocity;          // 직전 충돌 전 속도 저장

    // Gizmos 디버그용 필드
    private Vector2 debugHitPoint;
    private Vector2 debugNormalVector;
    private Vector2 debugReflectVector;
    private bool showGizmos = false;

    public override void Init(Vector2 direction, Player master, float customSpeed = -1f)
    {
        base.Init(direction, master, customSpeed);
        currentBounceCount = 0;
        Destroy(gameObject, lifeTime);
        Debug.Log($"[SoccerBullet] 축구공 발사 초기화 완료 | 위치: {transform.position}");
    }

    protected override void Start()
    {
        base.Start();
        if (lifeTime > 0f)
        {
            Destroy(gameObject, lifeTime);
        }
    }

    protected override void Update()
    {
        // 비행 중 회전 효과
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        // 매 프레임 직전 속도 기록
        if (rb != null)
        {
            lastVelocity = rb.linearVelocity;
        }

        base.Update();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        currentBounceCount++;

        // 1. 충돌 접점 및 충돌 표면 법선(Normal) 추출
        ContactPoint2D contact = collision.contacts[0];
        debugHitPoint = contact.point;
        debugNormalVector = contact.normal;

        // 2. 입사각에 따른 반사 벡터 계산
        debugReflectVector = Vector2.Reflect(lastVelocity.normalized, debugNormalVector);

        // 3. 반사 속도 적용
        float currentSpeed = lastVelocity.magnitude > 0.1f ? lastVelocity.magnitude : this.speed;
        if (rb != null)
        {
            rb.linearVelocity = debugReflectVector * currentSpeed;
        }

        showGizmos = true;

        // 디버그 레이 드로우
        // - 파란색: 입사선 (날아온 방향)
        // - 빨간색: 법선 (표면 수직선)
        // - 초록색: 반사선 (튕겨나갈 방향)
        Debug.DrawLine(debugHitPoint - (lastVelocity.normalized * rayLength), debugHitPoint, Color.blue, debugRayDuration);
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