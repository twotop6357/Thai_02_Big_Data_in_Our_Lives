// Run in Play from Idle after RfidPlayerSmoke completes; exercises the actual Input System mouse path.
Cysharp.Threading.Tasks.UniTask.Void(async () =>
{
    var checks = new Newtonsoft.Json.Linq.JArray();
    var report = new Newtonsoft.Json.Linq.JObject { ["task"] = "RFID-BLOCKS-001-pointer", ["checks"] = checks };
    using var deadline = new System.Threading.CancellationTokenSource(System.TimeSpan.FromSeconds(25));
    var token = deadline.Token;
    var sim = UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidSimulator>();
    bool visible = sim.IsVisible, oldBackground = Application.runInBackground;
    var oldMouse = UnityEngine.InputSystem.Mouse.current;
    UnityEngine.InputSystem.Mouse mouse = null;
    var oldFocus = UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior;
    Application.runInBackground = true;
    var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
    void Check(bool value, string name) { if (!value) throw new System.Exception("Pointer check failed: " + name); checks.Add(name); }
    async Cysharp.Threading.Tasks.UniTask Frames(int n = 3)
    { for(int i=0;i<n;i++) await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token); }
    try
    {
        await Cysharp.Threading.Tasks.UniTask.WaitUntil(() => sim.CanEdit && !UnityEngine.Object.FindFirstObjectByType<RfidPlayerSmoke>(), cancellationToken:token);
        sim.Hub.ClearReaders(); sim.SetVisible(true);
        UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior = UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
        mouse = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Mouse>("RFID pointer verification");
        await Frames();
        var rects = (Rect[])typeof(Exhibition.Blocks.RfidSimulator).GetField("_readers", flags).GetValue(sim);
        var cards = (Rect[])typeof(Exhibition.Blocks.RfidSimulator).GetField("_cards", flags).GetValue(sim);
        void Mouse(EventType kind, Vector2 normalized)
        {
            float scale = Mathf.Min(Screen.width / 1920f, Screen.height / 1080f);
            Vector2 point = normalized * scale + new Vector2((Screen.width - 1920 * scale) * .5f, (Screen.height - 1080 * scale) * .5f);
            point.y = Screen.height - point.y;
            UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState {
                position=point, buttons=(ushort)(kind==EventType.MouseUp ? 0 : 1) });
        }
        async Cysharp.Threading.Tasks.UniTask Drag(Vector2 from, Vector2 to)
        {
            Mouse(EventType.MouseDown, from); await Frames();
            Check(typeof(Exhibition.Blocks.RfidSimulator).GetField("_drag", flags).GetValue(sim) != null, "Mouse-down picks up a pictured block");
            Mouse(EventType.MouseDrag, Vector2.Lerp(from,to,.5f)); await Frames();
            Mouse(EventType.MouseDrag, to); await Frames(1);
            Mouse(EventType.MouseUp, to); await Frames();
        }
        await Drag(cards[0].center, rects[0].center);
        Check(sim.Hub.Readers[0]?.Id == "child", "Palette-to-reader drag places the block through GUI input");
        await Drag(cards[4].center, rects[1].center);
        Check(sim.Hub.Readers[1]?.Id == "bags" && sim.Hub.Readers[0]?.Id == "child", "The next reader is recognized independently");
        await Drag(rects[0].center, rects[3].center);
        Check(!sim.Hub.Readers[0].HasValue && sim.Hub.Readers[3]?.Id == "child" && sim.Hub.Readers[1]?.Id == "bags",
            "Reader-to-reader GUI drag moves only the intended block");
        await Drag(rects[3].center, new Vector2(760,930));
        Check(!sim.Hub.Readers[3].HasValue && sim.Hub.Readers[1]?.Id == "bags", "Dragging outside removes only the source reader");
        await Cysharp.Threading.Tasks.UniTask.WaitForEndOfFrame(sim,token);
        var screenshot=ScreenCapture.CaptureScreenshotAsTexture();
        try { System.IO.File.WriteAllBytes("docs/validation/ui/rfid_pointer.png",screenshot.EncodeToPNG()); }
        finally { UnityEngine.Object.Destroy(screenshot); }
        report["passed"] = true;
    }
    catch(System.Exception ex) { report["passed"] = false;report["error"] = ex.ToString();Debug.LogException(ex); }
    finally
    {
        sim.Hub.ClearReaders(); sim.SetVisible(visible); Application.runInBackground = oldBackground;
        if(mouse != null) UnityEngine.InputSystem.InputSystem.RemoveDevice(mouse);
        if(oldMouse != null && oldMouse.added) oldMouse.MakeCurrent();
        UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior = oldFocus;
        System.IO.File.WriteAllText("docs/validation/ui/rfid_pointer.json", report.ToString());
    }
});
return "Pointer drag verification started";
