using UnityEngine;

public class KioskInteraction : MonoBehaviour
{
    public Transform player;
    public float interactionDistance = 3f;
    public GameObject interactionText;

    [Header("Web View UI")]
    public GameObject webViewRoot;

    private PlayerController playerController;
    private WebViewWindow webViewWindow;
    private bool isWebViewOpen = false;

    void Start()
    {
        if (interactionText != null)
            interactionText.SetActive(false);

        if (webViewRoot != null)
        {
            webViewRoot.SetActive(false);
            webViewWindow = webViewRoot.GetComponent<WebViewWindow>();
        }

        if (player != null)
            playerController = player.GetComponent<PlayerController>();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        bool isPlayerNear = distance <= interactionDistance;

        if (!isWebViewOpen)
        {
            if (interactionText != null)
                interactionText.SetActive(isPlayerNear);

            if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
            {
                OpenWebView();
            }
        }
        else
        {
            if (interactionText != null)
                interactionText.SetActive(false);
        }
    }

    public void OpenWebView()
    {
        isWebViewOpen = true;

        if (interactionText != null)
            interactionText.SetActive(false);

        if (webViewRoot != null)
            webViewRoot.SetActive(true);

        if (webViewWindow == null && webViewRoot != null)
            webViewWindow = webViewRoot.GetComponent<WebViewWindow>();

        if (webViewWindow != null)
            webViewWindow.ShowWebView();

        if (playerController != null)
        {
            playerController.canControl = false;
            playerController.UnlockCursor();
        }
    }

    public void CloseWebView()
    {
        isWebViewOpen = false;

        if (webViewWindow != null)
            webViewWindow.HideWebView();

        if (webViewRoot != null)
            webViewRoot.SetActive(false);

        if (playerController != null)
        {
            playerController.canControl = true;
            playerController.LockCursor();
        }
    }
}