using System;

// 1. 힙(Heap)에 할당되는 참조 형식 클래스
public class CharacterData
{
    public string Name { get; set; }
    public int Hp { get; set; }
    public int AttackPower { get; set; }

    public CharacterData(string name, int hp, int attackPower)
    {
        Name = name;
        Hp = hp;
        AttackPower = attackPower;
    }

    public void PrintInfo(string label)
    {
        Console.WriteLine($"[{label}] 이름: {Name}, HP: {Hp}, 공격력: {AttackPower}");
    }
}

public class Program
{
    public static void Main()
    {
        // warrior 변수는 실제 객체가 아니라 힙에 생성된 인스턴스를 가리키는 '참조(주소)'를 가집니다.
        CharacterData warrior = new CharacterData("전사", 100, 25);
        warrior.PrintInfo("초기 상태");

        // [동작 1] 별칭(Alias) - unsafe의 포인터 복사와 동일한 동작
        // warrior와 aliasHero는 힙 상의 동일한 객체 주소를 가리킵니다.
        CharacterData aliasHero = warrior;
        aliasHero.Hp = 150; // aliasHero를 바꿨지만...

        warrior.PrintInfo("aliasHero 수정 후 warrior"); // warrior의 HP도 150으로 반영됨

        // [동작 2] 메서드로 참조 전달 (Call by Reference)
        // 객체 내부의 값을 메서드 안에서 직접 갱신합니다.
        BuffCharacter(warrior, 10);
        warrior.PrintInfo("버프 적용 후");

        // [동작 3] ref 키워드를 통한 참조 변수 자체의 교체 (포인터의 이중 포인터 효과)
        CharacterData mage = new CharacterData("마법사", 60, 50);
        SwapCharacters(ref warrior, ref mage);

        Console.WriteLine("\n--- Swap 실행 후 ---");
        warrior.PrintInfo("현재 warrior 변수");
        mage.PrintInfo("현재 mage 변수");
    }

    // 객체 내부 상태를 수정: C++의 void Buff(Character* target)과 동일하게 동작
    public static void BuffCharacter(CharacterData target, int extraAttack)
    {
        target.AttackPower += extraAttack;
    }

    // 참조 변수 자체(포인터 값 자체)를 교환: C++의 void Swap(Character** a, Character** b) 역할
    public static void SwapCharacters(ref CharacterData a, ref CharacterData b)
    {
        CharacterData temp = a;
        a = b;
        b = temp;
    }
}