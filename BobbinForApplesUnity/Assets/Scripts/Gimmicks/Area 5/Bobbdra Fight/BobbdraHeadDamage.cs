using UnityEngine;

public class BobbdraHeadDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private bool damageOnContact = true;
    [SerializeField] private float damageCooldown = 1f;
    
    private float lastDamageTime = -999f;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!damageOnContact)
        {
            return;
        }
        
        if (Time.time - lastDamageTime < damageCooldown)
        {
            return;
        }
        
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage();
            lastDamageTime = Time.time;
            Debug.Log($"Bobbdra head dealt damage to player at {transform.parent?.name}");
        }
    }
    
    public void EnableDamage()
    {
        damageOnContact = true;
    }
    
    public void DisableDamage()
    {
        damageOnContact = false;
    }
}
