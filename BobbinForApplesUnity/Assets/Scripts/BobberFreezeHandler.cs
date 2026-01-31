using UnityEngine;
using System.Collections;

public class BobberFreezeHandler : MonoBehaviour
{
    [Header("Freeze Settings")]
    [SerializeField] private Color freezeColor = Color.blue;
    
    private Rigidbody bobberRigidbody;
    private Renderer[] bobberRenderers;
    private Material[] originalMaterials;
    private Material[] freezeMaterials;
    private bool isFrozen = false;
    private Vector3 storedVelocity;
    private Vector3 storedAngularVelocity;
    
    private void Awake()
    {
        bobberRigidbody = GetComponent<Rigidbody>();
        
        bobberRenderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[bobberRenderers.Length];
        freezeMaterials = new Material[bobberRenderers.Length];
        
        for (int i = 0; i < bobberRenderers.Length; i++)
        {
            originalMaterials[i] = bobberRenderers[i].material;
            
            freezeMaterials[i] = new Material(originalMaterials[i]);
            freezeMaterials[i].color = freezeColor;
        }
    }
    
    public void Freeze(float duration)
    {
        if (!isFrozen)
        {
            StartCoroutine(FreezeCoroutine(duration));
        }
    }
    
    private IEnumerator FreezeCoroutine(float duration)
    {
        isFrozen = true;
        
        if (bobberRigidbody != null)
        {
            storedVelocity = bobberRigidbody.linearVelocity;
            storedAngularVelocity = bobberRigidbody.angularVelocity;
            
            bobberRigidbody.linearVelocity = Vector3.zero;
            bobberRigidbody.angularVelocity = Vector3.zero;
            bobberRigidbody.isKinematic = true;
        }
        
        SetMaterials(freezeMaterials);
        
        yield return new WaitForSeconds(duration);
        
        if (bobberRigidbody != null)
        {
            bobberRigidbody.isKinematic = false;
            bobberRigidbody.linearVelocity = storedVelocity;
            bobberRigidbody.angularVelocity = storedAngularVelocity;
        }
        
        SetMaterials(originalMaterials);
        
        isFrozen = false;
    }
    
    private void SetMaterials(Material[] materials)
    {
        for (int i = 0; i < bobberRenderers.Length; i++)
        {
            if (bobberRenderers[i] != null && materials[i] != null)
            {
                bobberRenderers[i].material = materials[i];
            }
        }
    }
}
