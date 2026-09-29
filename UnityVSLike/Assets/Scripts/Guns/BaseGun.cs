using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 총기/무기의 최상위 추상 기반 클래스 (상속과 다형성의 뼈대)
/// 공통 속성(파워, 쿨타임, 사거리, 프리팹)과 자동/수동(액티브) 발사 루틴을 캡슐화합니다.
/// </summary>
public abstract class BaseGun : MonoBehaviour
{
    [Header("=== 공통 기본 설정 (Base Settings) ===")]
    [Tooltip("탄환 발사 속도 및 위력")]
    public float shotPower = 10f;

    [Tooltip("자동 발사 주기 / 쿨타임 (초)")]
    public float fireInterval = 0.5f;

    [Tooltip("적 탐색 및 사거리 반경")]
    public float searchRadius = 10f;

    [Tooltip("발사할 탄환 프리팹")]
    public GameObject prefabBullet;

    [Header("=== 발사 타이머 및 소유자 ===")]
    [SerializeField] protected float fireTimer = 0f;
    public float FireTimer => fireTimer;

    public Transform firePoint;
    public Vector3 defaultFirePointOffset = new Vector3(0.5f, 0f, 0f);
    public Player master;
    public LayerMask monsterLayer;

    public GunInventory gunInventory;

    [Header("=== 액티브 / 자동 발사 제어 ===")]
    [Tooltip("체크 시 쿨타임(fireInterval)마다 자동 발사. 체크 해제 시 수동 액티브(ActiveShot) 호출로만 발사.")]
    public bool isAutoFire = true;

    protected virtual void Awake()
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
        InitDefaultSettings();

        if (gunInventory == null)
        {
            gunInventory = GetComponentInParent<GunInventory>();
            if (gunInventory == null)
            {
                gunInventory = GetComponent<GunInventory>();
            }
        }
    }

    /// <summary>
    /// 각 자식 무기 클래스가 기본 스펙(스피드, 쿨타임, 사거리, 프리팹 등)을 초기화할 때 오버라이드합니다.
    /// </summary>
    protected virtual void InitDefaultSettings() { }

    protected virtual void Update()
    {
        // 1. 타겟 갱신 (타겟팅이 필요한 자식 무기만 오버라이드하여 수행)
        UpdateTarget();

        // 2. 자동 발사가 활성화된 무기만 타이머를 누적하여 발사
        if (isAutoFire)
        {
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireInterval)
            {
                fireTimer = 0f;
                // 다형성(Polymorphism): 자식 클래스에서 오버라이드한 Shot()이 가상함수 테이블을 통해 호출됨
                Shot(GetPlayerFacingDirection());
            }
        }
    }

    /// <summary>
    /// 매 프레임 타겟 탐색 및 위치 갱신이 필요한 경우 자식 클래스에서 오버라이드합니다.
    /// </summary>
    protected virtual void UpdateTarget() { }

    /// <summary>
    /// [핵심 다형성 메서드] 각 자식 무기가 고유한 발사/공격 메커니즘을 구현해야 하는 추상 메서드입니다.
    /// </summary>
    /// <param name="dir">발사 기본 방향 벡터</param>
    public abstract void Shot(Vector3 dir);

    /// <summary>
    /// 기본 발사: 플레이어가 바라보는 방향으로 발사
    /// </summary>
    public virtual void Shot()
    {
        Shot((Vector3)GetPlayerFacingDirection());
    }

    /// <summary>
    /// 쿨타임(fireInterval) 대기 없이 즉각적으로 격발하는 액티브 발사 메서드
    /// </summary>
    /// <param name="dir">발사 방향 벡터</param>
    public virtual void ActiveShot(Vector3 dir)
    {
        // 쿨타임 타이머와 상관없이 즉각 Shot() 실행
        Shot(dir);
    }

    /// <summary>
    /// 쿨타임(fireInterval) 대기 없이 플레이어가 바라보는 방향으로 즉각 액티브 발사
    /// </summary>
    public virtual void ActiveShot()
    {
        ActiveShot((Vector3)GetPlayerFacingDirection());
    }

    #region 공통 유틸리티 메서드 (자식 클래스들이 자유롭게 재사용)

    protected virtual void InitFirePoint()
    {
        if (firePoint == null)
        {
            // 1. 부모/조상, 소유자(Player), 또는 씬 전체에서 GunDumy의 firepoint를 최우선 탐색
            Transform dummyFp = FindDummyFirePoint();
            if (dummyFp != null)
            {
                firePoint = dummyFp;
                return;
            }

            // 2. 자체 자식에서 FirePoint 탐색
            Transform found = transform.Find("FirePoint");
            if (found == null) found = transform.Find("firepoint");

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

    /// <summary>
    /// GunDumy 하위에 위치한 firepoint 트랜스폼을 계층 구조에서 탐색합니다.
    /// </summary>
    private Transform FindDummyFirePoint()
    {
        // 1) 직계 부모 또는 상위 계층에서 GunDumy 탐색
        Transform current = transform;
        while (current != null)
        {
            if (current.name.IndexOf("GunDum", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Transform fp = current.Find("firepoint");
                if (fp == null) fp = current.Find("FirePoint");
                if (fp != null) return fp;
            }
            current = current.parent;
        }

        // 2) 소유자 master(Player) 하위에서 GunDumy 탐색
        if (master != null)
        {
            Transform dummy = master.transform.Find("GunDumy");
            if (dummy != null)
            {
                Transform fp = dummy.Find("firepoint");
                if (fp == null) fp = dummy.Find("FirePoint");
                if (fp != null) return fp;
            }
        }

        // 3) 씬 전체에서 GunDumy 탐색 (Fallback)
        GameObject dummyObj = GameObject.Find("GunDumy");
        if (dummyObj != null)
        {
            Transform fp = dummyObj.transform.Find("firepoint");
            if (fp == null) fp = dummyObj.transform.Find("FirePoint");
            if (fp != null) return fp;
        }

        return null;
    }

    /// <summary>
    /// 탄환 생성 위치: firePoint(더미건의 파이어포인트)가 설정되어 있으면 무조건 해당 위치를 반환합니다.
    /// </summary>
    public virtual Vector3 GetSpawnPosition()
    {
        return firePoint != null ? firePoint.position : transform.position;
    }

    /// <summary>
    /// 탄환 생성 위치: firePoint(더미건의 파이어포인트)가 설정되어 있으면 방향과 무관하게 무조건 해당 위치를 반환합니다.
    /// </summary>
    public virtual Vector3 GetSpawnPosition(Vector3 dir)
    {
        if (firePoint != null)
        {
            return firePoint.position;
        }
        return transform.position + (dir.normalized * defaultFirePointOffset.magnitude);
    }

    public virtual Vector2 GetPlayerFacingDirection()
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

    public virtual Transform FindNearestEnemy(Vector3 origin, float radius)
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

    public virtual bool IsTargetValid(Transform target, float maxRadius)
    {
        if (target == null) return false;
        if (!target.gameObject.activeInHierarchy) return false;

        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= maxRadius;
    }

    public virtual Vector2 CalculateBallisticVelocity(Vector2 startPos, Vector2 targetPos, float time)
    {
        Vector2 vDist = targetPos - startPos;
        float gravity = Mathf.Abs(Physics2D.gravity.y);

        if (time <= 0.05f) time = 0.05f;

        float vx = vDist.x / time;
        float vy = (vDist.y / time) + (0.5f * gravity * time);

        return new Vector2(vx, vy);
    }

    #endregion

    protected virtual void OnDrawGizmos() { }
}
