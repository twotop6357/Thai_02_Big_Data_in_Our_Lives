# Retired ComfyUI implementation — 2026-10-22

This directory is a development archive, outside Unity Assets and excluded from the Player build. The application no longer starts, probes or calls ComfyUI, uploads guides, downloads server images, or sweeps its output directory. Do not use the historical commands below as current application setup.

`legacy_runtime/Assets/` preserves the removed scripts, metadata, settings, API graph, control guides and retention tests, including uncommitted QUALITY-001 work. External ComfyUI installations, model weights and generated files were not changed. Existing tools/results/validation are historical evidence; scripts are manual tools with old path assumptions and are not an active replacement backend.

Current operation: [BLOCK_SIMULATOR](../../docs/BLOCK_SIMULATOR.md). A replacement generator has not been configured.

---

# Product image workflow

See [setup and operation](../../docs/COMFYUI_SETUP.md).

- [ComfyUI UI workflow](workflows/product_480.ui.json) / [API graph](workflows/product_480.api.json).
- Animagine XL4.0 Opt, 1024 internal, 480 RGBA output, quality28 steps/CFG5/Euler ancestral.
- Canny Control-LoRA rank256 with14 original product guides; BiRefNet removes background with real alpha.
- model.json/background_model.json/control_model.json pin source revisions, sizes and SHA256.
- presets.json reads the runtime BlockCatalog.json; build_workflow.py regenerates API/UI from the four choices.
- install_control_guides.py uploads the bundled guides without overwriting different inputs. Unity installs the selected guide automatically.
- create_control_guides.py regenerates the original geometric guides (Pillow required); normal use needs no Pillow.
- download_model.py --manifest <manifest> --models-dir <model-specific directory> installs/verifies models.
- benchmark.py --profile quality measures unique seeds with separate warmup. Install guides first.
- python -m unittest discover -s Tools/ComfyUI -p test_workflow.py validates offline graph/input contracts.

[QUALITY-001 evidence](validation/quality/README.md) separates actual current-PC tests from pending RTX3060/Player/long-run verification. Older validation/current_pc.json is the previous SDXL baseline, not current timing. Single objects, exact colors and perfect alpha edges across every seed are not guaranteed.
