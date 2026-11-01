// Run through execute_code from Idle in Play. Synthetic fixtures test rejection/lifetimes, not illustration quality.
if (GameObject.Find("StyleRuntimeVerification")) return "Verification is already running.";
var root = new GameObject("StyleRuntimeVerification"); UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async () => {
 var checks = new Newtonsoft.Json.Linq.JArray();
 var report = new Newtonsoft.Json.Linq.JObject { ["task"]="STYLE-QUALITY-003 fixture lifecycle", ["checks"]=checks };
 using var deadline=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(80)); var token=deadline.Token;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();
 var timer=UnityEngine.Object.FindFirstObjectByType<HuliacDev.Core.InactivityTimer>();
 var keyboard=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
 bool oldKeyboard=keyboard&&keyboard.enabled, oldBackground=Application.runInBackground;
 if(keyboard)keyboard.enabled=false;Application.runInBackground=true;
 Exhibition.Blocks.BlockExperienceController blocks=null;
 void Save()=>System.IO.File.WriteAllText("docs/validation/comfy_style3/runtime_fixtures.json",report.ToString());
 void Check(bool value,string label){if(!value)throw new Exception(label);checks.Add(label);Save();}
 async Cysharp.Threading.Tasks.UniTask Wait(Func<bool> p)=>await Cysharp.Threading.Tasks.UniTask.WaitUntil(p,cancellationToken:token);
 async Cysharp.Threading.Tasks.UniTask Frames(int n=2){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Ready()=>await Wait(()=>!manager.IsTransitioning&&!manager.IsPresentationLocked);
 async Cysharp.Threading.Tasks.UniTask Tap(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Frames();}
 var fixture=new Exhibition.Blocks.ComfyImageClient(new Exhibition.Blocks.ComfyBlockSettings {serverUrl="http://127.0.0.1:18190"});
 async Cysharp.Threading.Tasks.UniTask Mode(string mode)=>await fixture.Json("/fixture_mode",new Newtonsoft.Json.Linq.JObject { ["mode"]=mode },token);
 try {
  await Wait(()=>manager&&manager.IsInputReady);await Ready();await Tap();await Ready();
  blocks=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Wait(()=>blocks&&blocks.IsReady);
  blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group,"child");await Ready();await Tap();await Ready();
  var settings=(Exhibition.Blocks.ComfyBlockSettings)blocks.GetType().GetField("_settings",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(blocks);
  settings.serverUrl="http://127.0.0.1:18190";
  await Mode("clipped");
  blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product,"hats");blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1,"yellow");blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2,"green");
  Check(blocks.IsGenerating&&manager.IsPresentationLocked&&timer.IsPaused,"Generation locks input and pauses inactivity");
  var view=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.BlockExperienceView>();view.SendMessage("Continue",SendMessageOptions.RequireReceiver);await Tap();
  Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="1_Game","Direct Continue and taps cannot navigate during generation");
  await Wait(()=>!blocks.IsGenerating);await Ready();
  Check(!blocks.HasGeneratedImage&&!manager.ResultTexture&&blocks.Status.Contains("failed"),"Clipped source is rejected without displaying a centered broken product");
  Check(!timer.IsPaused&&!manager.IsPresentationLocked,"Quality rejection releases input and resumes inactivity");
  report["rejectionStatus"]=blocks.Status;
  await Mode("valid");await Tap();await Wait(()=>!blocks.IsGenerating);await Ready();
  Check(blocks.HasGeneratedImage&&manager.ResultTexture&&manager.ResultTexture.width==480,"Fresh tap retries successfully after quality rejection");
  await Mode("delay");blocks.GenerateImage();await Frames();
  Check(blocks.IsGenerating&&manager.IsPresentationLocked&&!manager.ResultTexture,"A new generation clears the previous image and keeps input locked");
  blocks.CancelGeneration();await Ready();
  Check(!blocks.IsGenerating&&!timer.IsPaused&&!manager.IsPresentationLocked,"Explicit cancellation restores input and inactivity");
  settings.generationTimeoutSeconds=1;blocks.GenerateImage();await Wait(()=>!blocks.IsGenerating);await Ready();
  Check(!manager.ResultTexture&&!timer.IsPaused&&blocks.Status.StartsWith("Generation cancelled or timed out"),"Deadline cancels stalled generation without a late image");
  settings.generationTimeoutSeconds=10;
  await Mode("error");await Tap();await Wait(()=>!blocks.IsGenerating);await Ready();
  Check(blocks.Status.Contains("failed")&&!manager.IsPresentationLocked&&!timer.IsPaused&&!manager.ResultTexture,"Submission failure leaves a retryable panel with no stale image");
  await Mode("delay");blocks.GenerateImage();await Frames();manager.ReturnToIdleManually();await Ready();
  Check(manager.IsTitleScreen&&!timer.IsPaused&&!manager.IsPresentationLocked,"Home cancels the pending session and restores all lifetimes");
  report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally {if(blocks)blocks.CancelGeneration();if(manager&&!manager.IsTitleScreen)manager.ReturnToIdleManually();if(keyboard)keyboard.enabled=oldKeyboard;Application.runInBackground=oldBackground;Save();UnityEngine.Object.Destroy(root);}
});
return "Style quality/lifecycle fixture verification started.";
