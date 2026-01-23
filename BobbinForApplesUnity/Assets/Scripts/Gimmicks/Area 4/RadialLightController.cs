using UnityEngine;

public class RadialLightController : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private float lightRadius = 8f;
    [SerializeField] private float lightIntensity = 1.5f;
    [SerializeField] private Color lightColor = Color.white;
    [SerializeField] private float lightFalloff = 2f;
    [SerializeField] private float minBrightness = 0f;
    
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    
    private bool isActive = false;
    
    private static readonly int PlayerLightPositionID = Shader.PropertyToID("_PlayerLightPosition");
    private static readonly int PlayerLightIntensityID = Shader.PropertyToID("_PlayerLightIntensity");
    private static readonly int PlayerLightColorID = Shader.PropertyToID("_PlayerLightColor");
    
    private const string PLAYER_TAG = "Player";

    private void Awake()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    public void EnableRadialLight()
    {
        isActive = true;
        Debug.Log("Radial lighting enabled");
    }

    public void DisableRadialLight()
    {
        isActive = false;
        Shader.SetGlobalVector(PlayerLightPositionID, Vector3.zero);
        Shader.SetGlobalFloat(PlayerLightIntensityID, 0f);
        Debug.Log("Radial lighting disabled");
    }

    private void Update()
    {
        if (!isActive || playerTransform == null)
        {
            return;
        }
        
        Shader.SetGlobalVector(PlayerLightPositionID, playerTransform.position);
        Shader.SetGlobalFloat(PlayerLightIntensityID, lightIntensity);
        Shader.SetGlobalColor(PlayerLightColorID, lightColor);
    }

    private void OnDestroy()
    {
        DisableRadialLight();
    }
}
