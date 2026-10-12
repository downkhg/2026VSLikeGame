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
    /// 타겟팅된 적이 있다면 타겟 방향으로, 없다면 전달받은 방향(dir)으로 탄환 발사 (다형성 구현)
    /// </summary>
    public override void Shot(Vector3 dir)
    {
        if (prefabBullet == null) return;

        // 바라보는 방향과 상관없이 반경 내 타겟팅된 적이 있다면 해당 적으로 조준
        Transform target = FindNearestEnemy(transform.position, searchRadius);
        Vector3 spawnPos = GetSpawnPosition(dir);
        Vector3 finalDir = dir;
        if (target != null)
        {
            finalDir = (target.position - spawnPos).normalized;
        }

        GameObject copyBullet = Instantiate(prefabBullet, spawnPos, Quaternion.identity);
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(finalDir, master, shotPower);
        }
    }
}
