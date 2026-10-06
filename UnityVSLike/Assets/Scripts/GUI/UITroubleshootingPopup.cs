using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// F1 또는 버튼 클릭 시 트러블슈팅 수정 내역을 스크롤 뷰로 보여주는 UGUI 팝업 윈도우
/// </summary>
public class UITroubleshootingPopup : MonoBehaviour
{
    public Text textTitle;
    public Transform contentContainer;
    public GameObject entryPrefab;
    public Button buttonClose;

    private List<GameObject> spawnedEntries = new List<GameObject>();

    private void Awake()
    {
        if (buttonClose != null)
        {
            buttonClose.onClick.AddListener(Close);
        }
    }

    public void Open(List<GameManager.TroubleshootingEntry> entries)
    {
        gameObject.SetActive(true);

        if (textTitle != null)
        {
            textTitle.text = "<b>[트러블슈팅 수정 내역]</b>";
        }

        ClearEntries();

        if (entries != null && entryPrefab != null && contentContainer != null)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                GameObject obj = Instantiate(entryPrefab, contentContainer);
                UITroubleshootingEntry comp = obj.GetComponent<UITroubleshootingEntry>();
                if (comp != null)
                {
                    comp.Setup(i, entries[i]);
                }
                spawnedEntries.Add(obj);
            }
        }
    }

    public void Close()
    {
        ClearEntries();
        gameObject.SetActive(false);

        // 상위 PopupLayer가 관리 중이라면 닫기 상태 동기화
        GUIPopupLayer popupLayer = GetComponentInParent<GUIPopupLayer>();
        if (popupLayer != null && popupLayer.curPopupState == E_POPUP_STATE.TROUBLESHOOTING)
        {
            popupLayer.CloseCurrentPopup();
        }
    }

    private void ClearEntries()
    {
        for (int i = 0; i < spawnedEntries.Count; i++)
        {
            if (spawnedEntries[i] != null)
            {
                Destroy(spawnedEntries[i]);
            }
        }
        spawnedEntries.Clear();
    }
}
