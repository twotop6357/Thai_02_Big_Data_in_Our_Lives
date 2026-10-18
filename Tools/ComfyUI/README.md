# Product image workflow

Start with [the setup guide](../../docs/COMFYUI_SETUP.md).

- [Import into ComfyUI](workflows/product_480.ui.json)
- [API graph for Unity integration](workflows/product_480.api.json)
- [Example four-input selection](workflows/product_480.inputs.json)
- `presets.json`: audience themes, product parts, named colors and speed profiles.
- `model.json`: selected checkpoint, pinned source and SHA-256.
- `build_workflow.py`: generate a workflow from four selections without an LLM/custom node.
- `download_model.py`: install or verify the selected checkpoint in an explicit checkpoints folder.
- `benchmark.py`: measure request-to-PNG latency, with different seeds and separate warmup.
- `test_workflow.py`: offline prompt/graph/input tests.

The working baseline model is **SDXL Base 1.0**, 768x768 internal, 480x480 PNG output, 16 steps by default.
RTX 3060 timing is a target pending measurement. See `validation/current_pc.json` for the actual measured GPU.

Style target: polished subculture mobile RPG inventory illustration. This is a runnable prototype; visual consistency is not accepted. All four measured samples, including failures, are retained in `validation/`.
