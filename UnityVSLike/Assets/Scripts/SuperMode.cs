using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperMode : MonoBehaviour
{
    public bool isUse;
    public float Time;

    IEnumerator ProcessTimmer()
    {
        isUse = true;
        yield return new WaitForSeconds(Time);
        isUse = false;
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    public void OnMode()
    {
        StartCoroutine(ProcessTimmer());
    }

    // Start is called before the first frame update
    void Start()
    {
        if (isUse) OnMode();
    }

    // Update is called once per frame
    void Update()
    {
        if(isUse)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            Color color = spriteRenderer.color;
            if (color.a == 1) color.a = 0;
            else color.a = 1;
            spriteRenderer.color = color;
        }
    }
}
