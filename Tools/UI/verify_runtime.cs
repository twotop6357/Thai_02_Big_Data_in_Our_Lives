// Execute this method body through Unity MCP while playing from 0_Idle.
const string key = "RFID-BLOCKS-001-ui-regression-v1";
if (UnityEditor.SessionState.GetString(key, "") == "running") return "Already running";
UnityEditor.SessionState.SetString(key, "running");
Cysharp.Threading.Tasks.UniTask.Void(async () =>
{
    var checks = new Newtonsoft.Json.Linq.JArray();
    var report = new Newtonsoft.Json.Linq.JObject { ["task"] = "UI-LANG-001", ["unity"] = Application.unityVersion,
        ["checks"] = checks, ["video_end_verification"] = "Decoder playback followed by seek near end; not a full-duration viewing test." };
    using var limit = new System.Threading.CancellationTokenSource(System.TimeSpan.FromSeconds(240));
    var token = limit.Token;
    bool oldBackground = Application.runInBackground;
    Application.runInBackground = true;
    Core.GameManager manager = null;
    Core.KeyboardButtonSimulator simulator = null;
    bool simulatorWasEnabled = false;
    string languagePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Lang_En.json");
    string savedJson = null;
    void Check(bool condition, string label)
    {
        if (!condition) throw new System.Exception("Validation failed: " + label);
        checks.Add(label);
        System.IO.File.WriteAllText("docs/validation/ui/rfid_regression_progress.json", report.ToString());
    }
    async Cysharp.Threading.Tasks.UniTask Frames(int count = 3)
    {
        for (int i = 0; i < count; i++) await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update, token);
    }
    async Cysharp.Threading.Tasks.UniTask Wait(System.Func<bool> predicate)
        => await Cysharp.Threading.Tasks.UniTask.WaitUntil(predicate, cancellationToken: token);
    async Cysharp.Threading.Tasks.UniTask Delay(double seconds)
        => await Cysharp.Threading.Tasks.UniTask.Delay(System.TimeSpan.FromSeconds(seconds), Cysharp.Threading.Tasks.DelayType.UnscaledDeltaTime, cancellationToken: token);
    async Cysharp.Threading.Tasks.UniTask Tap()
    {
        await Wait(() => !manager.IsTransitioning && !manager.IsPresentationLocked);
        manager.SetButtonPressed(false); await Frames();
        manager.SetButtonPressed(true); await Frames();
        manager.SetButtonPressed(false); await Frames();
    }
    async Cysharp.Threading.Tasks.UniTask Hold(double seconds)
    {
        await Wait(() => !manager.IsTransitioning && !manager.IsPresentationLocked);
        manager.SetButtonPressed(false); await Frames();
        manager.SetButtonPressed(true); await Frames(); await Delay(seconds);
        manager.SetButtonPressed(false); await Frames();
    }
    async Cysharp.Threading.Tasks.UniTask Idle()
        => await Wait(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "0_Idle" && !manager.IsTransitioning);
    async Cysharp.Threading.Tasks.UniTask Capture(string name)
    {
        await Wait(() => !manager.IsTransitioning && !manager.IsPresentationLocked);
        await Frames();
        await Cysharp.Threading.Tasks.UniTask.WaitForEndOfFrame(manager);
        var image = ScreenCapture.CaptureScreenshotAsTexture();
        System.IO.File.WriteAllBytes("docs/validation/ui/" + name + ".png", image.EncodeToPNG());
        UnityEngine.Object.Destroy(image);
    }
    void Geometry(Exhibition.UI.LocalizedImageSet ui)
    {
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var bindings = (Exhibition.UI.LocalizedImageSet.Binding[])typeof(Exhibition.UI.LocalizedImageSet).GetField("bindings", flags).GetValue(ui);
        var doc = JsonUtility.FromJson<Exhibition.UI.ImageLanguageDocument>(System.IO.File.ReadAllText(
            System.IO.Path.Combine(Application.streamingAssetsPath, manager.LanguageCode == "en" ? "Lang_En.json" : "Lang_Th.json")));
        foreach (var binding in bindings)
        {
            var entry = System.Linq.Enumerable.First(doc.images, e => e.id == binding.id);
            var rect = binding.image.rectTransform;
            if (Vector2.Distance(rect.anchoredPosition, new Vector2(entry.x, -entry.y)) > .02f ||
                Vector2.Distance(rect.sizeDelta, new Vector2(entry.width, entry.height)) > .02f || binding.image.sprite.name != entry.sprite)
                throw new System.Exception("Geometry mismatch: " + binding.id);
        }
        var root = ui.transform;
        if (root.GetComponentsInChildren<TMPro.TMP_Text>(true).Length != 0 || root.GetComponentsInChildren<UnityEngine.UI.Text>(true).Length != 0)
            throw new System.Exception("Text components found in image UI.");
        Check(true, manager.LanguageCode + " " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + " localized geometry and image-only UI");
    }
    async Cysharp.Threading.Tasks.UniTask<Exhibition.Blocks.BlockExperienceController> StartGame()
    {
        await Tap();
        await Wait(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "1_Game" && !manager.IsTransitioning);
        var controller = UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();
        await Wait(() => controller && controller.IsReady);
        var ui = UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.LocalizedImageSet>();
        await Wait(() => ui.IsReady);
        Geometry(ui);
        return controller;
    }
    async Cysharp.Threading.Tasks.UniTask FinishSelection(Exhibition.Blocks.BlockExperienceController controller, string language)
    {
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group, "student");
        await Tap();
        Check(UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.BlockExperienceView>().IsChoosingProduct, language + " group selection advances only after recognition");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product, "bags");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1, "red");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2, "blue");
        await Wait(() => !controller.IsGenerating);
        Check(manager.ResultTexture && manager.ResultTexture.width == 480, language + " automatic coloring preserved");
        var frameImages = controller.gameObject.scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<UnityEngine.UI.Image>(true))
            .Where(i => i.name.StartsWith("GroupFrame") || i.name.StartsWith("SelectionFrame")).ToArray();
        Check(frameImages.Length == 8 && frameImages.All(i => i.color == Color.white && i.transform.childCount == 1 &&
            i.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().enabled &&
            i.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite), language + " selected block artwork appears inside untinted frames");
        await Capture("selection_" + language);
        await Tap();
        await Wait(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "2_Result" && !manager.IsTransitioning);
        var ui = UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.LocalizedImageSet>();
        await Wait(() => ui.IsReady); await Frames(); Geometry(ui);
        Check(manager.ResultTexture, language + " generated texture preserved in Result");
        await Capture("result_" + language);
    }
    try
    {
        var viewType = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.GameView");
        UnityEditor.EditorWindow.GetWindow(viewType).Repaint();
        manager = UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();
        await Wait(() => manager && manager.IsInputReady);
        simulator = UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
        simulatorWasEnabled = simulator && simulator.enabled;
        if (simulator) simulator.enabled = false;
        await Frames();
        var timer = UnityEngine.Object.FindFirstObjectByType<HuliacDev.Core.InactivityTimer>();
        var ui = UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.LocalizedImageSet>();
        await Wait(() => ui.IsReady && !manager.IsPresentationLocked); Geometry(ui); await Capture("title_th");
        Check(manager.LanguageCode == "th", "Initial title language Thai");
        var background = GameObject.Find("Intro_bg").GetComponent<UnityEngine.UI.Image>();
        var originalBackground = background.sprite;
        manager.SetButtonPressed(true); await Frames(); await Delay(2.08);
        Check(manager.IsLanguageChanging, "Two-second hold begins language fade without starting a session");
        Check(background.sprite == originalBackground && background.GetComponentsInParent<CanvasGroup>().All(g => Mathf.Abs(g.alpha - 1) < .0001f), "Background excluded from language fade");
        await Wait(() => !manager.IsLanguageChanging);
        Check(manager.LanguageCode == "en" && manager.IsTitleScreen, "Two-second title hold switches to English");
        manager.SetButtonPressed(false); await Frames(); Geometry(ui); await Capture("title_en");
        // Disabling during a fade must not leave a half-transparent or mixed-language title.
        var operation = ui.ApplyLanguageAsync(Core.SessionLanguage.Thai, true);
        await Frames(); ui.enabled = false;
        Check(!await operation, "Cancelled language fade reports cancellation");
        ui.enabled = true; await Wait(() => ui.IsReady);
        Check(ui.AppliedLanguage == manager.CurrentLanguage, "Re-enabled title restores current language");
        // Deliberately malformed local JSON; guaranteed restoration below, no user artwork is modified.
        savedJson = System.IO.File.ReadAllText(languagePath);
        ui.enabled = false; System.IO.File.WriteAllText(languagePath, "{ invalid UI-LANG-001 test }");
        ui.enabled = true; await Wait(() => ui.IsReady);
        Check(ui.UsingFallback && ui.AppliedLanguage == Core.SessionLanguage.English, "Invalid language JSON uses authored fallback without blocking input");
        System.IO.File.WriteAllText(languagePath, savedJson); savedJson = null;
        ui.enabled = false; ui.enabled = true; await Wait(() => ui.IsReady);
        Check(!ui.UsingFallback, "Restored language JSON loads normally");
        var controller = await StartGame();
        await Tap(); Check(!UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.BlockExperienceView>().IsChoosingProduct, "Unselected group cannot advance");
        await Capture("group_en");
        await Hold(2.15); Check(manager.LanguageCode == "en", "Language hold disabled in Game");
        await FinishSelection(controller, "en");
        var video = UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.ResultVideoController>();
        ui = UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.LocalizedImageSet>();
        var docField = typeof(Exhibition.UI.LocalizedImageSet).GetField("_english", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var doc = (Exhibition.UI.ImageLanguageDocument)docField.GetValue(ui);
        string validVideo = doc.video; doc.video = "Videos/Missing_UI_LANG_001";
        await Tap(); await Wait(() => video.State == Exhibition.UI.ResultVideoController.ScreenState.Failed);
        await Wait(() => !manager.IsPresentationLocked);
        Check(!timer.IsPaused, "Missing video failure resumes inactivity timer");
        doc.video = validVideo; await Tap();
        await Wait(() => video.State == Exhibition.UI.ResultVideoController.ScreenState.Playing);
        var player = video.GetComponent<UnityEngine.Video.VideoPlayer>();
        Check(player.clip.name == "Video_en" && timer.IsPaused, "English video retry plays correct clip and pauses inactivity timer");
        await Delay(.5); await Capture("video_en");
        await Hold(5.1); await Idle();
        Check(!timer.IsPaused && !manager.ResultTexture && !manager.HasLanguageSelection && manager.LanguageCode == "en", "Five-second return during video cancels playback, clears image/session, resumes timer and keeps language");
        await Hold(2.1); await Wait(() => !manager.IsLanguageChanging);
        Check(manager.LanguageCode == "th", "Repeated title hold toggles back to Thai");
        controller = await StartGame(); await Capture("group_th");
        await FinishSelection(controller, "th");
        video = UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.ResultVideoController>();
        await Tap(); await Wait(() => video.State == Exhibition.UI.ResultVideoController.ScreenState.Playing);
        player = video.GetComponent<UnityEngine.Video.VideoPlayer>();
        Check(player.clip.name == "Video_th" && timer.IsPaused, "Thai video plays correct clip");
        await Delay(.5); await Capture("video_th");
        player.time = System.Math.Max(0, player.clip.length - .5);
        await Wait(() => video.State == Exhibition.UI.ResultVideoController.ScreenState.Finished);
        await Wait(() => !manager.IsPresentationLocked);
        Check(!timer.IsPaused, "Decoder end event shows localized overlay and resumes timer");
        await Capture("video_end_th");
        for (int slot = 0; slot < 4; slot++) video.SetBlockPresent(slot, false);
        await Idle(); Check(!manager.ResultTexture, "All four block removals finish experience and release result");
        await Hold(2.1); await Wait(() => !manager.IsLanguageChanging);
        controller = await StartGame(); await FinishSelection(controller, "en");
        await Hold(5.1); await Idle();
        Check(!manager.ResultTexture && !timer.IsPaused, "Five-second return from advertisement step");
        controller = await StartGame(); await FinishSelection(controller, "en");
        video = UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.ResultVideoController>();
        await Tap(); await Wait(() => video.State == Exhibition.UI.ResultVideoController.ScreenState.Playing);
        player = video.GetComponent<UnityEngine.Video.VideoPlayer>();
        player.time = System.Math.Max(0, player.clip.length - .5);
        await Wait(() => video.State == Exhibition.UI.ResultVideoController.ScreenState.Finished);
        await Capture("video_end_en");
        await Hold(5.1); await Idle();
        Check(!timer.IsPaused && !manager.ResultTexture, "Five-second return from end popup releases final frame");
        controller = await StartGame(); await FinishSelection(controller, "en");
        video = UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.ResultVideoController>();
        await Wait(() => !manager.IsPresentationLocked);
        video.SendMessage("Continue", SendMessageOptions.RequireReceiver);
        manager.ReturnToIdleManually(); await Idle();
        Check(!timer.IsPaused && !manager.ResultTexture, "Return during video preparation cancels pending load");
        await Hold(2.1); await Wait(() => !manager.IsLanguageChanging);
        controller = await StartGame();
        await Hold(5.1); await Idle(); Check(!manager.HasLanguageSelection, "Five-second return from group step");
        controller = await StartGame();
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group, "child"); await Tap();
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product, "shoes");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1, "green");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2, "yellow");
        var sessionToken = manager.SessionCancellationToken;
        await Hold(5.1); await Idle();
        Check(sessionToken.IsCancellationRequested && !manager.ResultTexture, "Five-second return from product step cancels session and coloring");
        Check(UnityEngine.Object.FindObjectsByType<Core.GameManager>(FindObjectsSortMode.None).Length == 1 &&
            UnityEngine.Object.FindObjectsByType<Core.KeyboardButtonSimulator>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length == 1, "Repeated home returns keep one manager and input adapter");
        Check(UnityEngine.Resources.FindObjectsOfTypeAll<RenderTexture>().Count(t => t.name == "ExhibitionVideo") == 0, "No owned video render texture remains after home");
        report["passed"] = true;
    }
    catch (System.Exception ex)
    {
        report["passed"] = false; report["error"] = ex.ToString(); Debug.LogException(ex);
    }
    finally
    {
        if (savedJson != null) System.IO.File.WriteAllText(languagePath, savedJson);
        if (manager) manager.SetButtonPressed(false);
        if (simulator) simulator.enabled = simulatorWasEnabled;
        Application.runInBackground = oldBackground;
        System.IO.File.WriteAllText("docs/validation/ui/rfid_ui_regression.json", report.ToString());
        UnityEditor.SessionState.SetString(key, "done");
    }
});
return "UI runtime validation started; report: docs/validation/ui/rfid_ui_regression.json";
