using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum E_POPUP_STATE
{
    NONE = 0,
    INVENTORY,
    LEVEL_UP,
    TROUBLESHOOTING,
    GUN_STATUS
}

/// <summary>
/// 팝업 레이어의 유한상태머신(FSM)을 전담 관리하며, UGUI 레이아웃을 자동 구성 및 제어하는 컴포넌트
/// </summary>
public class GUIPopupLayer : MonoBehaviour
{
    [Header("공용 배경")]
    public GameObject objDimmedBg;

    [Header("하위 팝업 UI 오브젝트")]
    public GUIInventory guiInventory;
    public UILevelUpPopup guiLevelUp;
    public UITroubleshootingPopup guiTroubleshooting;
    public UIGunStatusPopup guiGunStatus;

    [Header("현재 팝업 상태 (FSM)")]
    public E_POPUP_STATE curPopupState = E_POPUP_STATE.NONE;

    GameManager gameManager;
    private List<ItemInfo> levelUpChoices;
    private Action<ItemInfo> onLevelUpSelected;

    private void Awake()
    {
        // 씬에 레이아웃 오브젝트가 누락된 경우 자동 빌드
        BuildLayoutIfMissing();

        // 시작 시 모든 팝업 비활성화
        CloseAllPopupsInternal();
    }

    /// <summary>
    /// GameManager 참조를 받아 초기화
    /// </summary>
    public void Initialize(GameManager _gameManager)
    {
        this.gameManager = _gameManager;
    }

    /// <summary>
    /// 레벨업 팝업에 필요한 선택지 및 콜백 데이터 초기화
    /// </summary>
    public void InitializeLevelUp(List<ItemInfo> choices, Action<ItemInfo> onSelected)
    {
        this.levelUpChoices = choices;
        this.onLevelUpSelected = onSelected;
    }

    public bool IsPopupOpen()
    {
        return curPopupState != E_POPUP_STATE.NONE;
    }

    /// <summary>
    /// 단순 유한상태머신(FSM) 기반 팝업 상태 전환 (Enum 상태값만 입력받아 변경)
    /// </summary>
    public void SetPopupState(E_POPUP_STATE state)
    {
        Debug.Log($"[GUIPopupLayer] SetPopupState: {curPopupState} -> {state}");

        // 이전 팝업 정리
        CloseAllPopupsInternal();

        curPopupState = state;

        switch (state)
        {
            case E_POPUP_STATE.NONE:
                ShowNone();
                break;

            case E_POPUP_STATE.INVENTORY:
                ShowInventory();
                break;

            case E_POPUP_STATE.LEVEL_UP:
                ShowLevelUp();
                break;

            case E_POPUP_STATE.TROUBLESHOOTING:
                ShowTroubleshooting();
                break;

            case E_POPUP_STATE.GUN_STATUS:
                ShowGunStatus();
                break;
        }
    }

    private void ShowNone()
    {
        if (objDimmedBg != null) objDimmedBg.SetActive(false);
        Time.timeScale = 1f; // 게임 재개
    }

    private void ShowInventory()
    {
        if (objDimmedBg != null) objDimmedBg.SetActive(true);
        Time.timeScale = 0f; // 일시정지
        if (guiInventory != null)
        {
            guiInventory.gameObject.SetActive(true);
            if (gameManager != null && gameManager.monsterInventory != null)
            {
                guiInventory.SetInventory(gameManager.monsterInventory);
            }
        }
    }

    private void ShowLevelUp()
    {
        if (objDimmedBg != null) objDimmedBg.SetActive(true);
        Time.timeScale = 0f; // 일시정지
        if (guiLevelUp != null)
        {
            guiLevelUp.gameObject.SetActive(true);
            if (levelUpChoices != null)
            {
                guiLevelUp.Open(levelUpChoices, OnLevelUpItemSelected);
            }
        }
    }

    private void OnLevelUpItemSelected(ItemInfo selected)
    {
        CloseCurrentPopup();
        onLevelUpSelected?.Invoke(selected);
    }

    private void ShowTroubleshooting()
    {
        if (objDimmedBg != null) objDimmedBg.SetActive(true);
        Time.timeScale = 0f; // 일시정지
        if (guiTroubleshooting != null)
        {
            guiTroubleshooting.gameObject.SetActive(true);
            if (gameManager != null && gameManager.troubleshootingList != null)
            {
                guiTroubleshooting.Open(gameManager.troubleshootingList);
            }
        }
    }

