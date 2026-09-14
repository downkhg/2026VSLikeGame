using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIStatusBar : MonoBehaviour
{
    public Text textLabel;
    public RectTransform rectBarBG;
    public RectTransform rectBar;

    public void SetBarSize(float cur, float max)
    {
        float rat = cur / max;
        Vector2 vSize = rectBarBG.sizeDelta;
        vSize.x = vSize.x * rat;
        rectBar.sizeDelta = vSize;
    }
}
