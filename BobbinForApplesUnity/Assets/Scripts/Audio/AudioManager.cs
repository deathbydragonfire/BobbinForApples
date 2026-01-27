using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioManager>();
                
                if (instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    [Header("Audio Source Pool")]
    [SerializeField] private int poolSize = 10;
    private List<AudioSource> audioSourcePool = new List<AudioSource>();
    private Queue<AudioSource> availableSources = new Queue<AudioSource>();
    
    [Header("Sound Library")]
    [SerializeField] private List<AudioEventMapping> soundMappings = new List<AudioEventMapping>();
    private Dictionary<AudioEventType, SoundData> soundDictionary = new Dictionary<AudioEventType, SoundData>();
    
    [Header("Master Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    
    public float MasterVolume
    {
        get { return masterVolume; }
        set { masterVolume = Mathf.Clamp01(value); }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
            BuildSoundDictionary();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeAudioSources()
    {
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            audioSourcePool.Add(source);
            availableSources.Enqueue(source);
        }
    }
    
    private void BuildSoundDictionary()
    {
        soundDictionary.Clear();
        foreach (AudioEventMapping mapping in soundMappings)
        {
            if (mapping.soundData != null)
            {
                soundDictionary[mapping.eventType] = mapping.soundData;
            }
        }
    }
    
    public void PlaySound(AudioEventType eventType)
    {
        if (soundDictionary.TryGetValue(eventType, out SoundData soundData))
        {
            PlaySound(soundData);
        }
        else
        {
            Debug.LogWarning($"Sound for event type {eventType} not found in AudioManager.");
        }
    }
    
    public void PlaySound(AudioEventType eventType, Vector3 position)
    {
        if (soundDictionary.TryGetValue(eventType, out SoundData soundData))
        {
            PlaySound(soundData, position);
        }
        else
        {
            Debug.LogWarning($"Sound for event type {eventType} not found in AudioManager.");
        }
    }
    
    public void PlaySound(SoundData soundData)
    {
        if (soundData == null || soundData.clip == null)
        {
            Debug.LogWarning("Attempted to play null sound data or clip.");
            return;
        }
        
        AudioSource source = GetAvailableAudioSource();
        ConfigureAudioSource(source, soundData);
        
        source.time = soundData.startOffset;
        source.Play();
        
        StartCoroutine(ReturnSourceToPool(source, soundData.clip.length - soundData.startOffset));
    }
    
    public void PlaySound(SoundData soundData, Vector3 position)
    {
        if (soundData == null || soundData.clip == null)
        {
            Debug.LogWarning("Attempted to play null sound data or clip.");
            return;
        }
        
        AudioSource source = GetAvailableAudioSource();
        ConfigureAudioSource(source, soundData);
        
        source.transform.position = position;
        source.time = soundData.startOffset;
        source.Play();
        
        StartCoroutine(ReturnSourceToPool(source, soundData.clip.length - soundData.startOffset));
    }
    
    public AudioSource PlaySoundWithReference(AudioEventType eventType)
    {
        if (soundDictionary.TryGetValue(eventType, out SoundData soundData))
        {
            return PlaySoundWithReference(soundData);
        }
        
        Debug.LogWarning($"Sound for event type {eventType} not found in AudioManager.");
        return null;
    }
    
    public AudioSource PlaySoundWithReference(SoundData soundData)
    {
        if (soundData == null || soundData.clip == null)
        {
            Debug.LogWarning("Attempted to play null sound data or clip.");
            return null;
        }
        
        AudioSource source = GetAvailableAudioSource();
        ConfigureAudioSource(source, soundData);
        
        source.time = soundData.startOffset;
        source.Play();
        
        StartCoroutine(ReturnSourceToPool(source, soundData.clip.length - soundData.startOffset));
        
        return source;
    }
    
    private AudioSource GetAvailableAudioSource()
    {
        if (availableSources.Count > 0)
        {
            return availableSources.Dequeue();
        }
        
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        audioSourcePool.Add(newSource);
        return newSource;
    }
    
    private void ConfigureAudioSource(AudioSource source, SoundData soundData)
    {
        source.clip = soundData.clip;
        source.volume = soundData.volume * masterVolume;
        source.pitch = soundData.GetPitch();
        
        if (soundData.is3DSound)
        {
            source.spatialBlend = 1f;
            source.minDistance = soundData.minDistance;
            source.maxDistance = soundData.maxDistance;
            source.rolloffMode = AudioRolloffMode.Linear;
        }
        else
        {
            source.spatialBlend = 0f;
        }
    }
    
    private System.Collections.IEnumerator ReturnSourceToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (source != null)
        {
            source.Stop();
            source.clip = null;
            availableSources.Enqueue(source);
        }
    }
    
    public void StopAllSounds()
    {
        foreach (AudioSource source in audioSourcePool)
        {
            if (source.isPlaying)
            {
                source.Stop();
            }
        }
        
        availableSources.Clear();
        foreach (AudioSource source in audioSourcePool)
        {
            availableSources.Enqueue(source);
        }
    }
    
    public void AddSoundMapping(AudioEventType eventType, SoundData soundData)
    {
        soundDictionary[eventType] = soundData;
        
        bool found = false;
        for (int i = 0; i < soundMappings.Count; i++)
        {
            if (soundMappings[i].eventType == eventType)
            {
                soundMappings[i] = new AudioEventMapping { eventType = eventType, soundData = soundData };
                found = true;
                break;
            }
        }
        
        if (!found)
        {
            soundMappings.Add(new AudioEventMapping { eventType = eventType, soundData = soundData });
        }
    }
}

[System.Serializable]
public class AudioEventMapping
{
    public AudioEventType eventType;
    public SoundData soundData;
}
