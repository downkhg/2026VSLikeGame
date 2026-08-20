using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterInventory : MonoBehaviour
{
    public List<string> listMonsters;


    public void AddMonster(string name)
    {
        listMonsters.Add(name);
    }

    public void RemoveMonster(string name)
    {
        listMonsters.Remove(name);
    }

    private void OnGUI()
    {
        int w = 100; 
        int h = 20;
        for(int i = 0;  i < listMonsters.Count; i++)
        {
            GUI.Box(new Rect(0, h * i, w, h), listMonsters[i]);
        }
    }
}
