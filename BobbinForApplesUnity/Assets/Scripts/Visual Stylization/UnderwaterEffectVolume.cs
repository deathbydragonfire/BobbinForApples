using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class UnderwaterEffectVolume : VolumeComponent
{
    [Header("Color Tint")]
    [Tooltip("Color of shallow water areas")]
    public ColorParameter shallowColor = new ColorParameter(new Color(0.2f, 0.6f, 0.8f, 1f));
    
    [Tooltip("Color of deep water areas")]
    public ColorParameter deepColor = new ColorParameter(new Color(0.05f, 0.3f, 0.5f, 1f));
    
    [Tooltip("Strength of the color tint effect")]
    public ClampedFloatParameter tintStrength = new ClampedFloatParameter(0.5f, 0f, 1f);
    
    [Header("Wave Distortion")]
    [Tooltip("Speed of the wave animation")]
    public FloatParameter waveSpeed = new FloatParameter(1.0f);
    
    [Tooltip("Frequency of the wave pattern")]
    public FloatParameter waveFrequency = new FloatParameter(20.0f);
    
    [Tooltip("Amplitude/strength of the wave distortion")]
    public FloatParameter waveAmplitude = new FloatParameter(0.003f);
    
    [Header("Caustics")]
    [Tooltip("Speed of the caustics animation")]
    public FloatParameter causticsSpeed = new FloatParameter(0.5f);
    
    [Tooltip("Scale of the caustics pattern")]
    public FloatParameter causticsScale = new FloatParameter(5.0f);
    
    [Tooltip("Intensity/brightness of the caustics")]
    public ClampedFloatParameter causticsIntensity = new ClampedFloatParameter(0.3f, 0f, 1f);
    
    [Header("Master Control")]
    [Tooltip("Overall intensity of the underwater effect (0 = off, 1 = full)")]
    public ClampedFloatParameter effectIntensity = new ClampedFloatParameter(0f, 0f, 1f);
}
