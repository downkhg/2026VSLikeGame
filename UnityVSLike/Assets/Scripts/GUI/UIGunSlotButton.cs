using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 총기 목록에서 각 총기 슬롯 버튼을 관리하며, 마우스 롤오버(호버) 및 클릭 이벤트를 처리하는 컴포넌트
/// </summary>
public class UIGunSlotButton : MonoBehaviour, IPointerEnterHandler
{
    public Text textGunName;
    public Text textGunLevel;
    public Image imageIcon;
    public Button button;

    public GunType gunType;
    private Action<GunType> onSelectCallback;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.AddListener(OnClickButton);
        }
    }

    public void Setup(GunType type, string name, int level, bool isEquipped, Action<GunType> onSelected)
    {
        this.gunType = type;
        this.onSelectCallback = onSelected;

        if (textGunName != null)
        {
            textGunName.text = name;
        }

        if (textGunLevel != null)
        {
            if (isEquipped)
            {
                textGunLevel.text = string.Format("장착중 Lv.{0}", level);
                textGunLevel.color = new Color(0.2f, 1f, 0.4f);
            }
            else
            {
                textGunLevel.text = "미장착";
                textGunLevel.color = new Color(0.7f, 0.7f, 0.7f);
            }
        }
    }

    private void OnClickButton()
    {
        if (onSelectCallback != null)
        {
            onSelectCallback.Invoke(gunType);
        }
    }

    // 마우스 롤오버(호버) 진입 시 선택 콜백 호출
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onSelectCallback != null)
        {
            onSelectCallback.Invoke(gunType);
        }
    }
}
