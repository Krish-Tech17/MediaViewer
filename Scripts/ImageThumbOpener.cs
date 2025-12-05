using UnityEngine;
using UnityEngine.UI;

public class ImageThumbOpener : MonoBehaviour
{
    [Header("Viewer Reference")]
    public ImageViewer imageViewer;

    [Header("Image Source")]
    public Sprite spriteImage;           // Thumbnail sprite
    public string resourcesPath;         // Optional: Resources folder loading

    private GameObject thumbnailRoot;    // The entire thumbnail object

    private void Awake()
    {
        thumbnailRoot = this.gameObject;
    }

    public void OnThumbnailClicked()
    {
        if (imageViewer == null)
        {
            Debug.LogWarning("ImageViewer reference is missing.");
            return;
        }

        // 1) Open from sprite
        if (spriteImage != null)
        {
            imageViewer.ShowSprite(spriteImage, thumbnailRoot);
            return;
        }

        // 2) Open from Resources
        if (!string.IsNullOrEmpty(resourcesPath))
        {
            imageViewer.ShowResource(resourcesPath, thumbnailRoot);
            return;
        }

        Debug.LogWarning("No sprite or resourcesPath set on ImageThumbOpener.");
    }
}
