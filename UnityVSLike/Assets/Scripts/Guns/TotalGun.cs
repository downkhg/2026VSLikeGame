using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TotalGun의 각 무기 동작을 유한상태(Finite State)로 캡슐화하는 인터페이스
/// </summary>
public interface ITotalGunState
{
    TotalGun.GunType Type { get; }
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

    [Header("=== 무기 타입 및 유한 상태 ===")]
    [SerializeField] private GunType currentGunType = GunType.DefaultGun;
    public GunType CurrentGunType => currentGunType;

    private ITotalGunState currentState;
    public ITotalGunState CurrentState => currentState;

    [Header("=== 공통 기본 설정 (Common Settings) ===")]
    [Tooltip("탄환 발사 속도 및 위력")]
    public float shotPower = 10f;

    [Tooltip("자동 발사 주기 / 쿨타임 (초)")]
    public float fireInterval = 0.5f;

    [Tooltip("적 탐색 및 사거리 반경")]
    public float searchRadius = 10f;

    [Tooltip("현재 상태에서 발사할 탄환 프리팹")]
    public GameObject prefabBullet;

    [SerializeField] private float fireTimer = 0f;
    public float FireTimer => fireTimer;

    [Header("=== 공통 총구 및 소유자 ===")]
    public Transform firePoint;
    public Vector3 defaultFirePointOffset = new Vector3(0.5f, 0f, 0f);
    public Player master;
    public LayerMask monsterLayer;

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
    }

    private void Start()
    {
        // 초기 무기 유한상태로 진입
        SetGunType(currentGunType);
    }

    private void Update()
    {
        // 1. 디버그 및 테스트용 숫자키 무기 상태 실시간 변경
        HandleWeaponSwitchInput();

        if (currentState == null) return;

        // 2. 현재 상태의 프레임별 타겟 추적/갱신 로직 단독 실행
        currentState.Update(this);

        // 3. 단일 공통 타이머로 공통 fireInterval 쿨타임 체크 후 발사
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            currentState.Shot(this, GetPlayerFacingDirection());
        }
    }

    #region 유한상태 머신 (FSM) 제어

    /// <summary>
    /// 무기 타입을 변경하고 해당 유한상태(State)로 전이합니다.
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
        fireTimer = 0f; // 상태 전환 시 타이머 리셋

        if (currentState != null)
        {
            currentGunType = currentState.Type;
            currentState.Enter(this);
            Debug.Log($"[TotalGun ({gameObject.name})] 유한상태 진입 -> {currentGunType} (위력: {shotPower}, 쿨타임: {fireInterval}s, 반경: {searchRadius})");
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

    #region 공통 유틸리티 메서드

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

    public Vector3 GetSpawnPosition()
    {
        return firePoint;
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

    public void Enter(TotalGun gun)
    {
        gun.shotPower = 10f;
        gun.fireInterval = 0.5f;
        gun.searchRadius = 10f;
        gun.prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/Bullet");
    }

    public void Update(TotalGun gun) { }

    public void Shot(TotalGun gun, Vector3 dir)
    {
        if (gun.prefabBullet == null) return;

        Vector3 spawnPos = gun.GetSpawnPosition(dir);
        GameObject copyBullet = UnityEngine.Object.Instantiate(gun.prefabBullet, spawnPos, Quaternion.identity);
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(dir, gun.master, gun.shotPower);
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

    public void Enter(TotalGun gun)
    {
        gun.shotPower = 10f;
        gun.fireInterval = 0.5f;
        gun.searchRadius = 10f;
        gun.prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/Bullet");
    }

    public void Update(TotalGun gun) { }

    public void Shot(TotalGun gun, Vector3 dir)
    {
        if (gun.prefabBullet == null) return;

        Transform nearestEnemy = gun.FindNearestEnemy(gun.transform.position, gun.searchRadius);
        Vector3 spawnPos = gun.GetSpawnPosition();
        Vector2 finalDir = nearestEnemy != null ? (Vector2)(nearestEnemy.position - spawnPos).normalized : (Vector2)dir;

        GameObject copyBullet = UnityEngine.Object.Instantiate(gun.prefabBullet, spawnPos, Quaternion.identity);
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(finalDir, gun.master, gun.shotPower);
        }
    }

    public void Exit(TotalGun gun) { }

    public void DrawGizmos(TotalGun gun)
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(gun.transform.position, gun.searchRadius);
    }
}

/// <summary>
/// 3. 샷건 상태 (ShotGunState)
/// </summary>
public class ShotGunState : ITotalGunState
{
    public TotalGun.GunType Type => TotalGun.GunType.ShotGun;

    public void Enter(TotalGun gun)
    {
        gun.shotPower = 10f;
        gun.fireInterval = 0.2f;
        gun.searchRadius = 10f;
        gun.prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/Bullet");
    }

    public void Update(TotalGun gun)
    {
        if (!gun.IsTargetValid(gun.shotgunCurrentTarget, gun.searchRadius))
        {
            gun.shotgunCurrentTarget = gun.FindNearestEnemy(gun.transform.position, gun.searchRadius);
        }
    }

