using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float strokeForce = 50f;
    [SerializeField] private float strokeTorque = 100f;
    [SerializeField] private float kickForce = 150f;

    [Header("2D Plane Settings")]
    [SerializeField] private Vector3 planeNormal = Vector3.forward;

    private Rigidbody rigidBody;
    private Keyboard keyboard;

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
    }

    private void ApplyRightStroke()
    {
        Vector3 forwardDirection = transform.up;
        rigidBody.AddForce(forwardDirection * strokeForce, ForceMode.Impulse);
        rigidBody.AddTorque(GetTorqueAxis() * -strokeTorque, ForceMode.Impulse);
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
