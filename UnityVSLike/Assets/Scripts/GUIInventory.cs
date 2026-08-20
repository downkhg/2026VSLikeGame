using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIInventory : MonoBehaviour
{
    public List<GUIIventoryButton> listGuiIventoryButtons;
    public GridLayoutGroup gridLayoutGroupContent;

    public void SetInventory(MonsterInventory monsterInventory)
    {
        //버튼의 프리팹을 로드한다.
        GameObject prefabsButton = Resources.Load("GUI/IventoryButton") as GameObject;
        //로드한 버튼을 동적생성하고, 버튼을 인벤토리에 저장된 정보에 맞게 설정하고, 리스트에 추가한다.
        foreach (var monster in monsterInventory.listMonsters)
        {
            GameObject objButton =
                Instantiate(prefabsButton, gridLayoutGroupContent.transform);
            GUIIventoryButton guiIventoryButon =
                objButton.GetComponent<GUIIventoryButton>();
            guiIventoryButon.Set(monster);
            Debug.Log("GUIIventoryButton:" + monster);
            listGuiIventoryButtons.Add(guiIventoryButon);
        }
        //리스트에 추가한 인벤토리라인수에 맞춰서 컨텐츠영역의 크기를 조절한다.
        int size = monsterInventory.listMonsters.Count;
        Vector2 vCellSize = gridLayoutGroupContent.cellSize;
        RectTransform rectTransformContent = gridLayoutGroupContent.GetComponent<RectTransform>();
        Vector2 vContentSize = rectTransformContent.sizeDelta;
        int nRow = (int)(vContentSize.x / vCellSize.x);
        int nCol = size / nRow;
        if (size % nRow > 0) nCol++;
        vContentSize.y = vCellSize.y * nCol;
        rectTransformContent.sizeDelta = vContentSize;
    }

    public void CloseIventory()
    {
        foreach (var button in listGuiIventoryButtons)
        {
            Destroy(button.gameObject);
        }
        listGuiIventoryButtons.Clear();
    }
}
