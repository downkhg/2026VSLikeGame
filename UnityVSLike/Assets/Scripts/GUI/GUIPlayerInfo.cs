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
        if (player == null) return;
        if (textName != null) textName.text = player.gameObject.name;
        if (textLv != null) textLv.text = string.Format("Lv.{0}", player.Lv);
        if (guiHPBar != null) guiHPBar.SetBarSize(player.hp, player.hpMax);
    }
}
