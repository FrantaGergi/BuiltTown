using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "NewSound", menuName = "Audio/Sound")]
public class SoundSO : ScriptableObject
{
    public enum Sound
    {
        Footstep,
        Jump,
        ChopTree,
        MineRock,
        WaterSplash,
        UI_Click,
        Music_MainTheme,
        // … tady si mùžeš doplòovat
    }

    [Header("Identifikátor zvuku")]
    public Sound sound;

    [Header("Audio data")]
    public AudioClip clip;
    public AudioMixerGroup mixerGroup;

    [Range(0f, 1f)]
    public float volume = 1f;

    public bool loop = false;

    [Header("3D nastavení")]
    [Range(0f, 1f)]
    public float SpatialBlend = 0f; // 0 = 2D, 1 = 3D
    public float MaxDistance = 500f;
}
