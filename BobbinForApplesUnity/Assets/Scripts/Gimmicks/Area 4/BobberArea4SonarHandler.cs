using System.Collections.Generic;
using UnityEngine;

public class BobberArea4SonarHandler : MonoBehaviour
{
    [Header("Bobber Puppet Parts")]
    [Tooltip("Names of GameObjects that should be illuminated (e.g., Head, Jaw, Neck, Mouth Back)")]
    [SerializeField] private string[] spritePartNames = new string[] { "Head", "Jaw", "Neck", "Mouth Back" };
    
    [Header("Additional Sprites")]
    [SerializeField] private bool includeCircleSprite = true;
    
    private List<SpriteRenderer> bobberSprites = new List<SpriteRenderer>();
    private List<Material> originalMaterials = new List<Material>();
    private bool isInArea4 = false;
    private SonarObstacleIlluminator sonarIlluminator;
    
    private void Awake()
    {
        FindBobberSprites();
        StoreOriginalMaterials();
    }
    
    private void FindBobberSprites()
    {
        bobberSprites.Clear();
        
        SpriteRenderer[] allSprites = GetComponentsInChildren<SpriteRenderer>(true);
        
        foreach (SpriteRenderer sprite in allSprites)
        {
            foreach (string partName in spritePartNames)
            {
                if (sprite.gameObject.name == partName)
                {
                    bobberSprites.Add(sprite);
                    break;
                }
            }
        }
        
        if (includeCircleSprite)
        {
            Transform circleTransform = transform.Find("Circle");
            if (circleTransform != null)
            {
                SpriteRenderer circleSprite = circleTransform.GetComponent<SpriteRenderer>();
                if (circleSprite != null && !bobberSprites.Contains(circleSprite))
                {
                    bobberSprites.Add(circleSprite);
                }
            }
        }
        
        Debug.Log($"BobberArea4SonarHandler: Found {bobberSprites.Count} sprite renderers to illuminate");
    }
    
    private void StoreOriginalMaterials()
    {
        originalMaterials.Clear();
        
        foreach (SpriteRenderer sprite in bobberSprites)
        {
            if (sprite != null)
            {
                originalMaterials.Add(sprite.sharedMaterial);
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Area4DarknessController>() != null && !isInArea4)
        {
            EnterArea4();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Area4DarknessController>() != null && isInArea4)
        {
            ExitArea4();
        }
    }
    
    private void EnterArea4()
    {
        isInArea4 = true;
        
        EnableEmissionOnSprites();
        
        sonarIlluminator = gameObject.GetComponent<SonarObstacleIlluminator>();
        if (sonarIlluminator == null)
        {
            sonarIlluminator = gameObject.AddComponent<SonarObstacleIlluminator>();
        }
        
        Debug.Log("Bobber entered Area 4 - Emission enabled on sprite parts");
    }
    
    private void ExitArea4()
    {
        isInArea4 = false;
        
        DisableEmissionOnSprites();
        
        if (sonarIlluminator != null)
        {
            Destroy(sonarIlluminator);
            sonarIlluminator = null;
        }
        
        Debug.Log("Bobber exited Area 4 - Emission disabled on sprite parts");
    }
    
    private void EnableEmissionOnSprites()
    {
        foreach (SpriteRenderer sprite in bobberSprites)
        {
            if (sprite != null)
            {
                Material mat = sprite.material;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.black);
            }
        }
    }
    
    private void DisableEmissionOnSprites()
    {
        for (int i = 0; i < bobberSprites.Count; i++)
        {
            if (bobberSprites[i] != null && i < originalMaterials.Count)
            {
                bobberSprites[i].sharedMaterial = originalMaterials[i];
            }
        }
    }
}
