"""Build viewable product/age contact sheets and verify fixed-input age diversity; images stay unchanged."""
import argparse
import hashlib
import itertools
import json
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageChops, ImageStat

NAMES = {"bags":"Bags", "shoes":"Shoes", "hats":"Bucket hat", "sunglasses":"Sunglasses", "headphones":"Headphones",
         "tops":"T-shirt", "bicycles":"Bicycle", "tumblers":"Tumbler", "wristwatches":"Wristwatch", "umbrellas":"Umbrella"}
AGES = ("child", "student", "working_adult", "senior")


def visible(path):
    image = Image.open(path).convert("RGBA")
    result = Image.new("RGBA",image.size,"white"); result.alpha_composite(image)
    return result.convert("RGB")


def sheet(root, images, cols, filename, width=280):
    out=Image.new("RGB",(cols*width,((len(images)+cols-1)//cols)*(width+42)),(247,245,249))
    draw=ImageDraw.Draw(out); font=ImageFont.truetype("C:/Windows/Fonts/malgun.ttf",18)
    for i,(name,label) in enumerate(images):
        path=root/"framed"/name
        if not path.exists(): continue
        image=Image.open(path).convert("RGBA");image.thumbnail((width-16,width-16),Image.Resampling.LANCZOS)
        x=(i%cols)*width+(width-image.width)//2;y=(i//cols)*(width+42)+8
        out.paste(image,(x,y),image);draw.text(((i%cols)*width+16,(i//cols)*(width+42)+width+4),label,fill=(50,57,68),font=font)
    out.save(root/filename)


def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument("root",type=Path);args=parser.parse_args()
    report=json.loads((args.root/"report.json").read_text())
    source={r["local_image"]:r for r in report["results"]}
    for seed,label in ((1206501,"first"),(1206502,"second")):
        images=[(f"{p}_student_{seed}.png",name) for p,name in NAMES.items()]
        sheet(args.root,images,5,"products_"+label+".png")
    sheet(args.root,[(f"{p}_{age}_1206501.png",NAMES[p]+" / "+age.replace("_"," ").title()) for p in ("bags","shoes") for age in AGES],
          4,"age_comparison.png")
    audit={"products":{},"ages":{},"completed":report.get("completed",False),"technical_failures":report["failed"]}
    for product in NAMES:
        names=[f"{product}_student_{seed}.png" for seed in (1206501,1206502)]
        audit["products"][product]={"files":names,"both_present":all((args.root/"framed"/n).is_file() for n in names)}
    for product in ("bags","shoes"):
        names=[f"{product}_{age}_1206501.png" for age in AGES]
        if not all((args.root/"framed"/n).exists() for n in names): continue
        inputs=[source[n]["selection"] for n in names]
        fixed=all(x["seed"]==1206501 and x["product"]==product and x["color1"]=="red" and x["color2"]=="blue" for x in inputs)
        hashes=[hashlib.sha256((args.root/"framed"/n).read_bytes()).hexdigest() for n in names]
        differences={}
        for a,b in itertools.combinations(range(4),2):
            difference=ImageChops.difference(visible(args.root/"framed"/names[a]),visible(args.root/"framed"/names[b]))
            differences[AGES[a]+" vs "+AGES[b]]=sum(ImageStat.Stat(difference).mean)/3
        audit["ages"][product]={"same_object_colors_seed":fixed,"distinct_images":len(set(hashes)),"mean_visible_rgb_difference":differences,
                                 "files":names}
    (args.root/"visual_audit.json").write_text(json.dumps(audit,indent=2))
    print(json.dumps({"products_present":sum(v["both_present"] for v in audit["products"].values()),"ages":audit["ages"],
                      "technical_failures":len(report["failed"]),"completed":report.get("completed",False)},indent=2))


if __name__=="__main__": main()
