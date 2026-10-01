using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레거시 IMGUI(OnGUI) 화면 출력을 정적 함수 호출 방식으로 일원화한 정적 파사드 클래스
/// </summary>
public static class LegacyGUI
{
    private static bool _isEnabled = false;

    /// <summary>
    /// 레거시 GUI 전체 표시 여부 (테스트/디버그용 On/Off 토글)
    /// </summary>
    public static bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }

    /// <summary>
    /// 레거시 GUI 표시 상태 토글 (On <-> Off)
    /// </summary>
    public static void Toggle()
    {
        _isEnabled = !_isEnabled;
    }

    /// <summary>
    /// 플레이어 인게임 HUD 정보 갱신
    /// </summary>
    public static void SetPlayerHUD(int lv, int exp, int expMax, int hp, int hpMax, int score, string activeGunName, int gunCount)
    {
        LegacyGUIRenderer.Instance.UpdateHUD(lv, exp, expMax, hp, hpMax, score, activeGunName, gunCount);
    }

    /// <summary>
    /// 레벨업 3택 선택 팝업창 오픈 요청
    /// </summary>
    public static void ShowLevelUp(List<ItemInfo> choices, Action<ItemInfo> onSelected)
    {
        LegacyGUIRenderer.Instance.OpenLevelUpPopup(choices, onSelected);
    }

    /// <summary>
    /// 몬스터 인벤토리 목록 갱신
    /// </summary>
    public static void SetMonsterList(List<string> monsters)
    {
        LegacyGUIRenderer.Instance.UpdateMonsterList(monsters);
    }

    /// <summary>
    /// 플로팅 대미지 텍스트 출력
    /// </summary>
    public static void ShowDamage(Vector3 worldPos, int damage, bool isPlayerHit = false)
    {
        DamageTextManager.ShowDamage(worldPos, damage, isPlayerHit);
    }
}

/// <summary>
/// 실제 OnGUI 이벤트를 수신하여 LegacyGUI 데이터를 일괄 렌더링하는 중앙 싱글톤 컴포넌트
/// </summary>
public class LegacyGUIRenderer : MonoBehaviour
{
    private static LegacyGUIRenderer _instance;
    public static LegacyGUIRenderer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<LegacyGUIRenderer>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("@LegacyGUIRenderer");
                    _instance = go.AddComponent<LegacyGUIRenderer>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    // HUD 데이터 캐싱
    private struct HUDData
    {
        public int lv;
        public int exp;
        public int expMax;
        public int hp;
        public int hpMax;
        public int score;
        public string activeGunName;
        public int gunCount;
        public bool hasData;
    }

    private HUDData currentHUD;

    // 레벨업 팝업 데이터
    private bool isLevelUpActive = false;
    private List<ItemInfo> levelUpChoices = new List<ItemInfo>();
    private Action<ItemInfo> onChoiceSelected;

    // 몬스터 목록 디버그 데이터
    private List<string> monsterList = new List<string>();

    // GUI 스타일
    private GUIStyle hudStyle;
    private GUIStyle titleStyle;
    private GUIStyle cardStyle;
    private bool stylesInitialized = false;

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

    public void UpdateHUD(int lv, int exp, int expMax, int hp, int hpMax, int score, string activeGunName, int gunCount)
    {
        currentHUD.lv = lv;
        currentHUD.exp = exp;
        currentHUD.expMax = expMax;
        currentHUD.hp = hp;
        currentHUD.hpMax = hpMax;
        currentHUD.score = score;
        currentHUD.activeGunName = activeGunName;
        currentHUD.gunCount = gunCount;
        currentHUD.hasData = true;
    }

    public void OpenLevelUpPopup(List<ItemInfo> choices, Action<ItemInfo> onSelected)
    {
        levelUpChoices = choices != null ? new List<ItemInfo>(choices) : new List<ItemInfo>();
        onChoiceSelected = onSelected;
        isLevelUpActive = true;
        Time.timeScale = 0f; // 선택 중 일시정지
    }

