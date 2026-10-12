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

    [Header("=== 타일 크기 설정 (기본: 가로 3.04 x 세로 2.88) ===")]
    public float tileWidth = 3.04f;
    public float tileHeight = 2.88f;

    [Header("=== 격자(Grid) 개수 설정 (기본 3x3) ===")]
    public int gridCountX = 3;
    public int gridCountY = 3;

    [Header("=== 디버그 로그 설정 ===")]
    public bool showDebugLog = true;

    [Header("=== 디버그 드로우(기즈모) 시각화 설정 ===")]
    [Tooltip("체크 시 씬 뷰에 타일 테두리, 재배치 한계선, 플레이어 연결선이 시각적으로 표시됩니다.")]
    public bool showGizmos = true;

    [Tooltip("타일의 기본 크기 사각형(녹색)을 그립니다.")]
    public bool drawTileBounds = true;

    [Tooltip("재배치가 발동하는 임계치 한계선 사각형(노란색/주황색)을 그립니다.")]
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
    /// Grid 컴포넌트의 셀 크기(0.16)와 Tilemap 타일 개수를 기반으로 정확한 월드 크기를 계산합니다.
    /// </summary>
    public void CalculateTileSize()
    {
        Grid grid = GetComponent<Grid>();
        if (grid == null)
        {
            grid = GetComponentInParent<Grid>();
        }

        Vector3 cellSize = new Vector3(0.16f, 0.16f, 0f);
        if (grid != null)
        {
            cellSize = grid.cellSize;
        }

        Tilemap tilemap = GetComponentInChildren<Tilemap>();
        if (tilemap != null)
        {
            tilemap.CompressBounds();
            if (tilemap.size.x > 0 && tilemap.size.y > 0)
            {
                tileWidth = tilemap.size.x * cellSize.x;
                tileHeight = tilemap.size.y * cellSize.y;
            }
        }

        // 안전 기본값 및 과대 보정 (Grid00: 19칸 * 0.16 = 3.04, 18칸 * 0.16 = 2.88)
        if (tileWidth <= 0f || tileWidth > 10f)
        {
            tileWidth = 3.04f;
        }
        if (tileHeight <= 0f || tileHeight > 10f)
        {
            tileHeight = 2.88f;
        }

        if (showDebugLog)
        {
            Debug.Log(string.Format("[RepositionGrid] '{0}' 타일 크기 산출 완료 -> 가로: {1:F2}, 세로: {2:F2} (재배치 기준 거리: 가로 {3:F2}, 세로 {4:F2})",
                gameObject.name, tileWidth, tileHeight, tileWidth * 1.5f, tileHeight * 1.5f));
        }
    }

    /// <summary>
    /// 플레이어와 현재 타일 사이의 거리를 계산하고, 기준 거리를 벗어났으면 플레이어 진행 방향 앞쪽으로 재배치합니다.
    /// </summary>
    private void CheckAndReposition()
    {
        // 1. 플레이어와 타일 사이의 거리 차이 계산
        float diffX = playerTransform.position.x - transform.position.x;
        float diffY = playerTransform.position.y - transform.position.y;

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

        // 전체 격자 너비(타일너비 * gridCountX)만큼 이동
        float moveDistanceX = directionX * tileWidth * (float)gridCountX;
        Vector3 previousPos = transform.position;
        Vector3 newPosition = transform.position;
        newPosition.x = newPosition.x + moveDistanceX;
        transform.position = newPosition;

        // 순간이동 궤적을 씬 뷰에 자홍색 선으로 2초간 시각화
        Debug.DrawLine(previousPos, newPosition, Color.magenta, 2.0f);

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

        // 전체 격자 높이(타일높이 * gridCountY)만큼 이동
        float moveDistanceY = directionY * tileHeight * (float)gridCountY;
        Vector3 previousPos = transform.position;
        Vector3 newPosition = transform.position;
        newPosition.y = newPosition.y + moveDistanceY;
        transform.position = newPosition;

        // 순간이동 궤적을 씬 뷰에 자홍색 선으로 2초간 시각화
        Debug.DrawLine(previousPos, newPosition, Color.magenta, 2.0f);

        if (showDebugLog)
        {
            Debug.Log(string.Format("[RepositionGrid] 🔄 '{0}' 세로(Y) 재배치! 거리차: {1:F2} -> 방향: {2}, 위치: {3:F2} => {4:F2}",
                gameObject.name, diffY, dirText, previousPos.y, newPosition.y));
        }
    }

    /// <summary>
    /// 유니티 에디터 씬(Scene) 뷰에 재배치 판정 기준과 거리 상태를 시각적으로 그립니다.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGizmos)
        {
            return;
        }

        Vector3 centerPos = transform.position;

        // 1. 타일 중심점 표시 (작은 흰색 구체)
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(centerPos, 0.12f);

        // 2. 타일 실제 크기 영역 표시 (초록색 와이어 박스)
        if (drawTileBounds)
        {
            Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.7f); // 반투명 녹색
            Gizmos.DrawWireCube(centerPos, new Vector3(tileWidth, tileHeight, 0f));
        }

        // 3. 재배치 발동 한계선(임계 영역) 표시 (노란색 와이어 박스)
        float thresholdX = tileWidth * 1.5f;
        float thresholdY = tileHeight * 1.5f;

        if (drawThresholdBox)
        {
            Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.85f); // 노란색
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

            // 거리에 따른 색상 변화:
            // 1) 1.0 초과: 빨간색 (즉시 재배치)
            // 2) 0.8 ~ 1.0: 주황색 (재배치 임박)
            // 3) 0.8 미만: 시안(하늘색) (안전 영역)
            if (maxRatio >= 1.0f)
            {
                Gizmos.color = Color.red;
            }
            else if (maxRatio >= 0.8f)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f); // 주황색
            }
            else
            {
                Gizmos.color = Color.cyan;
            }

            Gizmos.DrawLine(centerPos, playerTransform.position);
        }
    }
}
