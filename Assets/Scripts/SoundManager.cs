using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static SoundSO;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Mixery")]
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup environmentGroup;

    [Header("Zvukové zdroje")]
    public List<SoundSO> soundList = new();

    public bool PlayerIsInWater = false;
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
        }
    }

    public void PlayOnSource(AudioSource source, Sound sound)
    {
        SoundSO soundSO = GetSoundSO(sound);
        Debug.Log("Nìco jdu zahrát za zvuk");

        if (source == null || soundSO == null)
        {
            Debug.LogWarning($"PlayOnSource: AudioSource nebo Sound '{sound}' nenalezen.");
            return;
        }

        source.clip = soundSO.clip;
        source.outputAudioMixerGroup = soundSO.mixerGroup;
        source.volume = soundSO.volume;
        source.loop = soundSO.loop;
        source.minDistance = 0.001f;
        source.spatialBlend = soundSO.SpatialBlend;
        source.maxDistance = soundSO.MaxDistance;
        source.Play();
    }

    public void PlayOnSourceWithoutInterrupt(AudioSource source, Sound sound)
    {
        SoundSO soundSO = GetSoundSO(sound);

        if (source == null || soundSO == null)
        {
            Debug.LogWarning($"PlayOnSourceWithoutInterrupt: AudioSource nebo Sound '{sound}' nenalezen.");
            return;
        }

        source.outputAudioMixerGroup = soundSO.mixerGroup;
        source.minDistance = 0.001f;
        source.spatialBlend = soundSO.SpatialBlend;
        source.maxDistance = soundSO.MaxDistance;

        source.PlayOneShot(soundSO.clip, soundSO.volume);
    }


    public void StopSource(AudioSource source)
    {
        if (source != null && source.isPlaying)
        {
            source.Stop();
        }
    }

    public AudioClip GetAudioClipBySound(Sound sound)
    {
        SoundSO soundSO = GetSoundSO(sound);
        if (soundSO != null)
            return soundSO.clip;

        Debug.LogWarning($"Zvuk '{sound}' nebyl nalezen v seznamu.");
        return null;
    }

    private SoundSO GetSoundSO(Sound sound)
    {
        return soundList.Find(s => s.sound == sound);
    }



    public void PlayOnSourceWithoutInterrupt(AudioSource source, Sound sound, float delay = 0f)
    {
        StartCoroutine(PlayOnSourceWithoutInterruptDelayed(source, sound, delay));
    }

    private IEnumerator PlayOnSourceWithoutInterruptDelayed(AudioSource source, Sound sound, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        SoundSO soundSO = GetSoundSO(sound);

        if (source == null || soundSO == null)
        {
            Debug.LogWarning($"PlayOnSourceWithoutInterrupt: AudioSource nebo Sound '{sound}' nenalezen.");
            yield break;
        }

        source.outputAudioMixerGroup = soundSO.mixerGroup;
        source.minDistance = 0.001f;
        source.spatialBlend = soundSO.SpatialBlend;
        source.maxDistance = soundSO.MaxDistance;

        source.PlayOneShot(soundSO.clip, soundSO.volume);
    }



    public void PlayOnSource(AudioSource source, Sound sound, float delay = 0f)
    {
        StartCoroutine(PlayOnSourceDelayed(source, sound, delay));
    }

    private IEnumerator PlayOnSourceDelayed(AudioSource source, Sound sound, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        SoundSO soundSO = GetSoundSO(sound);
        Debug.Log("Nìco jdu zahrát za zvuk");

        if (source == null || soundSO == null)
        {
            Debug.LogWarning($"PlayOnSource: AudioSource nebo Sound '{sound}' nenalezen.");
            yield break;
        }

        source.clip = soundSO.clip;
        source.outputAudioMixerGroup = soundSO.mixerGroup;
        source.volume = soundSO.volume;
        source.loop = soundSO.loop;
        source.minDistance = 0.001f;
        source.spatialBlend = soundSO.SpatialBlend;
        source.maxDistance = soundSO.MaxDistance;
        source.Play();
    }
}
