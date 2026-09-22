using System;
using UnityEngine;

[System.Serializable]
public class ItemInfo
{
    public int ID;
    public string Name;
    public string Description;
    public TotalGun.GunType GunType;
    public int Score;
    public string PrefabPath;

    public ItemInfo()
    {
    }

    public ItemInfo(int id, string name, string description, TotalGun.GunType gunType, int score, string prefabPath)
    {
        this.ID = id;
        this.Name = name;
        this.Description = description;
        this.GunType = gunType;
        this.Score = score;
        this.PrefabPath = prefabPath;
    }

    /// <summary>
    /// 플레이어에게 아이템 효과 적용 (점수 추가, TotalGun 기믹 변경 및 GunInventory에 총기/실드 추가)
    /// </summary>
    public void Use(Dynamic dynamic)
    {
        if (dynamic == null) return;

        // 1. 점수 증가
        dynamic.Score += Score;

        // 2. 무기 상태 변경
        if (dynamic.gun != null)
        {
            dynamic.gun.SetGunType(GunType);
        }
        else
        {
            TotalGun totalGun = dynamic.GetComponentInChildren<TotalGun>();
            if (totalGun != null)
            {
                totalGun.SetGunType(GunType);
            }
        }

        // 3. GunInventory가 있으면 인벤토리에도 총기/실드 추가
        GunInventory gunInv = dynamic.GetComponent<GunInventory>();
        if (gunInv != null)
        {
            gunInv.AddGun(this);
        }

        Debug.Log($"[ItemInfo] 아이템 사용됨: {Name} (ID: {ID}) -> 무기: {GunType}, 획득점수: +{Score}");
    }
}
