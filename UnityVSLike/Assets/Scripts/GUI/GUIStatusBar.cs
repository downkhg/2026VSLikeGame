using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIStatusBar : MonoBehaviour
{
    public Text textLabel;
    public RectTransform rectBarBG;
    public RectTransform rectBar;

    /// <summary>
    /// 현재 수치/최대 수치에 맞춰 게이지 바 너비와 수치 텍스트를 갱신합니다.
    /// </summary>
    public void SetBarSize(float cur, float max)
    {
        if (max <= 0f) max = 1f;
        float safeCur = Mathf.Max(0f, cur);
        float rat = Mathf.Clamp01(safeCur / max);

        if (rectBarBG != null && rectBar != null)
        {
            Vector2 vSize = rectBarBG.sizeDelta;
            vSize.x = rectBarBG.sizeDelta.x * rat;
            rectBar.sizeDelta = vSize;
        }

        // GUI 상에 현재 남은 HP와 최대 HP 수치를 텍스트로 명확히 출력
        if (textLabel != null)
        {
            textLabel.text = $"HP {Mathf.RoundToInt(safeCur)}/{Mathf.RoundToInt(max)}";
        }
    }
}
