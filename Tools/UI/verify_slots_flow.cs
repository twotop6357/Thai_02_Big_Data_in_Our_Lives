// Run in Play mode from 0_Idle. Full exhibit flow on the final assets. Uses a prior generated PNG; no new AI request.
if(GameObject.Find("UISlotsFlow"))return "Already running.";
var root=new GameObject("UISlotsFlow");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="UI-SLOTS-002 flow"};var checks=new Newtonsoft.Json.Linq.JArray();report["checks"]=checks;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var hub=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidReaderHub>();var adapter=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
 bool wasEnabled=adapter&&adapter.enabled,background=Application.runInBackground;var initial=manager.CurrentLanguage;var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
 using var deadline=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(120));var token=deadline.Token;
 void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
 async Cysharp.Threading.Tasks.UniTask Frames(int n=5){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Ready(){await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsInputReady&&!manager.IsTransitioning&&!manager.IsPresentationLocked&&!manager.IsLanguageChanging&&hub.IsArtworkReady,cancellationToken:token);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Tap(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Frames();await Ready();}
 async Cysharp.Threading.Tasks.UniTask Language(Core.SessionLanguage language){await Ready();if(manager.CurrentLanguage==language)return;manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Cysharp.Threading.Tasks.UniTask.Delay(2300,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);manager.SetButtonPressed(false);await Frames();await Ready();Check(manager.CurrentLanguage==language,"Title language switch: "+language);}
 async Cysharp.Threading.Tasks.UniTask Shot(string name){Canvas.ForceUpdateCanvases();await Cysharp.Threading.Tasks.UniTask.WaitForEndOfFrame(hub,token);var shot=ScreenCapture.CaptureScreenshotAsTexture();System.IO.File.WriteAllBytes("docs/validation/ui_slots/"+name+".png",shot.EncodeToPNG());UnityEngine.Object.Destroy(shot);await Frames();}
 void Bindings(string code){var set=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.LocalizedImageSet>();var doc=JsonUtility.FromJson<Exhibition.UI.ImageLanguageDocument>(System.IO.File.ReadAllText("Assets/StreamingAssets/Lang_"+(code=="th"?"Th":"En")+".json"));foreach(var b in (Exhibition.UI.LocalizedImageSet.Binding[])set.GetType().GetField("bindings",flags).GetValue(set)){var e=System.Array.Find(doc.images,x=>x.id==b.id);Check(b.image&&b.image.sprite&&b.image.sprite.name==e.sprite,"Binding sprite: "+code+" / "+b.id);}}
 try{
  Application.runInBackground=true;if(adapter)adapter.enabled=false;await Ready();
  foreach(var language in new[]{Core.SessionLanguage.Thai,Core.SessionLanguage.English}){
   await Language(language);string code=language==Core.SessionLanguage.Thai?"th":"en";Bindings(code);await Shot("idle_"+code);
   await Tap();var controller=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>controller&&controller.IsReady,cancellationToken:token);await Ready();Bindings(code);
   var view=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.BlockExperienceView>();var bg=view.transform.Find("ProductAndColors/SelectAllPanel_bg") as RectTransform;
   Check(bg.anchorMin==Vector2.zero&&bg.anchorMax==Vector2.one&&bg.offsetMin==Vector2.zero&&bg.offsetMax==Vector2.zero,"Localized selection background still fills the panel: "+code);
   await Shot("group_empty_"+code);
   hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Group,"student");await Ready();await Shot("group_"+code);await Tap();Check(view.IsChoosingProduct,"Button moves to selection: "+code);await Shot("selection_group_only_"+code);
   hub.RecognizeReader(3,Exhibition.Blocks.BlockKind.Color,"red");hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Product,"shoes");hub.RecognizeReader(1,Exhibition.Blocks.BlockKind.Color,"blue");await Ready();await Shot("selection_full_"+code);
   Check(view.AreRequiredBlocksRecognized&&!controller.IsGenerating,"Composition ready without generating: "+code);
   var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);if(!ImageConversion.LoadImage(texture,System.IO.File.ReadAllBytes("docs/validation/comfy_style3/runtime_0_shoes.png"))||!controller.TryDisplayResult(texture,controller.Selection.Revision)){UnityEngine.Object.Destroy(texture);throw new Exception("Could not attach the prior PNG.");}
   await Ready();await Shot("selection_result_"+code);await Tap();Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="2_Result","Result transition preserved: "+code);await Shot("result_"+code);
   var video=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.ResultVideoController>();await Tap();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>video.State==Exhibition.UI.ResultVideoController.ScreenState.Playing,cancellationToken:token);Check((long)typeof(Exhibition.UI.ResultVideoController).GetField("_firstFrame",flags).GetValue(video)==0,"Video frame zero preserved: "+code);
   var player=(UnityEngine.Video.VideoPlayer)typeof(Exhibition.UI.ResultVideoController).GetField("player",flags).GetValue(video);player.time=Math.Max(0,player.length-.25);player.Play();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>video.State==Exhibition.UI.ResultVideoController.ScreenState.Finished,cancellationToken:token);await Ready();
   hub.ClearReaders();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsTitleScreen,cancellationToken:token);await Ready();Check(manager.IsTitleScreen&&hub.Readers.OccupiedCount==0,"Removing all blocks ends the session: "+code);
  }
  await Language(initial);report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{if(manager)manager.ResetButtonInput();if(adapter)adapter.enabled=wasEnabled;Application.runInBackground=background;System.IO.File.WriteAllText("docs/validation/ui_slots/flow.json",report.ToString());UnityEngine.Object.Destroy(root);}
});
return "Full flow verification started.";
