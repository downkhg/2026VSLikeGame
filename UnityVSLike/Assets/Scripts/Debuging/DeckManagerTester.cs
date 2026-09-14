using UnityEngine;

public class DeckTesterOnGUI : MonoBehaviour
{
    private void OnGUI()
    {
        if (DeckManager.Instance == null)
        {
            GUI.Label(new Rect(10, 10, 300, 20), "DeckManager 인스턴스를 찾을 수 없습니다.");
            return;
        }

        // Left Panel: 덱 컨트롤 및 수치
        GUILayout.BeginArea(new Rect(10, 10, 220, 350), "덱 상태 컨트롤", GUI.skin.window);
        GUILayout.Label($"남은 덱 (Draw): {DeckManager.Instance.DrawPileCount}장");
        GUILayout.Label($"버린 카드 (Discard): {DeckManager.Instance.DiscardPileCount}장");
        GUILayout.Space(10);

        if (GUILayout.Button("카드 1장 뽑기 (Draw)", GUILayout.Height(30)))
        {
            DeckManager.Instance.DrawCard();
        }

        if (GUILayout.Button("덱 초기화 (Reset)", GUILayout.Height(25)))
        {
            DeckManager.Instance.ResetDeck();
        }
        GUILayout.EndArea();

        // Right Panel: 현재 손패 및 무기 발동 버튼
        var hand = DeckManager.Instance.GetHand();
        GUILayout.BeginArea(new Rect(240, 10, 400, 450), $"현재 손패 ({hand.Count}장)", GUI.skin.window);

        if (hand.Count == 0)
        {
            GUILayout.Label("손패가 비어 있습니다. [Draw] 버튼을 눌러보세요.");
        }
        else
        {
            for (int i = 0; i < hand.Count; i++)
            {
                var card = hand[i];
                GUILayout.BeginHorizontal("box");

                string info = $"[{i}] {card.cardName}\n타입:{card.cardType} | 목표:{card.targetWeapon}";
                GUILayout.Label(info, GUILayout.Width(260));

                if (GUILayout.Button("사용 (Use)", GUILayout.Width(90), GUILayout.Height(35)))
                {
                    DeckManager.Instance.UseCard(i);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
        }
        GUILayout.EndArea();
    }
}