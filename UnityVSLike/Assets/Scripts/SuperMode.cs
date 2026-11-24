using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperMode : MonoBehaviour
{
    [Header("=== 무적 모드 설정 ===")]
    public bool isUse = false;
    [Tooltip("무적 지속 시간(초)")]
    public float duration = 1.0f;

    // 하위 호환용 프로퍼티 (Time 변수명 섀도잉 방지)
    public float Time
    {
        get => duration;
        set => duration = value;
    }

    private SpriteRenderer cachedRenderer;

    private void Awake()
    {
        cachedRenderer = GetComponent<SpriteRenderer>();
    }

    private IEnumerator ProcessTimer()
    {
        isUse = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += UnityEngine.Time.deltaTime;
            if (cachedRenderer != null)
            {
                // 피격 시 주기적인 반투명 깜빡임 효과
                float alpha = Mathf.PingPong(elapsed * 10f, 0.7f) + 0.3f;
                Color col = cachedRenderer.color;
                col.a = alpha;
                cachedRenderer.color = col;
            }
            yield return null;
        }

        isUse = false;
        if (cachedRenderer != null)
        {
            Color col = cachedRenderer.color;
            col.a = 1.0f;
            cachedRenderer.color = col;
        }
    }

    public void OnMode()
    {
        StartCoroutine(ProcessTimer());
    }

    // 하위 호환용 오타 메서드 별칭
    public IEnumerator ProcessTimmer()
    {
        return ProcessTimer();
    }

    void Start()
    {
        if (isUse) OnMode();
    }
}
