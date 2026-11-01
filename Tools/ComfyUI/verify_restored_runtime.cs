// Run via Unity MCP execute_code from Idle in Play mode. Uses the actual local ComfyUI for one generation.
if (GameObject.Find("ComfyRestoreVerification")) return "Comfy restoration verification is already running.";
var verificationRoot = new GameObject("ComfyRestoreVerification");
UnityEngine.Object.DontDestroyOnLoad(verificationRoot);
Cysharp.Threading.Tasks.UniTask.Void(async () =>
{
    var checks = new Newtonsoft.Json.Linq.JArray();
    var report = new Newtonsoft.Json.Linq.JObject { ["task"] = "COMFY-RESTORE-001", ["checks"] = checks };
    using var limit = new System.Threading.CancellationTokenSource(System.TimeSpan.FromSeconds(300));
    var token = limit.Token;
    var manager = UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();
    var keyboard = UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
    bool oldKeyboard = keyboard && keyboard.enabled, oldBackground = Application.runInBackground;
    if (keyboard) keyboard.enabled = false;
    Application.runInBackground = true;
    Exhibition.Blocks.BlockExperienceController blocks = null;
    var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
    void Save() => System.IO.File.WriteAllText("docs/validation/comfy_restore/runtime.json", report.ToString());
    void Check(bool value, string label) { if (!value) throw new Exception(label); checks.Add(label); Save(); }
    async Cysharp.Threading.Tasks.UniTask Frames(int n = 2) { for (int i=0; i<n; i++) await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update, token); }
    async Cysharp.Threading.Tasks.UniTask Wait(Func<bool> predicate) => await Cysharp.Threading.Tasks.UniTask.WaitUntil(predicate, cancellationToken: token);
    async Cysharp.Threading.Tasks.UniTask Ready() => await Wait(() => !manager.IsTransitioning && !manager.IsPresentationLocked);
    async Cysharp.Threading.Tasks.UniTask Delay(int ms) => await Cysharp.Threading.Tasks.UniTask.Delay(ms, Cysharp.Threading.Tasks.DelayType.Realtime, cancellationToken: token);
    async Cysharp.Threading.Tasks.UniTask Tap() { manager.SetButtonPressed(false); await Frames(); manager.SetButtonPressed(true); await Frames(); manager.SetButtonPressed(false); await Frames(); }
    async Cysharp.Threading.Tasks.UniTask Enter()
    {
        await Ready(); await Tap(); await Ready();
        blocks = UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();
        await Wait(() => blocks && blocks.IsReady);
    }
    Exhibition.UI.BlockExperienceView View() => UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.BlockExperienceView>();
    void Complete()
    {
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group, "student");
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product, "shoes");
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1, "red");
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2, "blue");
    }
    void DirectContinue() => View().SendMessage("Continue", SendMessageOptions.RequireReceiver);
    try
    {
        await Wait(() => manager && manager.IsInputReady);
        Check(UnityEngine.Object.FindObjectsByType<Exhibition.Blocks.ComfyServerLauncher>(FindObjectsSortMode.None).Length == 1 &&
            UnityEngine.Object.FindObjectsByType<Exhibition.Blocks.GeneratedImageRetention>(FindObjectsSortMode.None).Length == 1,
            "Exactly one persistent launcher and retention service are registered");
        await Enter();
        var hub = blocks.ReaderHub;
        foreach (var option in blocks.Catalog.products)
        {
            Check(hub.Artwork.Find(Exhibition.Blocks.BlockKind.Product, option.id) != null &&
                blocks.Catalog.Find(Exhibition.Blocks.BlockSlot.Product, option.id) != null,
                "Current product artwork and generation option: " + option.id);
        }
        Complete();
        double started = Time.realtimeSinceStartupAsDouble;
        Check(blocks.IsGenerating && manager.IsPresentationLocked, "Full composition locks input immediately on the group panel");
        DirectContinue(); await Tap();
        Check(!View().IsChoosingProduct, "Group panel ignores direct Continue and physical taps while generating");
        manager.SetButtonPressed(true); await Delay(5300); manager.SetButtonPressed(false); await Frames();
        Check(!manager.IsTitleScreen && !manager.IsTransitioning && !View().IsChoosingProduct, "Five-second hold during generation is discarded");
        await Wait(() => !blocks.IsGenerating);
        Check(blocks.HasGeneratedImage && manager.ResultTexture && manager.ResultTexture.width == 480 && manager.ResultTexture.height == 480,
            "Actual ComfyUI workflow returns a 480x480 RGBA image to Unity");
        report["generationSeconds"] = Time.realtimeSinceStartupAsDouble - started;
        await Ready();
        Check(blocks.Status == "Image ready.", "Success releases generation input lock");
        await Cysharp.Threading.Tasks.UniTask.WaitForEndOfFrame(blocks, token);
        var screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        System.IO.File.WriteAllBytes("docs/validation/comfy_restore/group_success.png", screenshot.EncodeToPNG());
        UnityEngine.Object.Destroy(screenshot);
        await Tap(); await Ready();
        Check(View().IsChoosingProduct, "Fresh button tap advances group panel after generation");
        var sharedTexture = manager.ResultTexture;
        // Export the actual Unity-received image, including alpha, without changing the runtime texture.
        var previousTarget = RenderTexture.active;
        var exportTarget = RenderTexture.GetTemporary(sharedTexture.width, sharedTexture.height, 0, RenderTextureFormat.ARGB32);
        var exported = new Texture2D(sharedTexture.width, sharedTexture.height, TextureFormat.RGBA32, false);
        try
        {
            Graphics.Blit(sharedTexture, exportTarget); RenderTexture.active = exportTarget;
            exported.ReadPixels(new Rect(0, 0, sharedTexture.width, sharedTexture.height), 0, 0); exported.Apply();
            System.IO.File.WriteAllBytes("docs/validation/comfy_restore/generated_product.png", exported.EncodeToPNG());
        }
        finally { RenderTexture.active = previousTarget; RenderTexture.ReleaseTemporary(exportTarget); UnityEngine.Object.Destroy(exported); }
        await Cysharp.Threading.Tasks.UniTask.WaitForEndOfFrame(blocks, token);
        var selectionShot = ScreenCapture.CaptureScreenshotAsTexture();
        System.IO.File.WriteAllBytes("docs/validation/comfy_restore/selection_success.png", selectionShot.EncodeToPNG());
        UnityEngine.Object.Destroy(selectionShot);
        await Tap(); await Ready();
        Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "2_Result" && manager.ResultTexture == sharedTexture,
            "Result scene receives the exact generated texture and existing entrance completes");
        manager.ReturnToIdleManually(); await Ready();
        Check(manager.IsTitleScreen && !manager.ResultTexture && hub.Readers.OccupiedCount == 0, "Home clears image, selection and readers");
        await Enter();
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group, "student"); await Ready(); await Tap(); await Ready();
        Check(View().IsChoosingProduct, "Group-only selection can still advance without starting generation");
        Complete();
        Check(blocks.IsGenerating && manager.IsPresentationLocked, "Product/color panel also locks immediately");
        DirectContinue(); await Tap();
        Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "1_Game", "Product/color panel ignores direct Continue and physical taps while generating");
        int staleRevision = blocks.Selection.Revision;
        blocks.RemoveBlock(Exhibition.Blocks.BlockSlot.Color2);
        await Ready();
        Check(!blocks.IsGenerating && !manager.ResultTexture && !View().AreRequiredBlocksRecognized,
            "Block removal cancels generation, releases lock and clears result");
        var stale = new Texture2D(2,2);
        Check(!blocks.TryDisplayResult(stale, staleRevision), "A late result for a previous selection is rejected");
        UnityEngine.Object.Destroy(stale);
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2, "green");
        Check(blocks.IsGenerating, "A changed color starts a replacement request");
        blocks.CancelGeneration(); await Ready();
        Check(!blocks.IsGenerating && !manager.ResultTexture, "Explicit cancellation restores input without displaying a result");
        var settings = (Exhibition.Blocks.ComfyBlockSettings)blocks.GetType().GetField("_settings",flags).GetValue(blocks);
        string realUrl = settings.serverUrl;
        settings.serverUrl = "http://127.0.0.1:1";
        blocks.GetType().GetField("_client",flags).SetValue(blocks,new Exhibition.Blocks.ComfyImageClient(settings));
        await Tap(); await Wait(() => !blocks.IsGenerating); await Ready();
        Check(blocks.Status.StartsWith("Image generation failed") && !manager.ResultTexture,
            "Unreachable server reports failure and releases the lock");
        settings.serverUrl = realUrl;
        await Tap();
        Check(blocks.IsGenerating && manager.IsPresentationLocked, "Fresh physical tap retries a failed request");
        blocks.CancelGeneration(); await Ready();
        blocks.GenerateImage();
        blocks.gameObject.SetActive(false); await Frames(); await Ready();
        Check(!blocks.IsGenerating && !manager.IsPresentationLocked, "Screen disable cancels and releases generation input");
        manager.ReturnToIdleManually(); await Ready();
        await Enter(); Complete();
        manager.ReturnToIdleManually(); await Ready();
        Check(manager.IsTitleScreen && !manager.ResultTexture && !manager.IsPresentationLocked,
            "Programmatic home during generation cancels the request and restores the session");
        report["passed"] = true;
        Check(!UnityEngine.Object.FindFirstObjectByType<HuliacDev.Core.InactivityTimer>().IsPaused, "Inactivity timer is resumed after all generation exit paths");
    }
    catch(Exception ex) { report["passed"] = false; report["error"] = ex.ToString(); Debug.LogException(ex); }
    finally
    {
        if (blocks) blocks.CancelGeneration();
        if (manager && !manager.IsTitleScreen) manager.ReturnToIdleManually();
        if (keyboard) keyboard.enabled = oldKeyboard;
        Application.runInBackground = oldBackground;
        Save();
        UnityEngine.Object.Destroy(verificationRoot);
    }
});
return "COMFY-RESTORE-001 runtime verification started.";
