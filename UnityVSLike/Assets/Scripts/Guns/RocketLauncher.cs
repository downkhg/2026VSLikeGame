using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    public GameObject prefabRocketBullet;
    public float launchForce = 15f;
    public float searchRadius = 10f;

    [Header("발사 설정")]
    public float fireInterval = 1.5f;
    private float fireTimer = 0f;

    public Player master;
    public LayerMask monsterLayer;

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
            launchDir = transform.right;
        }

        GameObject rocketObj = Instantiate(prefabRocketBullet, launchPosition, Quaternion.identity);

        Bullet bulletScript = rocketObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Init(launchDir, master, launchForce);
        }

        Debug.Log($"[RocketLauncher] 로켓 발사 완료 | 위치: {launchPosition} | 방향: {launchDir}");
    }

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