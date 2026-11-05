// Explicit Play check from Idle. External JSON/image files are restored in finally.
if(GameObject.Find("BlockArtLoadingVerification"))return "Already running.";
var root=new GameObject("BlockArtLoadingVerification");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="BLOCK-ART-LOADING-001"};var checks=new Newtonsoft.Json.Linq.JArray();report["checks"]=checks;
 string folder=System.IO.Path.Combine(Application.streamingAssetsPath,"BlockArtwork"),enPath=System.IO.Path.Combine(folder,"blocks_en.json"),thPath=System.IO.Path.Combine(folder,"blocks_th.json"),bagPath=System.IO.Path.Combine(folder,"Images/en/BagBlock_Eng.png");
 string originalEn=System.IO.File.ReadAllText(enPath),originalTh=System.IO.File.ReadAllText(thPath);byte[] originalBag=System.IO.File.ReadAllBytes(bagPath);
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var hub=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidReaderHub>();var adapter=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
 var timer=UnityEngine.Object.FindFirstObjectByType<HuliacDev.Core.InactivityTimer>();var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
 Exhibition.Blocks.BlockExperienceController blocks=null;bool oldAdapter=adapter&&adapter.enabled,oldBackground=Application.runInBackground;float oldScale=Time.timeScale;
 using var limit=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(160));var token=limit.Token;
 void Save()=>System.IO.File.WriteAllText("docs/validation/block_art_loading/runtime.json",report.ToString());
 void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);Save();}
 async Cysharp.Threading.Tasks.UniTask Frames(int n=4){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Ready(){await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>manager.IsInputReady&&!manager.IsTransitioning&&!manager.IsPresentationLocked&&hub.IsArtworkReady,cancellationToken:token);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Tap(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Frames();}
 Exhibition.Blocks.BlockArtworkCatalog.Entry Entry(string id)=>System.Array.Find(hub.Artwork.entries,e=>e.id==id);
 int TextureCount()=>System.Linq.Enumerable.Count(Resources.FindObjectsOfTypeAll<Texture2D>(),t=>t.name.StartsWith("ExternalBlock/"));
 string Pixels(Texture source){var rt=RenderTexture.GetTemporary(source.width,source.height,0,RenderTextureFormat.ARGB32);var previous=RenderTexture.active;var image=new Texture2D(source.width,source.height,TextureFormat.RGBA32,false);try{Graphics.Blit(source,rt);RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,source.width,source.height),0,0);image.Apply();using var sha=System.Security.Cryptography.SHA256.Create();return Convert.ToBase64String(sha.ComputeHash(image.EncodeToPNG()));}finally{RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.Destroy(image);}}
 async Cysharp.Threading.Tasks.UniTask Reload(){await hub.ReloadArtworkAsync();await Frames();}
 try{
  Application.runInBackground=true;if(adapter)adapter.enabled=false;await Ready();
  Check(hub.Artwork.entries.Length==20&&TextureCount()==36,"Startup reads 36 external textures for 20 bilingual blocks");
  foreach(var entry in hub.Artwork.entries){Check(entry.sprite.texture.name.StartsWith("ExternalBlock/")&&entry.thaiSprite.texture.name.StartsWith("ExternalBlock/"),"Both language resources load from StreamingAssets: "+entry.id);}
  var concurrent1=hub.ReloadArtworkAsync();var concurrent2=hub.ReloadArtworkAsync();await Cysharp.Threading.Tasks.UniTask.WhenAll(concurrent1,concurrent2);await Frames();Check(TextureCount()==36,"Overlapping reload callers share one load without duplicate textures");
  string thaiBefore=Pixels(Entry("bags").thaiSprite.texture),bagBefore=Pixels(Entry("bags").sprite.texture);
  System.IO.File.WriteAllBytes(bagPath,System.IO.File.ReadAllBytes(System.IO.Path.Combine(folder,"Images/en/ShoesBlock_Eng.png")));await Reload();
  Check(Pixels(Entry("bags").sprite.texture)==Pixels(Entry("shoes").sprite.texture)&&Pixels(Entry("bags").sprite.texture)!=bagBefore,"Replacing the PNG bytes changes the English block without rebuilding");
  Check(Pixels(Entry("bags").thaiSprite.texture)==thaiBefore,"English PNG replacement does not alter Thai artwork");
  System.IO.File.WriteAllBytes(bagPath,originalBag);await Reload();
  var doc=Newtonsoft.Json.Linq.JObject.Parse(originalTh);foreach(var item in doc["blocks"])if((string)item["id"]=="bags")item["image"]="Images/th/HatBlock_Thai.png";
  System.IO.File.WriteAllText(thPath,doc.ToString());await Reload();Check(Entry("bags").thaiSprite==Entry("hats").thaiSprite&&Pixels(Entry("bags").sprite.texture)==bagBefore,"Thai JSON path override applies independently and shares decoded texture");
  System.IO.File.WriteAllText(thPath,originalTh);await Reload();
  System.IO.File.WriteAllText(enPath,"{invalid");await Reload();Check(!Entry("bags").sprite.texture.name.StartsWith("ExternalBlock/")&&Entry("bags").thaiSprite.texture.name.StartsWith("ExternalBlock/"),"Malformed English JSON falls back while Thai remains external");
  System.IO.File.WriteAllText(enPath,originalEn);await Reload();
  doc=Newtonsoft.Json.Linq.JObject.Parse(originalEn);foreach(var item in doc["blocks"])if((string)item["id"]=="bags")item["image"]="Images/en/not_present.png";
  System.IO.File.WriteAllText(enPath,doc.ToString());await Reload();Check(!Entry("bags").sprite.texture.name.StartsWith("ExternalBlock/")&&Entry("shoes").sprite.texture.name.StartsWith("ExternalBlock/"),"One missing image falls back without losing other blocks");
  System.IO.File.WriteAllText(enPath,originalEn);for(int i=0;i<3;i++)await Reload();Check(TextureCount()==36,"Repeated replacement/recovery releases retired runtime textures");
  await Tap();await Ready();blocks=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>blocks&&blocks.IsReady,cancellationToken:token);
  hub.RecognizeReader(3,Exhibition.Blocks.BlockKind.Group,"student");hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Product,"shoes");hub.RecognizeReader(1,Exhibition.Blocks.BlockKind.Color,"red");hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Color,"blue");await Ready();
  var view=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.BlockExperienceView>();var loading=(Exhibition.UI.GenerationLoadingIndicator)blocks.GetType().GetField("_loadingIndicator",flags).GetValue(blocks);
  Check(view.IsChoosingProduct&&blocks.HasRequiredComposition&&!blocks.IsGenerating&&!loading.IsLoading,"Complete blocks wait for a button before generating");
  var result=(UnityEngine.UI.Image)blocks.GetType().GetField("resultImage",flags).GetValue(blocks);
  var fieldImages=(UnityEngine.UI.Image[])view.GetType().GetField("_selectionArtwork",flags).GetValue(view);Check(fieldImages[0].sprite==Entry("shoes").thaiSprite,"Thai mode screen displays the external Thai block");
  await Tap();Check(blocks.IsGenerating&&loading.IsLoading&&manager.IsPresentationLocked&&timer.IsPaused,"Actual button starts generation, spinner and input/timer lock");
  float angle=loading.Rotation;Time.timeScale=0;await Cysharp.Threading.Tasks.UniTask.Delay(450,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);
  Check(Mathf.Abs(Mathf.DeltaAngle(angle,loading.Rotation))>20,"Circular indicator keeps rotating at timeScale zero");Time.timeScale=oldScale;
  Check(!result.gameObject.activeSelf&&System.Linq.Enumerable.All(loading.GetComponentsInChildren<UnityEngine.UI.Graphic>(),g=>!g.raycastTarget),"Loading replaces the empty product area without intercepting input");
  Check(System.Linq.Enumerable.All(loading.GetComponentsInChildren<UnityEngine.UI.Graphic>(),g=>g.GetComponent<CanvasRenderer>()&&!g.canvasRenderer.cull&&g.canvasRenderer.GetAlpha()>0),"Every spinner layer has a visible CanvasRenderer");
  Check(loading.GetComponentsInChildren<UnityEngine.UI.Graphic>().Length==1&&loading.transform.Find("RotatingArc")&&!loading.transform.Find("WhiteMedallion")&&!loading.transform.Find("PastelTrack"),"Spinner contains only the rotating arc with no background or static decoration");
  var arc=loading.GetComponentInChildren<Exhibition.UI.LoadingRingGraphic>();var mesh=new Mesh();using(var vertices=new UnityEngine.UI.VertexHelper()){arc.GetType().GetMethod("OnPopulateMesh",flags|System.Reflection.BindingFlags.DeclaredOnly).Invoke(arc,new object[]{vertices});vertices.FillMesh(mesh);}
  Check(System.Linq.Enumerable.Any(mesh.colors32,c=>c.a==0)&&System.Linq.Enumerable.Any(mesh.colors32,c=>c.a>0&&c.a<255)&&System.Linq.Enumerable.Any(mesh.colors32,c=>c.a==255),"Arc tail mesh fades smoothly from fully transparent to opaque");UnityEngine.Object.Destroy(mesh);
  UnityEngine.ScreenCapture.CaptureScreenshot("docs/validation/block_art_loading/loading.png");await Frames();await Tap();Check(blocks.IsGenerating&&!manager.IsTransitioning,"Pressing during generation cannot navigate");
  await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>!blocks.IsGenerating,cancellationToken:token);await Ready();Check(blocks.HasGeneratedImage&&!loading.IsLoading&&!timer.IsPaused,"Actual success hides spinner and displays the completed image");
  UnityEngine.ScreenCapture.CaptureScreenshot("docs/validation/block_art_loading/success.png");await Frames();
  // Fixture failures are response/lifecycle tests, not new model quality samples.
  var settings=(Exhibition.Blocks.ComfyBlockSettings)blocks.GetType().GetField("_settings",flags).GetValue(blocks);settings.serverUrl="http://127.0.0.1:18191";
  var fixture=new Exhibition.Blocks.ComfyImageClient(settings);
  async Cysharp.Threading.Tasks.UniTask Mode(string mode)=>await fixture.Json("/mode",new Newtonsoft.Json.Linq.JObject{["mode"]=mode},token);
  await Mode("delay");blocks.GenerateImage();await Frames();Check(loading.IsLoading&&!blocks.HasGeneratedImage,"Retry clears previous image and shows spinner");blocks.CancelGeneration();await Ready();Check(!loading.IsLoading&&!blocks.IsGenerating&&!timer.IsPaused,"Explicit cancellation hides spinner and restores input");
  blocks.GenerateImage();await Frames();int readerRevision=hub.Readers.Revision;Check(!hub.RemoveReader(2)&&blocks.IsGenerating&&loading.IsLoading&&hub.Readers.Revision==readerRevision,"RFID loss during generation keeps the snapshot and spinner");blocks.CancelGeneration();await Ready();Check(!loading.IsLoading&&!hub.Readers.IsInputLocked,"Explicit cancellation releases the RFID snapshot");hub.RemoveReader(2);hub.RecognizeReader(2,Exhibition.Blocks.BlockKind.Color,"green");await Ready();Check(!blocks.IsGenerating,"New complete composition waits for a fresh button");
  settings.generationTimeoutSeconds=1;blocks.GenerateImage();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>!blocks.IsGenerating,cancellationToken:token);await Ready();Check(!loading.IsLoading&&!timer.IsPaused&&blocks.Status.Contains("timed out"),"Timeout hides spinner and restores retry state");settings.generationTimeoutSeconds=10;
  await Mode("error");await Tap();await Cysharp.Threading.Tasks.UniTask.WaitUntil(()=>!blocks.IsGenerating,cancellationToken:token);await Ready();Check(!loading.IsLoading&&!timer.IsPaused&&blocks.Status.Contains("failed"),"Server failure hides spinner and restores retry state");
  await Mode("delay");blocks.GenerateImage();blocks.CancelGeneration();blocks.GenerateImage();await Frames();Check(blocks.IsGenerating&&loading.IsLoading,"Old cancellation cannot hide the replacement request spinner");manager.ReturnToIdleManually();await Ready();Check(manager.IsTitleScreen&&!timer.IsPaused&&!manager.IsPresentationLocked&&!UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.GenerationLoadingIndicator>(),"Home destroys loading UI and cancels the active session");
  Check(TextureCount()==36,"Returning home reloads external resources without accumulation");report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{
  Time.timeScale=oldScale;System.IO.File.WriteAllText(enPath,originalEn);System.IO.File.WriteAllText(thPath,originalTh);System.IO.File.WriteAllBytes(bagPath,originalBag);
  if(blocks)blocks.CancelGeneration();if(manager&&!manager.IsTitleScreen)manager.ReturnToIdleManually();if(adapter)adapter.enabled=oldAdapter;Application.runInBackground=oldBackground;Save();UnityEngine.Object.Destroy(root);
 }
});
return "External block resources and loading lifecycle verification started.";
