using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eagle : MonoBehaviour
{
    public GameObject objTarget;
    public float Speed;
    public float Site = 0.5f;
    public GameObject objResponPoint;
    public GameObject objPatrolPoint;
    public bool isMove = false;

    public enum E_AI_STATE {NONE = -1, TRACKING, RETRUN, PATOL }
    public E_AI_STATE curState = E_AI_STATE.NONE;

    public void SetAIState(E_AI_STATE state)
    {
        if (curState == state) return;
        Debug.Log("SetAIState:"+state);
        switch(state)
        {
            case E_AI_STATE.TRACKING:
                break;
            case E_AI_STATE.RETRUN:
                objTarget = objResponPoint;
                break;
            case E_AI_STATE.PATOL:
                objTarget = objPatrolPoint;
                break;
        }
        curState = state;
    }

    public void UpdateAIState()
    {
        switch (curState)
        {
            case E_AI_STATE.TRACKING:
                TargetTrackingProcess();
                if (objTarget == null)
                    SetAIState(E_AI_STATE.RETRUN);
                break;
            case E_AI_STATE.RETRUN:
                TargetTrackingProcess();
                if (isMove == false)
                    SetAIState(E_AI_STATE.PATOL);
                break;
            case E_AI_STATE.PATOL:
                TargetTrackingProcess();
                PatolProcess();
                break;
        }
    }

    void PatolProcess()
    {
        if (objTarget)
        {
            if (isMove == false)
            {
                if (objTarget.name == objResponPoint.name)
                {
                    objTarget = objPatrolPoint;
                    //Debug.LogError(objPatrolPoint.name);
                }
                else if (objTarget.name == objPatrolPoint.name)
                {
                    objTarget = objResponPoint;
                    //Debug.LogError(objResponPoint.name);
                }
            }
        }
    }

    void TargetTrackingProcess()
    {
        if (objTarget != null)
        {
            Vector3 vTargetPos = objTarget.transform.position;
            Vector3 vPos = this.transform.position;
            Vector3 vDist = vTargetPos - vPos;
            Vector3 vDir = vDist.normalized;
            float fDist = vDist.magnitude;

            float fMove = Speed * Time.deltaTime;

            if (fDist > fMove)
            {
                transform.position += vDir * fMove;
                isMove = true;
            }
            else
            {
                isMove = false;
            }
        }
        else
        {
            objTarget = objResponPoint;
        }
    }

    void FindProcess()
    {
        int nLayer = 1 << LayerMask.NameToLayer("Player");
        Collider2D collider =
            //Physics2D.OverlapCircle(this.transform.position, Site);
            Physics2D.OverlapCircle(this.transform.position, Site, nLayer);
        if (collider)
        {
            objTarget = collider.gameObject;
            SetAIState(E_AI_STATE.TRACKING);
        }
    }

    private void Awake()
    {
        SetAIState(curState);
    }

    private void Start()
    {
        //SetAIState(curState);
    }

    void Update()
    {
        //TargetTrackingProcess();
        //PatolProcess();
        UpdateAIState();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(this.transform.position, Site);
    }

    private void FixedUpdate()
    {
        FindProcess();
    }

    // Update is called once per frame

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("OnTriggerEnter2D:"+collision.gameObject.name);
        if (collision.gameObject.tag == "Player")
        {
            Player target = collision.gameObject.GetComponent<Player>();
            Player attaker = this.gameObject.GetComponent<Player>();
            SuperMode superMode = target.GetComponent<SuperMode>();

            if (superMode.isUse == false)
            {
                attaker.Attack(target);
                superMode.OnMode();
            }
            //Destroy(collision.gameObject);
        }
    }
}
