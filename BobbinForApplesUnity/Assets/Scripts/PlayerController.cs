using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float strokeForce = 50f;
    [SerializeField] private float strokeTorque = 100f;
    [SerializeField] private float kickForce = 150f;
    [SerializeField] private float pushOffForce = 100f;

    [Header("2D Plane Settings")]
    [SerializeField] private Vector3 planeNormal = Vector3.forward;

    private Rigidbody rigidBody;
    private Keyboard keyboard;

    private Vector3 leftSurfaceNormal = Vector3.zero;
    private Vector3 rightSurfaceNormal = Vector3.zero;
    private bool isTouchingLeftSurface = false;
    private bool isTouchingRightSurface = false;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        
        if (rigidBody == null)
        {
            Debug.LogError("PlayerController requires a Rigidbody component!");
        }
        else
        {
            ConfigureRigidbodyFor2DPlane();
        }
        
        keyboard = Keyboard.current;
    }

    private void ConfigureRigidbodyFor2DPlane()
    {
        rigidBody.useGravity = false;
        
        if (planeNormal == Vector3.forward)
        {
            rigidBody.constraints = RigidbodyConstraints.FreezePositionZ | 
                                   RigidbodyConstraints.FreezeRotationX | 
                                   RigidbodyConstraints.FreezeRotationY;
        }
        else if (planeNormal == Vector3.up)
        {
            rigidBody.constraints = RigidbodyConstraints.FreezePositionY | 
                                   RigidbodyConstraints.FreezeRotationX | 
                                   RigidbodyConstraints.FreezeRotationZ;
        }
        else if (planeNormal == Vector3.right)
        {
            rigidBody.constraints = RigidbodyConstraints.FreezePositionX | 
                                   RigidbodyConstraints.FreezeRotationY | 
                                   RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private void Update()
    {
        if (keyboard == null)
        {
            return;
        }

        HandleArmStrokes();
        HandleKick();
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 contactNormal = contact.normal;
            float dotLeft = Vector3.Dot(contactNormal, transform.right);
            float dotRight = Vector3.Dot(contactNormal, -transform.right);

            if (dotLeft > 0.5f)
            {
                isTouchingLeftSurface = true;
                leftSurfaceNormal = contactNormal;
            }

            if (dotRight > 0.5f)
            {
                isTouchingRightSurface = true;
                rightSurfaceNormal = contactNormal;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isTouchingLeftSurface = false;
        isTouchingRightSurface = false;
        leftSurfaceNormal = Vector3.zero;
        rightSurfaceNormal = Vector3.zero;
    }

    private void HandleArmStrokes()
    {
        bool leftArmPressed = keyboard.aKey.wasPressedThisFrame;
        bool rightArmPressed = keyboard.lKey.wasPressedThisFrame;

        if (leftArmPressed && rightArmPressed)
        {
            ApplyForwardStroke();
        }
        else if (leftArmPressed)
        {
            ApplyLeftStroke();
        }
        else if (rightArmPressed)
        {
            ApplyRightStroke();
        }
    }

    private void ApplyLeftStroke()
    {
        Vector3 forwardDirection = transform.up;
        rigidBody.AddForce(forwardDirection * strokeForce, ForceMode.Impulse);
        rigidBody.AddTorque(GetTorqueAxis() * strokeTorque, ForceMode.Impulse);

        if (isTouchingLeftSurface)
        {
            rigidBody.AddForce(leftSurfaceNormal * pushOffForce, ForceMode.Impulse);
        }
    }

    private void ApplyRightStroke()
    {
        Vector3 forwardDirection = transform.up;
        rigidBody.AddForce(forwardDirection * strokeForce, ForceMode.Impulse);
        rigidBody.AddTorque(GetTorqueAxis() * -strokeTorque, ForceMode.Impulse);

        if (isTouchingRightSurface)
        {
            rigidBody.AddForce(rightSurfaceNormal * pushOffForce, ForceMode.Impulse);
        }
    }

    private void ApplyForwardStroke()
    {
        Vector3 forwardDirection = transform.up;
        rigidBody.AddForce(forwardDirection * strokeForce * 2f, ForceMode.Impulse);
    }

    private void HandleKick()
    {
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            Vector3 forwardDirection = transform.up;
            rigidBody.AddForce(forwardDirection * kickForce, ForceMode.Impulse);
        }
    }

    private Vector3 GetTorqueAxis()
    {
        return planeNormal.normalized;
    }
}
