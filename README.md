# 📸 Image Viewer Module (Unity)

A fully reusable, independent, and touch-friendly fullscreen image viewer for Unity projects.

This module provides:

- Fullscreen image display
- Fit-to-screen scaling
- Zoom In / Zoom Out / Reset
- Pan / Drag (only when zoomed)
- Scroll-wheel zoom (Editor)
- Mobile pinch-to-zoom
- Minimize & Close
- Thumbnail restore mechanism
- Optional texture caching
- Clean and simple API for developers
- A complete demo scene

The module is designed to be workflow-agnostic—it does not depend on any host project structures, and can be used in any Unity application (AR/VR, training systems, galleries, games, tools, HR portals, etc.).

---

## 📁 Package Contents

```
ImageViewer/
│
├── Prefabs/
│   └── ImageViewer.prefab
│
├── Scripts/
│   ├── ImageViewer.cs
│   └── ImageThumbOpener.cs
│
├── Scenes/
│   └── ImageViewer_Demo.unity
│
├── UI/
│   └── (optional icons, buttons, fonts)
│
└── README.md
```

---

## 🌟 1. Features Overview

### ✔ Fullscreen Image Viewer
Displays any image at maximum clarity with proper aspect ratio.

### ✔ Fit-to-Screen Auto Scaling
Image automatically resizes to fit within the viewport at 100% zoom.

### ✔ Zoom System
- Zoom In / Zoom Out buttons
- Scroll wheel zoom (Editor)
- Pinch zoom (Mobile)
- Zoom percentage indicator (e.g., 100%, 150%)

### ✔ Pan / Drag
Drag the image when zoomed in, with boundary clamping.

### ✔ Reset View
Restores the viewer to the original "fit-to-screen" display.

### ✔ Thumbnail → Viewer → Thumbnail Flow
- Clicking a thumbnail opens the image in fullscreen.
- Closing or minimizing restores the thumbnail automatically.

### ✔ Close & Minimize Support
- **Minimize**: viewer hides & thumbnail returns, image retained
- **Close**: viewer hides, thumbnail returns, image cleared

### ✔ Optional Texture Caching
Prevents reloading the same image multiple times.

### ✔ Reusable & Standalone
No dependencies on ANY project-specific scripts or data models.

---

## 🏗 2. How to Install (In Any Unity Project)

### Step 1 — Import the Package

Drag the `ImageViewer` folder into your Unity project:

```
Assets/SubModules/ImageViewer/
```

Or import the `.unitypackage` if provided.

### Step 2 — Add the Prefab to Your Scene

In your scene, add:

```
Canvas
 └── ImageViewer.prefab
```

Ensure:
- It sits under a Canvas
- Canvas has CanvasScaler
- Prefab's CanvasGroup is assigned and enabled

### Step 3 — Place Your Thumbnail

Your thumbnail can be:
- A UI Image
- A Button
- A card or tile UI element

Add: `ImageThumbOpener.cs`

Assign fields:

| Field | Description |
|-------|-------------|
| ImageViewer | Drag ImageViewer prefab instance |
| SpriteImage | Thumbnail Sprite |
| ResourcePath | Optional (load from Resources) |

Add Button component → Wire OnClick:
```
ImageThumbOpener.OnThumbnailClicked()
```

---

## 🧪 3. Demo Scene

Open:
```
ImageViewer/Scenes/ImageViewer_Demo.unity
```

Includes:
- Working thumbnail example
- Fully configured ImageViewer prefab
- Buttons for Zoom / Reset / Minimize / Close
- A real demonstration of module usage

Perfect for onboarding new users.

---

## 🧠 4. Using the Image Viewer in Your Own Project

You can open images in three ways:

### A) Show a Sprite
```csharp
imageViewer.ShowSprite(mySprite, thumbnailObject);
```

### B) Show a Texture2D
```csharp
imageViewer.ShowTexture(myTexture2D, thumbnailObject);
```

