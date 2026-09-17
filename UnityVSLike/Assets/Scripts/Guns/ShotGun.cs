using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotGun : MonoBehaviour
{
    public GameObject prefabBullet;
    public float ShotPower = 10f;
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

    public float angleDifference;

    private void Awake()
    {
        if (master == null)
        {
            master = GetComponentInParent<Player>();
        }
    }

    void Update()
    {
        fireTimer += Time.deltaTime;

        // 매 프레임 타겟 유효성 검사 및 탐색
        UpdateTarget();

        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
        }
    }

    // 타겟 갱신 메서드
    private void UpdateTarget()
    {
        if (!IsTargetValid(currentTarget))
        {
            currentTarget = FindNearestEnemy();
        }
    }

    // 플레이어 또는 무기의 실제 바라보는 방향 Vector2 추출
    private Vector2 GetPlayerFacingDirection()
    {
        if (master != null && master.transform.localScale.x < 0)
        {
            return Vector2.left;
        }

        if (Mathf.Abs(transform.right.x) > 0.01f || Mathf.Abs(transform.right.y) > 0.01f)
        {
            return transform.right;
        }

        return Vector2.right;
    }

    // 메인 발사 진입점
    public void Shot(Vector3 dir, Player master)
    {
        if (master == null || prefabBullet == null) return;

        UpdateTarget();

        // 1. 기본 발사 기준 각도 (바라보는 방향)
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 2. 타겟이 존재하고 정면 부채꼴 범위 내에 있는지 확인
        if (currentTarget != null)
        {
            Vector3 targetDist = currentTarget.position - transform.position;
            Vector2 targetDir = targetDist.normalized;

            angleDifference = Vector2.Angle(dir, targetDir);

            // 사이각이 spreadAngle 절반 이하인 경우 적 위치를 기준으로 조준
            if (angleDifference <= spreadAngle / 2f)
            {
                baseAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
                Debug.Log($"[Shotgun Action] 🎯 적 조준 발사 (기준 각도: {baseAngle:F1}°, 사이각: {angleDifference:F1}°)");
            }
            else
            {
                Debug.Log($"[Shotgun Action] 🔄 타겟이 조준 범위를 벗어남({angleDifference:F1}°) -> 정면 기준 발사 ({baseAngle:F1}°)");
            }
        }
        else
        {
            Debug.Log($"[Shotgun Check] 타겟 없음 -> 정면 기준 발사 ({baseAngle:F1}°)");
        }

        // 3. 조준 여부와 상관없이 항상 부채꼴(spreadAngle) 범위 내에서 랜덤 분산 각도 적용
        float halfSpread = spreadAngle / 2f;
        float randomOffset = Random.Range(-halfSpread, halfSpread);
        float finalAngle = baseAngle + randomOffset;

        float rad = finalAngle * Mathf.Deg2Rad;
        Vector2 finalDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        // 4. 디버그 레이 드로우
        if (showGizmos)
        {
            Debug.DrawLine(transform.position, (Vector2)transform.position + finalDir * 5f, spreadColor, debugRayDuration);
        }

        // 5. 탄환 생성 및 Rigidbody2D 물리 초기화
        GameObject copyBullet = Instantiate(prefabBullet, transform.position, Quaternion.identity);
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(finalDir, master, ShotPower);
        }
    }

    bool IsTargetValid(Transform target)
    {
        if (target == null) return false;
        if (!target.gameObject.activeInHierarchy) return false;

        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= searchRadius;
    }

    Transform FindNearestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, searchRadius, 1 << LayerMask.NameToLayer("Monster"));
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

        // 1. 탐색 범위 (초록 원)
        Gizmos.color = searchColor;
        Gizmos.DrawWireSphere(currentPos, searchRadius);

        // 2. 타겟 연결선 (노란 선)
        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(currentPos, currentTarget.position);
        }

        // 3. 부채꼴 드로우 (빨간 선)
        Gizmos.color = spreadColor;
        Vector2 playerDir = GetPlayerFacingDirection();
        float baseAngle = Vector2.SignedAngle(Vector2.right, playerDir);
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