using UnityEngine;

public class DeckManagerTester : MonoBehaviour
{
    private DeckManager deckManager;

    private void Start()
    {
        deckManager = DeckManager.Instance;
        if (deckManager == null)
        {
            deckManager = FindFirstObjectByType<DeckManager>();
        }
    }

    private void OnGUI()
    {
        if (deckManager == null)
        {
            GUI.Label(new Rect(10, 10, 300, 20), "Error: DeckManager를 찾을 수 없습니다.");
            return;
        }

        // 1. 덱 상태 요약 정보 박스
        GUILayout.BeginArea(new Rect(10, 10, 250, 400), "=== 덱 관리자 테스트 ===", GUI.skin.window);
        
        GUILayout.Label($"남은 덱 (Draw Pile): {deckManager.DrawPileCount}장");
        GUILayout.Label($"버린 카드 (Discard): {deckManager.DiscardPileCount}장");
        
        GUILayout.Space(10);

        // 카드 1장 수동 드로우 버튼
        if (GUILayout.Button("카드 1장 뽑기 (Draw)", GUILayout.Height(30)))
        {
            deckManager.DrawCard();
        }

        // 초기화 버튼
        if (GUILayout.Button("덱 초기화 (Reset)", GUILayout.Height(25)))
        {
            deckManager.InitializeDeck();
        }

        GUILayout.EndArea();

        // 2. 현재 손패(Hand) 목록 및 사용 버튼
        var hand = deckManager.GetHand();
        GUILayout.BeginArea(new Rect(270, 10, 450, 500), $"=== 현재 손패 ({hand.Count}장) ===", GUI.skin.window);

        if (hand.Count == 0)
        {
            GUILayout.Label("손패가 비어있습니다.");
        }
        else
        {
            for (int i = 0; i < hand.Count; i++)
            {
                CardData card = hand[i];
                
                GUILayout.BeginHorizontal("box");
                
                // 카드 정보 요약 출력
                string cardInfo = $"[{i}] {card.cardName} (비용:{card.manaCost})\n" +
                                 $"타입: {card.cardType} / 무기: {card.targetWeapon}";
                
                GUILayout.Label(cardInfo, GUILayout.Width(300));

                // 카드 사용 트리거 버튼
                if (GUILayout.Button("사용 (Use)", GUILayout.Width(100), GUILayout.Height(35)))
                {
                    deckManager.UseCard(i);
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
        }

        GUILayout.EndArea();
    }
}