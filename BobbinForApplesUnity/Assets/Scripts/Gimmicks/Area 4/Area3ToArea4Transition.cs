using System.Collections;
using UnityEngine;

public class Area3ToArea4Transition : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeToBlackDuration = 1f;
    [SerializeField] private float blackHoldDuration = 0.5f;
    [SerializeField] private float fadeFromBlackDuration = 1f;
    
    [Header("References")]
    [SerializeField] private ScreenFadeController screenFade;
    [SerializeField] private Area4DarknessController darknessController;
    [SerializeField] private Area4SonarManager sonarManager;
    [SerializeField] private GameObject area4PreventionBarrier;
    
    private bool hasTriggered;
    
    private void Awake()
    {
        if (screenFade == null)
        {
            screenFade = ScreenFadeController.Instance;
        }
        
        if (darknessController == null)
        {
            darknessController = GetComponent<Area4DarknessController>();
        }
        
        if (sonarManager == null)
        {
            sonarManager = GetComponent<Area4SonarManager>();
        }
    }
    
    public void OnPlayerEnterArea4()
    {
        if (hasTriggered)
        {
            return;
        }
        
        hasTriggered = true;
        StartCoroutine(TransitionFadeSequence());
        
        if (area4PreventionBarrier != null)
        {
            StartCoroutine(ActivateBarrierWhenPlayerBelowIt());
        }
    }
    
    private IEnumerator ActivateBarrierWhenPlayerBelowIt()
    {
        if (area4PreventionBarrier == null)
        {
            yield break;
        }
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Area3ToArea4Transition: Player not found!");
            yield break;
        }
        
        float barrierY = area4PreventionBarrier.transform.position.y;
        
        while (player.transform.position.y >= barrierY)
        {
            yield return null;
        }
        
        area4PreventionBarrier.SetActive(true);
        Debug.Log($"Area 4 Prevention barrier activated - player has crossed below Y: {barrierY}");
    }
    
    private IEnumerator TransitionFadeSequence()
    {
        if (screenFade == null)
        {
            Debug.LogWarning("Area3ToArea4Transition: ScreenFadeController not found!");
            yield break;
        }
        
        screenFade.FadeToBlack(fadeToBlackDuration);
        yield return new WaitForSeconds(fadeToBlackDuration);
        
        if (sonarManager != null)
        {
            sonarManager.TriggerInitializationWave();
        }
        
        yield return new WaitForSeconds(blackHoldDuration);
        
        screenFade.FadeFromBlack(fadeFromBlackDuration);
        yield return new WaitForSeconds(fadeFromBlackDuration);
        
        if (darknessController != null)
        {
            darknessController.EnableSonarAfterTransition();
        }
    }
}
