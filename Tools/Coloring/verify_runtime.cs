// Execute as a method body through Unity MCP while playing from 0_Idle.
// SessionState guard prevents an MCP retry from starting duplicate asynchronous checks.
const string key = "COLOR-001-runtime-v2";
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
    try
    {
        manager = UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();
        timer = UnityEngine.Object.FindFirstObjectByType<HuliacDev.Core.InactivityTimer>();
        await Wait(() => manager && manager.IsInputReady);
        simulator = UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
        simulatorWasEnabled = simulator && simulator.enabled;
        if (simulator) simulator.enabled = false;
        await Frames(2);
        manager.SetButtonPressed(true); await Frames(3); manager.SetButtonPressed(false);
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
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product, "equipment");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1, "black");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2, "white");
        await Wait(() => !controller.IsGenerating);
        Check(manager.ResultTexture && manager.ResultTexture.name.Contains("keyboards"), "Age-dependent Equipment template");
        var ready = manager.ResultTexture;
        await Frames(3); manager.SetButtonPressed(true); await Frames(3); manager.SetButtonPressed(false);
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
        manager.SetButtonPressed(true); await Frames(3); manager.SetButtonPressed(false); await Frames(2);
        manager.SetButtonPressed(true); await Frames(3); manager.SetButtonPressed(false);
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
        System.IO.File.WriteAllText("docs/validation/coloring_runtime.json", json);
        UnityEditor.SessionState.SetString(key, json);
    }
});
return "Runtime validation dispatched";
