// Focused final visual check; fixture delay prevents completion before the screenshot.
if(GameObject.Find("LoadingVisualVerification"))return "Already running.";
var root=new GameObject("LoadingVisualVerification");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="BLOCK-ART-LOADING-001 final red arc"};var checks=new Newtonsoft.Json.Linq.JArray();report["checks"]=checks;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var hub=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidReaderHub>();var adapter=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
 bool wasEnabled=adapter&&adapter.enabled,background=Application.runInBackground;float scale=Time.timeScale;Exhibition.Blocks.BlockExperienceController blocks=null;
 using var deadline=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(40));var token=deadline.Token;
 async Cysharp.Threading.Tasks.UniTask Frame(int n=4){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Ready(){await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsInputReady&&!manager.IsTransitioning&&!manager.IsPresentationLocked&&hub.IsArtworkReady,cancellationToken:token);await Frame();}
 async Cysharp.Threading.Tasks.UniTask Tap(){manager.SetButtonPressed(false);await Frame();manager.SetButtonPressed(true);await Frame();manager.SetButtonPressed(false);await Frame();}
 void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
 try{
  Application.runInBackground=true;if(adapter)adapter.enabled=false;await Ready();await Tap();await Ready();blocks=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>blocks&&blocks.IsReady,cancellationToken:token);
  hub.RecognizeReader(3,Exhibition.Blocks.BlockKind.Group,"student");hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Product,"shoes");hub.RecognizeReader(1,Exhibition.Blocks.BlockKind.Color,"red");hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Color,"blue");await Ready();
  var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;var settings=(Exhibition.Blocks.ComfyBlockSettings)blocks.GetType().GetField("_settings",flags).GetValue(blocks);settings.serverUrl="http://127.0.0.1:18191";var client=new Exhibition.Blocks.ComfyImageClient(settings);await client.Json("/mode",new Newtonsoft.Json.Linq.JObject{["mode"]="delay"},token);
  Check(!blocks.IsGenerating,"Ready blocks wait for the generation button");await Tap();var loading=(Exhibition.UI.GenerationLoadingIndicator)blocks.GetType().GetField("_loadingIndicator",flags).GetValue(blocks);await Frame();Check(loading.IsLoading&&blocks.IsGenerating,"Button displays the final spinner");
  var graphics=loading.GetComponentsInChildren<UnityEngine.UI.Graphic>();Check(graphics.Length==1&&graphics[0].name=="RotatingArc","Only the rotating ring is drawn without a plate or static decoration");
  Check(graphics[0].GetComponent<CanvasRenderer>()&&!graphics[0].canvasRenderer.cull,"Rotating ring has an active renderer");
  var arc=(Exhibition.UI.LoadingRingGraphic)graphics[0];var mesh=new Mesh();using(var vertices=new UnityEngine.UI.VertexHelper()){arc.GetType().GetMethod("OnPopulateMesh",flags|System.Reflection.BindingFlags.DeclaredOnly).Invoke(arc,new object[]{vertices});vertices.FillMesh(mesh);}var colors=mesh.colors32;Color32 expected=arc.color;
  Check(expected.r>expected.b&&System.Linq.Enumerable.All(colors,c=>c.r==expected.r&&c.g==expected.g&&c.b==expected.b),"Head and tail share one red-pink RGB");
  Check(System.Linq.Enumerable.Any(colors,c=>c.a==0)&&System.Linq.Enumerable.Any(colors,c=>c.a>0&&c.a<255)&&System.Linq.Enumerable.Any(colors,c=>c.a==255),"Only opacity changes along the smooth transparent tail");UnityEngine.Object.Destroy(mesh);
  float angle=loading.Rotation;Time.timeScale=0;await Cysharp.Threading.Tasks.UniTask.Delay(400,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);Check(Mathf.Abs(Mathf.DeltaAngle(angle,loading.Rotation))>20,"Unscaled infinite rotation continues");Time.timeScale=scale;
  UnityEngine.ScreenCapture.CaptureScreenshot("docs/validation/block_art_loading/loading.png");await Frame(6);blocks.CancelGeneration();await Ready();Check(!loading.IsLoading,"Cancellation hides the final arc");manager.ReturnToIdleManually();await Ready();Check(manager.IsTitleScreen&&!UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.GenerationLoadingIndicator>(),"Home disposes the final loading UI");report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{Time.timeScale=scale;if(blocks)blocks.CancelGeneration();if(manager&&!manager.IsTitleScreen)manager.ReturnToIdleManually();if(adapter)adapter.enabled=wasEnabled;Application.runInBackground=background;System.IO.File.WriteAllText("docs/validation/block_art_loading/visual.json",report.ToString());UnityEngine.Object.Destroy(root);}
});
return "Final red loading arc verification started.";
