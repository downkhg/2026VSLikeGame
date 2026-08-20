using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//플레이어가 죽으면 살린다.
//플레이어가 죽었다?
public class Responner : MonoBehaviour
{
    public GameObject prefabPlayer;
    public GameObject objPlayer = null;
    public float Time = 1;
    public bool isRespon = false;

    IEnumerator ProcessTimer()
    {
        //Debug.Log("ProcessTimer 1");
        isRespon = true;
        yield return new WaitForSeconds(Time);
        objPlayer = Instantiate(prefabPlayer);
        objPlayer.name = prefabPlayer.name;
        objPlayer.transform.position = this.gameObject.transform.position;
        isRespon = false;
        //Debug.Log("ProcessTimer 2");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(objPlayer == null && isRespon == false)
        {
            StartCoroutine(ProcessTimer());
        }
    }
}
