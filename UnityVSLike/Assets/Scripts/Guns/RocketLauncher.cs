using UnityEngine;

/// <summary>
/// 4. 로켓 런처 클래스 (BaseGun 상속 및 로켓 발사 다형성 구현)
/// </summary>
public class RocketLauncher : BaseGun
{
    // 레거시 프로퍼티 호환
    public GameObject prefabRocketBullet
    {
        get => prefabBullet;
        set => prefabBullet = value;
    }

    public float launchForce
    {
        get => shotPower;
        set => shotPower = value;
    }

    protected override void InitDefaultSettings()
    {
        shotPower = 15f;
        fireInterval = 1.5f;
        searchRadius = 10f;
        if (prefabBullet == null)
        {
            prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/RocketBullet");
        }
    }

    /// <summary>
    /// 가장 가까운 적 방향으로 로켓 탄환 발사 (다형성 구현)
    /// </summary>
    public override void Shot(Vector3 dir)
    {
        if (prefabBullet == null) return;

        Vector3 launchPosition = GetSpawnPosition();
        Transform nearestEnemy = FindNearestEnemy(transform.position, searchRadius);
        Vector2 launchDir = nearestEnemy != null ? (Vector2)(nearestEnemy.position - launchPosition).normalized : (Vector2)dir;

        GameObject rocketObj = Instantiate(prefabBullet, launchPosition, Quaternion.identity);
        Bullet bulletScript = rocketObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Init(launchDir, master, shotPower);
        }
    }

    // 하위 호환 메서드
    public void Shot(Player customMaster)
    {
        if (customMaster != null) this.master = customMaster;
        Shot((Vector3)GetPlayerFacingDirection());
    }

    protected override void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}