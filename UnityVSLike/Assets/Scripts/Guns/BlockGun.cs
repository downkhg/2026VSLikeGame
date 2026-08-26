using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGun : MonoBehaviour
{
    public GameObject prefabBlockBullet;
    public float shotSpeed = 12f;       // 기본 발사 속도 (대상이 없을 때 사용)
    public float maxDistance = 6f;       // 낙하하기 전까지 이동할 거리

    public LayerMask monsterLayer; // 인스펙터에서 몬스터 레이어를 지정해줘야 합니다.
    public float range = 8f;       // 탐색 범위

    [Header("Cooltime Settings")]
    public float attackInterval = 2.0f; // 쿨타임 (초 단위)
    private float lastShotTime = 0f;    // 마지막 발사 시각

    private Player ownerPlayer;

    Vector2 targetPos;

    private void Awake()
    {
        ownerPlayer = GetComponentInParent<Player>();
    }

    private void Update()
    {
        if (Time.time >= lastShotTime + attackInterval)
        {
            Shot();
            lastShotTime = Time.time;
        }
    }

    public void Shot()
    {
        if (prefabBlockBullet == null) return;

        // 발사 대상(가장 가까운 몬스터) 검색
        Transform targetTransform = GetNearestMonsterTransform();

        GameObject copyBullet = Instantiate(prefabBlockBullet, transform.position, Quaternion.identity);
        BlockBullet blockBullet = copyBullet.GetComponent<BlockBullet>();

        if (blockBullet == null) return;

        blockBullet.master = ownerPlayer;

        // 힘을 가할 오프셋 위치 (벽돌 회전을 유도하기 위해 중심보다 약간 위/옆)
        Vector3 forceOffsetPosition = transform.position + new Vector3(0.1f, 0.1f, 0f);

        if (targetTransform != null)
        {
            // 1. 대상이 있을 때: 포물선(역탄도) 속도 계산 후 발사
            targetPos = targetTransform.position;
        }
        else
        {
            // 2. 대상이 없을 때: 위쪽으로 쏘되 약간 앞(우측)으로 치우친 방향
            targetPos = transform.position + (ownerPlayer.GetComponent<Dynamic>().dir * 1.5f + Vector3.up).normalized ;
        }

        Vector2 launchVelocity = CalculateBallisticVelocity(transform.position, targetPos, 45f); // 45도 투사각 예시

        blockBullet.InitBulletWithVelocity(launchVelocity, forceOffsetPosition);
    }

    // 가장 가까운 몬스터의 Transform 반환 (없으면 null)
    private Transform GetNearestMonsterTransform()
    {
        Collider2D[] monsters = Physics2D.OverlapCircleAll(transform.position, range, monsterLayer);
        if (monsters.Length == 0) return null;

        GameObject nearestMonster = null;
        float minDistance = float.MaxValue;

        foreach (var monster in monsters)
        {
            float dist = Vector3.Distance(transform.position, monster.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestMonster = monster.gameObject;
            }
        }

        return nearestMonster != null ? nearestMonster.transform : null;
    }



    // 주어진 목표 지점(target)으로 도착하기 위한 포물선 초기 속도를 계산하는 함수
    private Vector2 CalculateBallisticVelocity(Vector2 startPos, Vector2 targetPos, float angleDeg)
    {
        float angleRad = angleDeg * Mathf.Deg2Rad;
        float gravity = Mathf.Abs(Physics2D.gravity.y) ; // BlockBullet의 gravityScale(2.5) 적용값

        float dirX = targetPos.x - startPos.x;
        float dirY = targetPos.y - startPos.y;
        float distHorizontal = Mathf.Abs(dirX);

        // 높이차와 거리를 기반으로 속도(v) 계산
        float vSquare = (gravity * distHorizontal * distHorizontal) / 
                        (2 * (distHorizontal * Mathf.Tan(angleRad) - dirY) * Mathf.Pow(Mathf.Cos(angleRad), 2));

        if (vSquare <= 0 || float.IsNaN(vSquare))
        {
            // 계산 불가한 가깝거나 특수한 위치일 경우 기본 방향 반환
            return (targetPos - startPos).normalized * shotSpeed;
        }

        float v = Mathf.Sqrt(vSquare);
        float vx = v * Mathf.Cos(angleRad) * Mathf.Sign(dirX);
        float vy = v * Mathf.Sin(angleRad);

        return new Vector2(vx, vy);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPos, 0.3f);
    }
}