using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SoundSO;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Zvuky")]
    public List<SoundSO> soundList = new();

    private void Awake()
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

    // ===============================
    // 3D NPC ZVUK – bez pøerušení
    // ===============================
    public void PlayOnSourceWithoutInterrupt(AudioSource source, Sound sound)
    {
        SoundSO soundSO = GetSoundSO(sound);

        if (source == null || soundSO == null)
        {
            Debug.LogError($"PlayOnSourceWithoutInterrupt: Missing AudioSource or SoundSO ({sound})");
            return;
        }

        Configure3DAudioSource(source, soundSO);
        source.PlayOneShot(soundSO.clip, soundSO.volume);
    }

    // ===============================
    // 3D NPC ZVUK – s pøerušením
    // ===============================
    public void PlayOnSource(AudioSource source, Sound sound)
    {
        SoundSO soundSO = GetSoundSO(sound);

        Debug.Log($"PlayOnSource: Playing sound {sound} on source {source.name}");

        if (source == null || soundSO == null)
        {
            Debug.LogError($"PlayOnSource: Missing AudioSource or SoundSO ({sound})");
            return;
        }

        Configure3DAudioSource(source, soundSO);

        source.clip = soundSO.clip;
        source.loop = soundSO.loop;
        source.volume = soundSO.volume;

        source.Play();
    }

    // ===============================
    // Zastavení zvuku
    // ===============================
    public void StopSource(AudioSource source)
    {
        if (source != null && source.isPlaying)
            source.Stop();
    }

    // ===============================
    // Zpoždìné pøehrání (NPC)
    // ===============================
    public void PlayOnSourceWithoutInterrupt(AudioSource source, Sound sound, float delay)
    {
        StartCoroutine(PlayDelayed(source, sound, delay, false));
    }

    public void PlayOnSource(AudioSource source, Sound sound, float delay)
    {
        StartCoroutine(PlayDelayed(source, sound, delay, true));
    }

    private IEnumerator PlayDelayed(AudioSource source, Sound sound, float delay, bool interrupt)
    {
        yield return new WaitForSeconds(delay);

        if (interrupt)
            PlayOnSource(source, sound);
        else
            PlayOnSourceWithoutInterrupt(source, sound);
    }

    // ===============================
    // SPOLEÈNÁ 3D KONFIGURACE (KLÍÈOVÉ)
    // ===============================
    private void Configure3DAudioSource(AudioSource source, SoundSO soundSO)
    {
        source.enabled = true;

        source.spatialBlend = soundSO.SpatialBlend;

        // KLÍÈOVÁ OPRAVA
        source.rolloffMode = AudioRolloffMode.Logarithmic;

        // Rozumné vzdálenosti
        source.minDistance = soundSO.MinDistance;
        source.maxDistance = soundSO.MaxDistance;

        // Bez pitch / doppler šíleností
        source.dopplerLevel = 0f;
    }

    // ===============================
    // Interní lookup
    // ===============================
    private SoundSO GetSoundSO(Sound sound)
    {
        return soundList.Find(s => s.sound == sound);
    }
}
