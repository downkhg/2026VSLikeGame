using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 레벨업 3택 팝업에서 개별 아이템 카드를 표시하고 선택 이벤트를 처리하는 UGUI 컴포넌트
/// </summary>
public class UILevelUpCard : MonoBehaviour
{
    public Image imageIcon;
    public Text textName;
    public Text textType;
    public Text textScore;
    public Text textDescription;
    public Button buttonSelect;

    private ItemInfo currentItem;
    private Action<ItemInfo> onSelectCallback;

    private void Awake()
    {
        if (buttonSelect == null)
            buttonSelect = GetComponent<Button>();

        if (buttonSelect != null)
        {
            buttonSelect.onClick.AddListener(OnClickCard);
        }
    }

    public void Setup(ItemInfo item, Action<ItemInfo> onSelected)
    {
        currentItem = item;
        onSelectCallback = onSelected;

        if (item == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (textName != null)
            textName.text = item.Name;

        if (textType != null)
            textType.text = $"타입: {item.GunType}";

        if (textScore != null)
            textScore.text = $"점수: +{item.Score}";

        if (textDescription != null)
            textDescription.text = item.Description;
    }

    private void OnClickCard()
    {
        if (currentItem != null)
        {
            onSelectCallback?.Invoke(currentItem);
        }
    }
}
