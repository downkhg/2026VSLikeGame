using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 좌측에는 보유/전체 총기 목록 리스트를 표시하고,
/// 우측에는 롤오버/선택된 총기의 레벨 및 상세 능력치를 보여주는 UGUI 팝업 윈도우
/// </summary>
public class UIGunStatusPopup : MonoBehaviour
{
    [Header("상단 타이틀 및 닫기 버튼")]
    public Text textTitle;
    public Button buttonClose;

    [Header("좌측: 총기 슬롯 리스트 컨테이너")]
    public Transform listContainer;
    public GameObject slotButtonPrefab;
    public List<UIGunSlotButton> fixedSlotButtons = new List<UIGunSlotButton>();

    [Header("우측: 선택된 총기 상세 정보 텍스트")]
    public Text textDetailName;
    public Text textDetailType;
    public Text textDetailStats;
    public Text textDetailDesc;

    private GunInventory currentGunInventory;
    private List<GameObject> spawnedButtons = new List<GameObject>();

    private void Awake()
    {
        if (buttonClose != null)
        {
            buttonClose.onClick.AddListener(Close);
        }
    }

    public void Open(GunInventory gunInventory)
    {
        this.currentGunInventory = gunInventory;
        gameObject.SetActive(true);

        if (textTitle != null)
        {
            textTitle.text = "<b>[ 무기 상태 및 레벨 정보 (G) ]</b>";
        }

        // 전체 총기 목록 슬롯 세팅
        SetupGunListSlots();

        // 기본으로 첫 번째 총기(DefaultGun) 세부 정보 표시
        DisplayGunDetails(GunType.DefaultGun);
    }

    private void SetupGunListSlots()
    {
        GunType[] allTypes = new GunType[]
        {
            GunType.DefaultGun,
            GunType.KunaiGun,
            GunType.ShotGun,
            GunType.RocketLauncher,
            GunType.SoccerGun,
            GunType.BlockGun,
            GunType.LightningShield
        };

        if (fixedSlotButtons != null && fixedSlotButtons.Count > 0)
        {
            for (int i = 0; i < fixedSlotButtons.Count; i++)
            {
                if (i < allTypes.Length)
                {
                    GunType gType = allTypes[i];
                    bool isEquipped = IsGunEquipped(gType);
                    int level = GetGunLevel(gType);
                    string nameText = GetGunDisplayName(gType);

                    fixedSlotButtons[i].gameObject.SetActive(true);
                    fixedSlotButtons[i].Setup(gType, nameText, level, isEquipped, OnGunSlotHoveredOrClicked);
                }
                else
                {
                    fixedSlotButtons[i].gameObject.SetActive(false);
                }
            }
        }
        else if (slotButtonPrefab != null && listContainer != null)
        {
            ClearSpawnedButtons();

            for (int i = 0; i < allTypes.Length; i++)
            {
                GunType gType = allTypes[i];
                bool isEquipped = IsGunEquipped(gType);
                int level = GetGunLevel(gType);
                string nameText = GetGunDisplayName(gType);

                GameObject btnGo = Instantiate(slotButtonPrefab, listContainer);
                UIGunSlotButton slotComp = btnGo.GetComponent<UIGunSlotButton>();
                if (slotComp != null)
                {
                    slotComp.Setup(gType, nameText, level, isEquipped, OnGunSlotHoveredOrClicked);
                }
                spawnedButtons.Add(btnGo);
            }
        }
    }

    private void OnGunSlotHoveredOrClicked(GunType type)
    {
        DisplayGunDetails(type);
    }

