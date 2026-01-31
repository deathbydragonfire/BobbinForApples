using UnityEngine;
using System.Collections.Generic;

public class ObstacleProximityReveal : MonoBehaviour
{
    [Header("Reveal Settings")]
    [SerializeField] private float revealRadius = 8f;
    [SerializeField] private AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    
    [Header("Performance")]
    [SerializeField] private float updateInterval = 0.1f;
    
    private Transform playerTransform;
    private List<ObstacleRevealData> obstacles = new List<ObstacleRevealData>();
    private float updateTimer = 0f;
    
    private class ObstacleRevealData
    {
        public MeshRenderer renderer;
        public Material material;
        public Color originalColor;
        public bool hasBaseColor;
    }
    
    private const string BASE_COLOR_PROPERTY = "_BaseColor";
    private const string COLOR_PROPERTY = "_Color";
    private const string PLAYER_TAG = "Player";

    private void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag(PLAYER_TAG);
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    public void EnableReveal()
    {
        FindAndPrepareObstacles();
        SetObstaclesToBlack();
        enabled = true;
    }

    public void DisableReveal()
    {
        RestoreOriginalColors();
        enabled = false;
    }

    private void FindAndPrepareObstacles()
    {
        obstacles.Clear();
        
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(false);
        
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.CompareTag(PLAYER_TAG))
            {
                continue;
            }
            
            Material mat = renderer.material;
            
            ObstacleRevealData data = new ObstacleRevealData
            {
                renderer = renderer,
                material = mat
            };
            
            if (mat.HasProperty(BASE_COLOR_PROPERTY))
            {
                data.originalColor = mat.GetColor(BASE_COLOR_PROPERTY);
                data.hasBaseColor = true;
            }
            else if (mat.HasProperty(COLOR_PROPERTY))
            {
                data.originalColor = mat.GetColor(COLOR_PROPERTY);
                data.hasBaseColor = false;
            }
            else
            {
                continue;
            }
            
            obstacles.Add(data);
        }
        
        Debug.Log($"Prepared {obstacles.Count} obstacles for proximity reveal");
    }

    private void SetObstaclesToBlack()
    {
        foreach (ObstacleRevealData data in obstacles)
        {
            if (data.material == null)
            {
                continue;
            }
            
            string propertyName = data.hasBaseColor ? BASE_COLOR_PROPERTY : COLOR_PROPERTY;
            data.material.SetColor(propertyName, Color.black);
        }
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            return;
        }
        
        updateTimer += Time.deltaTime;
        
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateObstacleReveal();
        }
    }

    private void UpdateObstacleReveal()
    {
        Vector3 playerPos = playerTransform.position;
        
        foreach (ObstacleRevealData data in obstacles)
        {
            if (data.renderer == null || data.material == null)
            {
                continue;
            }
            
            float distance = Vector3.Distance(playerPos, data.renderer.transform.position);
            
            Color targetColor;
            
            if (distance <= revealRadius)
            {
                float normalizedDistance = distance / revealRadius;
                float revealStrength = falloffCurve.Evaluate(normalizedDistance);
                
                targetColor = Color.Lerp(data.originalColor, Color.black, normalizedDistance);
            }
            else
            {
                targetColor = Color.black;
            }
            
            string propertyName = data.hasBaseColor ? BASE_COLOR_PROPERTY : COLOR_PROPERTY;
            data.material.SetColor(propertyName, targetColor);
        }
    }

    private void RestoreOriginalColors()
    {
        foreach (ObstacleRevealData data in obstacles)
        {
            if (data.material != null)
            {
                string propertyName = data.hasBaseColor ? BASE_COLOR_PROPERTY : COLOR_PROPERTY;
                data.material.SetColor(propertyName, data.originalColor);
            }
        }
        
        obstacles.Clear();
    }

    private void OnDestroy()
    {
        RestoreOriginalColors();
    }
}
