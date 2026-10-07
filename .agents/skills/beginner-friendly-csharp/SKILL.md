---
name: beginner-friendly-csharp
description: >-
  초심자 수업 및 학습용 C# 코딩 가이드라인 스킬.
  복잡한 람다식, 고난도 LINQ 등을 지양하고 함수, 변수, 클래스, 상속, 델리게이트 등 기본 문법만을 활용하여 직관적이고 이해하기 쉬운 코드를 작성합니다.
---

# Beginner-Friendly C# Coding Guidelines Skill

이 스킬은 초심자 대상 프로그래밍 수업 및 강의 환경에 맞춰, C# 코드를 가장 이해하기 쉽고 직관적인 기초 문법으로 작성·리팩토링하기 위한 가이드라인을 정의합니다.

---

## 🎯 핵심 원칙 (Core Principles)

1. **기초 문법 중심 구현**:
   - **변수 (Variables)**: 명시적 자료형(`int`, `float`, `string`, `bool`, `GameObject` 등)을 우선 사용하며, `var` 남용을 피합니다.
   - **함수 (Functions / Methods)**: 하나의 함수는 하나의 명확한 역할을 수행하며, 명시적인 이름(`ShowLevelUp()`, `OnLevelUpItemSelected()`)을 부여합니다.
   - **클래스 (Classes)**: 멤버 변수, 초기화 메서드(`Initialize`), 동작 메서드를 분리하여 객체지향 기초 구조를 명확히 보여줍니다.
   - **상속 (Inheritance)**: 공통 기능을 부모 클래스(`BaseGun`, `MonoBehaviour`)로 추상화하고 `virtual`/`override`를 활용합니다.
   - **델리게이트 (Delegates & Callbacks)**: 콜백이 필요할 때는 명시적 델리게이트(`delegate void ...`) 또는 `Action`을 사용하되, 연결할 때는 **람다식이 아닌 명명된 함수(Named Method)**를 등록합니다.

---

## 🚫 지양해야 할 고급/축약 문법 (Avoid)

| 지양하는 문법 (Avoid) | 권장하는 기초 문법 (Recommended) | 이유 |
| :--- | :--- | :--- |
| **인라인 람다식**<br>`Open(choices, (item) => { ... });` | **명명 함수 전달**<br>`Open(choices, OnItemSelected);`<br>`private void OnItemSelected(ItemInfo item) { ... }` | 실행 흐름과 콜백 대상이 함수 단위로 명확히 분리되어 디버깅과 이해가 쉬움 |
| **복잡한 LINQ 체이닝**<br>`list.Where(x => x.hp > 0).Select(...)` | **`for` / `foreach` + `if`**<br>`foreach(var item in list) { if(item.hp > 0) ... }` | 반복문과 조건문의 동작 메커니즘을 학습자가 단계별로 추적 가능 |
| **암시적 타입 `var` 남용**<br>`var result = GetData();` | **명시적 타입 선언**<br>`MonsterInventory result = GetData();` | 코드만 보고도 어떤 객체/자료형인지 즉시 파악 가능 |
| **복잡한 삼항 연산자 중첩**<br>`a ? b : (c ? d : e)` | **명시적 `if - else` 블록**<br>`if (a) ... else if (c) ... else ...` | 분기 흐름을 한눈에 읽기 쉬움 |
| **지나친 연산자 체이닝 (`?.`, `??`)**<br>`a?.b?.c?.Do();` | **명시적 `if` 널 체크**<br>`if (a != null && a.b != null) { ... }` | 널 예외 발생 지점과 방어 로직의 흐름을 명확히 학습 가능 |
| **튜플 데이터 묶음 (`Tuple<T1, T2>`)**<br>`SetState(state, Tuple.Create(a, b));` | **전용 클래스/구조체 또는 멤버 변수 초기화**<br>`Initialize(a, b); SetState(state);` | 데이터의 의미와 역할이 변수명/메서드명으로 드러남 |

---

## 📋 코드 작성 체크리스트

1. [ ] 콜백 처리에 익명 람다식 `(x) => { ... }` 대신 별도의 `private void 함수명(인자)`을 선언했는가?
2. [ ] 상태 전환 함수(`SetState`)에 여러 타입의 오브젝트를 넘겨 다운캐스팅하는 대신, `Initialize(...)` 메서드로 데이터를 사전 세팅했는가?
3. [ ] 모든 변수와 매개변수의 타입이 명시적으로 작성되었는가?
4. [ ] 컬렉션 탐색 시 복잡한 LINQ 대신 명시적인 `foreach` / `for` 루프를 사용했는가?
5. [ ] 함수명이 그 동작을 직관적으로 설명하고 있는가?
