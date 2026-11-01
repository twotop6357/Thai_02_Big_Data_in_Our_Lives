using System;
using System.IO;
using System.Linq;
using Exhibition.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Updates existing UI geometry and fallback documents without rebuilding UI or presentation components.</summary>
public static class LocalUIRefreshAuthoring
{
    public static string Apply()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode before applying the UI exports.");
        for (int i=0;i<SceneManager.sceneCount;i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Preserve unsaved scene edits before applying the UI exports.");
        var previous=SceneManager.GetActiveScene();
        var library=AssetDatabase.LoadAssetAtPath<ExhibitionUIAssets>("Assets/UI/ExhibitionUIAssets.asset");
        library.defaultThai=File.ReadAllText("Assets/StreamingAssets/Lang_Th.json");
        library.defaultEnglish=File.ReadAllText("Assets/StreamingAssets/Lang_En.json");
        var document=JsonUtility.FromJson<ImageLanguageDocument>(library.defaultThai);
        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssetIfDirty(library);
        var validation=new Newtonsoft.Json.Linq.JArray();
        foreach (var path in new[]{"Assets/Scenes/0_Idle.unity","Assets/Scenes/1_Game.unity","Assets/Scenes/2_Result.unity"})
        {
            var scene=SceneManager.GetSceneByPath(path);
            bool opened=!scene.IsValid() || !scene.isLoaded;
            if(opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            try
            {
                var localization=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<LocalizedImageSet>(true)).Single();
                var video=localization.transform.Find("ExplanationVideo");
                string Snapshot()=>video ? string.Join("\n",video.GetComponentsInChildren<Component>(true).Where(c=>c).Select(EditorJsonUtility.ToJson)) : "";
                string videoBefore=Snapshot();
                var serialized=new SerializedObject(localization);
                var bindings=serialized.FindProperty("bindings");
                int changed=0;
                for(int i=0;i<bindings.arraySize;i++)
                {
                    var binding=bindings.GetArrayElementAtIndex(i);
                    string id=binding.FindPropertyRelative("id").stringValue;
                    if(id.StartsWith("video.",StringComparison.Ordinal))continue;
                    var entry=document.images.Single(e=>e.id==id);
                    var image=(UnityEngine.UI.Image)binding.FindPropertyRelative("image").objectReferenceValue;
                    image.sprite=library.Find(entry.sprite);
                    if(image.rectTransform.pivot!=new Vector2(0,1))throw new InvalidOperationException("Unexpected localized image pivot: "+id);
                    image.rectTransform.anchoredPosition=new Vector2(entry.x,-entry.y);
                    image.rectTransform.sizeDelta=new Vector2(entry.width,entry.height);
                    changed++;
                }
                var board=localization.transform.Find("GroupSelection/GroupBoard") as RectTransform;
                if(board)board.anchoredPosition=new Vector2(board.anchoredPosition.x,-274);
                bool videoUnchanged=Snapshot()==videoBefore;
                if(!videoUnchanged)throw new InvalidOperationException("Video presentation changed during the UI refresh.");
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
                validation.Add(Newtonsoft.Json.Linq.JObject.FromObject(new{scene=path,localizedImages=changed,videoHierarchyUnchanged=videoUnchanged}));
            }
            finally{if(opened)EditorSceneManager.CloseScene(scene,true);}
        }
        SceneManager.SetActiveScene(previous);
        File.WriteAllText("docs/validation/ui_refresh2/editor_apply.json",validation.ToString());
        return "Updated 24 localized image references/rects and the group board offset; kept all presentation components and the video hierarchy.";
    }
}
