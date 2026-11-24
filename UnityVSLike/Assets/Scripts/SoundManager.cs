using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 초심자 친화적 오디오 매니저 (싱글톤)
/// Resources/Sounds 폴더에 위치한 효과음 애셋을 로드하여 상황에 맞게 재생합니다.
/// </summary>
public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SoundManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("@SoundManager");
                    _instance = go.AddComponent<SoundManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("=== 오디오 소스 ===")]
    public AudioSource sfxSource;
    public AudioSource bgmSource;

    [Header("=== 볼륨 설정 ===")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.5f;

    // 캐싱된 오디오 클립 사전 (반복 로드 방지)
    private Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // SFX 오디오 소스 생성 및 설정
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        // BGM 오디오 소스 생성 및 설정
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        PreloadClips();
    }

    /// <summary>
    /// 게임 시작 시 사용 빈도가 높은 효과음들을 미리 로드하여 메모리에 캐싱합니다.
    /// </summary>
    private void PreloadClips()
    {
        string[] soundNames = new string[]
        {
            "cannon_01", "cannon_02", "card", "crossbow",
            "electronic_01", "electronic_02", "heal", "laser_01",
            "machine_gun", "magic_01", "magic_02", "magic_03"
        };

        for (int i = 0; i < soundNames.Length; i++)
        {
            GetOrLoadClip(soundNames[i]);
        }
    }

    /// <summary>
    /// Resources/Sounds 폴더에서 오디오 클립을 가져오거나 새로 로드합니다.
    /// </summary>
    public AudioClip GetOrLoadClip(string clipName)
    {
        if (string.IsNullOrEmpty(clipName)) return null;

        if (clipCache.ContainsKey(clipName))
        {
            return clipCache[clipName];
        }

        AudioClip loadedClip = Resources.Load<AudioClip>($"Sounds/{clipName}");
        if (loadedClip != null)
        {
            clipCache.Add(clipName, loadedClip);
        }
        else
        {
            Debug.LogWarning($"[SoundManager] 오디오 클립을 찾을 수 없습니다: Resources/Sounds/{clipName}.wav");
        }

        return loadedClip;
    }

    /// <summary>
    /// 단발성 효과음(SFX)을 재생합니다.
    /// </summary>
    public void PlaySFX(string clipName, float volumeMultiplier = 1f)
    {
        AudioClip clip = GetOrLoadClip(clipName);
        if (clip != null && sfxSource != null)
        {
            float finalVolume = masterVolume * sfxVolume * volumeMultiplier;
            sfxSource.PlayOneShot(clip, finalVolume);
        }
    }

    /// <summary>
    /// 총기 종류(GunType)에 맞는 고유 발사 효과음을 재생합니다.
    /// </summary>
    public void PlayShootSound(GunType gunType)
    {
        switch (gunType)
        {
            case GunType.DefaultGun:
                PlaySFX("cannon_01", 0.8f);
                break;
            case GunType.KunaiGun:
                PlaySFX("crossbow", 0.9f);
                break;
            case GunType.ShotGun:
                PlaySFX("cannon_02", 1.0f);
                break;
            case GunType.RocketLauncher:
                PlaySFX("electronic_02", 1.0f);
                break;
            case GunType.SoccerGun:
                PlaySFX("magic_01", 0.9f);
                break;
            case GunType.BlockGun:
                PlaySFX("magic_03", 0.9f);
                break;
            case GunType.LightningShield:
                PlaySFX("electronic_01", 1.0f);
                break;
            default:
                PlaySFX("cannon_01", 0.7f);
                break;
        }
    }

    /// <summary>
    /// 레벨업 카드 선택 효과음
    /// </summary>
    public void PlayLevelUpSound()
    {
        PlaySFX("card", 1.0f);
    }

    /// <summary>
    /// 경험치 보석(ExpGem) 습득 효과음
    /// </summary>
    public void PlayGemSound()
    {
        PlaySFX("heal", 0.7f);
    }

    /// <summary>
    /// 플레이어 피격 효과음
    /// </summary>
    public void PlayHitSound()
    {
        PlaySFX("laser_01", 0.8f);
    }
}
