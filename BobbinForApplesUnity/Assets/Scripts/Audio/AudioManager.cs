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
        
        if (soundData.startOffset >= 0f)
        {
            source.time = soundData.startOffset;
            source.Play();
            float duration = CalculateSoundDuration(soundData);
            StartCoroutine(ReturnSourceToPool(source, duration));
        }
        else
        {
            StartCoroutine(PlayWithDelay(source, soundData, -soundData.startOffset));
        }
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
        
        if (soundData.startOffset >= 0f)
        {
            source.time = soundData.startOffset;
            source.Play();
            float duration = CalculateSoundDuration(soundData);
            StartCoroutine(ReturnSourceToPool(source, duration));
        }
        else
        {
            StartCoroutine(PlayWithDelay(source, soundData, -soundData.startOffset));
        }
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
        
        if (soundData.startOffset >= 0f)
        {
            source.time = soundData.startOffset;
            source.Play();
            float duration = CalculateSoundDuration(soundData);
            StartCoroutine(ReturnSourceToPool(source, duration));
        }
        else
        {
            StartCoroutine(PlayWithDelay(source, soundData, -soundData.startOffset));
        }
        
        return source;
    }
    
    public AudioSource PlaySoundWithPriority(SoundData soundData, int priority)
    {
        if (soundData == null || soundData.clip == null)
        {
            Debug.LogWarning("Attempted to play null sound data or clip.");
            return null;
        }
        
        AudioSource source = GetAvailableAudioSource();
        ConfigureAudioSource(source, soundData);
        source.priority = priority;
        
        if (soundData.startOffset >= 0f)
        {
            source.time = soundData.startOffset;
            source.Play();
            float duration = CalculateSoundDuration(soundData);
            StartCoroutine(ReturnSourceToPool(source, duration));
        }
        else
        {
            StartCoroutine(PlayWithDelay(source, soundData, -soundData.startOffset));
        }
        
        return source;
    }
    
    private float CalculateSoundDuration(SoundData soundData)
    {
        float clipDuration = soundData.clip.length - Mathf.Max(0f, soundData.startOffset);
        
        if (soundData.useReverb)
        {
            float reverbTail = soundData.decayTime * 1.5f;
            return clipDuration + reverbTail;
        }
        
        return clipDuration;
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
        source.priority = 64;
        
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
        
        AudioReverbFilter reverbFilter = source.GetComponent<AudioReverbFilter>();
        
        if (soundData.useReverb)
        {
            if (reverbFilter == null)
            {
                reverbFilter = source.gameObject.AddComponent<AudioReverbFilter>();
            }
            
            reverbFilter.enabled = true;
            reverbFilter.reverbPreset = AudioReverbPreset.User;
            source.reverbZoneMix = soundData.reverbZoneMix;
            reverbFilter.dryLevel = soundData.dryLevel;
            reverbFilter.room = soundData.room;
            reverbFilter.roomHF = soundData.roomHF;
            reverbFilter.reverbLevel = soundData.reverbLevel;
            reverbFilter.reflectionsLevel = soundData.reflectionsLevel;
            reverbFilter.decayTime = soundData.decayTime;
            reverbFilter.decayHFRatio = soundData.decayHFRatio;
            reverbFilter.density = soundData.density;
            reverbFilter.diffusion = soundData.diffusion;
        }
        else
        {
            if (reverbFilter != null)
            {
                reverbFilter.enabled = false;
            }
        }
    }
    
    private System.Collections.IEnumerator PlayWithDelay(AudioSource source, SoundData soundData, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (source != null)
        {
            source.Play();
            float duration = CalculateSoundDuration(soundData);
            StartCoroutine(ReturnSourceToPool(source, duration));
        }
    }
    
    private System.Collections.IEnumerator ReturnSourceToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (source != null)
        {
            source.Stop();
            source.clip = null;
            
            AudioReverbFilter reverbFilter = source.GetComponent<AudioReverbFilter>();
            if (reverbFilter != null)
            {
                reverbFilter.enabled = false;
            }
            
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
