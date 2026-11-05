// Actual manager/events use a local collector. The production endpoint is never called by this harness.
if(GameObject.Find("ContentLogVerification"))return "Already running.";
var root=new GameObject("ContentLogVerification");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="LOG-003",["receiver"]="loopback fixture"};var checks=new Newtonsoft.Json.Linq.JArray();report["checks"]=checks;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var api=UnityEngine.Object.FindFirstObjectByType<Network.APIManager>();var keyboard=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
 var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;var provider=(HuliacDev.Data.AppSettingsProvider)typeof(HuliacDev.Network.ApiManagerBase).GetProperty("SettingsProvider",flags).GetValue(api);
 var settings=await provider.GetAsync();string originalUrl=settings.apiUrl;bool oldBackground=Application.runInBackground,oldKeyboard=keyboard&&keyboard.enabled;
 using var limit=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(100));var token=limit.Token;
 void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
 async Cysharp.Threading.Tasks.UniTask Frames(int n=4){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Wait(Func<bool> p)=>await Cysharp.Threading.Tasks.UniTask.WaitUntil(p,cancellationToken:token);
 async Cysharp.Threading.Tasks.UniTask Ready(){await Wait(()=>manager.IsInputReady&&!manager.IsTransitioning&&!manager.IsPresentationLocked);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Tap(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Frames();}
 try{
  Application.runInBackground=true;if(keyboard)keyboard.enabled=false;settings.apiUrl="http://127.0.0.1:18196/log?message=";await Ready();
  int count=api.SentCount;api.GetType().GetMethod("SendStartupLogAsync",flags).Invoke(api,new object[]{token});await Wait(()=>api.SentCount==count+1);
  Check(api.LastSentMessage=="start","Startup is exactly start without restart/reason suffix");
  await Tap();await Ready();Check(!manager.IsTitleScreen,"Actual Game session starts for return tests");count=api.SentCount;manager.ReturnToIdleManually();await Ready();await Wait(()=>api.SentCount==count+1);
  Check(manager.IsTitleScreen&&api.LastSentMessage=="move_idle","Manual home sends one move_idle through existing event");
  await Tap();await Ready();var scope=UnityEngine.Object.FindFirstObjectByType<App.GameLifetimeScope>();var timeoutPublisher=(MessagePipe.IPublisher<HuliacDev.App.InactivityTimeoutEvent>)scope.Container.Resolve(typeof(MessagePipe.IPublisher<HuliacDev.App.InactivityTimeoutEvent>));
  count=api.SentCount;timeoutPublisher.Publish(new HuliacDev.App.InactivityTimeoutEvent());await Ready();await Wait(()=>api.SentCount==count+1);
  Check(manager.IsTitleScreen&&api.LastSentMessage=="move_idle_timeout","Timeout sends one dedicated message without duplicate move_idle");count=api.SentCount;timeoutPublisher.Publish(new HuliacDev.App.InactivityTimeoutEvent());await Frames(12);Check(api.SentCount==count,"Already-Idle timeout does not invent a screen move");
  int result=await api.ExecuteWithExternalApiLoggingAsync("https://content.example/api?item=hat&age=student",()=>Cysharp.Threading.Tasks.UniTask.FromResult(7),token);
  Check(result==7&&api.LastSentMessage=="Return OK","External success logs exact Call API URL and Return OK");
  bool failed=false;try{await api.ExecuteWithExternalApiLoggingAsync("https://content.example/api?item=bag&age=child",()=>Cysharp.Threading.Tasks.UniTask.FromException<int>(new InvalidOperationException("Original content failure")),token);}catch(InvalidOperationException ex){failed=ex.Message=="Original content failure";}
  Check(failed&&api.LastSentMessage=="Return fail","External failure logs exact Return fail and preserves original exception");
  count=api.SentCount;await api.ExecuteWithExternalApiLoggingAsync("http://127.0.0.1:8188/prompt",()=>Cysharp.Threading.Tasks.UniTask.FromResult(1),token);await api.ExecuteWithExternalApiLoggingAsync("https://comfy.example/prompt",()=>Cysharp.Threading.Tasks.UniTask.FromResult(1),token);await api.ExecuteWithExternalApiLoggingAsync("https://www.huliac.com/api/content_log.cfm?message=x",()=>Cysharp.Threading.Tasks.UniTask.FromResult(1),token);
  Check(api.SentCount==count,"Comfy and logging requests bypass both Call and Return messages");
  var generationServer=manager.GetComponent<Exhibition.Blocks.ComfyServerLauncher>();var generationSettings=(Exhibition.Blocks.ComfyBlockSettings)generationServer.GetType().GetField("_settings",flags).GetValue(generationServer);string oldGenerationUrl=generationSettings.serverUrl;
  try{generationSettings.serverUrl="https://192.0.2.10:8188";await api.ExecuteWithExternalApiLoggingAsync("https://192.0.2.10:8188/prompt",()=>Cysharp.Threading.Tasks.UniTask.FromResult(1),token);Check(api.SentCount==count,"Configured remote Comfy origin is excluded without relying on its host name");}finally{generationSettings.serverUrl=oldGenerationUrl;}
  await api.ExecuteWithExternalApiLoggingAsync("https://content.example/api?item=hat&api_key=fixture-secret",()=>Cysharp.Threading.Tasks.UniTask.FromResult(1),token);
  var wire=System.IO.File.ReadAllText("Temp/ContentLog/requests.jsonl");Check(wire.Contains("api_key=[REDACTED]")&&!wire.Contains("fixture-secret"),"Actual wire logs keep public parameters and redact credential values");count=api.SentCount;
  var calls=(MessagePipe.IPublisher<HuliacDev.App.ExternalApiCallEvent>)scope.Container.Resolve(typeof(MessagePipe.IPublisher<HuliacDev.App.ExternalApiCallEvent>));var returns=(MessagePipe.IPublisher<HuliacDev.App.ExternalApiReturnEvent>)scope.Container.Resolve(typeof(MessagePipe.IPublisher<HuliacDev.App.ExternalApiReturnEvent>));
  calls.Publish(new HuliacDev.App.ExternalApiCallEvent("http://127.0.0.1:8188/history"));returns.Publish(new HuliacDev.App.ExternalApiReturnEvent(true));await Frames(12);Check(api.SentCount==count,"Legacy paired events also exclude local Comfy returns");
  calls.Publish(new HuliacDev.App.ExternalApiCallEvent("https://content.example/api?message=a%26b&lang=th"));returns.Publish(new HuliacDev.App.ExternalApiReturnEvent(false,"Unsent diagnostic detail"));await Wait(()=>api.SentCount==count+2);Check(api.LastSentMessage=="Return fail","Legacy events preserve the exact failure vocabulary");
  var comfySettings=new Exhibition.Blocks.ComfyBlockSettings();var comfy=new Exhibition.Blocks.ComfyImageClient(comfySettings);count=api.SentCount;await comfy.Json("/system_stats",null,token);await Frames();Check(api.SentCount==count,"Actual Comfy API request produces no content telemetry");
  await (Cysharp.Threading.Tasks.UniTask)api.GetType().GetMethod("SendExitLogAsync",flags).Invoke(api,null);Check(api.LastSentMessage=="end","Normal exit sends exactly end without a quit-reason suffix");
  settings.apiUrl="http://127.0.0.1:18196/fail?message=";count=api.SentCount;var watch=System.Diagnostics.Stopwatch.StartNew();result=await api.ExecuteWithExternalApiLoggingAsync("https://content.example/retry?item=hat",()=>Cysharp.Threading.Tasks.UniTask.FromResult(8),token);
  Check(result==8&&api.SentCount==count&&watch.Elapsed.TotalSeconds<12,"Logging failure is bounded and does not fail the content action");
  report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{settings.apiUrl=originalUrl;Application.runInBackground=oldBackground;if(keyboard)keyboard.enabled=oldKeyboard;if(manager)manager.ResetButtonInput();System.IO.Directory.CreateDirectory("docs/validation/content_log");System.IO.File.WriteAllText("docs/validation/content_log/contract.json",report.ToString());UnityEngine.Object.Destroy(root);}
});
return "Local content-log contract and real scene-return verification started.";
