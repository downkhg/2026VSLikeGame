using UnityEngine;

/// <summary>
/// 1. 기본 권총 클래스 (BaseGun 상속 및 Shot 다형성 구현)
/// </summary>
public class DefaultGun : BaseGun
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
    /// 기본 직진 탄환 발사 로직 (다형성 구현)
    /// </summary>
    public override void Shot(Vector3 dir)
    {
        if (prefabBullet == null) return;

        Vector3 spawnPos = GetSpawnPosition(dir);
        GameObject copyBullet = Instantiate(prefabBullet, spawnPos, Quaternion.identity);
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(dir, master, shotPower);
        }
    }
}
