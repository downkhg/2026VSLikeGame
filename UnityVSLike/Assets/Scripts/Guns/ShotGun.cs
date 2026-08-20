using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotGun : MonoBehaviour
{
    public GameObject prefabBullet;
    public float ShotPower;
    public float searchRadius = 10f; // 적을 탐색할 최대 반경

    [Header("샷건 전용 설정")]
    public int bulletCount = 5;      // 한 번에 발사할 펠렛(탄환) 개수
    public float spreadAngle = 30f;  // 탄환들이 퍼지는 총 각도 범위 (예: 30도)

    [Header("발사 설정")]
    public float fireInterval = 1.0f; // 샷건은 연사력이 낮으므로 조금 길게 설정
    [SerializeField] private float fireTimer = 0f;

    public Player master;

    [Header("디버그 기즈모 설정")]
    public bool showGizmos = true;
    public Color searchColor = Color.green; // 탐색 반경 색상
    public Color spreadColor = Color.red;   // 부채꼴 발사 범위 색상

    void Update()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            Debug.Log("[Shotgun] 발사 주기 도달! 샷건 발사를 시작합니다.");
            Shot(master);
        }
    }

    // 1. 특정 타겟(적)을 기준으로 부채꼴 다발 발사
    public void Shot(Transform target, Player master)
    {
        if (target == null)
        {
            Debug.LogWarning("[Shotgun] Shot 실패: 타겟(Target)이 null입니다.");
            return;
        }

        Debug.Log($"[Shotgun] 타겟 발견! 이름: {target.name}, 위치: {target.position}");

        // 타겟을 향하는 기본 방향 벡터 및 각도 계산
        Vector2 targetDir = (target.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

        // 다발 발사 (부채꼴 분산)
        FireSpreadBullets(baseAngle, master);
    }

    // 2. 타겟이 없을 때 가장 가까운 적을 찾거나 전방으로 부채꼴 발사
    public void Shot(Player master)
    {
        Transform nearestEnemy = FindNearestEnemy();

        if (nearestEnemy != null)
        {
            Shot(nearestEnemy, master);
        }
        else
        {
            Debug.Log("[Shotgun] 주변에 적이 없습니다. 정면(transform.right)을 기준으로 발사합니다.");
            float baseAngle = Mathf.Atan2(transform.right.y, transform.right.x) * Mathf.Rad2Deg;
            FireSpreadBullets(baseAngle, master);
        }
    }

    // 3. 방향 벡터(Vector3) 기반 발사 오버로드
    public void Shot(Vector3 dir, Player master)
    {
        Debug.Log($"[Shotgun] 방향 벡터 기반 발사 실행 | 기준 방향: {dir}");
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        FireSpreadBullets(baseAngle, master);
    }

    // [핵심] 여러 발의 탄환을 각도별로 계산하여 생성하는 함수
    void FireSpreadBullets(float baseAngle, Player master)
    {
        // 탄환이 1발이거나 분산 각도가 0이면 그냥 1발만 발사
        if (bulletCount <= 1 || spreadAngle <= 0f)
        {
            SpawnSingleBullet(baseAngle, master);
            return;
        }

        // 각 탄환 사이의 각도 간격 계산
        float halfSpread = spreadAngle / 2f;
        float angleStep = spreadAngle / (bulletCount - 1);

        Debug.Log($"[Shotgun] 펠렛 총 {bulletCount} 발사 시작 (기준 각도: {baseAngle:F2}°, 분산 범위: {spreadAngle}°)");

        for (int i = 0; i < bulletCount; i++)
        {
            // 좌측 끝(-halfSpread)부터 우측 끝(+halfSpread)까지 순차적으로 각도 배분
            float currentAngle = (baseAngle - halfSpread) + (angleStep * i);
            SpawnSingleBullet(currentAngle, master);
        }
    }

    // 단발 탄환을 특정 각도로 생성하고 힘을 가하는 함수
    void SpawnSingleBullet(float angleZ, Player master)
    {
        // 각도를 방향 벡터로 변환
        float rad = angleZ * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        GameObject copyBullet = Instantiate(prefabBullet, transform.position, Quaternion.identity);
        Rigidbody2D rigidbody = copyBullet.GetComponent<Rigidbody2D>();
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        Debug.Log($"[Shotgun]:{copyBullet.transform.position} 펠렛복사");

        if (bullet != null)
        {
            bullet.master = master;
        }

        if (rigidbody != null)
        {
            rigidbody.AddForce(dir * ShotPower, ForceMode2D.Impulse);
        }

        // 탄환 회전값 설정
        copyBullet.transform.rotation = Quaternion.AngleAxis(angleZ, Vector3.forward);

        Debug.Log($"[Shotgun] 펠렛 생성 완료 | 각도: {angleZ:F2}° | 방향: {dir}");
    }

    // 가장 가까운 적을 찾는 헬퍼 함수
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

    // --- 💡 기즈모(Gizmos) 시각화 기능 ---
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Vector3 currentPos = transform.position;

        // 1. 적 탐색 반경 그리기 (초록색 원)[cite: 1]
        Gizmos.color = searchColor;
        Gizmos.DrawWireSphere(currentPos, searchRadius);

        // 2. 부채꼴 발사 범위 그리기 (빨간색 선)
        Gizmos.color = spreadColor;

        float baseAngle = Mathf.Atan2(transform.right.y, transform.right.x) * Mathf.Rad2Deg;
        float halfSpread = spreadAngle / 2f;

        float leftAngle = baseAngle - halfSpread;
        float rightAngle = baseAngle + halfSpread;

        Vector3 leftDir = new Vector3(Mathf.Cos(leftAngle * Mathf.Deg2Rad), Mathf.Sin(leftAngle * Mathf.Deg2Rad), 0f);
        Vector3 rightDir = new Vector3(Mathf.Cos(rightAngle * Mathf.Deg2Rad), Mathf.Sin(rightAngle * Mathf.Deg2Rad), 0f);

        // 범위의 양 끝단 직선 그리기 (길이 3f)
        Gizmos.DrawLine(currentPos, currentPos + leftDir * 3f);
        Gizmos.DrawLine(currentPos, currentPos + rightDir * 3f);
        Gizmos.DrawLine(currentPos + leftDir * 3f, currentPos + rightDir * 3f);
    }
}