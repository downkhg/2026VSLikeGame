using UnityEngine;

public class SoccerGun : MonoBehaviour
{
    public GameObject prefabSoccerBullet;
    public float shotPower = 12f;
    public float searchRadius = 10f;

    [Header("발사 설정")]
    public float fireInterval = 2.0f;
    private float fireTimer = 0f;

    public Player master;
    public LayerMask monsterLayer;

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

    public void Shot(Vector3 dir, Player master)
    {
        if (prefabSoccerBullet == null) return;

        GameObject soccerObj = Instantiate(prefabSoccerBullet, transform.position, Quaternion.identity);
        SoccerBullet bullet = soccerObj.GetComponent<SoccerBullet>();
        if (bullet != null)
        {
            bullet.Init(dir, master, shotPower);
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
            bullet.Init(dir, master, shotPower);
        }

        Debug.Log($"[SoccerGun] 축구공 발사 완료 | 방향: {dir}");
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