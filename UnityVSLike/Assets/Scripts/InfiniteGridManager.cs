using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 여러 가지 타일 패턴 프리팹을 배열에 등록하고,
/// 3x3 무한 격자 맵에 무작위(랜덤)로 배치 및 재배치 교체를 관리하는 매니저 컴포넌트입니다.
/// </summary>
public class InfiniteGridManager : MonoBehaviour
{
    [Header("=== 무한 맵 생성 설정 ===")]
    [Tooltip("체크 시 패턴 프리팹들로 3x3(9개) 무한 격자를 자동 생성합니다.")]
    public bool autoGenerate3x3 = true;

    [Header("=== 패턴 프리팹 배열 (인스펙터 등록) ===")]
    [Tooltip("무한 맵에 랜덤으로 배치할 타일 패턴 프리팹 목록입니다. 인스펙터에서 여러 개를 등록할 수 있습니다.")]
    public GameObject[] patternPrefabs;

    [Header("=== 시작 시 중앙 타일 고정 옵션 ===")]
    [Tooltip("체크 시 플레이어가 서 있는 시작 중앙 타일은 첫 번째 패턴(기본)으로 고정 생성합니다.")]
    public bool keepCenterDefault = false;

    [Header("=== 재배치 시 새로운 패턴 교체 설정 ===")]
    [Tooltip("체크 시 화면 밖으로 밀려난 타일이 재배치될 때도 패턴 배열에서 새로운 랜덤 패턴으로 교체됩니다.")]
    public bool changePatternOnReposition = true;

    [Header("=== 단일 프리팹 (하위 호환용 fallback) ===")]
    public GameObject gridPrefab;

    [Header("=== 추적 대상 플레이어 ===")]
    public Transform playerTransform;

    [Header("=== 타일 크기 설정 (Grid00 기준: 가로 2.08 x 세로 2.08) ===")]
    [Tooltip("타일 1개의 가로 크기입니다. Grid00은 13칸 * 0.16 = 2.08 입니다.")]
    public float tileWidth = 2.08f;
    [Tooltip("타일 1개의 세로 크기입니다. Grid00은 13칸 * 0.16 = 2.08 입니다.")]
    public float tileHeight = 2.08f;

    [Header("=== 타일 그래픽 중심 오프셋 (Pivot 보정) ===")]
    [Tooltip("타일맵이 오브젝트 원점(0,0)에서 어느 위치에 그려져 있는지 나타내는 중심 오프셋입니다.")]
    public Vector3 localCenterOffset = new Vector3(1.04f, 1.04f, 0f);

    [Header("=== 타일 스케일(배율) 설정 (기본: 1.0) ===")]
    [Tooltip("타일의 전체 크기(배율)를 키우거나 줄이고 싶을 때 사용합니다. 1.0이면 원본 크기입니다.")]
    public float tileScale = 1.0f;

    [Header("=== 격자 크기 (기본 가로 3 x 세로 3 = 9개) ===")]
    public int gridSizeX = 3;
    public int gridSizeY = 3;

    [Header("=== 관리 중인 타일 목록 ===")]
    public List<GameObject> activeGrids = new List<GameObject>();

    [Header("=== 디버그 로그 설정 ===")]
    public bool showDebugLog = true;

    private Vector3 lastLoggedPlayerPosition = Vector3.zero;

    private void Awake()
    {
        // 1. 인스펙터에 패턴 배열이 등록되어 있지 않다면 Resources/Prefabs/Tiles 에서 모든 프리팹 자동 로드
        if (patternPrefabs == null || patternPrefabs.Length == 0)
        {
            patternPrefabs = Resources.LoadAll<GameObject>("Prefabs/Tiles");
            if (patternPrefabs != null && patternPrefabs.Length > 0 && showDebugLog)
            {
                Debug.Log(string.Format("[InfiniteGridManager] Resources/Prefabs/Tiles에서 {0}개의 패턴 프리팹을 자동 등록했습니다.", patternPrefabs.Length));
            }
        }

        // 2. 단일 프리팹 fallback
        if ((patternPrefabs == null || patternPrefabs.Length == 0) && gridPrefab != null)
        {
            patternPrefabs = new GameObject[] { gridPrefab };
        }

        // 3. 씬 내 Grid00 오브젝트 fallback
        if (patternPrefabs == null || patternPrefabs.Length == 0)
        {
            GameObject sceneGrid = GameObject.Find("Grid00");
            if (sceneGrid != null)
            {
                patternPrefabs = new GameObject[] { sceneGrid };
            }
        }

        // 4. 대표 프리팹으로 타일 규격 자동 측정
        MeasureActualTileSize();
    }

