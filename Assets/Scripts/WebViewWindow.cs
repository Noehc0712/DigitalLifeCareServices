using System.Collections;
using UnityEngine;

public class WebViewWindow : MonoBehaviour
{
    public RectTransform webViewArea;
    public string url = "https://uhuhjin.github.io/digital-life-care-kiosk/";

    private WebViewObject webViewObject;
    private bool isVisible = false;

    public void ShowWebView()
    {
        Debug.Log("ShowWebView called");
        StartCoroutine(ShowWebViewRoutine());
    }

    IEnumerator ShowWebViewRoutine()
    {
        // 남아 있는 웹뷰가 있으면 제거
        if (webViewObject != null)
        {
            Destroy(webViewObject.gameObject);
            webViewObject = null;
            yield return null;
        }

        webViewObject = new GameObject("WebViewObject").AddComponent<WebViewObject>();

        webViewObject.Init(
            cb: (msg) => Debug.Log("WebView cb: " + msg),
            err: (msg) => Debug.LogError("WebView error: " + msg),
            started: (msg) => Debug.Log("WebView started: " + msg),
            hooked: (msg) => Debug.Log("WebView hooked: " + msg)
        );

        Canvas.ForceUpdateCanvases();
        yield return null;
        yield return new WaitForEndOfFrame();

        UpdateMargins();

        webViewObject.LoadURL(url);
        yield return null;

        webViewObject.SetVisibility(true);
        isVisible = true;
    }

    public void HideWebView()
    {
        Debug.Log("HideWebView called");

        isVisible = false;

        if (webViewObject != null)
        {
            webViewObject.SetVisibility(false);
            Destroy(webViewObject.gameObject);
            webViewObject = null;
        }
    }

    void LateUpdate()
    {
        if (!isVisible || webViewObject == null)
            return;

        UpdateMargins();
    }

    public void UpdateMargins()
    {
        if (webViewArea == null || webViewObject == null)
            return;

        Vector3[] corners = new Vector3[4];
        webViewArea.GetWorldCorners(corners);

        Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        Vector2 topRight = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

        int left = Mathf.RoundToInt(bottomLeft.x);
        int top = Mathf.RoundToInt(Screen.height - topRight.y);
        int right = Mathf.RoundToInt(Screen.width - topRight.x);
        int bottom = Mathf.RoundToInt(bottomLeft.y);

        webViewObject.SetMargins(left, top, right, bottom);
    }

    void OnDisable()
    {
        HideWebView();
    }

    void OnDestroy()
    {
        HideWebView();
    }
}