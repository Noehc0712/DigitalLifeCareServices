using UnityEngine;
using UnityEngine.EventSystems;

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

    // 현재 실제로 열려 있는 키오스크를 저장
    private static KioskInteraction currentOpenKiosk;

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

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PerformCloseWebView();
            }
        }
    }

    public void OpenWebView()
    {
        Debug.Log($"OpenWebView called on {gameObject.name} / ID: {GetInstanceID()}");

        isWebViewOpen = true;
        currentOpenKiosk = this;

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

    // Close 버튼은 이 함수 호출
    public void RequestCloseWebView()
    {
        Debug.Log($"RequestCloseWebView called on {gameObject.name} / ID: {GetInstanceID()}");

        // 버튼이 어떤 Kiosk를 가리키고 있든,
        // 현재 실제로 열려 있는 키오스크를 닫는다.
        if (currentOpenKiosk != null)
        {
            currentOpenKiosk.PerformCloseWebView();
        }
        else
        {
            Debug.LogWarning("No currentOpenKiosk found.");
        }
    }

    private void PerformCloseWebView()
    {
        Debug.Log($"PerformCloseWebView called on {gameObject.name} / ID: {GetInstanceID()}");

        isWebViewOpen = false;

        if (webViewWindow != null)
            webViewWindow.HideWebView();

        if (webViewRoot != null)
            webViewRoot.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (playerController != null)
        {
            playerController.canControl = true;
            playerController.LockCursor();
        }

        if (currentOpenKiosk == this)
        {
            currentOpenKiosk = null;
        }
    }
}