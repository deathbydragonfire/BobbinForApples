using UnityEngine;

public class AreaMusicTrigger : MonoBehaviour
{
    [Header("Music Settings")]
    [Tooltip("Name of the area - must match a music track in MusicManager")]
    [SerializeField] private string areaName;
    
    [Tooltip("Fade duration when transitioning to this music")]
    [SerializeField] private float fadeDuration = 2f;
    
    [Header("Auto Setup")]
    [Tooltip("If true, will try to get area name from AreaTrigger component")]
    [SerializeField] private bool autoGetAreaName = true;
    
    private void Start()
    {
        if (autoGetAreaName)
        {
            AreaTrigger areaTrigger = GetComponent<AreaTrigger>();
            if (areaTrigger != null && !string.IsNullOrEmpty(areaTrigger.areaName))
            {
                areaName = areaTrigger.areaName;
            }
        }
    }
    
    public void PlayAreaMusic()
    {
        if (MusicManager.Instance != null && !string.IsNullOrEmpty(areaName))
        {
            MusicManager.Instance.PlayMusic(areaName, fadeDuration);
            Debug.Log($"Playing music for area: {areaName}");
        }
        else
        {
            Debug.LogWarning($"Cannot play music - MusicManager: {MusicManager.Instance != null}, AreaName: '{areaName}'");
        }
    }
    
    public void StopMusic()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.StopMusic(fadeDuration);
        }
    }
}
