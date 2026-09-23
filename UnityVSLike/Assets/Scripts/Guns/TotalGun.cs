using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TotalGun의 각 무기 동작을 유한상태(Finite State)로 캡슐화하는 인터페이스
/// </summary>
public interface ITotalGunState
{
    TotalGun.GunType Type { get; }
    float FireInterval { get; }
    void Enter(TotalGun gun);
    void Update(TotalGun gun);
    void Shot(TotalGun gun, Vector3 dir);
    void Exit(TotalGun gun);
    void DrawGizmos(TotalGun gun);
}

public class TotalGun : MonoBehaviour
{
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

    [Header("무기 타입 및 유한 상태 설정")]
    [SerializeField] private GunType currentGunType = GunType.DefaultGun;
    public GunType CurrentGunType => currentGunType;

    // 현재 활성화된 유한 상태
    private ITotalGunState currentState;
    public ITotalGunState CurrentState => currentState;

    [Header("단일 공통 발사 타이머")]
    [SerializeField] private float fireTimer = 0f;
    public float FireTimer => fireTimer;

    [Header("공통 총구 (FirePoint) & 마스터")]
    public Transform firePoint;
    public Vector3 defaultFirePointOffset = new Vector3(0.5f, 0f, 0f);
    public Player master;
    public LayerMask monsterLayer;

    [Header("=== 1. Default Gun (기본 총) ===")]
    public GameObject prefabDefaultBullet;
    public float defaultShotPower = 10f;
    public float defaultFireInterval = 0.5f;

    [Header("=== 2. Kunai Gun (쿠나이) ===")]
    public GameObject prefabKunaiBullet;
    public float kunaiShotPower = 10f;
    public float kunaiSearchRadius = 10f;
    public float kunaiFireInterval = 0.5f;

    [Header("=== 3. Shot Gun (샷건) ===")]
    public GameObject prefabShotgunBullet;
    public float shotgunShotPower = 10f;
    public float shotgunSearchRadius = 10f;
    public float shotgunSpreadAngle = 30f;
    public float shotgunFireInterval = 0.2f;
    [SerializeField] public Transform shotgunCurrentTarget;
    public bool shotgunShowGizmos = true;
    public Color shotgunSearchColor = Color.green;
    public Color shotgunSpreadColor = Color.red;
    public float shotgunDebugRayDuration = 0.5f;
    [HideInInspector] public float shotgunAngleDifference;

    [Header("=== 4. Rocket Launcher (로켓 런처) ===")]
    public GameObject prefabRocketBullet;
    public float rocketLaunchForce = 15f;
    public float rocketSearchRadius = 10f;
    public float rocketFireInterval = 1.5f;

    [Header("=== 5. Soccer Gun (축구공) ===")]
    public GameObject prefabSoccerBullet;
    public float soccerShotPower = 12f;
    public float soccerSearchRadius = 10f;
    public float soccerFireInterval = 2.0f;

    [Header("=== 6. Block Gun (블록 포물선) ===")]
    public GameObject prefabBlockBullet;
    public float blockFlightTime = 1.0f;
    public float blockRange = 8f;
    public float blockAttackInterval = 2.0f;
    public Transform blockDefaultTarget;
    [HideInInspector] public Transform blockTargetTransform = null;

    [Header("=== 7. Lightning Shield (낙뢰 실드) ===")]
    public GameObject prefabLightningBullet;
    public float lightningDetectionRadius = 5f;
    public float lightningStrikeInterval = 1.5f;
    public int lightningMaxTargets = 3;
    [HideInInspector] public List<Transform> lightningCurrentTargets = new List<Transform>();

    private void Awake()
    {
        if (master == null)
        {
            master = GetComponentInParent<Player>();
        }

        if (monsterLayer == 0)
        {
            monsterLayer = 1 << LayerMask.NameToLayer("Monster");
        }

        InitFirePoint();
        InitDefaultTargets();
        AutoLoadResourcePrefabs();
    }

