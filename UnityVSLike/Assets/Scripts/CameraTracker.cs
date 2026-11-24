using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    [Header("=== 추적 대상 및 속도 ===")]
    public GameObject objTarget;
    [Tooltip("추적 부드러움 계수 (기본: 5.0)")]
    public float Speed = 5.0f;
    [Tooltip("카메라 Z 오프셋 (기본: -10)")]
    public float offsetZ = -10.0f;

    void Start()
    {
        FindTarget();
    }

    /// <summary>
    /// 대상이 지정되어 있지 않은 경우 플레이어를 자동 탐색합니다.
    /// </summary>
    public void FindTarget()
    {
        if (objTarget != null) return;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            objTarget = playerObj;
            return;
        }

        Player playerComp = FindFirstObjectByType<Player>();
        if (playerComp != null)
        {
            objTarget = playerComp.gameObject;
        }
    }

    void LateUpdate()
    {
        if (objTarget == null)
        {
            FindTarget();
            if (objTarget == null) return;
        }

        // 플레이어의 X, Y 좌표를 부드럽게 추적 (Z축은 카메라 깊이 유지)
        Vector3 targetPos = objTarget.transform.position;
        targetPos.z = offsetZ;

        // Vector3.Lerp를 이용한 부드러운 카메라 추적 (Jitter 떨림 방지)
        transform.position = Vector3.Lerp(transform.position, targetPos, Speed * Time.deltaTime);
    }
}