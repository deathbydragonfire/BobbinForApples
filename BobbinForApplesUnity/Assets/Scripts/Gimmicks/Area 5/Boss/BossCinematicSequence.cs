using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossCinematicSequence : MonoBehaviour
{
    [Header("Screen Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private Color fadeColor = Color.white;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    
    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float introZoomSize = 4f;
    [SerializeField] private float arenaZoomSize = 9f;
    [SerializeField] private float zoomInDuration = 1.5f;
    [SerializeField] private float holdDuration = 4f;
    [SerializeField] private float zoomOutDuration = 2f;
    
    [Header("Boss Spawn Settings")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform arenaCenter;
    
    [Header("Auto Setup")]
    [SerializeField] private bool createFadeImageOnAwake = true;
    
    private Canvas fadeCanvas;
    private bool isSequenceActive;
    private GameObject spawnedBoss;
    
    public GameObject SpawnedBoss => spawnedBoss;
    
    private void Awake()
    {
        if (createFadeImageOnAwake && fadeImage == null)
        {
            CreateFadeCanvas();
        }
        
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            fadeImage.enabled = false;
        }
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }
    
    private void CreateFadeCanvas()
    {
        GameObject canvasObject = new GameObject("BossCinematicFadeCanvas");
        canvasObject.transform.SetParent(transform);
        
        fadeCanvas = canvasObject.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 10000;
        
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObject.AddComponent<GraphicRaycaster>();
        
        GameObject imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(canvasObject.transform);
        
        fadeImage = imageObject.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        fadeImage.enabled = false;
        
        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
    }
    
    public void PlayCinematicSequence(GameObject previewBoss, BossArenaManager arenaManager = null)
    {
        if (isSequenceActive)
        {
            Debug.LogWarning("BossCinematicSequence: Sequence already active");
            return;
        }
        
        StartCoroutine(CinematicSequence(previewBoss, arenaManager));
    }
    
    private IEnumerator CinematicSequence(GameObject previewBoss, BossArenaManager arenaManager)
    {
        isSequenceActive = true;
        
        Debug.Log("Boss Cinematic: Starting sequence - fading to white immediately");
        
        yield return StartCoroutine(FadeToColor(fadeInDuration));
        Debug.Log("Boss Cinematic: Fade to white complete");
        
        if (previewBoss != null)
        {
            Debug.Log("Boss Cinematic: Destroying preview boss");
            Destroy(previewBoss);
        }
        
        Debug.Log("Boss Cinematic: Spawning real boss");
        SpawnRealBoss();
        Debug.Log($"Boss Cinematic: Real boss spawned, spawnedBoss is {(spawnedBoss != null ? "valid" : "null")}");
        
        yield return StartCoroutine(FadeFromColor(fadeOutDuration));
        Debug.Log("Boss Cinematic: Fade from white complete");
        
        if (mainCamera != null && spawnedBoss != null)
        {
            Debug.Log("Boss Cinematic: Starting camera zoom sequence");
            yield return StartCoroutine(CameraZoomSequence());
            Debug.Log("Boss Cinematic: Camera zoom sequence complete");
        }
        else
        {
            Debug.LogWarning($"Boss Cinematic: Skipping camera zoom - mainCamera: {mainCamera != null}, spawnedBoss: {spawnedBoss != null}");
        }
        
        Debug.Log("Boss Cinematic: Sequence complete, starting combat");
        
        if (arenaManager != null)
        {
            arenaManager.OnBossCinematicComplete(spawnedBoss);
        }
        else
        {
            BossController bossController = spawnedBoss.GetComponent<BossController>();
            if (bossController != null)
            {
                bossController.StartCombat();
            }
        }
        
        isSequenceActive = false;
    }
    
    private void SpawnRealBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("Boss Cinematic: Boss prefab not assigned");
            return;
        }
        
        Vector3 spawnPosition = arenaCenter != null ? arenaCenter.position : Vector3.zero;
        spawnPosition.z = 0f;
        
        spawnedBoss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        
        BossMovement bossMovement = spawnedBoss.GetComponent<BossMovement>();
        if (bossMovement != null && arenaCenter != null)
        {
            bossMovement.SetArenaCenter(arenaCenter.position);
        }
        
        Debug.Log($"Boss Cinematic: Boss spawned at {spawnPosition}");
    }
    
    private IEnumerator CameraZoomSequence()
    {
        CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.enabled = false;
        }
        
        Vector3 bossPosition = spawnedBoss.transform.position;
        Vector3 zoomPosition = new Vector3(bossPosition.x, bossPosition.y, mainCamera.transform.position.z);
        Vector3 startPosition = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;
        
        Debug.Log("Boss Cinematic: Zooming in on boss");
        float elapsed = 0f;
        while (elapsed < zoomInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomInDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            mainCamera.transform.position = Vector3.Lerp(startPosition, zoomPosition, smoothT);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, introZoomSize, smoothT);
            
            yield return null;
        }
        
        mainCamera.transform.position = zoomPosition;
        mainCamera.orthographicSize = introZoomSize;
        
        Debug.Log($"Boss Cinematic: Holding for {holdDuration} seconds");
        yield return new WaitForSeconds(holdDuration);
        
        Debug.Log("Boss Cinematic: Zooming out to arena view");
        Vector3 arenaCenterPos = arenaCenter != null ? arenaCenter.position : Vector3.zero;
        Vector3 arenaPosition = new Vector3(arenaCenterPos.x, arenaCenterPos.y, mainCamera.transform.position.z);
        elapsed = 0f;
        while (elapsed < zoomOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomOutDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            mainCamera.transform.position = Vector3.Lerp(zoomPosition, arenaPosition, smoothT);
            mainCamera.orthographicSize = Mathf.Lerp(introZoomSize, arenaZoomSize, smoothT);
            
            yield return null;
        }
        
        mainCamera.transform.position = arenaPosition;
        mainCamera.orthographicSize = arenaZoomSize;
        
        Debug.Log("Boss Cinematic: Camera sequence complete");
    }
    
    private IEnumerator FadeToColor(float duration)
    {
        if (fadeImage == null) yield break;
        
        fadeImage.enabled = true;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            Color currentColor = fadeColor;
            currentColor.a = Mathf.Lerp(0f, 1f, t);
            fadeImage.color = currentColor;
            
            yield return null;
        }
        
        Color finalColor = fadeColor;
        finalColor.a = 1f;
        fadeImage.color = finalColor;
    }
    
    private IEnumerator FadeFromColor(float duration)
    {
        if (fadeImage == null) yield break;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            Color currentColor = fadeColor;
            currentColor.a = Mathf.Lerp(1f, 0f, t);
            fadeImage.color = currentColor;
            
            yield return null;
        }
        
        Color finalColor = fadeColor;
        finalColor.a = 0f;
        fadeImage.color = finalColor;
        fadeImage.enabled = false;
    }
    
    public bool IsSequenceActive()
    {
        return isSequenceActive;
    }
}
