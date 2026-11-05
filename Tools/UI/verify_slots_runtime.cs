// Run in Play mode from 0_Idle. Uses no AI generation: the checks stop before the button would generate.
if(GameObject.Find("UISlotsVerification"))return "Already running.";
var root=new GameObject("UISlotsVerification");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="UI-SLOTS-002"};var checks=new Newtonsoft.Json.Linq.JArray();report["checks"]=checks;var motion=new Newtonsoft.Json.Linq.JObject();report["motion"]=motion;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var hub=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidReaderHub>();var adapter=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
 bool wasEnabled=adapter&&adapter.enabled,background=Application.runInBackground;var initial=manager.CurrentLanguage;
 using var deadline=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(120));var token=deadline.Token;
 void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
 async Cysharp.Threading.Tasks.UniTask Frames(int n=5){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Ready(){await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsInputReady&&!manager.IsTransitioning&&!manager.IsPresentationLocked&&!manager.IsLanguageChanging&&hub.IsArtworkReady,cancellationToken:token);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Press(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Language(Core.SessionLanguage language){await Ready();if(manager.CurrentLanguage==language)return;manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Cysharp.Threading.Tasks.UniTask.Delay(2300,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);manager.SetButtonPressed(false);await Frames();await Ready();Check(manager.CurrentLanguage==language,"Title language switch: "+language);}
 async Cysharp.Threading.Tasks.UniTask Enter(){await Press();await Ready();var c=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>c&&c.IsReady,cancellationToken:token);await Ready();}
 async Cysharp.Threading.Tasks.UniTask Shot(string name){Canvas.ForceUpdateCanvases();await Cysharp.Threading.Tasks.UniTask.WaitForEndOfFrame(hub,token);var shot=ScreenCapture.CaptureScreenshotAsTexture();System.IO.File.WriteAllBytes("docs/validation/ui_slots/"+name+".png",shot.EncodeToPNG());UnityEngine.Object.Destroy(shot);await Frames();}
 Exhibition.UI.BlockExperienceView View()=>UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.BlockExperienceView>();
 UnityEngine.UI.Image Frame(string path)=>View().transform.Find(path).GetComponent<UnityEngine.UI.Image>();
 UnityEngine.UI.Image Art(string path)=>View().transform.Find(path+"/BlockContent").GetComponent<UnityEngine.UI.Image>();
 void Empty(string path,string sprite,string code){var f=Frame(path);var a=Art(path);Check(f.enabled&&f.sprite&&f.sprite.name==sprite+"_"+code&&!a.enabled,"Empty slot shows "+sprite+": "+code+" / "+path);}
 void Filled(string path,Exhibition.Blocks.BlockKind kind,string id,string code){var f=Frame(path);var a=Art(path);var expected=hub.Artwork.Find(new Exhibition.Blocks.RfidBlock(kind,id),manager.CurrentLanguage);Check(!f.enabled&&a.enabled&&a.sprite==expected,"Block "+id+" fills its purpose slot: "+code+" / "+path);}
 void Rect(string path,float x,float y,float w,float h,string code){var r=(RectTransform)View().transform.Find(path);var c=new Vector3[4];r.GetWorldCorners(c);var canvas=r.GetComponentInParent<Canvas>().rootCanvas.GetComponent<RectTransform>();Vector2 bl=canvas.InverseTransformPoint(c[0]),tr=canvas.InverseTransformPoint(c[2]);Vector2 half=canvas.rect.size*.5f;float left=bl.x+half.x,top=half.y-tr.y;Check(Mathf.Abs(left-x)<.6f&&Mathf.Abs(top-y)<.6f&&Mathf.Abs(tr.x-bl.x-w)<.6f&&Mathf.Abs(tr.y-bl.y-h)<.6f,"Web geometry "+path+": "+code+" got "+left+","+top+","+(tr.x-bl.x)+","+(tr.y-bl.y));}
 var G=Exhibition.Blocks.BlockKind.Group;var P=Exhibition.Blocks.BlockKind.Product;var C=Exhibition.Blocks.BlockKind.Color;
 const string GF="GroupSelection/GroupFrame0",S0="ProductAndColors/SelectionFrame0",S1="ProductAndColors/SelectionFrame1",S2="ProductAndColors/SelectionFrame2",S3="ProductAndColors/SelectionFrame3";
 try{
  Application.runInBackground=true;if(adapter)adapter.enabled=false;await Ready();
  foreach(var language in new[]{Core.SessionLanguage.Thai,Core.SessionLanguage.English}){
   await Language(language);string code=language==Core.SessionLanguage.Thai?"th":"en";int top=code=="th"?394:395;
   await Enter();var view=View();
   Check(view.transform.Find("GroupSelection/GroupBoard/GroupFrame1")==null&&view.transform.Find("GroupSelection/GroupFrame1")==null,"Group panel has a single slot: "+code);
   Rect("GroupSelection/GroupBoard",708,top-40,504,504,code);Rect(GF,748,top,424,424,code);Empty(GF,"GroupSlot",code);await Shot("group_empty_"+code);
   // A group on the bottom-right reader still fills the only group slot.
   hub.RecognizeReader(3,G,"student");await Ready();Filled(GF,G,"student",code);await Shot("group_"+code);
   // Sample the real transfer frame by frame while the button drives it.
   manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);
   var sizes=new Newtonsoft.Json.Linq.JArray();float last=float.MaxValue;bool shrinking=true,seen=false,targetHidden=true;double started=0,ended=0;bool othersActive=true;
   await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>view.transform.Find("BlockSlotTransfer"),cancellationToken:token);
   started=Time.realtimeSinceStartupAsDouble;
   while(view.transform.Find("BlockSlotTransfer")){
    var moving=view.transform.Find("BlockSlotTransfer").childCount>0?(RectTransform)view.transform.Find("BlockSlotTransfer").GetChild(0):null;
    if(moving){seen=true;float size=moving.sizeDelta.x;if(size>last+.01f)shrinking=false;last=size;if(sizes.Count<40)sizes.Add(new Newtonsoft.Json.Linq.JArray(Math.Round(size,1),Math.Round(moving.anchoredPosition.x,1),Math.Round(moving.anchoredPosition.y,1)));
     Check(view.transform.Find("BlockSlotTransfer").childCount==1,"Only the group frame moves: "+code);
     if(view.transform.Find(S0).gameObject.activeSelf)targetHidden=false;
     if(!view.transform.Find(S1).gameObject.activeSelf||!view.transform.Find(S2).gameObject.activeSelf||!view.transform.Find(S3).gameObject.activeSelf)othersActive=false;
     if(sizes.Count==12)await Shot("transfer_"+code);}
    await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);
   }
   ended=Time.realtimeSinceStartupAsDouble;motion["sizes_"+code]=sizes;motion["seconds_"+code]=Math.Round(ended-started,3);
   Check(seen&&shrinking,"Group frame shrinks continuously: "+code);Check(targetHidden,"Destination stays hidden until handoff: "+code);Check(othersActive,"Other selection slots crossfade with the panel: "+code);Check(ended-started>.4&&ended-started<.75,"Transfer keeps the 0.5s motion: "+code);
   await Ready();Check(view.IsChoosingProduct&&!view.transform.Find(GF).gameObject.activeInHierarchy,"Selection panel reached: "+code);
   Rect(S0,268,320,252,252,code);Rect(S1,536,320,252,252,code);Rect(S2,268,588,252,252,code);Rect(S3,536,588,252,252,code);
   Filled(S0,G,"student",code);Empty(S1,"ProductSlot",code);Empty(S2,"ColorSlot",code);Empty(S3,"ColorSlot",code);await Shot("selection_group_only_"+code);
   // Readers are deliberately not in purpose order.
   hub.RecognizeReader(0,C,"red");await Ready();Filled(S2,C,"red",code);Empty(S3,"ColorSlot",code);
   hub.RecognizeReader(2,P,"shoes");await Ready();Filled(S1,P,"shoes",code);
   hub.RecognizeReader(1,C,"blue");await Ready();Filled(S3,C,"blue",code);Check(view.AreRequiredBlocksRecognized,"Any reader arrangement completes the composition: "+code);await Shot("selection_full_"+code);
   var controller=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();
   Check(hub.Readers.Color1Id=="red"&&hub.Readers.Color2Id=="blue","Generation colors match the visible slots: "+code);Check(controller.Selection[Exhibition.Blocks.BlockSlot.Color1]=="red"&&controller.Selection[Exhibition.Blocks.BlockSlot.Color2]=="blue","Controller selection follows the slots: "+code);
   // Removing the group reuses the group panel's frame at the selection size and stays on this page.
   hub.RemoveReader(3);await Ready();Empty(S0,"GroupSlot",code);Rect(S0,268,320,252,252,code);Check(view.IsChoosingProduct&&!view.AreRequiredBlocksRecognized,"Group removal keeps the page and blocks continue: "+code);await Shot("selection_group_removed_"+code);
   await Press();await Ready();Check(view.IsChoosingProduct&&!controller.IsGenerating,"Short input ignored without a group: "+code);
   hub.RecognizeReader(3,G,"senior");await Ready();Filled(S0,G,"senior",code);Check(view.AreRequiredBlocksRecognized,"Group can be re-recognized: "+code);
   // Removing the first color leaves the second in place.
   hub.RemoveReader(0);await Ready();Empty(S2,"ColorSlot",code);Filled(S3,C,"blue",code);
   hub.RecognizeReader(0,C,"green");await Ready();Filled(S2,C,"green",code);Filled(S3,C,"blue",code);
   // Duplicate product waits and takes over.
   hub.RemoveReader(1);hub.RecognizeReader(1,P,"bags");await Ready();Filled(S1,P,"shoes",code);Check(!view.AreRequiredBlocksRecognized,"Two products never complete the composition: "+code);
   hub.RemoveReader(2);await Ready();Filled(S1,P,"bags",code);
   // Home clears everything.
   manager.SetButtonPressed(true);await Cysharp.Threading.Tasks.UniTask.Delay(5200,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);manager.SetButtonPressed(false);await Ready();Check(manager.IsTitleScreen&&hub.Readers.OccupiedCount==0,"Five-second home clears readers: "+code);
   // Auto-advance path: a new non-group block starts the same transfer.
   await Enter();hub.RecognizeReader(1,G,"child");await Ready();Filled(GF,G,"child",code);hub.RecognizeReader(0,P,"hats");
   await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>View().IsChoosingProduct,cancellationToken:token);await Ready();Filled(S0,G,"child",code);Filled(S1,P,"hats",code);
   // Leaving during a transfer must not leak the layer or throw.
   manager.SetButtonPressed(true);await Cysharp.Threading.Tasks.UniTask.Delay(5200,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);manager.SetButtonPressed(false);await Ready();
   await Enter();hub.RecognizeReader(2,G,"child");await Ready();manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>View().transform.Find("BlockSlotTransfer"),cancellationToken:token);
   View().enabled=false;await Frames(8);Check(!View().transform.Find("BlockSlotTransfer"),"Cancelled transfer removes its layer: "+code);Check(View().transform.Find(GF).parent.name=="GroupSelection","Cancelled transfer restores the group frame: "+code);
   View().enabled=true;await Frames();
   await Ready();manager.SetButtonPressed(true);await Cysharp.Threading.Tasks.UniTask.Delay(5200,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);manager.SetButtonPressed(false);await Ready();Check(manager.IsTitleScreen,"Return to title after cancellation: "+code);
  }
  await Language(initial);report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{if(manager)manager.ResetButtonInput();if(adapter)adapter.enabled=wasEnabled;Application.runInBackground=background;System.IO.File.WriteAllText("docs/validation/ui_slots/runtime.json",report.ToString());UnityEngine.Object.Destroy(root);}
});
return "Purpose slot verification started.";
