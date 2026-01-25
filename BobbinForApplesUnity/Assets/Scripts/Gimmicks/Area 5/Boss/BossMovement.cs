using UnityEngine;

public class BossMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float phase1Speed = 2f;
    [SerializeField] private float phase2Speed = 3f;
    [SerializeField] private float phase3Speed = 4f;
    [SerializeField] private float phase3DashSpeed = 6f;
    
    [Header("Path Settings")]
    [SerializeField] private float circleRadius = 5f;
    [SerializeField] private Vector3 arenaCenter = Vector3.zero;
    [SerializeField] private float figureEightWidth = 6f;
    [SerializeField] private float figureEightHeight = 4f;
    
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
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }
    
    public void StartMovement(int phase)
    {
        currentPhase = phase;
        isMoving = true;
        movementTimer = 0f;
        isDashing = false;
        isPaused = false;
        
        Debug.Log($"Boss movement started - Phase {phase}");
    }
    
    public void StopMovement()
    {
        isMoving = false;
    }
    
    private void Update()
    {
        if (!isMoving) return;
        
        switch (currentPhase)
        {
            case 1:
                CircleMovement();
                break;
            case 2:
                FigureEightMovement();
                break;
            case 3:
                ErraticDashMovement();
                break;
        }
    }
    
    private void CircleMovement()
    {
        movementTimer += Time.deltaTime * phase1Speed;
        
        float angle = movementTimer * Mathf.Deg2Rad;
        float x = arenaCenter.x + Mathf.Cos(angle) * circleRadius;
        float y = arenaCenter.y + Mathf.Sin(angle) * circleRadius;
        
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
    }
}
