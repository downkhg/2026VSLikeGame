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

    // 1. 캡슐화: 외부에서 무기 타입을 안전하게 변경하는 메서드
    public void SetGunType(GunType newGunType)
    {
        currentGunType = newGunType;
        UpdateActiveGunState();
    }

    // 2. 외부 호출용 메인 발사 메서드 (내부 하위 무기로 위임)
    public void Shot()
    {
        switch (currentGunType)
        {
            case GunType.DefaultGun:
                if (gun != null) gun.Shot(transform.right, master);
                break;

            case GunType.KunaiGun:
                if (kunaiGun != null) kunaiGun.Shot(master);
                break;

            case GunType.ShotGun:
                if (shotGun != null) shotGun.Shot(master);
                break;

            case GunType.RocketLauncher:
                if (rocketLauncher != null) rocketLauncher.Shot(master);
                break;

            case GunType.SoccerGun:
                if (soccerGun != null) soccerGun.Shot(master);
                break;

            case GunType.BlockGun:
                if (blockGun != null) blockGun.Shot();
                break;

            case GunType.LightningShield:
                // LightningShield는 Update 기반 자동 발사이므로 필요 시 수동 호출
                break;
        }
    }

    // 3. 은닉성: 외부로 노출되지 않는 내부 컴포넌트 자동 탐색 로직
    private void InitGunComponents()
    {
        if (gun == null) gun = GetComponent<Gun>();
        if (kunaiGun == null) kunaiGun = GetComponent<KunaiGun>();
        if (shotGun == null) shotGun = GetComponent<ShotGun>();
        if (rocketLauncher == null) rocketLauncher = GetComponent<RocketLauncher>();
        if (soccerGun == null) soccerGun = GetComponent<SoccerGun>();
        if (blockGun == null) blockGun = GetComponent<BlockGun>();
        if (lightningShield == null) lightningShield = GetComponent<LightningShield>();
    }

    // 4. 은닉성: Enum 조건에 따라 선택된 무기 스크립트만 enabled 상태를 켬
    private void UpdateActiveGunState()
    {
        if (gun != null) gun.enabled = (currentGunType == GunType.DefaultGun);
        if (kunaiGun != null) kunaiGun.enabled = (currentGunType == GunType.KunaiGun);
        if (shotGun != null) shotGun.enabled = (currentGunType == GunType.ShotGun);
        if (rocketLauncher != null) rocketLauncher.enabled = (currentGunType == GunType.RocketLauncher);
        if (soccerGun != null) soccerGun.enabled = (currentGunType == GunType.SoccerGun);
        if (blockGun != null) blockGun.enabled = (currentGunType == GunType.BlockGun);
        if (lightningShield != null) lightningShield.enabled = (currentGunType == GunType.LightningShield);
    }
}