    private void DisplayGunDetails(GunType type)
    {
        string displayName = GetGunDisplayName(type);
        bool isEquipped = IsGunEquipped(type);
        int level = GetGunLevel(type);

        // 1. 이름 표시
        if (textDetailName != null)
        {
            textDetailName.text = string.Format("<b><color=yellow>[ {0} ]</color></b>", displayName);
        }

        // 2. 타입 및 장착 상태 표시
        if (textDetailType != null)
        {
            string equipStatusStr = isEquipped
                ? string.Format("<color=#00FF88>● 장착중 (Lv.{0})</color>", level)
                : "<color=#AAAAAA>○ 미장착 (획득 필요)</color>";

            textDetailType.text = string.Format("무기 분류: {0}\n장착 상태: {1}", type.ToString(), equipStatusStr);
        }

        // 3. 스펙 수치 정보 (파워, 쿨타임, 사거리)
        if (textDetailStats != null)
        {
            BaseGun gunComp = null;
            if (currentGunInventory != null)
            {
                gunComp = currentGunInventory.GetGun(type);
            }

            if (gunComp != null)
            {
                string fireMode = gunComp.isAutoFire ? "자동 연사" : "수동 발사";
                textDetailStats.text = string.Format(
                    "<b>[ 전투 스탯 ]</b>\n" +
                    "• 탄환 위력: <color=cyan>{0}</color>\n" +
                    "• 발사 쿨타임: <color=cyan>{1:F2}초</color>\n" +
                    "• 탐색 사거리: <color=cyan>{2}m</color>\n" +
                    "• 발사 모드: <color=yellow>{3}</color>",
                    gunComp.shotPower,
                    gunComp.fireInterval,
                    gunComp.searchRadius,
                    fireMode);
            }
            else
            {
                textDetailStats.text =
                    "<b>[ 기본 스탯 ]</b>\n" +
                    "• 탄환 위력: 10.0\n" +
                    "• 발사 쿨타임: 0.50초\n" +
                    "• 탐색 사거리: 10.0m\n" +
                    "• 발사 모드: 자동 연사";
            }
        }

        // 4. 상세 설명문 표시
        if (textDetailDesc != null)
        {
            ItemInfo itemInfo = ItemInfoManager.Instance.GetItemInfo(type);
            if (itemInfo != null && !string.IsNullOrEmpty(itemInfo.Description))
            {
                textDetailDesc.text = string.Format("<b>[ 무기 설명 ]</b>\n{0}", itemInfo.Description);
            }
            else
            {
                textDetailDesc.text = string.Format("<b>[ 무기 설명 ]</b>\n{0} 타입의 총기입니다. 레벨업 시 더욱 강력한 탄환을 발사합니다.", displayName);
            }
        }
    }

    private bool IsGunEquipped(GunType type)
    {
        if (currentGunInventory == null) return false;
        BaseGun gun = currentGunInventory.GetGun(type);
        return gun != null;
    }

    private int GetGunLevel(GunType type)
    {
        if (currentGunInventory == null) return 0;
        int count = 0;
        for (int i = 0; i < currentGunInventory.GunComponents.Count; i++)
        {
            BaseGun gun = currentGunInventory.GunComponents[i];
            if (gun != null)
            {
                if (type == GunType.DefaultGun && gun is DefaultGun) count++;
                else if (type == GunType.KunaiGun && gun is KunaiGun) count++;
                else if (type == GunType.ShotGun && gun is ShotGun) count++;
                else if (type == GunType.BlockGun && gun is BlockGun) count++;
                else if (type == GunType.SoccerGun && gun is SoccerGun) count++;
                else if (type == GunType.LightningShield && gun is LightningShield) count++;
            }
        }
        return count > 0 ? count : (IsGunEquipped(type) ? 1 : 0);
    }

    private string GetGunDisplayName(GunType type)
    {
        switch (type)
        {
            case GunType.DefaultGun: return "기본 총 (DefaultGun)";
            case GunType.KunaiGun: return "쿠나이건 (KunaiGun)";
            case GunType.ShotGun: return "샷건 (ShotGun)";
            case GunType.RocketLauncher: return "로켓런처 (RocketLauncher)";
            case GunType.SoccerGun: return "축구공 (SoccerGun)";
            case GunType.BlockGun: return "블록건 (BlockGun)";
            case GunType.LightningShield: return "낙뢰 실드 (LightningShield)";
            default: return type.ToString();
        }
    }

    public void Close()
    {
        ClearSpawnedButtons();
        gameObject.SetActive(false);

        GUIPopupLayer popupLayer = GetComponentInParent<GUIPopupLayer>();
        if (popupLayer != null && popupLayer.curPopupState == E_POPUP_STATE.GUN_STATUS)
        {
            popupLayer.CloseCurrentPopup();
        }
    }

    private void ClearSpawnedButtons()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i] != null)
            {
                Destroy(spawnedButtons[i]);
            }
        }
        spawnedButtons.Clear();
    }
}
