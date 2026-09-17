using UnityEngine;

public class BaseballBatAttack : MonoBehaviour
{
    [Header("부채꼴 공격 범위 설정")]
    [Tooltip("최대 공격 사거리 반경")]
    public float maxRadius = 5.0f;

    [Tooltip("최소 공격 사거리 반경")]
    public float minRadius = 1.0f;

    [Tooltip("부채꼴 공격 각도 (도)")]
    public float attackAngle = 90.0f;

    [Header("공격 및 피격 설정")]
    public float damage = 20.0f;
    public float knockbackForce = 10.0f;
    public LayerMask targetLayer;

    [Header("스윙 속도 설정")]
    public float swingSpeed = 360.0f; // 초당 회전 각도

    public Player master;
    public GameObject objTarget;

    public BatSwing batSwing;

    private void Awake()
    {
        if (batSwing != null)
        {
            batSwing.Init(swingSpeed, attackAngle);
        }
    }

    private void FixedUpdate()
    {
        Vector3 vPos = transform.position;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(vPos, maxRadius, targetLayer);

        foreach (Collider2D collider in colliders)
        {
            objTarget = collider.gameObject;
            break;
        }
    }

    public float fAngle;
    void AttackProcess()
    {
        if (objTarget)
        {
            Vector3 vPos = transform.position;
            Vector3 vTargetPos = objTarget.transform.position;
            Vector3 vDist = vTargetPos - vPos;
            float fDist = vDist.magnitude;
            float fHalf = attackAngle * 0.5f;

            if (fDist > maxRadius)
            {
                objTarget = null;
                return;
            }
            else if (fDist > minRadius)
            {
                fAngle = Vector3.Angle(transform.right, vDist);

                if (fAngle < fHalf)
                {
                    Player targetPlayer = objTarget.GetComponent<Player>();
                    if (targetPlayer && master)
                    {
                        SuperMode supermode = targetPlayer.GetComponent<SuperMode>();
                        if (!supermode.isUse)
                        {
                            batSwing.Init(swingSpeed, attackAngle);
                            batSwing.Swing();
                            //master.Attack(targetPlayer);
                            supermode.OnMode();
                            Debug.Log("Bat Attack!");
                        }
                    }
                    return;
                }
            }
        }
    }

    public float maxTime = 0.5f;
    public float currentTime = 0.0f;

    void UpdateTime()
    {
        currentTime += Time.deltaTime;

        if (currentTime >= maxTime)
        {
            currentTime = 0.0f;
            AttackProcess();
        }
    }

    private void Update()
    {
        UpdateTime();
    }

    // 에디터 씬 뷰 시각화 (공격 범위 및 부채꼴 영역 표시)
    private void OnDrawGizmosSelected()
    {
        float fHalf = attackAngle * 0.5f;
        Vector3 position = transform.position;
        Vector3 vBase = transform.right;
        Vector3 vRight;
        Vector3 vLeft;
        Quaternion qMultiple;
        Vector3 vLineEnd;

        qMultiple = Quaternion.Euler(0, 0, fHalf);
        vRight = qMultiple * vBase;
        qMultiple = Quaternion.Euler(0, 0, -fHalf);
        vLeft = qMultiple * vBase;

        Gizmos.color = Color.yellow;
        vLineEnd = position + vRight * maxRadius;
        Gizmos.DrawLine(position, vLineEnd);
        vLineEnd = position + vLeft * maxRadius;
        Gizmos.DrawLine(position, vLineEnd);

        // 최대 공격 반경
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(position, maxRadius);

        // 최소 공격 반경
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(position, minRadius);

        // 부채꼴 좌우 경계선
        Vector3 leftBoundary = Quaternion.Euler(0, 0, fHalf) * transform.right;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -fHalf) * transform.right;

        Gizmos.color = new Color(0, 0, 1);
        Gizmos.DrawRay(position + leftBoundary * minRadius, leftBoundary * (maxRadius - minRadius));
        Gizmos.DrawRay(position + rightBoundary * minRadius, rightBoundary * (maxRadius - minRadius));
    }
}