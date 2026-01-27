using UnityEngine;
using UnityEngine.Events;

public class AudioEventTrigger : MonoBehaviour
{
    [Header("Sound Configuration")]
    [SerializeField] private AudioEventType eventType;
    [SerializeField] private SoundData customSound;
    
    [Header("Trigger Settings")]
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private bool playOnEnable = false;
    [SerializeField] private bool use3DPosition = false;
    
    [Header("Events")]
    public UnityEvent onSoundPlayed;
    
    private void Start()
    {
        if (playOnStart)
        {
            PlaySound();
        }
    }
    
    private void OnEnable()
    {
        if (playOnEnable)
        {
            PlaySound();
        }
    }
    
    public void PlaySound()
    {
        if (customSound != null)
        {
            if (use3DPosition)
            {
                AudioManager.Instance.PlaySound(customSound, transform.position);
            }
            else
            {
                AudioManager.Instance.PlaySound(customSound);
            }
        }
        else
        {
            if (use3DPosition)
            {
                AudioManager.Instance.PlaySound(eventType, transform.position);
            }
            else
            {
                AudioManager.Instance.PlaySound(eventType);
            }
        }
        
        onSoundPlayed?.Invoke();
    }
    
    public void PlaySoundWithDelay(float delay)
    {
        Invoke(nameof(PlaySound), delay);
    }
}
