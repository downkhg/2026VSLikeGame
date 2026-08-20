using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KunaiGun : MonoBehaviour
{
    public GameObject prefabBullet;
    public float ShotPower;
    public float searchRadius = 10f; // 적을 탐색할 최대 반경

    [Header("발사 설정")]
    public float fireInterval = 0.5f; // 발사 주사 간격 (초 단위)
    private float fireTimer = 0f;     // 타이머용 변수

    public Player master; // 플레이어 참조 (인스펙터에서 할당하거나 코드로 가져옴)

    void Update()
    {
        // 시간이 흐를 때마다 타이머 증가
        fireTimer += Time.deltaTime;

        // 설정된 주기가 되면 샷 함수 호출
        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f; // 타이머 초기화

            // [로그] 발사 주기 도달 확인
            Debug.Log("[KunaiGun] 발사 주기가 되어 Shot(master)을 호출합니다.");

            // 타겟팅 자동 발사 실행
            Shot(master);
        }
    }

    // 1. 특정 타겟(적)을 지정해서 발사하는 메서드
    public void Shot(Transform target, Player master)
    {
        Debug.Log("[KunaiGun] Shot Start (Target 지정 발사)");

        if (target == null)
        {
            Debug.LogWarning("[KunaiGun] Shot 실패: 타겟(Target)이 null입니다.");
            return;
        }

        // [로그] 타겟 탐색 성공 정보
        Debug.Log($"[KunaiGun] 타겟 발견! 타겟 이름: {target.name}, 타겟 위치: {target.position}");

        Vector2 dir = (target.position - transform.position).normalized;

        GameObject copyBullet = Instantiate(prefabBullet, transform.position, Quaternion.identity);
        Rigidbody2D rigidbody = copyBullet.GetComponent<Rigidbody2D>();
        Bullet bullet = copyBullet.GetComponent<Bullet>();

        if (bullet != null)
        {
            bullet.master = master;
        }
        else
        {
            Debug.LogWarning("[KunaiGun] 생성된 쿠나이 프리팹에 'Bullet' 컴포넌트가 없습니다.");
        }

        if (rigidbody != null)
        {
            rigidbody.AddForce(dir * ShotPower, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogWarning("[KunaiGun] 생성된 쿠나이 프리팹에 'Rigidbody2D' 컴포넌트가 없습니다.");
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        copyBullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // [로그] 생성된 프리팹(쿠나이)의 정보 상세 출력
        Debug.Log($"[KunaiGun] 쿠나이 발사 완료! " +
                  $"| 오브젝트 이름: {copyBullet.name} " +
                  $"| 생성 위치: {copyBullet.transform.position} " +
                  $"| 회전 각도(Angle): {angle:F2}° " +
                  $"| 발사 방향(Dir): {dir} " +
                  $"| 샷 파워: {ShotPower}");

        Debug.Log("[KunaiGun] Shot End");
    }

    // 2. 타겟을 지정하지 않았을 때, 가장 가까운 적을 스스로 찾아 발사하는 메서드 (오버로드)
    public void Shot(Player master)
    {
        Debug.Log($"[KunaiGun] 가장 가까운 적을 탐색합니다. (탐색 반경: {searchRadius})");
        Transform nearestEnemy = FindNearestEnemy();

        if (nearestEnemy != null)
        {
            Shot(nearestEnemy, master);
        }
        else
        {
            Debug.Log("[KunaiGun] 주변에 적이 없습니다. 기본 전방(transform.right) 방향으로 발사합니다.");
            // 주변에 적이 없을 경우 기본 전방으로 발사
            Shot(transform.right, master);
        }
    }

    // 3. 기존 코드와의 호환을 위한 방향(Vector3) 기반 발사 메서드
    public void Shot(Vector3 dir, Player master)
    {
        Debug.Log($"[KunaiGun] Shot Start (방향 벡터 기반 발사) | 지정 방향: {dir}");

        GameObject copyBullet = Instantiate(prefabBullet, transform.position, Quaternion.identity);
        Rigidbody2D rigidbody = copyBullet.GetComponent<Rigidbody2D>();
        Bullet bullet = copyBullet.GetComponent<Bullet>();

        if (bullet != null)
        {
            bullet.master = master;
        }

        if (rigidbody != null)
        {
            rigidbody.AddForce(dir * ShotPower, ForceMode2D.Impulse);
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        copyBullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // [로그] 방향 발사 시 생성된 프리팹 정보 출력
        Debug.Log($"[KunaiGun] (방향) 쿠나이 발사 완료! " +
                  $"| 오브젝트 이름: {copyBullet.name} " +
                  $"| 생성 위치: {copyBullet.transform.position} " +
                  $"| 회전 각도(Angle): {angle:F2}°");

        Debug.Log("[KunaiGun] Shot End");
    }

    // 가장 가까운 적을 찾는 헬퍼 함수
    Transform FindNearestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(this.transform.position, searchRadius, 1<<LayerMask.NameToLayer("Monster"));
        Transform nearest = null;
        float minDistance = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        Debug.Log($"[KunaiGun] OverlapCircleAll 감지된 총 콜라이더 수: {enemies.Length}");

        foreach (Collider2D enemy in enemies)
        {
            // 자기 자신이나 플레이어 등 예외 처리가 필요하다면 여기서 조건 추가 가능
            float dist = Vector3.Distance(currentPos, enemy.transform.position);
            if (dist < minDistance && dist <= searchRadius)
            {
                minDistance = dist;
                nearest = enemy.transform;
            }
        }

        if (nearest != null)
        {
            Debug.Log($"[KunaiGun] 가장 가까운 적 확정 -> 이름: {nearest.name}, 거리: {minDistance:F2}");
        }
        else
        {
            Debug.Log("[KunaiGun] 탐색 반경 내에 유효한 적이 없습니다.");
        }

        return nearest;
    }
}