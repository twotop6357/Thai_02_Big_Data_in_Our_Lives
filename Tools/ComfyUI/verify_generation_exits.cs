// Start slow_test_server.py first. Run from Idle in Play; uses only the local delayed fixture for generation.
if (GameObject.Find("ComfyExitsVerification")) return "Generation-exit verification is already running.";
var verificationRoot = new GameObject("ComfyExitsVerification");
UnityEngine.Object.DontDestroyOnLoad(verificationRoot);
Cysharp.Threading.Tasks.UniTask.Void(async () =>
{
    var checks = new Newtonsoft.Json.Linq.JArray();
    var report = new Newtonsoft.Json.Linq.JObject { ["task"]="COMFY-RESTORE-001 exits", ["checks"]=checks };
    using var limit = new System.Threading.CancellationTokenSource(System.TimeSpan.FromSeconds(50));
    var token = limit.Token;
    var manager = UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();
    var keyboard = UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
    var timer = UnityEngine.Object.FindFirstObjectByType<HuliacDev.Core.InactivityTimer>();
    bool oldKeyboard = keyboard && keyboard.enabled, oldBackground = Application.runInBackground;
    if (keyboard) keyboard.enabled=false;
    Application.runInBackground=true;
    Exhibition.Blocks.BlockExperienceController blocks=null;
    void Save()=>System.IO.File.WriteAllText("docs/validation/comfy_restore/exits.json",report.ToString());
    void Check(bool v,string label){if(!v)throw new Exception(label);checks.Add(label);Save();}
    async Cysharp.Threading.Tasks.UniTask Wait(Func<bool> p)=>await Cysharp.Threading.Tasks.UniTask.WaitUntil(p,cancellationToken:token);
    async Cysharp.Threading.Tasks.UniTask Ready()=>await Wait(()=>!manager.IsTransitioning && !manager.IsPresentationLocked);
    async Cysharp.Threading.Tasks.UniTask Frame()=>await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);
    async Cysharp.Threading.Tasks.UniTask Delay(int ms)=>await Cysharp.Threading.Tasks.UniTask.Delay(ms,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);
    async Cysharp.Threading.Tasks.UniTask Tap(){manager.SetButtonPressed(false);await Frame();manager.SetButtonPressed(true);await Frame();manager.SetButtonPressed(false);await Frame();}
    try
    {
        await Ready();await Tap();await Ready();
        blocks=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Wait(()=>blocks && blocks.IsReady);
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group,"child");await Ready();await Tap();await Ready();
        var field=blocks.GetType().GetField("_settings",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
        var settings=(Exhibition.Blocks.ComfyBlockSettings)field.GetValue(blocks);
        settings.serverUrl="http://127.0.0.1:18189"; settings.generationTimeoutSeconds=1;
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product,"hats");
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1,"yellow");
        blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2,"green");
        Check(blocks.IsGenerating && manager.IsPresentationLocked && timer.IsPaused,"Request starts with input locked and inactivity paused");
        double started=Time.realtimeSinceStartupAsDouble;
        while(blocks.IsGenerating){manager.SetButtonPressed(true);await Frame();}
        await Ready();
        Check(Time.realtimeSinceStartupAsDouble-started < 2.5 && blocks.Status.StartsWith("Generation cancelled or timed out"),"Generation deadline cancels a stalled queue read");
        Check(!manager.IsPresentationLocked && !timer.IsPaused && !manager.ResultTexture,"Timeout releases input, resumes timer and leaves no image");
        for(int i=0;i<12;i++){manager.SetButtonPressed(true);await Frame();}
        manager.SetButtonPressed(false);await Frame();await Frame();
        Check(!blocks.IsGenerating,"A hold spanning timeout and its release cannot trigger a retry");
        settings.generationTimeoutSeconds=10;
        await Tap();Check(blocks.IsGenerating,"A fresh tap retries after timeout");
        var first=blocks.GetType().GetField("_requestCts",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(blocks);
        blocks.CancelGeneration();blocks.GenerateImage();
        var replacement=blocks.GetType().GetField("_requestCts",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(blocks);
        await Delay(150);
        Check(!ReferenceEquals(first,replacement) && blocks.IsGenerating && manager.IsPresentationLocked && timer.IsPaused,
            "Cancelled request cleanup cannot unlock or resume a replacement request");
        blocks.CancelGeneration();await Ready();Check(!timer.IsPaused,"Explicit cancellation resumes inactivity");
        // Exercise the real ComfyUI empty-body queue acknowledgement without deleting any known prompt.
        var realSettings=new Exhibition.Blocks.ComfyBlockSettings();
        var ack=await new Exhibition.Blocks.ComfyImageClient(realSettings).Json("/queue",new Newtonsoft.Json.Linq.JObject{
            ["delete"]=new Newtonsoft.Json.Linq.JArray(Guid.NewGuid().ToString())},token);
        Check(ack!=null,"Successful empty queue-delete response is accepted without a JSON warning");
        blocks.GenerateImage();manager.ReturnToIdleManually();await Ready();
        Check(manager.IsTitleScreen && !timer.IsPaused && !manager.IsPresentationLocked,"Home during a stalled request releases all lifetimes");
        report["passed"]=true;
    }
    catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
    finally
    {
        if(blocks)blocks.CancelGeneration();
        if(manager && !manager.IsTitleScreen)manager.ReturnToIdleManually();
        if(keyboard)keyboard.enabled=oldKeyboard;
        Application.runInBackground=oldBackground;Save();UnityEngine.Object.Destroy(verificationRoot);
    }
});
return "Generation-exit verification started.";
