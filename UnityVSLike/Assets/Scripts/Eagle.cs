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

    public LayerMask attacklayerMask;

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
        // 이미 대상을 추적 중이면 탐색 생략
        if (curState == E_AI_STATE.TRACKING && objTarget != null) return;

        int nLayer = 1 << LayerMask.NameToLayer("Player");
        Collider2D collider = Physics2D.OverlapCircle(this.transform.position, Site > 2.0f ? Site : 8.0f, nLayer);
        if (collider)
        {
            objTarget = collider.gameObject;
            SetAIState(E_AI_STATE.TRACKING);
            return;
        }

        // 탑다운 무한 맵 환경: 순찰 지점이 없으면 플레이어를 자동 탐색하여 추적
        if (objPatrolPoint == null && objResponPoint == null && objTarget == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                objTarget = playerObj;
                SetAIState(E_AI_STATE.TRACKING);
            }
        }
    }

    private void Awake()
    {
        if (Site < 2.0f) Site = 8.0f; // 탑다운 맵을 위한 시야 반경 기본값 보정
        SetAIState(curState);
    }

    private void Start()
    {
        //SetAIState(curState);
    }

    void Update()
    {
        UpdateAIState();

        // 추적 대상 방향으로 스프라이트 좌우 반전
        if (objTarget != null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.flipX = (objTarget.transform.position.x - transform.position.x) > 0;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(this.transform.position, Site);
    }

    private void FixedUpdate()
    {
        FindProcess();
        ProcessAttack(); 
    }

    void ProcessAttack()
    {
        CircleCollider2D circleCollider2D = GetComponent<CircleCollider2D>();
        if (circleCollider2D == null) return;

        Collider2D collider = Physics2D.OverlapCircle(transform.position, circleCollider2D.radius, attacklayerMask);
        if (collider != null)
        {
            Player target = collider.gameObject.GetComponent<Player>();
            Player attaker = this.gameObject.GetComponent<Player>();
            if (target != null && !target.isDead)
            {
                SuperMode superMode = target.GetComponent<SuperMode>();
                bool canAttack = (superMode == null || !superMode.isUse);

                if (canAttack)
                {
                    if (attaker != null)
                    {
                        attaker.Attack(target);
                    }
                    else
                    {
                        target.OnDamaged(10);
                    }

                    if (superMode != null)
                    {
                        superMode.OnMode();
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("OnTriggerEnter2D:"+collision.gameObject.name);
        if (collision.gameObject.tag == "Player")
        {
            objTarget = collision.gameObject;
            ProcessAttack();
        }
    }
}
