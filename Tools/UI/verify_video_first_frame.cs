// Execute through Unity MCP from a fresh Thai Idle. Native decoder and rendered Canvas checks.
const string key = "VIDEO-FIRST-001-validation-v2";
if (UnityEditor.SessionState.GetString(key, "") == "running") return "Already running";
UnityEditor.SessionState.SetString(key, "running");
Cysharp.Threading.Tasks.UniTask.Void(async () =>
{
    var checks = new Newtonsoft.Json.Linq.JArray();
    var samples = new Newtonsoft.Json.Linq.JArray();
    var report = new Newtonsoft.Json.Linq.JObject { ["task"] = "VIDEO-FIRST-001", ["unity"] = Application.unityVersion,
        ["checks"] = checks, ["playbacks"] = samples };
    using var limit = new System.Threading.CancellationTokenSource(System.TimeSpan.FromSeconds(150));
    var token = limit.Token;
    var manager = UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();
    var simulator = UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
    bool simEnabled = simulator && simulator.enabled, background = Application.runInBackground;
    Application.runInBackground = true;
    if (simulator) simulator.enabled = false;
    var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
    Exhibition.UI.ImageLanguageDocument changedDocument = null;
    string originalVideo = null;
    void Check(bool value, string label)
    {
        if (!value) throw new System.Exception("Video validation failed: " + label);
        checks.Add(label);
        System.IO.File.WriteAllText("docs/validation/ui/video_first_frame_progress.json", report.ToString());
    }
    async Cysharp.Threading.Tasks.UniTask Frames(int n = 2)
    { for (int i = 0; i < n; i++) await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update, token); }
    async Cysharp.Threading.Tasks.UniTask Wait(System.Func<bool> p)
        => await Cysharp.Threading.Tasks.UniTask.WaitUntil(p, cancellationToken: token);
    async Cysharp.Threading.Tasks.UniTask Ready() => await Wait(() => !manager.IsTransitioning && !manager.IsPresentationLocked);
    async Cysharp.Threading.Tasks.UniTask Tap()
    { await Ready(); manager.SetButtonPressed(false); await Frames(); manager.SetButtonPressed(true); await Frames(); manager.SetButtonPressed(false); await Frames(); }
    async Cysharp.Threading.Tasks.UniTask Home()
    { manager.ReturnToIdleManually(); await Ready(); await Frames(); Check(manager.IsTitleScreen, "Home returns to Idle with unlocked input"); }
    async Cysharp.Threading.Tasks.UniTask<Exhibition.UI.ResultVideoController> Result()
    {
        await Tap(); await Ready();
        var blocks = UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();
        await Wait(() => blocks && blocks.IsReady);
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group, "student"); await Tap(); await Ready();
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product, "bags");
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1, "red");
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2, "blue");
        await Wait(() => !blocks.IsGenerating && manager.ResultTexture); await Tap(); await Ready();
        return UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.ResultVideoController>();
    }
    Texture2D ReadTarget(RenderTexture target)
    {
        var small = RenderTexture.GetTemporary(96, 54, 0, RenderTextureFormat.ARGB32);
        var previous = RenderTexture.active;
        try
        {
            Graphics.Blit(target, small); RenderTexture.active = small;
            var image = new Texture2D(96, 54, TextureFormat.RGBA32, false);
            image.ReadPixels(new Rect(0, 0, 96, 54), 0, 0); image.Apply(); return image;
        }
        finally { RenderTexture.active = previous; RenderTexture.ReleaseTemporary(small); }
    }
    async Cysharp.Threading.Tasks.UniTask Opening(Exhibition.UI.ResultVideoController controller, string name)
    {
        var so = new UnityEditor.SerializedObject(controller);
        var ad = (GameObject)so.FindProperty("advertisement").objectReferenceValue;
        var raw = (UnityEngine.UI.RawImage)so.FindProperty("videoImage").objectReferenceValue;
        var player = controller.GetComponent<UnityEngine.Video.VideoPlayer>();
        var timer = UnityEngine.Object.FindFirstObjectByType<HuliacDev.Core.InactivityTimer>();
        var timeline = new Newtonsoft.Json.Linq.JArray();
        long firstCanvasFrame = -1; int uncovered = 0, preparationFrames = 0;
        bool capturePending = false, captured = false;
        double firstShownAt = 0;
        bool oldSkip = player.skipOnDrop;
        UnityEngine.Canvas.WillRenderCanvases render = () =>
        {
            if (!controller || manager.IsTransitioning) return;
            bool advertisement = ad.activeInHierarchy && ad.GetComponent<CanvasGroup>().alpha >= .999f;
            bool video = raw.enabled && raw.gameObject.activeInHierarchy && raw.GetComponent<CanvasGroup>().alpha >= .999f && raw.texture && player.frame >= 0;
            if (!advertisement && !video) uncovered++;
            if (controller.State == Exhibition.UI.ResultVideoController.ScreenState.Preparing && advertisement) preparationFrames++;
            if (video && firstCanvasFrame < 0)
            { firstCanvasFrame = player.frame; capturePending = true; firstShownAt = Time.realtimeSinceStartupAsDouble; }
            if (timeline.Count < 70) timeline.Add(new Newtonsoft.Json.Linq.JObject { ["state"] = controller.State.ToString(),
                ["frame"] = player.frame, ["ad"] = advertisement, ["video"] = video, ["alpha"] = raw.GetComponent<CanvasGroup>().alpha });
        };
        Canvas.willRenderCanvases += render;
        try
        {
            await Tap();
            while (controller.State != Exhibition.UI.ResultVideoController.ScreenState.Playing || player.frame < 8 || !captured)
            {
                token.ThrowIfCancellationRequested();
                if (controller.State == Exhibition.UI.ResultVideoController.ScreenState.Failed) throw new System.Exception("Playback failed while checking " + name);
                await Cysharp.Threading.Tasks.UniTask.WaitForEndOfFrame(manager, token);
                if (!capturePending || captured) continue;
                captured = true;
                var first = ReadTarget((RenderTexture)raw.texture);
                try
                {
                    var pixels = first.GetPixels();
                    float min = pixels.Min(p => p.grayscale), max = pixels.Max(p => p.grayscale);
                    Check(min < .9f && max > .1f && max - min > .08f, name + " decoded opening contains real image detail, not a black or white placeholder");
                    Check(pixels.All(p => p.a > .99f), name + " decoded opening is opaque and cannot expose the Skybox through alpha");
                    System.IO.File.WriteAllBytes("docs/validation/ui/video_first_" + name + "_target.png", first.EncodeToPNG());
                }
                finally { UnityEngine.Object.Destroy(first); }
                var screen = ScreenCapture.CaptureScreenshotAsTexture();
                try { System.IO.File.WriteAllBytes("docs/validation/ui/video_first_" + name + "_screen.png", screen.EncodeToPNG()); }
                finally { UnityEngine.Object.Destroy(screen); }
            }
            Check(firstCanvasFrame == 0, name + " first Canvas presentation is exactly decoded frame zero");
            Check((long)typeof(Exhibition.UI.ResultVideoController).GetField("_firstFrame", flags).GetValue(controller) == 0,
                name + " native frameReady supplies frame zero before reveal");
            Check(preparationFrames > 0 && uncovered == 0, name + " every rendered preparation/handoff frame is covered by the ad or a valid opaque video");
            Check(player.isPlaying && player.frame >= 8 && Time.realtimeSinceStartupAsDouble - firstShownAt < 3,
                name + " opening immediately advances instead of remaining frozen");
            Check(!ad.activeSelf && raw.enabled && raw.GetComponent<CanvasGroup>().alpha == 1, name + " handoff is fully opaque without a fade exposing the camera");
            Check(!manager.IsPresentationLocked && timer.IsPaused, name + " actual playback releases the visual lock and keeps inactivity paused");
            Check(player.skipOnDrop == oldSkip && !player.sendFrameReadyEvents &&
                Enumerable.Range(0, player.audioTrackCount).All(i => !player.GetDirectAudioMute((ushort)i)), name + " startup restores audio/drop settings and stops per-frame callbacks");
            samples.Add(new Newtonsoft.Json.Linq.JObject { ["name"] = name, ["firstPresentedFrame"] = firstCanvasFrame,
                ["progressFrame"] = player.frame, ["uncoveredRenderCallbacks"] = uncovered, ["timeline"] = timeline });
        }
        finally { Canvas.willRenderCanvases -= render; }
    }
    try
    {
        await Wait(() => manager && manager.IsInputReady); await Ready();
        var controller = await Result();
        controller.SendMessage("Continue", SendMessageOptions.RequireReceiver);
        var fields = new UnityEditor.SerializedObject(controller);
        var ad = (GameObject)fields.FindProperty("advertisement").objectReferenceValue;
        var raw = (UnityEngine.UI.RawImage)fields.FindProperty("videoImage").objectReferenceValue;
        Check(controller.State == Exhibition.UI.ResultVideoController.ScreenState.Preparing && ad.activeInHierarchy && !raw.enabled,
            "Before any decoder work, the existing ad remains visible and the uninitialized RawImage is disabled");
        await Home();
        Check(!UnityEngine.Object.FindObjectsByType<UnityEngine.Video.VideoPlayer>(FindObjectsSortMode.None).Any(p => p.isPlaying), "Home during prepare leaves no late playback");
        controller = await Result(); await Opening(controller, "th"); await Home();
        Check(Resources.FindObjectsOfTypeAll<RenderTexture>().Count(t => t.name == "ExhibitionVideo") == 0, "Thai home releases the decoder render target");
        manager.SetButtonPressed(false); await Frames(); manager.SetButtonPressed(true);
        await Cysharp.Threading.Tasks.UniTask.Delay(System.TimeSpan.FromSeconds(2.2), Cysharp.Threading.Tasks.DelayType.UnscaledDeltaTime, cancellationToken: token);
        manager.SetButtonPressed(false); await Frames(); await Ready();
        Check(manager.LanguageCode == "en", "English title language selection is preserved");
        controller = await Result();
        var localization = controller.GetComponent<Exhibition.UI.LocalizedImageSet>();
        changedDocument = (Exhibition.UI.ImageLanguageDocument)typeof(Exhibition.UI.LocalizedImageSet).GetField("_english", flags).GetValue(localization);
        originalVideo = changedDocument.video; changedDocument.video = "Videos/VIDEO_FIRST_001_missing";
        await Tap(); await Wait(() => controller.State == Exhibition.UI.ResultVideoController.ScreenState.Failed); await Ready();
        fields = new UnityEditor.SerializedObject(controller); ad = (GameObject)fields.FindProperty("advertisement").objectReferenceValue;
        raw = (UnityEngine.UI.RawImage)fields.FindProperty("videoImage").objectReferenceValue;
        var popup = (GameObject)fields.FindProperty("endPopup").objectReferenceValue;
        Check(ad.activeInHierarchy && ad.GetComponent<CanvasGroup>().alpha == 1 && !raw.enabled && popup.activeInHierarchy,
            "Missing-video failure keeps the opaque ad under its popup instead of exposing an empty video surface");
        Check(!UnityEngine.Object.FindFirstObjectByType<HuliacDev.Core.InactivityTimer>().IsPaused, "Failed prepare resumes inactivity and permits retry");
        changedDocument.video = originalVideo; changedDocument = null;
        await Opening(controller, "en_retry");
        var player = controller.GetComponent<UnityEngine.Video.VideoPlayer>(); player.time = System.Math.Max(0, player.clip.length - .5);
        await Wait(() => controller.State == Exhibition.UI.ResultVideoController.ScreenState.Finished); await Ready();
        Check(raw.enabled && raw.texture && popup.activeInHierarchy && !UnityEngine.Object.FindFirstObjectByType<HuliacDev.Core.InactivityTimer>().IsPaused,
            "Video completion retains its final decoded frame behind the existing popup and resumes inactivity");
        for (int slot = 0; slot < 4; slot++) controller.SetBlockPresent(slot, false);
        await Ready(); await Frames(); Check(manager.IsTitleScreen, "All four removal signals still return home after video completion");
        controller = await Result(); await Opening(controller, "en_repeat");
        player = controller.GetComponent<UnityEngine.Video.VideoPlayer>(); player.time = System.Math.Max(0, player.clip.length - .5);
        await Wait(() => controller.State == Exhibition.UI.ResultVideoController.ScreenState.Finished); await Ready();
        controller.enabled = false; await Frames();
        fields = new UnityEditor.SerializedObject(controller); ad = (GameObject)fields.FindProperty("advertisement").objectReferenceValue;
        raw = (UnityEngine.UI.RawImage)fields.FindProperty("videoImage").objectReferenceValue;
        Check(ad.activeInHierarchy && ad.GetComponent<CanvasGroup>().alpha == 1 && !raw.enabled && !raw.texture,
            "Disabling the finished-video controller restores the opaque ad before releasing the final frame");
        controller.enabled = true; await Ready(); await Frames();
        Check(controller.State == Exhibition.UI.ResultVideoController.ScreenState.Advertisement && ad.activeInHierarchy && !raw.enabled,
            "Re-enabling after final-frame cleanup restores a usable advertisement without exposing the camera");
        await Home();
        Check(Resources.FindObjectsOfTypeAll<RenderTexture>().Count(t => t.name == "ExhibitionVideo") == 0 && !manager.ResultTexture,
            "Repeated playback/home releases all owned render targets and result images");
        report["passed"] = true;
    }
    catch (System.Exception ex) { report["passed"] = false; report["error"] = ex.ToString(); Debug.LogException(ex); }
    finally
    {
        if (changedDocument != null) changedDocument.video = originalVideo;
        if (manager) manager.SetButtonPressed(false);
        if (simulator) simulator.enabled = simEnabled;
        Application.runInBackground = background;
        System.IO.File.WriteAllText("docs/validation/ui/video_first_frame.json", report.ToString());
        UnityEditor.SessionState.SetString(key, "done");
    }
});
return "Native first-frame validation dispatched";
