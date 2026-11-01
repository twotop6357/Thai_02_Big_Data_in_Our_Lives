// Run as a Unity MCP execute_code method body, playing from a fresh 0_Idle.
const string key = "UI-SLOTS-001-validation-v2";
if (UnityEditor.SessionState.GetString(key, "") == "running") return "Already running";
UnityEditor.SessionState.SetString(key, "running");
Cysharp.Threading.Tasks.UniTask.Void(async () =>
{
    var checks = new Newtonsoft.Json.Linq.JArray();
    var report = new Newtonsoft.Json.Linq.JObject { ["task"] = "UI-SLOTS-001", ["checks"] = checks };
    using var limit = new System.Threading.CancellationTokenSource(System.TimeSpan.FromSeconds(120));
    var token = limit.Token;
    float oldScale = Time.timeScale;
    bool oldBackground = Application.runInBackground;
    Application.runInBackground = true;
    Core.GameManager manager = null;
    Core.KeyboardButtonSimulator simulator = null;
    bool simulatorEnabled = false;
    GameObject fixture = null;
    void Check(bool condition, string label)
    {
        if (!condition) throw new System.Exception("Validation failed: " + label);
        checks.Add(label);
        System.IO.File.WriteAllText("docs/validation/ui/slot_transfer_progress.json", report.ToString());
    }
    async Cysharp.Threading.Tasks.UniTask Frames(int count = 3)
    {
        for (int i = 0; i < count; i++) await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update, token);
    }
    async Cysharp.Threading.Tasks.UniTask Delay(double seconds)
        => await Cysharp.Threading.Tasks.UniTask.Delay(System.TimeSpan.FromSeconds(seconds), Cysharp.Threading.Tasks.DelayType.UnscaledDeltaTime, cancellationToken: token);
    async Cysharp.Threading.Tasks.UniTask Wait(System.Func<bool> condition)
        => await Cysharp.Threading.Tasks.UniTask.WaitUntil(condition, cancellationToken: token);
    async Cysharp.Threading.Tasks.UniTask Tap()
    {
        await Wait(() => !manager.IsTransitioning && !manager.IsPresentationLocked);
        manager.SetButtonPressed(false); await Frames(); manager.SetButtonPressed(true); await Frames(); manager.SetButtonPressed(false); await Frames();
    }
    Vector3[] Corners(Transform t) { var points = new Vector3[4]; ((RectTransform)t).GetWorldCorners(points); return points; }
    float CornerDistance(Vector3[] a, Vector3[] b) => a.Zip(b, (x, y) => Vector3.Distance(x, y)).Max();
    void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
    {
        var so = new UnityEditor.SerializedObject(target); so.FindProperty(field).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo();
    }
    Exhibition.UI.UIAnimatedElement Card(GameObject target, Vector3 design)
    {
        target.transform.localScale = design;
        var element = target.AddComponent<Exhibition.UI.UIAnimatedElement>();
        var so = new UnityEditor.SerializedObject(element); so.FindProperty("motion").enumValueIndex = 1; so.ApplyModifiedPropertiesWithoutUndo();
        element.PrepareHidden(); return element;
    }
    try
    {
        manager = UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();
        await Wait(() => manager && manager.IsInputReady && !manager.IsPresentationLocked);
        simulator = UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>(); simulatorEnabled = simulator.enabled; simulator.enabled = false;
        await Frames();
        var presentation = manager.Presentation;
        Check(presentation.BlackoutAlpha == 0 && !presentation.IsInputLocked, "Initial scene is visible and unlocked after preparation");
        Check(Exhibition.UI.UIMotion.SceneSeconds == .5f && Exhibition.UI.UIMotion.PanelSeconds == .5f &&
            Exhibition.UI.UIMotion.HintSeconds == .25f && Exhibition.UI.UIMotion.BackdropSeconds == .25f &&
            Exhibition.UI.UIMotion.PopupSeconds == .3f && Exhibition.UI.UIMotion.ItemInterval == .2f, "Shared motion timings match the requested values");

        fixture = new GameObject("MotionValidationFixture", typeof(RectTransform));
        var cardObject = new GameObject("ValidationCard", typeof(RectTransform)); cardObject.transform.SetParent(fixture.transform, false);
        Vector3 design = new Vector3(.7f, 1.25f, .9f);
        var card = Card(cardObject, design);
        Time.timeScale = 0;
        double start = Time.realtimeSinceStartupAsDouble;
        var show = card.SetVisibleAsync(true, token);
        var duplicate = card.SetVisibleAsync(true, token);
        float largest = 0;
        while (card.IsAnimating) { largest = Mathf.Max(largest, card.transform.localScale.y); await Frames(1); }
        Check(await show && await duplicate && card.transform.localScale == design, "Repeated show is shared and final scale equals the non-unit design scale");
        Check(largest > design.y && Time.realtimeSinceStartupAsDouble - start < .65, "OutBack overshoots and completes with timeScale zero");
        var hide = card.SetVisibleAsync(false, token); await Delay(.09);
        Vector3 current = card.transform.localScale;
        var reverse = card.SetVisibleAsync(true, token);
        Check(Vector3.Distance(current, card.transform.localScale) < .0001f, "Opposite request continues at the current scale without snapping");
        Check(!await hide && await reverse && card.transform.localScale == design, "Reversed exit has no late completion that hides the re-shown card");
        await card.SetVisibleAsync(false, token);
        Check(!card.gameObject.activeSelf, "Exit disables the object only after animation completes");
        await card.SetVisibleAsync(true, token);
        Check(card.transform.localScale == design, "Re-entry restores original design scale");

        var overlay = new GameObject("ValidationOverlay", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        overlay.transform.SetParent(fixture.transform, false);
        var backdrop = overlay.GetComponent<UnityEngine.UI.Image>(); backdrop.color = new Color(.3f, .3f, .3f, .8f);
        var content = new GameObject("ValidationPopupContent", typeof(RectTransform)); content.transform.SetParent(overlay.transform, false);
        var contentMotion = Card(content, new Vector3(.8f, 1.3f, 1));
        var popup = fixture.AddComponent<Exhibition.UI.UIPopupPresentation>();
        Set(popup, "overlay", overlay); Set(popup, "backdrop", backdrop); Set(popup, "content", contentMotion);
        popup.ResetHidden(); start = Time.realtimeSinceStartupAsDouble;
        var popShow = popup.SetVisibleAsync(true, token);
        await Delay(.13);
        Check(backdrop.color.a > 0 && content.transform.localScale == Vector3.zero, "Popup backdrop darkens before card appears");
        Check(presentation.IsInputLocked, "Popup animation shares the global input lock");
        manager.SetButtonPressed(true); await Frames(); manager.SetButtonPressed(false);
        Check(await popShow && Time.realtimeSinceStartupAsDouble - start >= .53, "Backdrop 0.25 seconds precedes the 0.3-second popup animation");
        await Frames();
        Check(manager.IsTitleScreen && !manager.IsTransitioning && !presentation.IsInputLocked, "Button input during popup animation is ignored instead of queued");
        Check(content.transform.localScale == new Vector3(.8f, 1.3f, 1), "Popup retains its original non-unit scale");
        await popup.SetVisibleAsync(false, token);
        using (var cancel = new System.Threading.CancellationTokenSource())
        {
            var interrupted = popup.SetVisibleAsync(true, cancel.Token); await Delay(.1); cancel.Cancel();
            Check(!await interrupted && !overlay.activeSelf && !presentation.IsInputLocked, "Cancelled popup removes its dimmer and input lock");
        }
        UnityEngine.Object.Destroy(fixture); fixture = null;

        // Scene transition stays black until data is ready. All timing uses real time at timeScale zero.
        await Tap();
        await Delay(.15);
        Check(manager.IsTransitioning && presentation.IsInputLocked && presentation.BlackoutAlpha > 0 && presentation.BlackoutAlpha < 1,
            "Scene transition locks input and fades to black over 0.5 seconds");
        await Wait(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "1_Game");
        var scene = UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.UIScenePresentation>();
        var controller = UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();
        await Wait(() => !manager.IsTransitioning && !presentation.IsInputLocked);
        Check(scene.IsPrepared && controller.IsReady && presentation.BlackoutAlpha == 0, "Next scene is prepared before reveal and releases the blackout");
        var frames = scene.GetComponentsInChildren<Exhibition.UI.UIAnimatedElement>(true).Where(e => e.name.StartsWith("GroupFrame")).ToArray();
        Check(frames.Length == 4 && frames.All(e => e.transform.localScale == e.DesignScale && e.GetComponent<UnityEngine.UI.Image>().color == Color.white),
            "All four slots finish at the design scale without changing slot colors");
        var view = scene.GetComponent<Exhibition.UI.BlockExperienceView>();
        var viewFields = new UnityEditor.SerializedObject(view);
        var groupHint = (GameObject)viewFields.FindProperty("groupContinue").objectReferenceValue;
        var selectionHint = (GameObject)viewFields.FindProperty("selectionContinue").objectReferenceValue;
        Check(!groupHint.activeSelf && !view.AreRequiredBlocksRecognized, "Group hint stays hidden before the required group block is recognized");
        await Tap();
        Check(!view.IsChoosingProduct, "Short physical button input is blocked while the group block is missing");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group, "child");
        await Frames(1);
        var placedGroup = frames.First(e => e.name == "GroupFrame3");
        Check(placedGroup.IsAnimating && placedGroup.transform.localScale.x < placedGroup.DesignScale.x,
            "A newly placed block alone triggers the slot bounce");
        double placementStarted = Time.realtimeSinceStartupAsDouble;
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group, "child");
        await Wait(() => !presentation.IsInputLocked);
        Check(Time.realtimeSinceStartupAsDouble - placementStarted < .55 && frames.All(e => e.transform.localScale == e.DesignScale),
            "Repeated recognition does not restart the bounce and original slot scale is restored");
        Check(groupHint.activeInHierarchy && groupHint.GetComponent<CanvasGroup>().alpha == 1 && view.AreRequiredBlocksRecognized,
            "Group hint fades in after recognition and then allows continue");
        var group = scene.transform.Find("GroupSelection").GetComponent<CanvasGroup>();
        var selection = scene.transform.Find("ProductAndColors").gameObject;
        var slots = selection.GetComponentsInChildren<Exhibition.UI.UIAnimatedElement>(true).Where(e => e.name.StartsWith("SelectionFrame")).ToArray();
        var origin = frames.Select(e => Corners(e.transform)).ToArray();
        var destination = slots.Select(e => Corners(e.transform)).ToArray();
        var parents = frames.Select(e => e.transform.parent).ToArray();
        var contentChildren = frames.Select(e => e.transform.GetChild(0)).ToArray();
        await Tap(); await Delay(.15);
        Check(group.alpha > 0 && group.alpha < 1 && selection.activeSelf && selection.GetComponent<CanvasGroup>().alpha > 0 && presentation.IsInputLocked,
            "Pages crossfade concurrently while the four original frames remain visible above them");
        Check(presentation.BlackoutAlpha == 0 && !manager.IsTransitioning, "Group-to-placement transition never uses a scene blackout");
        Check(slots.All(e => !e.gameObject.activeSelf), "All four destination frames remain inactive during the move");
        Check(frames.All(e => e.gameObject.activeInHierarchy && e.transform.parent.name == "BlockSlotTransfer" && e.GetComponent<CanvasGroup>().alpha == 1),
            "The existing four frame objects move together without fading or cloning");
        Check(Enumerable.Range(0, 4).All(i => CornerDistance(Corners(frames[i].transform), origin[i]) > 1 &&
            CornerDistance(Corners(frames[i].transform), destination[i]) > 1 && contentChildren[i].parent == frames[i].transform),
            "Every frame and its existing content is between the original and destination corners");
        var beforeRemoval = frames.Select(e => Corners(e.transform)).ToArray();
        controller.RemoveBlock(Exhibition.Blocks.BlockSlot.Group);
        Check(view.IsChoosingProduct && controller.Selection[Exhibition.Blocks.BlockSlot.Group] == null &&
            Enumerable.Range(0, 4).All(i => CornerDistance(Corners(frames[i].transform), beforeRemoval[i]) < .01f),
            "Group recognition loss during travel leaves the group slot empty and continues the same forward move");
        var firstVisible = new double[4]; for (int i = 0; i < 4; i++) firstVisible[i] = -1;
        bool noBlackout = true;
        start = Time.realtimeSinceStartupAsDouble;
        while (presentation.IsInputLocked)
        {
            for (int i = 0; i < 4; i++)
                if (firstVisible[i] < 0 && slots[i].gameObject.activeSelf) firstVisible[i] = Time.realtimeSinceStartupAsDouble;
            noBlackout &= presentation.BlackoutAlpha == 0;
            await Frames(1);
        }
        for (int i = 0; i < 4; i++) if (firstVisible[i] < 0 && slots[i].gameObject.activeSelf) firstVisible[i] = Time.realtimeSinceStartupAsDouble;
        Check(noBlackout, "Blackout remains absent during slot travel");
        Check(slots.Length == 4 && firstVisible.All(t => t >= 0), "All four destination frames activate after the shared move finishes");
        Check(firstVisible.Max() - firstVisible.Min() < .03, "Destination handoff is simultaneous for all four frames");
        Check(Enumerable.Range(0, 4).All(i => CornerDistance(Corners(slots[i].transform), destination[i]) < .01f &&
            frames[i].transform.parent == parents[i] && CornerDistance(Corners(frames[i].transform), origin[i]) < .01f),
            "Final destination corners match the design and source frame layout is restored for re-entry");
        Check(Time.realtimeSinceStartupAsDouble - start < .8 && Time.timeScale == 0, "All four frames travel concurrently in unscaled time without sequential delays");
        Check(slots.All(e => e.Style == Exhibition.UI.UIAnimatedElement.Motion.Hint && e.transform.localScale == e.DesignScale),
            "Slot transfer never adds a placement bounce or changes the final frame scale");
        Check(group.alpha == 0 && !group.gameObject.activeSelf && selection.GetComponent<CanvasGroup>().alpha == 1,
            "Panel switch ends with exactly the intended visible panel");
        Check(view.IsChoosingProduct && !view.AreRequiredBlocksRecognized && !selectionHint.activeSelf,
            "Lost group recognition finishes on the placement page with continue hidden and blocked");
        await Tap();
        Check(view.IsChoosingProduct && !manager.IsTransitioning, "A missing group cannot advance or return to the previous panel");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group, "student"); await Wait(() => !presentation.IsInputLocked);
        Check(view.IsChoosingProduct && !scene.transform.Find("BlockSlotTransfer"), "Re-recognition after travel stays on the placement page without replaying the transfer");
        controller.RemoveBlock(Exhibition.Blocks.BlockSlot.Group); await Wait(() => !presentation.IsInputLocked);
        Check(view.IsChoosingProduct && !group.gameObject.activeSelf && selection.activeSelf &&
            controller.Selection[Exhibition.Blocks.BlockSlot.Group] == null, "Group loss after arrival keeps the placement page and clears only the recognized slot");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group, "student"); await Wait(() => !presentation.IsInputLocked);
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product, "bags");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1, "red");
        await Wait(() => !presentation.IsInputLocked);
        Check(!selectionHint.activeSelf && !view.AreRequiredBlocksRecognized, "Placement hint stays hidden with only three of four blocks");
        await Tap();
        Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "1_Game" && !manager.IsTransitioning,
            "Short physical button input is blocked with an incomplete four-block selection");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2, "blue");
        await Wait(() => !controller.IsGenerating && !presentation.IsInputLocked);
        Check(selectionHint.activeInHierarchy && selectionHint.GetComponent<CanvasGroup>().alpha == 1 && view.AreRequiredBlocksRecognized,
            "All four recognized blocks reveal the placement hint with fade");
        controller.RemoveBlock(Exhibition.Blocks.BlockSlot.Color2);
        await Wait(() => !presentation.IsInputLocked);
        Check(!selectionHint.activeSelf && !view.AreRequiredBlocksRecognized, "Removing any required block fades out the hint and blocks continue");
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2, "blue");
        await Wait(() => !controller.IsGenerating && !presentation.IsInputLocked);
        Check(selectionHint.activeInHierarchy && view.AreRequiredBlocksRecognized, "Replacing the required block restores the hint and continue readiness");
        Check(manager.ResultTexture && manager.ResultTexture.width == 480, "Existing coloring and texture handoff remain functional");
        Check(selection.GetComponentsInChildren<Exhibition.UI.UIAnimatedElement>(true).First(e => e.name == "ResultImage").Style == Exhibition.UI.UIAnimatedElement.Motion.Hint,
            "Right-side generated image uses fade only");
        await Tap(); await Wait(() => !manager.IsTransitioning && !presentation.IsInputLocked);
        Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "2_Result" && manager.ResultTexture,
            "Existing Game-to-Result action is preserved with presentation added");
        Check(UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.UIScenePresentation>().GetComponentsInChildren<Exhibition.UI.UIAnimatedElement>(true)
            .First(e => e.name == "ResultImage").Style == Exhibition.UI.UIAnimatedElement.Motion.Hint, "Advertisement image uses fade only");
        manager.ReturnToIdleManually(); await Wait(() => !manager.IsTransitioning && !presentation.IsInputLocked);
        Check(manager.IsTitleScreen && !manager.ResultTexture && presentation.BlackoutAlpha == 0, "Manual home clears visual state and retains original session cleanup");
        await Tap(); await Wait(() => !manager.IsTransitioning && !presentation.IsInputLocked);
        controller = UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group, "child"); await Tap(); await Delay(.12);
        manager.ReturnToIdleManually(); await Wait(() => !manager.IsTransitioning && !presentation.IsInputLocked); await Frames();
        Check(!UnityEngine.Object.FindObjectsOfType<RectTransform>().Any(t => t.name == "BlockSlotTransfer") && presentation.BlackoutAlpha == 0,
            "Home during frame travel cancels the transfer and leaves no moving layer or input lock");
        manager.SetButtonPressed(true); await Delay(2.1); manager.SetButtonPressed(false);
        await Wait(() => !manager.IsLanguageChanging && !presentation.IsInputLocked);
        Check(manager.LanguageCode == "en", "Repeated entry keeps the existing title language toggle");
        await Tap(); await Wait(() => !manager.IsTransitioning && !presentation.IsInputLocked);
        scene = UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.UIScenePresentation>();
        controller = UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();
        slots = scene.GetComponentsInChildren<Exhibition.UI.UIAnimatedElement>(true).Where(e => e.name.StartsWith("SelectionFrame")).ToArray();
        destination = slots.Select(e => Corners(e.transform)).ToArray();
        controller.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group, "senior"); await Tap(); await Delay(.1);
        Check(slots.All(e => !e.gameObject.activeSelf) && presentation.BlackoutAlpha == 0, "English page uses the same hidden-destination movement");
        await Wait(() => !presentation.IsInputLocked); await Frames();
        Check(Enumerable.Range(0, 4).All(i => slots[i].gameObject.activeInHierarchy && CornerDistance(Corners(slots[i].transform), destination[i]) < .01f),
            "English transfer finishes at all four exact design positions on repeated entry");
        manager.ReturnToIdleManually(); await Wait(() => !manager.IsTransitioning && !presentation.IsInputLocked);
        manager.SetButtonPressed(true); await Delay(2.1); manager.SetButtonPressed(false);
        await Wait(() => !manager.IsLanguageChanging && !presentation.IsInputLocked);
        report["passed"] = true;
    }
    catch (System.Exception ex) { report["passed"] = false; report["error"] = ex.ToString(); Debug.LogException(ex); }
    finally
    {
        if (fixture) UnityEngine.Object.Destroy(fixture);
        Time.timeScale = oldScale; Application.runInBackground = oldBackground;
        if (manager) manager.SetButtonPressed(false);
        if (simulator) simulator.enabled = simulatorEnabled;
        System.IO.File.WriteAllText("docs/validation/ui/slot_transfer_motion.json", report.ToString());
        UnityEditor.SessionState.SetString(key, "done");
    }
});
return "Motion checks dispatched; report: docs/validation/ui/slot_transfer_motion.json";
