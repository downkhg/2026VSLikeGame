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

    // 1. 특정 타겟(적)을 기준으로 범위 내 임의의 각도로 1발 발사
    public void Shot(Transform target, Player master)
    {
        if (target == null)
        {
            Debug.LogWarning("[Shotgun] Shot 실패: 타겟(Target)이 null입니다.");
            return;
        }

        Vector2 targetDir = (target.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

        FireSingleSpreadBullet(baseAngle, master);
    }

    // 2. 타겟 유지 로직이 적용된 메인 발사 함수
    public void Shot(Player master)
    {
        // 💡 타겟 유지 검증: 기존 타겟이 유효한지 확인
        if (!IsTargetValid(currentTarget))
        {
            // 기존 타겟이 죽었거나 없거나 범위를 벗어났으면 새로운 적 탐색
            currentTarget = FindNearestEnemy();
        }

        if (currentTarget != null)
        {
            Shot(currentTarget, master);
        }
        else
        {
            // 주변에 적이 없을 때는 전방으로 발사
            float baseAngle = Mathf.Atan2(transform.right.y, transform.right.x) * Mathf.Rad2Deg;
            FireSingleSpreadBullet(baseAngle, master);
        }
    }

    // 3. 방향 벡터(Vector3) 기반 발사 오버로드
    public void Shot(Vector3 dir, Player master)
    {
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        FireSingleSpreadBullet(baseAngle, master);
    }

    // 현재 타겟이 살아있고 탐색 범위 내에 있는지 검증하는 헬퍼 함수
    bool IsTargetValid(Transform target)
    {
        // 1. Unity에서는 파괴된 GameObject(적)는 null 체크 시 true를 반환합니다.
        if (target == null) return false;

        // 2. 해당 오브젝트가 비활성화(Die) 처리되었는지 확인
        if (!target.gameObject.activeInHierarchy) return false;

        // 3. 탐색 반경(searchRadius)을 벗어났는지 확인
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > searchRadius) return false;

        return true;
    }

    // 지정된 범위(spreadAngle) 내에서 임의의 각도를 추출하여 단 1발 발사
    void FireSingleSpreadBullet(float baseAngle, Player master)
    {
        float halfSpread = spreadAngle / 2f;
        float randomOffset = Random.Range(-halfSpread, halfSpread);
        float finalAngle = baseAngle + randomOffset;

        SpawnSingleBullet(finalAngle, master);
    }

    // 단발 탄환을 특정 각도로 생성하고 힘을 가하는 함수
    void SpawnSingleBullet(float angleZ, Player master)
    {
        float rad = angleZ * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        // 발사할 때마다 선택된 각도 방향으로 디버그 드로우 렌더링
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

        // 1. 적 탐색 반경 (초록색 원)
        Gizmos.color = searchColor;
        Gizmos.DrawWireSphere(currentPos, searchRadius);

        // 2. 현재 타겟이 있다면 타겟을 향한 빨간색 연결선 표시
        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(currentPos, currentTarget.position);
        }

        // 3. 랜덤 발사 각도가 정해지는 부채꼴 영역 전체 (빨간색)
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