using UnityEngine;

public class TotalGun : MonoBehaviour
{
    public enum GunType
    {
        DefaultGun,
        KunaiGun,
        ShotGun,
        RocketLauncher,
        SoccerGun,
        BlockGun,
        LightningShield
    }

    [Header("무기 타입 설정")]
    [SerializeField] private GunType currentGunType = GunType.DefaultGun;

    [Header("하위 무기 멤버 (컴포넌트 연결)")]
    [SerializeField] private Gun gun;
    [SerializeField] private KunaiGun kunaiGun;
    [SerializeField] private ShotGun shotGun;
    [SerializeField] private RocketLauncher rocketLauncher;
    [SerializeField] private SoccerGun soccerGun;
    [SerializeField] private BlockGun blockGun;
    [SerializeField] private LightningShield lightningShield;

    [SerializeField] private Player master;

    private void Awake()
    {
        master = GetComponentInParent<Player>();

        // Inspector에서 할당하지 않았을 경우 자동으로 자식/본인 객체에서 가져옴
        InitGunComponents();
    }

    private void Start()
    {
        // 시작 시 현재 설정된 무기 타입에 맞게 활성화 처리
        SetGunType(currentGunType);
    }

    // 1. 외부에서 무기 타입을 안전하게 변경하는 메서드
    public void SetGunType(GunType newGunType)
    {
        currentGunType = newGunType;
        UpdateActiveGunState();
    }

    // 2-1. 기본 발사 (플레이어 기본 정면/자동 탐색 대상 발사)
    public void Shot()
    {
        Vector3 defaultDir = transform.right;

        // 플레이어 좌우 반전(scale.x < 0) 처리 대응
        if (master != null && master.transform.localScale.x < 0)
        {
            defaultDir = Vector3.left;
        }

        Shot(defaultDir);
    }

    // 2-2. 방향(Vector3) 지정 발사
    public void Shot(Vector3 dir)
    {
        switch (currentGunType)
        {
            case GunType.DefaultGun:
                if (gun != null) gun.Shot(dir, master);
                break;

            case GunType.KunaiGun:
                if (kunaiGun != null) kunaiGun.Shot(dir, master);
                break;

            case GunType.ShotGun:
                if (shotGun != null) shotGun.Shot(dir, master);
                break;

            case GunType.RocketLauncher:
                if (rocketLauncher != null) rocketLauncher.Shot(dir, master);
                break;

            case GunType.SoccerGun:
                if (soccerGun != null) soccerGun.Shot(dir, master);
                break;

            case GunType.BlockGun:
                if (blockGun != null) blockGun.Shot(master);
                break;

            case GunType.LightningShield:
                if (lightningShield != null) lightningShield.Shot(master);
                break;
        }
    }

    private void Update()
    {
        // 디버그/테스트 편의를 위한 숫자키 1~7 무기 실시간 변경
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetGunType(GunType.DefaultGun);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SetGunType(GunType.KunaiGun);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SetGunType(GunType.ShotGun);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SetGunType(GunType.RocketLauncher);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) SetGunType(GunType.SoccerGun);
        else if (Input.GetKeyDown(KeyCode.Alpha6)) SetGunType(GunType.BlockGun);
        else if (Input.GetKeyDown(KeyCode.Alpha7)) SetGunType(GunType.LightningShield);
    }

    // 3. 내부 컴포넌트 자동 탐색 로직 (비활성화된 자식 GameObject도 포함 탐색)
    private void InitGunComponents()
    {
        if (gun == null) gun = GetComponentInChildren<Gun>(true);
        if (kunaiGun == null) kunaiGun = GetComponentInChildren<KunaiGun>(true);
        if (shotGun == null) shotGun = GetComponentInChildren<ShotGun>(true);
        if (rocketLauncher == null) rocketLauncher = GetComponentInChildren<RocketLauncher>(true);
        if (soccerGun == null) soccerGun = GetComponentInChildren<SoccerGun>(true);
        if (blockGun == null) blockGun = GetComponentInChildren<BlockGun>(true);
        if (lightningShield == null) lightningShield = GetComponentInChildren<LightningShield>(true);
    }

    // 4. 선택된 무기 GameObject 및 컴포넌트 활성화/비활성화 처리
    private void UpdateActiveGunState()
    {
        SetGunActive(gun, currentGunType == GunType.DefaultGun);
        SetGunActive(kunaiGun, currentGunType == GunType.KunaiGun);
        SetGunActive(shotGun, currentGunType == GunType.ShotGun);
        SetGunActive(rocketLauncher, currentGunType == GunType.RocketLauncher);
        SetGunActive(soccerGun, currentGunType == GunType.SoccerGun);
        SetGunActive(blockGun, currentGunType == GunType.BlockGun);
        SetGunActive(lightningShield, currentGunType == GunType.LightningShield);

        Debug.Log($"[TotalGun] 활성 무기 변경 -> {currentGunType}");
    }

    private void SetGunActive(MonoBehaviour gunComp, bool isActive)
    {
        if (gunComp != null)
        {
            // 부모인 TotalGun 오브젝트 자체가 아니라 자식 GameObject일 경우 GameObject 자체를 활성화/비활성화
            if (gunComp.gameObject != this.gameObject)
            {
                gunComp.gameObject.SetActive(isActive);
            }
            gunComp.enabled = isActive;
        }
    }
}