    private void Start()
    {
        // 시작 시 초기 무기 상태로 진입
        SetGunType(currentGunType);
    }

    private void Update()
    {
        // 1. 디버그/테스트 편의를 위한 1~7 숫자키 무기 상태 실시간 변경
        HandleWeaponSwitchInput();

        if (currentState == null) return;

        // 2. 현재 상태의 프레임별 타겟 갱신/상태 로직 실행
        currentState.Update(this);

        // 3. 단일 공통 타이머로 현재 상태의 쿨타임(FireInterval)만 체크하여 단일 작동
        fireTimer += Time.deltaTime;
        if (fireTimer >= currentState.FireInterval)
        {
            fireTimer = 0f;
            currentState.Shot(this, GetPlayerFacingDirection());
        }
    }

    #region 유한상태 머신 (FSM) 전이 및 무기 전환

    /// <summary>
    /// 무기 타입을 변경하고 해당 유한상태(State)로 전이합니다.
    /// updateObjectName이 true인 경우 오브젝트명에 상태명을 반영합니다.
    /// </summary>
    public void SetGunType(GunType newGunType, bool updateObjectName = false)
    {
        currentGunType = newGunType;
        ITotalGunState newState = CreateState(newGunType);
        ChangeState(newState);

        if (updateObjectName)
        {
            this.gameObject.name = $"TotalGun_{newGunType}";
        }
    }

    /// <summary>
    /// 유한상태를 안전하게 교체(Exit -> Enter)하고 단일 타이머를 초기화합니다.
    /// </summary>
    public void ChangeState(ITotalGunState newState)
    {
        if (currentState != null)
        {
            currentState.Exit(this);
        }

        currentState = newState;
        fireTimer = 0f; // 상태 전이 시 타이머 초기화

        if (currentState != null)
        {
            currentGunType = currentState.Type;
            currentState.Enter(this);
            Debug.Log($"[TotalGun ({gameObject.name})] 유한상태 진입 -> {currentGunType} (발사주기: {currentState.FireInterval}s)");
        }
    }

    private ITotalGunState CreateState(GunType gunType)
    {
        switch (gunType)
        {
            case GunType.DefaultGun: return new DefaultGunState();
            case GunType.KunaiGun: return new KunaiGunState();
            case GunType.ShotGun: return new ShotGunState();
            case GunType.RocketLauncher: return new RocketLauncherState();
            case GunType.SoccerGun: return new SoccerGunState();
            case GunType.BlockGun: return new BlockGunState();
            case GunType.LightningShield: return new LightningShieldState();
            default: return new DefaultGunState();
        }
    }

