using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Reusable fullscreen Image Viewer with:
/// - Fit to screen
/// - Zoom (buttons, scroll, pinch)
/// - Pan/drag when zoomed
/// - Minimize & Close
/// - Thumbnail restore
/// - Optional caching
/// Clean API: ShowSprite(), ShowTexture(), ShowResource()
/// </summary>
public class ImageViewer : MonoBehaviour, IPointerDownHandler, IDragHandler, IScrollHandler
{
    [Header("UI References (assign all in Inspector)")]
    public CanvasGroup rootCanvasGroup;
    public RectTransform viewportRect;
    public RawImage displayImage;

    public Button closeBtn;
    public Button minimizeBtn;
    public Button zoomInBtn;
    public Button zoomOutBtn;
    public Button resetBtn;

    public TMP_Text zoomPercentText;

    [Header("Zoom Settings")]
    public float minScale = 1f;       // normalized zoom
    public float maxScale = 4f;
    public float zoomStep = 0.25f;
    public float wheelZoomSpeed = 0.05f;

    [Header("Minimize Behavior")]
    public bool keepTextureOnMinimize = true;

    public System.Action onMinimized;
    public System.Action onClosed;

    // Internal state
    private float currentScale = 1f;
    private float baseFitScale = 1f;
    private bool isOpen = false;

    private Vector2 dragStartPointer;
    private Vector2 imageStartAnchoredPos;

    private GameObject activeThumbnail;
    private string currentCacheKey;

    // Global static Texture cache
    private static readonly System.Collections.Generic.Dictionary<string, Texture2D> textureCache =
        new System.Collections.Generic.Dictionary<string, Texture2D>();


    /*─────────────────────────────────────────
      INITIALIZATION
    ─────────────────────────────────────────*/
    private void Awake()
    {
        if (rootCanvasGroup == null)
            rootCanvasGroup = GetComponent<CanvasGroup>();

        HookButtons();
        HideInstant();
    }

    private void HookButtons()
    {
        if (closeBtn != null)     closeBtn.onClick.AddListener(CloseViewer);
        if (minimizeBtn != null)  minimizeBtn.onClick.AddListener(MinimizeViewer);
        if (zoomInBtn != null)    zoomInBtn.onClick.AddListener(() => ChangeZoom(currentScale + zoomStep));
        if (zoomOutBtn != null)   zoomOutBtn.onClick.AddListener(() => ChangeZoom(currentScale - zoomStep));
        if (resetBtn != null)     resetBtn.onClick.AddListener(ResetZoom);
    }


    /*─────────────────────────────────────────
      PUBLIC API — CLEAN & CLEAR
    ─────────────────────────────────────────*/

    /// <summary>Open viewer using a Sprite.</summary>
    public void ShowSprite(Sprite sprite, GameObject thumbnail = null, string cacheKey = null)
    {
        if (sprite == null) return;

        Texture2D tex = sprite.texture as Texture2D;
        if (tex == null)
        {
            Debug.LogWarning("[ImageViewer] Sprite texture is not Texture2D. Cannot display.");
            return;
        }

        ShowTexture(tex, thumbnail, cacheKey ?? ("sprite:" + sprite.name));
    }

    /// <summary>Open viewer using a Texture2D.</summary>
    public void ShowTexture(Texture2D tex, GameObject thumbnail = null, string cacheKey = null)
    {
        if (tex == null) return;

        activeThumbnail = thumbnail;
        if (activeThumbnail != null)
            activeThumbnail.SetActive(false);

        currentCacheKey = cacheKey;

        displayImage.texture = tex;

        PrepareOpen();
        FitToViewport();
        ShowViewer();
    }

    /// <summary>Open viewer from a Resources/ path.</summary>
    public void ShowResource(string resourcePath, GameObject thumbnail = null)
    {
        if (string.IsNullOrEmpty(resourcePath)) return;

        if (textureCache.TryGetValue(resourcePath, out Texture2D cached))
        {
            ShowTexture(cached, thumbnail, resourcePath);
            return;
        }

        Texture2D tex = Resources.Load<Texture2D>(resourcePath);
        if (tex == null)
        {
            Debug.LogWarning("[ImageViewer] No texture found at Resources/" + resourcePath);
            return;
        }

        textureCache[resourcePath] = tex;
        ShowTexture(tex, thumbnail, resourcePath);
    }


    /*─────────────────────────────────────────
      OPEN / MINIMIZE / CLOSE
    ─────────────────────────────────────────*/
    private void PrepareOpen()
    {
        isOpen = true;
        currentScale = 1f;
    }

    private void ShowViewer()
    {
        rootCanvasGroup.alpha = 1f;
        rootCanvasGroup.interactable = true;
        rootCanvasGroup.blocksRaycasts = true;
    }

