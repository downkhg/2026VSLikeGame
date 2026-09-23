using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGun : MonoBehaviour
{
    public GameObject prefabBlockBullet;
    public float shotSpeed = 12f;
    public float maxDistance = 6f;

    public LayerMask monsterLayer;
    public float range = 8f;

    [Header("타겟팅 및 기본 탄착점 설정")]
    [Header("총구 (FirePoint) 위치")]
    public Transform firePoint;

    [Header("타겟팅 및 기본 탄착점 설정")]
    public Transform defaultTarget;
    public Transform targetTransform = null;

    [Header("쿨타임 설정")]
    public float attackInterval = 2.0f;
    private float lastShotTime = 0f;

    private Player ownerPlayer;

    public Vector3 GetSpawnPosition()
    {
        return firePoint != null ? firePoint.position : transform.position;
    }

    private void Awake()
    {
        ownerPlayer = GetComponentInParent<Player>();
        if (monsterLayer == 0)
        {
            monsterLayer = 1 << LayerMask.NameToLayer("Monster");
        }

        if (defaultTarget == null)
        {
            Transform found = transform.Find("DefultTarget");
            if (found == null) found = transform.Find("DefaultTarget");
            if (found != null) defaultTarget = found;
        }
    }

    private void Update()
    {
        // 씬 뷰 기즈모 및 탄착점 확인을 위해 타겟 위치 실시간 갱신
        UpdateTargetPosition();

        if (Time.time >= lastShotTime + attackInterval)
        {
            Shot(ownerPlayer);
            lastShotTime = Time.time;
        }
    }

    private void UpdateTargetPosition()
    {
        targetTransform = GetNearestMonsterTransform();
        if (targetTransform == null)
        {
            targetTransform = defaultTarget;
        } 
    }

    public void Shot(Player customMaster = null)
    {
        if (customMaster != null) ownerPlayer = customMaster;
        if (prefabBlockBullet == null) return;

        // 발사 직전 가장 가까운 적 탐색 및 탄착점 최신화
        UpdateTargetPosition();

        Vector3 spawnPos = GetSpawnPosition();
        GameObject copyBullet = Instantiate(prefabBlockBullet, spawnPos, Quaternion.identity);
        BlockBullet blockBullet = copyBullet.GetComponent<BlockBullet>();

        if (blockBullet == null) return;

        Vector2 targetPos = targetTransform.position;
        Vector3 forceOffsetPosition = spawnPos + new Vector3(0.1f, 0.1f, 0f);
        Vector2 launchVelocity = CalculateBallisticVelocity(spawnPos, targetPos, flightTime);

        blockBullet.InitBulletWithVelocity(launchVelocity, forceOffsetPosition, ownerPlayer);

        Debug.Log($"[BlockGun({this.gameObject.name})] 블록 발사! 발사위치: {spawnPos} | 탄착 목표: {targetPos} | 계산된 초기 속도: {launchVelocity}");
    }

    private Transform GetNearestMonsterTransform()
    {
        Collider2D[] monsters = Physics2D.OverlapCircleAll(transform.position, range, monsterLayer);
        if (monsters.Length == 0) return null;

        GameObject nearestMonster = null;
        float minDistance = float.MaxValue;

        foreach (var monster in monsters)
        {
            if (monster == null || !monster.gameObject.activeInHierarchy) continue;

            float dist = Vector3.Distance(transform.position, monster.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestMonster = monster.gameObject;
            }
        }

        return nearestMonster != null ? nearestMonster.transform : null;
    }

    [Header("역탄도 (포물선) 설정")]
    public float flightTime = 1.0f; // 목표 지점까지 도달하는 시간 (초)

    private Vector2 CalculateBallisticVelocity(Vector2 startPos, Vector2 targetPos, float time)
    {
        // 강의 슬라이드 공식 적용:
        // vDist = target - start
        // vx = vDist.x / Time
        // vy = H = (vDist.y / Time) + (G / 2 * Time)
        Vector2 vDist = targetPos - startPos;
        float gravity = Mathf.Abs(Physics2D.gravity.y); // Unity 기본 9.81f

        if (time <= 0.05f) time = 0.05f; // 0으로 나누기 방지

        float vx = vDist.x / time;
        float vy = (vDist.y / time) + (0.5f * gravity * time);

        return new Vector2(vx, vy);
    }

    private void OnDrawGizmos()
    {
        // 1. 탐색 범위 (빨간 원)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);

        if (targetTransform == null) return;
        Vector2 targetPos = targetTransform.position;
        // 2. 탄착점 표시 (노란 구 및 십자선)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPos, 0.35f);
        Gizmos.DrawLine(targetPos + Vector2.left * 0.5f, targetPos + Vector2.right * 0.5f);
        Gizmos.DrawLine(targetPos + Vector2.down * 0.5f, targetPos + Vector2.up * 0.5f);

        // 3. 발사 위치 -> 탄착점 조준선 (초록 선)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, targetPos);
    }
}
