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

    protected override void Awake()
    {
        base.Awake();
        currentSpeed = initialSpeed;
    }

    public override void Init(Vector2 direction, Player master, float customSpeed = -1f)
    {
        this.master = master;
        vStart = transform.position;
        currentSpeed = initialSpeed;

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        // 초기 발사 방향 정렬
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        if (rb != null)
        {
            rb.linearVelocity = transform.right * currentSpeed;
        }

        targetEnemy = FindNearestEnemy();
        Debug.Log($"[RocketBullet] 로켓 발사 초기화 완료 | 타겟: {(targetEnemy != null ? targetEnemy.name : "없음")}");
    }

    protected override void Start()
    {
        base.Start();
        if (targetEnemy == null)
        {
            targetEnemy = FindNearestEnemy();
        }
    }

    protected override void Update()
    {
        // 1. 속도 가속 처리 (초기 속도 -> 최대 속도)
        if (currentSpeed < maxSpeed)
        {
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
        }

        // 2. 타겟이 없거나 비활성화되었다면 다시 탐색
        if (targetEnemy == null || !targetEnemy.gameObject.activeInHierarchy)
        {
            targetEnemy = FindNearestEnemy();
        }

        // 3. 타겟을 향해 부드럽게 회전 (유도 기능)
        if (rb != null)
        {
            if (targetEnemy != null)
            {
                Vector2 direction = (Vector2)targetEnemy.position - rb.position;
                direction.Normalize();

                // 현재 바라보는 방향과 목표 방향 사이의 각도 차이 계산
                float rotateAmount = Vector3.Cross(direction, transform.right).z;
                rb.angularVelocity = -rotateAmount * rotateSpeed;
            }
            else
            {
                rb.angularVelocity = 0f;
            }

            // 4. 로켓이 바라보는 전방(right) 방향으로 가속 이동
            rb.linearVelocity = transform.right * currentSpeed;
        }

        // 사거리 체크 소멸 (부모 Update 호출)
        base.Update();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[RocketBullet] 충돌 발생! | 충돌 대상: {collision.gameObject.name} | 태그: {collision.tag} | 충돌 위치: {transform.position}");
        Explode();
    }

    private void Explode()
    {
        Debug.Log($"[RocketBullet] Explode() 실행 | 폭발 위치: {transform.position}");

        // 폭발 이펙트 생성
        if (explosionEffect != null)
        {
            GameObject effectObj = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            ExplosionEffect effectComp = effectObj.GetComponent<ExplosionEffect>();
            if (effectComp != null)
            {
                effectComp.master = master;
                effectComp.damage = damage;
            }
            Destroy(effectObj, 0.5f);
            Debug.Log($"[RocketBullet] 폭발 이펙트 생성 완료! | 이펙트 이름: {effectObj.name}");
        }
        else
        {
            Debug.LogWarning("[RocketBullet] explosionEffect 프리팹이 할당되어 있지 않습니다.");
        }

        // 범위 내 적들에게 데미지 전달
        Collider2D[] hitMonsters = Physics2D.OverlapCircleAll(transform.position, explosionRadius, monsterLayer);
        Debug.Log($"[RocketBullet] 폭발 범위 내 감지된 적 수: {hitMonsters.Length}개 (범위: {explosionRadius})");

        foreach (var monsterCol in hitMonsters)
        {
            Player target = monsterCol.GetComponent<Player>();
            if (target != null)
            {
                OnHitMonster(target);
            }
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