    private void ShowGunStatus()
    {
        if (objDimmedBg != null) objDimmedBg.SetActive(true);
        Time.timeScale = 0f; // 일시정지
        if (guiGunStatus != null)
        {
            guiGunStatus.gameObject.SetActive(true);
            GunInventory gunInv = null;
            if (gameManager != null && gameManager.responnerPlayer != null && gameManager.responnerPlayer.objPlayer != null)
            {
                gunInv = gameManager.responnerPlayer.objPlayer.GetComponent<GunInventory>();
            }
            if (gunInv == null)
            {
                gunInv = FindFirstObjectByType<GunInventory>();
            }
            guiGunStatus.Open(gunInv);
        }
    }

    public void CloseCurrentPopup()
    {
        SetPopupState(E_POPUP_STATE.NONE);
    }

    /// <summary>
    /// 레벨업 팝업 오픈 전용 래퍼 함수 (데이터 초기화 후 Enum 상태 전환)
    /// </summary>
    public void OpenLevelUp(List<ItemInfo> choices, Action<ItemInfo> onSelected)
    {
        InitializeLevelUp(choices, onSelected);
        SetPopupState(E_POPUP_STATE.LEVEL_UP);
    }

    /// <summary>
    /// 총기 상태 팝업 오픈 전용 래퍼 함수
    /// </summary>
    public void OpenGunStatus()
    {
        SetPopupState(E_POPUP_STATE.GUN_STATUS);
    }

    private void CloseAllPopupsInternal()
    {
        if (guiInventory != null)
        {
            guiInventory.CloseIventory();
            guiInventory.gameObject.SetActive(false);
        }

        if (guiLevelUp != null)
        {
            guiLevelUp.Close();
        }

        if (guiTroubleshooting != null)
        {
            guiTroubleshooting.Close();
        }

        if (guiGunStatus != null)
        {
            guiGunStatus.Close();
        }

        if (objDimmedBg != null)
        {
            objDimmedBg.SetActive(false);
        }
    }

    /// <summary>
    /// 씬 내에 필요한 UGUI 팝업 레이아웃(LevelUp, Troubleshooting, GunStatus 등)이 없을 경우 동적으로 구성
    /// </summary>
    [ContextMenu("Rebuild UGUI Layout")]
    public void BuildLayoutIfMissing()
    {
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 1. DimmedBg 검사 및 생성
        if (objDimmedBg == null)
        {
            Transform existingBg = transform.Find("DimmedBg");
            if (existingBg != null)
            {
                objDimmedBg = existingBg.gameObject;
            }
            else
            {
                // PopupLayer 자체에 Image가 있으면 DimmedBg로 사용
                Image selfImg = GetComponent<Image>();
                if (selfImg != null)
                {
                    selfImg.color = new Color(0, 0, 0, 0.65f);
                    objDimmedBg = gameObject;
                }
            }
        }

        // 2. GUIInventory 탐색
        if (guiInventory == null)
        {
            guiInventory = GetComponentInChildren<GUIInventory>(true);
        }

        // 3. LevelUp 팝업 레이아웃 생성
        if (guiLevelUp == null)
        {
            Transform existingLevelUp = transform.Find("Popup_LevelUp");
            if (existingLevelUp != null)
            {
                guiLevelUp = existingLevelUp.GetComponent<UILevelUpPopup>();
            }
            else
            {
                guiLevelUp = CreateLevelUpPopupLayout(defaultFont);
            }
        }

        // 4. Troubleshooting 팝업 레이아웃 생성
        if (guiTroubleshooting == null)
        {
            Transform existingTrouble = transform.Find("Popup_Troubleshooting");
            if (existingTrouble != null)
            {
                guiTroubleshooting = existingTrouble.GetComponent<UITroubleshootingPopup>();
            }
            else
            {
                guiTroubleshooting = CreateTroubleshootingPopupLayout(defaultFont);
            }
        }

        // 5. GunStatus 팝업 레이아웃 생성
        if (guiGunStatus == null)
        {
            Transform existingGunStatus = transform.Find("Popup_GunStatus");
            if (existingGunStatus != null)
            {
                guiGunStatus = existingGunStatus.GetComponent<UIGunStatusPopup>();
            }
            else
            {
                guiGunStatus = CreateGunStatusPopupLayout(defaultFont);
            }
        }

        SetLayerRecursively(gameObject, 5);
    }

