#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using Exhibition.Blocks;
using Exhibition.UI;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Explicit development-build smoke test; normal Play/Player startup never creates this component.
public sealed class RfidPlayerSmoke : MonoBehaviour
{
    private string _report;
    private bool _quit;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == "--rfid-smoke") { Begin(args[i + 1], true); return; }
    }

    public static string Begin(string report, bool quit)
    {
        if (FindFirstObjectByType<RfidPlayerSmoke>()) return "RFID verification is already running.";
        var runner = new GameObject("RfidPlayerSmoke").AddComponent<RfidPlayerSmoke>();
        DontDestroyOnLoad(runner.gameObject);
        runner._report = Path.GetFullPath(report); runner._quit = quit;
        runner.RunAsync().Forget();
        return "RFID verification started: " + runner._report;
    }

    private async UniTask RunAsync()
    {
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(110));
        var token = deadline.Token;
        var checks = new JArray();
        var report = new JObject { ["task"] = "RFID-BLOCKS-001", ["unity"] = Application.unityVersion,
            ["editor"] = Application.isEditor, ["checks"] = checks };
        Directory.CreateDirectory(Path.GetDirectoryName(_report));
        GameManager manager = null;
        RfidSimulator sim = null;
        KeyboardButtonSimulator adapter = null;
        bool adapterEnabled = false, oldBackground = Application.runInBackground;
        var oldKeyboard = Keyboard.current;
        Keyboard keyboard = null;
        var oldFocus = InputSystem.settings.backgroundBehavior;
        Application.runInBackground = true;
        void Check(bool condition, string label)
        {
            if (!condition) throw new Exception("RFID validation failed: " + label);
            checks.Add(label);
            File.WriteAllText(Path.ChangeExtension(_report, ".progress.json"), report.ToString());
        }
        async UniTask Frames(int count = 2) { for (int i = 0; i < count; i++) await UniTask.Yield(PlayerLoopTiming.Update, token); }
        async UniTask Wait(Func<bool> condition) => await UniTask.WaitUntil(condition, cancellationToken: token);
        async UniTask Ready() => await Wait(() => manager && !manager.IsTransitioning && !manager.IsPresentationLocked);
        async UniTask Tap()
        {
            await Ready(); manager.SetButtonPressed(false); await Frames();
            manager.SetButtonPressed(true); await Frames(); manager.SetButtonPressed(false); await Frames();
        }
        async UniTask F9()
        {
            bool before = sim.IsVisible;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.F9)); await Frames(2);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); await Frames(2);
            Check(sim.IsVisible != before, "F9 keyboard input toggles the built-in RFID window");
        }
        async UniTask Capture(string suffix)
        {
            await UniTask.WaitForEndOfFrame(this, token);
            var image = ScreenCapture.CaptureScreenshotAsTexture();
            try { File.WriteAllBytes(Path.ChangeExtension(_report, "." + suffix + ".png"), image.EncodeToPNG()); }
            finally { Destroy(image); }
        }
        async UniTask Put(int reader, BlockKind kind, string id)
        { await Ready(); Check(sim.Place(reader, kind, id), "Reader " + (reader + 1) + " accepts " + kind + "/" + id); await Ready(); }
        try
        {
            await Wait(() => (manager = FindFirstObjectByType<GameManager>()) && manager.IsInputReady);
            await Ready();
            sim = FindFirstObjectByType<RfidSimulator>(); await Wait(() => sim && sim.CanEdit);
            var hub = sim.Hub;
            Check(hub.Artwork.entries.Length == 20, "All 20 supplied artwork references are available at runtime");
            foreach (var entry in hub.Artwork.entries) Check(entry.sprite && entry.sprite.texture.width == (entry.kind == BlockKind.Group ? 261 : 252),
                "Artwork is imported at its original size: " + entry.id);
            adapter = FindFirstObjectByType<KeyboardButtonSimulator>(); adapterEnabled = adapter && adapter.enabled;
            if (adapter) adapter.enabled = false;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            keyboard = InputSystem.AddDevice<Keyboard>("RFID verification keyboard");
            await F9(); Check(sim.IsVisible && Cursor.visible, "Test window exposes the cursor");
            await Put(2, BlockKind.Group, "child");
            int revision = hub.Readers.Revision;
            await F9(); Check(hub.Readers[2]?.Id == "child" && hub.Readers.Revision == revision, "Hiding F9 UI keeps reader occupancy");
            await F9(); await Capture("palette");
            await Tap(); await Ready();
            var controller = FindFirstObjectByType<BlockExperienceController>(); await Wait(() => controller && controller.IsReady); await Ready();
            var view = FindFirstObjectByType<BlockExperienceView>();
            Check(sim.IsVisible && hub.Readers[2]?.Id == "child", "F9 visibility and physical reader state survive Idle-to-Game");
            Check(controller.Catalog.products.Length == 10 && controller.Catalog.groups.Length == 4 && controller.Catalog.colors.Length == 6,
                "Selectable catalog contains only the 20 pictured blocks");
            Check(Array.Find(controller.Catalog.products, p => p.id == "equipment" || p.id == "phone_cases") == null,
                "Products with no image are absent from selection");
            Check(view.AreRequiredBlocksRecognized && !view.IsChoosingProduct, "First group step still accepts one group block in any reader");
            var groupImage = view.transform.Find("GroupSelection/GroupFrame0/BlockContent").GetComponent<UnityEngine.UI.Image>();
            Check(groupImage.enabled && groupImage.sprite.name == "ChildBlock", "Group artwork fills the group slot from any physical reader");
            await Tap(); await Ready(); Check(view.IsChoosingProduct, "Group-to-placement motion preserves existing progress");
            var selection = view.transform.Find("ProductAndColors");
            var fields = typeof(BlockExperienceView).GetField("selectionContinue", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var hint = (GameObject)fields.GetValue(view);
            Check(groupImage.sprite.name == selection.Find("SelectionFrame0/BlockContent").GetComponent<UnityEngine.UI.Image>().sprite.name,
                "The moving original frame hands off the same block artwork at its destination");
            hub.ClearReaders(); await Ready();
            await Put(0, BlockKind.Group, "child"); await Put(1, BlockKind.Group, "senior");
            await Put(2, BlockKind.Product, "bags"); await Put(3, BlockKind.Color, "red");
            Check(hub.Readers.OccupiedCount == 4 && !view.AreRequiredBlocksRecognized && !hint.activeSelf && !manager.ResultTexture,
                "Four occupied readers with two groups cannot reveal the hint or produce a result");
            await Tap(); Check(!manager.IsTransitioning && view.IsChoosingProduct, "Short button input is ignored for an invalid four-block composition");
            await Put(1, BlockKind.Color, "blue"); await Wait(() => !controller.IsGenerating); await Ready();
            Check(view.AreRequiredBlocksRecognized && hint.activeInHierarchy && manager.ResultTexture,
                "Exactly one group, one product and two colors enables the placement hint and result");
            await Put(2, BlockKind.Color, "yellow");
            Check(hub.Readers.OccupiedCount == 4 && !view.AreRequiredBlocksRecognized && !hint.activeSelf && !manager.ResultTexture,
                "One group plus three colors stays on the placement page with continue blocked");
            await Put(0, BlockKind.Product, "shoes");
            Check(!view.AreRequiredBlocksRecognized && !hint.activeSelf, "Four readers without a group stay blocked");
            await Put(1, BlockKind.Product, "bags");
            Check(!view.AreRequiredBlocksRecognized && !hint.activeSelf, "Duplicate products do not satisfy the required composition");
            hub.ClearReaders(); await Ready();
            await Put(0, BlockKind.Color, "blue"); await Put(1, BlockKind.Group, "student");
            await Put(2, BlockKind.Product, "bags"); await Put(3, BlockKind.Color, "red");
            await Wait(() => !controller.IsGenerating); await Ready();
            Check(controller.Selection[BlockSlot.Group] == "student" && controller.Selection[BlockSlot.Product] == "bags" &&
                controller.Selection[BlockSlot.Color1] == "blue" && controller.Selection[BlockSlot.Color2] == "red",
                "Logical generation inputs derive from arbitrary reader positions with ordered color roles");
            for (int i = 0; i < 4; i++)
            {
                var image = selection.Find("SelectionFrame" + i + "/BlockContent").GetComponent<UnityEngine.UI.Image>();
                Check(image.enabled && image.sprite == hub.Artwork.Find(hub.Readers[i]), "Reader image matches its own physical position: " + (i + 1));
                Check(image.transform.parent.GetComponent<UnityEngine.UI.Image>().color == Color.white, "Empty-frame tint stays unchanged: " + (i + 1));
            }
            await F9(); await Capture("blocks"); await F9();
            Check(sim.Remove(0), "A single reader can be removed independently"); await Ready();
            Check(hub.Readers[1]?.Id == "student" && hub.Readers[2]?.Id == "bags" && hub.Readers[3]?.Id == "red" &&
                !selection.Find("SelectionFrame0/BlockContent").GetComponent<UnityEngine.UI.Image>().enabled && !hint.activeSelf,
                "Removing a reader empties only that image and hides continue without returning panels");
            await Put(0, BlockKind.Color, "blue"); await Wait(() => !controller.IsGenerating); await Ready();
            Check(!hub.RecognizeReader(2, BlockKind.Product, "equipment") && !hub.Readers[2].HasValue && !view.AreRequiredBlocksRecognized,
                "Unregistered/image-less hardware tags cannot leave a stale valid reader"); await Ready();
            await Put(2, BlockKind.Product, "bags"); await Wait(() => !controller.IsGenerating); await Ready();
            Check(sim.Move(0, 3), "An occupied reader can be dragged onto another reader"); await Ready();
            Check(!hub.Readers[0].HasValue && hub.Readers[3]?.Id == "blue" && hub.Readers[1]?.Id == "student" &&
                !view.AreRequiredBlocksRecognized && !hint.activeSelf, "Moving replaces the target and empties only the source");
            await Put(0, BlockKind.Color, "red"); await Wait(() => !controller.IsGenerating); await Ready();
            await Tap(); await Ready();
            Check(FindFirstObjectByType<Exhibition.UI.ResultVideoController>() && sim.IsVisible && hub.Readers.HasRequiredComposition,
                "Reader state and the F9 test window persist in the Result scene");
            await F9(); await Tap();
            var video = FindFirstObjectByType<ResultVideoController>();
            await Wait(() => video.State == ResultVideoController.ScreenState.Playing); await Ready();
            Check((long)typeof(ResultVideoController).GetField("_firstFrame", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(video) == 0,
                "RFID integration preserves decoded video frame zero");
            var player = video.GetComponent<UnityEngine.Video.VideoPlayer>();
            player.time = Math.Max(0, player.clip.length - .5);
            await Wait(() => video.State == ResultVideoController.ScreenState.Finished); await Ready(); await F9();
            Check(sim.Remove(1), "RFID removal is available after leaving the placement scene"); await Ready();
            Check(sim.Remove(2) && sim.Remove(3), "Result observes independent removal signals from two more readers"); await Ready();
            Check(!manager.IsTitleScreen && hub.Readers[0].HasValue, "Completion waits for the remaining physical reader");
            Check(sim.Remove(0), "The final reader removal completes the video experience");
            await Ready(); await Frames();
            Check(manager.IsTitleScreen && hub.Readers.OccupiedCount == 0 && !manager.ResultTexture,
                "Home clears the four-reader session and the generated result");
            Check(FindObjectsByType<RfidReaderHub>(FindObjectsSortMode.None).Length == 1 && FindObjectsByType<RfidSimulator>(FindObjectsSortMode.None).Length == 1,
                "Returning home does not duplicate the persistent reader hub or simulator");
            await F9(); Check(!sim.IsVisible, "F9 still closes the same simulator after repeated scene transitions");
            report["passed"] = true;
        }
        catch (Exception ex) { report["passed"] = false; report["error"] = ex.ToString(); Debug.LogException(ex); }
        finally
        {
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            if (oldKeyboard != null && oldKeyboard.added) oldKeyboard.MakeCurrent();
            InputSystem.settings.backgroundBehavior = oldFocus;
            if (manager) manager.SetButtonPressed(false);
            if (adapter) adapter.enabled = adapterEnabled;
            Application.runInBackground = oldBackground;
            File.WriteAllText(_report, report.ToString());
            Debug.Log("[RFID verification] " + (report.Value<bool?>("passed") == true ? "PASS" : "FAIL") + ": " + _report);
            if (_quit) Application.Quit(report.Value<bool?>("passed") == true ? 0 : 1);
            Destroy(gameObject);
        }
    }
}
#endif
