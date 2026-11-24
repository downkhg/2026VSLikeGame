using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Opossum : MonoBehaviour
{
    [Header("=== 이동 설정 ===")]
    [Tooltip("탑다운 이동 속도")]
    public float moveSpeed = 1.2f;
    [Tooltip("추적 대상 플레이어")]
    public Transform playerTarget;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        FindPlayerTarget();
    }

    /// <summary>
    /// 씬에서 플레이어 트랜스폼을 탐색합니다.
    /// </summary>
    public void FindPlayerTarget()
    {
        if (playerTarget != null) return;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
            return;
        }

        Player playerComp = FindFirstObjectByType<Player>();
        if (playerComp != null)
        {
            playerTarget = playerComp.transform;
        }
    }

    void Update()
    {
        if (playerTarget == null)
        {
            FindPlayerTarget();
            // 플레이어가 없는 경우 기본 좌측 이동 (플랫포머 모드 호환)
            if (playerTarget == null)
            {
                transform.position += Vector3.left * moveSpeed * Time.deltaTime;
                return;
            }
        }

        // 1. 탑다운 무한 맵: 플레이어를 향해 부드럽게 추적 이동
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // 2. 진행 방향에 따른 스프라이트 좌우 반전
        if (spriteRenderer != null && Mathf.Abs(direction.x) > 0.05f)
        {
            spriteRenderer.flipX = direction.x > 0f;
        }
    }

    private void FixedUpdate()
    {
        CheckAndAttackPlayer();
    }

    /// <summary>
    /// 주변 콜라이더 오버랩을 통한 플레이어 접촉 공격 판정
    /// </summary>
    private void CheckAndAttackPlayer()
    {
        int playerLayerMask = 1 << LayerMask.NameToLayer("Player");
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null) return;

        Collider2D hitCollider = Physics2D.OverlapBox(transform.position, boxCollider.size, 0f, playerLayerMask);
        if (hitCollider != null)
        {
            ExecuteAttack(hitCollider.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ExecuteAttack(collision.gameObject);
        }
    }

    /// <summary>
    /// 플레이어에게 대미지를 가하고 무적 모드를 발동합니다.
    /// </summary>
    private void ExecuteAttack(GameObject targetObject)
    {
        if (targetObject == null) return;

        Player target = targetObject.GetComponent<Player>();
        Player attacker = GetComponent<Player>();

        if (target != null && !target.isDead)
        {
            SuperMode superMode = target.GetComponent<SuperMode>();
            bool canAttack = (superMode == null || !superMode.isUse);

            if (canAttack)
            {
                if (attacker != null)
                {
                    attacker.Attack(target);
                }
                else
                {
                    target.OnDamaged(10);
                }

                if (superMode != null)
                {
                    superMode.OnMode();
                }

                Debug.Log($"[Opossum] 🦝 오포섬이 플레이어 '{target.name}' 공격 성공!");
            }
        }
    }
}
