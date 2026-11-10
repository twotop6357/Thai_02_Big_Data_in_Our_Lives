using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core;
using Exhibition.Blocks;
using Exhibition.Result;
using Exhibition.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>Explicit authoring tool. Creates serialized, editable image UI in the existing three scenes.</summary>
public static class ExhibitionUIBuilder
{
    private static Dictionary<string, Sprite> _sprites;
    private static ExhibitionUIAssets _library;
    private static List<LocalizedImageSet.Binding> _bindings;
    private static List<CanvasGroup> _groups;

    public static string Build()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Stop Play mode before rebuilding UI.");
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scene changes before rebuilding UI.");
        ImportSprites();
        foreach (string name in new[] { "0_Idle", "1_Game", "2_Result" }) BuildScene(name);
        RfidBlockAuthoring.Configure();
        EditorSceneManager.OpenScene("Assets/Scenes/0_Idle.unity");
        AssetDatabase.SaveAssets();
        return "Authored image UI in 0_Idle, 1_Game and 2_Result.";
    }

    private static void ImportSprites()
    {
        _sprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
        foreach (string path in Directory.GetFiles("Assets/UI", "*.png", SearchOption.AllDirectories))
        {
            string assetPath = path.Replace('\\', '/');
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            _sprites.Add(Path.GetFileNameWithoutExtension(path), AssetDatabase.LoadAssetAtPath<Sprite>(assetPath));
        }
        const string libraryPath = "Assets/UI/ExhibitionUIAssets.asset";
        _library = AssetDatabase.LoadAssetAtPath<ExhibitionUIAssets>(libraryPath);
        if (!_library)
        {
            _library = ScriptableObject.CreateInstance<ExhibitionUIAssets>();
            AssetDatabase.CreateAsset(_library, libraryPath);
        }
        _library.sprites = _sprites.Select(p => new ExhibitionUIAssets.Entry { id = p.Key, sprite = p.Value }).ToArray();
        _library.defaultThai = File.ReadAllText("Assets/StreamingAssets/Lang_Th.json");
        _library.defaultEnglish = File.ReadAllText("Assets/StreamingAssets/Lang_En.json");
        EditorUtility.SetDirty(_library);
    }

    private static void BuildScene(string name)
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity");
        var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (!canvas) throw new InvalidOperationException("Canvas is missing in " + name);
        var controller = UnityEngine.Object.FindFirstObjectByType<BlockExperienceController>();
        // These committed text panels are replaced by the requested image UI. Non-UI roots are retained.
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
            UnityEngine.Object.DestroyImmediate(canvas.transform.GetChild(i).gameObject);
        var root = Rect("ImageUI", canvas.transform, new Rect(0, 0, 1920, 1080));
        Stretch(root);
        _bindings = new List<LocalizedImageSet.Binding>();
        _groups = new List<CanvasGroup>();
        var localization = root.gameObject.AddComponent<LocalizedImageSet>();
        if (name == "0_Idle") BuildTitle(root);
        else if (name == "1_Game") BuildGame(root, controller);
        else BuildResult(root, localization);
        var serialized = new SerializedObject(localization);
        serialized.FindProperty("assets").objectReferenceValue = _library;
        var bindings = serialized.FindProperty("bindings");
        bindings.arraySize = _bindings.Count;
        for (int i = 0; i < _bindings.Count; i++)
        {
            var entry = bindings.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("id").stringValue = _bindings[i].id;
            entry.FindPropertyRelative("image").objectReferenceValue = _bindings[i].image;
            entry.FindPropertyRelative("keepObjectName").boolValue = _bindings[i].keepObjectName;
        }
        var groups = serialized.FindProperty("localizedGroups");
        groups.arraySize = _groups.Count;
        for (int i = 0; i < _groups.Count; i++) groups.GetArrayElementAtIndex(i).objectReferenceValue = _groups[i];
        serialized.ApplyModifiedPropertiesWithoutUndo();
        UIMotionAuthoring.Configure(root.gameObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = .5f;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void BuildTitle(Transform root)
    {
        Background(root, "Intro_bg");
        IdleVideoAuthoring.Build(root.Find("Intro_bg"));
        var text = LocalizedLayer(root);
        Localized(text, "idle.title");
        Localized(text, "idle.subtitle");
        Localized(text, "idle.help");
        TitleBlockPopupAuthoring.Build(root);
    }

    private static void BuildGame(Transform root, BlockExperienceController controller)
    {
        if (!controller) throw new InvalidOperationException("BlockExperienceController is missing.");
        var group = Rect("GroupSelection", root, new Rect(0, 0, 1920, 1080));
        Stretch(group);
        Background(group, "SelectGroupPanel_bg");
        BindSlot("group.board", Image(group, "GroupBoard", "BoardImage", new Rect(708, 354, 504, 504)));
        var groupFrame = Image(group, "GroupFrame0", "GroupSlot_th", new Rect(748, 394, 424, 424));
        BindSlot("group.slot", groupFrame);
        AddSlotLabel(groupFrame, "group.slotLabel", "GroupSlotLabel_th");
        AddBlockContent(groupFrame);
        var groupText = LocalizedLayer(group);
        Localized(groupText, "group.title");
        Localized(groupText, "group.subtitle");
        var groupHelp = Localized(groupText, "group.help");

        var selection = Rect("ProductAndColors", root, new Rect(0, 0, 1920, 1080));
        Stretch(selection);
        Background(selection, "SelectAllPanel_bg");
        // The title glow behind this page differs by language.
        BindSlot("selection.background", selection.Find("SelectAllPanel_bg").GetComponent<Image>());
        Image(selection, "CenterBoard", "CenterBoard_selectallPanel", new Rect(207, 281, 1507, 597));
        // Purpose order: group, product, first color, second color. The group slot reuses the group panel's frame.
        string[] slotSprites = { "GroupSlot_th", "ProductSlot_th", "ColorSlot_th", "ColorSlot_th" };
        string[] slotIds = { "selection.groupSlot", "selection.productSlot", "selection.colorSlot1", "selection.colorSlot2" };
        for (int i = 0; i < slotSprites.Length; i++)
        {
            var frame = Image(selection, "SelectionFrame" + i, slotSprites[i], new Rect(268 + i % 2 * 268, 320 + i / 2 * 268, 252, 252));
            BindSlot(slotIds[i], frame);
            AddSlotLabel(frame, slotIds[i] + "Label", slotSprites[i].Replace("Slot_", "SlotLabel_"));
            AddBlockContent(frame);
        }
        var selectionText = LocalizedLayer(selection);
        Localized(selectionText, "selection.title");
        Localized(selectionText, "selection.subtitle");
        var productBoard = Localized(selectionText, "selection.productBoard");
        var result = Image(productBoard.transform, "ResultImage", null, new Rect(180, 75, 478, 390));
        result.gameObject.SetActive(false);
        var help = Localized(selectionText, "selection.help");
        var empty = Rect("EmptyResult", productBoard.transform, new Rect(0, 0, 838, 493));
        var fields = new SerializedObject(controller);
        foreach (string field in new[] { "groupText", "productText", "color1Text", "color2Text" })
            fields.FindProperty(field).objectReferenceValue = null;
        fields.FindProperty("resultImage").objectReferenceValue = result;
        fields.FindProperty("resultTextGroup").objectReferenceValue = empty.gameObject;
        fields.FindProperty("pressButtonText").objectReferenceValue = help.gameObject;
        fields.ApplyModifiedPropertiesWithoutUndo();
        var view = root.gameObject.AddComponent<BlockExperienceView>();
        Set(view, "controller", controller);
        Set(view, "groupPanel", group.gameObject);
        Set(view, "selectionPanel", selection.gameObject);
        Set(view, "groupContinue", groupHelp.gameObject);
        selection.gameObject.SetActive(false);
    }

    private static void BuildResult(Transform root, LocalizedImageSet localization)
    {
        var ad = Rect("RecommendedAdvertisement", root, new Rect(0, 0, 1920, 1080));
        Stretch(ad);
        Background(ad, "RecommendedAdPanel_bg");
        var board = Image(ad, "AdvertisementBoard", "AdBoard", new Rect(461, 320, 998, 537));
        var result = Image(board.transform, "ResultImage", null, new Rect(259, 28, 480, 480));
        // The existing presenter may be on a non-UI root. Reuse it in that case.
        var presenter = UnityEngine.Object.FindFirstObjectByType<ResultImagePresenter>();
        if (!presenter) presenter = root.gameObject.AddComponent<ResultImagePresenter>();
        Set(presenter, "resultImage", result);
        var text = LocalizedLayer(ad);
        Localized(text, "result.title");
        Localized(text, "result.subtitle");
        var advertisementHint = Localized(text, "result.help");

        var video = Rect("ExplanationVideo", root, new Rect(0, 0, 1920, 1080));
        Stretch(video);
        var raw = video.gameObject.AddComponent<RawImage>();
        raw.color = Color.black;
        raw.raycastTarget = false;
        var player = root.gameObject.AddComponent<VideoPlayer>();
        player.playOnAwake = false;
        player.aspectRatio = VideoAspectRatio.FitInside;
        var playbackText = LocalizedLayer(video);
        var playbackHint = Localized(playbackText, "video.home");
        playbackHint.gameObject.SetActive(false);
        var popup = Rect("VideoEndOverlay", video, new Rect(0, 0, 1920, 1080));
        Stretch(popup);
        var dim = popup.gameObject.AddComponent<Image>();
        dim.color = new Color(91f / 255, 91f / 255, 91f / 255, .8f);
        dim.raycastTarget = false;
        Image(popup, "VideoEndPopup", "VideoEndPopup", new Rect(263, 203, 1394, 674));
        var endText = LocalizedLayer(popup);
        Localized(endText, "video.end");
        popup.gameObject.SetActive(false);
        video.gameObject.SetActive(false);
        var controller = root.gameObject.AddComponent<ResultVideoController>();
        Set(controller, "advertisement", ad.gameObject);
        Set(controller, "videoPanel", video.gameObject);
        Set(controller, "endPopup", popup.gameObject);
        Set(controller, "playbackHint", playbackHint.gameObject);
        // The advertisement hint fades in once input is available; UIMotionAuthoring reuses this element.
        Set(controller, "advertisementHint", advertisementHint.gameObject.AddComponent<UIAnimatedElement>());
        Set(controller, "videoImage", raw);
        Set(controller, "player", player);
        Set(controller, "localization", localization);
    }

    private static RectTransform Rect(string name, Transform parent, Rect rect)
    {
        var result = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        result.SetParent(parent, false);
        result.anchorMin = result.anchorMax = result.pivot = new Vector2(0, 1);
        result.anchoredPosition = new Vector2(rect.x, -rect.y);
        result.sizeDelta = rect.size;
        return result;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static Image Image(Transform parent, string name, string sprite, Rect rect)
    {
        var image = Rect(name, parent, rect).gameObject.AddComponent<Image>();
        image.sprite = sprite == null ? null : _sprites[sprite];
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private static void Background(Transform parent, string sprite)
    {
        var image = Image(parent, sprite, sprite, new Rect(0, 0, 1920, 1080));
        image.preserveAspect = false;
        Stretch(image.rectTransform);
        // The logo is no longer baked into the backgrounds.
        LogoAuthoring.AddAbove(image.transform);
    }

    private static Transform LocalizedLayer(Transform parent)
    {
        var rect = Rect("LocalizedImages", parent, new Rect(0, 0, 1920, 1080));
        Stretch(rect);
        var group = rect.gameObject.AddComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;
        _groups.Add(group);
        return rect;
    }

    private static Image Localized(Transform parent, string id)
    {
        var doc = JsonUtility.FromJson<ImageLanguageDocument>(File.ReadAllText("Assets/StreamingAssets/Lang_Th.json"));
        var entry = doc.images.First(e => e.id == id);
        var image = Image(parent, entry.sprite, entry.sprite, new Rect(entry.x, entry.y, entry.width, entry.height));
        _bindings.Add(new LocalizedImageSet.Binding { id = id, image = image });
        return image;
    }

    private static void BindSlot(string id, Image image)
    {
        image.preserveAspect = false;
        _bindings.Add(new LocalizedImageSet.Binding { id = id, image = image, keepObjectName = true });
    }

    private static void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(field).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    // The frame PNG bakes its label into a flat fill. A cover hides those words so a separate label can bounce.
    private static void AddSlotLabel(Image frame, string id, string sprite)
    {
        const float inset = .12f;
        var cover = Image(frame.transform, "SlotLabelCover", "SlotLabelCover", new Rect(0, 0, 1, 1));
        cover.rectTransform.anchorMin = Vector2.one * inset;
        cover.rectTransform.anchorMax = Vector2.one * (1 - inset);
        cover.rectTransform.offsetMin = cover.rectTransform.offsetMax = Vector2.zero;
        var label = Image(cover.transform, "SlotLabel", sprite, new Rect(0, 0, 1, 1));
        // Anchors outside the cover make the label exactly as large as the frame.
        float beyond = inset / (1 - 2 * inset);
        label.rectTransform.pivot = Vector2.one * .5f;
        label.rectTransform.anchorMin = Vector2.one * -beyond;
        label.rectTransform.anchorMax = Vector2.one * (1 + beyond);
        label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
        BindSlot(id, label);
    }

    private static void AddBlockContent(Image frame)
    {
        // Future block artwork belongs here. The frame itself always stays untinted.
        var image = Image(frame.transform, "BlockContent", null, new Rect(8, 8, frame.rectTransform.sizeDelta.x - 16, frame.rectTransform.sizeDelta.y - 16));
        image.enabled = false;
    }
}
