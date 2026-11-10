using System;
using Exhibition.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Builds the title "remove all blocks" popup (web Group 149) with the video end popup's structure:
/// dim overlay -> PopupContent (Card motion) -> shared VideoEndPopup frame + one text image for both languages.
/// Idempotent. Called by ExhibitionUIBuilder and runnable alone on 0_Idle via Configure().
/// </summary>
public static class TitleBlockPopupAuthoring
{
    public const string FramePath = "Assets/UI/UIs_Common/VideoPanel/VideoEndPopup.png";
    public const string TextPath = "Assets/UI/UIs_Common/IdlePanel/TitleBlockPopupText.png";
    // Web: popup 1394x674 centred on 1920x1080; text at (98, 260) inside it.
    private static readonly Rect FrameRect = new Rect(263, 203, 1394, 674), TextRect = new Rect(361, 463, 1197, 159);

    public static string Configure()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Stop Play mode before authoring the title popup.");
        var scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/0_Idle.unity")
        {
            if (scene.isDirty) throw new InvalidOperationException("Save scene changes before authoring the title popup.");
            scene = EditorSceneManager.OpenScene("Assets/Scenes/0_Idle.unity");
        }
        var localization = UnityEngine.Object.FindFirstObjectByType<LocalizedImageSet>();
        if (!localization) throw new InvalidOperationException("0_Idle has no ImageUI root.");
        Build(localization.transform);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Authored TitleBlockOverlay in 0_Idle.";
    }

    public static void Build(Transform root)
    {
        var overlay = root.Find("TitleBlockOverlay") as RectTransform;
        if (!overlay)
        {
            overlay = new GameObject("TitleBlockOverlay", typeof(RectTransform)).GetComponent<RectTransform>();
            overlay.SetParent(root, false);
        }
        overlay.SetAsLastSibling();
        Stretch(overlay);
        var dim = Add<Image>(overlay.gameObject);
        dim.color = new Color(91f / 255, 91f / 255, 91f / 255, .8f);
        dim.raycastTarget = false;
        var content = overlay.Find("PopupContent") as RectTransform;
        if (!content)
        {
            content = new GameObject("PopupContent", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(overlay, false);
        }
        Stretch(content);
        content.pivot = new Vector2(.5f, .5f);
        Add<CanvasGroup>(content.gameObject);
        var element = Add<UIAnimatedElement>(content.gameObject);
        var serialized = new SerializedObject(element);
        serialized.FindProperty("motion").enumValueIndex = (int)UIAnimatedElement.Motion.Card;
        serialized.FindProperty("animateOnEnable").boolValue = false;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        Picture(content, "PopupFrame", FramePath, FrameRect);
        Picture(content, "TitleBlockPopupText", TextPath, TextRect);
        overlay.gameObject.SetActive(false);

        var presentation = Add<UIPopupPresentation>(root.gameObject);
        serialized = new SerializedObject(presentation);
        serialized.FindProperty("overlay").objectReferenceValue = overlay.gameObject;
        serialized.FindProperty("backdrop").objectReferenceValue = dim;
        serialized.FindProperty("content").objectReferenceValue = element;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        var controller = Add<TitleBlockPopup>(root.gameObject);
        serialized = new SerializedObject(controller);
        serialized.FindProperty("popup").objectReferenceValue = presentation;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Picture(Transform parent, string name, string path, Rect rect)
    {
        var target = parent.Find(name) as RectTransform;
        if (!target)
        {
            target = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            target.SetParent(parent, false);
        }
        target.anchorMin = target.anchorMax = target.pivot = new Vector2(0, 1);
        target.anchoredPosition = new Vector2(rect.x, -rect.y);
        target.sizeDelta = rect.size;
        var image = Add<Image>(target.gameObject);
        image.sprite = Sprite(path);
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    private static Sprite Sprite(string path)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (!importer) throw new InvalidOperationException("Popup image is missing: " + path);
        if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single ||
            importer.mipmapEnabled || !importer.alphaIsTransparency || importer.npotScale != TextureImporterNPOTScale.None ||
            importer.textureCompression != TextureImporterCompression.Uncompressed || importer.wrapMode != TextureWrapMode.Clamp)
        {
            // Same settings as ExhibitionUIBuilder uses for every UI PNG.
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static T Add<T>(GameObject target) where T : Component => target.TryGetComponent(out T found) ? found : target.AddComponent<T>();
}
