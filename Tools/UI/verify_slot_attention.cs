// Run in Play mode from 0_Idle. Uses no AI generation: the checks stop before the button would generate.
if(GameObject.Find("UIAttentionVerification"))return "Already running.";
var root=new GameObject("UIAttentionVerification");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="UI-ATTN-001"};var checks=new Newtonsoft.Json.Linq.JArray();report["checks"]=checks;var timing=new Newtonsoft.Json.Linq.JObject();report["timing"]=timing;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var hub=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidReaderHub>();var adapter=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
 bool wasEnabled=adapter&&adapter.enabled,background=Application.runInBackground;var initial=manager.CurrentLanguage;
 using var deadline=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(240));var token=deadline.Token;
 void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
 async Cysharp.Threading.Tasks.UniTask Frames(int n=5){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Ready(){await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsInputReady&&!manager.IsTransitioning&&!manager.IsPresentationLocked&&!manager.IsLanguageChanging&&hub.IsArtworkReady,cancellationToken:token);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Press(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Language(Core.SessionLanguage language){await Ready();if(manager.CurrentLanguage==language)return;manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Cysharp.Threading.Tasks.UniTask.Delay(2300,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);manager.SetButtonPressed(false);await Frames();await Ready();Check(manager.CurrentLanguage==language,"Title language switch: "+language);}
 async Cysharp.Threading.Tasks.UniTask Enter(){await Press();await Ready();var c=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>c&&c.IsReady,cancellationToken:token);await Ready();}
 async Cysharp.Threading.Tasks.UniTask Home(){manager.SetButtonPressed(true);await Cysharp.Threading.Tasks.UniTask.Delay(5200,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);manager.SetButtonPressed(false);await Ready();}
 async Cysharp.Threading.Tasks.UniTask Shot(string name){Canvas.ForceUpdateCanvases();await Cysharp.Threading.Tasks.UniTask.WaitForEndOfFrame(hub,token);var shot=ScreenCapture.CaptureScreenshotAsTexture();System.IO.File.WriteAllBytes("docs/validation/ui_attention/"+name+".png",shot.EncodeToPNG());UnityEngine.Object.Destroy(shot);}
 Exhibition.UI.BlockExperienceView View()=>UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.BlockExperienceView>();
 RectTransform Frame(string path)=>(RectTransform)View().transform.Find(path);
 RectTransform Label(string path)=>(RectTransform)Frame(path).Find("SlotLabelCover/SlotLabel");
 RectTransform Art(string path)=>(RectTransform)Frame(path).Find("BlockContent");
 bool Bouncing(string path){var l=Label(path);return l.gameObject.activeInHierarchy&&Mathf.Abs(l.localScale.x-1)>.01f;}
 bool Shaking(string path)=>Mathf.Abs(Art(path).anchoredPosition.x)>.5f;
 // Records each rest-to-moving start while also proving no unrelated slot moves and input stays unlocked.
 async Cysharp.Threading.Tasks.UniTask<System.Collections.Generic.List<double>> Watch(float seconds,Func<string,bool> motion,string[] expected,string[] still,string code,string shotName=null){
  var starts=new System.Collections.Generic.List<double>();bool was=false,shot=false;double begin=Time.realtimeSinceStartupAsDouble;
  while(Time.realtimeSinceStartupAsDouble-begin<seconds){
   bool now=false;foreach(var path in expected)now|=motion(path);
   // A cue oscillates through its rest value, so restarts within a second belong to the same cue.
   double elapsed=Time.realtimeSinceStartupAsDouble-begin;if(now&&!was&&(starts.Count==0||elapsed-starts[starts.Count-1]>1))starts.Add(Math.Round(elapsed,3));was=now;
   foreach(var path in still)Check0(!Bouncing(path)&&!Shaking(path),"Unrelated slot moved: "+path+" "+code);
   foreach(var path in expected)Check0(Mathf.Abs(Art(path).anchoredPosition.y)<.01f,"Shake stays horizontal: "+path+" "+code);
   // The continue hint's own fade briefly locks input right after a block change; the cues must not.
   Check0(Time.realtimeSinceStartupAsDouble-begin<.5||!manager.IsPresentationLocked,"Cue locked input: "+code);
   if(shotName!=null&&now&&!shot&&starts.Count==1&&Time.realtimeSinceStartupAsDouble-begin-starts[0]>.06){shot=true;await Shot(shotName);}
   await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);
  }
  return starts;
 }
 void Check0(bool ok,string label){if(!ok)throw new Exception(label);}
 void Spaced(System.Collections.Generic.List<double> s,int count,string label){Check(s.Count==count,label+": "+count+" starts, got "+s.Count);for(int i=1;i<s.Count;i++)Check(Math.Abs(s[i]-s[i-1]-3)<.3,label+": 3s spacing, got "+(s[i]-s[i-1]));}
 var G=Exhibition.Blocks.BlockKind.Group;var P=Exhibition.Blocks.BlockKind.Product;var C=Exhibition.Blocks.BlockKind.Color;
 const string GF="GroupSelection/GroupFrame0",S0="ProductAndColors/SelectionFrame0",S1="ProductAndColors/SelectionFrame1",S2="ProductAndColors/SelectionFrame2",S3="ProductAndColors/SelectionFrame3";
 var none=new string[0];
 try{
  Application.runInBackground=true;if(adapter)adapter.enabled=false;await Ready();
  foreach(var language in new[]{Core.SessionLanguage.Thai,Core.SessionLanguage.English}){
   await Language(language);string code=language==Core.SessionLanguage.Thai?"th":"en";
   await Enter();var view=View();
   // Empty group slot: nothing before 3s, then a label-only bounce that repeats.
   Check(Label(GF).GetComponent<UnityEngine.UI.Image>().sprite.name=="GroupSlotLabel_"+code,"Label follows the language: "+code);
   await Shot("rest_group_"+code);
   var frameScale=Frame(GF).localScale;
   var s=await Watch(10f,Bouncing,new[]{GF},none,code,"bounce_group_"+code);timing["group_empty_"+code]=new Newtonsoft.Json.Linq.JArray(s);
   // Watch starts after the rest screenshot, which itself takes a few tenths of a second.
   Check(s.Count>=3&&s[0]>2.4&&s[0]<3.3,"First bounce waits about 3s: "+code);Spaced(s,s.Count,"Empty group bounce "+code);
   await Cysharp.Threading.Tasks.UniTask.Delay(700,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);
   Check(Frame(GF).localScale==frameScale,"Frame stays still during the bounce: "+code);
   // A placed group hides the label and stays quiet.
   hub.RecognizeReader(3,G,"student");await Ready();
   Check(!Label(GF).gameObject.activeInHierarchy&&Label(GF).localScale==Vector3.one,"Placed block hides the label at rest scale: "+code);
   s=await Watch(3.6f,Shaking,new[]{GF},none,code);Check(s.Count==0,"One group never shakes: "+code);
   // The student+senior example.
   hub.RecognizeReader(1,G,"senior");
   s=await Watch(6.8f,Shaking,new[]{GF},none,code,"shake_group_"+code);timing["group_extra_"+code]=new Newtonsoft.Json.Linq.JArray(s);
   // "At once" here means right after the hint fade that the same block change starts.
   Check(s.Count==3&&s[0]<.5,"Extra group shakes at once: "+code);Spaced(s,3,"Extra group shake "+code);
   Check(hub.Readers.DisplayBlock(0).Value.Id=="student"&&!view.AreRequiredBlocksRecognized,"The existing block is the one shown and continue stays blocked: "+code);
   hub.RemoveReader(1);await Frames(45);
   Check(Art(GF).anchoredPosition==Vector2.zero,"Shake settles at rest: "+code);
   s=await Watch(3.6f,Shaking,new[]{GF},none,code);Check(s.Count==0,"Resolving the extra stops the shake: "+code);
   // Overfill, then advance as soon as input returns: the moving frame must start clean.
   hub.RecognizeReader(1,G,"senior");await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>Shaking(GF),cancellationToken:token);hub.RemoveReader(1);await Ready();
   manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);
   await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>view.IsChoosingProduct,cancellationToken:token);
   Check(Art("BlockSlotTransfer/GroupFrame0").anchoredPosition==Vector2.zero,"Transfer stops the shake: "+code);
   await Ready();
   Check(!Label(S0).gameObject.activeInHierarchy&&Label(S1).gameObject.activeInHierarchy,"Selection labels match occupancy: "+code);
   // Selection page: one random empty slot bounces per cue, never the placed group.
   await Shot("rest_selection_"+code);
   var empties=new[]{S1,S2,S3};var picks=new System.Collections.Generic.HashSet<string>();int cues=0;double last=-9;bool shotDone=false;double pickBegin=Time.realtimeSinceStartupAsDouble;
   while(Time.realtimeSinceStartupAsDouble-pickBegin<15.8){
    int moving=0;string which=null;foreach(var path in empties)if(Bouncing(path)){moving++;which=path;}
    Check0(moving<=1,"Only one label bounces per cue: "+code);Check0(!Bouncing(S0),"Placed group label stays hidden: "+code);
    double now=Time.realtimeSinceStartupAsDouble-pickBegin;
    if(which!=null&&now-last>1){cues++;last=now;picks.Add(which);}
    if(which!=null&&!shotDone&&now-last>.06){shotDone=true;await Shot("bounce_selection_"+code);}
    await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);
   }
   report["picks_"+code]=new Newtonsoft.Json.Linq.JArray(new System.Collections.Generic.List<string>(picks));
   Check(cues>=5,"Random cue repeats every 3s: "+code+" got "+cues);Check(picks.Count>=2,"The cue moves between empty slots: "+code+" got "+picks.Count);
   await Cysharp.Threading.Tasks.UniTask.Delay(700,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);
   hub.RecognizeReader(0,P,"bags");await Ready();
   s=await Watch(2.6f,Bouncing,new[]{S2,S3},new[]{S0,S1},code);Check(s.Count==0,"A placed block restarts the wait: "+code);
   s=await Watch(1.2f,Bouncing,new[]{S2,S3},new[]{S0,S1},code);Check(s.Count==1,"Bounce follows a fresh 3s: "+code);
   // Extra product: only the existing product shakes. Colors keep bouncing on their own timer.
   await Cysharp.Threading.Tasks.UniTask.Delay(700,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);
   hub.RecognizeReader(1,P,"shoes");
   s=await Watch(3.5f,Shaking,new[]{S1},new[]{S0},code,"shake_product_"+code);Check(s.Count==2&&s[0]<.2,"Extra product shakes the existing one: "+code);
   Check(hub.Readers.DisplayBlock(1).Value.Id=="bags","The existing product stays displayed: "+code);
   hub.RemoveReader(1);await Frames(45);
   // Two colors are required, so only a third is an excess. Both existing colors shake.
   hub.RecognizeReader(1,C,"red");await Ready();hub.RecognizeReader(2,C,"blue");await Ready();
   Check(view.AreRequiredBlocksRecognized,"Complete composition is ready: "+code);
   s=await Watch(3.6f,m=>Bouncing(m)||Shaking(m),new[]{S0,S1,S2,S3},none,code);Check(s.Count==0,"Complete composition is quiet: "+code);
   var groupHolder=hub.Readers[3].HasValue?3:-1;Check(groupHolder==3,"Group is on reader 3: "+code);
   hub.RemoveReader(0);await Ready();hub.RecognizeReader(0,C,"green");
   bool both=false;double begin=Time.realtimeSinceStartupAsDouble;while(Time.realtimeSinceStartupAsDouble-begin<.3){both|=Shaking(S2)&&Shaking(S3);await Frames(1);}
   Check(both,"Third color shakes both existing colors: "+code);await Shot("shake_colors_"+code);
   // Removing the group brings its label back at the selection size, and it bounces.
   hub.RemoveReader(0);await Ready();hub.RemoveReader(3);await Ready();hub.RecognizeReader(0,P,"bags");await Ready();
   s=await Watch(3.6f,Bouncing,new[]{S0},new[]{S1,S2,S3},code);Check(s.Count==1&&Label(S0).rect.width>251&&Label(S0).rect.width<253,"Removed group's label returns at 252 and bounces: "+code);
   // Disabling the view mid-cue leaves nothing displaced.
   hub.RecognizeReader(0,G,"child");hub.RecognizeReader(3,G,"senior");
   // The new group's placement pop holds the cue until that animation releases input.
   double held=Time.realtimeSinceStartupAsDouble;while(!Shaking(S0)&&Time.realtimeSinceStartupAsDouble-held<1)await Frames(1);
   Check(Shaking(S0),"Group extra shakes on the selection page: "+code);view.enabled=false;await Frames(2);
   Check(Art(S0).anchoredPosition==Vector2.zero,"Disable restores the block position: "+code);view.enabled=true;await Frames();
   await Home();Check(manager.IsTitleScreen&&hub.Readers.OccupiedCount==0,"Home clears after the cues: "+code);
  }
  await Language(initial);report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{if(manager)manager.ResetButtonInput();if(adapter)adapter.enabled=wasEnabled;Application.runInBackground=background;System.IO.File.WriteAllText("docs/validation/ui_attention/runtime.json",report.ToString());UnityEngine.Object.Destroy(root);}
});
return "Slot attention verification started.";
