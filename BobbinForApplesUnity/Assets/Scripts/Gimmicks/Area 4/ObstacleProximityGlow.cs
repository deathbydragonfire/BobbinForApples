using UnityEngine;
using System.Collections.Generic;

public class ObstacleProximityGlow : MonoBehaviour
{
    [Header("Glow Settings")]
    [SerializeField] private float glowRadius = 8f;
    [SerializeField] private Color glowColor = Color.white;
    [SerializeField] private float maxGlowIntensity = 2f;
    [SerializeField] private AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    
    [Header("Performance")]
    [SerializeField] private float updateInterval = 0.1f;
    
    private Transform playerTransform;
    private List<ObstacleGlowData> obstacles = new List<ObstacleGlowData>();
    private float updateTimer = 0f;
    
    private class ObstacleGlowData
    {
        public MeshRenderer renderer;
        public Material material;
        public Color originalEmission;
        public bool hasEmission;
    }
    
    private const string EMISSION_COLOR_PROPERTY = "_EmissionColor";
    private const string PLAYER_TAG = "Player";

    private void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag(PLAYER_TAG);
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    public void EnableGlow()
    {
        FindAndPrepareObstacles();
        enabled = true;
    }

    public void DisableGlow()
    {
        RestoreOriginalMaterials();
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
            
            ObstacleGlowData data = new ObstacleGlowData
            {
                renderer = renderer,
                material = mat,
                hasEmission = mat.HasProperty(EMISSION_COLOR_PROPERTY)
            };
            
            if (data.hasEmission)
            {
                data.originalEmission = mat.GetColor(EMISSION_COLOR_PROPERTY);
                mat.EnableKeyword("_EMISSION");
            }
            
            obstacles.Add(data);
        }
        
        Debug.Log($"Prepared {obstacles.Count} obstacles for proximity glow");
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
            UpdateObstacleGlow();
        }
    }

    private void UpdateObstacleGlow()
    {
        Vector3 playerPos = playerTransform.position;
        
        foreach (ObstacleGlowData data in obstacles)
        {
            if (data.renderer == null || data.material == null)
            {
                continue;
            }
            
            float distance = Vector3.Distance(playerPos, data.renderer.transform.position);
            
            if (distance <= glowRadius)
            {
                float normalizedDistance = distance / glowRadius;
                float glowStrength = falloffCurve.Evaluate(normalizedDistance);
                
                if (data.hasEmission)
                {
                    Color emissionColor = glowColor * (maxGlowIntensity * glowStrength);
                    data.material.SetColor(EMISSION_COLOR_PROPERTY, emissionColor);
                }
            }
            else
            {
                if (data.hasEmission)
                {
                    data.material.SetColor(EMISSION_COLOR_PROPERTY, Color.black);
                }
            }
        }
    }

    private void RestoreOriginalMaterials()
    {
        foreach (ObstacleGlowData data in obstacles)
        {
            if (data.material != null && data.hasEmission)
            {
                data.material.SetColor(EMISSION_COLOR_PROPERTY, data.originalEmission);
            }
        }
        
        obstacles.Clear();
    }

    private void OnDestroy()
    {
        RestoreOriginalMaterials();
    }
}
