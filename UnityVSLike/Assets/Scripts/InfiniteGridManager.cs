using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Grid00 프리팹을 활용하여 플레이어 주변에 3x3 격자 타일을 생성하고,
/// 무한 반복 맵을 빈틈없이 완벽하게 관리하는 매니저 컴포넌트입니다.
/// </summary>
public class InfiniteGridManager : MonoBehaviour
{
    [Header("=== 무한 맵 생성 설정 ===")]
    [Tooltip("체크 시 Grid00 프리팹으로 3x3(9개) 무한 격자를 자동 생성합니다.")]
    public bool autoGenerate3x3 = true;

    [Header("=== 타일 프리팹 (비어있으면 Resources/Prefabs/Tiles/Grid00 자동 로드) ===")]
    public GameObject gridPrefab;

    [Header("=== 추적 대상 플레이어 ===")]
    public Transform playerTransform;

    [Header("=== 타일 크기 설정 (Grid00 실제 크기: 가로 2.08 x 세로 2.08) ===")]
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
        // 1. 프리팹이 비어있으면 Resources에서 Grid00 로드
        if (gridPrefab == null)
        {
            gridPrefab = Resources.Load<GameObject>("Prefabs/Tiles/Grid00");
            if (gridPrefab != null && showDebugLog)
            {
                Debug.Log("[InfiniteGridManager] Resources/Prefabs/Tiles/Grid00 프리팹 로드 완료.");
            }
        }

        // 2. 씬에 이미 Grid00이 있다면 프리팹 fallback으로 참조
        if (gridPrefab == null)
        {
            GameObject sceneGrid = GameObject.Find("Grid00");
            if (sceneGrid != null)
            {
                gridPrefab = sceneGrid;
                if (showDebugLog)
                {
                    Debug.Log("[InfiniteGridManager] 씬 내 Grid00 오브젝트를 템플릿으로 참조 완료.");
                }
            }
        }

        // 3. 실제 그려진 타일 크기 자동 측정
        if (gridPrefab != null)
        {
            MeasureActualTileSize();
        }
        else
        {
            Debug.LogWarning("[InfiniteGridManager] ⚠️ Grid00 프리팹 또는 오브젝트를 찾을 수 없습니다.");
        }
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
        // 2. 플레이어 이동 상태 모니터링 로그 (1유닛 이상 이동할 때마다 출력)
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
    /// 임시 타일을 생성하여 Tilemap.CompressBounds()를 실행하고, 실제 그려진 정확한 타일 크기와 중심 오프셋을 측정합니다.
    /// </summary>
    private void MeasureActualTileSize()
    {
        if (gridPrefab == null) return;

        // 임시 오브젝트를 인스턴스화하여 정확한 타일 영역 측정
        GameObject tempObj = Instantiate(gridPrefab, new Vector3(9999f, 9999f, 0f), Quaternion.identity);
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
            Debug.Log(string.Format("[InfiniteGridManager] 📐 타일 규격 자동 측정 완료 -> 가로: {0:F2}, 세로: {1:F2}, 중심오프셋: {2}",
                tileWidth, tileHeight, localCenterOffset));
        }
    }

    /// <summary>
    /// 플레이어 시작 위치를 중심으로 3x3 격자 타일을 틈새 없이 완벽하게 밀착 생성하고 재배치 컴포넌트를 연결합니다.
    /// </summary>
    public void InitializeInfiniteGrids()
    {
        if (gridPrefab == null)
        {
            Debug.LogError("[InfiniteGridManager] ❌ Grid00 프리팹이 없어 무한 맵을 생성할 수 없습니다.");
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
            Debug.Log(string.Format("[InfiniteGridManager] 🛠️ {0}x{1} 무한 격자 타일 자동 밀착 생성 시작... (중심 좌표: {2}, 스케일: {3})",
                gridSizeX, gridSizeY, centerPosition, tileScale));
        }

        // 3x3 (총 9개) 격자 생성: x(-1 ~ 1), y(-1 ~ 1)
        for (int y = -halfY; y <= halfY; y++)
        {
            for (int x = -halfX; x <= halfX; x++)
            {
                // 타일의 실제 그래픽 중심이 정확히 (centerPosition + (x * width, y * height))에 오도록 스폰 좌표 계산
                float targetCenterX = centerPosition.x + ((float)x * effectiveWidth);
                float targetCenterY = centerPosition.y + ((float)y * effectiveHeight);
                Vector3 spawnPosition = new Vector3(targetCenterX - effectiveCenterOffset.x, targetCenterY - effectiveCenterOffset.y, 0f);

                GameObject newGrid = Instantiate(gridPrefab, spawnPosition, Quaternion.identity, this.transform);
                newGrid.name = string.Format("Grid_{0}_{1}", x + halfX, y + halfY);
                newGrid.transform.localScale = new Vector3(tileScale, tileScale, 1f);

                // RepositionGrid 컴포넌트 부착 및 설정
                RepositionGrid repositionComp = newGrid.GetComponent<RepositionGrid>();
                if (repositionComp == null)
                {
                    repositionComp = newGrid.AddComponent<RepositionGrid>();
                }

                repositionComp.playerTransform = playerTransform;
                repositionComp.tileWidth = effectiveWidth;
                repositionComp.tileHeight = effectiveHeight;
                repositionComp.localCenterOffset = effectiveCenterOffset;
                repositionComp.gridCountX = gridSizeX;
                repositionComp.gridCountY = gridSizeY;
                repositionComp.showDebugLog = showDebugLog;

                activeGrids.Add(newGrid);

                if (showDebugLog)
                {
                    Debug.Log(string.Format("[InfiniteGridManager]   -> 타일 밀착 생성: '{0}' (중심: X={1:F2}, Y={2:F2}, 스폰위치: {3})",
                        newGrid.name, targetCenterX, targetCenterY, spawnPosition));
                }
            }
        }

        Debug.Log(string.Format("[InfiniteGridManager] ✅ Grid00 기반 3x3 무한 맵 생성 완료! (총 {0}개 타일, 틈새 없는 규격: {1:F2} x {2:F2})",
            activeGrids.Count, effectiveWidth, effectiveHeight));
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
