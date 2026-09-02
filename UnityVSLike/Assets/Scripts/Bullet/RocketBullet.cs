using UnityEngine;

public class RocketBullet : Bullet
{
    [Header("로켓 추진 & 가속 설정")]
    public float initialSpeed = 2f;      // 발사 초기 속도 (낮게 시작)
    public float maxSpeed = 18f;         // 최고 속도
    public float acceleration = 20f;     // 가속도 (초당 속도 증가량)
    private float currentSpeed;

    [Header("유도 및 회전 설정")]
    public float rotateSpeed = 200f;     // 목표를 향해 회전하는 속도 (높을수록 기민함)
    public float searchRadius = 12f;     // 추적할 적 감지 범위
    public LayerMask monsterLayer;       // 몬스터 레이어
    private Transform targetEnemy;

    [Header("폭발 설정")]
    public float explosionRadius = 3f;   // 폭발 범위
    public float damage = 50f;           // 데미지
    public GameObject explosionEffect;   // 폭발 이펙트 프리팹

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // [로그 1] 객체 초기화 시점 위치
        Debug.Log($"[RocketBullet] 로켓 객체 초기화 (Start) | 현재 객체 위치: {transform.position}");

        currentSpeed = initialSpeed;

        // 발사 시 가장 가까운 적을 타겟으로 지정
        targetEnemy = FindNearestEnemy();
    }

    private void Update()
    {
        // 1. 속도 가속 처리 (초기 속도 -> 최대 속도)
        if (currentSpeed < maxSpeed)
        {
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
        }

        // 2. 타겟이 없거나 파괴되었다면 다시 탐색
        if (targetEnemy == null || !targetEnemy.gameObject.activeInHierarchy)
        {
            targetEnemy = FindNearestEnemy();
        }

        // 3. 타겟을 향해 부드럽게 회전 (유도 기능)
        if (targetEnemy != null)
        {
            Vector2 direction = (Vector2)targetEnemy.position - rb.position;
            direction.Normalize();

            // 현재 바라보는 방향과 목표 방향 사이의 각도 차이 계산
            float rotateAmount = Vector3.Cross(direction, transform.right).z;

            // 로켓의 회전 처리
            rb.angularVelocity = -rotateAmount * rotateSpeed;
        }
        else
        {
            // 타겟이 없으면 회전 정지 (직진)
            rb.angularVelocity = 0f;
        }

        // 4. 로켓이 바라보는 전방(right) 방향으로 가속 이동
        rb.linearVelocity = transform.right * currentSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // [로그 2] 충돌 감지 로그 (충돌한 대상 정보 및 위치 출력)
        Debug.Log($"[RocketBullet] 충돌 발생! | 충돌 대상: {collision.gameObject.name} | 태그: {collision.tag} | 충돌 위치: {transform.position}");

        // 몬스터나 벽에 충돌 시 폭발
        // if (collision.CompareTag("Monster") || collision.CompareTag("Wall"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        // [로그 3] 폭발 로직 진입 로그
        Debug.Log($"[RocketBullet] Explode() 실행 | 폭발 위치: {transform.position}");

        // 폭발 이펙트 생성
        if (explosionEffect != null)
        {
            GameObject effectObj = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effectObj, 0.5f); // 이펙트는 2초 후 제거
            // [로그 4] 폭발 이펙트 생성 성공 로그
            Debug.Log($"[RocketBullet] 폭발 이펙트 생성 완료! | 이펙트 이름: {effectObj.name} | 생성 위치: {effectObj.transform.position}");
        }
        else
        {
            // [경고 로그] 이펙트 프리팹이 비어있는 경우
            Debug.LogWarning("[RocketBullet] explosionEffect 프리팹이 할당되어 있지 않습니다.");
        }

        // 범위 내 적들에게 데미지 전달
        Collider2D[] hitMonsters = Physics2D.OverlapCircleAll(transform.position, explosionRadius, monsterLayer);

        // [로그 5] 폭발 범위 내 감지된 적 개수 출력
        Debug.Log($"[RocketBullet] 폭발 범위 내 감지된 적 수: {hitMonsters.Length}개 (범위: {explosionRadius})");

        foreach (var monster in hitMonsters)
        {
            // TODO: monster.GetComponent<Enemy>()?.TakeDamage(damage, master);
        }

        Destroy(gameObject);
    }

    private Transform FindNearestEnemy()
    {
        Collider2D[] monsters = Physics2D.OverlapCircleAll(transform.position, searchRadius, monsterLayer);
        if (monsters.Length == 0) return null;

        Transform nearest = null;
        float minDistance = float.MaxValue;

        foreach (var monster in monsters)
        {
            float dist = Vector3.Distance(transform.position, monster.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = monster.transform;
            }
        }

        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        // 폭발 범위 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        // 유도 감지 범위 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}