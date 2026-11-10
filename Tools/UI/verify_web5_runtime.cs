// Run in Play mode from 0_Idle with Unity execute_code. UI-WEB-005 / AUDIO-002:
// new Thai font assets on screen, language/selected sounds, and the 30 s no-block timeout on the block screen.
// Writes docs/validation/ui_web5/runtime.json and screenshots. No AI generation is requested.
if(GameObject.Find("UIWeb5Runtime"))return "Already running.";
var root=new GameObject("UIWeb5Runtime");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 const string Out="docs/validation/ui_web5/";
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="UI-WEB-005 / AUDIO-002 runtime"};var checks=new Newtonsoft.Json.Linq.JArray();report["checks"]=checks;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var hub=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidReaderHub>();var adapter=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
 var timer=UnityEngine.Object.FindFirstObjectByType<HuliacDev.Core.InactivityTimer>();
 bool wasEnabled=adapter&&adapter.enabled,background=Application.runInBackground;var initial=manager.CurrentLanguage;var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
 using var deadline=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(240));var token=deadline.Token;
 var played=new System.Collections.Generic.List<string>();
 Application.LogCallback onLog=(m,s,t)=>{if(m.StartsWith("[AudioManager] Played "))played.Add(m.Substring(22).TrimEnd('.'));};
 var inputEvents=new System.Collections.Generic.Dictionary<string,int>();bool countInput=false;
 Action<UnityEngine.InputSystem.LowLevel.InputEventPtr,UnityEngine.InputSystem.InputDevice> onInput=(e,d)=>{if(!countInput)return;string k=d!=null?d.layout+":"+d.name:"null";inputEvents[k]=inputEvents.TryGetValue(k,out int n)?n+1:1;};
 void Check(bool ok,string label){if(!ok)throw new Exception(label+" | played="+string.Join(",",played));checks.Add(label);}
 int Count(string clip){int n=0;foreach(var p in played)if(p==clip)n++;return n;}
 async Cysharp.Threading.Tasks.UniTask Frames(int n=5){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Ready(){await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsInputReady&&!manager.IsTransitioning&&!manager.IsPresentationLocked&&!manager.IsLanguageChanging&&hub.IsArtworkReady,cancellationToken:token);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Tap(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Frames();await Ready();}
 async Cysharp.Threading.Tasks.UniTask Toggle(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Cysharp.Threading.Tasks.UniTask.Delay(2300,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);manager.SetButtonPressed(false);await Frames();await Ready();}
 async Cysharp.Threading.Tasks.UniTask Shot(string name){Canvas.ForceUpdateCanvases();await Cysharp.Threading.Tasks.UniTask.WaitForEndOfFrame(hub,token);var shot=ScreenCapture.CaptureScreenshotAsTexture();System.IO.File.WriteAllBytes(Out+name+".png",shot.EncodeToPNG());UnityEngine.Object.Destroy(shot);await Frames();}
 void Bindings(string code){var set=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.LocalizedImageSet>();var doc=JsonUtility.FromJson<Exhibition.UI.ImageLanguageDocument>(System.IO.File.ReadAllText("Assets/StreamingAssets/Lang_"+(code=="th"?"Th":"En")+".json"));foreach(var b in (Exhibition.UI.LocalizedImageSet.Binding[])set.GetType().GetField("bindings",flags).GetValue(set)){var e=System.Array.Find(doc.images,x=>x.id==b.id);Check(b.image&&b.image.sprite&&b.image.sprite.name==e.sprite,"Binding sprite: "+code+" / "+b.id);}}
 try{
  Application.logMessageReceived+=onLog;UnityEngine.InputSystem.InputSystem.onEvent+=onInput;
  Application.runInBackground=true;if(adapter)adapter.enabled=false;await Ready();
  // Language sound: one per accepted toggle.
  int before=Count("language");await Toggle();Check(Count("language")==before+1,"Language toggle plays language once");
  if(manager.CurrentLanguage!=Core.SessionLanguage.Thai){await Toggle();Check(Count("language")==before+2,"Second toggle plays language again");}
  Check(manager.CurrentLanguage==Core.SessionLanguage.Thai,"Thai selected");Bindings("th");await Shot("idle_th");
  await Tap();var controller=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>controller&&controller.IsReady,cancellationToken:token);await Ready();Bindings("th");
  var view=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.BlockExperienceView>();await Shot("group_empty_th");
  // Group-first rejects a product before the group: no selected sound.
  int sel=Count("selected");hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Product,"shoes");await Frames();Check(Count("selected")==sel,"Rejected block on group page is silent");
  hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Group,"student");await Ready();Check(Count("selected")==sel+1,"Group recognition plays selected");
  var artwork=hub.Artwork.Find(new Exhibition.Blocks.RfidBlock(Exhibition.Blocks.BlockKind.Group,"student"),Core.SessionLanguage.Thai);
  Check(artwork&&artwork.name.Contains("Thai"),"Thai student block artwork: "+(artwork?artwork.name:"null"));await Shot("group_th");
  hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Group,"student");await Frames();Check(Count("selected")==sel+1,"Same card reported again is silent");
  await Tap();Check(view.IsChoosingProduct,"Button moves to selection");
  sel=Count("selected");hub.RecognizeReader(3,Exhibition.Blocks.BlockKind.Color,"red");hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Product,"shoes");hub.RecognizeReader(1,Exhibition.Blocks.BlockKind.Color,"blue");
  await Frames(1);Check(Count("selected")==sel+1,"Three cards in one frame play selected once");await Ready();await Shot("selection_full_th");
  sel=Count("selected");hub.RemoveReader(1);await Frames();Check(Count("selected")==sel,"Removal is silent");hub.RecognizeReader(1,Exhibition.Blocks.BlockKind.Color,"yellow");await Frames();Check(Count("selected")==sel+1,"Replacement plays selected");
  // No-block timeout: clear the board so the visitor has nothing placed, then wait without input.
  manager.ReturnToIdleManually();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsTitleScreen,cancellationToken:token);await Ready();
  await Tap();controller=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>controller&&controller.IsReady,cancellationToken:token);
  Check(hub.Readers.OccupiedCount==0,"Block screen has no blocks");
  float timeout=(float)typeof(HuliacDev.Core.InactivityTimer).GetField("_timeoutSeconds",flags).GetValue(timer);
  float last=(float)typeof(HuliacDev.Core.InactivityTimer).GetField("_lastActivityTime",flags).GetValue(timer);
  float start=Time.unscaledTime;inputEvents.Clear();countInput=true;int home=Count("home");
  await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsTransitioning||manager.IsTitleScreen,cancellationToken:token);
  float fired=Time.unscaledTime;countInput=false;
  await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsTitleScreen,cancellationToken:token);await Ready();
  report["timeoutSetting"]=timeout;report["timeoutSinceLastActivity"]=Math.Round(fired-last,2);report["timeoutSinceBlockScreenReady"]=Math.Round(fired-start,2);
  var ev=new Newtonsoft.Json.Linq.JObject();foreach(var kv in inputEvents)ev[kv.Key]=kv.Value;report["inputEventsWhileWaiting"]=ev;
  Check(timer.IsEnabled&&Math.Abs(timeout-30f)<.01f,"Timer enabled with 30 s");
  Check(fired-last>=timeout-.1f&&fired-last<=timeout+1.5f,"No-block screen returns home about 30 s after last activity ("+Math.Round(fired-last,2)+" s)");
  Check(Count("home")==home+1,"Timeout return plays home");
  // English pass for the edited help boxes.
  await Toggle();Check(manager.CurrentLanguage==Core.SessionLanguage.English,"English selected");Bindings("en");await Shot("idle_en");
  await Tap();controller=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>controller&&controller.IsReady,cancellationToken:token);await Ready();Bindings("en");await Shot("group_empty_en");
  hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Group,"student");await Ready();await Tap();await Shot("selection_group_only_en");
  manager.ReturnToIdleManually();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsTitleScreen,cancellationToken:token);await Ready();
  if(manager.CurrentLanguage!=initial)await Toggle();
  report["played"]=new Newtonsoft.Json.Linq.JArray(played.ToArray());report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();report["played"]=new Newtonsoft.Json.Linq.JArray(played.ToArray());Debug.LogException(ex);}
 finally{Application.logMessageReceived-=onLog;UnityEngine.InputSystem.InputSystem.onEvent-=onInput;if(manager)manager.ResetButtonInput();if(adapter)adapter.enabled=wasEnabled;Application.runInBackground=background;System.IO.File.WriteAllText(Out+"runtime.json",report.ToString());UnityEngine.Object.Destroy(root);}
});
return "UI-WEB-005 runtime verification started.";
