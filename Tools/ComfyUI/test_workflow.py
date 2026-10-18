import unittest
import itertools
from build_workflow import build, read_json


class WorkflowTests(unittest.TestCase):
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
        self.assertEqual(api["4"]["inputs"]["width"], 768)
        self.assertEqual(api["8"]["inputs"]["width"], 480)
        self.assertEqual(api["7"]["inputs"]["images"], ["8", 0])

    def test_invalid_inputs_are_rejected(self):
        for inputs in ({"group": "unknown"}, {"product": "unknown"}, {"color1": "unknown"},
                       {"profile": "unknown"}, {"seed": -1}, {"seed": 2 ** 64}):
            with self.assertRaises(ValueError):
                build(**inputs)

    def test_profiles_and_fixed_seed(self):
        for profile, steps in (("fast", 12), ("balanced", 16), ("quality", 20)):
            api, _, _ = build(profile=profile, seed=123)
            self.assertEqual(api["5"]["inputs"]["steps"], steps)
            self.assertEqual(api["5"]["inputs"]["seed"], 123)
            self.assertEqual(api["4"]["inputs"]["batch_size"], 1)


if __name__ == "__main__":
    unittest.main()
