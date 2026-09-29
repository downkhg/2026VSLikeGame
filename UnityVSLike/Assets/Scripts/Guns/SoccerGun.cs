using UnityEngine;

/// <summary>
/// 5. 축구공 건 클래스 (BaseGun 상속 및 바운스 축구공 발사 다형성 구현)
/// </summary>
public class SoccerGun : BaseGun
{
    // 레거시 프로퍼티 호환
    public GameObject prefabSoccerBullet
    {
        get => prefabBullet;
        set => prefabBullet = value;
    }

    protected override void InitDefaultSettings()
    {
        shotPower = 12f;
        fireInterval = 2.0f;
        searchRadius = 10f;
        if (prefabBullet == null)
        {
            prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/SoccerBullet");
        }
    }

    /// <summary>
    /// 가장 가까운 적 방향으로 축구공 발사 (다형성 구현)
    /// </summary>
    public override void Shot(Vector3 dir)
    {
        if (prefabBullet == null) return;

        Vector3 spawnPos = GetSpawnPosition();
        Transform target = FindNearestEnemy(transform.position, searchRadius);
        Vector2 finalDir = target != null ? (Vector2)(target.position - spawnPos).normalized : (Vector2)dir;

        GameObject soccerObj = Instantiate(prefabBullet, spawnPos, Quaternion.identity);
        SoccerBullet bullet = soccerObj.GetComponent<SoccerBullet>();
        if (bullet != null)
        {
            bullet.Init(finalDir, master, shotPower);
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}