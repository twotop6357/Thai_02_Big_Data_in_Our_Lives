// Actual generated results and rendered advertisement. No source scene/prefab is saved.
if(GameObject.Find("AdFx4Verification"))return "Already running.";
var root=new GameObject("AdFx4Verification");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var report=new Newtonsoft.Json.Linq.JObject{["task"]="AD-FX-004"};var checks=new Newtonsoft.Json.Linq.JArray();report["checks"]=checks;
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var adapter=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
 bool oldAdapter=adapter&&adapter.enabled,oldBackground=Application.runInBackground;float oldTime=Time.timeScale;
 using var limit=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(240));var token=limit.Token;
 var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
 void Save()=>System.IO.File.WriteAllText("docs/validation/ad_fx4/runtime.json",report.ToString());
 void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);Save();}
 async Cysharp.Threading.Tasks.UniTask Frames(int n=3){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Wait(Func<bool> predicate)=>await Cysharp.Threading.Tasks.UniTask.WaitUntil(predicate,cancellationToken:token);
 async Cysharp.Threading.Tasks.UniTask Delay(int ms)=>await Cysharp.Threading.Tasks.UniTask.Delay(ms,Cysharp.Threading.Tasks.DelayType.Realtime,cancellationToken:token);
 async Cysharp.Threading.Tasks.UniTask Ready(){await Wait(()=>manager.IsInputReady&&!manager.IsTransitioning&&!manager.IsPresentationLocked);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Tap(){manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Frames();}
 async Cysharp.Threading.Tasks.UniTask Shot(string name){ScreenCapture.CaptureScreenshot("docs/validation/ad_fx4/"+name+".png");await Frames(6);}
 try{
  System.IO.Directory.CreateDirectory("docs/validation/ad_fx4");Application.runInBackground=true;if(adapter)adapter.enabled=false;
  var cases=new[]{new[]{"bags","red","blue"},new[]{"hats","green","yellow"},new[]{"shoes","black","white"}};
  foreach(var test in cases){
   await Ready();Check(manager.IsTitleScreen,"Case starts on Idle: "+test[0]);await Tap();await Ready();
   var blocks=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();await Wait(()=>blocks&&blocks.IsReady);
   blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group,"student");await Frames();blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product,test[0]);blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1,test[1]);blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2,test[2]);await Ready();
   await Wait(()=>blocks.HasRequiredComposition);await Delay(400);await Ready();await Tap();
   await Wait(()=>!blocks.IsGenerating&&blocks.HasGeneratedImage);await Ready();
   ColorUtility.TryParseHtmlString(blocks.Catalog.Find(Exhibition.Blocks.BlockSlot.Color1,test[1]).hex,out var first);
   ColorUtility.TryParseHtmlString(blocks.Catalog.Find(Exhibition.Blocks.BlockSlot.Color2,test[2]).hex,out var second);
   Check(manager.HasResultPalette&&manager.ResultPrimaryColor==first&&manager.ResultSecondaryColor==second,"Generated result carries exact block HEX pair: "+test[1]+"/"+test[2]);
   await Tap();Exhibition.UI.AdProductHighlight effect=null;await Wait(()=>(effect=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.AdProductHighlight>())&&effect.IsPlaying);
   var so=new UnityEditor.SerializedObject(effect);var effectRoot=(GameObject)so.FindProperty("effectRoot").objectReferenceValue;var confetti=(GameObject)so.FindProperty("confettiRoot").objectReferenceValue;
   var emitters=effectRoot.GetComponentsInChildren<AssetKits.ParticleImage.ParticleImage>(true);var wave=emitters.Single(p=>p.name=="ProductRing");var glow=emitters.Single(p=>p.name=="ProductGlowRays");var stars=emitters.Single(p=>p.name=="ProductSparkles");
   var product=(RectTransform)effectRoot.transform.parent.Find("ResultImage");var board=(RectTransform)product.parent;
   Check(emitters.All(p=>p.startColor.mode==ParticleSystemGradientMode.TwoColors),"All inner emitters sample both generation colors: "+test[0]);
   Check(stars.spreadType==AssetKits.ParticleImage.Enumerations.SpreadType.Random&&stars.startSize.mainCurve.mode==ParticleSystemCurveMode.TwoConstants&&stars.lifetime.mode==ParticleSystemCurveMode.TwoConstants,"Sparkles vary their placement, size and lifetime: "+test[0]);
   Check(product.GetSiblingIndex()>effectRoot.transform.GetSiblingIndex()&&product.GetSiblingIndex()>confetti.transform.GetSiblingIndex(),"Image remains above effect layers: "+test[0]);
   var colorsBefore=emitters.Select(p=>p.startColor.colorMin).ToArray();
   var hub=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidReaderHub>();hub.RecognizeReader(3,Exhibition.Blocks.BlockKind.Color,"yellow");await Frames();
   Check(emitters.Select(p=>p.startColor.colorMin).SequenceEqual(colorsBefore)&&manager.ResultPrimaryColor==first&&manager.ResultSecondaryColor==second,"Later RFID signals cannot recolor the generated result: "+test[0]);
   Time.timeScale=0;await Tap();Check(effect.IsPlaying&&manager.IsPresentationLocked,"Entrance blocks input while paused: "+test[0]);await Delay(650);
   var starBounds=board.rect;starBounds.xMin+=16;starBounds.xMax-=16;starBounds.yMin+=16;starBounds.yMax-=16;
   Check(stars.particles.Count>0&&stars.particles.All(p=>p.points.All(point=>{var local=board.InverseTransformPoint(stars.transform.TransformPoint(point));return starBounds.Contains(new Vector2(local.x,local.y));})),"Sparkle quads stay inside the advertisement board: "+test[0]);await Shot(test[0]+"_entrance");
   var rates=new System.Collections.Generic.HashSet<float>();var colors=new System.Collections.Generic.HashSet<string>();int vertices=0;double start=Time.realtimeSinceStartupAsDouble;
   var bounds=board.rect;bounds.xMin+=16;bounds.xMax-=16;bounds.yMin+=16;bounds.yMax-=16;
   while(Time.realtimeSinceStartupAsDouble-start<9.5){
    rates.Add(wave.rateOverTime);
    foreach(var particle in wave.particles){colors.Add(ColorUtility.ToHtmlStringRGB(particle.Color));foreach(var point in particle.points){var local=board.InverseTransformPoint(wave.transform.TransformPoint(point));if(!bounds.Contains(new Vector2(local.x,local.y)))throw new Exception("Wave left board: "+local);vertices++;}}
    await Frames(1);
   }
   Check(rates.Count>=2&&vertices>100&&colors.Count>=2,"Irregular multicolor waves stay inside board at timeScale zero: "+test[0]);
   Check(!effect.IsPlaying&&!confetti.activeSelf&&effect.AreEffectsVisible&&!manager.IsPresentationLocked,"Finite confetti ends and releases input; ambient remains: "+test[0]);await Shot(test[0]+"_ambient");
   effect.StopEffects();await Frames();Check(!effect.AreEffectsVisible&&!manager.IsPresentationLocked,"Stop clears only owned effects: "+test[0]);
   effect.ResumeAmbient();await Frames();Check(effect.AreEffectsVisible&&!confetti.activeSelf&&manager.HasResultPalette,"Resume preserves palette and does not replay confetti: "+test[0]);
   Time.timeScale=1;manager.ReturnToIdleManually();await Ready();Check(!manager.HasResultPalette&&!manager.ResultTexture,"Home clears image and palette together: "+test[0]);
  }
  report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{Time.timeScale=oldTime;Application.runInBackground=oldBackground;if(adapter)adapter.enabled=oldAdapter;if(manager)manager.ResetButtonInput();Save();UnityEngine.Object.Destroy(root);}
});
return "Actual three-palette advertisement validation started.";
