// Real ComfyUI requests through the actual application, with both panels and Result transfer.
if(GameObject.Find("StyleSuccessVerification"))return "Already running.";
var root=new GameObject("StyleSuccessVerification");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var checks=new Newtonsoft.Json.Linq.JArray();var cases=new Newtonsoft.Json.Linq.JArray();
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="STYLE-QUALITY-003 actual app",["checks"]=checks,["cases"]=cases};
 using var limit=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(150));var token=limit.Token;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var timer=UnityEngine.Object.FindFirstObjectByType<HuliacDev.Core.InactivityTimer>();
 var keyboard=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();bool oldKeyboard=keyboard&&keyboard.enabled,oldBackground=Application.runInBackground;
 if(keyboard)keyboard.enabled=false;Application.runInBackground=true;Exhibition.Blocks.BlockExperienceController blocks=null;
 void Save()=>System.IO.File.WriteAllText("docs/validation/comfy_style3/runtime_success.json",report.ToString());
 void Check(bool v,string text){if(!v)throw new Exception(text);checks.Add(text);Save();}
 async Cysharp.Threading.Tasks.UniTask Wait(Func<bool> p)=>await Cysharp.Threading.Tasks.UniTask.WaitUntil(p,cancellationToken:token);
 async Cysharp.Threading.Tasks.UniTask Frame()=>await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);
 async Cysharp.Threading.Tasks.UniTask Ready()=>await Wait(()=>!manager.IsTransitioning&&!manager.IsPresentationLocked);
 async Cysharp.Threading.Tasks.UniTask Tap(){manager.SetButtonPressed(false);await Frame();manager.SetButtonPressed(true);await Frame();manager.SetButtonPressed(false);await Frame();}
 try{
  await Wait(()=>manager&&manager.IsInputReady);await Ready();await Tap();await Ready();
  blocks=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Wait(()=>blocks&&blocks.IsReady);
  var choices=new[]{new[]{"student","shoes","blue","white"},new[]{"child","hats","yellow","black"},new[]{"working_adult","bags","green","white"},new[]{"senior","wristwatches","red","green"}};
  for(int i=0;i<choices.Length;i++){
   var c=choices[i];blocks.RemoveBlock(Exhibition.Blocks.BlockSlot.Group);
   blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product,c[1]);blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1,c[2]);blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2,c[3]);
   double started=Time.realtimeSinceStartupAsDouble,completed=-1;
   Action onChanged=()=>{if(!blocks.IsGenerating&&blocks.HasGeneratedImage&&completed<0)completed=Time.realtimeSinceStartupAsDouble;};blocks.PresentationChanged+=onChanged;
   blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group,c[0]);
   Check(blocks.IsGenerating&&manager.IsPresentationLocked&&timer.IsPaused,"Actual request locks input/pause: "+c[1]);
   var view=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.BlockExperienceView>();bool wasChoosing=view.IsChoosingProduct;
   view.SendMessage("Continue",SendMessageOptions.RequireReceiver);await Tap();
   if(i==0){manager.SetButtonPressed(true);await Cysharp.Threading.Tasks.UniTask.Delay(5300,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);manager.SetButtonPressed(false);await Frame();}
   await Wait(()=>!blocks.IsGenerating);await Ready();blocks.PresentationChanged-=onChanged;
   Check(!manager.IsTitleScreen&&view.IsChoosingProduct==wasChoosing,"Both panel navigation and held input spanning completion are ignored: "+c[1]);
   Check(blocks.HasGeneratedImage&&manager.ResultTexture&&blocks.Status=="Image ready.","Actual current reference graph returns a valid image: "+c[1]);
   Check(!manager.IsPresentationLocked&&!timer.IsPaused,"Success restores input/timer: "+c[1]);
   var source=manager.ResultTexture;var previous=RenderTexture.active;var target=RenderTexture.GetTemporary(480,480,0,RenderTextureFormat.ARGB32);var export=new Texture2D(480,480,TextureFormat.RGBA32,false);
   try{
    Graphics.Blit(source,target);RenderTexture.active=target;export.ReadPixels(new Rect(0,0,480,480),0,0);export.Apply();var pixels=export.GetPixels32();int l=480,r=-1,b=480,t=-1;
    for(int y=0;y<480;y++)for(int x=0;x<480;x++)if(pixels[y*480+x].a>=8){l=Math.Min(l,x);r=Math.Max(r,x);b=Math.Min(b,y);t=Math.Max(t,y);}
    Check(l>=24&&b>=24&&r<456&&t<456&&Math.Abs((l+r+1)*.5-240)<=1&&Math.Abs((b+t+1)*.5-240)<=1,"Actual texture preserves alpha, center and safe margins: "+c[1]);
    Check(pixels[0].a==0&&pixels[479].a==0&&pixels[479*480].a==0&&pixels[480*480-1].a==0,"Actual texture has transparent corners: "+c[1]);
    string file="runtime_"+i+"_"+c[1]+".png";System.IO.File.WriteAllBytes("docs/validation/comfy_style3/"+file,export.EncodeToPNG());
    cases.Add(new Newtonsoft.Json.Linq.JObject{["age"]=c[0],["product"]=c[1],["first"]=c[2],["second"]=c[3],["seconds"]=completed>0?completed-started:Time.realtimeSinceStartupAsDouble-started,["image"]=file});Save();
   }finally{RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);UnityEngine.Object.Destroy(export);}
   if(i==0){await Tap();await Ready();Check(view.IsChoosingProduct,"Fresh input advances from group to product panel after completion");}
  }
  var shared=manager.ResultTexture;await Tap();await Ready();
  Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="2_Result"&&manager.ResultTexture==shared,"Result receives the same validated texture");
  manager.ReturnToIdleManually();await Ready();Check(manager.IsTitleScreen&&!manager.ResultTexture&&!timer.IsPaused,"Home releases the result and restores inactivity");
  report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{if(blocks)blocks.CancelGeneration();if(manager&&!manager.IsTitleScreen)manager.ReturnToIdleManually();if(keyboard)keyboard.enabled=oldKeyboard;Application.runInBackground=oldBackground;Save();UnityEngine.Object.Destroy(root);}
});return "Real application reference-quality verification started.";
