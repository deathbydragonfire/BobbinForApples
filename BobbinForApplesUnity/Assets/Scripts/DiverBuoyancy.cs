using UnityEngine;

public class DiverBuoyancy : MonoBehaviour
{
    [Header("Buoyancy Settings")]
    [Tooltip("Positive values push upward, negative values push downward")]
    public float buoyancyValue = 0f;
    
    [Tooltip("Multiplier to scale the buoyancy effect")]
    public float buoyancyCoefficient = 10f;

    private Rigidbody rigidBody;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        
        if (rigidBody == null)
        {
            Debug.LogError("DiverBuoyancy requires a Rigidbody component!");
        }
    }

    private void FixedUpdate()
    {
        if (rigidBody != null)
        {
            ApplyBuoyancyForce();
        }
    }

    private void ApplyBuoyancyForce()
    {
        Vector3 buoyancyForce = Vector3.up * buoyancyValue * buoyancyCoefficient;
        rigidBody.AddForce(buoyancyForce, ForceMode.Force);
    }
}
