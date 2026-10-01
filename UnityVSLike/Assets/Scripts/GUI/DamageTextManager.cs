using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 피격 시 피해량(대미지)을 화면에 플로팅 텍스트로 시각화해주는 GUI 매니저
/// </summary>
public class DamageTextManager : MonoBehaviour
{
    private static DamageTextManager _instance;
    public static DamageTextManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DamageTextManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("@DamageTextManager");
                    _instance = go.AddComponent<DamageTextManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private class DamageItem
    {
        public Vector3 worldPosition;
        public string text;
        public Color color;
        public float timer;
        public float duration;
        public Vector2 offset;
    }

    private List<DamageItem> activeItems = new List<DamageItem>();
    private GUIStyle damageStyle;
    private bool styleInitialized = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 대미지 수치를 해당 월드 좌표에 플로팅 텍스트로 표시합니다.
    /// </summary>
    public static void ShowDamage(Vector3 worldPos, int damage, Color color, string customText = null)
    {
        Instance.AddDamageText(worldPos, customText ?? $"-{damage}", color);
    }

    /// <summary>
    /// 일반 대미지 플로팅 텍스트 표시
    /// </summary>
    public static void ShowDamage(Vector3 worldPos, int damage, bool isPlayerHit = false)
    {
        Color col = isPlayerHit ? new Color(1f, 0.2f, 0.2f) : new Color(1f, 0.85f, 0.2f);
        Instance.AddDamageText(worldPos, $"-{damage}", col);
    }

    public void AddDamageText(Vector3 worldPos, string text, Color color, float duration = 0.85f)
    {
        // 겹치지 않도록 약간의 랜덤 x 오프셋 부여
        float randomX = Random.Range(-0.25f, 0.25f);
        Vector3 spawnPos = worldPos + new Vector3(randomX, 0.6f, 0f);

        activeItems.Add(new DamageItem()
        {
            worldPosition = spawnPos,
            text = text,
            color = color,
            timer = 0f,
            duration = duration,
            offset = Vector2.zero
        });
    }

    private void Update()
    {
        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            DamageItem item = activeItems[i];
            item.timer += Time.deltaTime;
            // 위로 서서히 떠오르는 효과
            item.offset.y += 45f * Time.deltaTime;

            if (item.timer >= item.duration)
            {
                activeItems.RemoveAt(i);
            }
        }
    }

    private void OnGUI()
    {
        if (!LegacyGUI.IsEnabled) return;
        if (activeItems.Count == 0) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        InitStyles();

        for (int i = 0; i < activeItems.Count; i++)
        {
            DamageItem item = activeItems[i];
            Vector3 screenPos = cam.WorldToScreenPoint(item.worldPosition);

            // 카메라 화면 전면에 있을 때만 렌더링
            if (screenPos.z > 0)
            {
                float progress = item.timer / item.duration;
                float alpha = Mathf.Clamp01(1f - progress);

                // 화면 좌표계 (Y축 반전)
                float x = screenPos.x - 50f;
                float y = Screen.height - screenPos.y - item.offset.y;

                Color textColor = item.color;
                textColor.a = alpha;
                damageStyle.normal.textColor = textColor;

                // 텍스트 외곽 그림자 효과
                Color shadowColor = Color.black;
                shadowColor.a = alpha * 0.8f;
                GUIStyle shadowStyle = new GUIStyle(damageStyle);
                shadowStyle.normal.textColor = shadowColor;

                GUI.Label(new Rect(x + 1, y + 1, 100, 30), item.text, shadowStyle);
                GUI.Label(new Rect(x, y, 100, 30), item.text, damageStyle);
            }
        }
    }

    private void InitStyles()
    {
        if (styleInitialized) return;

        damageStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        styleInitialized = true;
    }
}
