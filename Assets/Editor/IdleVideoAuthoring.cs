using System;
using Exhibition.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// IDLE-VIDEO-001: looping c02 video as the title background. Placed right after Intro_bg so the logo,
/// texts and popup stay above it. Idempotent. ExhibitionUIBuilder calls Build; Configure() updates 0_Idle.
/// </summary>
public static class IdleVideoAuthoring
{
    public const string ClipPath = "Assets/Resources/Videos/c02.mp4";
    public const string ObjectName = "IntroVideo";

    public static string Configure()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Stop Play mode before authoring the title video.");
        var scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/0_Idle.unity")
        {
            if (scene.isDirty) throw new InvalidOperationException("Save scene changes before authoring the title video.");
            scene = EditorSceneManager.OpenScene("Assets/Scenes/0_Idle.unity");
        }
        var localization = UnityEngine.Object.FindFirstObjectByType<LocalizedImageSet>();
        var background = localization ? localization.transform.Find("Intro_bg") : null;
        if (!background) throw new InvalidOperationException("0_Idle has no ImageUI/Intro_bg.");
        Build(background);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Authored " + ObjectName + " above Intro_bg in 0_Idle.";
    }

    public static void Build(Transform background)
    {
        var clip = AssetDatabase.LoadAssetAtPath<VideoClip>(ClipPath);
        if (!clip) throw new InvalidOperationException("Title video is missing: " + ClipPath);
        var parent = background.parent;
        var video = parent.Find(ObjectName) as RectTransform;
        if (!video)
        {
            video = new GameObject(ObjectName, typeof(RectTransform)).GetComponent<RectTransform>();
            video.SetParent(parent, false);
        }
        video.SetSiblingIndex(background.GetSiblingIndex() + 1);
        video.anchorMin = Vector2.zero;
        video.anchorMax = Vector2.one;
        video.offsetMin = video.offsetMax = Vector2.zero;
        var image = Add<RawImage>(video.gameObject);
        image.raycastTarget = false;
        image.enabled = false;
        var player = Add<VideoPlayer>(video.gameObject);
        player.playOnAwake = false;
        player.isLooping = true;
        player.audioOutputMode = VideoAudioOutputMode.None;
        player.renderMode = VideoRenderMode.RenderTexture;
        var component = Add<IdleVideoBackground>(video.gameObject);
        var serialized = new SerializedObject(component);
        serialized.FindProperty("clip").objectReferenceValue = clip;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    // Unity fake-null components defeat ??; check explicitly.
    private static T Add<T>(GameObject target) where T : Component => target.TryGetComponent(out T found) ? found : target.AddComponent<T>();
}
