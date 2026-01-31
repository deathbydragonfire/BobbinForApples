using System.Collections;
using UnityEngine;

public class Area4BossTransitionManager : MonoBehaviour
{
    private static Area4BossTransitionManager instance;
    public static Area4BossTransitionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<Area4BossTransitionManager>();
            }
            return instance;
        }
    }
    
    [Header("UI References")]
    [SerializeField] private ScreenFadeController screenFade;
    [SerializeField] private TypingTextUI typingText;
    
    [Header("Audio References")]
    [SerializeField] private SoundData bobbdraCinematicIntro;
    [SerializeField] private MusicTrack bossFightMusic;
    
    [Header("Arena Animation")]
    [SerializeField] private ArenaOutlineDrawAnimation arenaDrawAnimation;
    
    [Header("Area 4 Systems")]
    [SerializeField] private Area4SonarManager sonarManager;
    [SerializeField] private GameObject bobber;
    
    [Header("Text Messages")]
    [SerializeField] private string firstMessage = "What Happened?";
    [SerializeField] private string secondMessage = "What's that sound?";
    [SerializeField] private float textDelayAfterBlack = 2f;
    
    [Header("Timing")]
    [SerializeField] private float fadeToBlackDuration = 1.5f;
    [SerializeField] private float fadeFromBlackDuration = 2f;
    
    private bool transitionInProgress;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        if (screenFade == null)
        {
            screenFade = FindFirstObjectByType<ScreenFadeController>();
        }
        
        if (typingText == null)
        {
            typingText = FindFirstObjectByType<TypingTextUI>();
        }
        
        if (sonarManager == null)
        {
            sonarManager = FindFirstObjectByType<Area4SonarManager>();
        }
    }
    
    public void StartTransition(GameObject player, Vector3 arenaPosition, BossArenaManager arenaManager)
    {
        if (transitionInProgress)
        {
            Debug.LogWarning("Transition already in progress!");
            return;
        }
        
        StartCoroutine(TransitionSequence(player, arenaPosition, arenaManager));
    }
    
    private IEnumerator TransitionSequence(GameObject player, Vector3 arenaPosition, BossArenaManager arenaManager)
    {
        transitionInProgress = true;
        
        if (sonarManager != null)
        {
            sonarManager.PauseSonar();
            Debug.Log("Paused sonar system for transition");
        }
        
        if (bobber != null)
        {
            bobber.SetActive(false);
            Debug.Log("Bobber deactivated for boss transition");
        }
        
        Debug.Log("Starting Area 4 to Boss Arena transition...");
        
        Camera mainCamera = Camera.main;
        Vector3 cameraStartPosition = Vector3.zero;
        
        if (mainCamera != null)
        {
            cameraStartPosition = mainCamera.transform.position;
            Debug.Log($"Camera starting position captured: {cameraStartPosition}");
        }
        
        if (arenaManager != null)
        {
            arenaManager.DisableAutoTrigger();
            
            AreaTrigger area5Trigger = arenaManager.GetComponent<AreaTrigger>();
            if (area5Trigger != null)
            {
                area5Trigger.enabled = false;
                Debug.Log("Area 5 music trigger disabled");
            }
        }
        
        if (screenFade != null)
        {
            screenFade.FadeToBlack(fadeToBlackDuration);
            yield return new WaitForSeconds(fadeToBlackDuration);
        }
        
        Debug.Log("Screen is black - teleporting player");
        TeleportPlayer(player, arenaPosition);
        
        if (mainCamera != null)
        {
            Vector3 preFadePosition = new Vector3(cameraStartPosition.x, -795f, cameraStartPosition.z);
            mainCamera.transform.position = preFadePosition;
            Debug.Log($"Camera moved to pre-fade position: {preFadePosition}");
        }
        
        yield return new WaitForSeconds(textDelayAfterBlack);
        
        if (typingText != null)
        {
            Debug.Log("Displaying first message");
            bool firstMessageComplete = false;
            typingText.DisplayText(firstMessage, () => firstMessageComplete = true);
            
            while (!firstMessageComplete)
            {
                yield return null;
            }
            
            Debug.Log("Displaying second message");
            bool secondMessageComplete = false;
            typingText.DisplayText(secondMessage, () => secondMessageComplete = true);
            
            while (!secondMessageComplete)
            {
                yield return null;
            }
            
            typingText.StopTypingSound();
            yield return new WaitForSeconds(0.1f);
        }
        
        if (bobbdraCinematicIntro != null && AudioManager.Instance != null)
        {
            Debug.Log("Playing Bobbdra cinematic intro sound with high priority");
            AudioManager.Instance.PlaySoundWithPriority(bobbdraCinematicIntro, 0);
        }
        
        if (screenFade != null)
        {
            Debug.Log("Fading from black to reveal arena");
            screenFade.FadeFromBlack(fadeFromBlackDuration);
        }
        
        if (arenaManager != null)
        {
            Debug.Log("Starting camera zoom during fade");
            arenaManager.StartCameraZoomDuringFade(fadeFromBlackDuration);
        }
        
        yield return new WaitForSeconds(fadeFromBlackDuration);
        
        if (arenaManager != null)
        {
            Debug.Log("Triggering boss encounter");
            arenaManager.TriggerBossEncounter();
        }
        
        transitionInProgress = false;
        
        Debug.Log("Transition sequence complete!");
    }
    
    private void TeleportPlayer(GameObject player, Vector3 arenaPosition)
    {
        if (player == null) return;
        
        Debug.Log($"Teleporting player to arena at {arenaPosition}");
        
        player.transform.position = arenaPosition;
        
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            Debug.Log("Player velocity reset to zero");
        }
        
        DiverBuoyancy buoyancy = player.GetComponent<DiverBuoyancy>();
        if (buoyancy != null)
        {
            buoyancy.enabled = false;
            Debug.Log("Player buoyancy disabled for boss fight");
        }
    }
}
