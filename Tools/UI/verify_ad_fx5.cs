// Render-level mask, visibility, sorting and lifetime checks. Uses a prior PNG, no AI request.
if(GameObject.Find("AdFx5Verification"))return "Already running.";
var root=new GameObject("AdFx5Verification");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="AD-FX-005",["prior_png_fixture"]=true};var checks=new Newtonsoft.Json.Linq.JArray();var measurements=new Newtonsoft.Json.Linq.JArray();report["checks"]=checks;report["measurements"]=measurements;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var adapter=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();bool oldAdapter=adapter&&adapter.enabled,oldBackground=Application.runInBackground;float oldTime=Time.timeScale;
 using var limit=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(180));var token=limit.Token;
 GameObject probe=null,cover=null;Texture2D baseline=null,clipped=null,unclipped=null;
 void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);System.IO.File.WriteAllText("docs/validation/ad_fx5/runtime.json",report.ToString());}
 async Cysharp.Threading.Tasks.UniTask Frames(int n=4){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Wait(Func<bool> p)=>await Cysharp.Threading.Tasks.UniTask.WaitUntil(p,cancellationToken:token);
 async Cysharp.Threading.Tasks.UniTask Ready(){await Wait(()=>manager.IsInputReady&&!manager.IsTransitioning&&!manager.IsPresentationLocked);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Tap(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Frames();}
 async Cysharp.Threading.Tasks.UniTask<Texture2D> Capture(string name,MonoBehaviour owner){await Cysharp.Threading.Tasks.UniTask.WaitForEndOfFrame(owner,token);var image=ScreenCapture.CaptureScreenshotAsTexture();if(name!=null)System.IO.File.WriteAllBytes("docs/validation/ad_fx5/"+name+".png",image.EncodeToPNG());return image;}
 void ReleaseShots(){if(baseline)UnityEngine.Object.Destroy(baseline);if(clipped)UnityEngine.Object.Destroy(clipped);if(unclipped)UnityEngine.Object.Destroy(unclipped);baseline=clipped=unclipped=null;}
 float Delta(Color a,Color b)=>Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b);
 Rect ScreenRect(RectTransform rect){var corners=new Vector3[4];rect.GetWorldCorners(corners);var a=RectTransformUtility.WorldToScreenPoint(null,corners[0]);var b=RectTransformUtility.WorldToScreenPoint(null,corners[2]);return Rect.MinMaxRect(a.x,a.y,b.x,b.y);}
 GameObject ImageProbe(string name,Transform parent,Color color){var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(UnityEngine.UI.Image));go.transform.SetParent(parent,false);go.GetComponent<UnityEngine.UI.Image>().color=color;go.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;return go;}
 try{
  System.IO.Directory.CreateDirectory("docs/validation/ad_fx5");Application.runInBackground=true;if(adapter)adapter.enabled=false;
  foreach(var pair in new[]{new[]{"red","blue"},new[]{"black","white"},new[]{"white","white"}}){
   string label=pair[0]+"_"+pair[1];await Ready();Check(manager.IsTitleScreen,"Case starts on Idle: "+label);await Tap();await Ready();
   var b=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Wait(()=>b&&b.IsReady);b.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group,"student");await Frames();b.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product,"bags");b.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1,pair[0]);b.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2,pair[1]);await Ready();
   var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);ImageConversion.LoadImage(texture,System.IO.File.ReadAllBytes("docs/validation/comfy_style3/runtime_2_bags.png"));Check(b.TryDisplayResult(texture,b.Selection.Revision),"Result and palette bridge succeeds: "+label);await Ready();await Tap();
   Exhibition.UI.AdProductHighlight effect=null;await Wait(()=>(effect=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.AdProductHighlight>())&&(effect.IsPlaying||!manager.IsTransitioning));Check(effect.IsPlaying,"Advertisement entrance starts successfully: "+label);
   var so=new UnityEditor.SerializedObject(effect);var inner=(GameObject)so.FindProperty("effectRoot").objectReferenceValue;var confetti=(GameObject)so.FindProperty("confettiRoot").objectReferenceValue;var opacity=(CanvasGroup)so.FindProperty("effectOpacity").objectReferenceValue;
   var mask=inner.GetComponentInParent<UnityEngine.UI.Mask>();var board=(RectTransform)mask.transform.parent;var product=(RectTransform)board.Find("ResultImage");var particles=inner.GetComponentsInChildren<AssetKits.ParticleImage.ParticleImage>(true);var top=confetti.GetComponent<Canvas>();
   Check(mask&&mask.enabled&&!mask.showMaskGraphic&&mask.GetComponent<UnityEngine.UI.Image>().sprite==board.GetComponent<UnityEngine.UI.Image>().sprite,"Hidden rounded mask follows the actual board sprite: "+label);
   Check(particles.All(p=>p.maskable)&&inner.GetComponentsInChildren<UnityEngine.UI.MaskableGraphic>(true).All(g=>g.maskable),"Every inner particle/trail participates in masking: "+label);
   var canvasField=(Canvas)effect.GetType().GetField("_confettiCanvas",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(effect);
   measurements.Add(new Newtonsoft.Json.Linq.JObject{["canvasFound"]=(bool)top,["overrideSorting"]=top&&top.overrideSorting,["order"]=top?top.sortingOrder:-1,["insideMask"]=confetti.transform.IsChildOf(mask.transform),["canvasCount"]=confetti.GetComponents<Canvas>().Length,["componentId"]=top?top.GetInstanceID():0,["fieldId"]=canvasField?canvasField.GetInstanceID():0,["fieldOrder"]=canvasField?canvasField.sortingOrder:-1});
   Check(top&&top.overrideSorting&&top.sortingOrder==short.MaxValue-1&&!confetti.transform.IsChildOf(mask.transform),"Confetti has a separate foreground canvas outside the inner mask: "+label);
   Check(particles.Single(p=>p.name=="ProductGlowRays").startSize.mainCurve.constantMin>=740&&particles.Single(p=>p.name=="ProductRing").startSize.mainCurve.constantMin>=620,"Ambient minimum sizes are enlarged: "+label);
   Time.timeScale=0;await Cysharp.Threading.Tasks.UniTask.Delay(900,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);UnityEngine.Object.Destroy(await Capture(label+"_entrance",effect));
   if(label=="red_blue"){
    cover=ImageProbe("ConfettiOcclusionProbe",top.rootCanvas.transform,new Color(.02f,.06f,.1f));var rect=(RectTransform)cover.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;cover.transform.SetAsLastSibling();
    var confettiOpacity=confetti.GetComponent<CanvasGroup>();confettiOpacity.alpha=0;await Frames();baseline=await Capture(null,effect);confettiOpacity.alpha=1;await Frames();clipped=await Capture("confetti_over_all_ui",effect);int foreground=0;for(int y=0;y<baseline.height;y+=3)for(int x=0;x<baseline.width;x+=3)if(Delta(baseline.GetPixel(x,y),clipped.GetPixel(x,y))>.08f)foreground++;
    Check(foreground>100,"Rendered confetti remains above an opaque last-sibling screen cover");measurements.Add(new Newtonsoft.Json.Linq.JObject{["foregroundProbePixels"]=foreground});UnityEngine.Object.Destroy(cover);cover=null;ReleaseShots();await Frames();
    await manager.Presentation.CoverAsync(token);clipped=await Capture(null,effect);int leaks=0;for(int y=0;y<clipped.height;y+=8)for(int x=0;x<clipped.width;x+=8)if(clipped.GetPixel(x,y).maxColorComponent>.02f)leaks++;Check(leaks==0,"Scene transition blackout still covers foreground confetti");ReleaseShots();await manager.Presentation.UncoverAsync(token);
   }
   await Wait(()=>!effect.IsPlaying);await Ready();Check(effect.AreEffectsVisible&&!manager.IsPresentationLocked&&!confetti.activeSelf,"Entrance completes while ambient remains: "+label);
   var boardRect=ScreenRect(board);var productRect=ScreenRect(product);opacity.alpha=0;await Frames();baseline=await Capture(null,effect);opacity.alpha=1;await Frames(8);clipped=await Capture(label+"_ambient",effect);int visible=0;
   for(int y=Mathf.CeilToInt(boardRect.yMin+15);y<boardRect.yMax-15;y+=3)for(int x=Mathf.CeilToInt(boardRect.xMin+15);x<boardRect.xMax-15;x+=3)if(!productRect.Contains(new Vector2(x,y))&&Delta(baseline.GetPixel(x,y),clipped.GetPixel(x,y))>.04f)visible++;
   Check(visible>200,"Ambient color remains visibly present outside the product rectangle: "+label);measurements.Add(new Newtonsoft.Json.Linq.JObject{["palette"]=label,["visibleAmbientProbePixels"]=visible});ReleaseShots();
   probe=ImageProbe("MaskBoundaryProbe",inner.transform,Color.magenta);((RectTransform)probe.transform).sizeDelta=new Vector2(2600,1800);probe.transform.SetAsLastSibling();opacity.alpha=0;await Frames();baseline=await Capture(null,effect);opacity.alpha=1;await Frames();clipped=await Capture(label=="red_blue"?"mask_probe_clipped":null,effect);mask.enabled=false;await Frames();unclipped=await Capture(null,effect);mask.enabled=true;
   int outsideChanges=0,positiveControl=0;for(int y=1;y<baseline.height;y+=4)for(int x=1;x<baseline.width;x+=4)if(!boardRect.Contains(new Vector2(x,y))){if(Delta(baseline.GetPixel(x,y),clipped.GetPixel(x,y))>.015f)outsideChanges++;if(Delta(baseline.GetPixel(x,y),unclipped.GetPixel(x,y))>.08f)positiveControl++;}
   Check(outsideChanges==0&&positiveControl>500,"Oversized probe is clipped outside the box; disabling mask proves the probe would escape: "+label);
   var insidePoint=new Vector2(boardRect.center.x+350,boardRect.center.y);Check(Delta(baseline.GetPixel((int)insidePoint.x,(int)insidePoint.y),clipped.GetPixel((int)insidePoint.x,(int)insidePoint.y))>.3f,"Mask still renders valid content inside the box: "+label);
   var corner=new Vector2(boardRect.xMin+16,boardRect.yMax-16);Check(Delta(baseline.GetPixel((int)corner.x,(int)corner.y),clipped.GetPixel((int)corner.x,(int)corner.y))<.015f,"Rounded corner remains clipped: "+label);
   UnityEngine.Object.Destroy(probe);probe=null;ReleaseShots();await Frames();effect.StopEffects();await Frames();Check(!inner.activeSelf&&!mask.gameObject.activeSelf&&!top.enabled,"Stopping hides mask and foreground canvas: "+label);effect.ResumeAmbient();await Frames();Check(inner.activeInHierarchy&&mask.gameObject.activeSelf&&!confetti.activeSelf,"Resume restores the masked ambient only: "+label);
   Time.timeScale=1;manager.ReturnToIdleManually();await Ready();Check(manager.IsTitleScreen&&!manager.HasResultPalette,"Home clears scene-owned layers and palette: "+label);
  }
  report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{if(probe)UnityEngine.Object.Destroy(probe);if(cover)UnityEngine.Object.Destroy(cover);ReleaseShots();Time.timeScale=oldTime;Application.runInBackground=oldBackground;if(adapter)adapter.enabled=oldAdapter;if(manager)manager.ResetButtonInput();System.IO.File.WriteAllText("docs/validation/ad_fx5/runtime.json",report.ToString());UnityEngine.Object.Destroy(root);}
});
return "Mask, visibility and foreground confetti render verification started.";
