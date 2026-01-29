using UnityEngine;

public class BubbleParticleSetup : MonoBehaviour
{
    public Material bubbleMaterial;
    public float emissionRate = 20f;
    public float screenWidth = 20f;
    public float riseSpeed = 2f;
    public float turbulenceStrength = 0.5f;
    public float particleLifetime = 10f;
    public Vector2 bubbleSizeRange = new Vector2(0.3f, 0.8f);
    public float bounceSpeed = 2f;
    public float bounceAmount = 0.15f;

    private ParticleSystem ps;

    private void Awake()
    {
        SetupParticleSystem();
        StopEmission();
    }
    
    public void StartEmission()
    {
        if (ps != null)
        {
            ps.Play();
        }
    }
    
    public void StopEmission()
    {
        if (ps != null)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void SetupParticleSystem()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps == null)
        {
            ps = gameObject.AddComponent<ParticleSystem>();
        }

        var main = ps.main;
        main.startLifetime = particleLifetime;
        main.startSpeed = riseSpeed;
        main.startSize = new ParticleSystem.MinMaxCurve(bubbleSizeRange.x, bubbleSizeRange.y);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.startColor = Color.white;
        main.gravityModifier = -0.1f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 200;

        var emission = ps.emission;
        emission.rateOverTime = emissionRate;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(screenWidth, 0.1f, 1f);

        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(riseSpeed * 0.8f, riseSpeed * 1.2f);

        var noiseModule = ps.noise;
        noiseModule.enabled = true;
        noiseModule.strength = turbulenceStrength;
        noiseModule.frequency = 0.5f;
        noiseModule.scrollSpeed = 0.3f;
        noiseModule.damping = true;
        noiseModule.octaveCount = 2;
        noiseModule.quality = ParticleSystemNoiseQuality.High;

        AnimationCurve bounceCurve = new AnimationCurve();
        bounceCurve.AddKey(0f, 1f);
        bounceCurve.AddKey(0.25f, 1f - bounceAmount);
        bounceCurve.AddKey(0.5f, 1f);
        bounceCurve.AddKey(0.75f, 1f + bounceAmount);
        bounceCurve.AddKey(1f, 1f);
        
        for (int i = 0; i < bounceCurve.keys.Length; i++)
        {
            bounceCurve.SmoothTangents(i, 0f);
        }

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.separateAxes = true;
        
        ParticleSystem.MinMaxCurve xCurve = new ParticleSystem.MinMaxCurve(bounceSpeed, bounceCurve);
        ParticleSystem.MinMaxCurve yCurve = new ParticleSystem.MinMaxCurve(bounceSpeed, bounceCurve);
        
        sizeOverLifetime.x = xCurve;
        sizeOverLifetime.y = yCurve;
        sizeOverLifetime.z = new ParticleSystem.MinMaxCurve(1f);

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null && bubbleMaterial != null)
        {
            renderer.material = bubbleMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortMode = ParticleSystemSortMode.Distance;
        }
    }
}
