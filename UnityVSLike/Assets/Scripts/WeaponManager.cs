using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    [System.Serializable]
    public struct WeaponMapping
    {
        public WeaponType weaponType;
        public GameObject weaponObject; // player 하위의 실제 무기 GameObject
    }

    [Header("무기 오브젝트 매핑")]
    [SerializeField] private List<WeaponMapping> weaponMappings;

    private Dictionary<WeaponType, GameObject> weaponDict = new Dictionary<WeaponType, GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 딕셔너리 초기화 및 초기 비활성화
        foreach (var mapping in weaponMappings)
        {
            if (mapping.weaponObject != null && !weaponDict.ContainsKey(mapping.weaponType))
            {
                weaponDict.Add(mapping.weaponType, mapping.weaponObject);
                mapping.weaponObject.SetActive(false); // 기본 상태는 꺼둠
            }
        }
    }

    /// <summary>
    /// 카드 데이터 기반 무기 효과 실행
    /// </summary>
    public void ExecuteCardEffect(CardData card)
    {
        if (card == null) return;

        switch (card.cardType)
        {
            case CardType.DurationWeapon:
                StartCoroutine(Routine_ActivateWeaponDuration(card.targetWeapon, card.duration));
                break;

            case CardType.InstantAttack:
                TriggerInstantAttack(card.targetWeapon);
                break;

            case CardType.StatBuff:
                Debug.Log($"스탯 버프 발동: {card.value}");
                break;
        }
    }

    private IEnumerator Routine_ActivateWeaponDuration(WeaponType type, float duration)
    {
        if (weaponDict.TryGetValue(type, out GameObject weaponObj))
        {
            weaponObj.SetActive(true);
            Debug.Log($"[WeaponManager] {type} 무기 {duration}초간 활성화");

            yield return new WaitForSeconds(duration);

            weaponObj.SetActive(false);
            Debug.Log($"[WeaponManager] {type} 무기 비활성화");
        }
        else
        {
            Debug.LogWarning($"[WeaponManager] {type} 무기를 찾을 수 없습니다.");
        }
    }

    private void TriggerInstantAttack(WeaponType type)
    {
        if (weaponDict.TryGetValue(type, out GameObject weaponObj))
        {
            // 임시로 활성화하여 1회 공격 수행 후 비활성화하는 로직 예시
            weaponObj.SetActive(true);
            Debug.Log($"[WeaponManager] {type} 즉시 공격 발동!");
            // 단발 공격 후 바로 끄거나 무기 내부 스크립트의 Fire() 호출 가능
            Invoke(nameof(DisableAllInstantWeapons), 0.5f);
        }
    }

    private void DisableAllInstantWeapons()
    {
        // 필요 시 즉시 발동 무기 비활성화 처리
    }
}