    public void Shot(TotalGun gun, Vector3 dir)
    {
        if (gun.prefabBullet == null) return;

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

        GameObject copyBullet = UnityEngine.Object.Instantiate(gun.prefabBullet, spawnPos, Quaternion.identity);
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(finalDir, gun.master, gun.shotPower);
        }
    }

    public void Exit(TotalGun gun) { }

    public void DrawGizmos(TotalGun gun)
    {
        if (!gun.shotgunShowGizmos) return;
        Vector3 currentPos = gun.transform.position;

        Gizmos.color = gun.shotgunSearchColor;
        Gizmos.DrawWireSphere(currentPos, gun.searchRadius);

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

    public void Enter(TotalGun gun)
    {
        gun.shotPower = 15f;
        gun.fireInterval = 1.5f;
        gun.searchRadius = 10f;
        gun.prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/RocketBullet");
    }

    public void Update(TotalGun gun) { }

    public void Shot(TotalGun gun, Vector3 dir)
    {
        if (gun.prefabBullet == null) return;

        Vector3 launchPosition = gun.GetSpawnPosition();
        Transform nearestEnemy = gun.FindNearestEnemy(gun.transform.position, gun.searchRadius);
        Vector2 launchDir = nearestEnemy != null ? (Vector2)(nearestEnemy.position - launchPosition).normalized : (Vector2)dir;

        GameObject rocketObj = UnityEngine.Object.Instantiate(gun.prefabBullet, launchPosition, Quaternion.identity);
        Bullet bulletScript = rocketObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Init(launchDir, gun.master, gun.shotPower);
        }
    }

    public void Exit(TotalGun gun) { }

    public void DrawGizmos(TotalGun gun)
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(gun.transform.position, gun.searchRadius);
    }
}

/// <summary>
/// 5. 축구공 건 상태 (SoccerGunState)
/// </summary>
public class SoccerGunState : ITotalGunState
{
    public TotalGun.GunType Type => TotalGun.GunType.SoccerGun;

    public void Enter(TotalGun gun)
    {
        gun.shotPower = 12f;
        gun.fireInterval = 2.0f;
        gun.searchRadius = 10f;
        gun.prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/SoccerBullet");
    }

    public void Update(TotalGun gun) { }

    public void Shot(TotalGun gun, Vector3 dir)
    {
        if (gun.prefabBullet == null) return;

        Vector3 spawnPos = gun.GetSpawnPosition();
        Transform target = gun.FindNearestEnemy(gun.transform.position, gun.searchRadius);
        Vector2 finalDir = target != null ? (Vector2)(target.position - spawnPos).normalized : (Vector2)dir;

        GameObject soccerObj = UnityEngine.Object.Instantiate(gun.prefabBullet, spawnPos, Quaternion.identity);
        SoccerBullet bullet = soccerObj.GetComponent<SoccerBullet>();
        if (bullet != null)
        {
            bullet.Init(finalDir, gun.master, gun.shotPower);
        }
    }

    public void Exit(TotalGun gun) { }

    public void DrawGizmos(TotalGun gun)
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(gun.transform.position, gun.searchRadius);
    }
}

/// <summary>
/// 6. 블록 포물선 건 상태 (BlockGunState)
/// </summary>
public class BlockGunState : ITotalGunState
{
    public TotalGun.GunType Type => TotalGun.GunType.BlockGun;

    public void Enter(TotalGun gun)
    {
        gun.shotPower = 10f;
        gun.fireInterval = 2.0f;
        gun.searchRadius = 8f;
        gun.prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/BlockBellet");
    }

    public void Update(TotalGun gun)
    {
        gun.blockTargetTransform = gun.FindNearestEnemy(gun.transform.position, gun.searchRadius);
        if (gun.blockTargetTransform == null)
        {
            gun.blockTargetTransform = gun.blockDefaultTarget;
        }
    }

    public void Shot(TotalGun gun, Vector3 dir)
    {
        if (gun.prefabBullet == null) return;

        Vector3 spawnPos = gun.GetSpawnPosition();
        GameObject copyBullet = UnityEngine.Object.Instantiate(gun.prefabBullet, spawnPos, Quaternion.identity);
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
        Gizmos.DrawWireSphere(currentPos, gun.searchRadius);

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

    public void Enter(TotalGun gun)
    {
        gun.shotPower = 10f;
        gun.fireInterval = 1.5f;
        gun.searchRadius = 5f;
        gun.prefabBullet = Resources.Load<GameObject>("Prefabs/Bullet/LightningBullet");
    }

    public void Update(TotalGun gun)
    {
        gun.lightningCurrentTargets.Clear();

        Collider2D[] enemies = Physics2D.OverlapCircleAll(gun.transform.position, gun.searchRadius, gun.monsterLayer);
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
        if (gun.prefabBullet == null || gun.lightningCurrentTargets.Count == 0) return;

        foreach (Transform target in gun.lightningCurrentTargets)
        {
            if (target == null) continue;

            Vector3 spawnPosition = target.position;
            GameObject copyBullet = UnityEngine.Object.Instantiate(gun.prefabBullet, spawnPosition, Quaternion.identity);

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
        Gizmos.DrawWireSphere(currentPos, gun.searchRadius);

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
