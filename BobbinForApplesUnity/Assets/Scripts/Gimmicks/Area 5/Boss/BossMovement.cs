using UnityEngine;

public class BossMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float phase1Speed = 8f;
    [SerializeField] private float phase2Speed = 3f;
    [SerializeField] private float phase3Speed = 4f;
    [SerializeField] private float phase3DashSpeed = 6f;
    [SerializeField] private float transitionSpeed = 25f;
    
    [Header("Path Settings")]
    [SerializeField] private float circleRadius = 5f;
    [SerializeField] private Vector3 arenaCenter = Vector3.zero;
    [SerializeField] private float figureEightWidth = 6f;
    [SerializeField] private float figureEightHeight = 4f;
    
    [Header("Phase 1 Edge Movement")]
    [SerializeField] private BoxCollider arenaCollider;
    [SerializeField] private float edgePadding = 2f;
    
    [Header("Phase 3 Dash Settings")]
    [SerializeField] private float dashInterval = 3f;
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float pauseDuration = 1f;
    
    private int currentPhase = 1;
    private float movementTimer;
    private bool isMoving;
    private Vector3 dashTarget;
    private bool isDashing;
    private float dashTimer;
    private float pauseTimer;
    private bool isPaused;
    private Rigidbody rb;
    
    private Vector3[] edgeCorners;
    private int currentEdgeCorner;
    private bool isTransitioning;
    private Vector3 transitionTarget;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }
    
    private void CalculateEdgeCorners()
    {
        if (arenaCollider == null)
        {
            Debug.LogWarning("Arena collider not assigned! Using default corners relative to arena center.");
            edgeCorners = new Vector3[]
            {
                arenaCenter + new Vector3(-20f, 10f, 0f),
                arenaCenter + new Vector3(20f, 10f, 0f),
                arenaCenter + new Vector3(20f, -30f, 0f),
                arenaCenter + new Vector3(-20f, -30f, 0f)
            };
            return;
        }
        
        Vector3 size = arenaCollider.size;
        
        float halfWidth = (size.x * 0.5f) - edgePadding;
        float halfHeight = (size.y * 0.5f) - edgePadding;
        
        edgeCorners = new Vector3[]
        {
            arenaCenter + new Vector3(-halfWidth, halfHeight, 0f),
            arenaCenter + new Vector3(halfWidth, halfHeight, 0f),
            arenaCenter + new Vector3(halfWidth, -halfHeight, 0f),
            arenaCenter + new Vector3(-halfWidth, -halfHeight, 0f)
        };
        
        Debug.Log($"Edge corners calculated relative to arena center {arenaCenter}: TL={edgeCorners[0]}, TR={edgeCorners[1]}, BR={edgeCorners[2]}, BL={edgeCorners[3]}");
    }
    
    public void StartMovement(int phase)
    {
        currentPhase = phase;
        isMoving = true;
        movementTimer = 0f;
        isDashing = false;
        isPaused = false;
        isTransitioning = false;
        
        if (phase == 1)
        {
            if (edgeCorners == null || edgeCorners.Length == 0)
            {
                CalculateEdgeCorners();
            }
            currentEdgeCorner = 0;
            transform.position = edgeCorners[0];
            Debug.Log($"Boss positioned at first corner: {edgeCorners[0]}");
        }
        
        Debug.Log($"Boss movement started - Phase {phase}");
    }
    
    public void StopMovement()
    {
        isMoving = false;
    }
    
    private void Update()
    {
        if (!isMoving) return;
        
        if (isTransitioning)
        {
            TransitionToCenter();
            return;
        }
        
        switch (currentPhase)
        {
            case 1:
                EdgePatrolMovement();
                break;
            case 2:
                FigureEightMovement();
                break;
            case 3:
                ErraticDashMovement();
                break;
        }
    }
    
    private void EdgePatrolMovement()
    {
        int nextCorner = (currentEdgeCorner + 1) % edgeCorners.Length;
        Vector3 targetCorner = edgeCorners[nextCorner];
        
        float step = phase1Speed * Time.deltaTime;
        Vector3 newPosition = Vector3.MoveTowards(transform.position, targetCorner, step);
        
        if (rb != null)
        {
            rb.MovePosition(newPosition);
        }
        else
        {
            transform.position = newPosition;
        }
        
        if (Vector3.Distance(newPosition, targetCorner) < 0.1f)
        {
            currentEdgeCorner = nextCorner;
        }
    }
    
    public void TransitionToPhase2()
    {
        isTransitioning = true;
        isMoving = true;
        transitionTarget = arenaCenter;
        Debug.Log("Boss transitioning to center for Phase 2");
    }
    
    private void TransitionToCenter()
    {
        float step = transitionSpeed * Time.deltaTime;
        Vector3 newPosition = Vector3.MoveTowards(transform.position, transitionTarget, step);
        
        if (rb != null)
        {
            rb.MovePosition(newPosition);
        }
        else
        {
            transform.position = newPosition;
        }
        
        if (Vector3.Distance(newPosition, transitionTarget) < 0.1f)
        {
            isTransitioning = false;
            currentPhase = 2;
            movementTimer = 0f;
            Debug.Log("Boss reached center, starting Phase 2 movement");
        }
    }
    
    private void FigureEightMovement()
    {
        movementTimer += Time.deltaTime * phase2Speed;
        
        float t = movementTimer;
        float x = arenaCenter.x + Mathf.Sin(t) * figureEightWidth;
        float y = arenaCenter.y + Mathf.Sin(t * 2f) * figureEightHeight * 0.5f;
        
        Vector3 newPosition = new Vector3(x, y, 0f);
        
        if (rb != null)
        {
            rb.MovePosition(newPosition);
        }
        else
        {
            transform.position = newPosition;
        }
    }
    
    private void ErraticDashMovement()
    {
        if (isPaused)
        {
            pauseTimer += Time.deltaTime;
            if (pauseTimer >= pauseDuration)
            {
                isPaused = false;
                pauseTimer = 0f;
                SelectNewDashTarget();
            }
            return;
        }
        
        if (isDashing)
        {
            dashTimer += Time.deltaTime;
            
            float dashProgress = dashTimer / dashDuration;
            Vector3 newPosition = Vector3.Lerp(transform.position, dashTarget, dashProgress);
            
            if (rb != null)
            {
                rb.MovePosition(newPosition);
            }
            else
            {
                transform.position = newPosition;
            }
            
            if (dashTimer >= dashDuration)
            {
                isDashing = false;
                isPaused = true;
                dashTimer = 0f;
            }
        }
        else
        {
            movementTimer += Time.deltaTime;
            if (movementTimer >= dashInterval)
            {
                SelectNewDashTarget();
                movementTimer = 0f;
            }
        }
    }
    
    private void SelectNewDashTarget()
    {
        float randomX = Random.Range(-circleRadius, circleRadius);
        float randomY = Random.Range(-circleRadius * 0.5f, circleRadius * 0.5f);
        
        dashTarget = arenaCenter + new Vector3(randomX, randomY, 0f);
        dashTarget.z = transform.position.z;
        
        isDashing = true;
        dashTimer = 0f;
    }
    
    public void SetArenaCenter(Vector3 center)
    {
        arenaCenter = center;
        if (arenaCollider != null)
        {
            CalculateEdgeCorners();
        }
    }
    
    public void SetArenaCollider(BoxCollider collider)
    {
        arenaCollider = collider;
        CalculateEdgeCorners();
    }
}
