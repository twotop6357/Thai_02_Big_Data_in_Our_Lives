// Execute through Unity MCP from Idle in Play mode. Real rendered layering and lifetime checks.
Cysharp.Threading.Tasks.UniTask.Void(async () =>
{
    var checks=new Newtonsoft.Json.Linq.JArray();
    var report=new Newtonsoft.Json.Linq.JObject{["task"]="AD-FX-003",["checks"]=checks};
    using var limit=new System.Threading.CancellationTokenSource(System.TimeSpan.FromSeconds(100));
    var token=limit.Token;
    var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();
    var keyboard=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
    bool oldKeyboard=keyboard && keyboard.enabled,oldBackground=Application.runInBackground;
    if(keyboard)keyboard.enabled=false;
    Application.runInBackground=true;
    Texture2D opening=null,baseline=null;
    void Save()=>System.IO.File.WriteAllText("docs/validation/ad_fx3/runtime.json",report.ToString());
    void Check(bool value,string label){if(!value)throw new Exception(label);checks.Add(label);Save();}
    async Cysharp.Threading.Tasks.UniTask Frames(int n=2){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
    async Cysharp.Threading.Tasks.UniTask Wait(Func<bool> p)=>await Cysharp.Threading.Tasks.UniTask.WaitUntil(p,cancellationToken:token);
    async Cysharp.Threading.Tasks.UniTask Ready()=>await Wait(()=>!manager.IsTransitioning && !manager.IsPresentationLocked);
    async Cysharp.Threading.Tasks.UniTask Delay(int ms)=>await Cysharp.Threading.Tasks.UniTask.Delay(ms,Cysharp.Threading.Tasks.DelayType.UnscaledDeltaTime,cancellationToken:token);
    async Cysharp.Threading.Tasks.UniTask Tap(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Frames();}
    async Cysharp.Threading.Tasks.UniTask<Texture2D> Capture(string name,MonoBehaviour owner)
    {await Cysharp.Threading.Tasks.UniTask.WaitForEndOfFrame(owner,token);var t=ScreenCapture.CaptureScreenshotAsTexture();System.IO.File.WriteAllBytes("docs/validation/ad_fx3/"+name+".png",t.EncodeToPNG());return t;}
    async Cysharp.Threading.Tasks.UniTask EnterResult()
    {
        await Ready();await Tap();await Ready();
        var b=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Wait(()=>b && b.IsReady);
        b.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group,"student");await Frames();await Ready();await Tap();await Ready();
        b.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product,"bags");b.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1,"red");b.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2,"blue");
        await Wait(()=>!b.IsGenerating && manager.ResultTexture);await Ready();await Tap();
    }
    try
    {
        await EnterResult();
        Exhibition.UI.AdProductHighlight effect=null;
        await Wait(()=>(effect=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.AdProductHighlight>()) && effect.IsPlaying);
        double start=Time.realtimeSinceStartupAsDouble;
        var so=new UnityEditor.SerializedObject(effect);
        var root=(GameObject)so.FindProperty("effectRoot").objectReferenceValue;
        var confetti=(GameObject)so.FindProperty("confettiRoot").objectReferenceValue;
        var confettiEmitter=(AssetKits.ParticleImage.ParticleImage)so.FindProperty("confettiEmitter").objectReferenceValue;
        var particles=root.GetComponentsInChildren<AssetKits.ParticleImage.ParticleImage>(true);
        var glow=particles.First(p=>p.name=="ProductGlowRays");
        var ring=particles.First(p=>p.name=="ProductRing");
        var burst=particles.First(p=>p.name=="ProductSparkles");
        var product=(RectTransform)root.transform.parent.Find("ResultImage");
        var controller=effect.GetComponent<Exhibition.UI.ResultVideoController>();
        Check(root.transform.parent==product.parent && confetti.transform.parent==product.parent && product.GetSiblingIndex()>root.transform.GetSiblingIndex() && product.GetSiblingIndex()>confetti.transform.GetSiblingIndex(),"Product renders after every particle/trail layer");
        Check(glow.loop && ring.loop,"Both aura emitters loop until page exit");
        Check(Mathf.Abs(glow.rotationOverLifetime.mainCurve.constantMax/glow.lifetime.constantMax)<=15,"Aura rotates slowly at no more than 15 degrees per second");
        Check(burst.startSize.mainCurve.constantMax>=64 && glow.startSize.mainCurve.constantMax>=660,"Burst and the slow background aura retain their enlarged sizes");
        Check(Mathf.Abs(ring.startSize.mainCurve.constantMax*ring.sizeOverLifetime.mainCurve.Evaluate(0)-120)<.01f && Mathf.Abs(ring.startSize.mainCurve.constantMax*ring.sizeOverLifetime.mainCurve.Evaluate(1)-480)<.01f,"Wave starts at diameter 120 and expands only to diameter 480");
        Check(confetti.GetComponentsInChildren<AssetKits.ParticleImage.ParticleImage>(true).Length==3,"Full ConfettiFullscreen emitter and both side bursts are installed");
        Check(manager.IsPresentationLocked,"Finite entrance and confetti hold the common input lock");
        Time.timeScale=0;
        await Tap();controller.SendMessage("Continue",SendMessageOptions.RequireReceiver);
        Check(controller.State==Exhibition.UI.ResultVideoController.ScreenState.Advertisement,"Early physical taps and direct Continue are ignored");
        await Delay(450);opening=await Capture("entrance",effect);
        await Delay(1900);
        Check(effect.IsPlaying && confettiEmitter.isPlaying && confettiEmitter.particleCount>0,"Confetti remains alive beyond two seconds with timeScale zero");
        Check(glow.isPlaying && ring.isPlaying,"Aura continues during the longer confetti entrance");
        UnityEngine.Object.Destroy(await Capture("confetti_after_two_seconds",effect));
        await Wait(()=>!effect.IsPlaying);await Ready();
        report["finiteEntranceSeconds"]=Time.realtimeSinceStartupAsDouble-start;
        Check((double)report["finiteEntranceSeconds"]>2 && (double)report["finiteEntranceSeconds"]<9,"Confetti finishes naturally without a two-second cutoff or a stuck lock");
        Check(!confetti.activeSelf && confetti.GetComponentsInChildren<AssetKits.ParticleImage.ParticleImage>(true).All(p=>p.particleCount==0),"Finite confetti clears its particles and trails on completion");
        Check(effect.AreEffectsVisible && glow.isPlaying && ring.isPlaying && !manager.IsPresentationLocked,"Ambient aura remains visible after input is released");
        Check(!burst.isPlaying && burst.particleCount==0,"The enlarged opening burst still finishes independently");
        var board=(RectTransform)product.parent;
        var inside=board.rect;inside.xMin+=16;inside.xMax-=16;inside.yMin+=16;inside.yMax-=16;
        double waveStart=Time.realtimeSinceStartupAsDouble;int sampled=0;float largest=0;
        while(Time.realtimeSinceStartupAsDouble-waveStart<2.6)
        {
            foreach(var particle in ring.particles)
            {
                largest=Mathf.Max(largest,particle.Size.x);
                foreach(var point in particle.points)
                {
                    var local=board.InverseTransformPoint(ring.transform.TransformPoint(point));
                    if(!inside.Contains(new Vector2(local.x,local.y)))throw new Exception("Wave escaped the advertisement board at "+local);
                    sampled++;
                }
            }
            await Frames(1);
        }
        report["waveVerticesChecked"]=sampled;report["largestWaveDiameter"]=largest;
        Check(sampled>100 && largest<=480.01f,"A complete repeating wave stays within the advertisement board with 16px inner clearance");
        Check(glow.isPlaying && glow.particleCount>0 && ring.isPlaying,"Aura keeps emitting across multiple loop boundaries while paused");
        effect.StopEffects();await Frames();baseline=await Capture("product_without_effects",effect);
        var offsets=new[]{Vector2.zero,new Vector2(-45,-45),new Vector2(45,-45),new Vector2(0,70)};
        foreach(var offset in offsets)
        {
            var screen=RectTransformUtility.WorldToScreenPoint(null,product.TransformPoint(product.rect.center+offset));
            var a=opening.GetPixel(Mathf.RoundToInt(screen.x),Mathf.RoundToInt(screen.y));var b=baseline.GetPixel(Mathf.RoundToInt(screen.x),Mathf.RoundToInt(screen.y));
            Check(Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b)<.025f,"Opaque product pixels stay unchanged above aura and confetti at "+offset);
        }
        effect.ResumeAmbient();await Frames(3);
        Check(effect.AreEffectsVisible && glow.isPlaying && !confetti.activeSelf && !manager.IsPresentationLocked,"Restoring the advertisement resumes only ambient aura without replaying confetti");
        await effect.GetComponent<Exhibition.UI.UIScenePresentation>().PlayEntranceAsync(token);
        Check(!effect.IsPlaying && !confetti.activeSelf,"Repeated entrance notification does not restart confetti or input lock");
        UnityEngine.Object.Destroy(await Capture("looping_aura",effect));
        Time.timeScale=1;await Tap();
        float black=0;
        await Wait(()=>{black=Mathf.Max(black,manager.Presentation.BlackoutAlpha);return controller.State==Exhibition.UI.ResultVideoController.ScreenState.Playing;});
        Check(black>=.99f && !root.activeSelf && !confetti.activeSelf,"Video transition covers the screen and clears all advertisement effects");
        var first=typeof(Exhibition.UI.ResultVideoController).GetField("_firstFrame",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
        Check((long)first.GetValue(controller)==0,"Video still starts from decoded frame zero");
        manager.ReturnToIdleManually();await Ready();Check(manager.IsTitleScreen,"Home after video remains functional");
        await EnterResult();effect=null;await Wait(()=>(effect=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.AdProductHighlight>()) && effect.IsPlaying);
        so=new UnityEditor.SerializedObject(effect);root=(GameObject)so.FindProperty("effectRoot").objectReferenceValue;confetti=(GameObject)so.FindProperty("confettiRoot").objectReferenceValue;
        effect.enabled=false;await Ready();
        Check(!root.activeSelf && !confetti.activeSelf && !manager.IsPresentationLocked,"Disabling during confetti clears all effects and releases input");
        effect.enabled=true;
        using(var cancel=System.Threading.CancellationTokenSource.CreateLinkedTokenSource(token))
        {
            var operation=effect.PlayEntranceAsync(manager,cancel.Token).SuppressCancellationThrow();await Frames(4);cancel.Cancel();await operation;
            Check(!root.activeSelf && !confetti.activeSelf && !manager.IsPresentationLocked,"Cancellation cleans up both finite and looping effects");
        }
        effect.ResumeAmbient();manager.ReturnToIdleManually();await Ready();
        Check(manager.IsTitleScreen && !manager.IsPresentationLocked && manager.Presentation.BlackoutAlpha==0,"Page exit with ambient aura leaves no particles, blackout, or input lease");
        report["status"]="PASS";
    }
    catch(Exception ex){report["status"]="FAIL";report["error"]=ex.ToString();Debug.LogException(ex);}
    finally
    {
        Time.timeScale=1;Application.runInBackground=oldBackground;if(keyboard)keyboard.enabled=oldKeyboard;
        if(opening)UnityEngine.Object.Destroy(opening);if(baseline)UnityEngine.Object.Destroy(baseline);Save();
    }
});
return "AD-FX-003 validation started";
