// Execute in Unity Play mode from Idle. Exercises both languages with the existing physical-button API.
Cysharp.Threading.Tasks.UniTask.Void(async () =>
{
    var checks=new Newtonsoft.Json.Linq.JArray();
    var captures=new Newtonsoft.Json.Linq.JArray();
    var report=new Newtonsoft.Json.Linq.JObject{["task"]="UI-REFRESH-002",["checks"]=checks,["captures"]=captures};
    using var limit=new System.Threading.CancellationTokenSource(System.TimeSpan.FromSeconds(180));
    var token=limit.Token;
    var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();
    var keyboard=UnityEngine.Object.FindFirstObjectByType<Core.KeyboardButtonSimulator>();
    bool oldKeyboard=keyboard && keyboard.enabled,oldBackground=Application.runInBackground;
    Application.runInBackground=true;if(keyboard)keyboard.enabled=false;
    var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
    void Save()=>System.IO.File.WriteAllText("docs/validation/ui_refresh2/runtime.json",report.ToString());
    void Check(bool value,string label){if(!value)throw new Exception(label);checks.Add(label);Save();}
    async Cysharp.Threading.Tasks.UniTask Frames(int count=2){for(int i=0;i<count;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
    async Cysharp.Threading.Tasks.UniTask Wait(Func<bool> p)=>await Cysharp.Threading.Tasks.UniTask.WaitUntil(p,cancellationToken:token);
    async Cysharp.Threading.Tasks.UniTask Delay(int ms)=>await Cysharp.Threading.Tasks.UniTask.Delay(ms,Cysharp.Threading.Tasks.DelayType.UnscaledDeltaTime,cancellationToken:token);
    async Cysharp.Threading.Tasks.UniTask Ready()=>await Wait(()=>manager.IsInputReady && !manager.IsTransitioning && !manager.IsPresentationLocked && !manager.IsLanguageChanging);
    async Cysharp.Threading.Tasks.UniTask Tap(){await Ready();manager.SetButtonPressed(false);await Frames();manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Frames();}
    async Cysharp.Threading.Tasks.UniTask Hold(int ms){await Ready();manager.SetButtonPressed(true);await Delay(ms);manager.SetButtonPressed(false);await Frames();await Ready();}
    Exhibition.UI.LocalizedImageSet UI()=>UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.LocalizedImageSet>();
    void Geometry(string stage)
    {
        var ui=UI();var doc=JsonUtility.FromJson<Exhibition.UI.ImageLanguageDocument>(System.IO.File.ReadAllText("Assets/StreamingAssets/Lang_"+(manager.LanguageCode=="th"?"Th":"En")+".json"));
        var so=new UnityEditor.SerializedObject(ui);var bindings=so.FindProperty("bindings");
        for(int i=0;i<bindings.arraySize;i++)
        {
            var b=bindings.GetArrayElementAtIndex(i);string id=b.FindPropertyRelative("id").stringValue;
            var image=(UnityEngine.UI.Image)b.FindPropertyRelative("image").objectReferenceValue;
            var entry=doc.images.Single(e=>e.id==id);
            if(Vector2.Distance(image.rectTransform.anchoredPosition,new Vector2(entry.x,-entry.y))>.02f || Vector2.Distance(image.rectTransform.sizeDelta,new Vector2(entry.width,entry.height))>.02f || image.sprite.name!=entry.sprite)
                throw new Exception("Localized geometry mismatch: "+id);
        }
        Check(ui.GetComponentsInChildren<TMPro.TMP_Text>(true).Length==0 && ui.GetComponentsInChildren<UnityEngine.UI.Text>(true).Length==0,manager.LanguageCode+" "+stage+" uses only the registered images and correct geometry");
    }
    async Cysharp.Threading.Tasks.UniTask Capture(string stage)
    {
        await Ready();await Frames();Geometry(stage);Canvas.ForceUpdateCanvases();
        await Cysharp.Threading.Tasks.UniTask.WaitForEndOfFrame(UI(),token);
        var image=ScreenCapture.CaptureScreenshotAsTexture();
        string path="docs/validation/ui_refresh2/"+manager.LanguageCode+"_"+stage+".png";
        System.IO.File.WriteAllBytes(path,image.EncodeToPNG());UnityEngine.Object.Destroy(image);
        captures.Add(new Newtonsoft.Json.Linq.JObject{["language"]=manager.LanguageCode,["stage"]=stage,["path"]=path});Save();
    }
    try
    {
        await Ready();Check(manager.IsTitleScreen && UI().IsReady,"Start from a ready Idle scene");
        if(manager.LanguageCode!="th")await Hold(2150);
        foreach(string language in new[]{"th","en"})
        {
            Check(manager.LanguageCode==language,language+" mode is selected");await Capture("idle");
            var doc=JsonUtility.FromJson<Exhibition.UI.ImageLanguageDocument>(System.IO.File.ReadAllText("Assets/StreamingAssets/Lang_"+(language=="th"?"Th":"En")+".json"));
            var guide=doc.images.Single(e=>e.id=="idle.help");Check(guide.width==1315 && guide.x==307 && guide.y==879,language+" enlarged title hint fits the common guide position");
            await Tap();await Wait(()=>UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="1_Game");
            var blocks=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.BlockExperienceController>();
            await Wait(()=>blocks && blocks.IsReady);await Ready();
            var view=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.BlockExperienceView>();
            var root=UI().transform;var board=(RectTransform)root.Find("GroupSelection/GroupBoard");
            Check(Mathf.Abs(board.anchoredPosition.y+274)<.01f,language+" group board and all four readers use the new vertical alignment");
            var groupSlots=board.GetComponentsInChildren<Exhibition.UI.UIAnimatedElement>(true).Where(e=>e.name.StartsWith("GroupFrame")).ToArray();
            Check(groupSlots.Length==4 && groupSlots.All(s=>s.Style==Exhibition.UI.UIAnimatedElement.Motion.Hint && s.transform.localScale==s.DesignScale),language+" four empty slots fade at their design size");
            await Tap();Check(!view.IsChoosingProduct,language+" empty group cannot advance");
            blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group,"student");await Frames();await Ready();
            Check(view.AreRequiredBlocksRecognized,language+" one recognized group enables the group button");await Capture("group");
            await Tap();float blackout=0;bool sawMoving=false;
            while(manager.IsPresentationLocked)
            {
                blackout=Mathf.Max(blackout,manager.Presentation.BlackoutAlpha);
                sawMoving|=root.Find("MovingBlockSlots")!=null;
                await Frames(1);
            }
            await Ready();Check(view.IsChoosingProduct && blackout==0,language+" group-to-placement keeps the animated slot transfer without blackout");
            blocks.RemoveBlock(Exhibition.Blocks.BlockSlot.Group);await Frames();await Ready();
            Check(view.IsChoosingProduct && !view.AreRequiredBlocksRecognized,language+" reader removal keeps the placement page and disables continue");
            blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Group,"student");
            blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Product,"bags");
            blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color1,"red");await Frames();await Ready();
            var hint=(GameObject)typeof(Exhibition.UI.BlockExperienceView).GetField("selectionContinue",flags).GetValue(view);
            Check(!view.AreRequiredBlocksRecognized && !hint.activeSelf,language+" incomplete block composition hides the placement hint");
            await Tap();Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="1_Game",language+" incomplete composition cannot advance");
            blocks.RecognizeBlock(Exhibition.Blocks.BlockSlot.Color2,"blue");
            await Wait(()=>!blocks.IsGenerating && manager.ResultTexture);await Ready();
            Check(blocks.HasRequiredComposition && hint.activeSelf,language+" group/product/two colors complete the existing coloring and hint gate");
            var frames=root.GetComponentsInChildren<UnityEngine.UI.Image>(true).Where(i=>i.name.StartsWith("GroupFrame") || i.name.StartsWith("SelectionFrame")).ToArray();
            Check(frames.Length==8 && frames.All(i=>i.color==Color.white),language+" all slot frames keep their original untinted colors");await Capture("selection");
            await Tap();await Wait(()=>UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="2_Result");
            var effect=UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.AdProductHighlight>();
            await Wait(()=>effect && effect.IsPlaying);Check(manager.IsPresentationLocked,language+" advertisement entrance and confetti still lock input");
            manager.SetButtonPressed(true);await Frames();manager.SetButtonPressed(false);await Frames();
            var video=effect.GetComponent<Exhibition.UI.ResultVideoController>();
            Check(video.State==Exhibition.UI.ResultVideoController.ScreenState.Advertisement,language+" early advertisement tap is ignored");
            await Ready();Check(effect.AreEffectsVisible && !effect.IsPlaying,language+" slow aura keeps looping after the finite entrance");await Capture("result");
            await Tap();float maximumBlack=0;
            await Wait(()=>{maximumBlack=Mathf.Max(maximumBlack,manager.Presentation.BlackoutAlpha);return video.State==Exhibition.UI.ResultVideoController.ScreenState.Playing;});
            Check(maximumBlack>=.99f && (long)typeof(Exhibition.UI.ResultVideoController).GetField("_firstFrame",flags).GetValue(video)==0,language+" unchanged video transition covers the screen and delivers frame zero");
            var player=video.GetComponent<UnityEngine.Video.VideoPlayer>();
            Check(player.clip && player.clip.name.EndsWith(language),language+" mode still selects its original video");
            player.time=Math.Max(0,player.length-.2);player.Play();await Wait(()=>video.State==Exhibition.UI.ResultVideoController.ScreenState.Finished);await Ready();Geometry("video_end");
            Check(!effect.AreEffectsVisible,language+" advertisement effects are cleared before the video/end overlay");
            if(language=="th")
            {
                await Tap();await Ready();Check(manager.IsTitleScreen,"Finished video returns to Idle");
                await Hold(2150);Check(manager.LanguageCode=="en" && manager.IsTitleScreen,"Two-second title hold keeps the existing language fade/toggle");
            }
            else
            {
                await Hold(5150);Check(manager.IsTitleScreen && manager.LanguageCode=="en","Five-second button hold returns home and preserves the selected language");
            }
        }
        Check(UnityEngine.Object.FindObjectsByType<Core.GameManager>(UnityEngine.FindObjectsSortMode.None).Length==1,"Repeated sessions retain one manager");
        Check(UnityEngine.Resources.FindObjectsOfTypeAll<RenderTexture>().All(t=>t.name!="ExhibitionVideo"),"Video textures are released after returning home");
        report["status"]="PASS";
    }
    catch(Exception ex){report["status"]="FAIL";report["error"]=ex.ToString();Debug.LogException(ex);}
    finally{Application.runInBackground=oldBackground;if(keyboard)keyboard.enabled=oldKeyboard;Save();}
});
return "UI-REFRESH-002 bilingual runtime and presentation checks started";
