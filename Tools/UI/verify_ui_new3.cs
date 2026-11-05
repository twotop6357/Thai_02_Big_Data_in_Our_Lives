// Explicit asset-only UI check. Uses a prior generated PNG for Result; no new AI request.
if(GameObject.Find("UINew3Verification"))return "Already running.";
var root=new GameObject("UINew3Verification");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="UI-NEW-003"};var checks=new Newtonsoft.Json.Linq.JArray();report["checks"]=checks;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var hub=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidReaderHub>();var adapter=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
 bool wasEnabled=adapter&&adapter.enabled,background=Application.runInBackground;var initial=manager.CurrentLanguage;var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
 using var deadline=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(100));var token=deadline.Token;
 void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
 async Cysharp.Threading.Tasks.UniTask Frames(int n=5){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Ready(){await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsInputReady&&!manager.IsTransitioning&&!manager.IsPresentationLocked&&!manager.IsLanguageChanging&&hub.IsArtworkReady,cancellationToken:token);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Tap(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Frames();await Ready();}
 async Cysharp.Threading.Tasks.UniTask Language(Core.SessionLanguage language){await Ready();if(manager.CurrentLanguage==language)return;manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Cysharp.Threading.Tasks.UniTask.Delay(2300,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);manager.SetButtonPressed(false);await Frames();await Ready();Check(manager.CurrentLanguage==language,"Title language switch: "+language);}
 void Help(string id,string expected){var set=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.LocalizedImageSet>();var bindings=(Exhibition.UI.LocalizedImageSet.Binding[])set.GetType().GetField("bindings",flags).GetValue(set);var image=System.Array.Find(bindings,b=>b.id==id).image;Check(image&&image.sprite&&image.sprite.name==expected&&image.gameObject.activeInHierarchy&&image.color==Color.white,"Correct visible help: "+id+" / "+expected);}
 async Cysharp.Threading.Tasks.UniTask Shot(string name){ScreenCapture.CaptureScreenshot("docs/validation/ui_new3/"+name+".png");await Frames(6);}
 try{
  Application.runInBackground=true;if(adapter)adapter.enabled=false;await Ready();
  foreach(var language in new[]{Core.SessionLanguage.Thai,Core.SessionLanguage.English}){
   await Language(language);string code=language==Core.SessionLanguage.Thai?"th":"en";
   Help("idle.help","ButtonHelpBox_"+code);var backgroundImage=System.Linq.Enumerable.FirstOrDefault(UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Image>(FindObjectsSortMode.None),i=>i.sprite&&i.sprite.name=="Intro_bg");Check(backgroundImage&&backgroundImage.color==Color.white&&backgroundImage.sprite.texture.width==1920&&backgroundImage.sprite.texture.height==1080,"Updated full-size Intro_bg is displayed: "+code);await Shot("idle_"+code);
   await Tap();var controller=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>controller&&controller.IsReady,cancellationToken:token);
   hub.RecognizeReader(3,Exhibition.Blocks.BlockKind.Group,"student");await Ready();Help("group.help",language==Core.SessionLanguage.Thai?"ButtonHelpBox_groupselect_th":"ButtonHelpBox_groupselectPanel_en");await Shot("group_"+code);
   await Tap();hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Product,"shoes");hub.RecognizeReader(1,Exhibition.Blocks.BlockKind.Color,"red");hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Color,"blue");await Ready();Check(!controller.IsGenerating,"Resource check does not generate an AI image: "+code);Help("selection.help",language==Core.SessionLanguage.Thai?"ButtonHelpBox_selectall_th":"ButtonHelpBox_selectallPanel_en");await Shot("selection_"+code);
   var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);if(!ImageConversion.LoadImage(texture,System.IO.File.ReadAllBytes("docs/validation/comfy_style3/runtime_0_shoes.png"))||!controller.TryDisplayResult(texture,controller.Selection.Revision)){UnityEngine.Object.Destroy(texture);throw new Exception("Could not attach the prior PNG for UI verification.");}
   await Ready();await Tap();Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="2_Result","Normal result transition preserved: "+code);Help("result.help",language==Core.SessionLanguage.Thai?"ButtonHelpBox_recommendedAd_th":"ButtonHelpBox_RecommendedAd_en");await Shot("result_"+code);
   manager.ReturnToIdleManually();await Ready();Check(manager.IsTitleScreen,"Return to Idle preserved: "+code);
  }
  await Language(initial);report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{if(manager)manager.ResetButtonInput();if(adapter)adapter.enabled=wasEnabled;Application.runInBackground=background;System.IO.File.WriteAllText("docs/validation/ui_new3/runtime.json",report.ToString());UnityEngine.Object.Destroy(root);}
});
return "Nine replaced UI assets are being verified in both languages.";
