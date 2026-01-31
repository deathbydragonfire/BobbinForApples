using UnityEngine;

public class BoostPowerupEffect : MonoBehaviour
{
    public static BoostPowerupEffect Instance { get; private set; }
    
    [Header("Boost Settings")]
    [SerializeField] private float boostForce = 500f;
    [SerializeField] private bool useVelocityDirection = false;
    
    private Rigidbody playerRigidbody;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        playerRigidbody = GetComponent<Rigidbody>();
    }
    
    public void Activate()
    {
        if (playerRigidbody == null)
        {
            return;
        }
        
        Vector3 boostDirection;
        
        if (useVelocityDirection && playerRigidbody.linearVelocity.magnitude > 0.1f)
        {
            boostDirection = playerRigidbody.linearVelocity.normalized;
        }
        else
        {
            boostDirection = transform.up;
        }
        
        playerRigidbody.AddForce(boostDirection * boostForce, ForceMode.Impulse);
    }
}
