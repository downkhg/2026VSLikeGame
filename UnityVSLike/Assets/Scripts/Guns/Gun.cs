using UnityEngine;

/// <summary>
/// 레거시 및 하위 호환성을 지원하는 Gun 클래스 (DefaultGun 상속)
/// </summary>
public class Gun : DefaultGun
{
    public float ShotPower
    {
        get => shotPower;
        set => shotPower = value;
    }

    public void Shot(Vector3 dir, Player customMaster)
    {
        if (customMaster != null) this.master = customMaster;
        Shot(dir);
    }
}
