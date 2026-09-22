using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("아이템 데이터 ID (0이면 아래 gunType 직접 사용)")]
    public int itemID = 0;

    [Header("아이템 획득 시 점수 (CSV 데이터 없을 시 fallback)")]
    public int Score = 100;

    [Header("변경할 무기 타입 (TotalGun)")]
    public TotalGun.GunType gunType = TotalGun.GunType.KunaiGun;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Dynamic dynamic = collision.GetComponent<Dynamic>();

        if (dynamic != null)
        {
            Use(dynamic);
        }
    }

    /// <summary>
    /// 아이템 효과 사용 처리 (ItemInfoManager 조회 또는 직접 설정값 사용)
    /// </summary>
    public void Use(Dynamic dynamic)
    {
        if (dynamic == null) return;

        ItemInfo info = null;

        // 1. ItemInfoManager를 통한 정보 조회 시도
        if (itemID > 0)
        {
            info = ItemInfoManager.Instance.GetItemInfo(itemID);
        }
        else
        {
            info = ItemInfoManager.Instance.GetItemInfo(gunType);
        }

        // 2. 정보가 존재하면 ItemInfo.Use() 호출
        if (info != null)
        {
            info.Use(dynamic);
        }
        else
        {
            // fallback 직접 적용
            dynamic.Score += Score;

            if (dynamic.gun != null)
            {
                dynamic.gun.SetGunType(gunType);
            }
            else
            {
                TotalGun totalGun = dynamic.GetComponentInChildren<TotalGun>();
                if (totalGun != null)
                {
                    totalGun.SetGunType(gunType);
                }
            }

            Debug.Log($"[Item] Fallback 아이템 획득! GunType: {gunType}, 점수 +{Score}");
        }

        // 3. 아이템 오브젝트 제거
        Destroy(this.gameObject);
    }
}


