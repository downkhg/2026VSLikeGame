using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotGun : MonoBehaviour
{
    public GameObject prefabBullet;
    public float ShotPower;
    public float searchRadius = 10f; // 적을 탐색할 최대 반경

    [Header("샷건 전용 설정")]
    public float spreadAngle = 30f;  // 탄환이 퍼질 수 있는 총 각도 범위 (예: 30도)

    [Header("발사 설정")]
    public float fireInterval = 0.2f; // 단발 연사 주기에 맞게 조정
    [SerializeField] private float fireTimer = 0f;

    public Player master;

    [Header("타겟팅 설정")]
    [SerializeField] private Transform currentTarget; // 현재 유효한 타겟 저장

    [Header("디버그 기즈모 설정")]
    public bool showGizmos = true;
    public Color searchColor = Color.green; // 탐색 반경 색상
    public Color spreadColor = Color.red;   // 부채꼴 발사 범위 색상
    public float debugRayDuration = 0.5f;   // Debug.DrawLine 레이 표시 지속 시간

    void Update()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            Shot(master);
        }
    }

    // 1. 특정 타겟(적)을 기준으로 발사 판단 로직 적용
    public void Shot(Transform target, Player master)
    {
        // 기준 각도: 무기가 현재 바라보는 방향 (기본: transform.right)
        float baseAngle = Mathf.Atan2(transform.right.y, transform.right.x) * Mathf.Rad2Deg;

        if (target != null)
        {
            // 플레이어/무기 기준 타겟까지의 방향 벡터 및 각도 계산
            Vector2 targetDir = (target.position - transform.position).normalized;
            float targetAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

            // 현재 바라보는 방향과 타겟 방향 사이의 차이 각도 계산 (0 ~ 180도)
            float angleDifference = Mathf.Abs(Mathf.DeltaAngle(baseAngle, targetAngle));

            // 조건: 몬스터와의 각도 차이가 설정된 발사 반경(spreadAngle / 2) 이내일 때만 타겟 방향 기반으로 발사
            if (angleDifference <= spreadAngle / 2f)
            {
                baseAngle = targetAngle;
            }
            // 몬스터 방향과의 각도가 설정된 각도보다 크다면 baseAngle을 유지하여 전방 반경(spreadAngle) 내 랜덤 발사
        }

        FireSingleSpreadBullet(baseAngle, master);
    }

    // 2. 타겟 유지 로직이 적용된 메인 발사 함수
    public void Shot(Player master)
    {
        // 타겟 유지 검증
        if (!IsTargetValid(currentTarget))
        {
            currentTarget = FindNearestEnemy();
        }

        // 타겟 존재 유무와 상관없이 Shot(Transform, Player) 내부에서 각도 판별 처리
        Shot(currentTarget, master);
    }

    // 3. 방향 벡터(Vector3) 기반 발사 오버로드
    public void Shot(Vector3 dir, Player master)
    {
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        FireSingleSpreadBullet(baseAngle, master);
    }

    bool IsTargetValid(Transform target)
    {
        if (target == null) return false;
        if (!target.gameObject.activeInHierarchy) return false;

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > searchRadius) return false;

        return true;
    }

    void FireSingleSpreadBullet(float baseAngle, Player master)
    {
        float halfSpread = spreadAngle / 2f;
        float randomOffset = Random.Range(-halfSpread, halfSpread);
        float finalAngle = baseAngle + randomOffset;

        SpawnSingleBullet(finalAngle, master);
    }

    void SpawnSingleBullet(float angleZ, Player master)
    {
        float rad = angleZ * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        if (showGizmos)
        {
            Debug.DrawLine(transform.position, (Vector2)transform.position + dir * 5f, spreadColor, debugRayDuration);
        }

        GameObject copyBullet = Instantiate(prefabBullet, transform.position, Quaternion.identity);
        Rigidbody2D rigidbody = copyBullet.GetComponent<Rigidbody2D>();
        Bullet bullet = copyBullet.GetComponent<Bullet>();

        if (bullet != null)
        {
            bullet.master = master;
        }

        if (rigidbody != null)
        {
            rigidbody.AddForce(dir * ShotPower, ForceMode2D.Impulse);
        }

        copyBullet.transform.rotation = Quaternion.AngleAxis(angleZ, Vector3.forward);
    }

    Transform FindNearestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(this.transform.position, searchRadius, 1 << LayerMask.NameToLayer("Monster"));
        Transform nearest = null;
        float minDistance = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (Collider2D enemy in enemies)
        {
            float dist = Vector3.Distance(currentPos, enemy.transform.position);
            if (dist < minDistance && dist <= searchRadius)
            {
                minDistance = dist;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Vector3 currentPos = transform.position;

        Gizmos.color = searchColor;
        Gizmos.DrawWireSphere(currentPos, searchRadius);

        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(currentPos, currentTarget.position);
        }

        Gizmos.color = spreadColor;

        float baseAngle = Mathf.Atan2(transform.right.y, transform.right.x) * Mathf.Rad2Deg;
        float halfSpread = spreadAngle / 2f;

        float leftAngle = baseAngle - halfSpread;
        float rightAngle = baseAngle + halfSpread;

        Vector3 leftDir = new Vector3(Mathf.Cos(leftAngle * Mathf.Deg2Rad), Mathf.Sin(leftAngle * Mathf.Deg2Rad), 0f);
        Vector3 rightDir = new Vector3(Mathf.Cos(rightAngle * Mathf.Deg2Rad), Mathf.Sin(rightAngle * Mathf.Deg2Rad), 0f);

        Gizmos.DrawLine(currentPos, currentPos + leftDir * 3f);
        Gizmos.DrawLine(currentPos, currentPos + rightDir * 3f);
        Gizmos.DrawLine(currentPos + leftDir * 3f, currentPos + rightDir * 3f);
    }
}