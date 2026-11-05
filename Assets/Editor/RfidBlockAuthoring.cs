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
        var catalog = ConfigureArtwork();
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
        return "Mapped " + catalog.entries.Length + " block sprites and connected the persistent F9 RFID test environment.";
    }

    // Artwork-only updates must not rewrite scenes or unrelated dirty assets.
    public static BlockArtworkCatalog ConfigureArtwork()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Stop Play mode before authoring block artwork.");
        var entries = new List<BlockArtworkCatalog.Entry>();
        Add(entries, Groups, BlockKind.Group); Add(entries, Products, BlockKind.Product); Add(entries, Colors, BlockKind.Color);
        const string path = "Assets/Resources/RfidBlockArtwork.asset";
        var catalog = AssetDatabase.LoadAssetAtPath<BlockArtworkCatalog>(path);
        if (!catalog) { catalog = ScriptableObject.CreateInstance<BlockArtworkCatalog>(); AssetDatabase.CreateAsset(catalog, path); }
        catalog.entries = entries.ToArray(); EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssetIfDirty(catalog);
        return catalog;
    }

    private static void Add(List<BlockArtworkCatalog.Entry> entries, string[] mappings, BlockKind kind)
    {
        foreach (string mapping in mappings)
        {
            string[] parts = mapping.Split(':');
            string english = "Assets/UI/Blocks/EnglishBlocks/" + parts[1] + "Block_Eng.png";
            string thai = "Assets/UI/Blocks/ThaiBlocks/" + parts[1] + "Block_Thai.png";
            entries.Add(new BlockArtworkCatalog.Entry { kind = kind, id = parts[0],
                sprite = ImportSprite(english), thaiSprite = kind != BlockKind.Group || File.Exists(thai) ? ImportSprite(thai) : null });
        }
    }

    private static Sprite ImportSprite(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Required block artwork is missing.", path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single ||
            importer.npotScale != TextureImporterNPOTScale.None || importer.mipmapEnabled || !importer.alphaIsTransparency ||
            importer.textureCompression != TextureImporterCompression.Uncompressed || importer.wrapMode != TextureWrapMode.Clamp)
        {
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.npotScale = TextureImporterNPOTScale.None; importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (!sprite) throw new InvalidOperationException("Block sprite could not be imported: " + path);
        return sprite;
    }
}
