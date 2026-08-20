using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Player master;
    Vector3 vStart;

    // 1. 오브젝트가 생성되면서 메모리에 올라갈 때 (가장 먼저 호출)
    private void Awake()
    {
        Debug.Log($"[Bullet Lifecycle] Awake: {vStart}/{gameObject.name}");
    }

    // 2. 오브젝트가 활성화될 때 (인스턴스화 직후 또는 활성 상태가 될 때)
    private void OnEnable()
    {
        Debug.Log($"[Bullet Lifecycle] OnEnable: {vStart}/{gameObject.name}");
    }

    // Start is called before the first frame update
    void Start()
    {
        
        //Destroy(gameObject, 1);
        vStart = this.transform.position;
        Debug.Log($"[Bullet Lifecycle] Start:{vStart}/{gameObject.name}");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 vPos = this.transform.position;
        //벡터의 차를 이용하여 거리를 구하는 방법
        //Vector3 vDist = vStart - vPos;
        //float fDist = vDist.magnitude;
        float fDist = Vector3.Distance(vStart, vPos);

        if (fDist >= 1)
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
            //GameManager.GetInstacne().responnerPlayer.objPlayer.GetComponent<Player>();

            attaker.Attack(target);
            if (target.Death())
                GameManager.GetInstacne().monsterInventory.AddMonster(target.name);

            // 만약 몬스터와 충돌 시 총알을 파괴하고 싶다면 아래 주석을 해제하세요.
            // Debug.Log($"Hit Monster: {gameObject.name}");
            // Destroy(gameObject);
        }
    }

    // 3. 오브젝트가 비활성화될 때
    private void OnDisable()
    {
        Debug.Log($"[Bullet Lifecycle] OnDisable: {vStart},{this.transform.position}/{gameObject.name}");
    }

    // 4. 오브젝트가 완전히 파괴(Destroy)될 때 (가장 마지막에 호출)
    private void OnDestroy()
    {
        Debug.LogError($"[Bullet Lifecycle] OnDestroy (파괴됨): {vStart},{this.transform.position}/{gameObject.name}");
    }
}