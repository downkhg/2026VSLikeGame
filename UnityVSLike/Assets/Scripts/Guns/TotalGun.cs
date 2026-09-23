using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("무기 타입 설정")]
    [SerializeField] private GunType currentGunType = GunType.DefaultGun;
    public GunType CurrentGunType => currentGunType;

    [Header("공통 총구 (FirePoint) & 마스터")]
    public Transform firePoint;
    public Vector3 defaultFirePointOffset = new Vector3(0.5f, 0f, 0f);
    public Player master;
    public LayerMask monsterLayer;

    [Header("=== 1. Default Gun (기본 총) ===")]
    public GameObject prefabDefaultBullet;
    public float defaultShotPower = 10f;
    public float defaultFireInterval = 0.5f;
    private float defaultFireTimer = 0f;

    [Header("=== 2. Kunai Gun (쿠나이) ===")]
    public GameObject prefabKunaiBullet;
    public float kunaiShotPower = 10f;
    public float kunaiSearchRadius = 10f;
    public float kunaiFireInterval = 0.5f;
    private float kunaiFireTimer = 0f;

    [Header("=== 3. Shot Gun (샷건) ===")]
    public GameObject prefabShotgunBullet;
    public float shotgunShotPower = 10f;
    public float shotgunSearchRadius = 10f;
    public float shotgunSpreadAngle = 30f;
    public float shotgunFireInterval = 0.2f;
    private float shotgunFireTimer = 0f;
    [SerializeField] private Transform shotgunCurrentTarget;
    public bool shotgunShowGizmos = true;
    public Color shotgunSearchColor = Color.green;
    public Color shotgunSpreadColor = Color.red;
    public float shotgunDebugRayDuration = 0.5f;
    private float shotgunAngleDifference;

    [Header("=== 4. Rocket Launcher (로켓 런처) ===")]
    public GameObject prefabRocketBullet;
    public float rocketLaunchForce = 15f;
    public float rocketSearchRadius = 10f;
    public float rocketFireInterval = 1.5f;
    private float rocketFireTimer = 0f;

    [Header("=== 5. Soccer Gun (축구공) ===")]
    public GameObject prefabSoccerBullet;
    public float soccerShotPower = 12f;
    public float soccerSearchRadius = 10f;
    public float soccerFireInterval = 2.0f;
    private float soccerFireTimer = 0f;

    [Header("=== 6. Block Gun (블록 포물선) ===")]
    public GameObject prefabBlockBullet;
    public float blockFlightTime = 1.0f;
    public float blockRange = 8f;
    public float blockAttackInterval = 2.0f;
    private float blockLastShotTime = 0f;
    public Transform blockDefaultTarget;
    private Transform blockTargetTransform = null;

    [Header("=== 7. Lightning Shield (낙뢰 실드) ===")]
    public GameObject prefabLightningBullet;
    public float lightningDetectionRadius = 5f;
    public float lightningStrikeInterval = 1.5f;
    public int lightningMaxTargets = 3;
    private float lightningTimer = 0f;
    private List<Transform> lightningCurrentTargets = new List<Transform>();

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
        SetGunType(currentGunType);
    }

    private void Update()
    {
        // 1. 숫자키 1~7로 무기 실시간 변경 (디버그 / 테스트)
        HandleWeaponSwitchInput();

        // 2. 현재 선택된 무기 타입별 자동 발사 및 갱신 로직 직접 수행
        UpdateCurrentGunLogic();
    }

    #region 무기 전환 및 초기화

    public void SetGunT
        
        ype(GunType newGunType)
    {
        currentGunType = newGunType;
        Debug.Log($"[TotalGun] 활성 무기 직접 전환 -> {currentGunType}");
    }

    private void HandleWeaponSwitchInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetGunType(GunType.DefaultGun);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SetGunType(GunType.KunaiGun);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SetGunType(GunType.ShotGun);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SetGunType(GunType.RocketLauncher);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) SetGunType(GunType.SoccerGun);
        else if (Input.GetKeyDown(KeyCode.Alpha6)) SetGunType(GunType.BlockGun);
        else if (Input.GetKeyDown(KeyCode.Alpha7)) SetGunType(GunType.LightningShield);
    }

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

    private Vector2 GetPlayerFacingDirection()
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

    #endregion

    #region 무기별 Update 로직

    private void UpdateCurrentGunLogic()
    {
        switch (currentGunType)
        {
            case GunType.DefaultGun:
                defaultFireTimer += Time.deltaTime;
                if (defaultFireTimer >= defaultFireInterval)
                {
                    defaultFireTimer = 0f;
                    ShotDefault(GetPlayerFacingDirection());
                }
                break;

            case GunType.KunaiGun:
                kunaiFireTimer += Time.deltaTime;
                if (kunaiFireTimer >= kunaiFireInterval)
                {
                    kunaiFireTimer = 0f;
                    ShotKunai();
                }
                break;

            case GunType.ShotGun:
                shotgunFireTimer += Time.deltaTime;
                UpdateShotgunTarget();
                if (shotgunFireTimer >= shotgunFireInterval)
                {
                    shotgunFireTimer = 0f;
                    ShotShotgun(GetPlayerFacingDirection());
                }
                break;

            case GunType.RocketLauncher:
                rocketFireTimer += Time.deltaTime;
                if (rocketFireTimer >= rocketFireInterval)
                {
                    rocketFireTimer = 0f;
                    ShotRocket();
                }
                break;

            case GunType.SoccerGun:
                soccerFireTimer += Time.deltaTime;
                if (soccerFireTimer >= soccerFireInterval)
                {
                    soccerFireTimer = 0f;
                    ShotSoccer();
                }
                break;

            case GunType.BlockGun:
                UpdateBlockTargetPosition();
                if (Time.time >= blockLastShotTime + blockAttackInterval)
                {
                    blockLastShotTime = Time.time;
                    ShotBlock();
                }
                break;

            case GunType.LightningShield:
                UpdateLightningTargets();
                lightningTimer += Time.deltaTime;
                if (lightningTimer >= lightningStrikeInterval)
                {
                    lightningTimer = 0f;
                    ShotLightning();
                }
                break;
        }
    }

    #endregion

    #region 외부 통합 발사 진입점 (Shot)

    public void Shot()
    {
        Vector3 defaultDir = GetPlayerFacingDirection();
        Shot(defaultDir);
    }

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
                ShotBlock();
                break;
            case GunType.LightningShield:
                ShotLightning();
                break;
        }
    }

    #endregion

    #region 1. Default Gun 발사 로직

    public void ShotDefault(Vector3 dir)
    {
        if (prefabDefaultBullet == null) return;

        Vector3 spawnPos = GetSpawnPosition(dir);
        GameObject copyBullet = Instantiate(prefabDefaultBullet, spawnPos, Quaternion.identity);
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(dir, master, defaultShotPower);
        }
    }

    #endregion

    #region 2. Kunai Gun 발사 로직

    public void ShotKunai()
    {
        Transform nearestEnemy = FindNearestEnemy(transform.position, kunaiSearchRadius);
        if (nearestEnemy != null)
        {
            ShotKunai(nearestEnemy);
        }
        else
        {
            ShotKunai((Vector3)GetPlayerFacingDirection());
        }
    }

    public void ShotKunai(Transform target)
    {
        if (target == null) return;
        Vector3 spawnPos = GetSpawnPosition();
        Vector2 dir = (target.position - spawnPos).normalized;

        GameObject copyBullet = Instantiate(prefabKunaiBullet, spawnPos, Quaternion.identity);
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(dir, master, kunaiShotPower);
        }
    }

    public void ShotKunai(Vector3 dir)
    {
        if (prefabKunaiBullet == null) return;
        Vector3 spawnPos = GetSpawnPosition();
        GameObject copyBullet = Instantiate(prefabKunaiBullet, spawnPos, Quaternion.identity);
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(dir, master, kunaiShotPower);
        }
    }

    #endregion

    #region 3. Shot Gun 발사 로직

    private void UpdateShotgunTarget()
    {
        if (!IsTargetValid(shotgunCurrentTarget, shotgunSearchRadius))
        {
            shotgunCurrentTarget = FindNearestEnemy(transform.position, shotgunSearchRadius);
        }
    }

    public void ShotShotgun(Vector3 dir)
    {
        if (prefabShotgunBullet == null) return;

        UpdateShotgunTarget();

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

        GameObject copyBullet = Instantiate(prefabShotgunBullet, spawnPos, Quaternion.identity);
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(finalDir, master, shotgunShotPower);
        }
    }

    #endregion

    #region 4. Rocket Launcher 발사 로직

    public void ShotRocket()
    {
        if (prefabRocketBullet == null) return;

        Vector3 launchPosition = GetSpawnPosition();
        Transform nearestEnemy = FindNearestEnemy(transform.position, rocketSearchRadius);
        Vector2 launchDir = nearestEnemy != null ? (Vector2)(nearestEnemy.position - launchPosition).normalized : GetPlayerFacingDirection();

        GameObject rocketObj = Instantiate(prefabRocketBullet, launchPosition, Quaternion.identity);
        Bullet bulletScript = rocketObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Init(launchDir, master, rocketLaunchForce);
        }
    }

    public void ShotRocket(Vector3 dir)
    {
        if (prefabRocketBullet == null) return;
        Vector3 launchPosition = GetSpawnPosition();
        GameObject rocketObj = Instantiate(prefabRocketBullet, launchPosition, Quaternion.identity);
        Bullet bulletScript = rocketObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Init(dir, master, rocketLaunchForce);
        }
    }

    #endregion

    #region 5. Soccer Gun 발사 로직

    public void ShotSoccer()
    {
        if (prefabSoccerBullet == null) return;

        Vector3 spawnPos = GetSpawnPosition();
        Transform target = FindNearestEnemy(transform.position, soccerSearchRadius);
        Vector2 dir = target != null ? (Vector2)(target.position - spawnPos).normalized : GetPlayerFacingDirection();

        GameObject soccerObj = Instantiate(prefabSoccerBullet, spawnPos, Quaternion.identity);
        SoccerBullet bullet = soccerObj.GetComponent<SoccerBullet>();
        if (bullet != null)
        {
            bullet.Init(dir, master, soccerShotPower);
        }
    }

    public void ShotSoccer(Vector3 dir)
    {
        if (prefabSoccerBullet == null) return;

        Vector3 spawnPos = GetSpawnPosition();
        GameObject soccerObj = Instantiate(prefabSoccerBullet, spawnPos, Quaternion.identity);
        SoccerBullet bullet = soccerObj.GetComponent<SoccerBullet>();
        if (bullet != null)
        {
            bullet.Init(dir, master, soccerShotPower);
        }
    }

    #endregion

    #region 6. Block Gun 발사 로직

    private void UpdateBlockTargetPosition()
    {
        blockTargetTransform = FindNearestEnemy(transform.position, blockRange);
        if (blockTargetTransform == null)
        {
            blockTargetTransform = blockDefaultTarget;
        }
    }

    public void ShotBlock()
    {
        if (prefabBlockBullet == null) return;

        UpdateBlockTargetPosition();

        Vector3 spawnPos = GetSpawnPosition();
        GameObject copyBullet = Instantiate(prefabBlockBullet, spawnPos, Quaternion.identity);
        BlockBullet blockBullet = copyBullet.GetComponent<BlockBullet>();

        if (blockBullet == null) return;

        Vector2 targetPos = blockTargetTransform != null ? (Vector2)blockTargetTransform.position : (Vector2)spawnPos + (GetPlayerFacingDirection() * 3f);
        Vector3 forceOffsetPosition = spawnPos + new Vector3(0.1f, 0.1f, 0f);
        Vector2 launchVelocity = CalculateBallisticVelocity(spawnPos, targetPos, blockFlightTime);

        blockBullet.InitBulletWithVelocity(launchVelocity, forceOffsetPosition, master);
    }

    private Vector2 CalculateBallisticVelocity(Vector2 startPos, Vector2 targetPos, float time)
    {
        Vector2 vDist = targetPos - startPos;
        float gravity = Mathf.Abs(Physics2D.gravity.y);

        if (time <= 0.05f) time = 0.05f;

        float vx = vDist.x / time;
        float vy = (vDist.y / time) + (0.5f * gravity * time);

        return new Vector2(vx, vy);
    }

    #endregion

    #region 7. Lightning Shield 발사 로직

    private void UpdateLightningTargets()
    {
        lightningCurrentTargets.Clear();

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, lightningDetectionRadius, monsterLayer);
        if (enemies.Length == 0) return;

        int targetCount = Mathf.Min(enemies.Length, lightningMaxTargets);
        for (int i = 0; i < targetCount; i++)
        {
            if (enemies[i] != null)
            {
                lightningCurrentTargets.Add(enemies[i].transform);
            }
        }
    }

    public void ShotLightning()
    {
        UpdateLightningTargets();

        if (prefabLightningBullet == null || lightningCurrentTargets.Count == 0) return;

        foreach (Transform target in lightningCurrentTargets)
        {
            if (target == null) continue;

            Vector3 spawnPosition = target.position;
            GameObject copyBullet = Instantiate(prefabLightningBullet, spawnPosition, Quaternion.identity);

            LightningBullet bullet = copyBullet.GetComponent<LightningBullet>();
            if (bullet != null)
            {
                bullet.master = this.master;
            }
        }
    }

    #endregion

    #region 탐색 헬퍼 함수

    private Transform FindNearestEnemy(Vector3 origin, float radius)
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

    private bool IsTargetValid(Transform target, float maxRadius)
    {
        if (target == null) return false;
        if (!target.gameObject.activeInHierarchy) return false;

        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= maxRadius;
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
                Gizmos.DrawWireSphere(currentPos, shotgunSearchRadius);

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
                Gizmos.DrawWireSphere(currentPos, blockRange);
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
                Gizmos.DrawWireSphere(currentPos, lightningDetectionRadius);
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
                Gizmos.DrawWireSphere(currentPos, kunaiSearchRadius);
                break;

            case GunType.RocketLauncher:
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(currentPos, rocketSearchRadius);
                break;

            case GunType.SoccerGun:
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(currentPos, soccerSearchRadius);
                break;
        }
    }

    #endregion
}
