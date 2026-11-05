// Normal button -> real Comfy generation -> advertisement preview, without test probes.
if(GameObject.Find("AdFx5Preview"))return "Already running.";
var root=new GameObject("AdFx5Preview");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var adapter=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();bool keyboard=adapter&&adapter.enabled,background=Application.runInBackground;
 using var limit=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(100));var token=limit.Token;var report=new Newtonsoft.Json.Linq.JObject{["task"]="AD-FX-005",["real_generation"]=true};
 async Cysharp.Threading.Tasks.UniTask Frames(int n=4){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Wait(Func<bool> p)=>await Cysharp.Threading.Tasks.UniTask.WaitUntil(p,cancellationToken:token);
 async Cysharp.Threading.Tasks.UniTask Ready(){await Wait(()=>manager.IsInputReady&&!manager.IsPresentationLocked&&!manager.IsTransitioning);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Tap(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Frames();}
 try{
  Application.runInBackground=true;if(adapter)adapter.enabled=false;await Ready();await Tap();await Ready();var b=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Wait(()=>b&&b.IsReady);
  b.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group,"student");await Frames();b.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product,"bags");b.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1,"red");b.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2,"blue");await Ready();await Tap();await Wait(()=>!b.IsGenerating&&b.HasGeneratedImage);await Ready();
  report["primaryHex"]=ColorUtility.ToHtmlStringRGB(manager.ResultPrimaryColor);report["secondaryHex"]=ColorUtility.ToHtmlStringRGB(manager.ResultSecondaryColor);await Tap();
  Exhibition.UI.AdProductHighlight effect=null;await Wait(()=>(effect=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.AdProductHighlight>())&&effect.IsPlaying);await Cysharp.Threading.Tasks.UniTask.Delay(1000,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);ScreenCapture.CaptureScreenshot("docs/validation/ad_fx5/final_entrance.png");await Frames(8);
  await Wait(()=>!effect.IsPlaying);await Ready();await Cysharp.Threading.Tasks.UniTask.Delay(700,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);ScreenCapture.CaptureScreenshot("docs/validation/ad_fx5/final_ambient.png");await Frames(8);manager.ReturnToIdleManually();await Ready();report["passed"]=manager.IsTitleScreen;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{Application.runInBackground=background;if(adapter)adapter.enabled=keyboard;if(manager)manager.ResetButtonInput();System.IO.File.WriteAllText("docs/validation/ad_fx5/preview.json",report.ToString());UnityEngine.Object.Destroy(root);}
});
return "Real red/blue advertisement preview started.";
