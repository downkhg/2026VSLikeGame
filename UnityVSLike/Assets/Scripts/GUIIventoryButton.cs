using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIIventoryButton : MonoBehaviour
{
    public Text textName;
    public Image imgMonster;

    public void Set(string name)
    {
        textName.text = name;
        imgMonster.sprite = Resources.Load<Sprite>("GUI/"+name);
    }
}
