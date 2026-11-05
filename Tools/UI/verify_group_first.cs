// Explicit actual-app verification from Idle. Includes one normal ComfyUI request.
if(GameObject.Find("GroupFirstVerification"))return "Already running.";
var root=new GameObject("GroupFirstVerification");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="GROUP-FIRST-001"};var checks=new Newtonsoft.Json.Linq.JArray();report["checks"]=checks;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var adapter=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
 var hub=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidReaderHub>();
 Exhibition.UI.BlockExperienceView view=null;Exhibition.Blocks.BlockExperienceController controller=null;
 bool oldAdapter=adapter&&adapter.enabled,oldBackground=Application.runInBackground;var initialLanguage=manager.CurrentLanguage;
 IDisposable lease=null;using var limit=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(160));var token=limit.Token;
 void Save()=>System.IO.File.WriteAllText("docs/validation/group_first/runtime.json",report.ToString());
 void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);Save();}
 async Cysharp.Threading.Tasks.UniTask Frames(int n=4){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Ready(){await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsInputReady&&!manager.IsTransitioning&&!manager.IsPresentationLocked,cancellationToken:token);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Tap(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Enter(){await Ready();await Tap();await Ready();controller=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>controller&&controller.IsReady,cancellationToken:token);view=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.BlockExperienceView>();Check(view&&!view.IsChoosingProduct&&hub.Readers.RequireGroupFirst,"New session begins with group-first enabled");}
 async Cysharp.Threading.Tasks.UniTask Home(){manager.ReturnToIdleManually();await Ready();Check(manager.IsTitleScreen&&hub.Readers.OccupiedCount==0&&hub.Readers.RequireGroupFirst,"Home clears occupancy and restores group-first");}
 void PanelsMatch(){var f=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;var g=(UnityEngine.UI.Image[])typeof(Exhibition.UI.BlockExperienceView).GetField("_groupArtwork",f).GetValue(view);var s=(UnityEngine.UI.Image[])typeof(Exhibition.UI.BlockExperienceView).GetField("_selectionArtwork",f).GetValue(view);for(int i=0;i<4;i++){var sprite=hub.Artwork.Find(hub.Readers[i],manager.CurrentLanguage);Check(g[i].sprite==sprite&&s[i].sprite==sprite,"Both localized panels match reader "+i);}Check(manager.Presentation.BlackoutAlpha==0,"Panel transition adds no blackout");}
 try{
  System.IO.Directory.CreateDirectory("docs/validation/group_first");Application.runInBackground=true;if(adapter)adapter.enabled=false;
  await Enter();int revision=hub.Readers.Revision;
  foreach(var entry in hub.Artwork.entries)if(entry.kind!=Exhibition.Blocks.BlockKind.Group){Check(!hub.RecognizeReader(0,entry.kind,entry.id),"Before group, ignores "+entry.id);}
  await Frames();Check(hub.Readers.OccupiedCount==0&&hub.Readers.Revision==revision&&!view.AreRequiredBlocksRecognized&&!view.IsChoosingProduct,"Ignored blocks are not stored or displayed");PanelsMatch();
  hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Group,"child");await Ready();Check(view.AreRequiredBlocksRecognized&&!view.IsChoosingProduct&&hub.Readers.OccupiedCount==1,"One group alone waits for button or new non-group");
  hub.RecognizeReader(1,Exhibition.Blocks.BlockKind.Group,"senior");await Ready();await Tap();await Ready();Check(!view.IsChoosingProduct&&!view.AreRequiredBlocksRecognized,"Two groups block manual progress");
  Check(!hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Product,"bags")&&!hub.RecognizeReader(3,Exhibition.Blocks.BlockKind.Color,"red"),"Two groups reject non-group progress");await Ready();Check(!view.IsChoosingProduct,"Two groups block automatic progress");
  hub.RemoveReader(1);await Ready();await Tap();await Ready();Check(view.IsChoosingProduct&&!hub.Readers.RequireGroupFirst,"Existing button path still advances with one group");
  hub.RemoveReader(2);Check(hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Product,"bags"),"Product stage keeps accepting blocks after group removal");await Ready();Check(view.IsChoosingProduct,"Group removal after progress never returns to previous page");await Home();
  string[] ages={"child","student","working_adult","senior"};
  for(int i=0;i<ages.Length;i++){
   await Enter();lease=manager.Presentation.LockInput();int groupReader=i,other=(i+1)%4;
   hub.RecognizeReader(groupReader,Exhibition.Blocks.BlockKind.Group,ages[i]);
   bool color=i%2==1;Check(hub.RecognizeReader(other,color?Exhibition.Blocks.BlockKind.Color:Exhibition.Blocks.BlockKind.Product,color?"blue":"shoes"),"Accepts fresh non-group after "+ages[i]);
   await Frames();Check(!view.IsChoosingProduct,"Automatic progress waits for animation lock: "+ages[i]);lease.Dispose();lease=null;await Ready();
   Check(view.IsChoosingProduct&&!hub.Readers.RequireGroupFirst&&hub.Readers[other].HasValue,"Automatic button-equivalent progress retains new block: "+ages[i]);PanelsMatch();
   Check(!GameObject.Find("BlockSlotTransfer"),"Transfer layer cleaned up: "+ages[i]);await Home();
  }
  await Enter();lease=manager.Presentation.LockInput();hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Group,"child");hub.RecognizeReader(1,Exhibition.Blocks.BlockKind.Product,"bags");hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Group,"student");lease.Dispose();lease=null;await Ready();Check(!view.IsChoosingProduct,"A second group cancels queued automatic progress");
  hub.RemoveReader(2);await Ready();Check(!view.IsChoosingProduct,"Removing duplicate group alone does not replay the cancelled input");hub.RecognizeReader(1,Exhibition.Blocks.BlockKind.Product,"shoes");await Ready();Check(view.IsChoosingProduct,"Fresh non-group after resolving duplicate advances");await Home();
  await Enter();lease=manager.Presentation.LockInput();hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Group,"child");hub.RecognizeReader(1,Exhibition.Blocks.BlockKind.Color,"red");hub.RemoveReader(0);hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Group,"child");lease.Dispose();lease=null;await Ready();Check(!view.IsChoosingProduct,"Group removal cancels queued input even if group returns in same frame");
  hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Product,"bags");await Ready();Check(view.IsChoosingProduct,"A new non-group after restoring group advances normally");await Home();
  await Enter();hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Group,"child");Check(!hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Product,"bags"),"Replacing the sole group with product is ignored");await Ready();Check(hub.Readers.OccupiedCount==0&&!view.IsChoosingProduct,"Rejected replacement clears old group without advancing");await Home();
  await Enter();lease=manager.Presentation.LockInput();hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Group,"student");hub.RecognizeReader(1,Exhibition.Blocks.BlockKind.Product,"bags");hub.MoveReader(1,0);lease.Dispose();lease=null;await Ready();Check(hub.Readers.OccupiedCount==0&&!view.IsChoosingProduct,"Moving onto sole group cannot bypass gating or advance");await Home();
  await Enter();lease=manager.Presentation.LockInput();hub.RecognizeReader(3,Exhibition.Blocks.BlockKind.Group,"senior");hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Color,"blue");manager.ReturnToIdleManually();lease.Dispose();lease=null;await Ready();Check(manager.IsTitleScreen&&hub.Readers.OccupiedCount==0,"Home cancels queued automatic progress");await Enter();await Frames();Check(!view.IsChoosingProduct,"Cancelled automatic request never leaks into next session");await Home();
  // One real request verifies that queued navigation cannot bypass generation input locking.
  await Enter();hub.RecognizeReader(3,Exhibition.Blocks.BlockKind.Group,"student");hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Product,"shoes");hub.RecognizeReader(1,Exhibition.Blocks.BlockKind.Color,"red");hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Color,"blue");
  await Ready();Check(!controller.IsGenerating&&view.IsChoosingProduct,"Complete rapid input opens placement and waits for a generation button");await Tap();
  Check(controller.IsGenerating&&manager.IsPresentationLocked,"Fresh button starts normal generation with input locked");await Tap();Check(view.IsChoosingProduct&&!manager.IsTransitioning,"Manual navigation cannot leave placement during generation");
  await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>!controller.IsGenerating,cancellationToken:token);await Ready();Check(view.IsChoosingProduct&&!manager.IsTransitioning,"After generation, queued recognition opens only product selection");report["generation_status"]=controller.Status;report["generated_image_ready"]=controller.HasGeneratedImage;
  UnityEngine.ScreenCapture.CaptureScreenshot("docs/validation/group_first/after_auto.png");await Frames();await Home();
  report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{lease?.Dispose();if(controller)controller.CancelGeneration();if(manager)manager.ResetButtonInput();if(adapter)adapter.enabled=oldAdapter;Application.runInBackground=oldBackground;Save();UnityEngine.Object.Destroy(root);}
});
return "Group-first recognition and automatic panel verification started.";
