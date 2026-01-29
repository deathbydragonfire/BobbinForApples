using UnityEngine;

public class BossHeartbeatSound : MonoBehaviour
{
    public void PlayHeartbeatSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioEventType.BossHeartBeat, transform.position);
        }
    }
}
