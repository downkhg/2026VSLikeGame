using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject prefabBullet;
    public float ShotPower = 10f;

    [Header("총구 (Muzzle) 위치")]
    public Transform firePoint; // 총알이 실제 생성될 총구 위치 (비어있으면 자동 생성/보정)
    public Vector3 defaultMuzzleOffset = new Vector3(0.5f, 0f, 0f); // 기본 전방 오프셋

    [Header("발사 설정")]
    public float fireInterval = 0.5f; // 자동 발사 주기 (초)
    private float fireTimer = 0f;

    public Player master;

    private void Awake()
    {
        if (master == null)
        {
            master = GetComponentInParent<Player>();
        }

        InitFirePoint();
    }

    private void InitFirePoint()
    {
        if (firePoint == null)
        {
            Transform found = transform.Find("FirePoint");
            if (found != null)
            {
                firePoint = found;
            }
            else
            {
                GameObject muzzleObj = new GameObject("FirePoint");
                muzzleObj.transform.SetParent(transform);
                muzzleObj.transform.localPosition = defaultMuzzleOffset;
                firePoint = muzzleObj.transform;
            }
        }
    }

    public Vector3 GetSpawnPosition(Vector3 dir)
    {
        if (firePoint != null)
        {
            // 발사 방향에 맞춰 총구 오프셋 위치 동적 계산 (플레이어 뒤나 안쪽이 아닌 발사 방향 앞쪽)
            return transform.position + (dir.normalized * defaultMuzzleOffset.magnitude);
        }
        return transform.position + (dir.normalized * defaultMuzzleOffset.magnitude);
    }

    private void Update()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            Shot(GetPlayerFacingDirection(), master);
        }
    }

    private Vector2 GetPlayerFacingDirection()
    {
        if (master != null && master.transform.localScale.x < 0)
        {
            return Vector2.left;
        }

        if (Mathf.Abs(transform.right.x) > 0.01f || Mathf.Abs(transform.right.y) > 0.01f)
        {
            return transform.right;
        }

        return Vector2.right;
    }

    public void Shot(Vector3 dir, Player master)
    {
        if (prefabBullet == null) return;

        Vector3 spawnPos = GetSpawnPosition(dir);
        GameObject copyBullet = Instantiate(prefabBullet, spawnPos, Quaternion.identity);
        Bullet bullet = copyBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(dir, master, ShotPower);
        }
    }
}
