using UnityEngine;

public enum CardType
{
    DurationWeapon, // 일정 시간 동안 활성화되는 지속형 무기 (예: 야구방망이)
    InstantAttack,  // 사용 즉시 단발 발사/공격하는 무기 (예: 로켓, 번개)
    StatBuff        // 플레이어 스탯을 올려주는 버프 카드
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
    [Header("기본 카드 정보")]
    public string cardName;
    public Sprite cardIcon;
    [TextArea] public string description;
    public int manaCost = 1;

    [Header("카드 분류 및 연동")]
    public CardType cardType;
    public WeaponType targetWeapon;

    [Header("세부 효과 값")]
    public float duration = 5f;   // 지속형 무기의 활성화 시간
    public float value = 10f;     // 버프량 또는 추가 데미지
}