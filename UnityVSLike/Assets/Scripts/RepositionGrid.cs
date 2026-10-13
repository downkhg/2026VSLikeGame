using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 뱀서라이크 스타일의 무한 반복 맵을 위해,
/// 플레이어와의 거리에 따라 타일의 위치를 재배치(순간이동)하는 컴포넌트입니다.
/// </summary>
public class RepositionGrid : MonoBehaviour
{
    [Header("=== 추적 대상 플레이어 ===")]
    public Transform playerTransform;

    [Header("=== 타일 실제 크기 (Grid00 기준: 13칸 x 0.16 = 2.08) ===")]
    [Tooltip("타일의 가로 월드 크기입니다. 틈새가 생기지 않도록 실제 그려진 타일 크기와 정확히 일치해야 합니다.")]
    public float tileWidth = 2.08f;
    [Tooltip("타일의 세로 월드 크기입니다. 틈새가 생기지 않도록 실제 그려진 타일 크기와 정확히 일치해야 합니다.")]
    public float tileHeight = 2.08f;

    [Header("=== 타일 그래픽 중심 오프셋 (Pivot 보정) ===")]
    [Tooltip("타일맵이 오브젝트 원점(0,0)에서 어느 위치에 그려져 있는지 나타내는 중심 오프셋입니다.")]
    public Vector3 localCenterOffset = new Vector3(1.04f, 1.04f, 0f);

    [Header("=== 격자(Grid) 개수 설정 (기본 3x3) ===")]
    public int gridCountX = 3;
    public int gridCountY = 3;

    [Header("=== 디버그 로그 설정 ===")]
    public bool showDebugLog = true;

    [Header("=== 디버그 드로우(기즈모) 시각화 설정 ===")]
    [Tooltip("체크 시 씬 뷰에 타일 테두리, 재배치 한계선, 플레이어 연결선이 시각적으로 표시됩니다.")]
    public bool showGizmos = true;

    [Tooltip("타일의 실제 크기 사각형(녹색)을 그립니다.")]
    public bool drawTileBounds = true;

    [Tooltip("재배치가 발동하는 임계치 한계선 사각형(노란색)을 그립니다.")]
    public bool drawThresholdBox = true;

    [Tooltip("타일 중심에서 플레이어까지의 거리 선(파랑->주황->빨강)을 그립니다.")]
    public bool drawDistanceLine = true;

    private void Awake()
    {
        CalculateTileSize();
    }

    private void Start()
    {
        FindPlayer();
    }

