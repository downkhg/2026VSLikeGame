using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject prefabBullet;
    public float ShotPower = 10f;

    public void Shot(Vector3 dir, Player master)
    {
        if (prefabBullet == null) return;

        GameObject copyBullet = Instantiate(prefabBullet, transform.position, Quaternion.identity);
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(dir, master, ShotPower);
        }
    }
}
