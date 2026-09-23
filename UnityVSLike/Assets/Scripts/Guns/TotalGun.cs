using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 교육 및 학습용: 인터페이스 없이 순수 enum과 switch-case 기반의 유한상태(FSM)로 동작하는 TotalGun
/// </summary>
public class TotalGun : MonoBehaviour
{
    // 1. 유한 상태를 정의하는 enum
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

    [Header("=== 공통 기본 설정 (Common Settings) ===")]
    [Tooltip("탄환 발사 속도 및 위력")]
    public float shotPower = 10f;

    [Tooltip("자동 발사 주기 / 쿨타임 (초)")]
    public float fireInterval = 0.5f;

    [Tooltip("적 탐색 및 사거리 반경")]
    public float searchRadius = 10f;

    [Tooltip("현재 무기에서 발사할 탄환 프리팹")]
    public GameObject prefabBullet;

    [Tooltip("단일 공통 발사 타이머")]
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
        // 시작 시 현재 설정된 enum 상태로 초기화
        SetGunType(currentGunType);
    }

    private void Update()
    {
        // 1. 테스트용 숫자키 1~7로 무기 상태 실시간 변경
        HandleWeaponSwitchInput();

        // 2. 현재 enum 상태에 따른 실시간 타겟 추적/갱신
        UpdateCurrentState();

        // 3. 단일 공통 타이머로 쿨타임 체크 후 자동 발사
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            Shot(GetPlayerFacingDirection());
        }
    }

    #region enum 기반 유한상태 전환 (State Transition)

    /// <summary>
    /// enum 값을 받아 무기 상태를 전환하고, 해당 무기에 맞는 공통 스펙과 프리팹을 세팅합니다.
    /// </summary>
    public void SetGunType(GunType newGunType, bool updateObjectName = false)
    {
        currentGunType = newGunType;
        fireTimer = 0f; // 상태 전환 시 타이머 리셋

        // enum에 따라 각 무기의 기본 스펙과 프리팹 적용 (OnEnter 역할)
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

    #region enum 상태별 Update 및 Shot 처리 (switch-case)

    /// <summary>
    /// 매 프레임 현재 무기 상태에 맞는 타겟 탐색/갱신을 수행합니다.
    /// </summary>
    private void UpdateCurrentState()
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
                        if (enemies[i] != null)
                        {
                            lightningCurrentTargets.Add(enemies[i].transform);
                        }
                    }
                }
                break;
        }
    }

    public void Shot()
    {
        Vector3 defaultDir = GetPlayerFacingDirection();
        Shot(defaultDir);
    }

    /// <summary>
    /// 현재 enum 상태에 따라 해당하는 무기 발사 로직을 분기 실행합니다.
    /// </summary>
    public void Shot(Vector3 dir)
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

    // 1. 기본 총 발사
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

    // 2. 쿠나이 발사 (가장 가까운 적 조준)
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

    // 3. 샷건 발사 (부채꼴 산탄 분산)
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

    // 4. 로켓 런처 발사
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

    // 5. 축구공 발사
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

    // 6. 블록 포물선 발사
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

    // 7. 낙뢰 실드 시전
    private void ShotLightning(Vector3 dir)
    {
        if (prefabBullet == null) return;

        // 발사 직전 실시간 타겟 탐색 최신화
        UpdateCurrentState();

        if (lightningCurrentTargets != null && lightningCurrentTargets.Count > 0)
        {
            // 주변 몬스터가 있으면 각 몬스터 위치에 낙뢰 생성
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
            // 주변에 몬스터가 없으면 플레이어 전방 발사 위치에 Fallback 1회 낙뢰 생성
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
