if (GameObject.Find("UIWeb4Verification")) return "Wait for the bilingual check to finish.";
if (UnityEditor.SessionState.GetBool("UIWeb4LifecycleRunning", false)) return "Already running.";
UnityEditor.SessionState.SetBool("UIWeb4LifecycleRunning", true);
Cysharp.Threading.Tasks.UniTask.Void(async () => {
 var checks = new Newtonsoft.Json.Linq.JArray();
 var report = new Newtonsoft.Json.Linq.JObject { ["checks"] = checks };
 var manager = UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();
 var hub = UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidReaderHub>();
 var keyboard = UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
 bool enabled = keyboard && keyboard.enabled;
 var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
 using var deadline = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(90));
 var token = deadline.Token;
 void Check(bool value, string name) { if (!value) throw new Exception(name); checks.Add(name); }
 async Cysharp.Threading.Tasks.UniTask Wait(Func<bool> p) => await Cysharp.Threading.Tasks.UniTask.WaitUntil(p, cancellationToken: token);
 async Cysharp.Threading.Tasks.UniTask Frames(int n=4) { for(int i=0;i<n;i++) await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token); }
 async Cysharp.Threading.Tasks.UniTask Ready() { await Wait(()=>manager.IsInputReady&&!manager.IsPresentationLocked&&!manager.IsTransitioning&&!manager.IsLanguageChanging&&hub.IsArtworkReady); await Frames(); }
 async Cysharp.Threading.Tasks.UniTask Tap() { manager.SetButtonPressed(false); await Frames(); manager.SetButtonPressed(true); await Frames(); manager.SetButtonPressed(false); await Frames(); }
 async Cysharp.Threading.Tasks.UniTask<Exhibition.UI.ResultVideoController> Result() {
  await Ready(); await Tap(); await Wait(()=>UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="1_Game");
  var controller=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>(); await Wait(()=>controller&&controller.IsReady); await Ready();
  hub.RecognizeReader(3,Exhibition.Blocks.BlockKind.Group,"student"); await Ready(); await Tap(); await Ready();
  hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Product,"shoes");hub.RecognizeReader(1,Exhibition.Blocks.BlockKind.Color,"red");hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Color,"blue");await Ready();
  var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);ImageConversion.LoadImage(texture,System.IO.File.ReadAllBytes("docs/validation/comfy_style3/runtime_0_shoes.png"));
  if(!controller.TryDisplayResult(texture,controller.Selection.Revision)){UnityEngine.Object.Destroy(texture);throw new Exception("Fixture rejected.");}
  await Ready();await Tap();await Wait(()=>UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="2_Result");await Ready();
  return UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.ResultVideoController>();
 }
 try {
  Application.runInBackground=true;if(keyboard)keyboard.enabled=false;
  var video=await Result();var hint=(GameObject)video.GetType().GetField("playbackHint",flags).GetValue(video);
  Check(!hint.activeSelf,"Reentered advertisement starts with the video hint hidden");
  await Tap();await Wait(()=>video.State==Exhibition.UI.ResultVideoController.ScreenState.Playing);await Ready();
  Check(hint.activeInHierarchy,"Hint visible during actual playback");
  typeof(Exhibition.UI.ResultVideoController).GetField("_videoError",flags).SetValue(video,"UI-WEB-004 injected decoder failure");
  await Wait(()=>video.State==Exhibition.UI.ResultVideoController.ScreenState.Failed&&video.GetType().GetField("_playbackCts",flags).GetValue(video)==null);await Ready();
  Check(!hint.activeSelf,"Injected decoder failure hides the hint");
  Check(video.GetType().GetField("_target",flags).GetValue(video)==null,"Failure releases the media target");
  await Tap();await Wait(()=>video.State==Exhibition.UI.ResultVideoController.ScreenState.Playing);await Ready();
  Check(hint.activeInHierarchy,"Retry displays the same hint without duplication");
  manager.SetButtonPressed(true);await Cysharp.Threading.Tasks.UniTask.Delay(5200,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);manager.SetButtonPressed(false);await Ready();
  Check(manager.IsTitleScreen&&!hint,"Five-second home during playback cancels media and destroys the scene hint");
  video=await Result();hint=(GameObject)video.GetType().GetField("playbackHint",flags).GetValue(video);
  Check(!hint.activeSelf,"Repeated Result entry resets hint visibility");await Tap();await Wait(()=>video.State==Exhibition.UI.ResultVideoController.ScreenState.Preparing);
  Check(!hint.activeSelf,"Preparing video keeps hint hidden until an actual video frame is displayed");
  manager.ReturnToIdleManually();await Ready();Check(manager.IsTitleScreen&&!hint,"Cancellation during preparation leaves no hint behind");
  Check(System.Linq.Enumerable.All(Resources.FindObjectsOfTypeAll<RenderTexture>(),t=>t.name!="ExhibitionVideo"),"Cancellation releases video render textures");
  report["passed"]=true;
 } catch(Exception ex) { report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex); }
 finally { manager.ResetButtonInput();if(keyboard)keyboard.enabled=enabled;UnityEditor.SessionState.SetBool("UIWeb4LifecycleRunning",false);System.IO.File.WriteAllText("docs/validation/ui_web4/video_lifecycle.json",report.ToString()); }
});
return "Video hint failure, retry, repeated entry and cancellation verification started.";
