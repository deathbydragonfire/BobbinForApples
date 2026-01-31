using UnityEngine;
using System.Collections;

public class BuoyancyPowerup : MonoBehaviour
{
    [Header("Buoyancy Effect")]
    [Tooltip("The buoyancy value to set when picked up")]
    [SerializeField] private float buoyancyAmount = -5f;
    
    [Header("Effect Duration")]
    [Tooltip("Is this effect timed or permanent?")]
    [SerializeField] private bool isTimedEffect = false;
    
    [Tooltip("Duration in seconds (only used if Timed Effect is enabled)")]
    [SerializeField] private float effectDuration = 5f;
    
    [Header("Powerup Settings")]
    [Tooltip("Should the powerup be destroyed after pickup?")]
    [SerializeField] private bool destroyOnPickup = true;
    
    [Tooltip("Tag of the player GameObject")]
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            ApplyBuoyancyEffect(other.gameObject);
            
            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }

    private void ApplyBuoyancyEffect(GameObject player)
    {
        DiverBuoyancy diverBuoyancy = player.GetComponent<DiverBuoyancy>();
        
        if (diverBuoyancy != null)
        {
            if (isTimedEffect)
            {
                StartCoroutine(ApplyTimedBuoyancy(diverBuoyancy));
            }
            else
            {
                diverBuoyancy.buoyancyValue = buoyancyAmount;
                Debug.Log($"Permanent buoyancy powerup collected! Buoyancy set to: {buoyancyAmount}");
            }
        }
        else
        {
            Debug.LogWarning("Player does not have a DiverBuoyancy component!");
        }
    }

    private IEnumerator ApplyTimedBuoyancy(DiverBuoyancy diverBuoyancy)
    {
        float originalBuoyancy = diverBuoyancy.buoyancyValue;
        
        diverBuoyancy.buoyancyValue = buoyancyAmount;
        Debug.Log($"Timed buoyancy powerup collected! Buoyancy set to: {buoyancyAmount} for {effectDuration} seconds");
        
        yield return new WaitForSeconds(effectDuration);
        
        diverBuoyancy.buoyancyValue = originalBuoyancy;
        Debug.Log($"Timed buoyancy effect expired. Buoyancy restored to: {originalBuoyancy}");
    }
}
