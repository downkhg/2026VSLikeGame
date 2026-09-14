using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CardUI : MonoBehaviour
{
    [Header("UI 컴포넌트 연결")]
    [SerializeField] private TextMeshProUGUI textCardName;
    [SerializeField] private TextMeshProUGUI textManaCost;
    [SerializeField] private TextMeshProUGUI textDescription;
    [SerializeField] private TextMeshProUGUI textDuration;
    [SerializeField] private Image imageIcon;
    [SerializeField] private Button buttonUse;

    private int myHandIndex;
    private Action<int> onCardUsedCallback;

    /// <summary>
    /// 카드 UI 데이터 바인딩
    /// </summary>
    public void Setup(CardData data, int handIndex, Action<int> onCardUsed)
    {
        myHandIndex = handIndex;
        onCardUsedCallback = onCardUsed;

        if (textCardName != null) textCardName.text = data.cardName;
        if (textManaCost != null) textManaCost.text = data.manaCost.ToString();
        if (textDescription != null) textDescription.text = data.description;
        if (imageIcon != null && data.cardIcon != null) imageIcon.sprite = data.cardIcon;

        if (textDuration != null)
        {
            if (data.cardType == CardType.DurationWeapon)
                textDuration.text = $"{data.duration}초 지속";
            else
                textDuration.text = "즉시 발동";
        }

        // 버튼 클릭 이벤트 리스너 재설정
        if (buttonUse != null)
        {
            buttonUse.onClick.RemoveAllListeners();
            buttonUse.onClick.AddListener(OnClickCard);
        }
    }

    private void OnClickCard()
    {
        onCardUsedCallback?.Invoke(myHandIndex);
    }
}