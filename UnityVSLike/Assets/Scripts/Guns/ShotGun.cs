using UnityEngine;

/// <summary>
/// 3. 샷건 클래스 (BaseGun 상속 및 부채꼴 산탄 발사 다형성 구현)
/// </summary>
public class ShotGun : BaseGun
{
    [Header("=== 샷건 전용 특화 설정 ===")]
    public float spreadAngle = 30f;
    public bool showGizmos = true;
    public Color searchColor = Color.green;
    public Color spreadColor = Color.red;
    public float debugRayDuration = 0.5f;

    [SerializeField] private Transform currentTarget;
    public float angleDifference;

    protected override void InitDefaultSettings()
    {
        shotPower = 10f;
        fireInterval = 0.2f;
        searchRadius = 10f;
        if (prefabBullet == null)
        {
            prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/Bullet");
        }
    }

    /// <summary>
    /// 매 프레임 타겟 유효성 검사 및 갱신
    /// </summary>
    protected override void UpdateTarget()
    {
        if (!IsTargetValid(currentTarget, searchRadius))
        {
            currentTarget = FindNearestEnemy(transform.position, searchRadius);
        }
    }

    /// <summary>
    /// 부채꼴 산탄 분산 발사 (다형성 구현)
    /// </summary>
    public override void Shot(Vector3 dir)
    {
        if (prefabBullet == null) return;

        UpdateTarget();

        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (currentTarget != null)
        {
            Vector3 targetDist = currentTarget.position - transform.position;
            Vector2 targetDir = targetDist.normalized;
            angleDifference = Vector2.Angle(dir, targetDir);

            if (angleDifference <= spreadAngle / 2f)
            {
                baseAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
            }
        }

        float halfSpread = spreadAngle / 2f;
        float randomOffset = Random.Range(-halfSpread, halfSpread);
        float finalAngle = baseAngle + randomOffset;

        float rad = finalAngle * Mathf.Deg2Rad;
        Vector2 finalDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        Vector3 spawnPos = GetSpawnPosition();
        if (showGizmos)
        {
            Debug.DrawLine(spawnPos, (Vector2)spawnPos + (finalDir * 5f), spreadColor, debugRayDuration);
        }

        GameObject copyBullet = Instantiate(prefabBullet, spawnPos, Quaternion.identity);
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(finalDir, master, shotPower);
        }
    }

    // 하위 호환 메서드
    public void Shot(Vector3 dir, Player customMaster)
    {
        if (customMaster != null) this.master = customMaster;
        Shot(dir);
    }

    protected override void OnDrawGizmos()
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
        Vector2 playerDir = GetPlayerFacingDirection();
        float baseAngle = Vector2.SignedAngle(Vector2.right, playerDir);
        float halfSpread = spreadAngle / 2f;
        float leftAngle = baseAngle - halfSpread;
        float rightAngle = baseAngle + halfSpread;
        Vector3 leftDir = new Vector3(Mathf.Cos(leftAngle * Mathf.Deg2Rad), Mathf.Sin(leftAngle * Mathf.Deg2Rad), 0f);
        Vector3 rightDir = new Vector3(Mathf.Cos(rightAngle * Mathf.Deg2Rad), Mathf.Sin(rightAngle * Mathf.Deg2Rad), 0f);
        Gizmos.DrawLine(currentPos, currentPos + leftDir * 3f);
        Gizmos.DrawLine(currentPos, currentPos + rightDir * 3f);
    }
}
