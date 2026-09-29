using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 7. 낙뢰 실드 클래스 (BaseGun 상속 및 다중 낙뢰/Fallback 발사 다형성 구현)
/// </summary>
public class LightningShield : BaseGun
{
    [Header("=== 낙뢰 실드 전용 특화 설정 ===")]
    public int maxTargetsPerStrike = 3;
    [HideInInspector] public List<Transform> currentTargets = new List<Transform>();

    // 레거시 프로퍼티 호환
    public GameObject lightningBulletPrefab
    {
        get => prefabBullet;
        set => prefabBullet = value;
    }

    public float detectionRadius
    {
        get => searchRadius;
        set => searchRadius = value;
    }

    public float strikeInterval
    {
        get => fireInterval;
        set => fireInterval = value;
    }

    protected override void InitDefaultSettings()
    {
        shotPower = 10f;
        fireInterval = 1.5f;
        searchRadius = 10f;
        if (prefabBullet == null)
        {
            prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/LightningBullet");
        }
    }

    /// <summary>
    /// 매 프레임 반경 내 최대 N명의 몬스터 타겟 목록 갱신
    /// </summary>
    protected override void UpdateTarget()
    {
        currentTargets.Clear();

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, searchRadius, monsterLayer);
        if (enemies.Length == 0) return;

        int targetCount = Mathf.Min(enemies.Length, maxTargetsPerStrike);
        for (int i = 0; i < targetCount; i++)
        {
            if (enemies[i] != null && enemies[i].gameObject.activeInHierarchy)
            {
                currentTargets.Add(enemies[i].transform);
            }
        }
    }

    /// <summary>
    /// 타겟 위치들에 낙뢰 소환, 적 부재 시 전방 총구 위치에 Fallback 1회 낙뢰 소환 (다형성 구현)
    /// </summary>
    public override void Shot(Vector3 dir)
    {
        if (prefabBullet == null) return;

        UpdateTarget();

        if (currentTargets != null && currentTargets.Count > 0)
        {
            foreach (Transform target in currentTargets)
            {
                if (target == null || !target.gameObject.activeInHierarchy) continue;

                Vector3 spawnPosition = target.position;
                GameObject copyBullet = Instantiate(prefabBullet, spawnPosition, Quaternion.identity);

                LightningBullet bullet = copyBullet.GetComponent<LightningBullet>();
                if (bullet != null)
                {
                    bullet.master = this.master;
                }
            }
        }
        else
        {
            // 주변에 적이 없을 경우 전방 위치에 Fallback 낙뢰 1회 생성
            Vector3 fallbackPos = GetSpawnPosition(dir);
            GameObject copyBullet = Instantiate(prefabBullet, fallbackPos, Quaternion.identity);

            LightningBullet bullet = copyBullet.GetComponent<LightningBullet>();
            if (bullet != null)
            {
                bullet.master = this.master;
            }
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
        Vector3 currentPos = transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(currentPos, searchRadius);

        if (currentTargets != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform target in currentTargets)
            {
                if (target != null)
                {
                    Gizmos.DrawLine(currentPos, target.position);
                    Gizmos.DrawWireSphere(target.position, 0.5f);
                }
            }
        }
    }
}