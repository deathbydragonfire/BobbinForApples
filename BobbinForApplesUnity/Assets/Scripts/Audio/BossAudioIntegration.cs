using UnityEngine;

public class BossAudioIntegration : MonoBehaviour
{
    public void PlayBossRoar()
    {
        AudioManager.Instance.PlaySound(AudioEventType.BossRoar, transform.position);
    }
    
    public void PlayBossBite()
    {
        AudioManager.Instance.PlaySound(AudioEventType.BossBite, transform.position);
    }
    
    public void PlayBossSweep()
    {
        AudioManager.Instance.PlaySound(AudioEventType.BossSweep, transform.position);
    }
    
    public void PlayBossDamage()
    {
        AudioManager.Instance.PlaySound(AudioEventType.BossDamage, transform.position);
    }
    
    public void PlayBossDeath()
    {
        AudioManager.Instance.PlaySound(AudioEventType.BossDeath, transform.position);
    }
    
    public void PlayAttackIndicator()
    {
        AudioManager.Instance.PlaySound(AudioEventType.AttackIndicator, transform.position);
    }
}
