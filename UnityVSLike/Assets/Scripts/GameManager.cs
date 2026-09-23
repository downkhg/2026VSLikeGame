using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CameraTracker mainCameraTracker;
    public Responner responnerPlayer;
    public Responner responnerEagle;
    public Responner responnerOpossum;
    public MonsterInventory monsterInventory;

    static GameManager instance;

    public static GameManager GetInstacne()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }

    public GameObject objPopupLayer;
    public GUIInventory guiInventory;


    public enum E_GUI_STATUS { TITILE, GAMEOVER, THEEND, PLAY }
    public List<GameObject> listGUIScence;
    public E_GUI_STATUS curGUIStatus;

    private void Start()
    {
        EventShowMeTheItem();

        if (guiInventory != null)
            guiInventory.SetInventory(monsterInventory);

        SetGUIStatus(curGUIStatus);
    }

    public void ShowGUIScence(int idx)
    {
        if (listGUIScence == null) return;

        for (int i = 0; i < listGUIScence.Count; i++)
        {
            if (listGUIScence[i] != null)
            {
                if (i == idx) listGUIScence[i].SetActive(true);
                else listGUIScence[i].SetActive(false);
            }
        }
    }

    public void SetGUIStatus(E_GUI_STATUS status)
    {
        switch (status)
        {
            case E_GUI_STATUS.TITILE:
                Time.timeScale = 0;
                break;
            case E_GUI_STATUS.GAMEOVER:
                Time.timeScale = 0;
                break;
            case E_GUI_STATUS.THEEND:
                Time.timeScale = 0;
                break;
            case E_GUI_STATUS.PLAY:
                Time.timeScale = 1;
                break;
        }
        curGUIStatus = status;
        ShowGUIScence((int)status);
    }

    public void UpdateGUIStatus()
    {
        switch (curGUIStatus)
        {
            case E_GUI_STATUS.TITILE:
                break;
            case E_GUI_STATUS.GAMEOVER:
                break;
            case E_GUI_STATUS.THEEND:
                break;
            case E_GUI_STATUS.PLAY:
                EventGameOverProcess();
                EventInventoryInput();
                break;
        }
    }

    public void PopupLayerShow(bool active)
    {
        if (guiInventory != null)
        {
            if (active)
                guiInventory.SetInventory(monsterInventory);
            else
                guiInventory.CloseIventory();
        }

        if (objPopupLayer != null)
            objPopupLayer.SetActive(active);
    }

    public void EventInventoryInput()
    {
        if (objPopupLayer == null) return;

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (objPopupLayer.activeSelf)
            {
                PopupLayerShow(false);
            }
            else
            {
                PopupLayerShow(true);
            }
        }
    }

    public void EventGUISceneChange(E_GUI_STATUS state)
    {
        SetGUIStatus(state);
    }

    public void EventGUISceneChange(int idx)
    {
        SetGUIStatus((E_GUI_STATUS)idx);
    }

    public void EventStart()
    {
        SetGUIStatus(E_GUI_STATUS.PLAY);
    }

    public void EventExit()
    {
        Debug.Log("GameManager.EventExit()");
        Application.Quit();
    }

    public void EventGameOverProcess()
    {
        if (responnerPlayer == null) return;

        if (responnerPlayer.objPlayer == null)
        {
            SetGUIStatus(E_GUI_STATUS.GAMEOVER);
        }
    }

    public void EventShowMeTheItem()
    {
        if (monsterInventory == null) return;

        for (int i = 0; i < 5; i++)
        {
            monsterInventory.AddMonster("fox");
            monsterInventory.AddMonster("frog");
            monsterInventory.AddMonster("eagle");
            monsterInventory.AddMonster("opossum");
        }
    }

    void CameraTrackingTargetPlayerProcess()
    {
        if (mainCameraTracker == null || responnerPlayer == null) return;

        if (mainCameraTracker.objTarget == null)
        {
            if (responnerPlayer.objPlayer != null)
                mainCameraTracker.objTarget = responnerPlayer.objPlayer;
        }
    }

    void EaglePointSetting()
    {
        if (responnerEagle == null || responnerOpossum == null) return;

        if (responnerEagle.objPlayer != null)
        {
            Eagle eagle = responnerEagle.objPlayer.GetComponent<Eagle>();

            if (eagle == null) return;

            if (eagle.objPatrolPoint == null && eagle.objResponPoint == null)
            {
                eagle.objPatrolPoint = responnerOpossum.gameObject;
                eagle.objResponPoint = responnerEagle.gameObject;
                eagle.SetAIState(Eagle.E_AI_STATE.RETRUN);
            }
        }
    }

    public GUIPlayerInfo guiPlayerInfo;

    [Header("버전 관리 (트러블슈팅 문서 기준)")]
    public int majorVersion = 0;
    public int releaseVersion = 00;
    public int patchVersion = 07;

    [Header("트러블슈팅 OnGUI 디스플레이")]
    public bool showVersionGUI = true;
    public bool showTroubleshootingList = false;

    [System.Serializable]
    public class TroubleshootingEntry
    {
        public string type;           // 오류, 버그, 시스템, 건의사항, 변경사항
        public string foundVersion;   // 발견버전
        public string status;         // 수정요청, 수정중, 테스트요청, 완료, 검토중, 보류중
        public string appliedVersion; // 적용버전
        public string description;    // 발생현상/내용
        public string cause;          // 추정원인
        public string solution;       // 해결방안
    }

    [Header("트러블슈팅 내역")]
    public List<TroubleshootingEntry> troubleshootingList = new List<TroubleshootingEntry>()
    {
        new TroubleshootingEntry()
        {
            type = "오류",
            foundVersion = "0.00.00",
            status = "완료",
            appliedVersion = "0.00.01",
            description = "토탈건으로 종류를 변경하여도 작동은 하나 오브젝트가 활성화 되지않음.",
            cause = "비활성화 된 객체에서 api로 접근하기 때문에 작동 가능",
            solution = "TotalGun에서 SetGunType 시 하위 무기 활성화/비활성화 처리 로직 구현"
        },
        new TroubleshootingEntry()
        {
            type = "오류",
            foundVersion = "0.00.00",
            status = "완료",
            appliedVersion = "0.00.02",
            description = "샷건에서 발사각도를 수정해도 수정되지않음.",
            cause = "발사 위치 및 방향 설정 시 부채꼴 분산각(spreadAngle) 미반영 및 코드 중복 생성 결함",
            solution = "타겟 사이각 비교 후 정면/타겟 기준 각도 산출 및 Random.Range 기반 단일 탄환 발사 파이프라인 정리"
        },
        new TroubleshootingEntry()
        {
            type = "버그",
            foundVersion = "0.00.01",
            status = "테스트요청",
            appliedVersion = "0.00.02",
            description = "총알이 속도가 빠를 때 원하지 않는 방향으로 반사됨",
            cause = "총알의 속도가 빨라 터널링 현상이 발생하여 충돌체크에 오류가 일어남",
            solution = "터널링 현상을 해결하기 위해 FixedUpdate를 사용하고, 충돌 시 충돌 위치(접점/법선)를 확인하여 예외처리 및 위치 보정함"
        },
        new TroubleshootingEntry()
        {
            type = "시스템",
            foundVersion = "0.00.02",
            status = "수정완료",
            appliedVersion = "0.00.03",
            description = "토탈건에서 전체 무기 자동 발사가 제어되지 않고 수동 발사에만 의존하던 문제",
            cause = "Gun 스크립트의 Update 부재, TotalGun의 LightningShield/BlockGun/Rocket 분기 누락 및 개별 무기 Update 활성화 미연동",
            solution = "모든 무기에 fireInterval 기반 Update 자동 발사 구현, TotalGun에서 Shot(dir) 통합 인터페이스 완성 및 활성 무기 enabled 제어 연동"
        },
        new TroubleshootingEntry()
        {
            type = "오류",
            foundVersion = "0.00.03",
            status = "수정완료",
            appliedVersion = "0.00.04",
            description = "기본 건(Gun.cs)에서 총알이 캐릭터 중심 위치(0,0,0)에서 생성되어 발사 직후 콜라이더와 부딪히거나 삭제되는 문제",
            cause = "총구 위치가 아닌 무기 원점에서 생성되어 캐릭터 내부 콜라이더에 즉시 닿음",
            solution = "Gun.cs에 FirePoint(총구) 트랜스폼 및 GetSpawnPosition(dir) 오프셋을 구현하여 충돌하지 않는 총구 위치에서 탄환이 생성되도록 수정"
        },
        new TroubleshootingEntry()
        {
            type = "오류",
            foundVersion = "0.00.04",
            status = "테스트요청",
            appliedVersion = "0.00.05",
            description = "블록건에서 높은 위치의 적(독수리 등)이나 주변 적 위치로 탄착점 지정 및 조준이 정상 작동하지 않던 문제",
            cause = "도달시간(Time) 기반 역탄도 공식 부재 및 가장 가까운 적(targetPos) 실시간 탐색/갱신 누락",
            solution = "가장 가까운 적 위치로 targetPos를 실시간 갱신하고, 슬라이드 역탄도 공식[H(vy) = (vDist.y / Time) + (G/2 * Time), vx = vDist.x / Time]을 적용"
        }
    ,
        new TroubleshootingEntry()
        {
            type = "오류",
            foundVersion = "0.00.05",
            status = "수정완료",
            appliedVersion = "0.00.06",
            description = "블록건의 기본 탄착점이 지정되지 않아 타겟 부재 시 임의 위치로 발사되던 문제",
            cause = "DefultTarget 트랜스폼 연동 누락 및 적 탐색 상태(발견/이탈)에 따른 동적 탄착점 전환 로직 부재",
            solution = "DefultTarget을 기본 탄착점으로 자동 바인딩하고, 적 부재 시 DefultTarget 위치로 발사, 적 발견 시 해당 적 위치로 조준, 시야 이탈 시 다시 DefultTarget으로 복귀하도록 구현"
        }
    ,
        new TroubleshootingEntry()
        {
            type = "오류",
            foundVersion = "0.00.06",
            status = "수정완료",
            appliedVersion = "0.00.07",
            description = "라이트닝 실드(낙뢰)가 발사되지 않거나 주변 몬스터 부재 시 스킬이 반응하지 않는 문제",
            cause = "주변 반경 내 몬스터 부재 시 발사 취소(return) 처리, 좁은 탐색 반경(5f), 수동 발사 시 실시간 타겟 미탐색 및 LightningBullet의 monsterLayer 초기화 누락",
            solution = "발사 시 실시간 타겟 탐색 적용, 몬스터 부재 시 전방 총구 위치에 Fallback 1회 낙뢰 생성, 탐색 반경 10f 상향 및 monsterLayer 자동 할당 방어 코드 추가"
        }
    };

    public string GetVersionString()
    {
        return $"{majorVersion}.{releaseVersion:D2}.{patchVersion:D2}";
    }

    // Update is called once per frame
    void Update()
    {
        CameraTrackingTargetPlayerProcess();
        EaglePointSetting();
        UpdateGUIStatus();

        if (responnerPlayer != null && responnerPlayer.objPlayer != null)
        {
            if (guiPlayerInfo != null)
            {
                Player playerComp = responnerPlayer.objPlayer.GetComponent<Player>();
                if (playerComp != null)
                {
                    guiPlayerInfo.Set(playerComp);
                }
            }
        }
    }

    private void OnGUI()
    {
        if (!showVersionGUI) return;

        // 1. 우측 상단 버전 표시 박스
        int width = 220;
        int height = 55;
        int padding = 10;
        Rect versionRect = new Rect(Screen.width - width - padding, padding, width, height);

        GUI.color = Color.white;
        GUI.Box(versionRect, $"[VSLike Build Ver]\n<b>v{GetVersionString()}</b>");

        // 트러블슈팅 목록 토글 버튼
        Rect toggleBtnRect = new Rect(Screen.width - width - padding + 10, padding + 28, width - 20, 20);
        if (GUI.Button(toggleBtnRect, showTroubleshootingList ? "트러블슈팅 닫기 (F1)" : "트러블슈팅 보기 (F1)"))
        {
            showTroubleshootingList = !showTroubleshootingList;
        }

        // F1 키로도 토글 가능
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F1)
        {
            showTroubleshootingList = !showTroubleshootingList;
        }

        // 2. 트러블슈팅 세부 내역 팝업 박스
        if (showTroubleshootingList)
        {
            int listWidth = 520;
            int listHeight = 360;
            Rect listRect = new Rect(Screen.width - listWidth - padding, padding + height + 5, listWidth, listHeight);

            GUILayout.BeginArea(listRect, "<b>[트러블슈팅 수정 내역]</b>", GUI.skin.window);
            GUILayout.Space(20);

            for (int i = 0; i < troubleshootingList.Count; i++)
            {
                var item = troubleshootingList[i];
                string statusColor = item.status == "완료" ? "lime" : (item.status == "수정중" ? "yellow" : "orange");

                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label($"<b>#{i + 1} [{item.type}]</b> ({item.foundVersion} -> <color=cyan>{item.appliedVersion}</color>) | 상태: <color={statusColor}><b>{item.status}</b></color>");
                GUILayout.Label($"<b>현상:</b> {item.description}");
                if (!string.IsNullOrEmpty(item.cause))
                    GUILayout.Label($"<b>원인:</b> {item.cause}");
                if (!string.IsNullOrEmpty(item.solution))
                    GUILayout.Label($"<b>해결:</b> {item.solution}");
                GUILayout.EndVertical();
                GUILayout.Space(5);
            }

            GUILayout.EndArea();
        }
    }
}