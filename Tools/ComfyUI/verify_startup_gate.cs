// Execute in the actual Idle Play session. State changes below are explicit UI/input test injection.
if (GameObject.Find("ComfyStartupGateVerification")) return "Already running.";
var root = new GameObject("ComfyStartupGateVerification"); UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async () => {
 var report = new Newtonsoft.Json.Linq.JObject { ["task"] = "COMFY-AUTOSTART-002", ["state_injection"] = true, ["physical_button_tested"] = false };
 var checks = new Newtonsoft.Json.Linq.JArray(); report["checks"] = checks;
 var manager = UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();
 var server = manager.GetComponent<Exhibition.Blocks.ComfyServerLauncher>();
 var adapter = UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
 var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
 var setState = server.GetType().GetMethod("SetState", flags);
 var originalLanguage = manager.CurrentLanguage;
 var originalBackground = Application.runInBackground;
 var originalBehavior = UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior;
 UnityEngine.InputSystem.Keyboard keyboard = null;
 using var limit = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(90)); var token = limit.Token;
 void Check(bool ok, string label) { if (!ok) throw new Exception(label); checks.Add(label); }
 void State(Exhibition.Blocks.ComfyServerState state) { setState.Invoke(server, new object[] { state, null }); }
 async Cysharp.Threading.Tasks.UniTask Frames(int n = 4) { for (int i=0;i<n;i++) await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update, token); }
 async Cysharp.Threading.Tasks.UniTask Keys(params UnityEngine.InputSystem.Key[] keys) { UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard, new UnityEngine.InputSystem.LowLevel.KeyboardState(keys)); await Frames(); }
 async Cysharp.Threading.Tasks.UniTask Ready() { await Cysharp.Threading.Tasks.UniTask.WaitUntil(() => manager.IsInputReady && !manager.IsTransitioning && !manager.IsPresentationLocked && !manager.IsLanguageChanging, cancellationToken: token); await Frames(); }
 UnityEngine.UI.Image Help() { var localized=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.LocalizedImageSet>(); var bindings=(Exhibition.UI.LocalizedImageSet.Binding[])localized.GetType().GetField("bindings",flags).GetValue(localized); return System.Array.Find(bindings,b=>b.id=="idle.help").image; }
 async Cysharp.Threading.Tasks.UniTask Shot(string name) { ScreenCapture.CaptureScreenshot("docs/validation/comfy_autostart/"+name+".png"); await Frames(8); }
 try {
  Application.runInBackground=true; UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
  keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>("Comfy startup verification keyboard");
  adapter.SendMessage("OnApplicationFocus",true); await Keys(); await Ready();
  Check(manager.IsTitleScreen && Help().enabled, "Ready server enables title hint and input");
  foreach(var state in new[]{Exhibition.Blocks.ComfyServerState.Checking,Exhibition.Blocks.ComfyServerState.Starting,Exhibition.Blocks.ComfyServerState.Unavailable}) {
   State(state); await Frames(); Check(!manager.IsInputReady&&!Help().enabled,"Input and title hint blocked: "+state);
   await Keys(UnityEngine.InputSystem.Key.Digit1); await Keys(); await Frames();
   Check(manager.IsTitleScreen,"Top-row 1 ignored: "+state);
   await Keys(UnityEngine.InputSystem.Key.Numpad1); await Cysharp.Threading.Tasks.UniTask.Delay(2200,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token); await Keys();
   Check(manager.IsTitleScreen&&manager.CurrentLanguage==originalLanguage,"Numpad hold cannot change language or start: "+state);
   manager.SetButtonPressed(true); await Frames(); manager.SetButtonPressed(false); await Frames();
   Check(manager.IsTitleScreen,"Direct hardware adapter API ignored: "+state);
  }
  await Shot("title_waiting");
  await Keys(UnityEngine.InputSystem.Key.Digit1); State(Exhibition.Blocks.ComfyServerState.Ready); await Ready();
  await Cysharp.Threading.Tasks.UniTask.Delay(2400,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);
  Check(Help().enabled&&manager.IsTitleScreen&&manager.CurrentLanguage==originalLanguage,"Button held across readiness is discarded");
  await Keys(); await Frames(); Check(manager.IsTitleScreen,"Release of pre-ready press does not start"); await Shot("title_ready");
  server.enabled=false; await Frames(); Check(!manager.IsInputReady&&!Help().enabled,"Disabled launcher fails closed");
  server.enabled=true; await Ready(); Check(Help().enabled,"Re-enabled launcher reuses server and restores hint");
  await Keys(UnityEngine.InputSystem.Key.Digit1); await Cysharp.Threading.Tasks.UniTask.Delay(2300,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token); await Keys(); await Ready();
  Check(manager.CurrentLanguage!=originalLanguage&&Help().enabled,"Language change works after readiness");
  await Keys(UnityEngine.InputSystem.Key.Digit1); await Cysharp.Threading.Tasks.UniTask.Delay(2300,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token); await Keys(); await Ready();
  Check(manager.CurrentLanguage==originalLanguage,"Original language restored");
  await Keys(UnityEngine.InputSystem.Key.Numpad1); await Keys(); await Ready(); Check(!manager.IsTitleScreen,"Fresh press starts Game after readiness");
  manager.ReturnToIdleManually(); await Ready(); Check(manager.IsTitleScreen&&Help().enabled,"Home reload restores ready title hint"); report["passed"]=true;
 } catch(Exception ex) { report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex); }
 finally {
  if(server) { State(Exhibition.Blocks.ComfyServerState.Ready);server.enabled=true; }
  if(keyboard!=null) UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);
  if(manager) manager.ResetButtonInput();
  if(adapter) adapter.SendMessage("OnApplicationFocus",true);
  Application.runInBackground=originalBackground;UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=originalBehavior;
  System.IO.File.WriteAllText("docs/validation/comfy_autostart/gate.json",report.ToString());UnityEngine.Object.Destroy(root);
 }
});
return "Startup input and title hint verification started.";
