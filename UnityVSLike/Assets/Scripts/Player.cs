using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("=== 기본 스탯 ===")]
    public int hp = 100;
    public int atk = 10;
    public int hpMax;
    public int Lv = 1;

    [Header("=== 성장 및 경험치(EXP) ===")]
    public int exp = 0;
    public int expMax = 100;
    [Tooltip("해당 개체가 처치당했을 때 막타 공격자에게 주는 경험치")]
    public int expReward = 35;
    [Tooltip("해당 개체가 처치당했을 때 막타 공격자에게 주는 점수")]
    public int scoreReward = 100;

    [Header("=== 상태 플래그 ===")]
    public bool isDead = false;

    [Header("건 인벤토리 참조")]
    public GunInventory gunInventory;

    [Header("스테이터스 노출")]
    public GUIStatusBar guiStatusBarHp;

    private void Awake()
    {
        hpMax = hp;

        // GunInventory는 오직 Player 태그를 가진 플레이어에게만 연동/할당
        if (CompareTag("Player"))
        {
            if (gunInventory == null)
            {
                gunInventory = GetComponentInChildren<GunInventory>();
                if (gunInventory == null)
                {
                    gunInventory = GetComponent<GunInventory>();
                }
            }
        }
    }

    private void Update()
    {
        if (guiStatusBarHp)
            guiStatusBarHp.SetBarSize(hp, hpMax);

        if (Death() && !isDead)
        {
            isDead = true;
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// 대상에게 공격을 가합니다. 공격자 정보(this)를 전달하여 막타 판정에 사용됩니다.
    /// </summary>
    public void Attack(Player target)
    {
        Attack(target, atk);
    }

    /// <summary>
    /// 지정된 피해량으로 대상을 공격합니다.
    /// </summary>
    public void Attack(Player target, int customDamage)
    {
        if (target != null)
        {
            target.OnDamaged(customDamage > 0 ? customDamage : atk, this);
        }
    }

    /// <summary>
    /// 피격 처리 (HP 감소, 플로팅 대미지 출력 및 막타 처치자에게 보상 지급)
    /// </summary>
    /// <param name="damage">입는 피해량</param>
    /// <param name="attacker">피해를 가한 공격자 (막타 판정용)</param>
    public void OnDamaged(int damage, Player attacker = null)
    {
        if (isDead) return;

        // 1. 실제 체력 감소 적용
        hp = Mathf.Max(0, hp - damage);
        Debug.Log($"<color=red>[Player] '{gameObject.name}' 피격! -{damage} 데미지 (남은 HP: {hp}/{hpMax})</color>");

        // 2. 화면 GUI에 플로팅 대미지 텍스트 출력 (플레이어 피격: 빨간색, 몬스터 피격: 노란색)
        DamageTextManager.ShowDamage(transform.position, damage, CompareTag("Player"));

        // 3. 체력이 0 이하가 되어 사망 시: 막타(마지막 타격)를 친 공격자에게만 경험치 및 처치 보상 지급
        if (hp <= 0)
        {
            isDead = true;

            if (attacker != null)
            {
                // 막타 친 공격자에게 경험치 및 레벨업 판정 전달
                attacker.OnKillEnemy(this);
            }
            else
            {
                // 공격자 정보가 없는 환경에서의 fallback 인벤토리 등록
                if (GameManager.GetInstacne() != null && GameManager.GetInstacne().monsterInventory != null)
                {
                    GameManager.GetInstacne().monsterInventory.AddMonster(this.name);
                }
            }

            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// 레거시 하위 호환용 OnDamaged
    /// </summary>
    public void OnDamaged(int damage)
    {
        OnDamaged(damage, null);
    }

    /// <summary>
    /// 막타를 가해 적을 처치했을 때 호출되는 보상 메서드 (막타 친 사람에게만 실행됨)
    /// </summary>
    public void OnKillEnemy(Player victim)
    {
        int gainedExp = victim != null ? victim.expReward : 35;
        int gainedScore = victim != null ? victim.scoreReward : 100;

        // 1. 막타 공격자에게 경험치 지급 및 레벨업 체크
        AddExp(gainedExp);

        // 2. 막타 공격자에게 점수 가산 (Dynamic 컴포넌트 연동)
        Dynamic dynamic = GetComponent<Dynamic>();
        if (dynamic != null)
        {
            dynamic.Score += gainedScore;
        }

        // 3. 몬스터 인벤토리에 처치 기록 등록
        if (GameManager.GetInstacne() != null && GameManager.GetInstacne().monsterInventory != null && victim != null)
        {
            GameManager.GetInstacne().monsterInventory.AddMonster(victim.name);
        }

        Debug.Log($"<color=yellow>[Kill!] 🎯 {gameObject.name}이(가) {victim?.gameObject.name}을(를) 처치(막타)! EXP +{gainedExp}, Score +{gainedScore}</color>");
    }

    /// <summary>
    /// 경험치(EXP) 획득 및 필요 경험치 충족 시 자동 레벨업 루틴
    /// </summary>
    public void AddExp(int amount)
    {
        if (amount <= 0) return;

        exp += amount;
        Debug.Log($"<color=green>[Player ({gameObject.name})] EXP 획득: +{amount} (현재 EXP: {exp}/{expMax})</color>");

        while (exp >= expMax)
        {
            exp -= expMax;
            expMax = Mathf.RoundToInt(expMax * 1.5f); // 다음 레벨 요구 경험치 1.5배 증가
            LevelUp();
        }
    }

    /// <summary>
    /// 레벨업 처리 (레벨 상승 및 뱀서라이크식 3지선다 선택창 호출)
    /// </summary>
    public void LevelUp()
    {
        Lv++;
        Debug.Log($"<color=yellow>★ [Player] LEVEL UP! 현재 레벨: {Lv} (다음 레벨 필요 EXP: {expMax}) ★</color>");

        if (gunInventory != null)
        {
            gunInventory.ShowLevelUpSelection();
        }
    }

    public bool Death()
    {
        return hp <= 0 || isDead;
    }
}
