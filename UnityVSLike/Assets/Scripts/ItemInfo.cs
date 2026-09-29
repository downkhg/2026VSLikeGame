using System;
using UnityEngine;

[System.Serializable]
public class ItemInfo
{
    public int ID;
    public string Name;
    public string Description;
    public GunType GunType;
    public int Score;
    public string PrefabPath;

    public ItemInfo()
    {
    }

    public ItemInfo(int id, string name, string description, GunType gunType, int score, string prefabPath)
    {
        this.ID = id;
        this.Name = name;
        this.Description = description;
        this.GunType = gunType;
        this.Score = score;
        this.PrefabPath = prefabPath;
    }

    /// <summary>
    /// 플레이어에게 아이템 효과 적용 (점수 추가 및 GunInventory에 총기/실드 추가)
    /// </summary>
    public void Use(Dynamic dynamic)
    {
        if (dynamic == null) return;

        // 1. 점수 증가
        dynamic.Score += Score;

        // 2. GunInventory 탐색 및 총기/실드 추가
        GunInventory gunInv = dynamic.GetComponent<GunInventory>();
        if (gunInv == null) gunInv = dynamic.GetComponentInParent<GunInventory>();
        if (gunInv == null) gunInv = dynamic.GetComponentInChildren<GunInventory>();

        if (gunInv != null)
        {
            gunInv.AddGun(this);

            // 만약 Dynamic에 설정된 액티브 무기가 없다면, 새로 추가된 총기를 액티브로 등록
            if (dynamic.gun == null)
            {
                BaseGun newGun = gunInv.GetGun(GunType);
                if (newGun != null)
                {
                    dynamic.SetActiveGun(newGun);
                }
            }
        }

        Debug.Log($"[ItemInfo] 아이템 사용됨: {Name} (ID: {ID}) -> 무기: {GunType}, 획득점수: +{Score}");
    }
}
