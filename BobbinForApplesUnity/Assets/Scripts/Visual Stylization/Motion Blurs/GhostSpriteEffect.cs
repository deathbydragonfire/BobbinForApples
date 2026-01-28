using UnityEngine;
using System.Collections.Generic;

public class GhostSpriteEffect : MonoBehaviour
{
    [Header("Ghost Settings")]
    [SerializeField] private float ghostInterval = 0.1f;
    [SerializeField] private float ghostLifetime = 0.5f;
    [SerializeField] private Color ghostColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private int maxGhosts = 10;
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Activation")]
    [SerializeField] private bool alwaysActive = false;
    [SerializeField] private float minSpeedToActivate = 2f;
    [SerializeField] private bool useTransformVelocity = true;
    [SerializeField] private float velocitySmoothing = 0.15f;
    
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb2D;
    private float ghostTimer;
    private bool isActive;
    
    private Vector2 previousPosition;
    private Vector2 smoothedVelocity;
    
    private Queue<GameObject> ghostPool = new Queue<GameObject>();
    private List<GhostData> activeGhosts = new List<GhostData>();
    
    private class GhostData
    {
        public GameObject gameObject;
        public SpriteRenderer spriteRenderer;
        public float spawnTime;
    }
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb2D = GetComponent<Rigidbody2D>();
        previousPosition = transform.position;
        
        InitializeGhostPool();
    }
    
    private void InitializeGhostPool()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError($"GhostSpriteEffect on {gameObject.name}: SpriteRenderer is null! Cannot create ghost pool.");
            return;
        }
        
        for (int i = 0; i < maxGhosts; i++)
        {
            GameObject ghost = CreateGhostObject();
            ghost.SetActive(false);
            ghostPool.Enqueue(ghost);
        }
        
        Debug.Log($"GhostSpriteEffect on {gameObject.name}: Initialized pool with {maxGhosts} ghosts. AlwaysActive={alwaysActive}");
    }
    
    private GameObject CreateGhostObject()
    {
        GameObject ghost = new GameObject("Ghost");
        ghost.transform.SetParent(transform.parent);
        
        SpriteRenderer ghostSR = ghost.AddComponent<SpriteRenderer>();
        ghostSR.sortingLayerID = spriteRenderer.sortingLayerID;
        ghostSR.sortingOrder = spriteRenderer.sortingOrder;
        
        return ghost;
    }
    
    private void Update()
    {
        UpdateActiveState();
        
        if (isActive)
        {
            ghostTimer += Time.deltaTime;
            
            if (ghostTimer >= ghostInterval)
            {
                SpawnGhost();
                ghostTimer = 0f;
            }
        }
        
        UpdateActiveGhosts();
    }
    
    private void UpdateActiveState()
    {
        if (alwaysActive)
        {
            isActive = true;
            return;
        }
        
        float speed = GetCurrentSpeed();
        isActive = speed >= minSpeedToActivate;
    }
    
    private float GetCurrentSpeed()
    {
        if (useTransformVelocity || rb2D == null)
        {
            Vector2 currentPosition = transform.position;
            Vector2 frameVelocity = (currentPosition - previousPosition) / Time.deltaTime;
            smoothedVelocity = Vector2.Lerp(smoothedVelocity, frameVelocity, velocitySmoothing);
            previousPosition = currentPosition;
            return smoothedVelocity.magnitude;
        }
        else
        {
            return rb2D.linearVelocity.magnitude;
        }
    }
    
    private void SpawnGhost()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError($"GhostSpriteEffect on {gameObject.name}: SpriteRenderer is null! Cannot spawn ghost.");
            return;
        }
        
        if (ghostPool.Count == 0)
        {
            RecycleOldestGhost();
        }
        
        GameObject ghostObj = ghostPool.Dequeue();
        SpriteRenderer ghostSR = ghostObj.GetComponent<SpriteRenderer>();
        
        ghostObj.transform.position = transform.position;
        ghostObj.transform.rotation = transform.rotation;
        ghostObj.transform.localScale = transform.localScale;
        
        ghostSR.sprite = spriteRenderer.sprite;
        ghostSR.flipX = spriteRenderer.flipX;
        ghostSR.flipY = spriteRenderer.flipY;
        ghostSR.color = ghostColor;
        
        ghostObj.SetActive(true);
        
        GhostData ghostData = new GhostData
        {
            gameObject = ghostObj,
            spriteRenderer = ghostSR,
            spawnTime = Time.time
        };
        
        activeGhosts.Add(ghostData);
    }
    
    private void UpdateActiveGhosts()
    {
        for (int i = activeGhosts.Count - 1; i >= 0; i--)
        {
            GhostData ghost = activeGhosts[i];
            float age = Time.time - ghost.spawnTime;
            
            if (age >= ghostLifetime)
            {
                ReturnGhostToPool(ghost);
                activeGhosts.RemoveAt(i);
            }
            else
            {
                float normalizedAge = age / ghostLifetime;
                float alpha = fadeOutCurve.Evaluate(normalizedAge) * ghostColor.a;
                
                Color currentColor = ghost.spriteRenderer.color;
                currentColor.a = alpha;
                ghost.spriteRenderer.color = currentColor;
            }
        }
    }
    
    private void RecycleOldestGhost()
    {
        if (activeGhosts.Count > 0)
        {
            GhostData oldest = activeGhosts[0];
            ReturnGhostToPool(oldest);
            activeGhosts.RemoveAt(0);
        }
    }
    
    private void ReturnGhostToPool(GhostData ghost)
    {
        ghost.gameObject.SetActive(false);
        ghostPool.Enqueue(ghost.gameObject);
    }
    
    public void ActivateEffect(float duration)
    {
        StartCoroutine(TemporaryActivation(duration));
    }
    
    private System.Collections.IEnumerator TemporaryActivation(float duration)
    {
        bool previousState = alwaysActive;
        alwaysActive = true;
        yield return new WaitForSeconds(duration);
        alwaysActive = previousState;
    }
    
    public void ClearAllGhosts()
    {
        for (int i = activeGhosts.Count - 1; i >= 0; i--)
        {
            ReturnGhostToPool(activeGhosts[i]);
        }
        activeGhosts.Clear();
    }
    
    public void SetActive(bool active)
    {
        alwaysActive = active;
        if (!active)
        {
            ClearAllGhosts();
        }
    }
    
    public void SetGhostSettings(float interval, float lifetime, Color color, int maxCount)
    {
        ghostInterval = interval;
        ghostLifetime = lifetime;
        ghostColor = color;
        
        if (maxCount != maxGhosts)
        {
            ClearAllGhosts();
            maxGhosts = maxCount;
            
            while (ghostPool.Count > 0)
            {
                GameObject ghost = ghostPool.Dequeue();
                if (ghost != null) Destroy(ghost);
            }
            
            InitializeGhostPool();
        }
    }
    
    public void SetMinSpeedThreshold(float speed)
    {
        minSpeedToActivate = speed;
    }
    
    private void OnDisable()
    {
        ClearAllGhosts();
    }
}
