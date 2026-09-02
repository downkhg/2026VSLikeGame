using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    public GameObject prefabRocketBullet; // 발사할 로켓 투사체 프리팹
    public float launchForce = 15f;        // 로켓 발사 속도/힘
    public float searchRadius = 10f;       // 적 탐색 범주

    [Header("발사 설정")]
    public float fireInterval = 1.5f;     // 발사 간격 (쿨타임)
    private float fireTimer = 0f;

    public Player master;                 // 발사 주체 (플레이어)
    public LayerMask monsterLayer;        // 몬스터 레이어

    private void Awake()
    {
        if (master == null)
        {
            master = GetComponentInParent<Player>();
        }
    }

    private void Update()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            Shot(master);
        }
    }

    public void Shot(Player master)
    {
        if (prefabRocketBullet == null) return;

        // [로그 1] 발사 시도 및 발사 위치(무기 위치) 출력
        Vector3 launchPosition = transform.position;
        Debug.Log($"[RocketLauncher] 발사 시도 | 발사 위치: {launchPosition}");

        Transform nearestEnemy = FindNearestEnemy();
        Vector2 launchDir;

        if (nearestEnemy != null)
        {
            launchDir = (nearestEnemy.position - launchPosition).normalized;
        }
        else
        {
            // 적이 없으면 전방 방향으로 발사
            launchDir = transform.right;
        }

        // 로켓 프리팹 생성
        GameObject rocketObj = Instantiate(prefabRocketBullet, launchPosition, Quaternion.identity);

        // [로그 2] 객체 생성 직후 위치 출력
        Debug.Log($"[RocketLauncher] 로켓 객체 생성 완료 | 생성된 객체: {rocketObj.name} | 생성 위치: {rocketObj.transform.position}");

        // 회전 처리 (발사 방향으로 머리 돌리기)
        float angle = Mathf.Atan2(launchDir.y, launchDir.x) * Mathf.Rad2Deg;
        rocketObj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Rigidbody2D 및 RocketBullet 컴포넌트 전달
        Rigidbody2D rb = rocketObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(launchDir * launchForce, ForceMode2D.Impulse);
        }

        // 투사체에 master(플레이어) 전달
        Bullet bulletScript = rocketObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.master = master;
        }
    }

    // 가장 가까운 적 탐색 함수
    private Transform FindNearestEnemy()
    {
        Collider2D[] monsters = Physics2D.OverlapCircleAll(transform.position, searchRadius, monsterLayer);
        if (monsters.Length == 0) return null;

        Transform nearest = null;
        float minDistance = float.MaxValue;

        foreach (var monster in monsters)
        {
            float dist = Vector3.Distance(transform.position, monster.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = monster.transform;
            }
        }

        return nearest;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}