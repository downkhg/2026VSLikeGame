using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(gameObject.name+ ":OnTriggerEnter2D:"+collision.gameObject.name);
        if(collision.tag == "Player")
        {
            GameManager.GetInstacne().SetGUIStatus(GameManager.E_GUI_STATUS.THEEND);
        }
    }
}
