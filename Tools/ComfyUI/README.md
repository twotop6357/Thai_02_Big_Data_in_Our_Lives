# Current: reference illustration / STYLE-QUALITY-003

FLUX.2 Klein 4B FP8, 640px, 4 steps, Euler/Flux2Scheduler, original user illustration as a grayscale style reference. One generation, actual BiRefNet alpha, strict source completeness checks and centered480RGBA. Four age designs and actual block-circle HEX/RGB are preserved. The reference controls art style, not block-resource outlines or final coloring.

- Download model.json into diffusion_models, text_encoder.json into text_encoders, vae.json into vae, background_model.json into background_removal using download_model.py; all sources are pinned and hashed.
- build_workflow.py exports the UI/API/input graphs. Keep the StreamingAssets API in sync. Node36 RandomNoise owns the session seed; node5 SamplerCustomAdvanced uses BasicGuider/ReferenceLatent and must not be patched as a KSampler.
- prepare_style_reference.py verifies/uploads the immutable packaged reference. benchmark.py calls it automatically. The app does the equivalent verification and never overwrites a conflicting server input.
- quality/balanced/fast all retain the validated4-step schedule. No unreviewed2-step shortcut or automatic photo-model fallback.
- test_workflow.py checks graph/UI/seed/palette/all1440 prompt combinations. review_style_quality.py makes diagnostic sheets; iterate_style_quality.py contains isolated, non-production exploration graphs.
- sync_block_references.py does not activate block-shape guides for this mode. Model/code/PNG provenance and actual quality/lifecycle/performance evidence: [STYLE-QUALITY-003](../../docs/validation/comfy_style3/README.md). Deployment instructions: [COMFYUI_SETUP](../../docs/COMFYUI_SETUP.md).
- Runtime strict validation rejects edge-clipped/faint or substantially disconnected output before showing it; it is not an exhaustive semantic/style classifier. Failed output returns to the existing retry flow.

[Apache2.0 license](licenses/flux2-klein-LICENSE.md). Historical model notices are retained below.

---

# Historical: four-step SDXL-Turbo / COMFY-FAST-002

Active: SDXL-Turbo fp16 at512px,4steps,CFG1,SDTurboScheduler+Euler ancestral → one VAE decode → BiRefNet →480RGBA → largest connected foreground and centered framing. No guide/control network, adapter, LoRA, second generation or style-refining pass. Block colors remain actual circle HEX/RGB metadata with short matching color words for the model. Original artwork/scenes remain unchanged.

[Setup](../../docs/COMFYUI_SETUP.md) / [actual evidence and timings](../../docs/validation/comfy_fast2/README.md). The previous34s reference-quality fallback is superseded by the user's latest simplified-illustration/speed requirement. ActualRTX3060 verification remains separate from measurements on5070Ti Laptop.

- model.json/background_model.json pin source/revision/SHA. Turbo is a low-step model; its fp16 checkpoint is6.94GB.
- build_workflow.py createsAPI/UI/inputJSON; syncAPI intoStreamingAssets. SamplerCustom uses noise_seed, not KSampler seed.
- quality/balanced/fast use4/3/2steps. TurboCFG1 ignores negative prompts; the graph skips an unused negative encoding.
- benchmark.py --warmup 1 --runs 20 --target-seconds 15: ten products, varied ages/color pairs, unique seeds.
- verify_visual_matrix.py + frame_visual_matrix.cs + review_visual_matrix.py: two seeds per product and fixed-seed4-age comparisons with actual app framing.
- sync_block_palette.py [--check] (Pillow): sample actual circle interiors. sync_block_references.py does not reactivate guides inTurbo mode.
- test_workflow.py (Python withPillow): graph/runtime-copy/input/palette/UI/Turbo schedule+seed contracts.
- download_model.py: immutable weights verification. Old guide installers/workflows and legacy_runtime are historical assets.

Powered by Stability AI. Model license/attribution: [LICENSE](licenses/sdxl-turbo-LICENSE.md) / [NOTICE](licenses/NOTICE.txt).
