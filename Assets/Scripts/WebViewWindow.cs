using System.Collections;
using UnityEngine;

public class WebViewWindow : MonoBehaviour
{
    public RectTransform webViewArea;
    public string url = "https://uhuhjin.github.io/digital-life-care-kiosk/";

    private WebViewObject webViewObject;
    private bool hasLoaded = false;

    void Start()
    {
        webViewObject = new GameObject("WebViewObject").AddComponent<WebViewObject>();

        webViewObject.Init(
            cb: (msg) => Debug.Log("WebView cb: " + msg),
            err: (msg) => Debug.LogError("WebView error: " + msg),
            started: (msg) => Debug.Log("WebView started: " + msg),
            hooked: (msg) => Debug.Log("WebView hooked: " + msg)
        );

        webViewObject.SetVisibility(false);
    }

    public void ShowWebView()
    {
        StartCoroutine(ShowWebViewRoutine());
    }

    IEnumerator ShowWebViewRoutine()
    {
        Canvas.ForceUpdateCanvases();
        yield return null;
        yield return new WaitForEndOfFrame();

        UpdateMargins();

        if (!hasLoaded)
        {
            webViewObject.LoadURL(url);
            hasLoaded = true;
        }

        webViewObject.SetVisibility(true);
    }

    public void HideWebView()
    {
        if (webViewObject == null)
            return;

        webViewObject.SetVisibility(false);
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

    void OnDestroy()
    {
        if (webViewObject != null)
        {
            Destroy(webViewObject.gameObject);
        }
    }
}