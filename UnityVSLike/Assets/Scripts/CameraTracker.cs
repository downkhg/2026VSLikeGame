using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    public GameObject objTarget;
    public float Speed = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Find게임오브젝트를 사용하면 항상 모든 오브젝트중에서 필요한것을 검색하므로 느려진다.
        //objTarget = GameObject.FindGameObjectWithTag("Player");

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
