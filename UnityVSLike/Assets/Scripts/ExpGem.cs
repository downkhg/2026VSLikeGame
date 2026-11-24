using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 뱀서라이크 스타일의 경험치 보석(ExpGem) 컴포넌트
/// 몬스터 처치 시 바닥에 드롭되며, 플레이어가 가까이 오면 자석처럼 끌려와 경험치를 지급합니다.
/// </summary>
public class ExpGem : MonoBehaviour
{
    [Header("=== 경험치 및 자석 설정 ===")]
    [Tooltip("습득 시 지급되는 경험치량")]
    public int expValue = 35;

    [Tooltip("플레이어를 감지하고 끌려가기 시작하는 자석 반경")]
    public float magnetRadius = 3.5f;

    [Tooltip("초기 흡수 이동 속도")]
    public float initialSpeed = 5.0f;

    [Tooltip("흡수 중 가속도")]
    public float acceleration = 12.0f;

    private float currentSpeed;
    private bool isMagnetized = false;
    private Transform targetPlayer;
    private Vector3 basePosition;
    private float floatTimer;

    private void Start()
    {
        basePosition = transform.position;
        currentSpeed = initialSpeed;
        floatTimer = Random.Range(0f, Mathf.PI * 2f);
        FindPlayer();
    }

    private void FindPlayer()
    {
        if (targetPlayer != null) return;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            targetPlayer = playerObj.transform;
            return;
        }

        Player playerComp = FindFirstObjectByType<Player>();
        if (playerComp != null)
        {
            targetPlayer = playerComp.transform;
        }
    }

    private void Update()
    {
        if (targetPlayer == null)
        {
            FindPlayer();
            if (targetPlayer == null) return;
        }

        float distance = Vector3.Distance(transform.position, targetPlayer.position);

        // 1. 플레이어가 자석 반경 내로 들어오면 흡수 활성화
        if (!isMagnetized && distance <= magnetRadius)
        {
            isMagnetized = true;
        }

        // 2. 자석 흡수 이동
        if (isMagnetized)
        {
            currentSpeed += acceleration * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, currentSpeed * Time.deltaTime);

            // 3. 플레이어에게 도달 시 획득 처리
            if (distance <= 0.4f)
            {
                CollectGem();
            }
        }
        else
        {
            // 미흡수 상태일 때 바닥에서 부드러운 둥둥 뜨는 애니메이션
            floatTimer += Time.deltaTime * 3f;
            float offsetY = Mathf.Sin(floatTimer) * 0.08f;
            transform.position = new Vector3(basePosition.x, basePosition.y + offsetY, basePosition.z);
        }
    }

    /// <summary>
    /// 플레이어에게 경험치를 지급하고 효과음 재생 후 소멸
    /// </summary>
    private void CollectGem()
    {
        if (targetPlayer != null)
        {
            Player player = targetPlayer.GetComponent<Player>();
            if (player != null)
            {
                player.AddExp(expValue);
            }
        }

        // 습득 효과음 재생
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayGemSound();
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// 지정된 월드 위치에 경험치 젬을 동적으로 생성합니다.
    /// </summary>
    public static GameObject Spawn(Vector3 position, int expAmount)
    {
        GameObject gemObj = new GameObject("ExpGem");
        gemObj.transform.position = position;

        // 1. 스프라이트 렌더러 부착
        SpriteRenderer sr = gemObj.AddComponent<SpriteRenderer>();
        Sprite gemSprite = Resources.Load<Sprite>("Sprites/Items/Gem/gem-1");
        if (gemSprite != null)
        {
            sr.sprite = gemSprite;
        }
        sr.sortingOrder = 5;

        // 2. 콜라이더 부착 (트리거)
        CircleCollider2D col = gemObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.25f;

        // 3. ExpGem 컴포넌트 추가 및 경험치 설정
        ExpGem gemComp = gemObj.AddComponent<ExpGem>();
        gemComp.expValue = expAmount > 0 ? expAmount : 35;

        return gemObj;
    }
}
