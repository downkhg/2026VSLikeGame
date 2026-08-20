using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject prefabBullet;
    public float ShotPower;

    public void Shot(Vector3 dir, Player master)
    {
        GameObject copyBullet = Instantiate(prefabBullet); 
        Rigidbody2D rigidbody = copyBullet.GetComponent<Rigidbody2D>();
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        bullet.master = master;
        rigidbody.AddForce(dir * ShotPower);
        copyBullet.transform.position = this.transform.position;
    }
}