    private void HandleWeaponSwitchInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetGunType(GunType.DefaultGun, true);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SetGunType(GunType.KunaiGun, true);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SetGunType(GunType.ShotGun, true);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SetGunType(GunType.RocketLauncher, true);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) SetGunType(GunType.SoccerGun, true);
        else if (Input.GetKeyDown(KeyCode.Alpha6)) SetGunType(GunType.BlockGun, true);
        else if (Input.GetKeyDown(KeyCode.Alpha7)) SetGunType(GunType.LightningShield, true);
    }

    #endregion

    #region 외부 발사 인터페이스

    public void Shot()
    {
        Vector3 defaultDir = GetPlayerFacingDirection();
        Shot(defaultDir);
    }

    public void Shot(Vector3 dir)
    {
        if (currentState != null)
        {
            currentState.Shot(this, dir);
        }
    }

    #endregion

    #region 공통 헬퍼 메서드

    private void InitFirePoint()
    {
        if (firePoint == null)
        {
            Transform found = transform.Find("FirePoint");
            if (found != null)
            {
                firePoint = found;
            }
            else
            {
                GameObject fpObj = new GameObject("FirePoint");
                fpObj.transform.SetParent(transform);
                fpObj.transform.localPosition = defaultFirePointOffset;
                fpObj.transform.localRotation = Quaternion.identity;
                firePoint = fpObj.transform;
            }
        }
    }

    private void InitDefaultTargets()
    {
        if (blockDefaultTarget == null)
        {
            Transform found = transform.Find("DefultTarget");
            if (found == null) found = transform.Find("DefaultTarget");
            if (found != null) blockDefaultTarget = found;
        }
    }

    private void AutoLoadResourcePrefabs()
    {
        if (prefabDefaultBullet == null) prefabDefaultBullet = Resources.Load<GameObject>("Prefabs/Bullet/Bullet");
        if (prefabKunaiBullet == null) prefabKunaiBullet = Resources.Load<GameObject>("Prefabs/Bullet/Bullet");
        if (prefabShotgunBullet == null) prefabShotgunBullet = Resources.Load<GameObject>("Prefabs/Bullet/Bullet");
        if (prefabRocketBullet == null) prefabRocketBullet = Resources.Load<GameObject>("Prefabs/Bullet/RocketBullet");
        if (prefabSoccerBullet == null) prefabSoccerBullet = Resources.Load<GameObject>("Prefabs/Bullet/SoccerBullet");
        if (prefabBlockBullet == null) prefabBlockBullet = Resources.Load<GameObject>("Prefabs/Bullet/BlockBellet");
        if (prefabLightningBullet == null) prefabLightningBullet = Resources.Load<GameObject>("Prefabs/Bullet/LightningBullet");
    }

    public Vector3 GetSpawnPosition()
    {
        return firePoint != null ? firePoint.position : transform.position;
    }

    public Vector3 GetSpawnPosition(Vector3 dir)
    {
        if (firePoint != null)
        {
            return transform.position + (dir.normalized * defaultFirePointOffset.magnitude);
        }
        return transform.position + (dir.normalized * defaultFirePointOffset.magnitude);
    }

    public Vector2 GetPlayerFacingDirection()
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

    public Transform FindNearestEnemy(Vector3 origin, float radius)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(origin, radius, monsterLayer);
        if (enemies.Length == 0) return null;

        Transform nearest = null;
        float minDistance = float.MaxValue;

        foreach (Collider2D enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

            float dist = Vector3.Distance(origin, enemy.transform.position);
            if (dist < minDistance && dist <= radius)
            {
                minDistance = dist;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }

    public bool IsTargetValid(Transform target, float maxRadius)
    {
        if (target == null) return false;
        if (!target.gameObject.activeInHierarchy) return false;

        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= maxRadius;
    }

    public Vector2 CalculateBallisticVelocity(Vector2 startPos, Vector2 targetPos, float time)
    {
        Vector2 vDist = targetPos - startPos;
        float gravity = Mathf.Abs(Physics2D.gravity.y);

        if (time <= 0.05f) time = 0.05f;

        float vx = vDist.x / time;
        float vy = (vDist.y / time) + (0.5f * gravity * time);

        return new Vector2(vx, vy);
    }

    #endregion

    #region Scene Gizmos

    private void OnDrawGizmos()
    {
        if (currentState != null)
        {
            currentState.DrawGizmos(this);
        }
    }

    #endregion
}

#region 유한상태 (State) 구현 클래스들

/// <summary>
/// 1. 기본 총 상태 (DefaultGunState)
/// </summary>
public class DefaultGunState : ITotalGunState
{
    public TotalGun.GunType Type => TotalGun.GunType.DefaultGun;
    public float FireInterval => cachedInterval;
    private float cachedInterval = 0.5f;

    public void Enter(TotalGun gun)
    {
        cachedInterval = gun.defaultFireInterval;
    }

    public void Update(TotalGun gun) { }

    public void Shot(TotalGun gun, Vector3 dir)
    {
        if (gun.prefabDefaultBullet == null) return;

        Vector3 spawnPos = gun.GetSpawnPosition(dir);
        GameObject copyBullet = UnityEngine.Object.Instantiate(gun.prefabDefaultBullet, spawnPos, Quaternion.identity);
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(dir, gun.master, gun.defaultShotPower);
        }
    }

    public void Exit(TotalGun gun) { }
    public void DrawGizmos(TotalGun gun) { }
}