    public void UpdateMonsterList(List<string> monsters)
    {
        monsterList = monsters != null ? new List<string>(monsters) : new List<string>();
    }

    private void OnGUI()
    {
        // 1. 전체 On/Off 스위치 체크
        if (!LegacyGUI.IsEnabled) return;

        InitStyles();

        // 2. 인게임 플레이어 & 건 HUD 렌더링 (화면 좌측 상단)
        if (currentHUD.hasData)
        {
            DrawPlayerHUD();
        }

        // 3. 레벨업 3택 카드 팝업 렌더링 (화면 중앙)
        if (isLevelUpActive && levelUpChoices.Count > 0)
        {
            DrawLevelUpPopup();
        }

        // 4. 몬스터 인벤토리 디버그 박스 렌더링 (HUD 하단)
        if (monsterList.Count > 0)
        {
            DrawMonsterList();
        }
    }

    private void DrawPlayerHUD()
    {
        // 상단 실드/총기 수량 및 가이드 박스
        string shieldInfo = $"🛡️ 보유 총기(실드): {currentHUD.gunCount}개 | [1~7]: 무기변경 | [X]: 액티브발사";
        GUI.Box(new Rect(10, 30, 380, 25), shieldInfo, hudStyle);

        // 플레이어 스탯 및 액티브 무기 박스
        string statInfo = $"★ Lv.{currentHUD.lv} (EXP: {currentHUD.exp}/{currentHUD.expMax}) | HP: {currentHUD.hp}/{currentHUD.hpMax}\n" +
                          $"Score: {currentHUD.score} | [Active]: <color=yellow>{currentHUD.activeGunName}</color>";
        GUI.Box(new Rect(10, 60, 380, 45), statInfo);
    }

    private void DrawLevelUpPopup()
    {
        float winWidth = 650f;
        float winHeight = 320f;
        float winX = (Screen.width - winWidth) * 0.5f;
        float winY = (Screen.height - winHeight) * 0.5f;

        // 배경 윈도우 박스
        GUI.Box(new Rect(winX, winY, winWidth, winHeight), "★ LEVEL UP! 새로운 총기를 선택하세요 ★", titleStyle);

        float cardWidth = 190f;
        float cardHeight = 220f;
        float spacing = 15f;
        float startX = winX + 25f;
        float startY = winY + 65f;

        for (int i = 0; i < levelUpChoices.Count; i++)
        {
            ItemInfo choice = levelUpChoices[i];
            float x = startX + i * (cardWidth + spacing);
            Rect cardRect = new Rect(x, startY, cardWidth, cardHeight);

            // 카드 배경
            GUI.Box(cardRect, "");

            string cardText = $"<b><color=#FFD700>[ {choice.Name} ]</color></b>\n\n" +
                              $"타입: {choice.GunType}\n" +
                              $"점수: +{choice.Score}\n\n" +
                              $"<size=11>{choice.Description}</size>";

            if (GUI.Button(cardRect, cardText, cardStyle))
            {
                isLevelUpActive = false;
                Time.timeScale = 1f;
                onChoiceSelected?.Invoke(choice);
                break;
            }
        }
    }

    private void DrawMonsterList()
    {
        int w = 110;
        int h = 20;
        int startY = 115;
        for (int i = 0; i < monsterList.Count; i++)
        {
            GUI.Box(new Rect(10, startY + (h * i), w, h), monsterList[i]);
        }
    }

    private void InitStyles()
    {
        if (stylesInitialized) return;

        hudStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleLeft
        };
        hudStyle.normal.textColor = Color.cyan;

        titleStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperCenter
        };
        titleStyle.normal.textColor = Color.yellow;

        cardStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 13,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            richText = true
        };
        cardStyle.normal.textColor = Color.white;

        stylesInitialized = true;
    }
}
