using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// LOGO-SPLIT-001: the NSM logo is its own image right above each screen background (web position
/// 1746,40 · 135x116), so it fades and moves with its panel exactly like the logo that used to be baked in.
/// Idempotent. ExhibitionUIBuilder calls AddAbove for every background; Configure() updates the saved scenes.
/// </summary>
public static class LogoAuthoring
{
    public const string LogoPath = "Assets/UI/UIs_Common/Logo/NSMLogo.png";
    public const string ObjectName = "NSMLogo";
    private static readonly Rect LogoRect = new Rect(1746, 40, 135, 116);
    private static readonly string[] Backgrounds =
    {
        "Assets/Scenes/0_Idle.unity:Canvas/ImageUI/Intro_bg",
        "Assets/Scenes/1_Game.unity:Canvas/ImageUI/GroupSelection/SelectGroupPanel_bg",
        "Assets/Scenes/1_Game.unity:Canvas/ImageUI/ProductAndColors/SelectAllPanel_bg",
        "Assets/Scenes/2_Result.unity:Canvas/ImageUI/RecommendedAdvertisement/RecommendedAdPanel_bg",
    };

    public static string Configure()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Stop Play mode before authoring the logo.");
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scene changes before authoring the logo.");
        string previous = SceneManager.GetActiveScene().path;
        int added = 0;
        foreach (string entry in Backgrounds)
        {
            string[] parts = entry.Split(':');
            var scene = EditorSceneManager.OpenScene(parts[0]);
            string[] path = parts[1].Split('/');
            Transform background = null;
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == path[0]) background = root.transform.Find(string.Join("/", path, 1, path.Length - 1));
            if (!background) throw new InvalidOperationException("Background is missing: " + entry);
            AddAbove(background);
            added++;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous);
        return "Placed " + ObjectName + " above " + added + " backgrounds.";
    }

    public static Image AddAbove(Transform background)
    {
        var parent = background.parent;
        var logo = parent.Find(ObjectName) as RectTransform;
        if (!logo)
        {
            logo = new GameObject(ObjectName, typeof(RectTransform)).GetComponent<RectTransform>();
            logo.SetParent(parent, false);
        }
        logo.SetSiblingIndex(background.GetSiblingIndex() + 1);
        logo.anchorMin = logo.anchorMax = logo.pivot = new Vector2(0, 1);
        logo.anchoredPosition = new Vector2(LogoRect.x, -LogoRect.y);
        logo.sizeDelta = LogoRect.size;
        var image = logo.TryGetComponent(out Image found) ? found : logo.gameObject.AddComponent<Image>();
        image.sprite = Sprite();
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private static Sprite Sprite()
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(LogoPath);
        if (!importer) throw new InvalidOperationException("Logo image is missing: " + LogoPath);
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
        return AssetDatabase.LoadAssetAtPath<Sprite>(LogoPath);
    }
}