/// <summary>
/// 2. 쿠나이 건 상태 (KunaiGunState)
/// </summary>
public class KunaiGunState : ITotalGunState
{
    public TotalGun.GunType Type => TotalGun.GunType.KunaiGun;
    public float FireInterval => cachedInterval;
    private float cachedInterval = 0.5f;

    public void Enter(TotalGun gun)
    {
        cachedInterval = gun.kunaiFireInterval;
    }

    public void Update(TotalGun gun) { }

    public void Shot(TotalGun gun, Vector3 dir)
    {
        if (gun.prefabKunaiBullet == null) return;

        Transform nearestEnemy = gun.FindNearestEnemy(gun.transform.position, gun.kunaiSearchRadius);
        Vector3 spawnPos = gun.GetSpawnPosition();
        Vector2 finalDir;

        if (nearestEnemy != null)
        {
            finalDir = (nearestEnemy.position - spawnPos).normalized;
        }
        else
        {
            finalDir = dir;
        }

        GameObject copyBullet = UnityEngine.Object.Instantiate(gun.prefabKunaiBullet, spawnPos, Quaternion.identity);
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(finalDir, gun.master, gun.kunaiShotPower);
        }
    }

    public void Exit(TotalGun gun) { }

    public void DrawGizmos(TotalGun gun)
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(gun.transform.position, gun.kunaiSearchRadius);
    }
}

/// <summary>
/// 3. 샷건 상태 (ShotGunState)
/// </summary>
public class ShotGunState : ITotalGunState
{
    public TotalGun.GunType Type => TotalGun.GunType.ShotGun;
    public float FireInterval => cachedInterval;
    private float cachedInterval = 0.2f;

    public void Enter(TotalGun gun)
    {
        cachedInterval = gun.shotgunFireInterval;
    }

    public void Update(TotalGun gun)
    {
        if (!gun.IsTargetValid(gun.shotgunCurrentTarget, gun.shotgunSearchRadius))
        {
            gun.shotgunCurrentTarget = gun.FindNearestEnemy(gun.transform.position, gun.shotgunSearchRadius);
        }
    }

    public void Shot(TotalGun gun, Vector3 dir)
    {
        if (gun.prefabShotgunBullet == null) return;

        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (gun.shotgunCurrentTarget != null)
        {
            Vector3 targetDist = gun.shotgunCurrentTarget.position - gun.transform.position;
            Vector2 targetDir = targetDist.normalized;
            gun.shotgunAngleDifference = Vector2.Angle(dir, targetDir);

            if (gun.shotgunAngleDifference <= gun.shotgunSpreadAngle / 2f)
            {
                baseAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
            }
        }

        float halfSpread = gun.shotgunSpreadAngle / 2f;
        float randomOffset = UnityEngine.Random.Range(-halfSpread, halfSpread);
        float finalAngle = baseAngle + randomOffset;

        float rad = finalAngle * Mathf.Deg2Rad;
        Vector2 finalDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        Vector3 spawnPos = gun.GetSpawnPosition();
        if (gun.shotgunShowGizmos)
        {
            Debug.DrawLine(spawnPos, (Vector2)spawnPos + (finalDir * 5f), gun.shotgunSpreadColor, gun.shotgunDebugRayDuration);
        }

        GameObject copyBullet = UnityEngine.Object.Instantiate(gun.prefabShotgunBullet, spawnPos, Quaternion.identity);
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(finalDir, gun.master, gun.shotgunShotPower);
        }
    }

    public void Exit(TotalGun gun) { }

    public void DrawGizmos(TotalGun gun)
    {
        if (!gun.shotgunShowGizmos) return;
        Vector3 currentPos = gun.transform.position;

        Gizmos.color = gun.shotgunSearchColor;
        Gizmos.DrawWireSphere(currentPos, gun.shotgunSearchRadius);

        if (gun.shotgunCurrentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(currentPos, gun.shotgunCurrentTarget.position);
        }

        Gizmos.color = gun.shotgunSpreadColor;
        Vector2 playerDir = gun.GetPlayerFacingDirection();
        float baseAngle = Vector2.SignedAngle(Vector2.right, playerDir);
        float halfSpread = gun.shotgunSpreadAngle / 2f;
        float leftAngle = baseAngle - halfSpread;
        float rightAngle = baseAngle + halfSpread;
        Vector3 leftDir = new Vector3(Mathf.Cos(leftAngle * Mathf.Deg2Rad), Mathf.Sin(leftAngle * Mathf.Deg2Rad), 0f);
        Vector3 rightDir = new Vector3(Mathf.Cos(rightAngle * Mathf.Deg2Rad), Mathf.Sin(rightAngle * Mathf.Deg2Rad), 0f);
        Gizmos.DrawLine(currentPos, currentPos + leftDir * 3f);
        Gizmos.DrawLine(currentPos, currentPos + rightDir * 3f);
    }
}