    private void SetLayerRecursively(GameObject go, int layer)
    {
        if (go == null) return;
        go.layer = layer;
        for (int i = 0; i < go.transform.childCount; i++)
        {
            SetLayerRecursively(go.transform.GetChild(i).gameObject, layer);
        }
    }

    private UILevelUpPopup CreateLevelUpPopupLayout(Font font)
    {
        GameObject popupGo = new GameObject("Popup_LevelUp", typeof(RectTransform));
        popupGo.transform.SetParent(transform, false);
        RectTransform rt = popupGo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(720, 420);

        // 배경 패널
        Image bgImg = popupGo.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.12f, 0.16f, 0.95f);

        // 타이틀
        GameObject titleGo = new GameObject("Text_Title", typeof(RectTransform), typeof(Text));
        titleGo.transform.SetParent(popupGo.transform, false);
        RectTransform titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 1);
        titleRt.anchoredPosition = new Vector2(0, -15);
        titleRt.sizeDelta = new Vector2(0, 40);

        Text titleText = titleGo.GetComponent<Text>();
        titleText.font = font;
        titleText.fontSize = 20;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(1f, 0.85f, 0.2f);
        titleText.text = "★ LEVEL UP! 새로운 총기를 선택하세요 ★";

        // 카드 컨테이너 (HorizontalLayoutGroup)
        GameObject containerGo = new GameObject("CardContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        containerGo.transform.SetParent(popupGo.transform, false);
        RectTransform containerRt = containerGo.GetComponent<RectTransform>();
        containerRt.anchorMin = new Vector2(0.5f, 0.5f);
        containerRt.anchorMax = new Vector2(0.5f, 0.5f);
        containerRt.pivot = new Vector2(0.5f, 0.5f);
        containerRt.anchoredPosition = new Vector2(0, -20);
        containerRt.sizeDelta = new Vector2(680, 290);

        HorizontalLayoutGroup hlg = containerGo.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 15;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        UILevelUpPopup levelUpPopup = popupGo.AddComponent<UILevelUpPopup>();
        levelUpPopup.textTitle = titleText;
        levelUpPopup.cardContainer = containerGo.transform;
        levelUpPopup.fixedCardSlots = new List<UILevelUpCard>();

        // 3개의 고정 카드 슬롯 생성
        for (int i = 0; i < 3; i++)
        {
            GameObject cardGo = CreateCardSlotGameObject(containerGo.transform, i + 1, font);
            UILevelUpCard cardComp = cardGo.GetComponent<UILevelUpCard>();
            levelUpPopup.fixedCardSlots.Add(cardComp);
        }

        popupGo.SetActive(false);
        return levelUpPopup;
    }

    private GameObject CreateCardSlotGameObject(Transform parent, int slotIndex, Font font)
    {
        GameObject cardGo = new GameObject($"Card_Slot_{slotIndex}", typeof(RectTransform), typeof(Image), typeof(Button));
        cardGo.transform.SetParent(parent, false);
        RectTransform rt = cardGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(210, 270);

        Image cardImg = cardGo.GetComponent<Image>();
        cardImg.color = new Color(0.18f, 0.2f, 0.26f, 1f);

        Button btn = cardGo.GetComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.3f, 0.35f, 0.45f, 1f);
        cb.pressedColor = new Color(0.15f, 0.17f, 0.22f, 1f);
        btn.colors = cb;

        // 1. 이름 텍스트
        GameObject nameGo = new GameObject("Text_Name", typeof(RectTransform), typeof(Text));
        nameGo.transform.SetParent(cardGo.transform, false);
        RectTransform nameRt = nameGo.GetComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0, 1);
        nameRt.anchorMax = new Vector2(1, 1);
        nameRt.pivot = new Vector2(0.5f, 1);
        nameRt.anchoredPosition = new Vector2(0, -15);
        nameRt.sizeDelta = new Vector2(-20, 30);
        Text nameText = nameGo.GetComponent<Text>();
        nameText.font = font;
        nameText.fontSize = 17;
        nameText.fontStyle = FontStyle.Bold;
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.color = new Color(1f, 0.85f, 0f);

        // 2. 무기 타입 텍스트
        GameObject typeGo = new GameObject("Text_Type", typeof(RectTransform), typeof(Text));
        typeGo.transform.SetParent(cardGo.transform, false);
        RectTransform typeRt = typeGo.GetComponent<RectTransform>();
        typeRt.anchorMin = new Vector2(0, 1);
        typeRt.anchorMax = new Vector2(1, 1);
        typeRt.pivot = new Vector2(0.5f, 1);
        typeRt.anchoredPosition = new Vector2(0, -48);
        typeRt.sizeDelta = new Vector2(-20, 22);
        Text typeText = typeGo.GetComponent<Text>();
        typeText.font = font;
        typeText.fontSize = 13;
        typeText.alignment = TextAnchor.MiddleCenter;
        typeText.color = new Color(0.3f, 0.9f, 1f);

        // 3. 점수 텍스트
        GameObject scoreGo = new GameObject("Text_Score", typeof(RectTransform), typeof(Text));
        scoreGo.transform.SetParent(cardGo.transform, false);
        RectTransform scoreRt = scoreGo.GetComponent<RectTransform>();
        scoreRt.anchorMin = new Vector2(0, 1);
        scoreRt.anchorMax = new Vector2(1, 1);
        scoreRt.pivot = new Vector2(0.5f, 1);
        scoreRt.anchoredPosition = new Vector2(0, -72);
        scoreRt.sizeDelta = new Vector2(-20, 20);
        Text scoreText = scoreGo.GetComponent<Text>();
        scoreText.font = font;
        scoreText.fontSize = 12;
        scoreText.alignment = TextAnchor.MiddleCenter;
        scoreText.color = new Color(0.4f, 1f, 0.4f);

        // 4. 설명문 텍스트
        GameObject descGo = new GameObject("Text_Desc", typeof(RectTransform), typeof(Text));
        descGo.transform.SetParent(cardGo.transform, false);
        RectTransform descRt = descGo.GetComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0, 0);
        descRt.anchorMax = new Vector2(1, 1);
        descRt.pivot = new Vector2(0.5f, 0.5f);
        descRt.anchoredPosition = new Vector2(0, -45);
        descRt.sizeDelta = new Vector2(-24, -120);
        Text descText = descGo.GetComponent<Text>();
        descText.font = font;
        descText.fontSize = 12;
        descText.alignment = TextAnchor.UpperLeft;
        descText.horizontalOverflow = HorizontalWrapMode.Wrap;
        descText.verticalOverflow = VerticalWrapMode.Truncate;
        descText.color = new Color(0.9f, 0.9f, 0.9f);

        UILevelUpCard cardComp = cardGo.AddComponent<UILevelUpCard>();
        cardComp.textName = nameText;
        cardComp.textType = typeText;
        cardComp.textScore = scoreText;
        cardComp.textDescription = descText;
        cardComp.buttonSelect = btn;

        return cardGo;
    }

    private UITroubleshootingPopup CreateTroubleshootingPopupLayout(Font font)
    {
        GameObject popupGo = new GameObject("Popup_Troubleshooting", typeof(RectTransform));
        popupGo.transform.SetParent(transform, false);
        RectTransform rt = popupGo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(640, 500);

        // 배경 패널
        Image bgImg = popupGo.AddComponent<Image>();
        bgImg.color = new Color(0.08f, 0.1f, 0.14f, 0.96f);

        // 1. 헤더 영역
        GameObject headerGo = new GameObject("Header", typeof(RectTransform));
        headerGo.transform.SetParent(popupGo.transform, false);
        RectTransform headerRt = headerGo.GetComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0, 1);
        headerRt.anchorMax = new Vector2(1, 1);
        headerRt.pivot = new Vector2(0.5f, 1);
        headerRt.anchoredPosition = Vector2.zero;
        headerRt.sizeDelta = new Vector2(0, 50);

        // 타이틀 텍스트
        GameObject titleGo = new GameObject("Text_Title", typeof(RectTransform), typeof(Text));
        titleGo.transform.SetParent(headerGo.transform, false);
        RectTransform titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 0);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0, 0.5f);
        titleRt.anchoredPosition = new Vector2(15, 0);
        titleRt.sizeDelta = new Vector2(-80, 0);
        Text titleText = titleGo.GetComponent<Text>();
        titleText.font = font;
        titleText.fontSize = 17;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleLeft;
        titleText.color = new Color(0.2f, 0.9f, 1f);
        titleText.text = "<b>[VSLike 트러블슈팅 수정 내역]</b>";

        // 닫기 버튼
        GameObject closeBtnGo = new GameObject("Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnGo.transform.SetParent(headerGo.transform, false);
        RectTransform closeRt = closeBtnGo.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(1, 0.5f);
        closeRt.anchorMax = new Vector2(1, 0.5f);
        closeRt.pivot = new Vector2(1, 0.5f);
        closeRt.anchoredPosition = new Vector2(-10, 0);
        closeRt.sizeDelta = new Vector2(34, 34);
        Image closeImg = closeBtnGo.GetComponent<Image>();
        closeImg.color = new Color(0.8f, 0.2f, 0.2f, 1f);

        GameObject closeTextGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
        closeTextGo.transform.SetParent(closeBtnGo.transform, false);
        RectTransform ctRt = closeTextGo.GetComponent<RectTransform>();
        ctRt.anchorMin = Vector2.zero;
        ctRt.anchorMax = Vector2.one;
        ctRt.sizeDelta = Vector2.zero;
        Text closeText = closeTextGo.GetComponent<Text>();
        closeText.font = font;
        closeText.fontSize = 16;
        closeText.fontStyle = FontStyle.Bold;
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.color = Color.white;
        closeText.text = "✕";

        // 2. ScrollView 영역
        GameObject scrollGo = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
        scrollGo.transform.SetParent(popupGo.transform, false);
        RectTransform scrollRt = scrollGo.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.pivot = new Vector2(0.5f, 0.5f);
        scrollRt.anchoredPosition = new Vector2(0, -25);
        scrollRt.sizeDelta = new Vector2(-20, -65);

        ScrollRect scrollRect = scrollGo.GetComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        // Viewport
        GameObject viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
        viewportGo.transform.SetParent(scrollGo.transform, false);
        RectTransform vpRt = viewportGo.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.pivot = new Vector2(0, 1);
        vpRt.sizeDelta = Vector2.zero;
        viewportGo.GetComponent<Mask>().showMaskGraphic = false;
        scrollRect.viewport = vpRt;

        // Content
        GameObject contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentGo.transform.SetParent(viewportGo.transform, false);
        RectTransform contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0, 1);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0, 0);

        VerticalLayoutGroup vlg = contentGo.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.padding = new RectOffset(5, 5, 5, 5);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        ContentSizeFitter csf = contentGo.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentRt;

        // 3. 템플릿 Entry 프리팹 생성
        GameObject templateEntry = CreateTroubleshootingEntryTemplate(font);
        templateEntry.transform.SetParent(popupGo.transform, false);
        templateEntry.SetActive(false);

        UITroubleshootingPopup troublePopup = popupGo.AddComponent<UITroubleshootingPopup>();
        troublePopup.textTitle = titleText;
        troublePopup.contentContainer = contentRt;
        troublePopup.entryPrefab = templateEntry;
        troublePopup.buttonClose = closeBtnGo.GetComponent<Button>();

        popupGo.SetActive(false);
        return troublePopup;
    }

    private GameObject CreateTroubleshootingEntryTemplate(Font font)
    {
        GameObject entryGo = new GameObject("UI_TroubleEntry_Template", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        RectTransform rt = entryGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 100);

        Image img = entryGo.GetComponent<Image>();
        img.color = new Color(0.14f, 0.16f, 0.22f, 0.9f);

        VerticalLayoutGroup vlg = entryGo.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 4;
        vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        ContentSizeFitter csf = entryGo.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Header (Type / Version / Status)
        GameObject headerGo = new GameObject("Text_Header", typeof(RectTransform), typeof(Text));
        headerGo.transform.SetParent(entryGo.transform, false);
        Text headerText = headerGo.GetComponent<Text>();
        headerText.font = font;
        headerText.fontSize = 13;
        headerText.fontStyle = FontStyle.Bold;
        headerText.color = Color.white;

        // Status
        GameObject statusGo = new GameObject("Text_Status", typeof(RectTransform), typeof(Text));
        statusGo.transform.SetParent(entryGo.transform, false);
        Text statusText = statusGo.GetComponent<Text>();
        statusText.font = font;
        statusText.fontSize = 12;
        statusText.fontStyle = FontStyle.Bold;
        statusText.color = Color.cyan;

        // Desc
        GameObject descGo = new GameObject("Text_Desc", typeof(RectTransform), typeof(Text));
        descGo.transform.SetParent(entryGo.transform, false);
        Text descText = descGo.GetComponent<Text>();
        descText.font = font;
        descText.fontSize = 12;
        descText.color = new Color(0.9f, 0.9f, 0.9f);

        // Cause
        GameObject causeGo = new GameObject("Text_Cause", typeof(RectTransform), typeof(Text));
        causeGo.transform.SetParent(entryGo.transform, false);
        Text causeText = causeGo.GetComponent<Text>();
        causeText.font = font;
        causeText.fontSize = 11;
        causeText.color = new Color(0.75f, 0.75f, 0.75f);

        // Solution
        GameObject solGo = new GameObject("Text_Solution", typeof(RectTransform), typeof(Text));
        solGo.transform.SetParent(entryGo.transform, false);
        Text solText = solGo.GetComponent<Text>();
        solText.font = font;
        solText.fontSize = 11;
        solText.color = new Color(0.6f, 0.9f, 0.6f);

        UITroubleshootingEntry entryComp = entryGo.AddComponent<UITroubleshootingEntry>();
        entryComp.textHeader = headerText;
        entryComp.textStatus = statusText;
        entryComp.textDescription = descText;
        entryComp.textCause = causeText;
        entryComp.textSolution = solText;

        return entryGo;
    }

    private UIGunStatusPopup CreateGunStatusPopupLayout(Font font)
    {
        GameObject popupGo = new GameObject("Popup_GunStatus", typeof(RectTransform));
        popupGo.transform.SetParent(transform, false);
        RectTransform rt = popupGo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(680, 420);

        // 윈도우 배경 패널
        Image bgImg = popupGo.AddComponent<Image>();
        bgImg.color = new Color(0.09f, 0.11f, 0.15f, 0.97f);

        // 1. 헤더 영역 (타이틀 + 닫기 버튼)
        GameObject headerGo = new GameObject("Header", typeof(RectTransform));
        headerGo.transform.SetParent(popupGo.transform, false);
        RectTransform headerRt = headerGo.GetComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0, 1);
        headerRt.anchorMax = new Vector2(1, 1);
        headerRt.pivot = new Vector2(0.5f, 1);
        headerRt.anchoredPosition = Vector2.zero;
        headerRt.sizeDelta = new Vector2(0, 45);

        GameObject titleGo = new GameObject("Text_Title", typeof(RectTransform), typeof(Text));
        titleGo.transform.SetParent(headerGo.transform, false);
        RectTransform titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 0);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0, 0.5f);
        titleRt.anchoredPosition = new Vector2(15, 0);
        titleRt.sizeDelta = new Vector2(-70, 0);
        Text titleText = titleGo.GetComponent<Text>();
        titleText.font = font;
        titleText.fontSize = 18;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleLeft;
        titleText.color = new Color(1f, 0.85f, 0.2f);
        titleText.text = "<b>[ 무기 상태 및 레벨 정보 (G) ]</b>";

        GameObject closeBtnGo = new GameObject("Btn_Close", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnGo.transform.SetParent(headerGo.transform, false);
        RectTransform closeRt = closeBtnGo.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(1, 0.5f);
        closeRt.anchorMax = new Vector2(1, 0.5f);
        closeRt.pivot = new Vector2(1, 0.5f);
        closeRt.anchoredPosition = new Vector2(-10, 0);
        closeRt.sizeDelta = new Vector2(30, 30);
        Image closeImg = closeBtnGo.GetComponent<Image>();
        closeImg.color = new Color(0.8f, 0.2f, 0.2f, 1f);

        GameObject closeTextGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
        closeTextGo.transform.SetParent(closeBtnGo.transform, false);
        RectTransform ctRt = closeTextGo.GetComponent<RectTransform>();
        ctRt.anchorMin = Vector2.zero;
        ctRt.anchorMax = Vector2.one;
        ctRt.sizeDelta = Vector2.zero;
        Text closeText = closeTextGo.GetComponent<Text>();
        closeText.font = font;
        closeText.fontSize = 15;
        closeText.fontStyle = FontStyle.Bold;
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.color = Color.white;
        closeText.text = "✕";

        // 2. 좌측 패널: 총기 슬롯 리스트 (Width: 260, Height: 350)
        GameObject leftPanelGo = new GameObject("LeftPanel_GunList", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        leftPanelGo.transform.SetParent(popupGo.transform, false);
        RectTransform leftRt = leftPanelGo.GetComponent<RectTransform>();
        leftRt.anchorMin = new Vector2(0, 0);
        leftRt.anchorMax = new Vector2(0, 1);
        leftRt.pivot = new Vector2(0, 0.5f);
        leftRt.anchoredPosition = new Vector2(15, -20);
        leftRt.sizeDelta = new Vector2(260, -60);

        Image leftImg = leftPanelGo.GetComponent<Image>();
        leftImg.color = new Color(0.13f, 0.15f, 0.2f, 0.9f);

        VerticalLayoutGroup leftVlg = leftPanelGo.GetComponent<VerticalLayoutGroup>();
        leftVlg.spacing = 6;
        leftVlg.padding = new RectOffset(8, 8, 8, 8);
        leftVlg.childForceExpandWidth = true;
        leftVlg.childForceExpandHeight = false;
        leftVlg.childControlWidth = true;
        leftVlg.childControlHeight = false;

        // 3. 우측 패널: 선택된 총기 상세 정보 (Width: 360, Height: 350)
        GameObject rightPanelGo = new GameObject("RightPanel_GunDetail", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        rightPanelGo.transform.SetParent(popupGo.transform, false);
        RectTransform rightRt = rightPanelGo.GetComponent<RectTransform>();
        rightRt.anchorMin = new Vector2(1, 0);
        rightRt.anchorMax = new Vector2(1, 1);
        rightRt.pivot = new Vector2(1, 0.5f);
        rightRt.anchoredPosition = new Vector2(-15, -20);
        rightRt.sizeDelta = new Vector2(360, -60);

        Image rightImg = rightPanelGo.GetComponent<Image>();
        rightImg.color = new Color(0.15f, 0.18f, 0.24f, 0.95f);

        VerticalLayoutGroup rightVlg = rightPanelGo.GetComponent<VerticalLayoutGroup>();
        rightVlg.spacing = 10;
        rightVlg.padding = new RectOffset(14, 14, 14, 14);
        rightVlg.childForceExpandWidth = true;
        rightVlg.childForceExpandHeight = false;
        rightVlg.childControlWidth = true;
        rightVlg.childControlHeight = false;

        // 우측 텍스트들
        GameObject dNameGo = new GameObject("Text_DetailName", typeof(RectTransform), typeof(Text));
        dNameGo.transform.SetParent(rightPanelGo.transform, false);
        RectTransform dnRt = dNameGo.GetComponent<RectTransform>();
        dnRt.sizeDelta = new Vector2(0, 30);
        Text dNameText = dNameGo.GetComponent<Text>();
        dNameText.font = font;
        dNameText.fontSize = 18;
        dNameText.fontStyle = FontStyle.Bold;
        dNameText.color = new Color(1f, 0.85f, 0f);
        dNameText.text = "[ 총기명 ]";

        GameObject dTypeGo = new GameObject("Text_DetailType", typeof(RectTransform), typeof(Text));
        dTypeGo.transform.SetParent(rightPanelGo.transform, false);
        RectTransform dtRt = dTypeGo.GetComponent<RectTransform>();
        dtRt.sizeDelta = new Vector2(0, 40);
        Text dTypeText = dTypeGo.GetComponent<Text>();
        dTypeText.font = font;
        dTypeText.fontSize = 13;
        dTypeText.color = new Color(0.3f, 0.9f, 1f);
        dTypeText.text = "무기 분류: -\n장착 상태: -";

        GameObject dStatsGo = new GameObject("Text_DetailStats", typeof(RectTransform), typeof(Text));
        dStatsGo.transform.SetParent(rightPanelGo.transform, false);
        RectTransform dsRt = dStatsGo.GetComponent<RectTransform>();
        dsRt.sizeDelta = new Vector2(0, 100);
        Text dStatsText = dStatsGo.GetComponent<Text>();
        dStatsText.font = font;
        dStatsText.fontSize = 12;
        dStatsText.color = new Color(0.9f, 0.9f, 0.9f);
        dStatsText.text = "전투 스탯 정보";

        GameObject dDescGo = new GameObject("Text_DetailDesc", typeof(RectTransform), typeof(Text));
        dDescGo.transform.SetParent(rightPanelGo.transform, false);
        RectTransform ddRt = dDescGo.GetComponent<RectTransform>();
        ddRt.sizeDelta = new Vector2(0, 110);
        Text dDescText = dDescGo.GetComponent<Text>();
        dDescText.font = font;
        dDescText.fontSize = 12;
        dDescText.color = new Color(0.85f, 0.85f, 0.85f);
        dDescText.horizontalOverflow = HorizontalWrapMode.Wrap;
        dDescText.text = "무기 상세 설명문";

        UIGunStatusPopup statusPopup = popupGo.AddComponent<UIGunStatusPopup>();
        statusPopup.textTitle = titleText;
        statusPopup.buttonClose = closeBtnGo.GetComponent<Button>();
        statusPopup.listContainer = leftPanelGo.transform;
        statusPopup.textDetailName = dNameText;
        statusPopup.textDetailType = dTypeText;
        statusPopup.textDetailStats = dStatsText;
        statusPopup.textDetailDesc = dDescText;
        statusPopup.fixedSlotButtons = new List<UIGunSlotButton>();

        // 좌측 고정 슬롯 버튼 7개 생성
        for (int i = 0; i < 7; i++)
        {
            GameObject slotGo = CreateGunSlotButtonGameObject(leftPanelGo.transform, i + 1, font);
            UIGunSlotButton slotComp = slotGo.GetComponent<UIGunSlotButton>();
            statusPopup.fixedSlotButtons.Add(slotComp);
        }

        popupGo.SetActive(false);
        return statusPopup;
    }

    private GameObject CreateGunSlotButtonGameObject(Transform parent, int slotIndex, Font font)
    {
        GameObject slotGo = new GameObject($"Gun_Slot_{slotIndex}", typeof(RectTransform), typeof(Image), typeof(Button));
        slotGo.transform.SetParent(parent, false);
        RectTransform rt = slotGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 42);

        Image img = slotGo.GetComponent<Image>();
        img.color = new Color(0.2f, 0.23f, 0.3f, 1f);

        Button btn = slotGo.GetComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.35f, 0.4f, 0.55f, 1f);
        cb.pressedColor = new Color(0.15f, 0.18f, 0.25f, 1f);
        btn.colors = cb;

        // 이름 텍스트 (좌측)
        GameObject nameGo = new GameObject("Text_GunName", typeof(RectTransform), typeof(Text));
        nameGo.transform.SetParent(slotGo.transform, false);
        RectTransform nameRt = nameGo.GetComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0, 0);
        nameRt.anchorMax = new Vector2(0.7f, 1);
        nameRt.pivot = new Vector2(0, 0.5f);
        nameRt.anchoredPosition = new Vector2(10, 0);
        nameRt.sizeDelta = new Vector2(-10, 0);
        Text nameText = nameGo.GetComponent<Text>();
        nameText.font = font;
        nameText.fontSize = 13;
        nameText.fontStyle = FontStyle.Bold;
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.color = Color.white;
        nameText.text = "Gun Name";

        // 레벨 텍스트 (우측)
        GameObject lvGo = new GameObject("Text_GunLevel", typeof(RectTransform), typeof(Text));
        lvGo.transform.SetParent(slotGo.transform, false);
        RectTransform lvRt = lvGo.GetComponent<RectTransform>();
        lvRt.anchorMin = new Vector2(0.7f, 0);
        lvRt.anchorMax = new Vector2(1, 1);
        lvRt.pivot = new Vector2(1, 0.5f);
        lvRt.anchoredPosition = new Vector2(-8, 0);
        lvRt.sizeDelta = new Vector2(-8, 0);
        Text lvText = lvGo.GetComponent<Text>();
        lvText.font = font;
        lvText.fontSize = 12;
        lvText.fontStyle = FontStyle.Bold;
        lvText.alignment = TextAnchor.MiddleRight;
        lvText.color = new Color(0.2f, 1f, 0.4f);
        lvText.text = "Lv.1";

        UIGunSlotButton slotComp = slotGo.AddComponent<UIGunSlotButton>();
        slotComp.textGunName = nameText;
        slotComp.textGunLevel = lvText;
        slotComp.button = btn;

        return slotGo;
    }
}
