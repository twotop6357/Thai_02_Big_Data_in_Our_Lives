// Execute via Unity MCP from Idle in Play. A real ComfyUI image must be centered after Download.
if (GameObject.Find("ComfyFramingVerification")) return "Framing verification is already running.";
var root = new GameObject("ComfyFramingVerification"); UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async () =>
{
    var checks = new Newtonsoft.Json.Linq.JArray();
    var report = new Newtonsoft.Json.Linq.JObject { ["task"]="COMFY-FRAMING-001", ["checks"]=checks };
    using var limit = new System.Threading.CancellationTokenSource(System.TimeSpan.FromSeconds(220));
    var token = limit.Token;
    var manager = UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();
    var keyboard = UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
    bool oldKeyboard = keyboard && keyboard.enabled, oldBackground = Application.runInBackground;
    if (keyboard) keyboard.enabled=false; Application.runInBackground=true;
    void Save()=>System.IO.File.WriteAllText("docs/validation/comfy_framing/runtime.json",report.ToString());
    void Check(bool value,string label){if(!value)throw new Exception(label);checks.Add(label);Save();}
    async Cysharp.Threading.Tasks.UniTask Frame()=>await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);
    async Cysharp.Threading.Tasks.UniTask Wait(Func<bool> p)=>await Cysharp.Threading.Tasks.UniTask.WaitUntil(p,cancellationToken:token);
    async Cysharp.Threading.Tasks.UniTask Ready()=>await Wait(()=>!manager.IsTransitioning && !manager.IsPresentationLocked);
    async Cysharp.Threading.Tasks.UniTask Tap(){manager.SetButtonPressed(false);await Frame();manager.SetButtonPressed(true);await Frame();manager.SetButtonPressed(false);await Frame();}
    try
    {
        await Wait(()=>manager && manager.IsInputReady);await Ready();await Tap();await Ready();
        var blocks=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Wait(()=>blocks && blocks.IsReady);
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group,"student");await Ready();await Tap();await Ready();
        var view=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.BlockExperienceView>();
        Check(view.IsChoosingProduct,"Product panel accepts the existing group-only progression");
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product,"shoes");
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1,"red");
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2,"blue");
        double started=Time.realtimeSinceStartupAsDouble;
        Check(blocks.IsGenerating && manager.IsPresentationLocked,"Input remains locked while generating and centering");
        await Wait(()=>!blocks.IsGenerating);await Ready();
        Check(manager.ResultTexture && blocks.HasGeneratedImage,"The real generated product is accepted by Unity");
        report["generationSeconds"]=Time.realtimeSinceStartupAsDouble-started;
        var source=manager.ResultTexture;
        var previous=RenderTexture.active;
        var target=RenderTexture.GetTemporary(480,480,0,RenderTextureFormat.ARGB32);
        var image=new Texture2D(480,480,TextureFormat.RGBA32,false);
        try
        {
            Graphics.Blit(source,target);RenderTexture.active=target;image.ReadPixels(new Rect(0,0,480,480),0,0);image.Apply();
            var pixels=image.GetPixels32(); int left=480,right=-1,bottom=480,top=-1;
            for(int y=0;y<480;y++)for(int x=0;x<480;x++)if(pixels[y*480+x].a>=Exhibition.Blocks.GeneratedImageFraming.VisibleAlpha)
            {left=Math.Min(left,x);right=Math.Max(right,x);bottom=Math.Min(bottom,y);top=Math.Max(top,y);}
            double cx=(left+right+1)*.5,cy=(bottom+top+1)*.5;
            report["bounds"]=new Newtonsoft.Json.Linq.JArray(left,bottom,right+1,top+1);
            report["center"]=new Newtonsoft.Json.Linq.JArray(cx,cy);
            Check(image.width==480 && image.height==480 && Math.Abs(cx-240)<=1 && Math.Abs(cy-240)<=1,"Visible product bounds are centered at (240,240) within one raster pixel");
            Check(left>=24 && bottom>=24 && right<456 && top<456,"Product fits within the transparent 24-pixel safety margin");
            Check(pixels[0].a==0 && pixels[479].a==0 && pixels[479*480].a==0 && pixels[480*480-1].a==0,"All four canvas corners remain transparent");
            System.IO.File.WriteAllBytes("docs/validation/comfy_framing/generated_product.png",image.EncodeToPNG());
        }
        finally{RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);UnityEngine.Object.Destroy(image);}
        await Cysharp.Threading.Tasks.UniTask.WaitForEndOfFrame(blocks,token);
        var screen=ScreenCapture.CaptureScreenshotAsTexture();
        System.IO.File.WriteAllBytes("docs/validation/comfy_framing/selection.png",screen.EncodeToPNG());UnityEngine.Object.Destroy(screen);
        await Tap();await Ready();
        Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="2_Result" && manager.ResultTexture==source,"Result keeps the exact centered texture");
        manager.ReturnToIdleManually();await Ready();
        Check(manager.IsTitleScreen && !manager.ResultTexture && !UnityEngine.Object.FindFirstObjectByType<HuliacDev.Core.InactivityTimer>().IsPaused,"Home releases the image and restores inactivity");
        report["passed"]=true;
    }
    catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
    finally
    {
        if(manager && !manager.IsTitleScreen)manager.ReturnToIdleManually();
        if(keyboard)keyboard.enabled=oldKeyboard;Application.runInBackground=oldBackground;
        Save();UnityEngine.Object.Destroy(root);
    }
});
return "Framing runtime verification started.";
