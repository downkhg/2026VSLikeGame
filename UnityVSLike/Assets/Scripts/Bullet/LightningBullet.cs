using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningBullet : MonoBehaviour
{
    public Player master;

    [Header("Lightning Damage Settings")]
    [SerializeField] private float attackRadius = 1.5f; // 낙뢰 타격 범위
    [SerializeField] private LayerMask monsterLayer;   // 몬스터 레이어

    private Vector3 vStart;

    private void Awake()
    {
        Debug.Log($"[LightningBullet Lifecycle] Awake: {gameObject.name}");
    }

    private void OnEnable()
    {
        Debug.Log($"[LightningBullet Lifecycle] OnEnable: {gameObject.name}");
    }

    void Start()
    {
        vStart = this.transform.position;
        Debug.Log($"[LightningBullet Lifecycle] Start: {vStart}/{gameObject.name}");
    }

    // 💡 Animation Event: 애니메이션의 각 프레임마다 호출되어 범위 내 몬스터를 타격
    public void DealFrameDamage()
    {
        Debug.Log($"[LightningBullet] DealFrameDamage 1: {gameObject.name}");
        Collider2D[] hitMonsters = Physics2D.OverlapCircleAll(transform.position, attackRadius, monsterLayer);

        foreach (Collider2D collision in hitMonsters)
        {
            if (collision.CompareTag("Monster"))
            {
                Player target = collision.GetComponent<Player>();
                Player attacker = master;
                if (target != null)
                {
                    if (attacker != null)
                    {
                        Debug.Log($"[LightningBullet] DealFrameDamage Attack!: {gameObject.name}");
                        attacker.Attack(target);
                    }
 
                    if (target.Death())
                    {
                        Debug.Log($"[LightningBullet] DealFrameDamage 3: {gameObject.name}");
                        GameManager.GetInstacne().monsterInventory.AddMonster(target.name);
                    }
                }
            }
        }
    }

    // 💡 Animation Event: 번개 애니메이션이 끝나는 마지막 프레임에 호출되어 오브젝트 파괴
    public void OnAnimationEnd()
    {
        Debug.Log($"[LightningBullet] Animation Ended: {gameObject.name}");
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        Debug.Log($"[LightningBullet Lifecycle] OnDisable: {vStart},{this.transform.position}/{gameObject.name}");
    }

    private void OnDestroy()
    {
        Debug.LogError($"[LightningBullet Lifecycle] OnDestroy (파괴됨): {vStart},{this.transform.position}/{gameObject.name}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}