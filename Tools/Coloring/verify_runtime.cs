// Execute as a method body through Unity MCP while playing from a fresh Thai 0_Idle.
// UI-LANG-001: title hold toggles language; the first Game tap advances the group step.
// SessionState guard prevents an MCP retry from starting duplicate asynchronous checks.
const string key = "RFID-BLOCKS-001-color-regression";
if (UnityEditor.SessionState.GetString(key, "") == "running") return "Already running";
UnityEditor.SessionState.SetString(key, "running");
Cysharp.Threading.Tasks.UniTask.Void(async () =>
{
    var checks = new Newtonsoft.Json.Linq.JArray();
    var report = new Newtonsoft.Json.Linq.JObject { ["task"] = "COLOR-001", ["environment"] = Application.unityVersion,
        ["checks"] = checks, ["artwork"] = "48 replaceable geometric sample templates" };
    var limit = new System.Threading.CancellationTokenSource(System.TimeSpan.FromSeconds(60));
    var token = limit.Token;
    bool oldBackground = Application.runInBackground;
    Application.runInBackground = true;
    void Check(bool condition, string name)
    {
        if (!condition) throw new System.Exception("Validation failed: " + name);
        checks.Add(name);
    }
    async Cysharp.Threading.Tasks.UniTask Frames(int count = 2)
    {
        for (int i = 0; i < count; i++) await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update, token);
    }
    async Cysharp.Threading.Tasks.UniTask Wait(System.Func<bool> predicate)
    {
        await Cysharp.Threading.Tasks.UniTask.WaitUntil(predicate, cancellationToken: token);
    }
    Core.GameManager manager = null;
    HuliacDev.Core.InactivityTimer timer = null;
    Core.KeyboardButtonSimulator simulator = null;
    bool simulatorWasEnabled = false;
    async Cysharp.Threading.Tasks.UniTask Tap()
    {
        await Wait(() => !manager.IsTransitioning && !manager.IsPresentationLocked);
        manager.SetButtonPressed(false); await Frames(3);
        manager.SetButtonPressed(true); await Frames(3); manager.SetButtonPressed(false); await Frames(3);
    }
    try
    {
        manager = UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();
        timer = UnityEngine.Object.FindFirstObjectByType<HuliacDev.Core.InactivityTimer>();
        await Wait(() => manager && manager.IsInputReady && !manager.IsPresentationLocked);
        simulator = UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
        simulatorWasEnabled = simulator && simulator.enabled;
        if (simulator) simulator.enabled = false;
        await Frames(2);
        await Tap();
        await Wait(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "1_Game" && !manager.IsTransitioning);
        var controller = UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();
        await Wait(() => controller && controller.IsReady);
        Check(manager.LanguageCode == "th", "Thai session entry");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group, "child");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product, "bags");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1, "red");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2, "blue");
        await Wait(() => !controller.IsGenerating);
        Check(manager.ResultTexture && manager.ResultTexture.width == 480, "Automatic local coloring and result handoff");
        var original = manager.ResultTexture;
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1, "red"); await Frames();
        Check(manager.ResultTexture == original, "Repeated recognition reuses existing result");
        for (int i = 0; i < 3; i++)
        {
            controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1, "yellow");
            controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1, "green");
            controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group, i % 2 == 0 ? "senior" : "student");
            await Wait(() => !controller.IsGenerating);
            Check(manager.ResultTexture && manager.ResultTexture.name.StartsWith(i % 2 == 0 ? "Colored_senior" : "Colored_student"), "Latest selection wins " + i);
        }
        Check(!original, "Superseded texture destroyed");
        controller.GenerateImage(); controller.CancelGeneration(); await Frames(4);
        Check(!controller.IsGenerating && !manager.ResultTexture, "Explicit cancellation releases pending result");
        controller.GenerateImage(); await Wait(() => !controller.IsGenerating);
        Check(manager.ResultTexture, "Retry after cancellation");
        // Deliberately missing file in memory only: exercise controller failure, then recover.
        var field = typeof(Exhibition.Blocks.BlockExperienceController).GetField("_coloring", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var catalog = (Exhibition.Blocks.BlockColoringCatalog)field.GetValue(controller);
        var template = catalog.Find(controller.Selection[Exhibition.Blocks.BlockSlot.Group], "bags");
        string saved = template.baseImage;
        try
        {
            template.baseImage = "Coloring/COLOR-001-deliberately-missing.png";
            controller.GenerateImage(); await Wait(() => !controller.IsGenerating);
            Check(controller.Status.StartsWith("Coloring failed") && !manager.ResultTexture, "Missing asset failure has no stale image");
        }
        finally { template.baseImage = saved; }
        controller.GenerateImage(); await Wait(() => !controller.IsGenerating);
        Check(manager.ResultTexture, "Retry after asset restoration");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2, "white");
        controller.enabled = false; await Frames(5);
        Check(!manager.ResultTexture && !controller.Selection.Complete && !controller.IsGenerating, "Screen disable cancels and clears selection");
        controller.enabled = true; await Wait(() => controller.IsReady);
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group, "working_adult");
        await Tap();
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product, "umbrellas");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1, "black");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2, "white");
        await Wait(() => !controller.IsGenerating);
        Check(manager.ResultTexture && manager.ResultTexture.name.Contains("umbrellas"), "Age-dependent supported product template");
        var ready = manager.ResultTexture;
        await Frames(3); await Tap();
        await Wait(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "2_Result" && !manager.IsTransitioning);
        await Frames(3);
        var presenter = UnityEngine.Object.FindFirstObjectByType<Exhibition.Result.ResultImagePresenter>();
        var resultField = typeof(Exhibition.Result.ResultImagePresenter).GetField("resultImage", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var resultImage = (UnityEngine.UI.Image)resultField.GetValue(presenter);
        Check(resultImage.sprite && resultImage.sprite.texture == ready && manager.ResultTexture == ready, "Result scene displays same colored texture");
        manager.ReturnToIdleManually();
        await Wait(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "0_Idle" && !manager.IsTransitioning);
        await Frames(3);
        Check(!ready && !manager.ResultTexture && !manager.HasLanguageSelection, "Idle releases image and language selection");
        Check(timer && !timer.IsPaused, "Inactivity timer remains active");
        var ui = UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.LocalizedImageSet>();
        await Wait(() => ui && ui.IsReady);
        await Wait(() => !manager.IsPresentationLocked);
        manager.SetButtonPressed(true); await Frames(3);
        await Cysharp.Threading.Tasks.UniTask.Delay(System.TimeSpan.FromSeconds(2.1), Cysharp.Threading.Tasks.DelayType.UnscaledDeltaTime, cancellationToken: token);
        manager.SetButtonPressed(false); await Wait(() => !manager.IsLanguageChanging);
        await Frames(3);
        await Tap();
        await Wait(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "1_Game" && !manager.IsTransitioning);
        controller = UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();
        await Wait(() => controller && controller.IsReady);
        Check(manager.LanguageCode == "en", "English repeated session entry");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group, "student");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product, "shoes");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1, "yellow");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2, "black");
        var sessionToken = manager.SessionCancellationToken;
        manager.ReturnToIdleManually();
        await Wait(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "0_Idle" && !manager.IsTransitioning);
        await Frames(5);
        Check(sessionToken.IsCancellationRequested && !manager.ResultTexture, "Return during coloring cancels session without late result");
        report["result"] = "PASS";
        report["expected_error"] = "Two diagnostic entries for deliberate missing-file failure (message + original exception).";
    }
    catch (System.Exception ex)
    {
        report["result"] = "FAIL";
        report["error"] = ex.ToString();
        Debug.LogException(ex);
    }
    finally
    {
        limit.Dispose();
        Application.runInBackground = oldBackground;
        if (simulator) simulator.enabled = simulatorWasEnabled;
        string json = report.ToString();
        System.IO.Directory.CreateDirectory("docs/validation");
        System.IO.File.WriteAllText("docs/validation/ui/rfid_color_regression.json", json);
        UnityEditor.SessionState.SetString(key, json);
    }
});
return "Runtime validation dispatched";
