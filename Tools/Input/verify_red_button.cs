// Run through Unity execute_code in Play mode from 0_Idle. Legacy hardware is not emulated.
if(GameObject.Find("RedButtonVerification"))return "Already running.";
var root=new GameObject("RedButtonVerification");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="RED-BUTTON-001",["hardware_tested"]=false};
 var checks=new Newtonsoft.Json.Linq.JArray();report["checks"]=checks;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();
 var adapter=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
 var oldBehavior=UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior;bool oldBackground=Application.runInBackground;
 UnityEngine.InputSystem.Keyboard keyboard=null;int taps=0;Action onTap=()=>taps++;
 using var limit=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(50));var token=limit.Token;
 void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
 async Cysharp.Threading.Tasks.UniTask Frames(int count=3){for(int i=0;i<count;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Ready(){await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager&&!manager.IsTransitioning&&!manager.IsPresentationLocked&&manager.IsInputReady,cancellationToken:token);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Keys(params UnityEngine.InputSystem.Key[] keys){UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(keys));await Frames();}
 try{
  Application.runInBackground=true;UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
  keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>("RedButton verification keyboard");
  await Ready();Check(manager.IsTitleScreen&&adapter&&adapter.enabled,"Idle adapter is active");
  adapter.SendMessage("OnApplicationFocus",true);await Keys();
  Check(!UnityEngine.Input.GetButton("RedButton"),"Configured legacy RedButton can be read at rest without an exception");
  await Keys(UnityEngine.InputSystem.Key.Digit1);await Keys();await Ready();
  Check(!manager.IsTitleScreen,"Top-row 1 starts Game through the actual adapter");manager.ShortButtonPressed+=onTap;
  await Keys(UnityEngine.InputSystem.Key.Digit1,UnityEngine.InputSystem.Key.Numpad1);await Keys(UnityEngine.InputSystem.Key.Numpad1);
  Check(taps==0,"Releasing one overlapping input does not release the combined button");
  await Keys();Check(taps==1,"Releasing all inputs produces exactly one short press");
  await Keys(UnityEngine.InputSystem.Key.Digit1);adapter.SendMessage("OnApplicationFocus",false);await Keys();adapter.SendMessage("OnApplicationFocus",true);await Keys();
  Check(taps==1,"Focus loss cancels a pending press without advancing");
  await Keys(UnityEngine.InputSystem.Key.Numpad1);await Cysharp.Threading.Tasks.UniTask.Delay(5200,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);await Keys();await Ready();
  Check(manager.IsTitleScreen,"Numpad 1 five-second hold returns home");
  var language=manager.CurrentLanguage;await Keys(UnityEngine.InputSystem.Key.Digit1);await Cysharp.Threading.Tasks.UniTask.Delay(2300,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);await Keys();await Ready();
  Check(manager.IsTitleScreen&&manager.CurrentLanguage!=language,"Top-row 1 two-second hold changes title language without starting");
  await Keys(UnityEngine.InputSystem.Key.Digit1);await Cysharp.Threading.Tasks.UniTask.Delay(2300,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);await Keys();await Ready();
  Check(manager.CurrentLanguage==language,"Repeated language hold restores original language");
  await Keys(UnityEngine.InputSystem.Key.Numpad1);await Keys();await Ready();Check(!manager.IsTitleScreen,"Numpad 1 also starts Game through the adapter");
  manager.ShortButtonPressed-=onTap;manager.ReturnToIdleManually();await Ready();Check(manager.IsTitleScreen,"Verification returns to Idle");report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{
  if(manager)manager.ShortButtonPressed-=onTap;
  if(keyboard!=null)UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);
  if(adapter){adapter.SendMessage("OnApplicationFocus",true);if(manager)manager.ResetButtonInput();}
  UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=oldBehavior;Application.runInBackground=oldBackground;
  System.IO.Directory.CreateDirectory("docs/validation/red_button");System.IO.File.WriteAllText("docs/validation/red_button/runtime.json",report.ToString());UnityEngine.Object.Destroy(root);
 }
});
return "RedButton keyboard and lifecycle verification started.";
