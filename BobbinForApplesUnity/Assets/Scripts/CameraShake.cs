using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Quaternion originalRotation;
    private bool isShaking;

    private void Awake()
    {
        originalRotation = transform.localRotation;
    }

    public void Shake(float duration, float magnitude)
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine(duration, magnitude));
        }
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        isShaking = true;
        
        Vector3 originalPosition = transform.localPosition;
        
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPosition + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        isShaking = false;
    }
}
