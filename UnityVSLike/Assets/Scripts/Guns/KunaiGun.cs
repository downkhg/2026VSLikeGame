using UnityEngine;

/// <summary>
/// 2. 쿠나이 건 클래스 (BaseGun 상속 및 타겟팅 조준 Shot 다형성 구현)
/// </summary>
public class KunaiGun : BaseGun
{
    protected override void InitDefaultSettings()
    {
        shotPower = 10f;
        fireInterval = 0.5f;
        searchRadius = 10f;
        if (prefabBullet == null)
        {
            prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/Bullet");
        }
    }

    /// <summary>
    /// 가장 가까운 적을 탐색하여 조준 발사 (다형성 구현)
    /// </summary>
    public override void Shot(Vector3 dir)
    {
        if (prefabBullet == null) return;

        Transform nearestEnemy = FindNearestEnemy(transform.position, searchRadius);
        Vector3 spawnPos = GetSpawnPosition();
        Vector2 finalDir = nearestEnemy != null ? (Vector2)(nearestEnemy.position - spawnPos).normalized : (Vector2)dir;

        GameObject copyBullet = Instantiate(prefabBullet, spawnPos, Quaternion.identity);
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(finalDir, master, shotPower);
        }
    }

    // 하위 호환성 메서드들
    public void Shot(Transform target, Player customMaster = null)
    {
        if (customMaster != null) this.master = customMaster;
        if (target == null) return;
        Vector3 spawnPos = GetSpawnPosition();
        Vector2 dir = (target.position - spawnPos).normalized;
        Shot(dir);
    }

    public void Shot(Player customMaster)
    {
        if (customMaster != null) this.master = customMaster;
        Shot((Vector3)GetPlayerFacingDirection());
    }

    protected override void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}