/// <summary>
/// 4. 로켓 런처 상태 (RocketLauncherState)
/// </summary>
public class RocketLauncherState : ITotalGunState
{
    public TotalGun.GunType Type => TotalGun.GunType.RocketLauncher;
    public float FireInterval => cachedInterval;
    private float cachedInterval = 1.5f;

    public void Enter(TotalGun gun)
    {
        cachedInterval = gun.rocketFireInterval;
    }

    public void Update(TotalGun gun) { }

    public void Shot(TotalGun gun, Vector3 dir)
    {
        if (gun.prefabRocketBullet == null) return;

        Vector3 launchPosition = gun.GetSpawnPosition();
        Transform nearestEnemy = gun.FindNearestEnemy(gun.transform.position, gun.rocketSearchRadius);
        Vector2 launchDir = nearestEnemy != null ? (Vector2)(nearestEnemy.position - launchPosition).normalized : (Vector2)dir;

        GameObject rocketObj = UnityEngine.Object.Instantiate(gun.prefabRocketBullet, launchPosition, Quaternion.identity);
        Bullet bulletScript = rocketObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Init(launchDir, gun.master, gun.rocketLaunchForce);
        }
    }

    public void Exit(TotalGun gun) { }

    public void DrawGizmos(TotalGun gun)
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(gun.transform.position, gun.rocketSearchRadius);
    }
}

/// <summary>
/// 5. 축구공 건 상태 (SoccerGunState)
/// </summary>
public class SoccerGunState : ITotalGunState
{
    public TotalGun.GunType Type => TotalGun.GunType.SoccerGun;
    public float FireInterval => cachedInterval;
    private float cachedInterval = 2.0f;

    public void Enter(TotalGun gun)
    {
        cachedInterval = gun.soccerFireInterval;
    }

    public void Update(TotalGun gun) { }

    public void Shot(TotalGun gun, Vector3 dir)
    {
        if (gun.prefabSoccerBullet == null) return;

        Vector3 spawnPos = gun.GetSpawnPosition();
        Transform target = gun.FindNearestEnemy(gun.transform.position, gun.soccerSearchRadius);
        Vector2 finalDir = target != null ? (Vector2)(target.position - spawnPos).normalized : (Vector2)dir;

        GameObject soccerObj = UnityEngine.Object.Instantiate(gun.prefabSoccerBullet, spawnPos, Quaternion.identity);
        SoccerBullet bullet = soccerObj.GetComponent<SoccerBullet>();
        if (bullet != null)
        {
            bullet.Init(finalDir, gun.master, gun.soccerShotPower);
        }
    }

    public void Exit(TotalGun gun) { }

    public void DrawGizmos(TotalGun gun)
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(gun.transform.position, gun.soccerSearchRadius);
    }
}

