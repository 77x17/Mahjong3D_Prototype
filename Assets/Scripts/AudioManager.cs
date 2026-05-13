using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SoundEffect
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.5f, 1.5f)] public float pitch = 1f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Cấu hình SFX")]
    public List<SoundEffect> sfxList;
    
    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Tự động thêm AudioSource để phát SFX
        sfxSource = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// Phát hiệu ứng âm thanh theo tên đã đặt trong Inspector
    /// </summary>
    public void PlaySFX(string sfxName)
    {
        SoundEffect s = sfxList.Find(x => x.name == sfxName);
        
        if (s != null)
        {
            // PlayOneShot cho phép phát nhiều âm thanh cùng lúc (không bị ngắt tiếng cũ)
            sfxSource.pitch = s.pitch;
            sfxSource.PlayOneShot(s.clip, s.volume);
        }
        else
        {
            Debug.LogWarning("Không tìm thấy âm thanh: " + sfxName);
        }
    }
}