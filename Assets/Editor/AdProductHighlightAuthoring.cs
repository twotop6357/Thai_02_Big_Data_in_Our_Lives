using System;
using System.IO;
using System.Linq;
using AssetKits.ParticleImage;
using AssetKits.ParticleImage.Enumerations;
using Exhibition.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Copies the open demo Item without saving or changing the user's source scene.</summary>
public static class AdProductHighlightAuthoring
{
    public const string PrefabPath = "Assets/UI/Effects/AdItemEmphasis.prefab";
    public const string ConfettiPath = "Assets/UI/Effects/AdConfettiFullscreen.prefab";

    public static string UpgradeAdvertisementEffects()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode before configuring advertisement effects.");
        var previous = SceneManager.GetActiveScene();
        var root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            foreach (var p in root.GetComponentsInChildren<ParticleImage>(true))
            {
                bool glow = p.name == "ProductGlowRays", ring = p.name == "ProductRing";
                p.loop = glow || ring;
                p.duration = glow ? 6 : ring ? 2.4f : .65f;
                p.lifetime = new ParticleSystem.MinMaxCurve(glow ? 6 : ring ? 2.4f : .9f);
                p.startDelay = 0;
                p.rotationOverLifetime = new SeparatedMinMaxCurve(glow ? 60 : 0);
                // A 120-to-480 wave stays inside the product's 480px area and the advertisement board.
                p.startSize = new SeparatedMinMaxCurve(glow ? 660 : ring ? 480 : 64);
                p.sizeOverLifetime = ring ? new SeparatedMinMaxCurve(AnimationCurve.Linear(0, .25f, 1, 1)) : new SeparatedMinMaxCurve(1);
                p.startSpeed = new ParticleSystem.MinMaxCurve(glow || ring ? 0 : .4f);
                p.startColor = new ParticleSystem.MinMaxGradient(new Color(1, .73f, .2f, glow ? .45f : ring ? .45f : 1));
                var fade = new Gradient();
                fade.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, glow
                    ? new[] { new GradientAlphaKey(.7f, 0), new GradientAlphaKey(1, .5f), new GradientAlphaKey(.7f, 1) }
                    : new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, .15f), new GradientAlphaKey(0, 1) });
                p.colorOverLifetime = new ParticleSystem.MinMaxGradient(fade);
                if (!glow && !ring)
                {
                    p.SetBurst(0, 0, 14); p.rateOverTime = 16;
                    ((RectTransform)p.transform).sizeDelta = new Vector2(500, 500);
                }
            }
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }

        if (!File.Exists(ConfettiPath))
        {
            var preview = EditorSceneManager.NewPreviewScene();
            try
            {
                var canvas = new GameObject("ConfettiImportCanvas", typeof(RectTransform), typeof(Canvas));
                SceneManager.MoveGameObjectToScene(canvas, preview); canvas.SetActive(false);
                var viewport = new GameObject("AdConfettiFullscreen", typeof(RectTransform), typeof(CanvasGroup));
                viewport.transform.SetParent(canvas.transform, false);
                Center((RectTransform)viewport.transform, new Vector2(1920, 1080));
                var opacity = viewport.GetComponent<CanvasGroup>(); opacity.alpha = 0;
                opacity.interactable = opacity.blocksRaycasts = false;
                var source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AssetKits/ParticleImage/Demo/Prefabs/ConfettiFullscreen.prefab");
                // A detached copy avoids Unity rewriting a legacy vendor prefab while saving nested overrides.
                var copy = UnityEngine.Object.Instantiate(source, viewport.transform, false);
                copy.name = "ConfettiFullscreen";
                foreach (var p in copy.GetComponentsInChildren<ParticleImage>(true))
                {
                    var so = new SerializedObject(p); so.FindProperty("_playMode").enumValueIndex = 0;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    p.loop = false; p.timeScale = TimeScale.Unscaled;
                    var size = p.startSize;
                    size.mainCurve = ScaleCurve(size.mainCurve, 2); size.xCurve = ScaleCurve(size.xCurve, 2);
                    size.yCurve = ScaleCurve(size.yCurve, 2); size.zCurve = ScaleCurve(size.zCurve, 2);
                    p.startSize = size;
                    var rotation = p.rotationOverLifetime;
                    rotation.mainCurve = ScaleCurve(rotation.mainCurve, .2f); rotation.xCurve = ScaleCurve(rotation.xCurve, .2f);
                    rotation.yCurve = ScaleCurve(rotation.yCurve, .2f); rotation.zCurve = ScaleCurve(rotation.zCurve, .2f);
                    p.rotationOverLifetime = rotation;
                    if (p.transform.parent == copy.transform)
                    {
                        var rect = (RectTransform)p.transform;
                        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 990);
                    }
                }
                foreach (var graphic in copy.GetComponentsInChildren<Graphic>(true))
                { graphic.raycastTarget = false; if (graphic is MaskableGraphic masked) masked.maskable = false; }
                viewport.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(viewport, ConfettiPath);
            }
            finally { EditorSceneManager.ClosePreviewScene(preview); }
        }

        const string resultPath = "Assets/Scenes/2_Result.unity";
        var scene = SceneManager.GetSceneByPath(resultPath);
        bool opened = !scene.IsValid() || !scene.isLoaded;
        if (opened) scene = EditorSceneManager.OpenScene(resultPath, OpenSceneMode.Additive);
        try
        {
            if (!opened && scene.isDirty) throw new InvalidOperationException("Result has unsaved changes. Preserve them before configuring effects.");
            var ui = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<UIScenePresentation>(true)).Single();
            var board = (RectTransform)ui.transform.Find("RecommendedAdvertisement/AdvertisementBoard");
            var product = (RectTransform)board.Find("ResultImage");
            var effect = product.Find("AdItemEmphasis") ?? board.Find("AdItemEmphasis");
            effect.SetParent(board, false);
            var effectRect = (RectTransform)effect;
            effectRect.anchorMin = product.anchorMin; effectRect.anchorMax = product.anchorMax;
            effectRect.pivot = product.pivot; effectRect.sizeDelta = product.sizeDelta;
            effectRect.anchoredPosition3D = product.anchoredPosition3D;
            var confetti = board.Find("AdConfettiFullscreen");
            if (!confetti) confetti = ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ConfettiPath), board)).transform;
            Center((RectTransform)confetti, new Vector2(1920, 1080));
            var uiRect = (RectTransform)ui.transform;
            confetti.position = uiRect.TransformPoint(uiRect.rect.center);
            // Ordinary sibling order keeps every particle/trail behind the product without sorting overrides.
            effect.SetAsFirstSibling(); confetti.SetSiblingIndex(1); product.SetAsLastSibling();
            var highlight = ui.GetComponent<AdProductHighlight>();
            Set(highlight, "confettiRoot", confetti.gameObject);
            Set(highlight, "confettiOpacity", confetti.GetComponent<CanvasGroup>());
            Set(highlight, "confettiEmitter", confetti.GetComponentInChildren<ParticleImage>(true));
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }
        finally
        {
            if (opened) EditorSceneManager.CloseScene(scene, true);
            SceneManager.SetActiveScene(previous);
        }
        return "Product now renders above slow looping aura, larger bursts, and the full-screen ConfettiFullscreen instance.";
    }

    private static ParticleSystem.MinMaxCurve ScaleCurve(ParticleSystem.MinMaxCurve curve, float factor)
    {
        if (curve.mode == ParticleSystemCurveMode.Constant || curve.mode == ParticleSystemCurveMode.TwoConstants)
        { curve.constantMin *= factor; curve.constantMax *= factor; }
        else curve.curveMultiplier *= factor;
        return curve;
    }

    public static string ImportOpenItem()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode before importing the effect.");
        if (File.Exists(PrefabPath)) throw new InvalidOperationException("The project product emphasis prefab already exists. Edit that prefab instead of importing again.");
        var sourceScene = SceneManager.GetActiveScene();
        var item = sourceScene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Transform>(true))
            .First(x => x.name == "Item" && x.GetComponentsInChildren<ParticleImage>(true).Length == 3);
        var preview = EditorSceneManager.NewPreviewScene();
        try
        {
            var canvas = new GameObject("EffectImportCanvas", typeof(RectTransform), typeof(Canvas));
            SceneManager.MoveGameObjectToScene(canvas, preview);
            canvas.SetActive(false);
            var copy = UnityEngine.Object.Instantiate(item.gameObject, canvas.transform, false);
            copy.name = "AdItemEmphasis";
            UnityEngine.Object.DestroyImmediate(copy.GetComponent<RectMask2D>());
            UnityEngine.Object.DestroyImmediate(copy.GetComponent<Image>());
            var icon = copy.transform.Find("Icon");
            if (icon) UnityEngine.Object.DestroyImmediate(icon.gameObject);
            var root = (RectTransform)copy.transform;
            Center(root, new Vector2(480, 480));
            var opacity = copy.AddComponent<CanvasGroup>();
            opacity.alpha = 0; opacity.blocksRaycasts = false; opacity.interactable = false;
            var particles = copy.GetComponentsInChildren<ParticleImage>(true);
            for (int i = 0; i < particles.Length; i++)
            {
                var p = particles[i];
                // Set serialized autoplay directly while inactive: the vendor initializes main/children on enable.
                var serialized = new SerializedObject(p);
                serialized.FindProperty("_playMode").enumValueIndex = (int)AssetKits.ParticleImage.Enumerations.PlayMode.None;
                serialized.FindProperty("_bursts").ClearArray();
                serialized.ApplyModifiedPropertiesWithoutUndo();
                p.raycastTarget = false; p.maskable = false;
                p.loop = false; p.prewarm = false; p.timeScale = TimeScale.Unscaled;
                p.duration = i == 2 ? .65f : .15f;
                p.startDelay = i == 1 ? .08f : 0;
                p.lifetime = new ParticleSystem.MinMaxCurve(i == 0 ? 1.55f : i == 1 ? 1.05f : .65f);
                p.rateOverTime = i == 2 ? 12 : 0;
                p.rateOverLifetime = 0; p.rateOverDistance = 0;
                p.AddBurst(0, i == 2 ? 7 : 1);
                // ParticleImage multiplies speed by 100 internally: .25 means about 25 UI units/second.
                p.startSpeed = new ParticleSystem.MinMaxCurve(i == 2 ? .25f : 0);
                p.startSize = new SeparatedMinMaxCurve(i == 0 ? 510 : i == 1 ? 370 : 32);
                p.startColor = new ParticleSystem.MinMaxGradient(new Color(1, .73f, .2f, i == 0 ? .22f : i == 1 ? .4f : 1));
                var fade = new Gradient();
                fade.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                    new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, .12f), new GradientAlphaKey(.8f, .6f), new GradientAlphaKey(0, 1) });
                p.colorOverLifetime = new ParticleSystem.MinMaxGradient(fade);
                Center((RectTransform)p.transform, new Vector2(i == 2 ? 410 : 480, i == 2 ? 410 : 480));
                if (i == 2)
                {
                    p.fitRect = true; p.emitOnSurface = false;
                    serialized.Update(); serialized.FindProperty("_spread").enumValueIndex = (int)SpreadType.Uniform;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                p.name = i == 0 ? "ProductGlowRays" : i == 1 ? "ProductRing" : "ProductSparkles";
            }
            copy.SetActive(false);
            Directory.CreateDirectory("Assets/UI/Effects");
            PrefabUtility.SaveAsPrefabAsset(copy, PrefabPath);
        }
        finally { EditorSceneManager.ClosePreviewScene(preview); }

        const string targetPath = "Assets/Scenes/2_Result.unity";
        var result = SceneManager.GetSceneByPath(targetPath);
        bool opened = !result.IsValid() || !result.isLoaded;
        if (opened) result = EditorSceneManager.OpenScene(targetPath, OpenSceneMode.Additive);
        try
        {
            var ui = result.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<UIScenePresentation>(true)).Single();
            var product = ui.transform.Find("RecommendedAdvertisement/AdvertisementBoard/ResultImage");
            if (!product) throw new InvalidOperationException("Advertisement product image is missing.");
            if (product.Find("AdItemEmphasis")) throw new InvalidOperationException("Product emphasis is already installed.");
            var fx = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), product);
            Center((RectTransform)fx.transform, new Vector2(480, 480));
            var highlight = ui.gameObject.AddComponent<AdProductHighlight>();
            Set(highlight, "effectRoot", fx); Set(highlight, "effectOpacity", fx.GetComponent<CanvasGroup>());
            var so = new SerializedObject(highlight);
            var roots = fx.GetComponentsInChildren<ParticleImage>(true).Where(x => x.transform.parent == fx.transform).ToArray();
            var field = so.FindProperty("emitters"); field.arraySize = roots.Length;
            for (int i = 0; i < roots.Length; i++) field.GetArrayElementAtIndex(i).objectReferenceValue = roots[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            Set(ui, "productHighlight", highlight);
            Set(ui.GetComponent<ResultVideoController>(), "productHighlight", highlight);
            EditorSceneManager.MarkSceneDirty(result);
            EditorSceneManager.SaveScene(result);
        }
        finally
        {
            if (opened) EditorSceneManager.CloseScene(result, true);
            SceneManager.SetActiveScene(sourceScene);
        }
        return "Copied two Item effect branches (three ParticleImages), installed the 1.8-second advertisement emphasis, and preserved the open source scene.";
    }

    private static void Center(RectTransform rect, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition3D = Vector3.zero; rect.sizeDelta = size;
        rect.localScale = Vector3.one; rect.localRotation = Quaternion.identity;
    }

    private static void Set(UnityEngine.Object target, string name, UnityEngine.Object value)
    {
        var so = new SerializedObject(target); so.FindProperty(name).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
