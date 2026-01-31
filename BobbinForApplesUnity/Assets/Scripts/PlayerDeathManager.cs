using UnityEngine;
using TMPro;

public class PlayerDeathManager : MonoBehaviour
{
    public static PlayerDeathManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Canvas gameOverCanvas;
    [SerializeField] private TextMeshProUGUI gameOverText;

    [Header("Settings")]
    [SerializeField] private string gameOverMessage = "GAME OVER";

    private bool isDead = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (gameOverCanvas != null)
        {
            gameOverCanvas.gameObject.SetActive(false);
        }
    }

    public void TriggerDeath(string deathReason = null)
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        Time.timeScale = 0f;

        if (gameOverCanvas != null)
        {
            gameOverCanvas.gameObject.SetActive(true);
        }

        if (gameOverText != null)
        {
            string message = string.IsNullOrEmpty(deathReason) ? gameOverMessage : deathReason;
            gameOverText.text = message;
        }

        Debug.Log($"Player Death Triggered: {deathReason ?? gameOverMessage}");
    }

    public void ResetDeathState()
    {
        isDead = false;
        Time.timeScale = 1f;

        if (gameOverCanvas != null)
        {
            gameOverCanvas.gameObject.SetActive(false);
        }
    }
}
