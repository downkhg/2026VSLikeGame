using System.Collections.Generic;
using UnityEngine;

public class GUIManager : MonoBehaviour
{
    public GameObject objPopupLayer;
    public GUIInventory guiInventory;


    public enum E_GUI_STATUS { TITILE, GAMEOVER, THEEND, PLAY }
    public List<GameObject> listGUIScence;
    public E_GUI_STATUS curGUIStatus;

    private void Start()
    {
        

        SetGUIStatus(curGUIStatus);
    }

    GameManager gameManager;

    public void Initialize(GameManager _gameManager)
    {
        _gameManager.EventShowMeTheItem();

        if (guiInventory != null)
            guiInventory.SetInventory(_gameManager.monsterInventory);

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
                if (gameManager != null)
                {
                    SetGUIStatus(E_GUI_STATUS.GAMEOVER);
                    gameManager.EventGameOverProcess();
                }
                EventInventoryInput();
                break;
        }
    }

    public void PopupLayerShow(bool active)
    {
        if (guiInventory != null)
        {
            if (active)
                guiInventory.SetInventory(gameManager.monsterInventory);
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
        Debug.Log("GUIManager.EventExit()");
        Application.Quit();
    }
}
