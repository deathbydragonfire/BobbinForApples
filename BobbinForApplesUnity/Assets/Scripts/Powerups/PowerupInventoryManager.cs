using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PowerupInventoryManager : MonoBehaviour
{
    public static PowerupInventoryManager Instance { get; private set; }
    
    [Header("UI Reference")]
    [SerializeField] private PowerupUI powerupUI;
    
    private List<PowerupType> inventory = new List<PowerupType>();
    private Dictionary<PowerupType, Sprite> powerupSprites = new Dictionary<PowerupType, Sprite>();
    private int currentIndex = 0;
    private PowerupType activeEffect = PowerupType.None;
    private Keyboard keyboard;
    
    public PowerupType ActiveEffect => activeEffect;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        keyboard = Keyboard.current;
    }
    
    private void Update()
    {
        if (keyboard == null || inventory.Count == 0)
        {
            return;
        }
        
        if (keyboard.qKey.wasPressedThisFrame)
        {
            CyclePowerupBackward();
        }
        
        if (keyboard.eKey.wasPressedThisFrame)
        {
            CyclePowerupForward();
        }
        
        if (keyboard.fKey.wasPressedThisFrame)
        {
            ActivateCurrentPowerup();
        }
    }
    
    public void AddPowerup(PowerupType type, SpriteRenderer spriteRenderer)
    {
        if (type == PowerupType.None)
        {
            return;
        }
        
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            powerupSprites[type] = spriteRenderer.sprite;
        }
        
        if (type == PowerupType.Heavy)
        {
            ActivateHeavyEffect();
        }
        else
        {
            inventory.Add(type);
            
            if (inventory.Count == 1)
            {
                currentIndex = 0;
            }
            
            UpdateUI();
        }
    }
    
    private void CyclePowerupForward()
    {
        if (inventory.Count <= 1)
        {
            return;
        }
        
        currentIndex = (currentIndex + 1) % inventory.Count;
        UpdateUI();
    }
    
    private void CyclePowerupBackward()
    {
        if (inventory.Count <= 1)
        {
            return;
        }
        
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = inventory.Count - 1;
        }
        
        UpdateUI();
    }
    
    private void ActivateCurrentPowerup()
    {
        if (inventory.Count == 0)
        {
            return;
        }
        
        PowerupType typeToActivate = inventory[currentIndex];
        
        switch (typeToActivate)
        {
            case PowerupType.Boost:
                BoostPowerupEffect.Instance?.Activate();
                break;
            case PowerupType.Buster:
                BusterPowerupEffect.Instance?.Activate();
                break;
            case PowerupType.Freeze:
                FreezePowerupEffect.Instance?.Activate();
                break;
        }
        
        RemoveCurrentPowerup();
    }
    
    private void RemoveCurrentPowerup()
    {
        if (inventory.Count == 0)
        {
            return;
        }
        
        if (powerupUI != null)
        {
            powerupUI.PlayConsumeAnimation(currentIndex, () =>
            {
                inventory.RemoveAt(currentIndex);
                
                if (inventory.Count > 0)
                {
                    if (currentIndex >= inventory.Count)
                    {
                        currentIndex = inventory.Count - 1;
                    }
                }
                else
                {
                    currentIndex = 0;
                }
                
                UpdateUI();
            });
        }
        else
        {
            inventory.RemoveAt(currentIndex);
            
            if (inventory.Count > 0)
            {
                if (currentIndex >= inventory.Count)
                {
                    currentIndex = inventory.Count - 1;
                }
            }
            else
            {
                currentIndex = 0;
            }
            
            UpdateUI();
        }
    }
    
    private void ActivateHeavyEffect()
    {
        activeEffect = PowerupType.Heavy;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            DiverBuoyancy diverBuoyancy = player.GetComponent<DiverBuoyancy>();
            if (diverBuoyancy != null)
            {
                diverBuoyancy.buoyancyValue -= 6f;
                Debug.Log($"Heavy powerup collected! Buoyancy increased from {diverBuoyancy.buoyancyValue - 1f} to {diverBuoyancy.buoyancyValue}");
            }
        }
    }
    
    public void ClearAllPowerups()
    {
        inventory.Clear();
        currentIndex = 0;
        
        if (activeEffect == PowerupType.Heavy)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                DiverBuoyancy diverBuoyancy = player.GetComponent<DiverBuoyancy>();
                if (diverBuoyancy != null)
                {
                    diverBuoyancy.buoyancyValue = 0f;
                }
            }
            
            activeEffect = PowerupType.None;
        }
        
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (powerupUI != null)
        {
            List<Sprite> sprites = new List<Sprite>();
            foreach (PowerupType type in inventory)
            {
                if (powerupSprites.ContainsKey(type))
                {
                    sprites.Add(powerupSprites[type]);
                }
                else
                {
                    sprites.Add(null);
                }
            }
            
            powerupUI.UpdateDisplay(sprites, currentIndex);
        }
    }
}
