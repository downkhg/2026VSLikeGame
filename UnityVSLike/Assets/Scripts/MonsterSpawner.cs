using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 뱀서라이크 스타일의 무한 맵 동적 몬스터 스포너
/// 플레이어가 무한 맵을 자유롭게 이동할 때, 카메라 시야 밖(일정 반경)에서 몬스터들을 지속적으로 소환합니다.
/// </summary>
public class MonsterSpawner : MonoBehaviour
{
    [Header("=== 스폰 대상 몬스터 프리팹 목록 ===")]
    [Tooltip("소환할 몬스터 프리팹 배열입니다. 비어 있으면 Resources/Prefabs에서 opossum, eagle을 자동 로드합니다.")]
    public GameObject[] monsterPrefabs;

    [Header("=== 추적 플레이어 ===")]
    public Transform playerTransform;

    [Header("=== 스폰 범위 설정 ===")]
    [Tooltip("카메라 시야 밖 최소 스폰 반경 (카메라 화면에 바로 나타나지 않도록 설정)")]
    public float minSpawnRadius = 8.5f;

    [Tooltip("최대 스폰 반경")]
    public float maxSpawnRadius = 13.5f;

    [Tooltip("플레이어와 너무 멀어져 제거(재스폰)할 한계 거리")]
    public float despawnRadius = 25.0f;

    [Header("=== 스폰 주기 및 수량 제한 ===")]
    [Tooltip("스폰 주기 (초)")]
    public float spawnInterval = 2.0f;

    [Tooltip("필드에 동시에 존재할 수 있는 최대 몬스터 수")]
    public int maxAliveMonsters = 30;

    [Tooltip("스폰 활성화 여부")]
    public bool isSpawning = true;

    [Header("=== 현재 생존 몬스터 목록 ===")]
    public List<GameObject> aliveMonsters = new List<GameObject>();

    private float spawnTimer = 0f;
    private float elapsedTime = 0f;

    private void Awake()
    {
        // 프리팹 배열이 비어있을 경우 Resources/Prefabs에서 기본 몬스터들 자동 로드
        if (monsterPrefabs == null || monsterPrefabs.Length == 0)
        {
            List<GameObject> loadedList = new List<GameObject>();
            GameObject opossumPrefab = Resources.Load<GameObject>("Prefabs/opossum");
            if (opossumPrefab != null) loadedList.Add(opossumPrefab);

            GameObject eaglePrefab = Resources.Load<GameObject>("Prefabs/eagle");
            if (eaglePrefab != null) loadedList.Add(eaglePrefab);

            monsterPrefabs = loadedList.ToArray();
            Debug.Log($"[MonsterSpawner] 기본 몬스터 프리팹 {monsterPrefabs.Length}종 자동 로드 완료");
        }
    }

    private void Start()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        if (playerTransform != null) return;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            return;
        }

        Player playerComp = FindFirstObjectByType<Player>();
        if (playerComp != null)
        {
            playerTransform = playerComp.transform;
        }
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return;
        }

        if (!isSpawning) return;

        elapsedTime += Time.deltaTime;

        // 시간 경과에 따라 스폰 주기 점진적 단축 (난이도 점진 상승: 2.0초 -> 최저 0.8초)
        float currentInterval = Mathf.Max(0.8f, spawnInterval - (elapsedTime * 0.005f));

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= currentInterval)
        {
            spawnTimer = 0f;
            CleanUpAndDespawnFarMonsters();

            if (aliveMonsters.Count < maxAliveMonsters)
            {
                SpawnMonsterNearPlayer();
            }
        }
    }

    /// <summary>
    /// 플레이어 주변 카메라 시야 밖 원형 영역에서 랜덤한 위치에 몬스터를 생성합니다.
    /// </summary>
    public void SpawnMonsterNearPlayer()
    {
        if (monsterPrefabs == null || monsterPrefabs.Length == 0) return;

        // 1. 랜덤 각도 및 반경 산출
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float randomRadius = Random.Range(minSpawnRadius, maxSpawnRadius);

        Vector3 spawnOffset = new Vector3(Mathf.Cos(randomAngle) * randomRadius, Mathf.Sin(randomAngle) * randomRadius, 0f);
        Vector3 spawnPosition = playerTransform.position + spawnOffset;

        // 2. 랜덤 몬스터 프리팹 선택
        int randomIndex = Random.Range(0, monsterPrefabs.Length);
        GameObject selectedPrefab = monsterPrefabs[randomIndex];
        if (selectedPrefab == null) return;

        // 3. 인스턴스화
        GameObject newMonster = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
        newMonster.name = $"{selectedPrefab.name}_{aliveMonsters.Count + 1}";

        // 플레이어 추적 타겟 전달
        Opossum opossumComp = newMonster.GetComponent<Opossum>();
        if (opossumComp != null)
        {
            opossumComp.playerTarget = playerTransform;
        }

        Eagle eagleComp = newMonster.GetComponent<Eagle>();
        if (eagleComp != null)
        {
            eagleComp.objTarget = playerTransform.gameObject;
            eagleComp.SetAIState(Eagle.E_AI_STATE.TRACKING);
        }

        aliveMonsters.Add(newMonster);
    }

    /// <summary>
    /// 이미 사망했거나 플레이어로부터 너무 멀어진(despawnRadius 초과) 몬스터들을 정리합니다.
    /// </summary>
    private void CleanUpAndDespawnFarMonsters()
    {
        for (int i = aliveMonsters.Count - 1; i >= 0; i--)
        {
            GameObject monster = aliveMonsters[i];

            // 1. 이미 파괴된 몬스터 제거
            if (monster == null)
            {
                aliveMonsters.RemoveAt(i);
                continue;
            }

            // 2. 플레이어와 너무 멀어진 몬스터 디스폰(삭제하여 메모리/오브젝트 관리)
            float dist = Vector3.Distance(monster.transform.position, playerTransform.position);
            if (dist > despawnRadius)
            {
                aliveMonsters.RemoveAt(i);
                Destroy(monster);
            }
        }
    }

    /// <summary>
    /// 에디터 씬 뷰에서 스폰 영역 시각화 (녹색/노란색 원)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Vector3 center = playerTransform != null ? playerTransform.position : transform.position;

        // 최소 스폰 반경 (시야 밖 시작선)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, minSpawnRadius);

        // 최대 스폰 반경
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, maxSpawnRadius);

        // 디스폰 한계선
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, despawnRadius);
    }
}
