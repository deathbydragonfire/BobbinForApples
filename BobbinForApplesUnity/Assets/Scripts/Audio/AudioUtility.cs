using UnityEngine;

public static class AudioUtility
{
    public static void PlaySoundAtPosition(AudioEventType eventType, Vector3 position)
    {
        AudioManager.Instance.PlaySound(eventType, position);
    }
    
    public static void PlaySound2D(AudioEventType eventType)
    {
        AudioManager.Instance.PlaySound(eventType);
    }
    
    public static void PlayRandomPitchSound(AudioEventType eventType, float pitchMin = 0.9f, float pitchMax = 1.1f)
    {
        AudioSource source = AudioManager.Instance.PlaySoundWithReference(eventType);
        if (source != null)
        {
            source.pitch *= Random.Range(pitchMin, pitchMax);
        }
    }
    
    public static void PlayDelayedSound(AudioEventType eventType, float delay)
    {
        AudioDelayHelper.PlayDelayed(eventType, delay);
    }
    
    public static void PlayDelayedSound(AudioEventType eventType, Vector3 position, float delay)
    {
        AudioDelayHelper.PlayDelayed(eventType, position, delay);
    }
    
    private class AudioDelayHelper : MonoBehaviour
    {
        private static AudioDelayHelper instance;
        
        private static AudioDelayHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("AudioDelayHelper");
                    instance = go.AddComponent<AudioDelayHelper>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }
        
        public static void PlayDelayed(AudioEventType eventType, float delay)
        {
            Instance.StartCoroutine(PlayDelayedCoroutine(eventType, delay));
        }
        
        public static void PlayDelayed(AudioEventType eventType, Vector3 position, float delay)
        {
            Instance.StartCoroutine(PlayDelayedCoroutine(eventType, position, delay));
        }
        
        private static System.Collections.IEnumerator PlayDelayedCoroutine(AudioEventType eventType, float delay)
        {
            yield return new WaitForSeconds(delay);
            AudioManager.Instance.PlaySound(eventType);
        }
        
        private static System.Collections.IEnumerator PlayDelayedCoroutine(AudioEventType eventType, Vector3 position, float delay)
        {
            yield return new WaitForSeconds(delay);
            AudioManager.Instance.PlaySound(eventType, position);
        }
    }
}
