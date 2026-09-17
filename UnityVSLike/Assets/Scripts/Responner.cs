using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 플레이어가 사망했을 때 재생성(리스폰)을 담당하는 스포너
public class Responner : MonoBehaviour
{
    public GameObject prefabPlayer;
    public GameObject objPlayer = null;
    public float Time = 1;
    public bool isRespon = false;

    IEnumerator ProcessTimer()
    {
        isRespon = true;
        yield return new WaitForSeconds(Time);
        objPlayer = Instantiate(prefabPlayer);
        objPlayer.name = prefabPlayer.name;
        objPlayer.transform.position = this.gameObject.transform.position;
        isRespon = false;
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (objPlayer == null && isRespon == false)
        {
            StartCoroutine(ProcessTimer());
        }
    }
}