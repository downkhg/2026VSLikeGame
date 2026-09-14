using UnityEngine;

public enum CardType
{
    DurationWeapon, // 일정 시간 동안 활성화되는 지속형 무기 (예: 야구방망이)
    InstantAttack,  // 사용 즉시 단발 발사/공격하는 무기 (예: 로켓, 번개)
    StatBuff        // 플레이어 스탯 버프
}

public enum WeaponType
{
    BaseballBat,
    SoccerGun,
    RocketLauncher,
    BlockGun,
    LightningShield
}

[CreateAssetMenu(fileName = "NewCardData", menuName = "Card System/Card Data")]
public class CardData : ScriptableObject
{
    [Header("카드 기본 정보")]
    public string cardName;
    public int manaCost = 1;
    [TextArea] public string description;

    [Header("카드 기능 및 연동")]
    public CardType cardType;
    public WeaponType targetWeapon;

    [Header("수치 정보")]
    public float duration = 5f;   // 지속형 무기 활성화 시간
    public float value = 10f;     // 데미지 또는 버프 수치
}