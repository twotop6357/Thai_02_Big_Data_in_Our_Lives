// UI-WEB-007: 0_Idle에서 Play 중 Unity execute_code로 실행한다.
// 태국어 그룹·선택·결과·영상 종료 화면에서 새 문구 6개가 새 스프라이트와 Lang_Th.json 좌표로 표시되는지 확인하고 화면을 저장한다.
// AI 생성 요청은 하지 않고 이전 검증 PNG를 결과로 붙인다. 결과: docs/validation/ui_web7/runtime.json
if(GameObject.Find("Web7Verify"))return "Already running.";
var root=new GameObject("Web7Verify");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 const string Out="docs/validation/ui_web7/";
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="UI-WEB-007 runtime"};var checks=new Newtonsoft.Json.Linq.JArray();report["checks"]=checks;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var hub=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidReaderHub>();var adapter=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
 bool wasEnabled=adapter&&adapter.enabled,background=Application.runInBackground;var initial=manager.CurrentLanguage;var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
 using var deadline=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(240));var token=deadline.Token;
 var doc=JsonUtility.FromJson<Exhibition.UI.ImageLanguageDocument>(System.IO.File.ReadAllText("Assets/StreamingAssets/Lang_Th.json"));
 void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
 async Cysharp.Threading.Tasks.UniTask Frames(int n=5){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Ready(){await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsInputReady&&!manager.IsTransitioning&&!manager.IsPresentationLocked&&!manager.IsLanguageChanging&&hub.IsArtworkReady,cancellationToken:token);await Frames(20);}
 async Cysharp.Threading.Tasks.UniTask Tap(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Frames();await Ready();}
 async Cysharp.Threading.Tasks.UniTask Toggle(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Cysharp.Threading.Tasks.UniTask.Delay(2300,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);manager.SetButtonPressed(false);await Frames();await Ready();}
 async Cysharp.Threading.Tasks.UniTask Shot(string name){Canvas.ForceUpdateCanvases();await Cysharp.Threading.Tasks.UniTask.WaitForEndOfFrame(hub,token);var shot=ScreenCapture.CaptureScreenshotAsTexture();System.IO.File.WriteAllBytes(Out+name+".png",shot.EncodeToPNG());UnityEngine.Object.Destroy(shot);await Frames();}
 // 화면에 켜진 이미지 중 해당 id의 스프라이트를 찾아 크기·좌표가 Lang_Th.json과 같은지 본다.
 void Placed(string id){
  var e=System.Array.Find(doc.images,x=>x.id==id);UnityEngine.UI.Image found=null;
  foreach(var im in UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Image>(FindObjectsSortMode.None))if(im.isActiveAndEnabled&&im.sprite&&im.sprite.name==e.sprite)found=im;
  Check(found,"Shown: "+id);var rt=found.rectTransform;
  Check(Mathf.Approximately(found.sprite.rect.width,e.width)&&Mathf.Approximately(found.sprite.rect.height,e.height),"Sprite size matches web export: "+id+" "+found.sprite.rect.size);
  Check(rt.pivot==new Vector2(0,1)&&Vector2.Distance(rt.anchoredPosition,new Vector2(e.x,-e.y))<.5f&&Vector2.Distance(rt.sizeDelta,new Vector2(e.width,e.height))<.5f,"Rect matches Lang_Th: "+id+" pos="+rt.anchoredPosition+" size="+rt.sizeDelta);}
 try{
  Application.runInBackground=true;if(adapter)adapter.enabled=false;await Ready();
  if(manager.CurrentLanguage!=Core.SessionLanguage.Thai)await Toggle();Check(manager.CurrentLanguage==Core.SessionLanguage.Thai,"Thai");
  await Tap();var controller=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>controller&&controller.IsReady,cancellationToken:token);await Ready();
  Placed("group.title");Placed("group.subtitle");await Shot("group_empty_th");
  hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Group,"student");await Ready();await Tap();
  hub.RecognizeReader(3,Exhibition.Blocks.BlockKind.Color,"red");hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Product,"shoes");hub.RecognizeReader(1,Exhibition.Blocks.BlockKind.Color,"blue");await Ready();
  await Cysharp.Threading.Tasks.UniTask.Delay(1000,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);
  Placed("selection.title");Placed("selection.subtitle");await Shot("selection_full_th");
  var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);if(!ImageConversion.LoadImage(texture,System.IO.File.ReadAllBytes("docs/validation/comfy_style3/runtime_0_shoes.png"))||!controller.TryDisplayResult(texture,controller.Selection.Revision)){UnityEngine.Object.Destroy(texture);throw new Exception("Could not attach the prior PNG.");}
  await Ready();await Tap();Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="2_Result","Result");
  await Cysharp.Threading.Tasks.UniTask.Delay(4500,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);await Ready();
  Placed("result.title");Placed("result.subtitle");await Shot("result_th");
  var video=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.ResultVideoController>();await Tap();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>video.State==Exhibition.UI.ResultVideoController.ScreenState.Playing,cancellationToken:token);
  var player=(UnityEngine.Video.VideoPlayer)typeof(Exhibition.UI.ResultVideoController).GetField("player",flags).GetValue(video);player.time=Math.Max(0,player.length-.25);player.Play();
  await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>video.State==Exhibition.UI.ResultVideoController.ScreenState.Finished,cancellationToken:token);await Ready();await Cysharp.Threading.Tasks.UniTask.Delay(800,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);
  Placed("video.end");await Shot("video_end_th");
  manager.ReturnToIdleManually();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsTitleScreen,cancellationToken:token);for(int i=0;i<4;i++)hub.RemoveReader(i);await Ready();
  if(manager.CurrentLanguage!=initial)await Toggle();
  report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{if(manager)manager.ResetButtonInput();if(adapter)adapter.enabled=wasEnabled;Application.runInBackground=background;System.IO.File.WriteAllText(Out+"runtime.json",report.ToString());UnityEngine.Object.Destroy(root);}
});
return "UI-WEB-007 runtime verification started.";
