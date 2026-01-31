using UnityEngine;
using System.Collections;

public class BusterPowerupEffect : MonoBehaviour
{
    public static BusterPowerupEffect Instance { get; private set; }
    
    [Header("Buster Settings")]
    [SerializeField] private float effectDuration = 5f;
    
    private bool isActive = false;
    
    public bool IsActive => isActive;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    public void Activate()
    {
        if (!isActive)
        {
            StartCoroutine(ActivateEffect());
        }
    }
    
    private IEnumerator ActivateEffect()
    {
        isActive = true;
        
        yield return new WaitForSeconds(effectDuration);
        
        isActive = false;
    }
}
