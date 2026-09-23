using System;
using System.Collections.Generic;
using UnityEngine;

public class GunInventory : MonoBehaviour
{
    [Header("소유자(플레이어)")]
    [SerializeField] private Player player;

    [Header("현재 장착된 총기 목록")]
    [SerializeField] private List<ItemInfo> equippedGuns = new List<ItemInfo>();

    [Header("플레이어 자식으로 생성된 총기 게임오브젝트 목록")]
    [SerializeField] private List<GameObject> gunObjects = new List<GameObject>();

    [Header("레벨업 선택창 활성화 여부")]
    public bool isLevelUpSelecting = false;

    // 레벨업 시 추첨된 3개의 선택지
    private List<ItemInfo> currentChoices = new List<ItemInfo>();

    // GUI 스타일 캐싱
    private GUIStyle titleStyle;
    private GUIStyle cardStyle;
    private GUIStyle cardTitleStyle;
    private GUIStyle cardDescStyle;
    private GUIStyle hudStyle;
    private bool isStyleInitialized = false;

    private void Awake()
    {
        if (player == null)
        {
            player = GetComponent<Player>();
        }
    }

    private void Update()
    {
        // 1. 테스트용 숫자키 1~7: 각 기믹 무기를 건인벤토리에 즉시 추가
        if (Input.GetKeyDown(KeyCode.Alpha1)) AddGunByType(TotalGun.GunType.DefaultGun);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) AddGunByType(TotalGun.GunType.KunaiGun);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) AddGunByType(TotalGun.GunType.ShotGun);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) AddGunByType(TotalGun.GunType.RocketLauncher);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) AddGunByType(TotalGun.GunType.SoccerGun);
        else if (Input.GetKeyDown(KeyCode.Alpha6)) AddGunByType(TotalGun.GunType.BlockGun);
        else if (Input.GetKeyDown(KeyCode.Alpha7)) AddGunByType(TotalGun.GunType.LightningShield);

        // 2. L 키: 레벨업 선택창 열기
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (!isLevelUpSelecting)
            {
                ShowLevelUpSelection();
            }
        }

        // 3. K 키: 플레이어 테스트 피격 (실드 소모 또는 HP 감소 테스트)
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (player != null)
            {
                player.OnDamaged(10);
            }
        }
    }

    /// <summary>
    /// GunType enum 값을 받아 ItemInfoManager에서 정보를 찾아 건인벤토리에 추가합니다.
    /// </summary>
    public void AddGunByType(TotalGun.GunType gunType)
    {
        ItemInfo info = ItemInfoManager.Instance.GetItemInfo(gunType);
        if (info == null)
        {
            info = new ItemInfo((int)gunType + 100, gunType.ToString(), $"{gunType} 무기", gunType, 100, "");
        }
        AddGun(info);
    }

    /// <summary>
    /// 뱀서라이크식 레벨업 3지선다 선택창을 띄우고 게임을 일시정지합니다.
    /// </summary>
    public void ShowLevelUpSelection()
    {
        currentChoices = ItemInfoManager.Instance.GetRandomItemInfos(3);
        if (currentChoices == null || currentChoices.Count == 0)
        {
            Debug.LogWarning("[GunInventory] 선택할 수 있는 아이템 정보가 없습니다.");
            return;
        }

        isLevelUpSelecting = true;
        Time.timeScale = 0f; // 게임 일시정지
    }

    /// <summary>
    /// 선택한 아이템 정보를 인벤토리에 추가하고 플레이어 자식으로 독립된 TotalGun 오브젝트를 동적 생성하여 무기로 관리합니다.
    /// </summary>
    public void AddGun(ItemInfo itemInfo)
    {
        if (itemInfo == null) return;

        equippedGuns.Add(itemInfo);

        // 플레이어 자식으로 총기 GameObject 생성 (아이템/무기 타입명을 명시한 오브젝트명 부여)
        int index = gunObjects.Count;
        string gunName = $"TotalGun_{itemInfo.GunType}_{index + 1}";
        GameObject newGunObj = new GameObject(gunName);
        newGunObj.transform.SetParent(this.transform);

        // 총기별 위치 분산 (원형/오프셋 배치)
        float angle = index * (360f / 6f) * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angle) * 0.8f, Mathf.Sin(angle) * 0.8f, 0f);
        newGunObj.transform.localPosition = offset;
        newGunObj.transform.localRotation = Quaternion.identity;

        // TotalGun 컴포넌트를 부착/인스턴스화하고 해당 무기의 유한상태(State)로 진입
        AttachGunComponent(newGunObj, itemInfo.GunType);

        gunObjects.Add(newGunObj);

        Debug.Log($"[GunInventory] 새로운 무기 추가됨: {itemInfo.Name} (오브젝트명: {gunName}) | 보유 무기(실드) 수: {equippedGuns.Count}");
    }

    /// <summary>
    /// 총기 타입에 맞는 컴포넌트나 프리팹을 새 자식 오브젝트에 연결하고 유한상태를 설정합니다.
    /// </summary>
    private void AttachGunComponent(GameObject gunObj, TotalGun.GunType gunType)
    {
        GameObject totalGunPrefab = Resources.Load<GameObject>("Prefabs/Guns/TotalGun");
        TotalGun totalGun = null;

        if (totalGunPrefab != null)
        {
            GameObject instance = Instantiate(totalGunPrefab, gunObj.transform);
            instance.name = $"TotalGunInstance_{gunType}";
            totalGun = instance.GetComponent<TotalGun>();
        }
        else
        {
            totalGun = gunObj.AddComponent<TotalGun>();
        }

        if (totalGun != null)
        {
            totalGun.SetGunType(gunType, false);
        }
    }

    /// <summary>
    /// 피격 시 총기를 실드처럼 1개 소모하여 데미지를 방어합니다.
    /// 방어 성공 시 true, 총기가 없어 방어 실패 시 false 반환.
    /// </summary>
    public bool ConsumeShieldGun()
    {
        if (equippedGuns.Count > 0 && gunObjects.Count > 0)
        {
            int lastIndex = equippedGuns.Count - 1;
            ItemInfo removedGun = equippedGuns[lastIndex];
            GameObject removedObj = gunObjects[lastIndex];

            equippedGuns.RemoveAt(lastIndex);
            gunObjects.RemoveAt(lastIndex);

            if (removedObj != null)
            {
                Destroy(removedObj);
            }

            Debug.Log($"[GunInventory] 🛡️ 실드 발동! 총기 '{removedGun.Name}'({removedGun.GunType})를 소모하여 피해를 완벽히 막았습니다! 남은 총기: {equippedGuns.Count}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 현재 보유 중인 총기(실드) 개수
    /// </summary>
    public int GetGunCount()
    {
        return equippedGuns.Count;
    }

    /// <summary>
    /// 레거시 OnGUI를 사용한 뱀서라이크식 선택 팝업 및 HUD
    /// </summary>
    private void OnGUI()
    {
        InitStyles();

        // 1. 좌측 상단 HUD 표시 (보유 총기/실드 수량)
        GUI.Box(new Rect(10, 30, 240, 50), $"🛡️ 보유 총기(실드): {equippedGuns.Count}개\n[1~7]: 기믹 추가 | [L]: 레벨업 | [K]: 피격", hudStyle);

        // 2. 레벨업 선택 팝업창 표시
        if (isLevelUpSelecting && currentChoices != null && currentChoices.Count > 0)
        {
            // 화면 중앙 배경 윈도우 박스
            float winWidth = 650f;
            float winHeight = 320f;
            float winX = (Screen.width - winWidth) * 0.5f;
            float winY = (Screen.height - winHeight) * 0.5f;

            GUI.Box(new Rect(winX, winY, winWidth, winHeight), "★ LEVEL UP! 새로운 총기를 선택하세요 ★", titleStyle);

            // 3개의 카드형 버튼 배치
            float cardWidth = 190f;
            float cardHeight = 220f;
            float spacing = 15f;
            float startX = winX + 25f;
            float startY = winY + 65f;

            for (int i = 0; i < currentChoices.Count; i++)
            {
                ItemInfo choice = currentChoices[i];
                float x = startX + i * (cardWidth + spacing);
                Rect cardRect = new Rect(x, startY, cardWidth, cardHeight);

                // 카드 배경 박스
                GUI.Box(cardRect, "");

                // 아이템 정보 텍스트 구성
                string cardText = $"<b><color=#FFD700>[ {choice.Name} ]</color></b>\n\n" +
                                  $"타입: {choice.GunType}\n" +
                                  $"점수: +{choice.Score}\n\n" +
                                  $"<size=11>{choice.Description}</size>";

                // 카드 클릭(버튼) 처리
                if (GUI.Button(cardRect, cardText, cardStyle))
                {
                    SelectChoice(choice);
                }
            }
        }
    }

    private void SelectChoice(ItemInfo selectedInfo)
    {
        AddGun(selectedInfo);

        // 일시정지 해제 및 창 닫기
        isLevelUpSelecting = false;
        Time.timeScale = 1f;
    }

    private void InitStyles()
    {
        if (isStyleInitialized) return;

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

        hudStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 12,
            alignment = TextAnchor.UpperLeft
        };
        hudStyle.normal.textColor = Color.cyan;

        isStyleInitialized = true;
    }
}
