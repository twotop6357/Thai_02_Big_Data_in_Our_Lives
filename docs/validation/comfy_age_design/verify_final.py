import sys,json,hashlib,statistics
from pathlib import Path
sys.path.insert(0,'Tools/ComfyUI')
from build_workflow import build,read_json
from verify_visual_matrix import case_hash,config_hash
from benchmark import http
from PIL import Image
import numpy as np
out=Path('docs/validation/comfy_age_design')
rows=json.loads((out/'csharp_prompts.json').read_text(encoding='utf-8-sig'))
for row in rows:
    api,_,s=build(row['groupId'],row['productId'],row['color1'],row['color2'])
    pairs={'positive':s['positive_prompt'],'primaryHex':s['primary_hex'],'secondaryHex':s['secondary_hex'],'audienceFragment':api['10']['inputs']['value'],'productFragment':api['11']['inputs']['value'],'primaryFragment':api['12']['inputs']['value'],'secondaryFragment':api['13']['inputs']['value'],'primaryReference':api['12']['_meta']['color_reference'],'secondaryReference':api['13']['_meta']['color_reference'],'negative':api['3']['inputs']['text']}
    for key,expected in pairs.items(): assert row[key]==expected,(row['groupId'],row['productId'],key)
assert len(rows)==1440
current=config_hash();products=read_json('presets.json')['products'];ages=list(read_json('presets.json')['groups'])
verified=[]
for folder in ('approved','second_seed_final'):
    path=out/folder;report=json.loads((path/'report.json').read_text())
    assert report['completed'] and report['config_sha256']==current
    assert len(report['results'])==40
    assert {(r['selection']['product'],r['selection']['group']) for r in report['results']}=={(p,a) for p in products for a in ages}
    for r in report['results']:
        s=r['selection'];f=path/r['local_image']
        assert r['api_sha256']==case_hash(s['product'],s['group'],s['seed'])
        assert hashlib.sha256(f.read_bytes()).hexdigest()==r['png_sha256']
        assert s['positive_prompt']==build(s['group'],s['product'],'red','blue',seed=s['seed'])[2]['positive_prompt']
        with Image.open(f) as im: assert im.mode=='RGBA' and im.size==(480,480);im.verify()
        with Image.open(path/'framed'/f.name) as im:
            data=np.asarray(im);yy,xx=np.where(data[:,:,3]>=8)
            assert len(xx) and abs((xx.min()+xx.max()+1)/2-240)<=1 and abs((yy.min()+yy.max()+1)/2-240)<=1
            assert xx.min()>=23 and yy.min()>=23 and xx.max()<457 and yy.max()<457
    with Image.open(path/'all_products_by_age.png') as im: assert im.size==(2248,5330);im.verify()
    times=[r['seconds_request_to_png'] for r in report['results']]
    verified.append({'folder':folder,'seed':report['seed'],'actual_output_cases':40,'api_sha_matches_current':40,'framing_center_and_margin_pass':40,'median_seconds':statistics.median(times),'max_seconds':max(times),'visual_review':'Both halves inspected. Seed 1206502 is the representative sheet; 1206501 retains clutter/cropping in some products. No per-cell seed selection.'})
(out/'preview.png').write_bytes((out/'approved/all_products_by_age.png').read_bytes())
stats=http('http://127.0.0.1:8188','/system_stats')
report={'task':'AGE-DESIGN-002','config_sha256':current,'distinct_age_subjects':40,'csharp_offline_compile':True,'csharp_python_prompt_parity_combinations':len(rows),'rejection_paths':6,'python_tests':8,'unity_editor_compile_and_play':False,'unity_connector_error':'Transport closed','actual_output_checks':verified,'measured_devices':[d['name'] for d in stats['devices']],'target_rtx3060_measured':False,'preview_sha256':hashlib.sha256((out/'preview.png').read_bytes()).hexdigest(),'preview_size':[2248,5330],'known_visual_limits':['Some seeds create repeated objects or clipped garments, bands or wheels.','Selected colors are conveyed, not enforced pixel by pixel.','Student folding umbrella may render open.'],'framing_execution':'Exact production C# array helper in a separate Add-Type process with Color32 byte shim. Initial duplicate shim type error was fixed by using a fresh process. No Unity native API/JsonUtility/Play validation.'}
(out/'verification.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'parity':len(rows),'actual_output_cases':80,'sets':verified,'preview':str(out/'preview.png')},ensure_ascii=False,indent=2))