    private void HideInstant()
    {
        rootCanvasGroup.alpha = 0f;
        rootCanvasGroup.interactable = false;
        rootCanvasGroup.blocksRaycasts = false;
    }

    private void MinimizeViewer()
    {
        HideInstant();
        isOpen = false;

        onMinimized?.Invoke();

        if (activeThumbnail != null)
            activeThumbnail.SetActive(true);

        if (!keepTextureOnMinimize)
            ReleaseCurrentTexture();
    }

    private void CloseViewer()
    {
        HideInstant();
        isOpen = false;

        onClosed?.Invoke();

        if (activeThumbnail != null)
            activeThumbnail.SetActive(true);

        ReleaseCurrentTexture();
        ResetZoom();
    }

    private void ReleaseCurrentTexture()
    {
        if (!string.IsNullOrEmpty(currentCacheKey))
        {
            textureCache.Remove(currentCacheKey);
            currentCacheKey = null;
        }

        displayImage.texture = null;
    }


    /*─────────────────────────────────────────
      FIT TO SCREEN + ZOOM SYSTEM
    ─────────────────────────────────────────*/

    /// <summary>Fits the loaded image into the viewport at perfect scale.</summary>
    private void FitToViewport()
    {
        Texture2D tex = displayImage.texture as Texture2D;
        if (tex == null) return;

        float texW = tex.width;
        float texH = tex.height;

        float viewW = viewportRect.rect.width;
        float viewH = viewportRect.rect.height;

        RectTransform rt = displayImage.rectTransform;

        rt.sizeDelta = new Vector2(texW, texH);

        baseFitScale = Mathf.Min(viewW / texW, viewH / texH);

        currentScale = 1f;
        float actualScale = baseFitScale * currentScale;

        rt.localScale = new Vector3(actualScale, actualScale, 1f);
        rt.anchoredPosition = Vector2.zero;

        UpdateZoomLabel();
    }

    private void ChangeZoom(float newScale)
    {
        if (!isOpen) return;

        currentScale = Mathf.Clamp(newScale, minScale, maxScale);

        float actualScale = baseFitScale * currentScale;

        RectTransform rt = displayImage.rectTransform;
        rt.localScale = new Vector3(actualScale, actualScale, 1f);

        ClampImagePosition();
        UpdateZoomLabel();
    }

    private void ResetZoom()
    {
        Texture2D tex = displayImage.texture as Texture2D;
        if (tex == null) return;

        currentScale = 1f;

        float actualScale = baseFitScale * currentScale;
        RectTransform rt = displayImage.rectTransform;

        rt.localScale = new Vector3(actualScale, actualScale, 1f);
        rt.anchoredPosition = Vector2.zero;

        UpdateZoomLabel();
    }

    private void UpdateZoomLabel()
    {
        if (zoomPercentText != null)
            zoomPercentText.text = Mathf.RoundToInt(currentScale * 100f) + "%";
    }

    private void ClampImagePosition()
    {
        RectTransform rt = displayImage.rectTransform;

        Vector2 viewSize = viewportRect.rect.size;
        Vector2 imgSize = Vector2.Scale(rt.sizeDelta, rt.localScale);

        float limitX = Mathf.Max((imgSize.x - viewSize.x) / 2f, 0f);
        float limitY = Mathf.Max((imgSize.y - viewSize.y) / 2f, 0f);

        Vector2 pos = rt.anchoredPosition;
        pos.x = Mathf.Clamp(pos.x, -limitX, limitX);
        pos.y = Mathf.Clamp(pos.y, -limitY, limitY);

        rt.anchoredPosition = pos;
    }


    /*─────────────────────────────────────────
      DRAG / SCROLL / PINCH
    ─────────────────────────────────────────*/
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isOpen || currentScale <= 1f) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            viewportRect, eventData.position, eventData.pressEventCamera, out dragStartPointer);

        imageStartAnchoredPos = displayImage.rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isOpen || currentScale <= 1f) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            viewportRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

        Vector2 delta = localPoint - dragStartPointer;
        displayImage.rectTransform.anchoredPosition = imageStartAnchoredPos + delta;

        ClampImagePosition();
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (!isOpen) return;

        if (Mathf.Abs(eventData.scrollDelta.y) > 0.01f)
            ChangeZoom(currentScale + eventData.scrollDelta.y * wheelZoomSpeed);
    }

    private void Update()
    {
        if (!isOpen) return;

        // Pinch zoom
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 prev0 = t0.position - t0.deltaPosition;
            Vector2 prev1 = t1.position - t1.deltaPosition;

            float prevDist = (prev0 - prev1).magnitude;
            float currDist = (t0.position - t1.position).magnitude;

            float delta = currDist - prevDist;

            ChangeZoom(currentScale + delta * 0.005f);
        }
    }
}
