// Cancellation/disable/video regression; uses a prior PNG solely as a result bridge fixture.
if(GameObject.Find("AdFx4Verification")||GameObject.Find("AdFx4Lifecycle"))return "Wait for the active advertisement verification.";
var root=new GameObject("AdFx4Lifecycle");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="AD-FX-004-lifecycle",["prior_png_fixture"]=true};var checks=new Newtonsoft.Json.Linq.JArray();report["checks"]=checks;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var adapter=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();bool oldAdapter=adapter&&adapter.enabled,background=Application.runInBackground;
 using var limit=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(80));var token=limit.Token;
 void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
 async Cysharp.Threading.Tasks.UniTask Frames(int n=4){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Wait(Func<bool> p)=>await Cysharp.Threading.Tasks.UniTask.WaitUntil(p,cancellationToken:token);
 async Cysharp.Threading.Tasks.UniTask Ready(){await Wait(()=>manager.IsInputReady&&!manager.IsTransitioning&&!manager.IsPresentationLocked);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Tap(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Frames();}
 try{
  Application.runInBackground=true;if(adapter)adapter.enabled=false;await Ready();await Tap();await Ready();
  var b=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Wait(()=>b&&b.IsReady);b.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group,"student");await Frames();b.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product,"bags");b.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1,"red");b.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2,"blue");await Ready();
  var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);ImageConversion.LoadImage(texture,System.IO.File.ReadAllBytes("docs/validation/comfy_style3/runtime_2_bags.png"));Check(b.TryDisplayResult(texture,b.Selection.Revision),"Prepared result bridge carries palette");await Ready();await Tap();
  Exhibition.UI.AdProductHighlight effect=null;await Wait(()=>(effect=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.AdProductHighlight>())&&effect.IsPlaying);effect.enabled=false;await Ready();Check(!effect.AreEffectsVisible&&!manager.IsPresentationLocked,"Disable during entrance cancels particles and input lease");
  effect.enabled=true;using(var cancel=System.Threading.CancellationTokenSource.CreateLinkedTokenSource(token)){var work=effect.PlayEntranceAsync(manager,cancel.Token).SuppressCancellationThrow();await Frames(7);cancel.Cancel();await work;await Frames();Check(!effect.AreEffectsVisible&&!effect.IsPlaying&&!manager.IsPresentationLocked,"Explicit cancellation releases only the owning entrance");}
  effect.ResumeAmbient();await Frames();Check(effect.AreEffectsVisible&&!effect.IsPlaying&&!manager.IsPresentationLocked&&manager.HasResultPalette,"Advertisement restoration resumes only palette-bound ambient effects");await effect.PlayEntranceAsync(manager,token);Check(!effect.IsPlaying&&!manager.IsPresentationLocked,"Duplicate entrance does not replay confetti or lock input");
  var video=effect.GetComponent<Exhibition.UI.ResultVideoController>();await Tap();float black=0;await Wait(()=>{black=Mathf.Max(black,manager.Presentation.BlackoutAlpha);return video.State==Exhibition.UI.ResultVideoController.ScreenState.Playing;});
  Check(black>=.99f&&!effect.AreEffectsVisible,"Video transition covers the screen and clears ambient effects");var first=typeof(Exhibition.UI.ResultVideoController).GetField("_firstFrame",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);Check((long)first.GetValue(video)==0,"Video still displays decoded frame zero first");
  manager.ReturnToIdleManually();await Ready();Check(manager.IsTitleScreen&&!manager.HasResultPalette&&manager.Presentation.BlackoutAlpha==0,"Home clears result palette, video and blackout");report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{Application.runInBackground=background;if(adapter)adapter.enabled=oldAdapter;if(manager)manager.ResetButtonInput();System.IO.File.WriteAllText("docs/validation/ad_fx4/lifecycle.json",report.ToString());UnityEngine.Object.Destroy(root);}
});
return "Advertisement cancellation and video regression started.";
