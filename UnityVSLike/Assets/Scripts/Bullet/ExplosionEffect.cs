using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public float damage = 50f;
    public Player master; // 필요 시 데미지 전달을 위한 공격자 플레이어 참조

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 몬스터 레이어 및 태그 식별
        if (collision.CompareTag("Monster"))
        {
            Debug.Log($"[ExplosionEffect] 폭발 범위 피격 감지! | 대상: {collision.name} | 데미지: {damage}");
            Player target = collision.GetComponent<Player>();
            if (target != null && master != null)
            {
                master.Attack(target);
            }
        }
    }

    // 애니메이션 이벤트 또는 수명 만료 시 자동 파괴
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}