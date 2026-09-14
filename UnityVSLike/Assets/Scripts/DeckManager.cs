using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("초기 덱 설정")]
    [SerializeField] private List<CardData> initialDeck = new List<CardData>();
    [SerializeField] private int maxHandSize = 5;

    private List<CardData> drawPile = new List<CardData>();
    private List<CardData> hand = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ResetDeck();
    }

    public void ResetDeck()
    {
        drawPile.Clear();
        hand.Clear();
        discardPile.Clear();

        drawPile.AddRange(initialDeck);
        ShuffleDrawPile();
        Debug.Log("[DeckManager] 덱 초기화 완료");
    }

    public bool DrawCard()
    {
        if (hand.Count >= maxHandSize)
        {
            Debug.Log("[DeckManager] 손패가 가득 찼습니다.");
            return false;
        }

        if (drawPile.Count == 0)
        {
            if (discardPile.Count == 0)
            {
                Debug.Log("[DeckManager] 남은 카드가 없습니다.");
                return false;
            }
            ReshuffleDiscardToDraw();
        }

        CardData drawnCard = drawPile[0];
        drawPile.RemoveAt(0);
        hand.Add(drawnCard);
        return true;
    }

    public void UseCard(int handIndex)
    {
        if (handIndex < 0 || handIndex >= hand.Count) return;

        CardData cardToUse = hand[handIndex];

        // 1단계에서 작성한 WeaponManager 호출하여 실제 무기 스크립트/오브젝트 동작
        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.ExecuteCardEffect(cardToUse);
        }

        hand.RemoveAt(handIndex);
        discardPile.Add(cardToUse);
        Debug.Log($"[DeckManager] {cardToUse.cardName} 사용완료 -> 버린 카드 더미로 이동");
    }

    private void ShuffleDrawPile()
    {
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int rand = UnityEngine.Random.Range(0, i + 1);
            var temp = drawPile[i];
            drawPile[i] = drawPile[rand];
            drawPile[rand] = temp;
        }
    }

    private void ReshuffleDiscardToDraw()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDrawPile();
        Debug.Log("[DeckManager] 버린 카드 더미를 섞어 Draw Pile로 이동했습니다.");
    }

    public List<CardData> GetHand() => hand;
    public int DrawPileCount => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;
}