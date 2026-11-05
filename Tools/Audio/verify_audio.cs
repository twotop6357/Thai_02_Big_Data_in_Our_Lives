// Run in Play mode from 0_Idle. Uses a prior generated PNG; no new AI request. Takes about 2 minutes (includes a real 30s timeout).
if(GameObject.Find("AudioVerification"))return "Already running.";
var root=new GameObject("AudioVerification");UnityEngine.Object.DontDestroyOnLoad(root);
var played=new System.Collections.Generic.List<string>();
Application.LogCallback listener=(message,stack,type)=>{if(message.StartsWith("[AudioManager] Played "))played.Add(message.Substring(22).TrimEnd('.'));};
Application.logMessageReceived+=listener;
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="AUDIO-001"};var checks=new Newtonsoft.Json.Linq.JArray();report["checks"]=checks;var sound=new Newtonsoft.Json.Linq.JObject();report["output_peak"]=sound;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var hub=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidReaderHub>();var adapter=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
 bool wasEnabled=adapter&&adapter.enabled,background=Application.runInBackground;var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
 var G=Exhibition.Blocks.BlockKind.Group;var P=Exhibition.Blocks.BlockKind.Product;var C=Exhibition.Blocks.BlockKind.Color;
 using var deadline=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(200));var token=deadline.Token;
 void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
 async Cysharp.Threading.Tasks.UniTask Frames(int n=5){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Ready(){await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsInputReady&&!manager.IsTransitioning&&!manager.IsPresentationLocked&&!manager.IsLanguageChanging&&hub.IsArtworkReady,cancellationToken:token);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Press(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);}
 async Cysharp.Threading.Tasks.UniTask Hold(int ms){manager.SetButtonPressed(true);await Cysharp.Threading.Tasks.UniTask.Delay(ms,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);manager.SetButtonPressed(false);}
 // Real mixer output, sampled across the next half second, proves the sound is heard and not cut by the scene change.
 async Cysharp.Threading.Tasks.UniTask<float> Peak(string name,double seconds=.2){var buffer=new float[1024];float peak=0;double begin=Time.realtimeSinceStartupAsDouble;while(Time.realtimeSinceStartupAsDouble-begin<seconds){AudioListener.GetOutputData(buffer,0);foreach(var v in buffer)peak=Mathf.Max(peak,Mathf.Abs(v));await Frames(1);}sound[name]=Math.Round(peak,4);return peak;}
 void Sounds(string expected,string label){Check(played.Count==1&&played[0]==expected,label+": expected "+expected+", got ["+string.Join(",",played)+"]");played.Clear();}
 void Silent(string label){Check(played.Count==0,label+": expected no sound, got ["+string.Join(",",played)+"]");}
 void Audible(float peak,string label){Check(peak>.001f,label+" reaches the audio output, peak "+peak);}
 Exhibition.UI.BlockExperienceView View()=>UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.BlockExperienceView>();
 async Cysharp.Threading.Tasks.UniTask ToGame(){await Press();await Ready();var c=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>c&&c.IsReady,cancellationToken:token);await Ready();played.Clear();}
 try{
  Application.runInBackground=true;if(adapter)adapter.enabled=false;await Ready();played.Clear();
  Check(manager.GetComponents<Core.AudioManager>().Length<=1,"One audio manager at most");
  // Language change is not a screen change.
  await Hold(2300);await Frames();await Ready();Silent("Title language toggle");await Hold(2300);await Frames();await Ready();Silent("Title language toggle back");
  // Title to game.
  await Press();await Frames(2);Audible(await Peak("title_to_game"),"Title confirm");Sounds("confirm","Title to game");
  await Ready();var controller=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>controller&&controller.IsReady,cancellationToken:token);await Ready();played.Clear();
  // Input that changes nothing stays silent.
  await Press();await Frames();await Ready();Silent("Short press with no group");
  hub.RecognizeReader(3,G,"student");await Ready();Silent("Placing a block");
  hub.RecognizeReader(1,G,"senior");await Ready();await Press();await Frames();await Ready();Silent("Short press with two groups");hub.RemoveReader(1);await Ready();
  // Button: group to selection.
  await Press();await Frames(2);Audible(await Peak("group_to_selection"),"Panel confirm");Sounds("confirm","Group to selection");await Ready();
  Check(View().IsChoosingProduct,"Panel changed");
  await Press();await Frames();await Ready();Silent("Short press with an incomplete selection");
  hub.RecognizeReader(2,P,"shoes");hub.RecognizeReader(0,C,"red");hub.RecognizeReader(1,C,"blue");await Ready();Silent("Completing the selection");
  var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);if(!ImageConversion.LoadImage(texture,System.IO.File.ReadAllBytes("docs/validation/comfy_style3/runtime_0_shoes.png"))||!controller.TryDisplayResult(texture,controller.Selection.Revision)){UnityEngine.Object.Destroy(texture);throw new Exception("Could not attach the prior PNG.");}
  await Ready();
  // Button: selection to result.
  await Press();await Frames(2);Audible(await Peak("selection_to_result"),"Result confirm");Sounds("confirm","Selection to result");
  await Ready();Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="2_Result","Result reached");
  // Button: advertisement to video.
  var video=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.ResultVideoController>();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsInputReady&&!manager.IsPresentationLocked,cancellationToken:token);await Frames();played.Clear();
  await Press();await Frames(2);Audible(await Peak("ad_to_video"),"Video confirm");Sounds("confirm","Advertisement to video");
  await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>video.State==Exhibition.UI.ResultVideoController.ScreenState.Playing,cancellationToken:token);await Ready();
  await Press();await Frames();await Ready();Silent("Short press during video");
  var player=(UnityEngine.Video.VideoPlayer)typeof(Exhibition.UI.ResultVideoController).GetField("player",flags).GetValue(video);player.time=Math.Max(0,player.length-.25);player.Play();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>video.State==Exhibition.UI.ResultVideoController.ScreenState.Finished,cancellationToken:token);await Ready();Silent("Video ending by itself");
  // Finished popup: button goes home.
  await Press();await Frames(2);Audible(await Peak("finished_to_home",.3),"Finished home");Sounds("home","Finished popup to title");
  await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsTitleScreen,cancellationToken:token);await Ready();Silent("After arriving home");
  // Five-second hold home, with the source surviving StopSessionMedia.
  await ToGame();manager.SetButtonPressed(true);await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>played.Count>0,cancellationToken:token);Audible(await Peak("hold_to_home",.3),"Hold home");manager.SetButtonPressed(false);Sounds("home","Five-second hold to title");
  await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsTitleScreen,cancellationToken:token);await Ready();Silent("Hold home plays once");
  // Removing every block at the end of the video goes home through the same path; the real timeout is checked here.
  await ToGame();double wait=Time.realtimeSinceStartupAsDouble;await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>played.Count>0,cancellationToken:token);report["timeout_seconds"]=Math.Round(Time.realtimeSinceStartupAsDouble-wait,1);Audible(await Peak("timeout_to_home",.3),"Timeout home");Sounds("home","Inactivity timeout to title");
  await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsTitleScreen,cancellationToken:token);await Ready();
  // Auto-advance uses the button's transition, so it plays confirm once.
  await ToGame();hub.RecognizeReader(1,G,"child");await Ready();played.Clear();hub.RecognizeReader(0,P,"hats");await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>View().IsChoosingProduct,cancellationToken:token);await Ready();Sounds("confirm","Auto-advance to selection");
  await Hold(5200);await Ready();played.Clear();Check(manager.IsTitleScreen,"Back on title");
  Check(manager.GetComponents<Core.AudioManager>().Length==1,"One audio manager after repeated returns");Check(UnityEngine.Object.FindObjectsByType<Core.AudioManager>(FindObjectsSortMode.None).Length==1,"One audio manager in the app");
  report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{Application.logMessageReceived-=listener;if(manager)manager.ResetButtonInput();if(adapter)adapter.enabled=wasEnabled;Application.runInBackground=background;System.IO.Directory.CreateDirectory("docs/validation/audio");System.IO.File.WriteAllText("docs/validation/audio/runtime.json",report.ToString());UnityEngine.Object.Destroy(root);}
});
return "Audio verification started.";
