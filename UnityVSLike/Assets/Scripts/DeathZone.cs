using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(collision.gameObject);
        //Debug.Log("OnTriggerEnter2D:"+collision.gameObject.name);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(collision.gameObject);
        //Debug.Log("OnCollisionEnter2D:" + collision.gameObject.name);
    }
}
