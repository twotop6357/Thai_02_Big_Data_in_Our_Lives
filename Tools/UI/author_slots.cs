// Run once in Edit mode after apply_slots_assets.py. Extends 1_Game in place; never rebuilds the exhibit.
if (Application.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
    throw new Exception("Stop Play mode and preserve pending scene edits first.");
var library = AssetDatabase.LoadAssetAtPath<Exhibition.UI.ExhibitionUIAssets>("Assets/UI/ExhibitionUIAssets.asset");
var entries = new System.Collections.Generic.List<Exhibition.UI.ExhibitionUIAssets.Entry>(library.sprites);
var slotAssets = new[] { "UIs_En/GroupSelect/GroupSlot_en", "UIs_Th/GroupSelect/GroupSlot_th", "UIs_En/SelectAll/ProductSlot_en",
    "UIs_Th/SelectAll/ProductSlot_th", "UIs_En/SelectAll/ColorSlot_en", "UIs_Th/SelectAll/ColorSlot_th" };
AssetDatabase.ImportAsset("Assets/UI/UIs_Common/SelectGroupPanel/BoardImage.png", ImportAssetOptions.ForceSynchronousImport);
foreach (string asset in slotAssets)
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
System.IO.File.Copy(scenePath, "docs/validation/ui_slots/before/1_Game.unity", true);
var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
var localization = UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.LocalizedImageSet>();
var view = UnityEngine.Object.FindFirstObjectByType<Exhibition.UI.BlockExperienceView>(FindObjectsInactive.Include);
var root = localization.transform;
var groupPanel = root.Find("GroupSelection");
var board = (RectTransform)groupPanel.Find("GroupBoard");
if (!board.Find("GroupFrame1")) throw new Exception("Slots already authored.");

Action<RectTransform, Vector2, float, float, float, float> place = (rect, pivot, x, y, w, h) =>
{
    rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
    rect.pivot = pivot;
    rect.sizeDelta = new Vector2(w, h);
    rect.anchoredPosition = new Vector2(x + w * pivot.x, -(y + h * (1 - pivot.y)));
};
Action<RectTransform, string> skin = (rect, sprite) =>
{
    var image = rect.GetComponent<UnityEngine.UI.Image>();
    image.sprite = library.Find(sprite);
    image.type = UnityEngine.UI.Image.Type.Simple;
    image.preserveAspect = false;
    image.color = Color.white;
    image.raycastTarget = false;
    EditorUtility.SetDirty(image);
};
var center = new Vector2(.5f, .5f);
var frame = (RectTransform)board.Find("GroupFrame0");
frame.SetParent(groupPanel, false);
frame.SetSiblingIndex(board.GetSiblingIndex() + 1);
for (int i = 1; i < 4; i++) UnityEngine.Object.DestroyImmediate(board.Find("GroupFrame" + i).gameObject);
place(board, new Vector2(0, 1), 708, 354, 504, 504);
skin(board, "BoardImage");
place(frame, center, 748, 394, 424, 424);
skin(frame, "GroupSlot_th");
var panel = root.Find("ProductAndColors");
string[] sprites = { "GroupSlot_th", "ProductSlot_th", "ColorSlot_th", "ColorSlot_th" };
string[] ids = { "selection.groupSlot", "selection.productSlot", "selection.colorSlot1", "selection.colorSlot2" };
var serialized = new SerializedObject(localization);
var bindings = serialized.FindProperty("bindings");
Action<string, RectTransform> bind = (id, rect) =>
{
    int index = bindings.arraySize;
    bindings.InsertArrayElementAtIndex(index);
    var binding = bindings.GetArrayElementAtIndex(index);
    binding.FindPropertyRelative("id").stringValue = id;
    binding.FindPropertyRelative("image").objectReferenceValue = rect.GetComponent<UnityEngine.UI.Image>();
    binding.FindPropertyRelative("keepObjectName").boolValue = true;
};
bind("group.board", board);
bind("group.slot", frame);
var slotObject = new SerializedObject(view);
var slots = slotObject.FindProperty("selectionSlots"); slots.arraySize = 4;
for (int i = 0; i < 4; i++)
{
    var rect = (RectTransform)panel.Find("SelectionFrame" + i);
    place(rect, center, 268 + i % 2 * 268, 320 + i / 2 * 268, 252, 252);
    skin(rect, sprites[i]);
    bind(ids[i], rect);
    slots.GetArrayElementAtIndex(i).objectReferenceValue = rect.GetComponent<Exhibition.UI.UIAnimatedElement>();
}
serialized.ApplyModifiedPropertiesWithoutUndo();
slotObject.FindProperty("groupSlot").objectReferenceValue = frame.GetComponent<Exhibition.UI.UIAnimatedElement>();
slotObject.ApplyModifiedPropertiesWithoutUndo();
// Drop the deleted frames from the entrance list instead of leaving missing references.
var presentation = root.GetComponent<Exhibition.UI.UIScenePresentation>();
var presentationObject = new SerializedObject(presentation);
var cards = presentationObject.FindProperty("cards");
for (int i = cards.arraySize - 1; i >= 0; i--)
    if (!cards.GetArrayElementAtIndex(i).objectReferenceValue)
    {
        cards.GetArrayElementAtIndex(i).objectReferenceValue = null;
        cards.DeleteArrayElementAtIndex(i);
    }
presentationObject.ApplyModifiedPropertiesWithoutUndo();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/0_Idle.unity");
return "Authored the single group slot and purpose-labelled selection slots; cards=" + cards.arraySize + ".";
