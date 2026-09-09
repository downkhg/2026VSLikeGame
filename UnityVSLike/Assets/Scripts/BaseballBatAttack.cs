using UnityEngine;

public class BaseballBatAttack : MonoBehaviour
{
    [Header("공격 범위 설정")]
    [Tooltip("최대 공격 사거리 (rr)")]
    public float maxRadius = 5.0f;

    [Tooltip("최소 공격 사거리 / 안쪽 제외 범위 (br)")]
    public float minRadius = 1.0f;

    [Tooltip("부채꼴 전체 각도 (deg)")]
    public float attackAngle = 90.0f;

    [Header("공격 및 넉백 옵션")]
    public float damage = 20.0f;
    public float knockbackForce = 10.0f;
    public LayerMask targetLayer;

    // 공격 실행 및 판정 확인 함수
    public void PerformAttack()
    {
        Debug.Log($"<color=yellow>[야구배트 공격 시작]</color> 위치: {transform.position}, 전방: {transform.forward}");

        // 1. OverlapSphere로 maxRadius 내의 모든 대상 1차 감지
        Collider[] targetsInMaxRadius = Physics.OverlapSphere(transform.position, maxRadius, targetLayer);

        if (targetsInMaxRadius.Length == 0)
        {
            Debug.Log("[결과] 최대 사거리 내에 감지된 대상이 없습니다.");
            return;
        }

        int hitCount = 0;

        foreach (Collider targetCollider in targetsInMaxRadius)
        {
            Transform target = targetCollider.transform;

            // P = 플레이어 위치, M = 몬스터 위치
            Vector3 p = transform.position;
            Vector3 m = target.position;

            // d = (m - p).magnitude
            Vector3 dirToTarget = (m - p);
            float d = dirToTarget.magnitude;
            Vector3 normalizedDir = dirToTarget.normalized;

            // 2. 거리 조건 검증: br < d && d < rr
            bool isDistanceValid = (d > minRadius && d < maxRadius);

            // 3. 각도 조건 검증: 전방 기준 부채꼴 내부 판정
            float angleToTarget = Vector3.Angle(transform.forward, normalizedDir);
            bool isAngleValid = (angleToTarget <= attackAngle / 2.0f);

            // 로그 출력: 각 타겟별 세부 데이터 확인
            string targetName = targetCollider.name;

            if (!isDistanceValid)
            {
                Debug.LogWarning($"[판정 실패 - 거리] 대상: {targetName} | 거리 d={d:F2} (허용 범위: {minRadius} < d < {maxRadius})");
                // 씬 뷰에 실패선 표시 (주황색)
                Debug.DrawLine(p, m, Color.magenta, 1.5f);
            }
            else if (!isAngleValid)
            {
                Debug.LogWarning($"[판정 실패 - 각도] 대상: {targetName} | 거리 d={d:F2} | 각도={angleToTarget:F1}° (허용 각도: ±{attackAngle / 2.0f}°)");
                // 씬 뷰에 실패선 표시 (노란색)
                Debug.DrawLine(p, m, Color.yellow, 1.5f);
            }
            else
            {
                // 성공 판정
                hitCount++;
                Debug.Log($"<color=green>[판정 성공 - 적격!]</color> 대상: {targetName} | 거리 d={d:F2} | 각도={angleToTarget:F1}°");

                // 씬 뷰에 명중선 및 넉백 방향 표시 (녹색 & 빨간색)
                Debug.DrawLine(p, m, Color.green, 1.5f);
                Debug.DrawRay(m, normalizedDir * 2.0f, Color.red, 1.5f);

                // 4. 피격 및 넉백 처리 실행
                ApplyHitAndKnockback(targetCollider, normalizedDir);
            }
        }

        Debug.Log($"<color=cyan>[공격 종료]</color> 총 피격 대상: {hitCount}명 / 범위 내 감지: {targetsInMaxRadius.Length}명");
    }

    private void ApplyHitAndKnockback(Collider target, Vector3 knockbackDirection)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            Debug.Log($" └─ [넉백 적용] {target.name}에게 {knockbackForce}의 힘 전달");
        }
        else
        {
            Debug.LogWarning($" └─ [넉백 실패] {target.name}에 Rigidbody 컴포넌트가 없습니다.");
        }
    }

    // 에디터 씬 뷰 상시/선택 시 visualizer (부채꼴 및 최소/최대 사거리 그리기)
    private void OnDrawGizmosSelected()
    {
        Vector3 position = transform.position;

        // 최대 사거리 (rr) - 파란색 원
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(position, maxRadius);

        // 최소 사거리 (br) - 빨간색 원
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(position, minRadius);

        // 부채꼴 좌우 경계선 - 파란색 선
        Vector3 leftBoundary = Quaternion.Euler(0, -attackAngle / 2.0f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, attackAngle / 2.0f, 0) * transform.forward;

        Gizmos.DrawRay(position + leftBoundary * minRadius, leftBoundary * (maxRadius - minRadius));
        Gizmos.DrawRay(position + rightBoundary * minRadius, rightBoundary * (maxRadius - minRadius));
    }
}