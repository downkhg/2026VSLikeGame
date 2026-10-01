using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterInventory : MonoBehaviour
{
    public List<string> listMonsters;


    public void AddMonster(string name)
    {
        listMonsters.Add(name);
        LegacyGUI.SetMonsterList(listMonsters);
    }

    public void RemoveMonster(string name)
    {
        listMonsters.Remove(name);
        LegacyGUI.SetMonsterList(listMonsters);
    }
}
