// Run once in Edit mode after extract_slot_labels.py. Extends 1_Game in place; never rebuilds the exhibit.
if (Application.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
    throw new Exception("Stop Play mode and preserve pending scene edits first.");
var library = AssetDatabase.LoadAssetAtPath<Exhibition.UI.ExhibitionUIAssets>("Assets/UI/ExhibitionUIAssets.asset");
var entries = new System.Collections.Generic.List<Exhibition.UI.ExhibitionUIAssets.Entry>(library.sprites);
var assets = new[] { "UIs_En/GroupSelect/GroupSlotLabel_en", "UIs_Th/GroupSelect/GroupSlotLabel_th", "UIs_En/SelectAll/ProductSlotLabel_en",
    "UIs_Th/SelectAll/ProductSlotLabel_th", "UIs_En/SelectAll/ColorSlotLabel_en", "UIs_Th/SelectAll/ColorSlotLabel_th", "UIs_Common/SelectGroupPanel/SlotLabelCover" };
AssetDatabase.Refresh();
foreach (string asset in assets)
{
    string path = "Assets/UI/" + asset + ".png";
    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
    var importer = (TextureImporter)AssetImporter.GetAtPath(path);
    importer.textureType = TextureImporterType.Sprite;
    importer.spriteImportMode = SpriteImportMode.Single;
    importer.mipmapEnabled = false;
    importer.alphaIsTransparency = true;
    importer.npotScale = TextureImporterNPOTScale.None;
    importer.textureCompression = TextureImporterCompression.Uncompressed;
    importer.wrapMode = TextureWrapMode.Clamp;
    importer.maxTextureSize = 2048;
    importer.SaveAndReimport();
    string id = System.IO.Path.GetFileName(asset);
    entries.RemoveAll(e => e.id == id);
    entries.Add(new Exhibition.UI.ExhibitionUIAssets.Entry { id = id, sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path) });
}
library.sprites = entries.ToArray();
library.defaultThai = System.IO.File.ReadAllText("Assets/StreamingAssets/Lang_Th.json");
library.defaultEnglish = System.IO.File.ReadAllText("Assets/StreamingAssets/Lang_En.json");
EditorUtility.SetDirty(library);
AssetDatabase.SaveAssetIfDirty(library);

const string scenePath = "Assets/Scenes/1_Game.unity";
System.IO.Directory.CreateDirectory("docs/validation/ui_attention/before");
System.IO.File.Copy(scenePath, "docs/validation/ui_attention/before/1_Game.unity", true);
var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
var localization = UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.LocalizedImageSet>(FindObjectsInactive.Include);
var root = localization.transform;
var serialized = new SerializedObject(localization);
var bindings = serialized.FindProperty("bindings");
const float inset = .12f;
float beyond = inset / (1 - 2 * inset);
Func<Transform, string, string, UnityEngine.UI.Image> image = (parent, name, sprite) =>
{
    var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
    go.layer = parent.gameObject.layer;
    go.transform.SetParent(parent, false);
    var result = go.GetComponent<UnityEngine.UI.Image>();
    result.sprite = library.Find(sprite);
    result.raycastTarget = false;
    return result;
};
Action<string, string, string> add = (path, id, sprite) =>
{
    var frame = root.Find(path);
    if (!frame) throw new Exception("Missing slot: " + path);
    if (frame.Find("SlotLabelCover")) throw new Exception("Slot labels already authored.");
    var cover = image(frame, "SlotLabelCover", "SlotLabelCover");
    cover.rectTransform.anchorMin = Vector2.one * inset;
    cover.rectTransform.anchorMax = Vector2.one * (1 - inset);
    cover.rectTransform.offsetMin = cover.rectTransform.offsetMax = Vector2.zero;
    cover.transform.SetAsFirstSibling();
    var label = image(cover.transform, "SlotLabel", sprite);
    label.rectTransform.pivot = Vector2.one * .5f;
    label.rectTransform.anchorMin = Vector2.one * -beyond;
    label.rectTransform.anchorMax = Vector2.one * (1 + beyond);
    label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
    int index = bindings.arraySize;
    bindings.InsertArrayElementAtIndex(index);
    var binding = bindings.GetArrayElementAtIndex(index);
    binding.FindPropertyRelative("id").stringValue = id;
    binding.FindPropertyRelative("image").objectReferenceValue = label;
    binding.FindPropertyRelative("keepObjectName").boolValue = true;
};
add("GroupSelection/GroupFrame0", "group.slotLabel", "GroupSlotLabel_th");
add("ProductAndColors/SelectionFrame0", "selection.groupSlotLabel", "GroupSlotLabel_th");
add("ProductAndColors/SelectionFrame1", "selection.productSlotLabel", "ProductSlotLabel_th");
add("ProductAndColors/SelectionFrame2", "selection.colorSlot1Label", "ColorSlotLabel_th");
add("ProductAndColors/SelectionFrame3", "selection.colorSlot2Label", "ColorSlotLabel_th");
serialized.ApplyModifiedPropertiesWithoutUndo();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/0_Idle.unity");
return "Authored 5 slot labels; bindings=" + bindings.arraySize + ".";
