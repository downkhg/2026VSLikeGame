using UnityEngine;

public class SoccerBullet : Bullet
{
    public float damage = 20f;             // 타격 데미지[cite: 7]
    public int maxBounceCount = 5;         // 최대 반사 횟수[cite: 7]
    public float lifeTime = 5.0f;          // 최대 생존 시간[cite: 7]
    public float rotateSpeed = 360f;       // 축구공 회전 연출 속도[cite: 7]

    [Header("디버그 시각화 설정")]
    public float debugRayDuration = 1.5f;  // 디버그 레이 유지 시간 (초)
    public float rayLength = 2.0f;         // 기즈모/레이 화살표 길량

    private int currentBounceCount = 0; 
    private Rigidbody2D rb; 
    private Vector2 lastVelocity;          // 물리 충돌 직전 속도 저장용

    // Gizmos 기치용 변수
    private Vector2 debugHitPoint;
    private Vector2 debugNormalVector;
    private Vector2 debugReflectVector;
    private bool showGizmos = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); 
    }

    private void Start()
    {
        Debug.Log($"[SoccerBullet] 축구공 생성됨 | 위치: {transform.position}"); 
        //Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // 날아가는 동안 회전 연출
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime); 
        
        // 매 프레임 충돌 직전 속도 기록
        lastVelocity = rb.linearVelocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        currentBounceCount++;
        
        // 1. 충돌 지점 및 법선 벡터(Normal) 추출
        ContactPoint2D contact = collision.contacts[0];
        debugHitPoint = contact.point;
        debugNormalVector = contact.normal;

        // 2. 입사각 기반 반사 벡터 계산
        debugReflectVector = Vector2.Reflect(lastVelocity.normalized, debugNormalVector);

        // 3. 반사 속도 적용
        float speed = lastVelocity.magnitude;
        rb.linearVelocity = debugReflectVector * speed;

        // Gizmos 활성화 Flag
        showGizmos = true;

        // 4. [Game View 디버깅] Debug.DrawLine으로 레이 렌더링
        // - 파란색: 입사 벡터 (들어오는 방향)
        // - 빨간색: 법선 벡터 (노말)
        // - 초록색: 반사 벡터 (튀어나가는 방향)
        Debug.DrawLine(debugHitPoint - (lastVelocity.normalized * rayLength), debugHitPoint, Color.blue, debugRayDuration);
        Debug.DrawLine(debugHitPoint, debugHitPoint + (debugNormalVector * rayLength), Color.red, debugRayDuration);
        Debug.DrawLine(debugHitPoint, debugHitPoint + (debugReflectVector * rayLength), Color.green, debugRayDuration);

        // 상세 로그 출력
        Debug.Log($"<color=yellow>[SoccerBullet Bounce]</color> " +
                  $"충돌 대상: {collision.gameObject.name} | " +
                  $"충돌 지점: {debugHitPoint} | " +
                  $"법선(Normal): {debugNormalVector} | " +
                  $"반사(Reflect): {debugReflectVector}");

        // 몬스터 데미지 타격
        if (collision.gameObject.CompareTag("Monster"))
        {
            Debug.Log($"[SoccerBullet] 몬스터 데미지 타격! | 대상: {collision.gameObject.name} | 데미지: {damage}");
            // collision.gameObject.GetComponent<Enemy>()?.TakeDamage(damage, master);[cite: 7]
        }

        if (currentBounceCount >= maxBounceCount)
        {
            Debug.Log("[SoccerBullet] 최대 반사 횟수 도달로 인한 파괴");
            //Destroy(gameObject);
        }
    }

    // [Scene View 디버깅] 기즈모를 이용한 수치 및 방향 시각화
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // 1. 충돌 지점 표시 (노란색 점)
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(debugHitPoint, 0.15f);

        // 2. 법선 벡터 표시 (빨간색 선 - 충돌면 직각 방향)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(debugHitPoint, debugHitPoint + (debugNormalVector * rayLength));

        // 3. 반사 벡터 표시 (초록색 선 - 튕겨나가는 방향)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(debugHitPoint, debugHitPoint + (debugReflectVector * rayLength));
    }
}