using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningShield : MonoBehaviour
{
    // Gun.cs와 동일하게 마스터(주인) 플레이어 참조
    public Player master;

    [Header("Bullet Prefab")]
    [SerializeField] private GameObject lightningBulletPrefab;

    [Header("Shield Settings")]
    [SerializeField] private float detectionRadius = 5f;  // 감지 범위
    [SerializeField] private float strikeInterval = 1.5f; // 낙뢰 주기
    [SerializeField] private int maxTargetsPerStrike = 3; // 최대 타깃 수
    [SerializeField] private LayerMask enemyLayer;       // 몬스터 레이어

    private float timer;
    private List<Transform> currentTargets = new List<Transform>(); // Gizmo 표시용 타겟 리스트

    private void Awake()
    {
        // 동일 오브젝트에 Player 스크립트가 있다면 자동으로 master 할당
        if (master == null)
        {
            master = GetComponent<Player>();
        }
    }

    private void Update()
    {
        // 씬 뷰 기즈모 갱신을 위해 매 프레임 타겟 탐색
        UpdateTargets();

        timer += Time.deltaTime;

        if (timer >= strikeInterval)
        {
            CastLightning();
            timer = 0f;
        }
    }

    private void UpdateTargets()
    {
        currentTargets.Clear();

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        if (enemies.Length == 0) return;

        int targetCount = Mathf.Min(enemies.Length, maxTargetsPerStrike);
        for (int i = 0; i < targetCount; i++)
        {
            currentTargets.Add(enemies[i].transform);
        }
    }

    private void CastLightning()
    {
        if (lightningBulletPrefab == null || currentTargets.Count == 0) return;

        foreach (Transform target in currentTargets)
        {
            if (target == null) continue;

            Vector3 spawnPosition = target.position;
            GameObject copyBullet = Instantiate(lightningBulletPrefab, spawnPosition, Quaternion.identity);

            // Gun.cs의 Shot 메서드처럼 생성된 Bullet에 master를 전달
            LightningBullet bullet = copyBullet.GetComponent<LightningBullet>();
            if (bullet != null)
            {
                bullet.master = this.master; //[cite: 3]
            }
        }
    }

    private void OnDrawGizmos()
    {
        // 1. 감지 범위 표시 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // 2. 타겟팅된 몬스터 조준선 및 원 표시 (빨간색)
        if (Application.isPlaying && currentTargets != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform target in currentTargets)
            {
                if (target != null)
                {
                    Gizmos.DrawLine(transform.position, target.position);
                    Gizmos.DrawWireSphere(target.position, 0.5f);
                }
            }
        }
    }
}