/// <summary>
/// 6. 블록 포물선 건 상태 (BlockGunState)
/// </summary>
public class BlockGunState : ITotalGunState
{
    public TotalGun.GunType Type => TotalGun.GunType.BlockGun;
    public float FireInterval => cachedInterval;
    private float cachedInterval = 2.0f;

    public void Enter(TotalGun gun)
    {
        cachedInterval = gun.blockAttackInterval;
    }

    public void Update(TotalGun gun)
    {
        gun.blockTargetTransform = gun.FindNearestEnemy(gun.transform.position, gun.blockRange);
        if (gun.blockTargetTransform == null)
        {
            gun.blockTargetTransform = gun.blockDefaultTarget;
        }
    }

    public void Shot(TotalGun gun, Vector3 dir)
    {
        if (gun.prefabBlockBullet == null) return;

        Vector3 spawnPos = gun.GetSpawnPosition();
        GameObject copyBullet = UnityEngine.Object.Instantiate(gun.prefabBlockBullet, spawnPos, Quaternion.identity);
        BlockBullet blockBullet = copyBullet.GetComponent<BlockBullet>();

        if (blockBullet == null) return;

        Vector2 targetPos = gun.blockTargetTransform != null ? (Vector2)gun.blockTargetTransform.position : (Vector2)spawnPos + ((Vector2)dir * 3f);
        Vector3 forceOffsetPosition = spawnPos + new Vector3(0.1f, 0.1f, 0f);
        Vector2 launchVelocity = gun.CalculateBallisticVelocity(spawnPos, targetPos, gun.blockFlightTime);

        blockBullet.InitBulletWithVelocity(launchVelocity, forceOffsetPosition, gun.master);
    }

    public void Exit(TotalGun gun) { }

    public void DrawGizmos(TotalGun gun)
    {
        Vector3 currentPos = gun.transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(currentPos, gun.blockRange);

        if (gun.blockTargetTransform != null)
        {
            Vector2 targetPos = gun.blockTargetTransform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPos, 0.35f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(currentPos, targetPos);
        }
    }
}

/// <summary>
/// 7. 낙뢰 실드 상태 (LightningShieldState)
/// </summary>
public class LightningShieldState : ITotalGunState
{
    public TotalGun.GunType Type => TotalGun.GunType.LightningShield;
    public float FireInterval => cachedInterval;
    private float cachedInterval = 1.5f;

    public void Enter(TotalGun gun)
    {
        cachedInterval = gun.lightningStrikeInterval;
    }

    public void Update(TotalGun gun)
    {
        gun.lightningCurrentTargets.Clear();

        Collider2D[] enemies = Physics2D.OverlapCircleAll(gun.transform.position, gun.lightningDetectionRadius, gun.monsterLayer);
        if (enemies.Length == 0) return;

        int targetCount = Mathf.Min(enemies.Length, gun.lightningMaxTargets);
        for (int i = 0; i < targetCount; i++)
        {
            if (enemies[i] != null)
            {
                gun.lightningCurrentTargets.Add(enemies[i].transform);
            }
        }
    }

    public void Shot(TotalGun gun, Vector3 dir)
    {
        if (gun.prefabLightningBullet == null || gun.lightningCurrentTargets.Count == 0) return;

        foreach (Transform target in gun.lightningCurrentTargets)
        {
            if (target == null) continue;

            Vector3 spawnPosition = target.position;
            GameObject copyBullet = UnityEngine.Object.Instantiate(gun.prefabLightningBullet, spawnPosition, Quaternion.identity);

            LightningBullet bullet = copyBullet.GetComponent<LightningBullet>();
            if (bullet != null)
            {
                bullet.master = gun.master;
            }
        }
    }

    public void Exit(TotalGun gun) { }

    public void DrawGizmos(TotalGun gun)
    {
        Vector3 currentPos = gun.transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(currentPos, gun.lightningDetectionRadius);

        if (gun.lightningCurrentTargets != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform target in gun.lightningCurrentTargets)
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

#endregion
