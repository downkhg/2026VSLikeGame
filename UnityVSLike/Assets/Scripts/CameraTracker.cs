using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    public GameObject objTarget;
    public float Speed = 1;

    void Start()
    {
        
    }

    void Update()
    {
        // FindGameObjectWithTag를 매 프레임 호출하면 씬 내 모든 오브젝트를 검색하므로 성능상 비효율적입니다.
        // objTarget = GameObject.FindGameObjectWithTag("Player");

        if (objTarget != null)
        {
            Vector3 vTargetPos = objTarget.transform.position;
            Vector3 vPos = this.transform.position;
            vTargetPos.z = vPos.z;
            Vector3 vDist = vTargetPos - vPos;
            Vector3 vDir = vDist.normalized;
            float fDist = vDist.magnitude;

            float fMove = Speed * Time.deltaTime;

            if (fDist > fMove)
                transform.position += vDir * fMove;
        }
    }
}