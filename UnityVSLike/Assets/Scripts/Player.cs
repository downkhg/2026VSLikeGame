using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int hp = 100;
    public int atk = 10;
    public int hpMax;
    public int Lv;

    private void Awake()
    {
        hpMax = hp;
    }

    private void Update()
    {
        if (Death())
        {
            Destroy(this.gameObject);
        }
    }

    public void Attack(Player target)
    {
        //target.hp -= atk;
        target.hp = target.hp - atk;
    }

    public bool Death()
    {
        if (hp <= 0)
            return true;
        else
            return false;
    }
    //public int idx = 0;
    //private void OnGUI()
    //{
    //    int w = 100;
    //    int h = 60;
    //    Rect rect = new Rect(0, h * idx, w, h);
    //    GUI.Box(rect, "Name:"+gameObject.name+"\nHP:"+ hp + "\nATK:"+atk);
    //}
}