    private void Start()
    {
        FindPlayer();
        InitializeInfiniteGrids();
    }

    private void Update()
    {
        // 1. 시작 시 플레이어가 아직 스폰되지 않았을 경우 지속적으로 탐색하여 타일들에게 전달
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform != null)
            {
                for (int i = 0; i < activeGrids.Count; i++)
                {
                    if (activeGrids[i] != null)
                    {
                        RepositionGrid repositionComp = activeGrids[i].GetComponent<RepositionGrid>();
                        if (repositionComp != null)
                        {
                            repositionComp.playerTransform = playerTransform;
                        }
                    }
                }
                lastLoggedPlayerPosition = playerTransform.position;
            }
        }
        // 2. 플레이어 이동 상태 모니터링 로그
        else if (showDebugLog)
        {
            float movedDistance = Vector3.Distance(playerTransform.position, lastLoggedPlayerPosition);
            if (movedDistance >= 1.0f)
            {
                lastLoggedPlayerPosition = playerTransform.position;
                Debug.Log(string.Format("[InfiniteGridManager] 🏃 플레이어 이동 감지! 현재 좌표: ({0:F2}, {1:F2}) -> 무한 맵 정상 추적 중",
                    playerTransform.position.x, playerTransform.position.y));
            }
        }
    }

    /// <summary>
    /// 등록된 패턴 프리팹 배열에서 무작위(랜덤)로 1개의 프리팹을 선택하여 반환합니다.
    /// </summary>
    public GameObject GetRandomPatternPrefab()
    {
        if (patternPrefabs == null || patternPrefabs.Length == 0)
        {
            return gridPrefab;
        }

        int randomIndex = Random.Range(0, patternPrefabs.Length);
        return patternPrefabs[randomIndex];
    }

    /// <summary>
    /// 씬에서 플레이어를 탐색합니다.
    /// </summary>
    public void FindPlayer()
    {
        if (playerTransform != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            lastLoggedPlayerPosition = playerTransform.position;
            if (showDebugLog)
            {
                Debug.Log(string.Format("[InfiniteGridManager] 플레이어 태그 탐색 성공: {0} (좌표: {1})", playerTransform.name, playerTransform.position));
            }
            return;
        }

        Player playerComponent = FindFirstObjectByType<Player>();
        if (playerComponent != null)
        {
            playerTransform = playerComponent.transform;
            lastLoggedPlayerPosition = playerTransform.position;
            if (showDebugLog)
            {
                Debug.Log(string.Format("[InfiniteGridManager] 플레이어 컴포넌트 탐색 성공: {0} (좌표: {1})", playerTransform.name, playerTransform.position));
            }
        }
    }

    /// <summary>
    /// 샘플 타일을 임시 생성하여 실제 그려진 타일 크기(cellBounds)와 중심 오프셋을 100% 정밀 측정합니다.
    /// </summary>
    private void MeasureActualTileSize()
    {
        GameObject samplePrefab = null;
        if (patternPrefabs != null && patternPrefabs.Length > 0)
        {
            samplePrefab = patternPrefabs[0];
        }
        else if (gridPrefab != null)
        {
            samplePrefab = gridPrefab;
        }

        if (samplePrefab == null)
        {
            return;
        }

        GameObject tempObj = Instantiate(samplePrefab, new Vector3(9999f, 9999f, 0f), Quaternion.identity);
        Tilemap tilemap = tempObj.GetComponentInChildren<Tilemap>();
        if (tilemap != null)
        {
            tilemap.CompressBounds();
            BoundsInt bounds = tilemap.cellBounds;
            Vector3 cellSize = tilemap.cellSize;

            if (bounds.size.x > 0 && bounds.size.y > 0)
            {
                tileWidth = bounds.size.x * cellSize.x;
                tileHeight = bounds.size.y * cellSize.y;

                float centerX = (bounds.xMin + bounds.xMax) * 0.5f * cellSize.x;
                float centerY = (bounds.yMin + bounds.yMax) * 0.5f * cellSize.y;
                localCenterOffset = new Vector3(centerX, centerY, 0f);
            }
        }
        Destroy(tempObj);

        // 안전 기본값 (Grid00 프리팹: 13칸 * 0.16 = 2.08)
        if (tileWidth <= 0f)
        {
            tileWidth = 2.08f;
        }
        if (tileHeight <= 0f)
        {
            tileHeight = 2.08f;
        }

        if (showDebugLog)
        {
            Debug.Log(string.Format("[InfiniteGridManager] 📐 타일 규격 자동 측정 완료 -> 가로: {0:F2}, 세로: {1:F2}, 중심오프셋: {2} (등록된 패턴 수: {3}개)",
                tileWidth, tileHeight, localCenterOffset, patternPrefabs != null ? patternPrefabs.Length : 0));
        }
    }

    /// <summary>
    /// 플레이어 시작 위치를 중심으로 3x3 격자 타일을 패턴 배열에서 무작위 선택하여 틈새 없이 생성합니다.
    /// </summary>
    public void InitializeInfiniteGrids()
    {
        if (patternPrefabs == null || patternPrefabs.Length == 0)
        {
            Debug.LogError("[InfiniteGridManager] ❌ 등록된 타일 패턴 프리팹이 없어 무한 맵을 생성할 수 없습니다.");
            return;
        }

        if (!autoGenerate3x3)
        {
            if (showDebugLog)
            {
                Debug.Log("[InfiniteGridManager] autoGenerate3x3 설정이 꺼져 있어 타일 자동 생성을 건너뜁니다.");
            }
            return;
        }

        // 기존에 이미 생성된 자식 타일들이 있다면 정리
        for (int i = 0; i < activeGrids.Count; i++)
        {
            if (activeGrids[i] != null)
            {
                Destroy(activeGrids[i]);
            }
        }
        activeGrids.Clear();

        // 스케일 안전 보정
        if (tileScale <= 0f)
        {
            tileScale = 1.0f;
        }

        float effectiveWidth = tileWidth * tileScale;
        float effectiveHeight = tileHeight * tileScale;
        Vector3 effectiveCenterOffset = localCenterOffset * tileScale;

        // 기준 시작 좌표 (플레이어 위치 또는 원점)
        Vector3 centerPosition = Vector3.zero;
        if (playerTransform != null)
        {
            centerPosition = playerTransform.position;
        }

        int halfX = gridSizeX / 2;
        int halfY = gridSizeY / 2;

        if (showDebugLog)
        {
            Debug.Log(string.Format("[InfiniteGridManager] 🛠️ {0}x{1} 무한 격자 타일 랜덤 패턴 생성 시작... (패턴 종류: {2}개, 중심 좌표: {3})",
                gridSizeX, gridSizeY, patternPrefabs.Length, centerPosition));
        }

        // 3x3 (총 9개) 격자 생성: x(-1 ~ 1), y(-1 ~ 1)
        for (int y = -halfY; y <= halfY; y++)
        {
            for (int x = -halfX; x <= halfX; x++)
            {
                // 중앙 타일이고 keepCenterDefault 옵션이 켜져 있으면 첫 번째 패턴, 그 외에는 랜덤 선택
                GameObject selectedPrefab = null;
                if (x == 0 && y == 0 && keepCenterDefault)
                {
                    selectedPrefab = patternPrefabs[0];
                }
                else
                {
                    selectedPrefab = GetRandomPatternPrefab();
                }

                if (selectedPrefab == null)
                {
                    continue;
                }

                // 타일의 실제 그래픽 중심이 정확히 (centerPosition + (x * width, y * height))에 오도록 스폰 좌표 계산
                float targetCenterX = centerPosition.x + ((float)x * effectiveWidth);
                float targetCenterY = centerPosition.y + ((float)y * effectiveHeight);
                Vector3 spawnPosition = new Vector3(targetCenterX - effectiveCenterOffset.x, targetCenterY - effectiveCenterOffset.y, 0f);

                GameObject newGrid = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity, this.transform);
                newGrid.name = string.Format("Grid_{0}_{1}_{2}", x + halfX, y + halfY, selectedPrefab.name);
                newGrid.transform.localScale = new Vector3(tileScale, tileScale, 1f);

                // RepositionGrid 컴포넌트 부착 및 설정
                RepositionGrid repositionComp = newGrid.GetComponent<RepositionGrid>();
                if (repositionComp == null)
                {
                    repositionComp = newGrid.AddComponent<RepositionGrid>();
                }

                repositionComp.infiniteGridManager = this;
                repositionComp.playerTransform = playerTransform;
                repositionComp.tileWidth = effectiveWidth;
                repositionComp.tileHeight = effectiveHeight;
                repositionComp.localCenterOffset = effectiveCenterOffset;
                repositionComp.gridCountX = gridSizeX;
                repositionComp.gridCountY = gridSizeY;
                repositionComp.changePatternOnReposition = changePatternOnReposition;
                repositionComp.showDebugLog = showDebugLog;

                activeGrids.Add(newGrid);

                if (showDebugLog)
                {
                    Debug.Log(string.Format("[InfiniteGridManager]   -> 타일 생성: '{0}' (선택패턴: {1}, 중심좌표: X={2:F2}, Y={3:F2})",
                        newGrid.name, selectedPrefab.name, targetCenterX, targetCenterY));
                }
            }
        }

        Debug.Log(string.Format("[InfiniteGridManager] ✅ 무한 맵 3x3 랜덤 생성 완료! (총 {0}개 타일, 패턴 풀: {1}개)",
            activeGrids.Count, patternPrefabs.Length));
    }

    /// <summary>
    /// 화면 밖으로 나간 타일을 제거하고, 이동된 새 위치에 패턴 배열에서 무작위로 선택한 새 패턴을 생성하여 교체합니다.
    /// </summary>
    public void ReplaceTile(GameObject oldTile, Vector3 newPosition)
    {
        if (oldTile != null)
        {
            activeGrids.Remove(oldTile);
            Destroy(oldTile);
        }

        GameObject randomPrefab = GetRandomPatternPrefab();
        if (randomPrefab == null)
        {
            return;
        }

        float effectiveWidth = tileWidth * tileScale;
        float effectiveHeight = tileHeight * tileScale;
        Vector3 effectiveCenterOffset = localCenterOffset * tileScale;

        GameObject newGrid = Instantiate(randomPrefab, newPosition, Quaternion.identity, this.transform);
        newGrid.name = string.Format("Grid_{0}", randomPrefab.name);
        newGrid.transform.localScale = new Vector3(tileScale, tileScale, 1f);

        RepositionGrid repositionComp = newGrid.GetComponent<RepositionGrid>();
        if (repositionComp == null)
        {
            repositionComp = newGrid.AddComponent<RepositionGrid>();
        }

        repositionComp.infiniteGridManager = this;
        repositionComp.playerTransform = playerTransform;
        repositionComp.tileWidth = effectiveWidth;
        repositionComp.tileHeight = effectiveHeight;
        repositionComp.localCenterOffset = effectiveCenterOffset;
        repositionComp.gridCountX = gridSizeX;
        repositionComp.gridCountY = gridSizeY;
        repositionComp.changePatternOnReposition = changePatternOnReposition;
        repositionComp.showDebugLog = showDebugLog;

        activeGrids.Add(newGrid);

        if (showDebugLog)
        {
            Debug.Log(string.Format("[InfiniteGridManager] 🎲 타일 재배치 & 새 패턴 교체 완료: '{0}' (위치: {1})",
                randomPrefab.name, newPosition));
        }
    }

    /// <summary>
    /// 3x3 전체 무한 맵 영역과 플레이어 추적 중심을 씬 뷰에 시각화합니다.
    /// </summary>
    private void OnDrawGizmos()
    {
        Vector3 center = playerTransform != null ? playerTransform.position : transform.position;

        float effectiveWidth = tileWidth * tileScale;
        float effectiveHeight = tileHeight * tileScale;

        // 3x3 전체 맵 범위 (보라색 와이어 박스)
        Gizmos.color = new Color(0.8f, 0.2f, 1f, 0.6f);
        Gizmos.DrawWireCube(center, new Vector3(effectiveWidth * (float)gridSizeX, effectiveHeight * (float)gridSizeY, 0f));

        // 플레이어 중심점 (빨간색 구체)
        if (playerTransform != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(playerTransform.position, 0.2f);
        }
    }
}
