using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DepthBasedCollisionFilter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float waterPlaneZPosition = 4.62f;
    [SerializeField] private float collisionDepthThreshold = 0.1f;
    [SerializeField] private bool debugMode = false;
    
    private Rigidbody rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    private void OnCollisionStay(Collision collision)
    {
        if (ShouldIgnoreCollision(collision))
        {
            FilterCollisionContacts(collision);
        }
    }
    
    private bool ShouldIgnoreCollision(Collision collision)
    {
        Transform root = collision.transform.root;
        
        if (root.name.Contains("Arena") || 
            root.name.Contains("Boss") || 
            root.name.Contains("Bobbdra") ||
            root.name == "Bobber")
        {
            return false;
        }
        
        return true;
    }
    
    private void FilterCollisionContacts(Collision collision)
    {
        bool hasValidContact = false;
        Vector3 validNormalSum = Vector3.zero;
        int validContactCount = 0;
        
        foreach (ContactPoint contact in collision.contacts)
        {
            float contactZ = contact.point.z;
            
            if (contactZ <= waterPlaneZPosition + collisionDepthThreshold)
            {
                hasValidContact = true;
                validNormalSum += contact.normal;
                validContactCount++;
                
                if (debugMode)
                {
                    Debug.DrawRay(contact.point, contact.normal * 0.5f, Color.green, 0.1f);
                }
            }
            else
            {
                if (debugMode)
                {
                    Debug.DrawRay(contact.point, contact.normal * 0.5f, Color.red, 0.1f);
                    Debug.Log($"Ignoring contact at z={contactZ:F2} (threshold={waterPlaneZPosition})");
                }
            }
        }
        
        if (!hasValidContact && collision.contactCount > 0)
        {
            Vector3 separationDirection = (transform.position - collision.transform.position).normalized;
            separationDirection.z = 0;
            
            float separationForce = 5f;
            rb.AddForce(separationDirection * separationForce, ForceMode.VelocityChange);
            
            if (debugMode)
            {
                Debug.Log($"No valid contacts with {collision.gameObject.name}, applying separation force");
            }
        }
    }
    
    public void SetWaterPlaneZPosition(float zPos)
    {
        waterPlaneZPosition = zPos;
    }
}
