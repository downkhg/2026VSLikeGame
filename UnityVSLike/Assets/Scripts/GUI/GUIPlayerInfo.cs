using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIPlayerInfo : MonoBehaviour
{
    public Text textName;
    public Text textLv;

    public GUIStatusBar guiHPBar;

    public void Set(Player player)
    {
        textName.text = player.gameObject.name;
        textLv.text = string.Format("Lv.{0}", player.Lv);
        guiHPBar.SetBarSize(player.hp, player.hpMax);
    }
}
