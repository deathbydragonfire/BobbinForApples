using UnityEngine;
using System.Collections;

public class FreezePowerupEffect : MonoBehaviour
{
    public static FreezePowerupEffect Instance { get; private set; }
    
    [Header("Freeze Settings")]
    [SerializeField] private float effectDuration = 5f;
    [SerializeField] private GameObject bobberPrefab;
    
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
        if (bobberPrefab != null)
        {
            BobberFreezeHandler freezeHandler = bobberPrefab.GetComponent<BobberFreezeHandler>();
            if (freezeHandler != null)
            {
                freezeHandler.Freeze(effectDuration);
            }
        }
    }
}