### C) Show from Resources
```csharp
imageViewer.ShowResource("Images/MyImage", thumbnailObject);
```

---

## 🧩 5. Module Architecture

### ▶ ImageViewer.cs

Handles:
- Fullscreen display
- Fit-to-screen algorithm
- Zoom system (scroll, pinch, buttons)
- Drag & boundary clamping
- Minimize & close logic
- Thumbnail restore
- Texture caching

### ▶ ImageThumbOpener.cs

Connector for thumbnails:
- Detects thumbnail click
- Chooses correct loading method (Sprite / Texture2D / Resources)
- Stores reference to clicked thumbnail
- Restores thumbnail visibility on minimize/close

This separation keeps the module clean and reusable.

---

## 🛠 6. Important Public Methods Explained

### `ShowSprite(Sprite sprite, GameObject thumbnail)`
Opens viewer using a sprite.

### `ShowTexture(Texture2D texture, GameObject thumbnail, string cacheKey = null)`
Opens viewer using a raw texture.

### `ShowResource(string resourcePath, GameObject thumbnail)`
Loads a texture from Resources folder, caches it, and opens it.

### `MinimizeViewer()`
Hides viewer and restores the thumbnail, preserving the texture.

### `CloseViewer()`
Hides viewer, restores thumbnail, and clears texture.

### `SetCachePolicy(bool keep)`
Controls whether minimizing keeps or destroys the loaded texture.

---

## 📐 7. Required UI Setup

Assign the following in the Inspector:

| Component | Purpose |
|-----------|---------|
| rootCanvasGroup | Handles visibility & interaction |
| viewportRect | Defines image bounding area |
| displayImage | RawImage to render texture |
| closeBtn, minimizeBtn | Viewer control |
| zoomInBtn, zoomOutBtn, resetBtn | Zoom control |
| zoomPercentText | UI text showing zoom % |

### RectTransform Rules for displayImage
```
Anchor = Middle Center
Pivot  = (0.5, 0.5)
Scale  = (1,1,1)
```

---

## 🔧 8. Integration Example

```csharp
public class MediaController : MonoBehaviour
{
    public ImageViewer viewer;

    public void OpenStepImage(Sprite sprite)
    {
        viewer.ShowSprite(sprite, null);
    }

    public void OpenDownloaded(Texture2D tex)
    {
        viewer.ShowTexture(tex, null);
    }
}
```

---

## 🧯 9. Troubleshooting

### Thumbnail not clickable
- ✔ Add Button component
- ✔ Wire OnClick → ImageThumbOpener
- ✔ Ensure GraphicRaycaster is on canvas
- ✔ Ensure ImageViewer rootCanvasGroup.blocksRaycasts = false when hidden

### Image not fitting screen
- ✔ viewportRect assigned
- ✔ displayImage anchors centered
- ✔ FitToViewport() executed before opening

### Zoom not working
- ✔ currentScale <= 1 prevents dragging
- ✔ Ensure minScale = 1

### Pinch not working
- ✔ Works only on mobile device
- ✔ Ensure Input.multiTouchEnabled

---

## 🔁 10. Why This Module Is Reusable

- Zero dependencies
- Works with Sprite, Texture2D, Resources
- Thumbnail behavior optional and pluggable
- Clean API surface
- No assumptions about workflow structure
- Fully documented
- Includes demo scene for immediate understanding
- Easy to extend (animations, slideshow, transitions, etc.)

---

## 🎉 11. Conclusion

The Image Viewer module is a complete, reusable Unity component featuring:

- ✔ Advanced zoom & pan
- ✔ Fit-to-screen rendering
- ✔ Thumbnail → viewer → thumbnail flow
- ✔ Minimal integration effort
- ✔ Crystal-clear README for onboarding
- ✔ Scalable architecture for AR/VR workflows

It is built to drop into any Unity project and work instantly with minimal setup.

---
