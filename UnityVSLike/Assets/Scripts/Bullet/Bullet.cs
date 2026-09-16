using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Player master;
    public float speed = 10f;       // 탄환 이동 속도
    public float maxDistance = 10f; // 최대 이동 가능 거리

    private Vector3 vStart;
    //private Vector3 moveDirection = Vector3.right;

    private void Awake()
    {
        //Debug.Log($"[Bullet Lifecycle] Awake: {vStart}/{gameObject.name}");
    }

    private void OnEnable()
    {
        //Debug.Log($"[Bullet Lifecycle] OnEnable: {vStart}/{gameObject.name}");
    }

    void Start()
    {
        vStart = this.transform.position;
        //Debug.Log($"[Bullet Lifecycle] Start:{vStart}/{gameObject.name}");
    }

    void Update()
    {
        // 💡 등속 운동 공식 적용 (가속도 없는 일정한 속도 이동)
        //transform.position += moveDirection * speed * Time.deltaTime;

        // 이동 거리 체크 후 파괴 처리
        float fDist = Vector3.Distance(vStart, transform.position);
        if (fDist >= maxDistance)
        {
            Debug.Log($"Out Distance[{fDist}]: {gameObject.name}");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Monster")
        {
            Player target = collision.gameObject.GetComponent<Player>();
            Player attaker = master;

            if (target != null && target.Death())
            {
                attaker.Attack(target);
                GameManager.GetInstacne().monsterInventory.AddMonster(target.name);
            }

            // 몬스터 충돌 시 탄환 파괴
            //Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        Debug.Log($"[Bullet Lifecycle] OnDisable: {vStart},{this.transform.position}/{gameObject.name}");
    }

    private void OnDestroy()
    {
        Debug.LogError($"[Bullet Lifecycle] OnDestroy (파괴됨): {vStart},{this.transform.position}/{gameObject.name}");
    }
}