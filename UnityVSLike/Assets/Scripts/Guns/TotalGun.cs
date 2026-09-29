using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BaseGun을 상속받으며, enum 기반으로 7종 무기 상태를 전환/실행할 수 있는 복합 총기 클래스
/// </summary>
public class TotalGun : BaseGun
{
    // 1. 무기 상태를 정의하는 enum
    public enum GunType
    {
        DefaultGun,
        KunaiGun,
        ShotGun,
        RocketLauncher,
        SoccerGun,
        BlockGun,
        LightningShield
    }

    [Header("=== 현재 무기 상태 (Gun State) ===")]
    [SerializeField] private GunType currentGunType = GunType.DefaultGun;
    public GunType CurrentGunType => currentGunType;

    [Header("=== 무기별 특화 설정: 샷건 (ShotGun) ===")]
    public float shotgunSpreadAngle = 30f;
    public bool shotgunShowGizmos = true;
    public Color shotgunSearchColor = Color.green;
    public Color shotgunSpreadColor = Color.red;
    public float shotgunDebugRayDuration = 0.5f;
    [HideInInspector] public Transform shotgunCurrentTarget;
    [HideInInspector] public float shotgunAngleDifference;

    [Header("=== 무기별 특화 설정: 블록건 (BlockGun) ===")]
    public float blockFlightTime = 1.0f;
    public Transform blockDefaultTarget;
    [HideInInspector] public Transform blockTargetTransform = null;

    [Header("=== 무기별 특화 설정: 낙뢰실드 (LightningShield) ===")]
    public int lightningMaxTargets = 3;
    [HideInInspector] public List<Transform> lightningCurrentTargets = new List<Transform>();

    protected override void Awake()
    {
        base.Awake();
        InitDefaultTargets();
    }

    private void Start()
    {
        // 시작 시 현재 설정된 enum 상태로 초기화
        SetGunType(currentGunType);
    }

    protected override void Update()
    {
        // 1. 단독 테스트용 숫자키 입력 (GunInventory가 없을 때만 동작)
        HandleWeaponSwitchInput();

        // 2. 부모의 Update() 실행: UpdateTarget() 호출 및 단일 fireTimer 쿨타임 기반 다형적 Shot() 호출
        base.Update();
    }

    #region enum 기반 유한상태 전환 (State Transition)

    /// <summary>
    /// enum 값을 받아 무기 상태를 전환하고, 해당 무기에 맞는 공통 스펙과 프리팹을 세팅합니다.
    /// </summary>
    public void SetGunType(GunType newGunType, bool updateObjectName = false)
    {
        currentGunType = newGunType;
        fireTimer = 0f; // 상태 전환 시 타이머 리셋

        // enum에 따라 각 무기의 기본 스펙과 프리팹 적용
        switch (currentGunType)
        {
            case GunType.DefaultGun:
                shotPower = 10f;
                fireInterval = 0.5f;
                searchRadius = 10f;
                prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/Bullet");
                break;

            case GunType.KunaiGun:
                shotPower = 10f;
                fireInterval = 0.5f;
                searchRadius = 10f;
                prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/Bullet");
                break;

            case GunType.ShotGun:
                shotPower = 10f;
                fireInterval = 0.2f;
                searchRadius = 10f;
                prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/Bullet");
                break;

            case GunType.RocketLauncher:
                shotPower = 15f;
                fireInterval = 1.5f;
                searchRadius = 10f;
                prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/RocketBullet");
                break;

            case GunType.SoccerGun:
                shotPower = 12f;
                fireInterval = 2.0f;
                searchRadius = 10f;
                prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/SoccerBullet");
                break;

            case GunType.BlockGun:
                shotPower = 10f;
                fireInterval = 2.0f;
                searchRadius = 8f;
                prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/BlockBellet");
                break;

            case GunType.LightningShield:
                shotPower = 10f;
                fireInterval = 1.5f;
                searchRadius = 10f;
                prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/LightningBullet");
                break;
        }

        if (updateObjectName)
        {
            this.gameObject.name = $"TotalGun_{newGunType}";
        }

        Debug.Log($"[TotalGun ({gameObject.name})] enum 상태 전환 -> {currentGunType} (위력: {shotPower}, 쿨타임: {fireInterval}s, 반경: {searchRadius})");
    }

