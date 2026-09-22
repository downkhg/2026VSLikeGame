using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemInfoManager : MonoBehaviour
{
    private static ItemInfoManager _instance;
    public static ItemInfoManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ItemInfoManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("@ItemInfoManager");
                    _instance = go.AddComponent<ItemInfoManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("로드된 아이템 정보 목록 (인스펙터 확인용)")]
    [SerializeField] private List<ItemInfo> itemInfoList = new List<ItemInfo>();

    // 빠른 검색을 위한 Dictionary (ID 및 GunType 기준)
    private Dictionary<int, ItemInfo> itemDictById = new Dictionary<int, ItemInfo>();
    private Dictionary<TotalGun.GunType, ItemInfo> itemDictByGunType = new Dictionary<TotalGun.GunType, ItemInfo>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 시작 시 CSV 파일로부터 아이템 데이터 자동 로드
        LoadItemDataFromCSV();
    }

    /// <summary>
    /// Resources/Data/ItemData.csv 파일을 읽어와 아이템 목록을 구성합니다.
    /// </summary>
    public void LoadItemDataFromCSV(string resourcePath = "Data/ItemData")
    {
        ClearAll();

        TextAsset csvAsset = Resources.Load<TextAsset>(resourcePath);
        if (csvAsset == null)
        {
            Debug.LogError($"[ItemInfoManager] CSV 파일을 찾을 수 없습니다: Resources/{resourcePath}.csv");
            return;
        }

        string text = csvAsset.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length <= 1)
        {
            Debug.LogWarning("[ItemInfoManager] CSV 파일에 데이터 행이 없습니다.");
            return;
        }

        // 1번째 줄은 헤더이므로 index 1부터 순회
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

            string[] tokens = line.Split(',');
            if (tokens.Length < 5)
            {
                Debug.LogWarning($"[ItemInfoManager] 데이터 포맷 오류 (Line {i + 1}): {line}");
                continue;
            }

            // 파싱 처리
            int id = int.Parse(tokens[0].Trim());
            string name = tokens[1].Trim();
            string description = tokens[2].Trim();

            // GunType Enum 파싱
            if (!Enum.TryParse(tokens[3].Trim(), out TotalGun.GunType gunType))
            {
                Debug.LogWarning($"[ItemInfoManager] 알 수 없는 GunType: '{tokens[3]}' (Line {i + 1})");
                gunType = TotalGun.GunType.DefaultGun;
            }

            int score = int.Parse(tokens[4].Trim());
            string prefabPath = tokens.Length > 5 ? tokens[5].Trim() : "";

            ItemInfo info = new ItemInfo(id, name, description, gunType, score, prefabPath);
            AddItemInfo(info);
        }

        Debug.Log($"[ItemInfoManager] CSV 로드 완료: 총 {itemInfoList.Count}개의 아이템 정보 등록됨.");
    }

    /// <summary>
    /// 새로운 아이템 정보를 목록에 등록합니다.
    /// </summary>
    public void AddItemInfo(ItemInfo info)
    {
        if (info == null) return;

        itemInfoList.Add(info);

        if (!itemDictById.ContainsKey(info.ID))
        {
            itemDictById.Add(info.ID, info);
        }

        if (!itemDictByGunType.ContainsKey(info.GunType))
        {
            itemDictByGunType.Add(info.GunType, info);
        }
    }

    /// <summary>
    /// ID로 아이템 정보를 가져옵니다.
    /// </summary>
    public ItemInfo GetItemInfo(int id)
    {
        if (itemDictById.TryGetValue(id, out ItemInfo info))
        {
            return info;
        }
        Debug.LogWarning($"[ItemInfoManager] ID {id} 에 해당하는 아이템 정보가 없습니다.");
        return null;
    }

    /// <summary>
    /// GunType으로 아이템 정보를 가져옵니다.
    /// </summary>
    public ItemInfo GetItemInfo(TotalGun.GunType gunType)
    {
        if (itemDictByGunType.TryGetValue(gunType, out ItemInfo info))
        {
            return info;
        }
        Debug.LogWarning($"[ItemInfoManager] GunType {gunType} 에 해당하는 아이템 정보가 없습니다.");
        return null;
    }

    /// <summary>
    /// 등록된 전체 아이템 목록을 반환합니다.
    /// </summary>
    public List<ItemInfo> GetAllItemInfos()
    {
        return itemInfoList;
    }

    /// <summary>
    /// 등록된 아이템 목록 중 중복 없이 count개를 무작위로 선택하여 반환합니다.
    /// </summary>
    public List<ItemInfo> GetRandomItemInfos(int count = 3)
    {
        List<ItemInfo> result = new List<ItemInfo>();
        if (itemInfoList == null || itemInfoList.Count == 0)
        {
            // 혹시 아직 로드되지 않은 경우 로드 시도
            LoadItemDataFromCSV();
            if (itemInfoList.Count == 0) return result;
        }

        List<ItemInfo> pool = new List<ItemInfo>(itemInfoList);
        int pickCount = Mathf.Min(count, pool.Count);

        for (int i = 0; i < pickCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, pool.Count);
            result.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);
        }

        return result;
    }

    /// <summary>
    /// 등록된 모든 아이템 정보를 제거/초기화합니다.
    /// </summary>
    public void ClearAll()
    {
        itemInfoList.Clear();
        itemDictById.Clear();
        itemDictByGunType.Clear();
    }
}
