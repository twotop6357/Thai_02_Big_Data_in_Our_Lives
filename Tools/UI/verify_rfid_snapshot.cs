// Explicit Play-mode check from Idle. One real generation plus local fixture lifecycle checks.
if(GameObject.Find("RfidSnapshotVerification"))return "Already running.";
var root=new GameObject("RfidSnapshotVerification");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="RFID-SNAPSHOT-001"};var checks=new Newtonsoft.Json.Linq.JArray();report["checks"]=checks;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var hub=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidReaderHub>();var adapter=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();var timer=UnityEngine.Object.FindFirstObjectByType<HuliacDev.Core.InactivityTimer>();
 Exhibition.Blocks.BlockExperienceController blocks=null;bool wasEnabled=adapter&&adapter.enabled,background=Application.runInBackground;
 int changed=0,readerChanged=0;Action onChanged=()=>changed++;Action<int,Exhibition.Blocks.RfidBlock?,Exhibition.Blocks.RfidBlock?> onReader=(i,a,b)=>readerChanged++;
 using var limit=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(90));var token=limit.Token;
 void Save()=>System.IO.File.WriteAllText("docs/validation/rfid_snapshot/runtime.json",report.ToString());
 void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);Save();}
 async Cysharp.Threading.Tasks.UniTask Frames(int n=4){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Ready(){await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsInputReady&&!manager.IsTransitioning&&!manager.IsPresentationLocked&&hub.IsArtworkReady,cancellationToken:token);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Tap(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Frames();}
 var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
 void Noise(){for(int i=0;i<4;i++){Check(!hub.RemoveReader(i),"Loss on reader "+i+" is ignored");Check(!hub.RecognizeReader(i,Exhibition.Blocks.BlockKind.Color,"yellow"),"Replacement on reader "+i+" is ignored");Check(!hub.Readers.Set(i,null),"Direct state removal on reader "+i+" is ignored");}Check(!hub.MoveReader(0,3),"Reader move is ignored");hub.ClearReaders();}
 try{
  Application.runInBackground=true;if(adapter)adapter.enabled=false;await Ready();await Tap();await Ready();blocks=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>blocks&&blocks.IsReady,cancellationToken:token);
  hub.RecognizeReader(3,Exhibition.Blocks.BlockKind.Group,"student");hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Product,"shoes");hub.RecognizeReader(1,Exhibition.Blocks.BlockKind.Color,"red");hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Color,"blue");await Ready();
  var view=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.BlockExperienceView>();var group=(UnityEngine.UI.Image[])view.GetType().GetField("_groupArtwork",flags).GetValue(view);var selection=(UnityEngine.UI.Image[])view.GetType().GetField("_selectionArtwork",flags).GetValue(view);
  var snapshot=new Exhibition.Blocks.RfidBlock?[4];var sprites=new Sprite[4];for(int i=0;i<4;i++){snapshot[i]=hub.Readers[i];sprites[i]=selection[i].sprite;}
  int revision=hub.Readers.Revision,selectionRevision=blocks.Selection.Revision;hub.Changed+=onChanged;hub.ReaderChanged+=onReader;
  await Tap();Check(blocks.IsGenerating&&hub.Readers.IsInputLocked,"Actual generation freezes the selected reader snapshot synchronously");Noise();await Frames();
  Check(hub.Readers.Revision==revision&&blocks.Selection.Revision==selectionRevision&&changed==0&&readerChanged==0,"RFID noise changes neither revision nor selection/events");
  for(int i=0;i<4;i++)Check(Nullable.Equals(hub.Readers[i],snapshot[i])&&group[i].sprite==sprites[i]&&selection[i].sprite==sprites[i],"Both panels preserve the generation-start block on reader "+i);
  Check(blocks.IsGenerating&&timer.IsPaused&&manager.IsPresentationLocked,"RFID loss does not cancel generation or release loading/input");UnityEngine.ScreenCapture.CaptureScreenshot("docs/validation/rfid_snapshot/generating_snapshot.png");await Frames();
  await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>!blocks.IsGenerating,cancellationToken:token);await Ready();
  Check(blocks.HasGeneratedImage&&!hub.Readers.IsInputLocked&&hub.Readers.Revision==revision&&changed==0,"Actual success keeps all original blocks and does not replay losses");UnityEngine.ScreenCapture.CaptureScreenshot("docs/validation/rfid_snapshot/success_snapshot.png");await Frames();
  Check(hub.RemoveReader(2)&&!blocks.HasGeneratedImage,"A fresh RFID signal after generation is accepted normally");hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Color,"blue");await Ready();
  var settings=(Exhibition.Blocks.ComfyBlockSettings)blocks.GetType().GetField("_settings",flags).GetValue(blocks);settings.serverUrl="http://127.0.0.1:18191";var fixture=new Exhibition.Blocks.ComfyImageClient(settings);
  async Cysharp.Threading.Tasks.UniTask Mode(string value)=>await fixture.Json("/mode",new Newtonsoft.Json.Linq.JObject{["mode"]=value},token);
  await Mode("delay");blocks.GenerateImage();await Frames();Noise();blocks.CancelGeneration();await Ready();Check(!hub.Readers.IsInputLocked&&hub.Readers.OccupiedCount==4&&!timer.IsPaused,"Explicit cancellation releases the lease without applying ignored changes");
  blocks.GenerateImage();blocks.CancelGeneration();blocks.GenerateImage();await Frames();Check(hub.Readers.IsInputLocked&&!hub.RemoveReader(0),"An old request finalizer cannot unlock the replacement generation");blocks.CancelGeneration();await Ready();
  settings.generationTimeoutSeconds=1;blocks.GenerateImage();await Frames();Check(!hub.RemoveReader(3),"Group loss during timeout wait is ignored");await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>!blocks.IsGenerating,cancellationToken:token);await Ready();Check(!hub.Readers.IsInputLocked&&hub.Readers.OccupiedCount==4&&!timer.IsPaused,"Timeout releases the lease and preserves retry selection");settings.generationTimeoutSeconds=10;
  await Mode("error");blocks.GenerateImage();Check(hub.Readers.IsInputLocked,"Submission failure starts under the same snapshot lock");await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>!blocks.IsGenerating,cancellationToken:token);await Ready();Check(!hub.Readers.IsInputLocked&&hub.Readers.OccupiedCount==4&&blocks.Status.Contains("failed"),"Server failure releases the RFID lease without losing the snapshot");
  await Mode("delay");blocks.GenerateImage();await Frames();blocks.enabled=false;await Frames();Check(!hub.Readers.IsInputLocked&&!timer.IsPaused,"Disabling the content screen releases its RFID lease");blocks.enabled=true;await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>blocks.IsReady,cancellationToken:token);await Ready();
  blocks.GenerateImage();await Frames();Check(hub.Readers.IsInputLocked,"Repeated entry can acquire a new generation snapshot");manager.ReturnToIdleManually();await Ready();Check(manager.IsTitleScreen&&hub.Readers.OccupiedCount==0&&!hub.Readers.IsInputLocked&&!timer.IsPaused,"Home cancels generation, clears all readers and releases the lease");
  await Tap();await Ready();blocks=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>blocks&&blocks.IsReady,cancellationToken:token);Check(hub.Readers.OccupiedCount==0&&!hub.Readers.IsInputLocked,"Ignored signals never reappear in the next session");manager.ReturnToIdleManually();await Ready();report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{hub.Changed-=onChanged;hub.ReaderChanged-=onReader;if(blocks)blocks.CancelGeneration();if(manager&&!manager.IsTitleScreen)manager.ReturnToIdleManually();if(adapter)adapter.enabled=wasEnabled;Application.runInBackground=background;Save();UnityEngine.Object.Destroy(root);}
});
return "RFID snapshot generation/lifecycle verification started.";
