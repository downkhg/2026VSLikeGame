using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BlockBullet : MonoBehaviour
{
    public Player master;
    public float speed = 10f;             // 초기 날아가는 속도 (타겟 없을 시)
    public float maxDistance = 6f;        // 중력이 적용되기 전까지 이동할 거리
    public float destroyDelay = 3f;       // 중력 적용 후 소멸까지의 시간
    public int maxHitCount = 3;           // 최대 타격/관통 가능 횟수

    private Vector3 vStart;
    private Rigidbody2D rb;
    private bool isFalling = false;
    private bool isBallisticMode = false; // 역탄도 모드 여부
    private int currentHitCount = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        vStart = transform.position;
    }

    void FixedUpdate()
    {
        // 타겟 없는 직진 비행 모드일 때만 거리 체크하여 낙하 전환
        if (!isBallisticMode && !isFalling)
        {
            float fDist = Vector3.Distance(vStart, transform.position);
            if (fDist >= maxDistance)
            {
                EnableGravity();
            }
        }
    }

    // 1. 대상이 있을 때 (역탄도 모드): 처음부터 중력을 켜고 계산된 포물선 속도 적용
    public void InitBulletWithVelocity(Vector2 velocity, Vector3 forcePosition)
    {
        isBallisticMode = true;

        // 원하는 위치에 충격량 전달
        //rb.AddForceAtPosition(velocity, forcePosition, ForceMode2D.Impulse);
        rb.linearVelocity = velocity;

        // 자동 소멸 타이머 시작
        //Destroy(gameObject, destroyDelay + 2f);
    }

    // 2. 대상이 없을 때: 윗쪽+약간 앞으로 직진 비행 후 중력 적용
    public void InitBulletWithDirection(Vector3 direction, float bulletSpeed, float dist)
    {
        isBallisticMode = false;
        speed = bulletSpeed;
        maxDistance = dist;
        rb.AddForce(direction.normalized * speed, ForceMode2D.Impulse);
    }

    // 지정 거리 도달 시 중력 적용 (타겟 없는 모드용)
    private void EnableGravity()
    {
        if (isFalling) return;
        isFalling = true;

        //Destroy(gameObject, destroyDelay);
    }

    // Trigger 콜라이더와 충돌 시 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 땅(지면)에 닿았을 때 삭제 처리
        if (collision.CompareTag("Ground") || collision.CompareTag("Floor") || collision.CompareTag("Tilemap"))
        {
            //Destroy(gameObject);
            return;
        }

        if (collision.gameObject.CompareTag("Monster"))
        {
            Player target = collision.gameObject.GetComponent<Player>();
            Player attacker = master;

            if (target != null && target.Death())
            {
                attacker.Attack(target);
                GameManager.GetInstacne().monsterInventory.AddMonster(target.name);
            }

            currentHitCount++;
            if (currentHitCount >= maxHitCount)
            {
                //Destroy(gameObject);
            }
        }
    }

    // 일반 Collider2D(Is Trigger가 꺼진 물리 콜라이더)로 지면과 충돌했을 때도 삭제 처리
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Tilemap"))
        {
            //Destroy(gameObject);
        }
    }
}