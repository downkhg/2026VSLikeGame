using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Unity 에디터 메뉴 및 배치모드에서 Game 씬의 Canvas/PopupLayer 하위에 UGUI 레이아웃을 정확히 구성하고 씬을 저장하는 빌더 유틸리티
/// </summary>
public static class UGUILayoutBuilder
{
    [MenuItem("VSLike/Build UGUI Layout In Scene")]
    public static void BuildUGUILayoutInScene()
    {
        string scenePath = "Assets/Scenes/Game.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Canvas 탐색 (비활성 포함)
        Canvas canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null)
        {
            Debug.LogError("[UGUILayoutBuilder] Canvas를 찾을 수 없습니다.");
            return;
        }

        // 2. GUIManager 탐색
        GUIManager guiMgr = Object.FindFirstObjectByType<GUIManager>(FindObjectsInactive.Include);

        // 3. 씬 루트에 잘못 생성된 중복 PopupLayer가 있다면 정리
        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (var rootGo in rootObjects)
        {
            if (rootGo.name == "PopupLayer" && rootGo.transform.parent == null)
            {
                Debug.Log("[UGUILayoutBuilder] 루트에 존재하던 중복 PopupLayer 정리: " + rootGo.name);
                Object.DestroyImmediate(rootGo);
            }
        }

        // 4. Canvas 하위의 실제 PopupLayer 탐색
        Transform popupLayerTr = canvas.transform.Find("PopupLayer");
        GameObject popupLayerGo = null;
        if (popupLayerTr != null)
        {
            popupLayerGo = popupLayerTr.gameObject;
        }
        else if (guiMgr != null && guiMgr.objPopupLayer != null)
        {
            popupLayerGo = guiMgr.objPopupLayer;
        }

        if (popupLayerGo == null)
        {
            popupLayerGo = new GameObject("PopupLayer", typeof(RectTransform));
            popupLayerGo.transform.SetParent(canvas.transform, false);
            RectTransform rt = popupLayerGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
        }

        // 5. UI Layer(5) 설정
        SetLayerRecursively(popupLayerGo, 5);

        // 6. GUIPopupLayer 컴포넌트 부착/확인
        GUIPopupLayer popupLayer = popupLayerGo.GetComponent<GUIPopupLayer>();
        if (popupLayer == null)
        {
            popupLayer = popupLayerGo.AddComponent<GUIPopupLayer>();
        }

        // 7. 레이아웃 빌드
        popupLayer.BuildLayoutIfMissing();
        SetLayerRecursively(popupLayerGo, 5);

        // 8. GUIManager 바인딩
        if (guiMgr != null)
        {
            guiMgr.objPopupLayer = popupLayerGo;
            guiMgr.popupLayer = popupLayer;
            if (popupLayer.guiInventory != null)
                guiMgr.guiInventory = popupLayer.guiInventory;
            EditorUtility.SetDirty(guiMgr);
        }

        EditorUtility.SetDirty(popupLayerGo);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("<color=cyan>[UGUILayoutBuilder] Canvas/PopupLayer 하위에 UGUI 레이아웃이 성공적으로 구성 및 저장되었습니다!</color>");
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        if (go == null) return;
        go.layer = layer;
        foreach (Transform child in go.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
