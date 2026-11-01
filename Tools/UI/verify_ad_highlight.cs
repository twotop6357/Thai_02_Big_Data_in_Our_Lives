// Historical AD-FX-001 checks. AD-FX-002 replaces the finite-aura requirements; use verify_ad_highlight_loop.cs.
Cysharp.Threading.Tasks.UniTask.Void(async () =>
{
    var checks = new Newtonsoft.Json.Linq.JArray();
    var report = new Newtonsoft.Json.Linq.JObject { ["task"] = "AD-FX-001", ["checks"] = checks, ["unity"] = Application.unityVersion };
    using var limit = new System.Threading.CancellationTokenSource(System.TimeSpan.FromSeconds(70));
    var token = limit.Token;
    var manager = UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();
    var keyboard = UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
    bool keyboardEnabled = keyboard && keyboard.enabled;
    if (keyboard) keyboard.enabled = false;
    bool background = Application.runInBackground;
    Application.runInBackground = true;
    var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
    void Save() => System.IO.File.WriteAllText("docs/validation/ad_fx/runtime.json", report.ToString());
    void Check(bool ok, string message) { if (!ok) throw new System.Exception(message); checks.Add(message); Save(); }
    async Cysharp.Threading.Tasks.UniTask Frames(int n = 2)
    { for (int i=0;i<n;i++) await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token); }
    async Cysharp.Threading.Tasks.UniTask Wait(System.Func<bool> condition)
    { await Cysharp.Threading.Tasks.UniTask.WaitUntil(condition,cancellationToken:token); }
    async Cysharp.Threading.Tasks.UniTask Ready() => await Wait(()=>!manager.IsTransitioning && !manager.IsPresentationLocked);
    async Cysharp.Threading.Tasks.UniTask Tap()
    { manager.SetButtonPressed(false); await Frames(); manager.SetButtonPressed(true); await Frames(); manager.SetButtonPressed(false); await Frames(); }
    async Cysharp.Threading.Tasks.UniTask Capture(string filename, MonoBehaviour owner)
    {
        await Cysharp.Threading.Tasks.UniTask.WaitForEndOfFrame(owner,token);
        var image = ScreenCapture.CaptureScreenshotAsTexture();
        System.IO.File.WriteAllBytes("docs/validation/ad_fx/"+filename+".png",image.EncodeToPNG());
        UnityEngine.Object.Destroy(image);
    }
    async Cysharp.Threading.Tasks.UniTask EnterResult()
    {
        await Ready(); await Tap(); await Ready();
        var blocks = UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();
        await Wait(()=>blocks && blocks.IsReady);
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group,"student");
        await Frames(); await Ready(); await Tap(); await Ready();
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product,"bags");
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1,"red");
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2,"blue");
        await Wait(()=>!blocks.IsGenerating && manager.ResultTexture); await Ready();
        await Tap();
    }
    try
    {
        await EnterResult();
        Exhibition.UI.AdProductHighlight effect = null;
        await Wait(()=> (effect=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.AdProductHighlight>()) && effect.IsPlaying);
        double start = Time.realtimeSinceStartupAsDouble;
        var so = new UnityEditor.SerializedObject(effect);
        var root = (GameObject)so.FindProperty("effectRoot").objectReferenceValue;
        var particles = root.GetComponentsInChildren<AssetKits.ParticleImage.ParticleImage>(true);
        var controller = effect.GetComponent<Exhibition.UI.ResultVideoController>();
        Check(particles.Length==3,"Two Item branches including the nested ring are present");
        Check(manager.IsPresentationLocked,"Physical and keyboard input share the effect input lock");
        Check(root.activeInHierarchy,"Effect starts only on the visible advertisement");
        Check(particles.All(p=>!p.loop && p.timeScale==AssetKits.ParticleImage.Enumerations.TimeScale.Unscaled),"All emitters are non-looping and unscaled");
        Time.timeScale=0;
        await Tap();
        controller.SendMessage("Continue",SendMessageOptions.RequireReceiver);
        Check(controller.State==Exhibition.UI.ResultVideoController.ScreenState.Advertisement,"Button taps and direct continue requests are ignored during emphasis");
        await Cysharp.Threading.Tasks.UniTask.Delay(350,Cysharp.Threading.Tasks.DelayType.UnscaledDeltaTime,cancellationToken:token);
        Check(particles.Sum(p=>p.particleCount)>0,"Product emphasis emits visible particles while the game is paused");
        await Capture("advertisement_emphasis",effect);
        var stars = particles.First(p=>p.name=="ProductSparkles");
        Check(stars.particles.SelectMany(p=>p.points).All(point=>Mathf.Abs(point.x)<245 && Mathf.Abs(point.y)<245),"Sparkles remain within the product image instead of crossing the page");
        await Wait(()=>!effect.IsPlaying);
        double elapsed=Time.realtimeSinceStartupAsDouble-start;
        report["observedSeconds"]=elapsed;
        Check(elapsed<=2,"Effect finishes within two real-time seconds even at timeScale zero");
        Check(!root.activeSelf && particles.All(p=>p.particleCount==0),"Completion clears all particles and hides the effect");
        Time.timeScale=1;
        await Ready(); await Frames(8);
        Check(controller.State==Exhibition.UI.ResultVideoController.ScreenState.Advertisement,"Ignored taps are not replayed after unlocking");
        Check(!manager.IsPresentationLocked && manager.Presentation.BlackoutAlpha==0,"Completion leaves input unlocked and no blackout");
        await effect.GetComponent<Exhibition.UI.UIScenePresentation>().PlayEntranceAsync(token);
        Check(!effect.IsPlaying && !root.activeSelf,"Repeated entrance notification does not restart the same advertisement effect");
        await Capture("advertisement_after",effect);
        await Tap();
        float darkest=0;
        await Wait(()=>{ darkest=Mathf.Max(darkest,manager.Presentation.BlackoutAlpha);return controller.State==Exhibition.UI.ResultVideoController.ScreenState.Playing; });
        Check(darkest>=.99f,"Advertisement to video still uses a full black transition");
        Check((long)typeof(Exhibition.UI.ResultVideoController).GetField("_firstFrame",flags).GetValue(controller)==0,"Video decoder still hands over opening frame zero");
        Check(!manager.IsPresentationLocked && manager.Presentation.BlackoutAlpha==0,"Video entry releases blackout and input lock");
        manager.ReturnToIdleManually(); await Ready();
        Check(manager.IsTitleScreen,"Home after video still returns to Idle");
        await EnterResult();
        effect=null;
        await Wait(()=> (effect=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.AdProductHighlight>()) && effect.IsPlaying);
        Check(manager.IsPresentationLocked,"Advertisement reentry starts a fresh locked effect");
        so=new UnityEditor.SerializedObject(effect); root=(GameObject)so.FindProperty("effectRoot").objectReferenceValue;
        effect.enabled=false;
        await Ready();
        Check(!root.activeSelf && !effect.IsPlaying && !manager.IsPresentationLocked,"Disabling the effect cancels it and releases its input lease");
        effect.enabled=true;
        using(var cancel=System.Threading.CancellationTokenSource.CreateLinkedTokenSource(token))
        {
            var operation=effect.PlayEntranceAsync(manager,cancel.Token).SuppressCancellationThrow();
            await Frames(3); Check(effect.IsPlaying,"Reenabled effect can start from a clean state");
            cancel.Cancel(); await operation;
            Check(!effect.IsPlaying && !root.activeSelf && !manager.IsPresentationLocked,"Cancellation clears particles and releases input");
        }
        effect.enabled=false; effect.enabled=true;
        var homeOperation=effect.PlayEntranceAsync(manager,token).SuppressCancellationThrow();
        await Frames(3); manager.ReturnToIdleManually(); await homeOperation; await Ready();
        Check(manager.IsTitleScreen && !manager.IsPresentationLocked && manager.Presentation.BlackoutAlpha==0,"Scene exit during emphasis leaves no stale effect or input lock");
        report["status"]="PASS";
    }
    catch(System.Exception ex) { report["status"]="FAIL";report["error"]=ex.ToString();Debug.LogException(ex); }
    finally
    {
        Time.timeScale=1;Application.runInBackground=background;
        if(keyboard)keyboard.enabled=keyboardEnabled;
        Save();
    }
});
return "AD-FX-001 runtime validation started";
