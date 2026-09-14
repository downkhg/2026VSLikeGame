using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("카드 덱 구성 (에디터 지정)")]
    [SerializeField] private List<CardData> initialDeck = new List<CardData>();

    [Header("덱 설정")]
    [SerializeField] private int maxHandSize = 5;

    // 현재 카드 상태 리스트
    private List<CardData> drawPile = new List<CardData>();
    private List<CardData> hand = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();

    // UI 및 외부에 상태 변경을 알리는 이벤트
    public event Action OnHandChanged;
    public event Action OnDeckCountChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        InitializeDeck();
    }

    /// <summary>
    /// 게임 시작 시 초기 덱 설정 및 셔플
    /// </summary>
    public void InitializeDeck()
    {
        drawPile.Clear();
        hand.Clear();
        discardPile.Clear();

        drawPile.AddRange(initialDeck);
        ShuffleDrawPile();

        OnDeckCountChanged?.Invoke();
    }

    /// <summary>
    /// 덱에서 카드를 1장 뽑아 손패로 가져옴
    /// </summary>
    public bool DrawCard()
    {
        if (hand.Count >= maxHandSize)
        {
            Debug.Log("손패가 가득 차서 카드를 뽑을 수 없습니다.");
            return false;
        }

        // Draw Pile이 비어있으면 Discard Pile을 다시 셔플
        if (drawPile.Count == 0)
        {
            if (discardPile.Count == 0)
            {
                Debug.Log("덱과 버린 카드 더미가 모두 비어있습니다.");
                return false;
            }
            ReshuffleDiscardIntoDraw();
        }

        CardData drawnCard = drawPile[0];
        drawPile.RemoveAt(0);
        hand.Add(drawnCard);

        OnHandChanged?.Invoke();
        OnDeckCountChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 손패의 인덱스에 위치한 카드를 사용
    /// </summary>
    public void UseCard(int handIndex)
    {
        if (handIndex < 0 || handIndex >= hand.Count) return;

        CardData cardToUse = hand[handIndex];

        // TODO: 3단계에서 구현할 WeaponManager/Player와 연동하여 카드 효과 발동
        Debug.Log($"카드 사용: {cardToUse.cardName}");

        // 사용한 카드를 손패에서 제거 후 버린 카드 더미로 이동
        hand.RemoveAt(handIndex);
        discardPile.Add(cardToUse);

        OnHandChanged?.Invoke();
        OnDeckCountChanged?.Invoke();
    }

    /// <summary>
    /// Fisher-Yates 알고리즘 기반 Draw Pile 셔플
    /// </summary>
    private void ShuffleDrawPile()
    {
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            CardData temp = drawPile[i];
            drawPile[i] = drawPile[randomIndex];
            drawPile[randomIndex] = temp;
        }
    }

    /// <summary>
    /// 버린 카드 더미를 Draw Pile로 되돌리고 셔플
    /// </summary>
    private void ReshuffleDiscardIntoDraw()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDrawPile();
        Debug.Log("버린 카드 더미를 다시 섞어 덱으로 만들었습니다.");
    }

    // Getters (UI 표시용)
    public IReadOnlyList<CardData> GetHand() => hand.AsReadOnly();
    public int DrawPileCount => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;
}