    private void HandleWeaponSwitchInput()
    {
        // GunInventory가 부모에 있는 경우 건인벤토리의 AddGun 단축키와 충돌하지 않도록 스킵
        if (GetComponentInParent<GunInventory>() != null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SetGunType(GunType.DefaultGun, true);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SetGunType(GunType.KunaiGun, true);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SetGunType(GunType.ShotGun, true);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SetGunType(GunType.RocketLauncher, true);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) SetGunType(GunType.SoccerGun, true);
        else if (Input.GetKeyDown(KeyCode.Alpha6)) SetGunType(GunType.BlockGun, true);
        else if (Input.GetKeyDown(KeyCode.Alpha7)) SetGunType(GunType.LightningShield, true);
    }

    #endregion

    #region enum 상태별 UpdateTarget 및 Shot 다형성 구현

    /// <summary>
    /// BaseGun의 가상 메서드 오버라이드: 현재 무기 상태에 맞는 타겟 탐색 수행
    /// </summary>
    protected override void UpdateTarget()
    {
        switch (currentGunType)
        {
            case GunType.ShotGun:
                if (!IsTargetValid(shotgunCurrentTarget, searchRadius))
                {
                    shotgunCurrentTarget = FindNearestEnemy(transform.position, searchRadius);
                }
                break;

            case GunType.BlockGun:
                blockTargetTransform = FindNearestEnemy(transform.position, searchRadius);
                if (blockTargetTransform == null)
                {
                    blockTargetTransform = blockDefaultTarget;
                }
                break;

            case GunType.LightningShield:
                lightningCurrentTargets.Clear();
                Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, searchRadius, monsterLayer);
                if (enemies.Length > 0)
                {
                    int targetCount = Mathf.Min(enemies.Length, lightningMaxTargets);
                    for (int i = 0; i < targetCount; i++)
                    {
                        if (enemies[i] != null && enemies[i].gameObject.activeInHierarchy)
                        {
                            lightningCurrentTargets.Add(enemies[i].transform);
                        }
                    }
                }
                break;
        }
    }

    /// <summary>
    /// BaseGun의 추상 메서드 오버라이드: 현재 enum 상태에 따라 해당하는 무기 발사 로직 실행
    /// </summary>
    public override void Shot(Vector3 dir)
    {
        switch (currentGunType)
        {
            case GunType.DefaultGun:
                ShotDefault(dir);
                break;

            case GunType.KunaiGun:
                ShotKunai(dir);
                break;

            case GunType.ShotGun:
                ShotShotgun(dir);
                break;

            case GunType.RocketLauncher:
                ShotRocket(dir);
                break;

            case GunType.SoccerGun:
                ShotSoccer(dir);
                break;

            case GunType.BlockGun:
                ShotBlock(dir);
                break;

            case GunType.LightningShield:
                ShotLightning(dir);
                break;
        }
    }

    #endregion

    #region 무기별 발사 세부 메서드

    private void ShotDefault(Vector3 dir)
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

    private void ShotKunai(Vector3 dir)
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

    private void ShotShotgun(Vector3 dir)
    {
        if (prefabBullet == null) return;

        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (shotgunCurrentTarget != null)
        {
            Vector3 targetDist = shotgunCurrentTarget.position - transform.position;
            Vector2 targetDir = targetDist.normalized;
            shotgunAngleDifference = Vector2.Angle(dir, targetDir);

            if (shotgunAngleDifference <= shotgunSpreadAngle / 2f)
            {
                baseAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
            }
        }

        float halfSpread = shotgunSpreadAngle / 2f;
        float randomOffset = UnityEngine.Random.Range(-halfSpread, halfSpread);
        float finalAngle = baseAngle + randomOffset;

        float rad = finalAngle * Mathf.Deg2Rad;
        Vector2 finalDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        Vector3 spawnPos = GetSpawnPosition();
        if (shotgunShowGizmos)
        {
            Debug.DrawLine(spawnPos, (Vector2)spawnPos + (finalDir * 5f), shotgunSpreadColor, shotgunDebugRayDuration);
        }

        GameObject copyBullet = Instantiate(prefabBullet, spawnPos, Quaternion.identity);
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(finalDir, master, shotPower);
        }
    }

    private void ShotRocket(Vector3 dir)
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

    private void ShotSoccer(Vector3 dir)
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

    private void ShotBlock(Vector3 dir)
    {
        if (prefabBullet == null) return;

        Vector3 spawnPos = GetSpawnPosition();
        GameObject copyBullet = Instantiate(prefabBullet, spawnPos, Quaternion.identity);
        BlockBullet blockBullet = copyBullet.GetComponent<BlockBullet>();

        if (blockBullet == null) return;

        Vector2 targetPos = blockTargetTransform != null ? (Vector2)blockTargetTransform.position : (Vector2)spawnPos + ((Vector2)dir * 3f);
        Vector3 forceOffsetPosition = spawnPos + new Vector3(0.1f, 0.1f, 0f);
        Vector2 launchVelocity = CalculateBallisticVelocity(spawnPos, targetPos, blockFlightTime);

        blockBullet.InitBulletWithVelocity(launchVelocity, forceOffsetPosition, master);
    }

    private void ShotLightning(Vector3 dir)
    {
        if (prefabBullet == null) return;

        UpdateTarget();

        if (lightningCurrentTargets != null && lightningCurrentTargets.Count > 0)
        {
            foreach (Transform target in lightningCurrentTargets)
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
            Vector3 fallbackPos = GetSpawnPosition(dir);
            GameObject copyBullet = Instantiate(prefabBullet, fallbackPos, Quaternion.identity);

            LightningBullet bullet = copyBullet.GetComponent<LightningBullet>();
            if (bullet != null)
            {
                bullet.master = this.master;
            }
        }
    }

    #endregion

    private void InitDefaultTargets()
    {
        if (blockDefaultTarget == null)
        {
            Transform found = transform.Find("DefultTarget");
            if (found == null) found = transform.Find("DefaultTarget");
            if (found != null) blockDefaultTarget = found;
        }
    }

    #region Scene Gizmos

    protected override void OnDrawGizmos()
    {
        Vector3 currentPos = transform.position;

        switch (currentGunType)
        {
            case GunType.ShotGun:
                if (!shotgunShowGizmos) return;
                Gizmos.color = shotgunSearchColor;
                Gizmos.DrawWireSphere(currentPos, searchRadius);

                if (shotgunCurrentTarget != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(currentPos, shotgunCurrentTarget.position);
                }

                Gizmos.color = shotgunSpreadColor;
                Vector2 playerDir = GetPlayerFacingDirection();
                float baseAngle = Vector2.SignedAngle(Vector2.right, playerDir);
                float halfSpread = shotgunSpreadAngle / 2f;
                float leftAngle = baseAngle - halfSpread;
                float rightAngle = baseAngle + halfSpread;
                Vector3 leftDir = new Vector3(Mathf.Cos(leftAngle * Mathf.Deg2Rad), Mathf.Sin(leftAngle * Mathf.Deg2Rad), 0f);
                Vector3 rightDir = new Vector3(Mathf.Cos(rightAngle * Mathf.Deg2Rad), Mathf.Sin(rightAngle * Mathf.Deg2Rad), 0f);
                Gizmos.DrawLine(currentPos, currentPos + leftDir * 3f);
                Gizmos.DrawLine(currentPos, currentPos + rightDir * 3f);
                break;

            case GunType.BlockGun:
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(currentPos, searchRadius);
                if (blockTargetTransform != null)
                {
                    Vector2 targetPos = blockTargetTransform.position;
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(targetPos, 0.35f);
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(currentPos, targetPos);
                }
                break;

            case GunType.LightningShield:
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(currentPos, searchRadius);
                if (lightningCurrentTargets != null)
                {
                    Gizmos.color = Color.red;
                    foreach (Transform target in lightningCurrentTargets)
                    {
                        if (target != null)
                        {
                            Gizmos.DrawLine(currentPos, target.position);
                            Gizmos.DrawWireSphere(target.position, 0.5f);
                        }
                    }
                }
                break;

            case GunType.KunaiGun:
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(currentPos, searchRadius);
                break;

            case GunType.RocketLauncher:
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(currentPos, searchRadius);
                break;

            case GunType.SoccerGun:
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(currentPos, searchRadius);
                break;
        }
    }

    #endregion
}
