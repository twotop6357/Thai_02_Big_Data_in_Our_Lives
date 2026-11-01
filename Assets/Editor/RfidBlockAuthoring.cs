using System;
using System.Collections.Generic;
using System.IO;
using Core;
using Exhibition.Blocks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RfidBlockAuthoring
{
    private static readonly string[] Groups = { "child:Child", "student:Student", "working_adult:WorkingAdult", "senior:Senior" };
    private static readonly string[] Products = { "bags:Bag", "shoes:Shoes", "hats:Hat", "sunglasses:Sunglasses", "headphones:Headphones",
        "tops:Top", "bicycles:Bicycle", "tumblers:Tumbler", "wristwatches:WristWatch", "umbrellas:Umbrella" };
    private static readonly string[] Colors = { "black:Black", "white:White", "blue:Blue", "red:Red", "green:Green", "yellow:Yellow" };

    public static string Configure()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Stop Play mode before authoring RFID support.");
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scene changes before authoring RFID support.");
        string previous = SceneManager.GetActiveScene().path;
        const string path = "Assets/Resources/RfidBlockArtwork.asset";
        var catalog = AssetDatabase.LoadAssetAtPath<BlockArtworkCatalog>(path);
        if (!catalog) { catalog = ScriptableObject.CreateInstance<BlockArtworkCatalog>(); AssetDatabase.CreateAsset(catalog, path); }
        var entries = new List<BlockArtworkCatalog.Entry>();
        Add(entries, Groups, BlockKind.Group); Add(entries, Products, BlockKind.Product); Add(entries, Colors, BlockKind.Color);
        catalog.entries = entries.ToArray(); EditorUtility.SetDirty(catalog);
        var idle = EditorSceneManager.OpenScene("Assets/Scenes/0_Idle.unity");
        var environment = GameObject.Find("RFIDTestEnvironment");
        if (!environment) environment = new GameObject("RFIDTestEnvironment");
        var hub = environment.GetComponent<RfidReaderHub>() ?? environment.AddComponent<RfidReaderHub>();
        var fields = new SerializedObject(hub); fields.FindProperty("artwork").objectReferenceValue = catalog;
        fields.ApplyModifiedPropertiesWithoutUndo();
        if (!environment.GetComponent<RfidSimulator>()) environment.AddComponent<RfidSimulator>();
        EditorSceneManager.MarkSceneDirty(idle); EditorSceneManager.SaveScene(idle);
        var game = EditorSceneManager.OpenScene("Assets/Scenes/1_Game.unity");
        var controller = UnityEngine.Object.FindFirstObjectByType<BlockExperienceController>();
        fields = new SerializedObject(controller);
        fields.FindProperty("enableEditorKeyboard").boolValue = false;
        fields.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(game); EditorSceneManager.SaveScene(game);
        AssetDatabase.SaveAssets();
        if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous);
        return "Mapped " + entries.Count + " block sprites and connected the persistent F9 RFID test environment.";
    }

    private static void Add(List<BlockArtworkCatalog.Entry> entries, string[] mappings, BlockKind kind)
    {
        foreach (string mapping in mappings)
        {
            string[] parts = mapping.Split(':');
            string path = "Assets/UI/Blocks/" + parts[1] + "Block.png";
            if (!File.Exists(path)) continue;
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.npotScale = TextureImporterNPOTScale.None; importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (!sprite) throw new InvalidOperationException("Block sprite could not be imported: " + path);
            entries.Add(new BlockArtworkCatalog.Entry { kind = kind, id = parts[0], sprite = sprite });
        }
    }
}
