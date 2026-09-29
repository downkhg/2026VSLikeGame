using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dynamic : MonoBehaviour
{
    public float JumpPower = 10;
    public bool isJump = false;
    public int Score = 0;
    public float Speed = 1;

    [Header("=== 액티브 무기 (BaseGun) ===")]
    [Tooltip("사용자가 지정한 특정 무기. 쿨타임과 무관하게 X키로 즉시 발사되며, 자동 발사는 비활성화됩니다.")]
    public BaseGun gun;
    public Vector3 dir = Vector3.right;

    private void Awake()
    {
        if (gun == null)
        {
            gun = GetComponentInChildren<BaseGun>();
        }

        if (gun != null)
        {
            // 설정된 액티브 무기는 자동 발사를 끄고 수동 액티브 전용으로 전환
            gun.isAutoFire = false;
        }
    }

    /// <summary>
    /// 사용자가 설정한 특정 무기를 액티브 무기로 등록합니다.
    /// 등록된 액티브 무기는 자동 발사가 비활성화되고, X키 입력 시 쿨타임과 무관하게 즉발됩니다.
    /// </summary>
    public void SetActiveGun(BaseGun newGun)
    {
        if (gun != null && gun != newGun)
        {
            // 기존 무기는 다시 패시브 자동 발사로 복원
            gun.isAutoFire = true;
        }

        gun = newGun;

        if (gun != null)
        {
            gun.isAutoFire = false;
            Debug.Log($"[Dynamic] 액티브 무기 설정 완료: {gun.GetType().Name} (자동 발사 OFF, 쿨타임 무시 즉발 사용 가능)");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.position += Vector3.right * Speed * Time.deltaTime;
            transform.localRotation = Quaternion.Euler(0, 0, 0);
            dir = Vector3.right;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.position += Vector3.left * Speed * Time.deltaTime;
            transform.localRotation = Quaternion.Euler(0, 180, 0);
            dir = Vector3.left;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            //transform.position += Vector3.down * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isJump == false)
            {
                Rigidbody2D rigidbody2D = this.gameObject.GetComponent<Rigidbody2D>();
                rigidbody2D.AddForce(Vector3.up * JumpPower);
                isJump = true;
                GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (gun != null)
            {
                // 쿨타임과 상관없이 즉시 액티브 발사
                gun.ActiveShot(dir);
            }
        }

        if (transform.position.y < -4)
        {
            //Destroy(this.gameObject);
        }
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(0, 0, 100, 20), "Score:" + Score);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isJump = false;
        //Debug.Log("OnCollisionEnter2D:" + collision.gameObject.name);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        //if(collision.gameObject.name == "Plaform")
        //    GetComponent<Rigidbody2D>().velocity = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Object")
        {
            GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            Debug.Log($"{this.gameObject.name}.OnTriggerEnter2D:{collision.gameObject.name}");
        }
    }
}
