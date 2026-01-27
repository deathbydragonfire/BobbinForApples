using System;
using UnityEngine;

public class BossUIManager : MonoBehaviour
{
    public event Action OnHealthBarReady;
    
    [Header("UI Components")]
    [SerializeField] private BossHealthBarUI healthBarUI;
    [SerializeField] private BossTitleCardUI titleCardUI;
    [SerializeField] private CrosshairUI crosshairUI;
    [SerializeField] private PlayerHealthUI playerHealthUI;
    
    private void Awake()
    {
        if (healthBarUI == null)
        {
            healthBarUI = FindFirstObjectByType<BossHealthBarUI>();
        }
        
        if (healthBarUI != null)
        {
            healthBarUI.OnFadeInComplete += HandleHealthBarFadeInComplete;
        }
        
        if (titleCardUI == null)
        {
            titleCardUI = FindFirstObjectByType<BossTitleCardUI>();
        }
        
        if (crosshairUI == null)
        {
            crosshairUI = FindFirstObjectByType<CrosshairUI>();
        }
        
        if (playerHealthUI == null)
        {
            playerHealthUI = FindFirstObjectByType<PlayerHealthUI>();
        }
    }
    
    private void OnDestroy()
    {
        if (healthBarUI != null)
        {
            healthBarUI.OnFadeInComplete -= HandleHealthBarFadeInComplete;
        }
    }
    
    private void HandleHealthBarFadeInComplete()
    {
        Debug.Log("Health bar ready - boss fight can begin");
        OnHealthBarReady?.Invoke();
    }
    
    public void StartBossEncounter()
    {
        if (crosshairUI != null)
        {
            crosshairUI.ShowCrosshair();
        }
        
        if (titleCardUI != null)
        {
            titleCardUI.PlayIntroSequence();
        }
    }
    
    public void InitializeBossHealth(float maxHealth)
    {
        if (healthBarUI != null)
        {
            healthBarUI.Initialize(maxHealth);
        }
    }
    
    public void ShowHealthBar(float maxHealth)
    {
        if (healthBarUI != null)
        {
            healthBarUI.Initialize(maxHealth);
            healthBarUI.Show();
        }
    }
    
    public void ShowHealthUI()
    {
        if (healthBarUI != null)
        {
            healthBarUI.Show();
        }
        
        if (playerHealthUI != null)
        {
            playerHealthUI.Show();
        }
    }
    
    public void UpdateHealthBar(float currentHealth)
    {
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealth(currentHealth);
        }
    }
    
    public void HideHealthBar()
    {
        if (healthBarUI != null)
        {
            healthBarUI.Hide();
        }
    }
    
    public void EndBossEncounter()
    {
        if (healthBarUI != null)
        {
            healthBarUI.Hide();
        }
        
        if (crosshairUI != null)
        {
            crosshairUI.HideCrosshair();
        }
        
        if (playerHealthUI != null)
        {
            playerHealthUI.Hide();
        }
    }
    
    public BossHealthBarUI GetHealthBarUI()
    {
        return healthBarUI;
    }
}
