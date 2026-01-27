using UnityEngine;

public class BobberMouthController : MonoBehaviour
{
    [Header("Animator Settings")]
    [SerializeField] private Animator animator;

    [Header("Timing Settings")]
    [SerializeField] private float initialWaitTime = 3f;
    [SerializeField] private float timeInAreaToClose = 2f;
    [SerializeField] private float periodicCloseInterval = 5f;
    [SerializeField] private float mouthClosedDuration = 5f;

    [Header("Bite Area Settings")]
    [SerializeField] private SphereCollider biteAreaCollider;

    private const string IS_OPEN_PARAM = "isOpen";

    private bool isMouthOpen = true;
    private float playerInAreaTimer = 0f;
    private float periodicCloseTimer = 0f;
    private float mouthClosedTimer = 0f;
    private bool isPlayerInArea = false;

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (biteAreaCollider == null)
        {
            Debug.LogError($"BobberMouthController on {gameObject.name}: BiteArea SphereCollider not assigned.");
            enabled = false;
            return;
        }

        SetMouthState(true);
        periodicCloseTimer = initialWaitTime;
    }

    private void Update()
    {
        if (isMouthOpen)
        {
            HandleOpenMouth();
        }
        else
        {
            HandleClosedMouth();
        }
    }

    private void HandleOpenMouth()
    {
        periodicCloseTimer -= Time.deltaTime;

        if (periodicCloseTimer <= 0f)
        {
            CloseMouth("Periodic timer elapsed");
            periodicCloseTimer = periodicCloseInterval;
            return;
        }

        if (isPlayerInArea)
        {
            playerInAreaTimer += Time.deltaTime;

            if (playerInAreaTimer >= timeInAreaToClose)
            {
                CloseMouth("Player remained in bite area");
                playerInAreaTimer = 0f;
            }
        }
    }

    private void HandleClosedMouth()
    {
        mouthClosedTimer += Time.deltaTime;

        if (mouthClosedTimer >= mouthClosedDuration)
        {
            OpenMouth();
            mouthClosedTimer = 0f;
        }
    }

    private void CloseMouth(string reason)
    {
        if (!isMouthOpen) return;

        isMouthOpen = false;
        SetMouthState(false);

        Debug.Log($"[BobberMouth] Bite triggered on '{gameObject.name}'. Reason: {reason}");
    }

    private void OpenMouth()
    {
        isMouthOpen = true;
        SetMouthState(true);
        periodicCloseTimer = periodicCloseInterval;
        playerInAreaTimer = 0f;
    }

    private void SetMouthState(bool open)
    {
        animator.SetBool(IS_OPEN_PARAM, open);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInArea = true;
            Debug.Log($"[BobberMouth] Player entered bite area on '{gameObject.name}'.");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInArea = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInArea = false;
            playerInAreaTimer = 0f;
        }
    }
}
