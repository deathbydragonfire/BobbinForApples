using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    public static MusicManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<MusicManager>();
            }
            return instance;
        }
    }
    
    [Header("Music Players")]
    [SerializeField] private AudioSource primaryMusicSource;
    [SerializeField] private AudioSource secondaryMusicSource;
    
    [Header("Music Library")]
    [SerializeField] private List<MusicTrackMapping> musicTracks = new List<MusicTrackMapping>();
    private Dictionary<string, MusicTrack> musicDictionary = new Dictionary<string, MusicTrack>();
    
    [Header("Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float defaultFadeDuration = 2f;
    
    private AudioSource currentSource;
    private AudioSource nextSource;
    private Coroutine fadeCoroutine;
    private string currentTrackName;
    
    public float MusicVolume
    {
        get { return musicVolume; }
        set 
        { 
            musicVolume = Mathf.Clamp01(value);
            if (currentSource != null)
            {
                currentSource.volume = musicVolume;
            }
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMusicSources();
            BuildMusicDictionary();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeMusicSources()
    {
        if (primaryMusicSource == null)
        {
            primaryMusicSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (secondaryMusicSource == null)
        {
            secondaryMusicSource = gameObject.AddComponent<AudioSource>();
        }
        
        ConfigureMusicSource(primaryMusicSource);
        ConfigureMusicSource(secondaryMusicSource);
        
        currentSource = primaryMusicSource;
        nextSource = secondaryMusicSource;
    }
    
    private void ConfigureMusicSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = true;
        source.volume = 0f;
        source.spatialBlend = 0f;
        source.priority = 0;
    }
    
    private void BuildMusicDictionary()
    {
        musicDictionary.Clear();
        foreach (MusicTrackMapping mapping in musicTracks)
        {
            if (mapping.track != null && !string.IsNullOrEmpty(mapping.areaName))
            {
                musicDictionary[mapping.areaName] = mapping.track;
            }
        }
    }
    
    public void PlayMusic(string areaName)
    {
        PlayMusic(areaName, defaultFadeDuration);
    }
    
    public void PlayMusic(string areaName, float fadeDuration)
    {
        if (string.IsNullOrEmpty(areaName))
        {
            Debug.LogWarning("Area name is null or empty.");
            return;
        }
        
        if (currentTrackName == areaName)
        {
            Debug.Log($"Music for '{areaName}' is already playing.");
            return;
        }
        
        if (musicDictionary.TryGetValue(areaName, out MusicTrack track))
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            fadeCoroutine = StartCoroutine(CrossfadeMusic(track, fadeDuration));
            currentTrackName = areaName;
        }
        else
        {
            Debug.LogWarning($"Music track for area '{areaName}' not found in MusicManager.");
        }
    }
    
    public void StopMusic(float fadeDuration = 0f)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        if (fadeDuration > 0f)
        {
            fadeCoroutine = StartCoroutine(FadeOutMusic(fadeDuration));
        }
        else
        {
            if (currentSource != null && currentSource.isPlaying)
            {
                currentSource.Stop();
                currentSource.volume = 0f;
            }
        }
        
        currentTrackName = null;
    }
    
    private IEnumerator CrossfadeMusic(MusicTrack track, float fadeDuration)
    {
        nextSource.clip = track.clip;
        nextSource.volume = 0f;
        nextSource.pitch = track.pitch;
        nextSource.Play();
        
        float elapsed = 0f;
        float startVolume = currentSource.volume;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            currentSource.volume = Mathf.Lerp(startVolume, 0f, t);
            nextSource.volume = Mathf.Lerp(0f, musicVolume * track.volume, t);
            
            yield return null;
        }
        
        currentSource.Stop();
        currentSource.volume = 0f;
        nextSource.volume = musicVolume * track.volume;
        
        AudioSource temp = currentSource;
        currentSource = nextSource;
        nextSource = temp;
        
        fadeCoroutine = null;
    }
    
    private IEnumerator FadeOutMusic(float fadeDuration)
    {
        float elapsed = 0f;
        float startVolume = currentSource.volume;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            currentSource.volume = Mathf.Lerp(startVolume, 0f, t);
            
            yield return null;
        }
        
        currentSource.Stop();
        currentSource.volume = 0f;
        
        fadeCoroutine = null;
    }
    
    public void PauseMusic()
    {
        if (currentSource != null && currentSource.isPlaying)
        {
            currentSource.Pause();
        }
    }
    
    public void ResumeMusic()
    {
        if (currentSource != null && currentSource.clip != null)
        {
            currentSource.UnPause();
        }
    }
    
    public void AddMusicTrack(string areaName, MusicTrack track)
    {
        musicDictionary[areaName] = track;
        
        bool found = false;
        for (int i = 0; i < musicTracks.Count; i++)
        {
            if (musicTracks[i].areaName == areaName)
            {
                musicTracks[i] = new MusicTrackMapping { areaName = areaName, track = track };
                found = true;
                break;
            }
        }
        
        if (!found)
        {
            musicTracks.Add(new MusicTrackMapping { areaName = areaName, track = track });
        }
    }
}

[System.Serializable]
public class MusicTrackMapping
{
    public string areaName;
    public MusicTrack track;
}
