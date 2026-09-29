using UnityEngine;

/// <summary>
/// 6. 블록 포물선 건 클래스 (BaseGun 상속 및 역탄도 포물선 발사 다형성 구현)
/// </summary>
public class BlockGun : BaseGun
{
    [Header("=== 블록건 전용 특화 설정 ===")]
    public float flightTime = 1.0f;
    public Transform defaultTarget;
    [HideInInspector] public Transform targetTransform = null;

    // 레거시 프로퍼티 호환
    public GameObject prefabBlockBullet
    {
        get => prefabBullet;
        set => prefabBullet = value;
    }

    public float range
    {
        get => searchRadius;
        set => searchRadius = value;
    }

    public float attackInterval
    {
        get => fireInterval;
        set => fireInterval = value;
    }

    protected override void InitDefaultSettings()
    {
        shotPower = 10f;
        fireInterval = 2.0f;
        searchRadius = 8f;
        if (prefabBullet == null)
        {
            prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/BlockBellet");
        }

        if (defaultTarget == null)
        {
            Transform found = transform.Find("DefultTarget");
            if (found == null) found = transform.Find("DefaultTarget");
            if (found != null) defaultTarget = found;
        }
    }

    /// <summary>
    /// 매 프레임 타겟 몬스터 또는 기본 탄착점 갱신
    /// </summary>
    protected override void UpdateTarget()
    {
        targetTransform = FindNearestEnemy(transform.position, searchRadius);
        if (targetTransform == null)
        {
            targetTransform = defaultTarget;
        }
    }

    /// <summary>
    /// 목표 지점까지의 포물선(역탄도) 궤적을 계산하여 발사 (다형성 구현)
    /// </summary>
    public override void Shot(Vector3 dir)
    {
        if (prefabBullet == null) return;

        UpdateTarget();

        Vector3 spawnPos = GetSpawnPosition();
        GameObject copyBullet = Instantiate(prefabBullet, spawnPos, Quaternion.identity);
        BlockBullet blockBullet = copyBullet.GetComponent<BlockBullet>();

        if (blockBullet == null) return;

        Vector2 targetPos = targetTransform != null ? (Vector2)targetTransform.position : (Vector2)spawnPos + ((Vector2)dir * 3f);
        Vector3 forceOffsetPosition = spawnPos + new Vector3(0.1f, 0.1f, 0f);
        Vector2 launchVelocity = CalculateBallisticVelocity(spawnPos, targetPos, flightTime);

        blockBullet.InitBulletWithVelocity(launchVelocity, forceOffsetPosition, master);
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(currentPos, searchRadius);

        if (targetTransform != null)
        {
            Vector2 targetPos = targetTransform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPos, 0.35f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(currentPos, targetPos);
        }
    }
}
