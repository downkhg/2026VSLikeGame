using UnityEngine;

public class SoccerGun : MonoBehaviour
{
    public GameObject prefabSoccerBullet; // 축구공 프리팹
    public float shotPower = 12f;          // 발사 힘
    public float searchRadius = 10f;       // 적 탐색 반경

    [Header("발사 설정")]
    public float fireInterval = 2.0f;     // 발사 주기 (초 단위)
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
        if (prefabSoccerBullet == null) return;

        Debug.Log($"[SoccerGun] 축구공 발사 시도 | 발사 위치: {transform.position}");

        Transform target = FindNearestEnemy();
        Vector2 dir = target != null ? (target.position - transform.position).normalized : (Vector2)transform.right;

        GameObject soccerObj = Instantiate(prefabSoccerBullet, transform.position, Quaternion.identity);

        SoccerBullet bullet = soccerObj.GetComponent<SoccerBullet>();
        if (bullet != null)
        {
            bullet.master = master;
        }

        Rigidbody2D rb = soccerObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 초기 발사 힘 전달
            rb.AddForce(dir * shotPower, ForceMode2D.Impulse);
        }

        Debug.Log($"[SoccerGun] 축구공 생성 완료 | 방향: {dir}");
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}