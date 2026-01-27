using UnityEngine;

[CreateAssetMenu(fileName = "New Sound", menuName = "Audio/Sound Data")]
public class SoundData : ScriptableObject
{
    [Header("Audio Clip")]
    public AudioClip clip;
    
    [Header("Playback Settings")]
    [Range(0f, 1f)]
    public float volume = 1f;
    
    [Range(0f, 2f)]
    public float pitch = 1f;
    
    [Tooltip("Time in seconds to start playing from")]
    [Min(0f)]
    public float startOffset = 0f;
    
    [Header("Randomization")]
    public bool randomizePitch = false;
    
    [Range(0f, 0.5f)]
    public float pitchVariation = 0.1f;
    
    [Header("3D Sound Settings")]
    public bool is3DSound = false;
    
    [Range(0f, 500f)]
    public float minDistance = 1f;
    
    [Range(0f, 500f)]
    public float maxDistance = 50f;
    
    public float GetPitch()
    {
        if (randomizePitch)
        {
            return pitch + Random.Range(-pitchVariation, pitchVariation);
        }
        return pitch;
    }
}
