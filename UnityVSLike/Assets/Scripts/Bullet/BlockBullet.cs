using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BlockBullet : Bullet
{
    public float destroyDelay = 3f;       // 중력 적용 후 소멸까지의 시간
    public int maxHitCount = 3;           // 최대 타격/관통 가능 횟수

    private bool isFalling = false;
    private bool isBallisticMode = false; // 역탄도 모드 여부
    private int currentHitCount = 0;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    // 1. 역탄도 모드 초기화 (계산된 포물선 속도 직접 적용)
    public void InitBulletWithVelocity(Vector2 velocity, Vector3 forcePosition, Player master = null)
    {
        if (master != null)
        {
            this.master = master;
        }

        isBallisticMode = true;
        vStart = transform.position;

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }

        // 역탄도 탄환의 최대 수명 설정 (도달 후 잔여 시간 경과 시 소멸)
        Destroy(gameObject, destroyDelay);
    }

    // 2. 직진 비행 후 낙하 모드 초기화
    public void InitBulletWithDirection(Vector3 direction, float bulletSpeed, float dist, Player master = null)
    {
        if (master != null)
        {
            this.master = master;
        }

        isBallisticMode = false;
        speed = bulletSpeed;
        maxDistance = dist;
        vStart = transform.position;

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }
    }

    // 기본 Init 오버라이드
    public override void Init(Vector2 direction, Player master, float customSpeed = -1f)
    {
        InitBulletWithDirection(direction, customSpeed > 0 ? customSpeed : speed, maxDistance, master);
    }

    protected override void Update()
    {
        // 역탄도 모드(isBallisticMode)일 때는 곡선 호를 그리므로 단순 직선 거리(fDist >= maxDistance)로 파괴하지 않음
        if (isBallisticMode)
        {
            return;
        }

        base.Update();
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

    private void EnableGravity()
    {
        if (isFalling) return;
        isFalling = true;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // 지면에 닿았을 때
        if (collision.CompareTag("Ground") || collision.CompareTag("Floor") || collision.CompareTag("Tilemap"))
        {
            return;
        }

        if (collision.gameObject.CompareTag("Monster"))
        {
            Player target = collision.gameObject.GetComponent<Player>();
            if (target != null)
            {
                OnHitMonster(target);
            }

            currentHitCount++;
            if (currentHitCount >= maxHitCount)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Tilemap"))
        {
            // 지면 충돌 처리 (필요 시)
        }
    }
}