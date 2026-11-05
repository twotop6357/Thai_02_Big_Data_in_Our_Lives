// Run in Play mode from 0_Idle. Uses a prior generated PNG; no new AI request. The failed-video case injects a missing resource.
if(GameObject.Find("AdHintVerification"))return "Already running.";
var root=new GameObject("AdHintVerification");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="UI-ADHINT-001"};var checks=new Newtonsoft.Json.Linq.JArray();report["checks"]=checks;var timing=new Newtonsoft.Json.Linq.JObject();report["timing"]=timing;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var hub=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidReaderHub>();var adapter=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
 bool wasEnabled=adapter&&adapter.enabled,background=Application.runInBackground;var initial=manager.CurrentLanguage;var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
 var G=Exhibition.Blocks.BlockKind.Group;var P=Exhibition.Blocks.BlockKind.Product;var C=Exhibition.Blocks.BlockKind.Color;var S=typeof(Exhibition.UI.ResultVideoController);
 using var deadline=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(200));var token=deadline.Token;
 void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
 async Cysharp.Threading.Tasks.UniTask Frames(int n=5){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Ready(){await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsInputReady&&!manager.IsTransitioning&&!manager.IsPresentationLocked&&!manager.IsLanguageChanging&&hub.IsArtworkReady,cancellationToken:token);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Press(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);}
 // A lock release waits for the button to be seen up, so a hold must start from released.
 async Cysharp.Threading.Tasks.UniTask Home(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Cysharp.Threading.Tasks.UniTask.Delay(5200,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);manager.SetButtonPressed(false);await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsTitleScreen,cancellationToken:token);await Ready();}
 async Cysharp.Threading.Tasks.UniTask Language(Core.SessionLanguage language){await Ready();if(manager.CurrentLanguage==language)return;manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Cysharp.Threading.Tasks.UniTask.Delay(2300,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);manager.SetButtonPressed(false);await Frames();await Ready();Check(manager.CurrentLanguage==language,"Title language switch: "+language);}
 async Cysharp.Threading.Tasks.UniTask Shot(string name){Canvas.ForceUpdateCanvases();await Cysharp.Threading.Tasks.UniTask.WaitForEndOfFrame(hub,token);var shot=ScreenCapture.CaptureScreenshotAsTexture();System.IO.File.WriteAllBytes("docs/validation/ad_hint/"+name+".png",shot.EncodeToPNG());UnityEngine.Object.Destroy(shot);}
 // Drives to the Result scene and returns once the entrance effects have started.
 async Cysharp.Threading.Tasks.UniTask<Exhibition.UI.ResultVideoController> ToAdvertisement(){
  await Press();await Ready();var c=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>c&&c.IsReady,cancellationToken:token);await Ready();
  hub.RecognizeReader(3,G,"student");await Ready();await Press();await Frames();await Ready();
  hub.RecognizeReader(2,P,"shoes");hub.RecognizeReader(0,C,"red");hub.RecognizeReader(1,C,"blue");await Ready();
  var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);if(!ImageConversion.LoadImage(texture,System.IO.File.ReadAllBytes("docs/validation/comfy_style3/runtime_0_shoes.png"))||!c.TryDisplayResult(texture,c.Selection.Revision)){UnityEngine.Object.Destroy(texture);throw new Exception("Could not attach the prior PNG.");}
  await Ready();await Press();
  await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="2_Result"&&UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.AdProductHighlight>()&&UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.AdProductHighlight>().IsPlaying,cancellationToken:token);
  return UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.ResultVideoController>();
 }
 Exhibition.UI.UIAnimatedElement Hint(Exhibition.UI.ResultVideoController v)=>(Exhibition.UI.UIAnimatedElement)S.GetField("advertisementHint",flags).GetValue(v);
 float Alpha(Exhibition.UI.UIAnimatedElement h)=>h.gameObject.activeInHierarchy?h.GetComponent<CanvasGroup>().alpha:0;
 bool Confetti(){var h=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.AdProductHighlight>();var r=(GameObject)typeof(Exhibition.UI.AdProductHighlight).GetField("confettiRoot",flags).GetValue(h);return r&&r.activeSelf;}
 try{
  Application.runInBackground=true;if(adapter)adapter.enabled=false;await Ready();
  foreach(var language in new[]{Core.SessionLanguage.Thai,Core.SessionLanguage.English}){
   await Language(language);string code=language==Core.SessionLanguage.Thai?"th":"en";
   var video=await ToAdvertisement();var hint=Hint(video);var highlight=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.AdProductHighlight>();
   Check(hint,"Hint reference is assigned: "+code);
   // Hidden and locked for three seconds; pressing early does nothing.
   double start=Time.realtimeSinceStartupAsDouble;bool shot=false;
   while(manager.IsPresentationLocked||manager.IsTransitioning){
    if(Alpha(hint)!=0)throw new Exception("Hint showed while input was locked: "+code);
    if(!shot&&Time.realtimeSinceStartupAsDouble-start>1.5){shot=true;await Shot("before_input_"+code);}
    await Frames(1);
   }
   double released=Time.realtimeSinceStartupAsDouble-start;timing["input_released_"+code]=Math.Round(released,3);
   Check(released>2.8&&released<3.3,"Input opens about 3s after the effects start: "+code+" got "+released);
   Check(Confetti(),"Confetti is still playing when input opens: "+code);
   Check(!highlight.IsPlaying&&video.State==Exhibition.UI.ResultVideoController.ScreenState.Advertisement,"Advertisement waits for the button: "+code);
   // Real fade, no input lock.
   float last=-1;bool rising=true,partial=false;double fadeStart=Time.realtimeSinceStartupAsDouble;
   while(Alpha(hint)<.999f&&Time.realtimeSinceStartupAsDouble-fadeStart<1){float a=Alpha(hint);if(a<last-.001f)rising=false;if(a>.05f&&a<.95f)partial=true;last=a;if(manager.IsPresentationLocked)throw new Exception("Hint fade locked input: "+code);await Frames(1);}
   timing["hint_fade_"+code]=Math.Round(Time.realtimeSinceStartupAsDouble-fadeStart,3);
   Check(Alpha(hint)>.999f&&rising&&partial,"Hint fades in: "+code);await Frames(2);await Shot("hint_during_confetti_"+code);
   Check(Confetti(),"Confetti continues under the hint: "+code);
   // Natural confetti end leaves the hint alone.
   await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>!Confetti(),cancellationToken:token);timing["confetti_seconds_"+code]=Math.Round(Time.realtimeSinceStartupAsDouble-start,3);
   Check(Alpha(hint)>.999f&&!manager.IsPresentationLocked,"Confetti end keeps the hint and input: "+code);
   // Failed video returns to the advertisement with the hint available again.
   var set=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.LocalizedImageSet>();var doc=(Exhibition.UI.ImageLanguageDocument)typeof(Exhibition.UI.LocalizedImageSet).GetField(code=="th"?"_thai":"_english",flags).GetValue(set);string original=doc.video;
   doc.video="Videos/__missing_for_test";await Press();
   await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>video.State!=Exhibition.UI.ResultVideoController.ScreenState.Advertisement,cancellationToken:token);await Frames(20);Check(Alpha(hint)<.999f,"Pressing starts hiding the hint: "+code);
   await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>video.State==Exhibition.UI.ResultVideoController.ScreenState.Failed,cancellationToken:token);await Ready();doc.video=original;
   await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>Alpha(hint)>.999f,cancellationToken:token);Check(true,"Failed video shows the hint again when input returns: "+code);
   // Retry works and the hint leaves with the advertisement.
   await Press();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>video.State==Exhibition.UI.ResultVideoController.ScreenState.Playing,cancellationToken:token);Check(Alpha(hint)==0,"Hint is gone during the video: "+code);
   await Home();
   // Pressing during confetti goes straight to the video.
   video=await ToAdvertisement();hint=Hint(video);
   await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>!manager.IsPresentationLocked&&!manager.IsTransitioning,cancellationToken:token);Check(Confetti(),"Confetti still playing before the early press: "+code);
   manager.SetButtonPressed(false);await Frames(2);manager.SetButtonPressed(true);await Frames(3);manager.SetButtonPressed(false);
   await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>video.State==Exhibition.UI.ResultVideoController.ScreenState.Playing,cancellationToken:token);
   Check(!Confetti()&&Alpha(hint)==0,"Pressing during confetti clears it and reaches the video: "+code);
   Check((long)S.GetField("_firstFrame",flags).GetValue(video)==0,"Video frame zero preserved: "+code);
   await Home();
   // Leaving during the locked entrance leaves no lock behind: five-second hold is ignored while locked, so disable instead.
   video=await ToAdvertisement();highlight=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.AdProductHighlight>();await Frames(10);
   highlight.enabled=false;await Frames(3);Check(!highlight.IsPlaying&&!Confetti(),"Disabling mid-entrance clears the effects: "+code);
   await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>!manager.IsPresentationLocked&&!manager.IsTransitioning,cancellationToken:token);Check(true,"Disabling mid-entrance returns input: "+code);
   highlight.enabled=true;await Frames();await Home();Check(manager.IsTitleScreen&&hub.Readers.OccupiedCount==0,"Home clears: "+code);
  }
  await Language(initial);report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{if(manager)manager.ResetButtonInput();if(adapter)adapter.enabled=wasEnabled;Application.runInBackground=background;System.IO.File.WriteAllText("docs/validation/ad_hint/runtime.json",report.ToString());UnityEngine.Object.Destroy(root);}
});
return "Advertisement hint verification started.";
