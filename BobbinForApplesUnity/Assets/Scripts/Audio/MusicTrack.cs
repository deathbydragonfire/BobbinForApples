using UnityEngine;

[CreateAssetMenu(fileName = "New Music Track", menuName = "Audio/Music Track")]
public class MusicTrack : ScriptableObject
{
    [Header("Music Clip")]
    public AudioClip clip;
    
    [Header("Playback Settings")]
    [Range(0f, 1f)]
    public float volume = 1f;
    
    [Range(0.5f, 2f)]
    public float pitch = 1f;
    
    [Header("Info")]
    [TextArea(2, 4)]
    public string description;
}
