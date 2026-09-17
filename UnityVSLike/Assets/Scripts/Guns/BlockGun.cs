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

    [Header("쿨타임 설정")]
    public float attackInterval = 2.0f;
    private float lastShotTime = 0f;

    private Player ownerPlayer;
    private Vector2 targetPos;

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

        Transform targetTransform = GetNearestMonsterTransform();

        GameObject copyBullet = Instantiate(prefabBlockBullet, transform.position, Quaternion.identity);
        BlockBullet blockBullet = copyBullet.GetComponent<BlockBullet>();

        if (blockBullet == null) return;

        Vector3 forceOffsetPosition = transform.position + new Vector3(0.1f, 0.1f, 0f);

        if (targetTransform != null)
        {
            targetPos = targetTransform.position;
        }
        else
        {
            Vector3 defaultDir = ownerPlayer != null && ownerPlayer.GetComponent<Dynamic>() != null
                ? ownerPlayer.GetComponent<Dynamic>().dir
                : Vector3.right;
            targetPos = transform.position + (defaultDir * 1.3f + Vector3.up).normalized;
        }

        Vector2 launchVelocity = CalculateBallisticVelocity(transform.position, targetPos, 45f);
        blockBullet.InitBulletWithVelocity(launchVelocity, forceOffsetPosition, ownerPlayer);
    }

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

    private Vector2 CalculateBallisticVelocity(Vector2 startPos, Vector2 targetPos, float angleDeg)
    {
        float angleRad = angleDeg * Mathf.Deg2Rad;
        float gravity = Mathf.Abs(Physics2D.gravity.y);

        float dirX = targetPos.x - startPos.x;
        float dirY = targetPos.y - startPos.y;
        float distHorizontal = Mathf.Abs(dirX);

        float vSquare = (gravity * distHorizontal * distHorizontal) / 
                        (2 * (distHorizontal * Mathf.Tan(angleRad) - dirY) * Mathf.Pow(Mathf.Cos(angleRad), 2));

        if (vSquare <= 0 || float.IsNaN(vSquare))
        {
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