// Run in Play mode from 0_Idle with Unity execute_code. UI-IDLEPOP-001: title "remove all blocks" popup.
// Writes docs/validation/idle_popup/runtime.json and screenshots.
if(GameObject.Find("TitleBlockPopupVerify"))return "Already running.";
var root=new GameObject("TitleBlockPopupVerify");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 const string Out="docs/validation/idle_popup/";
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="UI-IDLEPOP-001 runtime"};var checks=new Newtonsoft.Json.Linq.JArray();report["checks"]=checks;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var hub=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidReaderHub>();var adapter=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
 bool wasEnabled=adapter&&adapter.enabled,background=Application.runInBackground;var initial=manager.CurrentLanguage;
 using var deadline=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(200));var token=deadline.Token;
 var timings=new Newtonsoft.Json.Linq.JObject();report["timings"]=timings;
 void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
 async Cysharp.Threading.Tasks.UniTask Frames(int n=5){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Ready(){await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsInputReady&&!manager.IsTransitioning&&!manager.IsPresentationLocked&&!manager.IsLanguageChanging&&hub.IsArtworkReady,cancellationToken:token);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Press(int ms){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Cysharp.Threading.Tasks.UniTask.Delay(ms,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);manager.SetButtonPressed(false);await Frames(10);}
 async Cysharp.Threading.Tasks.UniTask Shot(string name){Canvas.ForceUpdateCanvases();await Cysharp.Threading.Tasks.UniTask.WaitForEndOfFrame(hub,token);var shot=ScreenCapture.CaptureScreenshotAsTexture();System.IO.File.WriteAllBytes(Out+name+".png",shot.EncodeToPNG());UnityEngine.Object.Destroy(shot);await Frames();}
 Exhibition.UI.TitleBlockPopup Popup()=>UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.TitleBlockPopup>();
 GameObject Overlay()=>Popup().transform.Find("TitleBlockOverlay").gameObject;
 async Cysharp.Threading.Tasks.UniTask Shown(string label){var p=Popup();float t=Time.unscaledTime;await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>p.IsRequested,cancellationToken:token);float req=Time.unscaledTime-t;
  var content=Overlay().transform.Find("PopupContent").GetComponent<CanvasGroup>();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>Overlay().activeSelf&&content.alpha>.99f&&!UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.UIPopupPresentation>().IsAnimating,cancellationToken:token);
  timings[label]=Math.Round(Time.unscaledTime-t,3);Check(p.IsBlockingInput&&manager.IsPresentationLocked,"Popup shown and input blocked: "+label);}
 async Cysharp.Threading.Tasks.UniTask Hidden(string label){var p=Popup();float t=Time.unscaledTime;await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>!Overlay().activeSelf&&!p.IsBlockingInput,cancellationToken:token);timings[label]=Math.Round(Time.unscaledTime-t,3);await Ready();Check(!manager.IsPresentationLocked,"Popup hidden and input restored: "+label);}
 async Cysharp.Threading.Tasks.UniTask Blocked(string code){
  var language=manager.CurrentLanguage;await Press(120);await Frames(30);Check(manager.IsTitleScreen&&!manager.IsTransitioning,"Short press does not start while popup is shown: "+code);
  await Press(2300);await Frames(30);Check(manager.CurrentLanguage==language&&!manager.IsLanguageChanging,"Long press does not change language while popup is shown: "+code);}
 try{
  Application.runInBackground=true;if(adapter)adapter.enabled=false;await Ready();
  Check(!Overlay().activeSelf&&!hub.HasPresentBlocks,"Empty board: no popup");
  if(manager.CurrentLanguage!=Core.SessionLanguage.Thai){await Press(2300);await Ready();}
  Check(manager.CurrentLanguage==Core.SessionLanguage.Thai,"Thai title");
  // A group card on the title screen.
  hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Group,"student");await Shown("show_group_th");await Shot("popup_th");
  await Blocked("th");
  // Group-first ignores a lone product in the session, but the card is on the board.
  hub.RemoveReader(2);await Hidden("hide_th");
  hub.RecognizeReader(1,Exhibition.Blocks.BlockKind.Product,"shoes");Check(hub.Readers.OccupiedCount==0&&hub.HasPresentBlocks,"Ignored product still counts as present");await Shown("show_ignored_product");
  // Hardware path: TCP reports presence directly.
  hub.ReportPresence(3,new Exhibition.Blocks.RfidBlock(Exhibition.Blocks.BlockKind.Color,"red"));hub.RemoveReader(1);await Frames(20);Check(Popup().IsRequested&&Overlay().activeSelf,"One remaining card keeps the popup");
  hub.ReportPresence(3,null);await Hidden("hide_after_last_card");
  // Unregistered card is not a block.
  hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Product,"not_registered");await Frames(30);Check(!Popup().IsRequested&&!Overlay().activeSelf,"Unregistered card does not show the popup");hub.RemoveReader(0);
  // Rapid place/remove/place settles shown, then hidden.
  hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Group,"child");await Frames(3);hub.RemoveReader(2);await Frames(3);hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Group,"child");await Shown("show_after_flicker");hub.RemoveReader(2);await Hidden("hide_after_flicker");
  // Start works again once the board is empty.
  await Press(120);await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="1_Game",cancellationToken:token);await Ready();Check(true,"Short press starts after the board is cleared");
  // Leave a card on the board and return home: the session forgets it, the popup does not.
  hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Group,"student");await Ready();manager.ReturnToIdleManually();
  await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsTitleScreen,cancellationToken:token);
  Check(hub.Readers.OccupiedCount==0&&hub.HasPresentBlocks,"Home resets the session but keeps the card present");
  await Shown("show_after_return_home");await Blocked("th-return");hub.RemoveReader(2);await Hidden("hide_after_return_home");
  // English: same image.
  await Press(2300);await Ready();Check(manager.CurrentLanguage==Core.SessionLanguage.English,"English title");
  hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Color,"blue");await Shown("show_en");await Shot("popup_en");await Blocked("en");
  var text=Overlay().transform.Find("PopupContent/TitleBlockPopupText").GetComponent<UnityEngine.UI.Image>();Check(text.sprite&&text.sprite.name=="TitleBlockPopupText","Same popup image in English");
  hub.RemoveReader(0);await Hidden("hide_en");
  if(manager.CurrentLanguage!=initial){await Press(2300);await Ready();}
  report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{if(manager)manager.ResetButtonInput();if(adapter)adapter.enabled=wasEnabled;Application.runInBackground=background;System.IO.File.WriteAllText(Out+"runtime.json",report.ToString());UnityEngine.Object.Destroy(root);}
});
return "UI-IDLEPOP-001 verification started.";
