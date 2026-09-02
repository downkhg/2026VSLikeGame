using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public float damage = 50f;
    public Player master; // 필요 시 데미지 주체 전달용

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 몬스터 레이어나 태그 판별
        if (collision.CompareTag("Monster"))
        {
            Debug.Log($"[ExplosionEffect] 몬스터 폭발 범위 감지! | 대상: {collision.name} | 데미지: {damage}");

            // 몬스터 체력 스크립트에 데미지 전달
            // collision.GetComponent<Enemy>()?.TakeDamage(damage, master);
        }
    }

    // 애니메이션이 끝나거나 일정 시간이 지나면 자동으로 파괴되도록 연동
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}