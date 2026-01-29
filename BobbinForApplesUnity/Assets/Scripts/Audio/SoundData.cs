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
    
    [Tooltip("Time in seconds to start playing from. Negative values will delay the start.")]
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
    
    [Header("Reverb Settings")]
    [Tooltip("Apply reverb filter to this sound")]
    public bool useReverb = false;
    
    [Tooltip("Reverb zone mix level")]
    [Range(0f, 1.1f)]
    public float reverbZoneMix = 1f;
    
    [Tooltip("Dry signal level (unprocessed audio) in millibels")]
    [Range(-10000f, 0f)]
    public float dryLevel = 0f;
    
    [Tooltip("Room effect level in millibels")]
    [Range(-10000f, 0f)]
    public float room = 0f;
    
    [Tooltip("Room high-frequency effect in millibels")]
    [Range(-10000f, 0f)]
    public float roomHF = 0f;
    
    [Tooltip("Reverb level in millibels")]
    [Range(-10000f, 2000f)]
    public float reverbLevel = 0f;
    
    [Tooltip("Reflections level in millibels")]
    [Range(-10000f, 1000f)]
    public float reflectionsLevel = -10000f;
    
    [Tooltip("Reverb decay time in seconds")]
    [Range(0.1f, 20f)]
    public float decayTime = 1.49f;
    
    [Tooltip("High frequency decay ratio")]
    [Range(0.1f, 2f)]
    public float decayHFRatio = 0.83f;
    
    [Tooltip("Reverb density (0 to 100%)")]
    [Range(0f, 100f)]
    public float density = 100f;
    
    [Tooltip("Reverb diffusion (0 to 100%)")]
    [Range(0f, 100f)]
    public float diffusion = 100f;
    
    public float GetPitch()
    {
        if (randomizePitch)
        {
            return pitch + Random.Range(-pitchVariation, pitchVariation);
        }
        return pitch;
    }
}
