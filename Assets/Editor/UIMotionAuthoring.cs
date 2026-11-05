using System;
using System.Collections.Generic;
using System.Linq;
using Exhibition.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Adds presentation components to existing UI without rebuilding layout or changing content bindings.</summary>
public static class UIMotionAuthoring
{
    public static string ConfigureScenes()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Stop Play mode before configuring presentation.");
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scene changes before configuring presentation.");
        string previous = SceneManager.GetActiveScene().path;
        foreach (string scenePath in new[] { "Assets/Scenes/0_Idle.unity", "Assets/Scenes/1_Game.unity", "Assets/Scenes/2_Result.unity" })
        {
            var scene = EditorSceneManager.OpenScene(scenePath);
            var root = GameObject.Find("Canvas/ImageUI");
            if (!root) throw new InvalidOperationException("ImageUI is missing in " + scenePath);
            Configure(root);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous);
        return "Presentation components connected in all three existing scenes.";
    }

    public static void Configure(GameObject root)
    {
        var scenePresentation = Add<UIScenePresentation>(root);
        var localization = root.GetComponent<LocalizedImageSet>();
        var elements = new List<UIAnimatedElement>();
        var bindings = new SerializedObject(localization).FindProperty("bindings");
        for (int i = 0; i < bindings.arraySize; i++)
        {
            var binding = bindings.GetArrayElementAtIndex(i);
            string id = binding.FindPropertyRelative("id").stringValue;
            // The video end text belongs to the single shared popup content animation.
            if (id == "video.end" || id == "selection.productBoard" || id.EndsWith("Label", StringComparison.Ordinal)) continue;
            var image = (UnityEngine.UI.Image)binding.FindPropertyRelative("image").objectReferenceValue;
            elements.Add(Element(image.gameObject, UIAnimatedElement.Motion.Hint));
        }
        foreach (var rect in root.GetComponentsInChildren<RectTransform>(true))
        {
            if (rect.name.StartsWith("GroupFrame", StringComparison.Ordinal) || rect.name.StartsWith("SelectionFrame", StringComparison.Ordinal))
            {
                CenterPivot(rect);
                elements.Add(Element(rect.gameObject, UIAnimatedElement.Motion.Hint));
            }
            else if (rect.name == "ResultImage")
            {
                CenterPivot(rect);
                bool automatic = root.scene.name == "1_Game";
                var element = Element(rect.gameObject, UIAnimatedElement.Motion.Hint, automatic);
                if (!automatic) elements.Add(element);
            }
        }
        foreach (string panelName in new[] { "GroupSelection", "ProductAndColors", "RecommendedAdvertisement", "ExplanationVideo" })
        {
            var panel = root.transform.Find(panelName);
            if (panel) Element(panel.gameObject, UIAnimatedElement.Motion.Panel);
        }
        Set(scenePresentation, "localization", localization);
        Set(scenePresentation, "blocks", UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>());
        var serialized = new SerializedObject(scenePresentation);
        var items = serialized.FindProperty("cards"); items.arraySize = elements.Count;
        for (int i = 0; i < elements.Count; i++) items.GetArrayElementAtIndex(i).objectReferenceValue = elements[i];
        serialized.ApplyModifiedPropertiesWithoutUndo();

        var blockView = root.GetComponent<BlockExperienceView>();
        if (blockView)
        {
            var controller = UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();
            var controllerFields = new SerializedObject(controller);
            var selectionHint = controllerFields.FindProperty("pressButtonText").objectReferenceValue as GameObject;
            Set(blockView, "selectionContinue", selectionHint);
            if (selectionHint) selectionHint.SetActive(false);
            var groupHint = (GameObject)new SerializedObject(blockView).FindProperty("groupContinue").objectReferenceValue;
            groupHint.SetActive(false);
            var slotObject = new SerializedObject(blockView);
            slotObject.FindProperty("groupSlot").objectReferenceValue =
                root.transform.Find("GroupSelection/GroupFrame0").GetComponent<UIAnimatedElement>();
            var slots = slotObject.FindProperty("selectionSlots"); slots.arraySize = 4;
            for (int i = 0; i < 4; i++)
                slots.GetArrayElementAtIndex(i).objectReferenceValue =
                    root.transform.Find("ProductAndColors/SelectionFrame" + i).GetComponent<UIAnimatedElement>();
            slotObject.ApplyModifiedPropertiesWithoutUndo();
        }

        var overlay = root.transform.Find("ExplanationVideo/VideoEndOverlay");
        if (overlay)
        {
            var content = overlay.Find("PopupContent") as RectTransform;
            if (!content)
            {
                content = new GameObject("PopupContent", typeof(RectTransform)).GetComponent<RectTransform>();
                content.SetParent(overlay, false);
                content.anchorMin = Vector2.zero; content.anchorMax = Vector2.one;
                content.offsetMin = content.offsetMax = Vector2.zero;
                foreach (var child in new[] { overlay.Find("VideoEndPopup"), overlay.Find("LocalizedImages") })
                    if (child) child.SetParent(content, false);
            }
            var popup = Add<UIPopupPresentation>(root);
            Set(popup, "overlay", overlay.gameObject);
            Set(popup, "backdrop", overlay.GetComponent<UnityEngine.UI.Image>());
            Set(popup, "content", Element(content.gameObject, UIAnimatedElement.Motion.Card));
        }
    }

    private static T Add<T>(GameObject target) where T : Component => target.GetComponent<T>() ?? target.AddComponent<T>();

    private static UIAnimatedElement Element(GameObject target, UIAnimatedElement.Motion motion, bool automatic = false)
    {
        var element = Add<UIAnimatedElement>(target);
        var serialized = new SerializedObject(element);
        serialized.FindProperty("motion").enumValueIndex = (int)motion;
        serialized.FindProperty("animateOnEnable").boolValue = automatic;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return element;
    }

    private static void CenterPivot(RectTransform rect)
    {
        Vector2 delta = new Vector2(.5f, .5f) - rect.pivot;
        rect.anchoredPosition += Vector2.Scale(rect.rect.size, delta);
        rect.pivot = new Vector2(.5f, .5f);
    }

    private static void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(field).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
