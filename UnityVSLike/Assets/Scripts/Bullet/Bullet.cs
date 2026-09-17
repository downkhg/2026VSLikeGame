using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    public Player master;
    public float speed = 10f;       // 탄환 이동 속도
    public float maxDistance = 10f; // 최대 이동 가능 거리

    protected Rigidbody2D rb;
    protected Vector3 vStart;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Start()
    {
        if (vStart == Vector3.zero)
        {
            vStart = transform.position;
        }
    }

    /// <summary>
    /// Rigidbody2D 기반 공통 탄환 초기화 및 발사 메서드
    /// </summary>
    public virtual void Init(Vector2 direction, Player master, float customSpeed = -1f)
    {
        this.master = master;
        if (customSpeed > 0f)
        {
            this.speed = customSpeed;
        }

        vStart = transform.position;

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (rb != null)
        {
            // 발사 방향으로 즉시 물리 속도 적용
            rb.linearVelocity = direction.normalized * this.speed;
        }

        // 회전 각도 설정 (진행 방향을 바라보도록 정렬)
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    protected virtual void Update()
    {
        // 이동 거리 체크 후 파괴 처리
        float fDist = Vector3.Distance(vStart, transform.position);
        if (fDist >= maxDistance)
        {
            Debug.Log($"Out Distance[{fDist:F1}]: {gameObject.name}");
            Destroy(gameObject);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            Player target = collision.gameObject.GetComponent<Player>();
            if (target != null)
            {
                OnHitMonster(target);
            }
        }
    }

    /// <summary>
    /// 몬스터 피격 시 처리 가상 메서드 (자식 클래스에서 재정의 가능)
    /// </summary>
    protected virtual void OnHitMonster(Player target)
    {
        Player attacker = master;

        if (attacker != null)
        {
            attacker.Attack(target);
        }

        if (target != null && target.Death())
        {
            if (GameManager.GetInstacne() != null && GameManager.GetInstacne().monsterInventory != null)
            {
                GameManager.GetInstacne().monsterInventory.AddMonster(target.name);
            }
        }
    }

    protected virtual void OnDisable()
    {
        Debug.Log($"[Bullet Lifecycle] OnDisable: {vStart},{this.transform.position}/{gameObject.name}");
    }

    protected virtual void OnDestroy()
    {
        Debug.Log($"[Bullet Lifecycle] OnDestroy (파괴됨): {vStart},{this.transform.position}/{gameObject.name}");
    }
}