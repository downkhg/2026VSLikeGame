using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandUI : MonoBehaviour
{
    [Header("UI 레이아웃 연결")]
    [SerializeField] private Transform handContainer;  // 카드가 생성될 부모 Transform (Horizontal Layout Group 권장)
    [SerializeField] private GameObject cardPrefab;    // CardUI 컴포넌트가 붙어있는 프리팹

    [Header("덱 상태 텍스트 연결")]
    [SerializeField] private TextMeshProUGUI textDrawPileCount;
    [SerializeField] private TextMeshProUGUI textDiscardPileCount;

    [Header("드로우 수동 테스트 버튼 (선택사항)")]
    [SerializeField] private Button buttonDraw;

    private List<GameObject> activeCardUIList = new List<GameObject>();

    private void Start()
    {
        // DeckManager 이벤트 구독
        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.OnHandChanged += RefreshHandUI;
            DeckManager.Instance.OnDeckCountChanged += RefreshDeckCountUI;
        }

        if (buttonDraw != null)
        {
            buttonDraw.onClick.AddListener(() =>
            {
                DeckManager.Instance.DrawCard();
            });
        }

        // 초기 화면 갱신
        RefreshHandUI();
        RefreshDeckCountUI();
    }

    private void OnDestroy()
    {
        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.OnHandChanged -= RefreshHandUI;
            DeckManager.Instance.OnDeckCountChanged -= RefreshDeckCountUI;
        }
    }

    /// <summary>
    /// 손패 UI 전체 재구성
    /// </summary>
    private void RefreshHandUI()
    {
        // 기존 생성된 카드 UI 파괴
        foreach (var cardObj in activeCardUIList)
        {
            Destroy(cardObj);
        }
        activeCardUIList.Clear();

        // DeckManager에서 현재 손패 데이터 가져오기
        var currentHand = DeckManager.Instance.GetHand();

        for (int i = 0; i < currentHand.Count; i++)
        {
            GameObject newCardObj = Instantiate(cardPrefab, handContainer);
            CardUI cardUI = newCardObj.GetComponent<CardUI>();

            if (cardUI != null)
            {
                // 카드 UI 세팅 및 사용 시 사용 처리 콜백 전달
                cardUI.Setup(currentHand[i], i, OnRequestUseCard);
            }

            activeCardUIList.Add(newCardObj);
        }
    }

    /// <summary>
    /// 남은 덱/버린 카드 더미 개수 표시 갱신
    /// </summary>
    private void RefreshDeckCountUI()
    {
        if (textDrawPileCount != null)
            textDrawPileCount.text = $"덱: {DeckManager.Instance.DrawPileCount}";

        if (textDiscardPileCount != null)
            textDiscardPileCount.text = $"버린 카드: {DeckManager.Instance.DiscardPileCount}";
    }

    /// <summary>
    /// 카드가 클릭되어 사용 요청이 왔을 때 호출
    /// </summary>
    private void OnRequestUseCard(int handIndex)
    {
        DeckManager.Instance.UseCard(handIndex);
    }
}