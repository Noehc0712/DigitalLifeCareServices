using UnityEngine;
using TMPro;

public class DoorTeleport : MonoBehaviour
{
    public Transform player;
    public Transform teleportTarget;

    public TMP_Text interactionText;
    public string promptMessage = "Press E to enter";
    public float interactionDistance = 2f;
    public float teleportCooldown = 0.5f;

    private CharacterController playerController;
    private static float lastTeleportTime = -999f;

    void Start()
    {
        if (player != null)
            playerController = player.GetComponent<CharacterController>();

        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null || teleportTarget == null)
            return;

        float distance = Vector3.Distance(player.position, transform.position);
        bool canInteract = distance <= interactionDistance;
        bool cooldownFinished = Time.time >= lastTeleportTime + teleportCooldown;

        if (interactionText != null)
        {
            if (canInteract && cooldownFinished)
            {
                interactionText.text = promptMessage;
                interactionText.gameObject.SetActive(true);
            }
            else
            {
                interactionText.gameObject.SetActive(false);
            }
        }

        if (canInteract && cooldownFinished && Input.GetKeyDown(KeyCode.E))
        {
            TeleportPlayer();
        }
    }

    void TeleportPlayer()
    {
        lastTeleportTime = Time.time;

        if (playerController != null)
            playerController.enabled = false;

        player.position = teleportTarget.position;

        if (playerController != null)
            playerController.enabled = true;

        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
    }
}