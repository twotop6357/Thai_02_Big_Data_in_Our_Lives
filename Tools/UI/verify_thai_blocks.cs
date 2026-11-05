// Explicit Play-mode verification from Idle; no image generation or source asset editing.
if(GameObject.Find("ThaiBlocksVerification"))return "Already running.";
var root=new GameObject("ThaiBlocksVerification");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="THAI-BLOCKS-001"};var checks=new Newtonsoft.Json.Linq.JArray();report["checks"]=checks;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var adapter=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
 bool oldAdapter=adapter&&adapter.enabled,oldBackground=Application.runInBackground;var initial=manager.CurrentLanguage;
 using var limit=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(100));var token=limit.Token;
 void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
 async Cysharp.Threading.Tasks.UniTask Frames(int count=3){for(int i=0;i<count;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Ready(){await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager&&!manager.IsTransitioning&&!manager.IsPresentationLocked&&manager.IsInputReady,cancellationToken:token);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Tap(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Frames();await Ready();}
 async Cysharp.Threading.Tasks.UniTask Language(Core.SessionLanguage language){await Ready();if(manager.CurrentLanguage==language)return;manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Cysharp.Threading.Tasks.UniTask.Delay(2300,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);manager.SetButtonPressed(false);await Frames();await Ready();Check(manager.CurrentLanguage==language,"Title language switched to "+language);}
 try{
  Application.runInBackground=true;if(adapter)adapter.enabled=false;await Ready();Check(manager.IsTitleScreen,"Verification begins in Idle");
  var hub=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidReaderHub>();var catalog=hub.Artwork;
  Check(catalog.entries.Length==20,"All 20 recognition entries retained");int localized=0;
  foreach(var entry in catalog.entries){
   string en=entry.sprite.texture.name;Check(en.StartsWith("ExternalBlock/Images/en/"),"English external source: "+entry.id);
   if(entry.kind!=Exhibition.Blocks.BlockKind.Group){string th=entry.thaiSprite.texture.name;Check(th==en.Replace("/en/","/th/").Replace("_Eng.png","_Thai.png"),"Thai external source: "+entry.id);localized++;}
   Check(catalog.Find(entry.Block,Core.SessionLanguage.English)==entry.sprite&&catalog.Find(entry.kind,entry.id)==entry,"English sprite and stable recognition id: "+entry.id);
  }
  Check(localized==16,"16 localized products/colors registered");
  Check(catalog.Find(null,Core.SessionLanguage.Thai)==null&&catalog.Find(new Exhibition.Blocks.RfidBlock(Exhibition.Blocks.BlockKind.Product,"unregistered"),Core.SessionLanguage.English)==null,"Empty/unknown blocks remain empty");
  int slotChecks=0;
  foreach(var language in new[]{Core.SessionLanguage.Thai,Core.SessionLanguage.English,Core.SessionLanguage.Thai}){
   await Language(language);await Tap();
   var controller=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();
   await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>controller&&controller.IsReady,cancellationToken:token);await Ready();
   var view=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.BlockExperienceView>();
   hub.ClearReaders();hub.RecognizeReader(3,Exhibition.Blocks.BlockKind.Group,"child");await Ready();await Tap();
   Check(view.IsChoosingProduct,"Group-first button entry enables full artwork inspection in "+language);
   var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
   var group=(UnityEngine.UI.Image[])typeof(Exhibition.UI.BlockExperienceView).GetField("_groupArtwork",flags).GetValue(view);
   var selection=(UnityEngine.UI.Image[])typeof(Exhibition.UI.BlockExperienceView).GetField("_selectionArtwork",flags).GetValue(view);
   for(int i=0;i<catalog.entries.Length;i++){
    hub.ClearReaders();var entry=catalog.entries[i];int reader=i%4;Check(hub.RecognizeReader(reader,entry.kind,entry.id),language+" recognizes "+entry.id);await Frames();
    var expected=language==Core.SessionLanguage.Thai&&entry.thaiSprite?entry.thaiSprite:entry.sprite;
    for(int slot=0;slot<4;slot++){var sprite=slot==reader?expected:null;Check(group[slot].sprite==sprite&&selection[slot].sprite==sprite&&group[slot].enabled==(sprite!=null)&&selection[slot].enabled==(sprite!=null),language+" panels match "+entry.id+" / reader "+slot);slotChecks+=2;}
    Check(!controller.IsGenerating,"Single-block display does not generate images: "+entry.id);
   }
   // An incomplete composition displays Thai/English labels without triggering generation.
   hub.ClearReaders();hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Product,"bags");hub.RecognizeReader(1,Exhibition.Blocks.BlockKind.Color,"red");hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Color,"blue");hub.RecognizeReader(3,Exhibition.Blocks.BlockKind.Product,"shoes");await Ready();await Frames(5);
   UnityEngine.ScreenCapture.CaptureScreenshot("docs/validation/thai_blocks/runtime_"+language+".png");await Frames(4);
   var simulator=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidSimulator>();simulator.SetVisible(true);await Frames(5);
   Check(simulator.IsVisible,"F9 palette opens in "+language);UnityEngine.ScreenCapture.CaptureScreenshot("docs/validation/thai_blocks/simulator_"+language+".png");await Frames(4);simulator.SetVisible(false);
   manager.ReturnToIdleManually();await Ready();Check(hub.Readers.OccupiedCount==0,"Home clears readers after "+language);
  }
  report["image_slot_checks"]=slotChecks;await Language(initial);Check(manager.IsTitleScreen,"Idle restored");report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{
  if(manager)manager.ResetButtonInput();if(adapter)adapter.enabled=oldAdapter;Application.runInBackground=oldBackground;
  System.IO.File.WriteAllText("docs/validation/thai_blocks/runtime.json",report.ToString());UnityEngine.Object.Destroy(root);
 }
});
return "Thai/English/Thai block UI verification started.";
