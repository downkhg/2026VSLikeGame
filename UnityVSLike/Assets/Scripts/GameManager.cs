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
        guiInventory.SetInventory(monsterInventory);
        SetGUIStatus(curGUIStatus);
    }

    public void ShowGUIScence(int idx)
    {
        for(int i = 0; i < listGUIScence.Count; i++)
        {
            if (i == idx) listGUIScence[i].SetActive(true);
            else listGUIScence[i].SetActive(false);
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
        if (active)
            guiInventory.SetInventory(monsterInventory);
        else
            guiInventory.CloseIventory();
        
        objPopupLayer.SetActive(active);
    }

    public void EventInventoryInput()
    {
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
        if(responnerPlayer.objPlayer == null)
        {
            SetGUIStatus(E_GUI_STATUS.GAMEOVER);
        }
    }

    public void EventShowMeTheItem()
    {
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
        if(mainCameraTracker.objTarget == null)
        {
            if(responnerPlayer.objPlayer != null)
                mainCameraTracker.objTarget = responnerPlayer.objPlayer;
        }
    }

    void EaglePointSetting()
    {
        if(responnerEagle.objPlayer)
        {
            Eagle eagle = responnerEagle.objPlayer.GetComponent<Eagle>();

            if (eagle.objPatrolPoint == null && eagle.objResponPoint == null)
            {
                eagle.objPatrolPoint = responnerOpossum.gameObject;
                eagle.objResponPoint = responnerEagle.gameObject;
                eagle.SetAIState(Eagle.E_AI_STATE.RETRUN);
            }
        }
    }

    public GUIPlayerInfo guiPlayerInfo;

    // Update is called once per frame
    void Update()
    {
        CameraTrackingTargetPlayerProcess();
        EaglePointSetting();
        UpdateGUIStatus();
        if(responnerPlayer.objPlayer)
            guiPlayerInfo.Set(responnerPlayer.objPlayer.GetComponent<Player>());
    }
}
