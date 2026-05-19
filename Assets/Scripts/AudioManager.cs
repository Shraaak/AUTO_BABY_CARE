using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundData
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    public bool loop = false;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance{ get; private set; }

    [Header(" Auido Data ")]
    public List<SoundData> sounds;
    private Dictionary<string, SoundData> soundDict;

    [Header(" Audio Sources ")]
    public AudioSource bgmSource;

    [Header(" SFX Pool ")]
    public int poolSize = 100;
    private List<AudioSource> sfxPool = new List<AudioSource>();

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitDictionary();
        InitSFXPool();
    }

    void InitDictionary()
    {
        soundDict = new Dictionary<string, SoundData>();

        foreach (var sound in sounds)
        {
            soundDict[sound.name] = sound;
        }
    }

    void InitSFXPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = new GameObject("SFX_Source" + i);
            obj.transform.parent = transform;
            AudioSource source = obj.AddComponent<AudioSource>();
            sfxPool.Add(source);
        }
    }

    AudioSource GetFreeSFXSource()
    {
        foreach (var source in sfxPool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        //如果不够 自动扩容
        GameObject obj = new GameObject("SFX_Source_Extra");
        obj.transform.parent = transform;
        AudioSource sourceExtra = obj.AddComponent<AudioSource>();
        sfxPool.Add(sourceExtra);
        return sourceExtra;
    }

#region BGM
    public void PlayBGM(string id)
    {
        if (!soundDict.ContainsKey(id))
        {
            Debug.LogError($"BGM {id} not found");
            return;
        }

        SoundData sound = soundDict[id];

        bgmSource.clip = sound.clip;
        bgmSource.volume = sound.volume;
        bgmSource.loop = true;

        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }
#endregion



#region SFX
    public void PlaySFX(string id)
    {
        if (!soundDict.ContainsKey(id))
        {
            Debug.LogWarning($"SFX {id} not found");

            return;
        }

        SoundData sound = soundDict[id];

        AudioSource source = GetFreeSFXSource();
        source.clip = sound.clip;

        source.volume = sound.volume;

        source.loop = false;

        source.Play();
    }
#endregion



#region Volume
    public void SetBGMVolume(float value)
    {
        bgmSource.volume = value;
    }

    public void SetAllSFXVolume(float value)
    {
        foreach (var source in sfxPool)
        {
            source.volume = value;
        }
    }
#endregion
}
