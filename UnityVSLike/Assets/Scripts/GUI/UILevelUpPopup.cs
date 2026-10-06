using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 레벨업 시 3개의 아이템/총기 선택지를 제공하는 UGUI 팝업 윈도우
/// </summary>
public class UILevelUpPopup : MonoBehaviour
{
    public Text textTitle;
    public Transform cardContainer;
    public GameObject cardPrefab;
    public List<UILevelUpCard> fixedCardSlots;

    private List<UILevelUpCard> spawnedCards = new List<UILevelUpCard>();
    private Action<ItemInfo> onItemSelectedCallback;

    public void Open(List<ItemInfo> choices, Action<ItemInfo> onSelected)
    {
        onItemSelectedCallback = onSelected;
        gameObject.SetActive(true);

        if (textTitle != null)
        {
            textTitle.text = "★ LEVEL UP! 새로운 총기를 선택하세요 ★";
        }

        // 고정 슬롯이 등록되어 있는 경우
        if (fixedCardSlots != null && fixedCardSlots.Count > 0)
        {
            for (int i = 0; i < fixedCardSlots.Count; i++)
            {
                if (i < choices.Count)
                {
                    fixedCardSlots[i].Setup(choices[i], OnCardClicked);
                }
                else
                {
                    fixedCardSlots[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            // 동적 생성 방식
            ClearSpawnedCards();
            if (cardPrefab != null && cardContainer != null)
            {
                for (int i = 0; i < choices.Count; i++)
                {
                    GameObject cardGo = Instantiate(cardPrefab, cardContainer);
                    UILevelUpCard cardComp = cardGo.GetComponent<UILevelUpCard>();
                    if (cardComp != null)
                    {
                        cardComp.Setup(choices[i], OnCardClicked);
                        spawnedCards.Add(cardComp);
                    }
                }
            }
        }
    }

    private void OnCardClicked(ItemInfo item)
    {
        onItemSelectedCallback?.Invoke(item);
    }

    public void Close()
    {
        ClearSpawnedCards();
        gameObject.SetActive(false);
    }

    private void ClearSpawnedCards()
    {
        for (int i = 0; i < spawnedCards.Count; i++)
        {
            if (spawnedCards[i] != null)
            {
                Destroy(spawnedCards[i].gameObject);
            }
        }
        spawnedCards.Clear();
    }
}
