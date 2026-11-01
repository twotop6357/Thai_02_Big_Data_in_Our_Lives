import unittest
import itertools
from build_workflow import build, read_json


class WorkflowTests(unittest.TestCase):
    def test_runtime_workflow_matches_current_builder(self):
        from pathlib import Path
        import json
        runtime = Path(__file__).resolve().parents[2] / "Assets/StreamingAssets/BlockGeneration/product_480.api.json"
        self.assertEqual(json.loads(runtime.read_text(encoding="utf-8")), build()[0])

    def test_palette_matches_block_circle_pixels(self):
        from sync_block_palette import sync
        palette = sync(check=True)
        for first in palette:
            for second in palette:
                api, _, selection = build(color1=first, color2=second)
                self.assertIn(palette[first], api["12"]["_meta"]["color_reference"])
                self.assertIn(palette[second], api["13"]["_meta"]["color_reference"])
                self.assertEqual(selection["primary_hex"], palette[first])
                self.assertEqual(selection["secondary_hex"], palette[second])

    def test_all_input_combinations_and_prompt_assembly(self):
        presets = read_json("presets.json")
        for group, product, first, second in itertools.product(
                presets["groups"], presets["products"], presets["colors"], presets["colors"]):
            api, _, selection = build(group, product, first, second)

            def evaluate(key):
                node = api[key]
                if node["class_type"] == "PrimitiveString":
                    return node["inputs"]["value"]
                data = node["inputs"]
                a, b = data["string_a"], data["string_b"]
                return (evaluate(a[0]) if isinstance(a, list) else a) + data["delimiter"] + (evaluate(b[0]) if isinstance(b, list) else b)

            self.assertEqual(selection["positive_prompt"], evaluate("18"))
            self.assertIn(first, api["12"]["inputs"]["value"])
            self.assertIn(second, api["13"]["inputs"]["value"])

    def test_ui_links_reference_correct_inputs_and_outputs(self):
        api, ui, _ = build()
        nodes = {node["id"]: node for node in ui["nodes"]}
        self.assertEqual(len(nodes), len(api))
        self.assertEqual(ui["last_node_id"], max(nodes))
        for link, origin, source_slot, target, target_slot, kind in ui["links"]:
            self.assertIn(link, nodes[origin]["outputs"][source_slot]["links"])
            self.assertEqual(nodes[target]["inputs"][target_slot]["link"], link)
            self.assertEqual(nodes[origin]["outputs"][source_slot]["type"], kind)
        self.assertEqual(api["4"]["inputs"]["width"], 640)
        self.assertEqual(api["8"]["inputs"]["width"], 480)
        self.assertEqual(api["7"]["inputs"]["images"], ["8", 0])

    def test_invalid_inputs_are_rejected(self):
        for inputs in ({"group": "unknown"}, {"product": "unknown"}, {"color1": "unknown"},
                       {"profile": "unknown"}, {"seed": -1}, {"seed": 2 ** 64}):
            with self.assertRaises(ValueError):
                build(**inputs)

    def test_audience_subject_replaces_generic_shape_and_color_parts(self):
        presets = read_json("presets.json")
        for product, item in presets["products"].items():
            subjects = set()
            self.assertEqual(len(item["ageVariants"]), 4)
            for design in item["ageVariants"]:
                api, _, selection = build(design["groupId"], product, "red", "blue")
                subjects.add(api["11"]["inputs"]["value"])
                self.assertEqual(api["11"]["inputs"]["value"], design["prompt"])
                self.assertTrue(selection["positive_prompt"].startswith(design["prompt"] + ". "))
                self.assertEqual(api["12"]["inputs"]["value"], "Main color #EC6868 (soft red) on " + design["primaryPart"])
                self.assertEqual(api["13"]["inputs"]["value"], "Accent color #5FB0E3 (light blue) on " + design["secondaryPart"])
            self.assertEqual(len(subjects), 4, product)
        for age, keyword in (("child", "kindergarten"), ("student", "beanie"),
                             ("working_adult", "baseball cap"), ("senior", "fedora")):
            api = build(age, "hats")[0]
            self.assertIn(keyword, api["11"]["inputs"]["value"])
        self.assertNotIn("brim", build("student", "hats")[0]["13"]["inputs"]["value"])

    def test_profiles_and_fixed_seed(self):
        for profile in ("fast", "balanced", "quality"):
            api, _, _ = build(profile=profile, seed=123)
            self.assertEqual(api["29"]["inputs"]["steps"], 4)
            self.assertEqual(api["29"]["class_type"], "Flux2Scheduler")
            self.assertEqual(api["36"]["inputs"]["noise_seed"], 123)
            self.assertEqual(api["5"]["class_type"], "SamplerCustomAdvanced")
            self.assertEqual(api["5"]["inputs"]["noise"], ["36", 0])
            self.assertEqual(api["5"]["inputs"]["guider"], ["37", 0])
            self.assertNotIn("seed", api["5"]["inputs"])
            self.assertNotIn("positive", api["5"]["inputs"])
            self.assertEqual(api["37"]["inputs"]["conditioning"], ["35", 0])
            self.assertEqual(api["4"]["inputs"]["batch_size"], 1)

    def test_original_style_reference_and_alpha_are_connected_without_pose_or_photo_fallback(self):
        from pathlib import Path
        import hashlib
        ref=read_json("model.json")["style_reference"]
        path=Path(__file__).resolve().parents[2]/"Assets/StreamingAssets/BlockGeneration/StyleReferences"/ref["filename"]
        self.assertEqual(hashlib.sha256(path.read_bytes()).hexdigest(), ref["sha256"])
        for product in read_json("presets.json")["products"]:
            api, _, selection = build(product=product)
            self.assertEqual(api["1"]["class_type"], "UNETLoader")
            self.assertEqual(api["1"]["inputs"]["unet_name"], "flux-2-klein-4b-fp8.safetensors")
            self.assertEqual(api["32"]["inputs"]["image"], ref["subfolder"]+"/"+ref["filename"])
            self.assertEqual(api["35"]["inputs"]["latent"], ["34", 0])
            self.assertEqual(api["34"]["inputs"]["pixels"], ["33", 0])
            self.assertEqual(api["33"]["inputs"]["image"], ["39", 0])
            self.assertEqual(api["39"]["inputs"]["mask"], ["38", 0])
            self.assertEqual(api["38"]["inputs"]["image"], ["32", 0])
            self.assertEqual(api["6"]["inputs"]["vae"], ["31", 0])
            self.assertFalse(any(n["class_type"] in ("ControlNetLoader","Canny","ControlNetApplyAdvanced","CheckpointLoaderSimple") for n in api.values()))
            self.assertIn("Entirely hand-drawn flat illustration", selection["positive_prompt"])
            self.assertEqual(api["21"]["inputs"]["image"], ["6", 0])
            self.assertEqual(api["22"]["inputs"]["mask"], ["21", 0])
            self.assertEqual(api["23"]["inputs"]["alpha"], ["22", 0])
            self.assertEqual(api["8"]["inputs"]["image"], ["23", 0])



if __name__ == "__main__":
    unittest.main()
