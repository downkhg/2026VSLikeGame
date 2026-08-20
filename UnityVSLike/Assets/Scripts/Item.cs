using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public int Score;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Dynamic dynamic = collision.GetComponent<Dynamic>();

        if(dynamic)
        {
            dynamic.Score += Score;
            Destroy(this.gameObject);
        }
    }
}