    private void LateUpdate()
    {
        // 플레이어 참조가 없으면 재탐색
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null)
            {
                return;
            }
        }

        CheckAndReposition();
    }

    /// <summary>
    /// 씬에서 플레이어 트랜스폼을 탐색하여 할당합니다.
    /// </summary>
    public void FindPlayer()
    {
        if (playerTransform != null)
        {
            return;
        }

        // 1. 태그로 플레이어 검색
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            if (showDebugLog)
            {
                Debug.Log(string.Format("[RepositionGrid] '{0}' 추적 대상 플레이어 탐색 완료: {1}", gameObject.name, playerTransform.name));
            }
            return;
        }

        // 2. 컴포넌트로 플레이어 검색
        Player playerComponent = FindFirstObjectByType<Player>();
        if (playerComponent != null)
        {
            playerTransform = playerComponent.transform;
            if (showDebugLog)
            {
                Debug.Log(string.Format("[RepositionGrid] '{0}' 추적 대상 플레이어 탐색 완료: {1}", gameObject.name, playerTransform.name));
            }
        }
    }

    /// <summary>
    /// Tilemap에 실제로 그려진 타일 영역(cellBounds)을 계산하여 틈새 없는 정확한 타일 크기와 중심점을 산출합니다.
    /// </summary>
    public void CalculateTileSize()
    {
        Tilemap tilemap = GetComponentInChildren<Tilemap>();
        if (tilemap != null)
        {
            // 그려진 타일에 맞게 영역 압축
            tilemap.CompressBounds();
            BoundsInt bounds = tilemap.cellBounds;
            Vector3 cellSize = tilemap.cellSize;

            float currentScaleX = transform.localScale.x;
            float currentScaleY = transform.localScale.y;

            if (bounds.size.x > 0 && bounds.size.y > 0)
            {
                // 실제 그려진 타일 개수 * 셀 크기 * 오브젝트 스케일
                tileWidth = bounds.size.x * cellSize.x * currentScaleX;
                tileHeight = bounds.size.y * cellSize.y * currentScaleY;

                // 타일맵의 실제 그래픽 중심 오프셋 계산 (피벗이 모서리에 있는 문제 보정)
                float centerX = (bounds.xMin + bounds.xMax) * 0.5f * cellSize.x * currentScaleX;
                float centerY = (bounds.yMin + bounds.yMax) * 0.5f * cellSize.y * currentScaleY;
                localCenterOffset = new Vector3(centerX, centerY, 0f);
            }
        }

        // 안전 기본값 (Grid00 프리팹: 13칸 * 0.16 = 2.08)
        if (tileWidth <= 0f)
        {
            tileWidth = 2.08f * transform.localScale.x;
        }
        if (tileHeight <= 0f)
        {
            tileHeight = 2.08f * transform.localScale.y;
        }

        if (showDebugLog)
        {
            Debug.Log(string.Format("[RepositionGrid] '{0}' 타일 크기 산출 완료 -> 가로: {1:F2}, 세로: {2:F2} (중심오프셋: {3}, 재배치 기준: 가로 {4:F2}, 세로 {5:F2})",
                gameObject.name, tileWidth, tileHeight, localCenterOffset, tileWidth * 1.5f, tileHeight * 1.5f));
        }
    }

    /// <summary>
    /// 타일 그래픽의 실제 월드 중심 좌표를 반환합니다.
    /// </summary>
    public Vector3 GetTileCenter()
    {
        return transform.position + localCenterOffset;
    }

    /// <summary>
    /// 플레이어와 현재 타일의 중심 사이 거리를 계산하고, 기준 거리를 벗어났으면 플레이어 진행 방향 앞쪽으로 재배치합니다.
    /// </summary>
    private void CheckAndReposition()
    {
        Vector3 tileCenter = GetTileCenter();

        // 1. 플레이어와 타일 중심 사이의 거리 차이 계산
        float diffX = playerTransform.position.x - tileCenter.x;
        float diffY = playerTransform.position.y - tileCenter.y;

        // 2. 가로(X축) 재배치 판정: 타일 너비의 1.5배 이상 멀어졌을 때
        float thresholdX = tileWidth * 1.5f;
        if (Mathf.Abs(diffX) > thresholdX)
        {
            RepositionX(diffX);
        }

        // 3. 세로(Y축) 재배치 판정: 타일 높이의 1.5배 이상 멀어졌을 때
        float thresholdY = tileHeight * 1.5f;
        if (Mathf.Abs(diffY) > thresholdY)
        {
            RepositionY(diffY);
        }
    }

    /// <summary>
    /// X축 방향으로 타일을 재배치합니다.
    /// </summary>
    private void RepositionX(float diffX)
    {
        // 플레이어가 오른쪽에 있으면 +1, 왼쪽에 있으면 -1
        float directionX = 0f;
        string dirText = "";
        if (diffX > 0f)
        {
            directionX = 1f;
            dirText = "오른쪽(+)";
        }
        else if (diffX < 0f)
        {
            directionX = -1f;
            dirText = "왼쪽(-)";
        }

        // 전체 격자 너비(타일너비 * gridCountX)만큼 이동하여 빈틈없이 정렬
        float moveDistanceX = directionX * tileWidth * (float)gridCountX;
        Vector3 previousPos = transform.position;
        Vector3 newPosition = transform.position;
        newPosition.x = newPosition.x + moveDistanceX;
        transform.position = newPosition;

        // 순간이동 궤적을 씬 뷰에 자홍색 선으로 2초간 시각화
        Debug.DrawLine(previousPos + localCenterOffset, newPosition + localCenterOffset, Color.magenta, 2.0f);

        if (showDebugLog)
        {
            Debug.Log(string.Format("[RepositionGrid] 🔄 '{0}' 가로(X) 재배치! 거리차: {1:F2} -> 방향: {2}, 위치: {3:F2} => {4:F2}",
                gameObject.name, diffX, dirText, previousPos.x, newPosition.x));
        }
    }

    /// <summary>
    /// Y축 방향으로 타일을 재배치합니다.
    /// </summary>
    private void RepositionY(float diffY)
    {
        // 플레이어가 위쪽에 있으면 +1, 아래쪽에 있으면 -1
        float directionY = 0f;
        string dirText = "";
        if (diffY > 0f)
        {
            directionY = 1f;
            dirText = "위쪽(+)";
        }
        else if (diffY < 0f)
        {
            directionY = -1f;
            dirText = "아래쪽(-)";
        }

        // 전체 격자 높이(타일높이 * gridCountY)만큼 이동하여 빈틈없이 정렬
        float moveDistanceY = directionY * tileHeight * (float)gridCountY;
        Vector3 previousPos = transform.position;
        Vector3 newPosition = transform.position;
        newPosition.y = newPosition.y + moveDistanceY;
        transform.position = newPosition;

        // 순간이동 궤적을 씬 뷰에 자홍색 선으로 2초간 시각화
        Debug.DrawLine(previousPos + localCenterOffset, newPosition + localCenterOffset, Color.magenta, 2.0f);

        if (showDebugLog)
        {
            Debug.Log(string.Format("[RepositionGrid] 🔄 '{0}' 세로(Y) 재배치! 거리차: {1:F2} -> 방향: {2}, 위치: {3:F2} => {4:F2}",
                gameObject.name, diffY, dirText, previousPos.y, newPosition.y));
        }
    }

    /// <summary>
    /// 유니티 에디터 씬(Scene) 뷰에 실제 타일 영역과 재배치 판정 기준을 정확히 그립니다.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGizmos)
        {
            return;
        }

        // 실제 타일 그래픽의 중심 위치
        Vector3 centerPos = GetTileCenter();

        // 1. 타일 중심점 표시 (작은 흰색 구체)
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(centerPos, 0.12f);

        // 2. 타일 실제 크기 영역 표시 (초록색 와이어 박스 - 실제 스프라이트 영역과 100% 일치)
        if (drawTileBounds)
        {
            Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.8f);
            Gizmos.DrawWireCube(centerPos, new Vector3(tileWidth, tileHeight, 0f));
        }

        // 3. 재배치 발동 한계선(임계 영역) 표시 (노란색 와이어 박스)
        float thresholdX = tileWidth * 1.5f;
        float thresholdY = tileHeight * 1.5f;

        if (drawThresholdBox)
        {
            Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.85f);
            Gizmos.DrawWireCube(centerPos, new Vector3(thresholdX * 2f, thresholdY * 2f, 0f));
        }

        // 4. 플레이어와의 거리 및 위험도 연결선 표시
        if (drawDistanceLine && playerTransform != null)
        {
            float diffX = Mathf.Abs(playerTransform.position.x - centerPos.x);
            float diffY = Mathf.Abs(playerTransform.position.y - centerPos.y);

            // 위험도 비율 계산 (0.0: 중심 일치, 1.0: 임계선 도달)
            float ratioX = diffX / thresholdX;
            float ratioY = diffY / thresholdY;
            float maxRatio = Mathf.Max(ratioX, ratioY);

            // 1) 1.0 이상: 빨간색 (재배치)
            // 2) 0.8 ~ 1.0: 주황색 (재배치 임박)
            // 3) 0.8 미만: 하늘색 (안전)
            if (maxRatio >= 1.0f)
            {
                Gizmos.color = Color.red;
            }
            else if (maxRatio >= 0.8f)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f);
            }
            else
            {
                Gizmos.color = Color.cyan;
            }

            Gizmos.DrawLine(centerPos, playerTransform.position);
        }
    }
}
