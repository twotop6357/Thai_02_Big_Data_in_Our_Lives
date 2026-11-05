"""Native Comfy nodes: protect painted white/color surfaces against a neutral-gray matte."""


def alpha_nodes():
    def node(kind, title, **inputs):
        return {"class_type": kind, "inputs": inputs, "_meta": {"title": title}}
    def combine(destination, source, operation, title):
        return node("MaskComposite", title, destination=[str(destination), 0], source=[str(source), 0], x=0, y=0, operation=operation)
    return {
        "40": node("ImageToMask", "Original red channel for opacity protection", image=["6", 0], channel="red"),
        "41": node("ImageToMask", "Original green channel for opacity protection", image=["6", 0], channel="green"),
        "42": node("ImageToMask", "Original blue channel for opacity protection", image=["6", 0], channel="blue"),
        "43": combine(40, 41, "multiply", "White surface support R x G"),
        "44": combine(43, 42, "multiply", "White surface support R x G x B"),
        "45": node("ThresholdMask", "Protect white and pale white shading, retain neutral-gray background", mask=["44", 0], value=0.65),
        "46": combine(40, 41, "subtract", "Positive red-green chroma"),
        "47": combine(41, 40, "subtract", "Positive green-red chroma"),
        "48": combine(40, 42, "subtract", "Positive red-blue chroma"),
        "49": combine(42, 40, "subtract", "Positive blue-red chroma"),
        "50": combine(46, 47, "add", "Absolute red-green chroma"),
        "51": combine(48, 49, "add", "Absolute red-blue chroma"),
        "52": combine(50, 51, "add", "Color surface support, independent of chosen hue"),
        "53": node("ThresholdMask", "Protect colored lenses, pads and accents", mask=["52", 0], value=0.12),
        "54": combine(45, 53, "add", "Painted white or colored material"),
        "61": node("ThresholdMask", "Exclude neutral-gray backdrop from dark material", mask=["44", 0], value=0.015),
        "62": node("InvertMask", "Protect opaque black and dark shaded material", mask=["61", 0]),
        "63": combine(54, 62, "add", "White, colored or dark material, excluding neutral backdrop"),
        "55": combine(21, 63, "add", "Restore material incorrectly removed by segmentation"),
        "56": node("StringContains", "Bicycle wheel and frame openings stay transparent", string=["11", 0], substring="bicycle", case_sensitive=False),
        "57": node("ComfySwitchNode", "Keep bicycle topology; protect solid surfaces for other items", switch=["56", 0], on_false=["55", 0], on_true=["21", 0]),
        "58": node("ThresholdMask", "Reliable foreground, excluding faint cast-shadow probabilities", mask=["57", 0], value=0.5),
        "59": node("GrowMask", "Solid interior with one-pixel original edge", mask=["58", 0], expand=-1, tapered_corners=True),
        "60": combine(21, 59, "add", "Opaque material interior with unchanged original edge alpha")
    }
