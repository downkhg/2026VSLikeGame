using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    public GameObject objPopupLayer;
    public GUIInventory guiInventory;
    public GUIPopupLayer popupLayer;

    public enum E_GUI_STATUS { TITILE, GAMEOVER, THEEND, PLAY }
    public List<GameObject> listGUIScence;
    public E_GUI_STATUS curGUIStatus;

    private void Awake()
    {
        Debug.Log("GUIManager.Awake()");
        if (popupLayer == null && objPopupLayer != null)
        {
            popupLayer = objPopupLayer.GetComponent<GUIPopupLayer>();
            if (popupLayer == null)
            {
                popupLayer = objPopupLayer.AddComponent<GUIPopupLayer>();
                popupLayer.guiInventory = guiInventory;
            }
        }

        if (listGUIScence == null || listGUIScence.Count == 0)
        {
            Debug.LogError("GUIManager: listGUIScence is not set or empty.");
            return;
        }
        ShowGUIScence(-1); // Hide all GUI scenes initially
    }

    private void Start()
    {
        Debug.Log("GUIManager.Start()");
        SetGUIStatus(curGUIStatus);
    }

    GameManager gameManager;

    public void Initialize(GameManager _gameManager)
    {
        _gameManager.EventShowMeTheItem();

        if (popupLayer != null)
        {
            popupLayer.Initialize(_gameManager);
        }
        else if (guiInventory != null)
        {
            guiInventory.SetInventory(_gameManager.monsterInventory);
        }

        this.gameManager = _gameManager;
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
        Debug.Log($@"GUIManager.SetGUIStatus(""{status.ToString()}"")");
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
                Time.timeScale = (popupLayer != null && popupLayer.IsPopupOpen()) ? 0 : 1;
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
                if (gameManager != null && gameManager.responnerPlayer != null && gameManager.responnerPlayer.objPlayer == null)
                {
                    SetGUIStatus(E_GUI_STATUS.GAMEOVER);
                    gameManager.EventGameOverProcess();
                }
                EventInventoryInput();
                EventGunStatusInput();
                EventTroubleshootingInput();
                break;
        }
    }

    public void PopupLayerShow(bool active)
    {
        Debug.Log($@"GUIManager.PopupLayerShow({active})");
        if (popupLayer != null)
        {
            if (active)
            {
                popupLayer.SetPopupState(E_POPUP_STATE.INVENTORY);
            }
            else
            {
                popupLayer.CloseCurrentPopup();
            }
        }
        else
        {
            if (guiInventory != null)
            {
                if (active)
                    guiInventory.SetInventory(gameManager?.monsterInventory);
                else
                    guiInventory.CloseIventory();
            }

            if (objPopupLayer != null)
                objPopupLayer.SetActive(active);
        }
    }

    public void EventInventoryInput()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (popupLayer != null)
            {
                if (popupLayer.curPopupState == E_POPUP_STATE.INVENTORY)
                {
                    popupLayer.CloseCurrentPopup();
                }
                else
                {
                    popupLayer.SetPopupState(E_POPUP_STATE.INVENTORY);
                }
            }
            else if (objPopupLayer != null)
            {
                PopupLayerShow(!objPopupLayer.activeSelf);
            }
        }
    }

    public void EventGunStatusInput()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("GUIManager: EventGunStatusInput() - G Key Pressed");
            if (popupLayer != null)
            {
                if (popupLayer.curPopupState == E_POPUP_STATE.GUN_STATUS)
                {
                    popupLayer.CloseCurrentPopup();
                }
                else
                {
                    popupLayer.SetPopupState(E_POPUP_STATE.GUN_STATUS);
                }
            }
            else if (objPopupLayer != null)
            {
                PopupLayerShow(!objPopupLayer.activeSelf);
            }
        }
    }

    public void EventTroubleshootingInput()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            // 레거시 GUI가 꺼져있는 경우 UGUI 팝업으로 표시
            if (gameManager != null && !gameManager.enableLegacyGUI)
            {
                if (popupLayer != null)
                {
                    if (popupLayer.curPopupState == E_POPUP_STATE.TROUBLESHOOTING)
                    {
                        popupLayer.CloseCurrentPopup();
                    }
                    else
                    {
                        popupLayer.SetPopupState(E_POPUP_STATE.TROUBLESHOOTING);
                    }
                }
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
        Debug.Log("GUIManager.EventExit()");
        Application.Quit();
    }

    public Button tilteEndButton;

    public void SetTitleButtonEvet()
    {
        if (tilteEndButton != null)
        {
            tilteEndButton.onClick.AddListener(() =>
            {
                Debug.Log("GUIInventory: Title Button Clicked");
                EventStart();
            });
        }
    }
}
