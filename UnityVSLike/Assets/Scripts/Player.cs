using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int hp = 100;
    public int atk = 10;
    public int hpMax;
    public int Lv = 1;

    [Header("건 인벤토리 참조")]
    public GunInventory gunInventory;

    private void Awake()
    {
        hpMax = hp;
        if (gunInventory == null)
        {
            gunInventory = GetComponent<GunInventory>();
            if (gunInventory == null)
            {
                gunInventory = gameObject.AddComponent<GunInventory>();
            }
        }
    }

    private void Update()
    {
        if (Death())
        {
            Destroy(this.gameObject);
        }
    }

    public void Attack(Player target)
    {
        if (target != null)
        {
            target.OnDamaged(atk);
        }
    }

    /// <summary>
    /// 플레이어 피격 처리 (총기가 있으면 실드로 1개 소모하여 피해 무효화, 없으면 HP 감소)
    /// </summary>
    public void OnDamaged(int damage)
    {
        // 1. 건 인벤토리에 총기가 존재하면 실드로 1개 파괴하고 피해 방어
        if (gunInventory != null && gunInventory.ConsumeShieldGun())
        {
            Debug.Log($"<color=cyan>[Player] 총기를 실드로 소모하여 {damage} 피해를 완벽히 막아냈습니다! (현재 HP: {hp}/{hpMax})</color>");
            return;
        }

        // 2. 총기가 없으면 실제 플레이어 HP 감소
        hp = Mathf.Max(0, hp - damage);
        Debug.Log($"<color=red>[Player] 플레이어 피격! -{damage} 데미지 (남은 HP: {hp}/{hpMax})</color>");
    }

    /// <summary>
    /// 레벨업 처리 (레벨 상승 및 뱀서라이크식 3지선다 선택창 호출)
    /// </summary>
    public void LevelUp()
    {
        Lv++;
        Debug.Log($"<color=yellow>[Player] 레벨업! 현재 레벨: {Lv}</color>");

        if (gunInventory != null)
        {
            gunInventory.ShowLevelUpSelection();
        }
    }

    public bool Death()
    {
        if (hp <= 0)
            return true;
        else
            return false;
    }
}

