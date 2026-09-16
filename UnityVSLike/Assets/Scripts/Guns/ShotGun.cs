using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShotGun : MonoBehaviour
{
    public GameObject prefabBullet;
    public float ShotPower;
    public float searchRadius = 10f; // 적을 탐색할 최대 반경

    [Header("샷건 전용 설정")]
    public float spreadAngle = 30f;  // 탄환이 퍼질 수 있는 총 각도 범위 (예: 30도)

    [Header("발사 설정")]
    public float fireInterval = 0.2f; // 단발 연사 주기에 맞게 조정
    [SerializeField] private float fireTimer = 0f;

    public Player master;

    [Header("타겟팅 설정")]
    [SerializeField] private Transform currentTarget; // 현재 유효한 타겟 저장

    [Header("디버그 기즈모 설정")]
    public bool showGizmos = true;
    public Color searchColor = Color.green; // 탐색 반경 색상
    public Color spreadColor = Color.red;   // 부채꼴 발사 범위 색상
    public float debugRayDuration = 0.5f;   // Debug.DrawLine 레이 표시 지속 시간

    public float angleDifference;

    void Update()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            //Shot(master);
        }
    }

    // 플레이어 또는 무기의 '실제 바라보는 방향 Vector2' 추출
    private Vector2 GetPlayerFacingDirection()
    {
        if (master != null && master.transform.localScale.x < 0)
        {
            return Vector2.left;
        }

        if (Mathf.Abs(transform.right.x) > 0.01f || Mathf.Abs(transform.right.y) > 0.01f)
        {
            return transform.right;
        }

        return Vector2.right;
    }

    // 1. 타겟 유지 로직이 적용된 메인 발사 진입점
    public void Shot(Vector3 dir, Player master)
    {
        if (master == null) return;
        Transform target = currentTarget;

        // 플레이어 정면 방향 및 월드 기준 각도 (0~360도)
        if (target != null)
        {
            // 별도 함수 분리 없이 인라인 처리
            // 1. 퍼짐 오프셋 연산
            float halfSpread = spreadAngle / 2f;
            float randomOffset = Random.Range(-halfSpread, halfSpread);
            float baseAngle = Vector2.SignedAngle(dir, dir);
            float finalAngle = baseAngle + randomOffset;

            // 2. 삼각함수로 이동 방향 계산
            float rad = finalAngle * Mathf.Deg2Rad;

            Vector3 targetDist = target.position - transform.position;
            Vector2 targetDir = targetDist.normalized;

            angleDifference = Vector2.Angle(dir, targetDir);

            // 사이각이 spreadAngle 절반 이하인 경우 적 위치 조준 발사
            if (angleDifference <= spreadAngle / 2f)
            {
                baseAngle = Vector2.SignedAngle(Vector2.right, targetDir);
                dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                Debug.Log($"[Shotgun Action] 🎯 적 조준 발사 (기준 각도: {baseAngle:F1}°)");
            }
            else if(angleDifference >= 90)
            {
                Debug.Log($"[Shotgun Action] 🔄 조준 범위를 벗어남({angleDifference:F1}°) -> 플레이어 방향으로 발사 ({baseAngle:F1}°)");
            }
            else
            {
                Debug.Log($"[Shotgun Action] 🔄 조준 범위를 벗어남({angleDifference:F1}°) -> 정면 발사 ({baseAngle:F1}°)");
            }

            // 4. 인스턴스화 및 컴포넌트 데이터 세팅
            GameObject copyBullet = Instantiate(prefabBullet, transform.position, Quaternion.identity);
            copyBullet.transform.rotation = Quaternion.AngleAxis(finalAngle, Vector3.forward);

            Bullet bullet = copyBullet.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.master = master;
                //bullet.InitBullet(dir, ShotPower);
               
            }

            Rigidbody2D rigidbody = copyBullet.GetComponent<Rigidbody2D>();
            if (rigidbody != null)
            {
                rigidbody.AddForce(dir * ShotPower, ForceMode2D.Impulse);
                copyBullet.transform.rotation = Quaternion.AngleAxis(finalAngle, Vector3.forward);
            }
        }
        else
        {
            Debug.Log($"[Shotgun Check] 타겟 없음 -> 무기 정면 방향 기준 발사");

            // 4. 인스턴스화 및 컴포넌트 데이터 세팅
            GameObject copyBullet = Instantiate(prefabBullet, transform.position, Quaternion.identity);
            //copyBullet.transform.rotation = Quaternion.AngleAxis(finalAngle, Vector3.forward);

            Bullet bullet = copyBullet.GetComponent<Bullet>();
            f//loat randomOffset = Random.Range(-halfSpread, halfSpread);
            if (bullet != null)
            {
                bullet.master = master;
                //bullet.InitBullet(dir, ShotPower);
            }

            Rigidbody2D rigidbody = copyBullet.GetComponent<Rigidbody2D>();
            if (rigidbody != null)
            {
                rigidbody.AddForce(dir * ShotPower, ForceMode2D.Impulse);
                //copyBullet.transform.rotation = Quaternion.AngleAxis(finalAngle, Vector3.forward);
            }
        }

      
        // 3. 디버그 레이 드로우
        if (showGizmos)
        {
           //Debug.DrawLine(transform.position, transform.position + dir * 5f, spreadColor, debugRayDuration);
        }

       
    }

    // 탄환 생성 및 물리 처리를 담당하는 단일 실행 블록
    private void ExecuteBulletFiring(float baseAngle, Player master)
    {
        // 1. 퍼짐 오프셋 연산
        float halfSpread = spreadAngle / 2f;
        float randomOffset = Random.Range(-halfSpread, halfSpread);
        float finalAngle = baseAngle + randomOffset;

        // 2. 삼각함수로 이동 방향 계산
        float rad = finalAngle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        // 3. 디버그 레이 드로우
        if (showGizmos)
        {
            Debug.DrawLine(transform.position, (Vector2)transform.position + dir * 5f, spreadColor, debugRayDuration);
        }

        // 4. 인스턴스화 및 컴포넌트 데이터 세팅
        GameObject copyBullet = Instantiate(prefabBullet, transform.position, Quaternion.identity);
        copyBullet.transform.rotation = Quaternion.AngleAxis(finalAngle, Vector3.forward);

        Bullet bullet = copyBullet.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.master = master;
        }

        Rigidbody2D rigidbody = copyBullet.GetComponent<Rigidbody2D>();
        if (rigidbody != null)
        {
            rigidbody.AddForce(dir * ShotPower, ForceMode2D.Impulse);
        }
    }

    bool IsTargetValid(Transform target)
    {
        if (target == null) return false;
        if (!target.gameObject.activeInHierarchy) return false;

        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= searchRadius;
    }

    Transform FindNearestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, searchRadius, 1 << LayerMask.NameToLayer("Monster"));
        Transform nearest = null;
        float minDistance = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (Collider2D enemy in enemies)
        {
            float dist = Vector3.Distance(currentPos, enemy.transform.position);
            if (dist < minDistance && dist <= searchRadius)
            {
                minDistance = dist;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Vector3 currentPos = transform.position;

        // 1. 탐색 범위
        Gizmos.color = searchColor;
        Gizmos.DrawWireSphere(currentPos, searchRadius);

        // 2. 타겟 연결선
        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(currentPos, currentTarget.position);
        }

        // 3. 부채꼴 드로우
        Gizmos.color = spreadColor;
        Vector2 playerDir = GetPlayerFacingDirection();
        float baseAngle = Vector2.SignedAngle(Vector2.right, playerDir);
        float halfSpread = spreadAngle / 2f;

        float leftAngle = baseAngle - halfSpread;
        float rightAngle = baseAngle + halfSpread;

        Vector3 leftDir = new Vector3(Mathf.Cos(leftAngle * Mathf.Deg2Rad), Mathf.Sin(leftAngle * Mathf.Deg2Rad), 0f);
        Vector3 rightDir = new Vector3(Mathf.Cos(rightAngle * Mathf.Deg2Rad), Mathf.Sin(rightAngle * Mathf.Deg2Rad), 0f);

        Gizmos.DrawLine(currentPos, currentPos + leftDir * 3f);
        Gizmos.DrawLine(currentPos, currentPos + rightDir * 3f);
        Gizmos.DrawLine(currentPos + leftDir * 3f, currentPos + rightDir * 3f);
    }
}