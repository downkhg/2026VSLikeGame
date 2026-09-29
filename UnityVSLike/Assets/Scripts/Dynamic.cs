using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dynamic : MonoBehaviour
{
    public float JumpPower = 10;
    public bool isJump = false;
    public int Score = 0;
    public float Speed = 1;

    [Header("=== 건 인벤토리 참조 ===")]
    public GunInventory gunInventory;

    [Header("=== 액티브 무기 (BaseGun) ===")]
    [Tooltip("사용자가 지정한 특정 무기. 쿨타임과 무관하게 X키로 즉시 발사되며, 자동 발사는 비활성화됩니다.")]
    public BaseGun gun;
    public Vector3 dir = Vector3.right;

    private void Awake()
    {
        InitGunInventoryReference();

        if (gun == null)
        {
            //gun = new BaseGun();  //
            gun =  GetComponentInChildren<BaseGun>();
        }

        if (gun != null)
        {
            // 설정된 액티브 무기는 자동 발사를 끄고 수동 액티브 전용으로 전환
            gun.isAutoFire = false;
        }
    }

    private void Start()
    {
        InitGunInventoryReference();

        // 씬 시작 시 액티브 무기가 없다면 GunInventory에서 첫 번째 총기를 가져와 액티브로 등록
        if (gun == null && gunInventory != null)
        {
            if (gunInventory.GunComponents.Count > 0)
            {
                SetActiveGun(gunInventory.GunComponents[0]);
            }
            else
            {
                BaseGun defaultGun = gunInventory.GetGun(GunType.DefaultGun);
                if (defaultGun != null)
                {
                    SetActiveGun(defaultGun);
                }
            }
        }
    }

    private void InitGunInventoryReference()
    {
        if (gunInventory == null)
        {
            gunInventory = GetComponent<GunInventory>();
            if (gunInventory == null) gunInventory = GetComponentInParent<GunInventory>();
            if (gunInventory == null) gunInventory = GetComponentInChildren<GunInventory>();
            if (gunInventory == null) gunInventory = FindFirstObjectByType<GunInventory>();
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
            Debug.Log($"[Dynamic] 🎯 액티브 무기 설정 완료: {gun.GetType().Name} (자동 발사 OFF, 쿨타임 무시 즉발 사용 가능)");
        }
    }

    /// <summary>
    /// GunType에 따라 인벤토리에서 총기를 찾아 액티브 무기로 전환하고, 인벤토리에 없으면 새로 추가하여 장착합니다.
    /// </summary>
    public void SwitchActiveGun(GunType gunType)
    {
        InitGunInventoryReference();
        if (gunInventory == null) return;

        // 1. 이미 인벤토리에 해당 무기가 있다면 액티브로 전환
        BaseGun existingGun = gunInventory.GetGun(gunType);
        if (existingGun != null)
        {
            SetActiveGun(existingGun);
            return;
        }

        // 2. 인벤토리에 없다면 새로 추가하고 반환받은 총기를 액티브로 설정
        BaseGun newGun = gunInventory.AddGunByType(gunType);
        if (newGun != null)
        {
            SetActiveGun(newGun);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 1. 숫자키 (1~7): 액티브 무기 실시간 변경
        HandleWeaponSwitchInput();

        // 2. 이동 조작
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

        // 3. 점프 조작
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

        // 4. X 키: 쿨타임과 무관하게 즉시 액티브 발사
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (gun != null)
            {
                gun.ActiveShot(dir);
            }
        }

        if (transform.position.y < -4)
        {
            //Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// 숫자키 1~7 입력 시 해당 무기로 액티브 무기를 교체합니다.
    /// </summary>
    private void HandleWeaponSwitchInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) SwitchActiveGun(GunType.DefaultGun);
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) SwitchActiveGun(GunType.KunaiGun);
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) SwitchActiveGun(GunType.ShotGun);
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) SwitchActiveGun(GunType.RocketLauncher);
        else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) SwitchActiveGun(GunType.SoccerGun);
        else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) SwitchActiveGun(GunType.BlockGun);
        else if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7)) SwitchActiveGun(GunType.LightningShield);
    }

    private void OnGUI()
    {
        string activeGunName = gun != null ? gun.GetType().Name : "없음";
        GUI.Box(new Rect(10, 85, 240, 45), $"Score: {Score}\n[Active Gun]: <color=yellow>{activeGunName}</color> (X:발사 | 1~7:무기변경)");
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
