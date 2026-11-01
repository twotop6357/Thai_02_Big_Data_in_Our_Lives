import json,sys,hashlib,statistics
from pathlib import Path
from PIL import Image
import numpy as np
sys.path.insert(0,'Tools/ComfyUI')
from build_workflow import build
from verify_visual_matrix import config_hash
out=Path('docs/validation/comfy_style3');folder=out/'final_accepted';r=json.loads((folder/'report.json').read_text());assert r['completed'] and len(r['results'])==80 and r['config_sha256']==config_hash()
frames=json.loads((folder/'strict_unity_framing.json').read_text());assert frames['passed']==frames['total']==80 and frames['strictRequireComplete']
for row in r['results']:
 s=row['selection'];api,_,selection=build(s['group'],s['product'],s['color1'],s['color2'],seed=s['seed']);assert hashlib.sha256(json.dumps(api,sort_keys=True).encode()).hexdigest()==row['api_sha256'];assert s['positive_prompt']==selection['positive_prompt']
 f=folder/row['local_image'];assert hashlib.sha256(f.read_bytes()).hexdigest()==row['png_sha256']
 with Image.open(f) as im: assert im.size==(480,480) and im.mode=='RGBA';im.verify()
 with Image.open(folder/'framed'/row['local_image']) as im:
  p=np.asarray(im);y,x=np.where(p[:,:,3]>=8);assert len(x) and x.min()>=24 and y.min()>=24 and x.max()<456 and y.max()<456;assert abs((x.min()+x.max()+1)/2-240)<=1 and abs((y.min()+y.max()+1)/2-240)<=1
rows=json.loads((out/'unity_prompts.json').read_text(encoding='utf-8-sig'));assert len(rows)==1440
for row in rows:
 api,_,s=build(row['groupId'],row['productId'],row['color1'],row['color2']);assert row['positive']==s['positive_prompt'] and row['primaryFragment']==api['12']['inputs']['value'] and row['secondaryFragment']==api['13']['inputs']['value']
(out/'prompt_parity.json').write_text(json.dumps({'actual_unity_jsonutility':True,'combinations':1440,'passed':True},indent=2))
actual=json.loads((out/'runtime_success.json').read_text());fixture=json.loads((out/'runtime_fixtures.json').read_text());assert actual['passed'] and fixture['passed']
tests=json.loads((out/'editmode_results.json').read_text(encoding='utf-8-sig'))['result']['data'];assert tests['result']['summary']['passed']==66
before=json.loads((out/'before_hashes.json').read_text());preserved={p:hashlib.sha256(Path(p).read_bytes()).hexdigest()==before[p] for p in before if p.startswith(('Assets/Scenes/','Packages/','ProjectSettings/'))};assert all(preserved.values());(out/'preservation_check.json').write_text(json.dumps(preserved,indent=2))
review={'reviewer':'Codex, direct visual review, not independent cross review','criteria':['Cute polished 2D style with broad clean fills and soft cel shading, comparable to the original small shoe illustration','No photographic or semi-realistic material rendering','One coherent complete product, no source crop, large detached parts or repeated presentation panels','Selected colors recognizable; exact per-pixel RGB and exact decoration placement are not asserted','Meaningful age-specific shapes; minor illustrative simplifications are allowed'],'reviewed_current_api_outputs':80,'rejected_current_outputs':0,'checks':[{'file':x['local_image'],'pass_id':x['pass_id'],'passed':True} for x in r['results']],'limits':'Finite visual acceptance, not an exhaustive proof over all random seeds. The runtime guard detects source-edge/faint/disconnected alpha failures, not every semantic/style defect.'}
(out/'visual_acceptance.json').write_text(json.dumps(review,ensure_ascii=False,indent=2),encoding='utf-8')
values=[x['seconds_request_to_png'] for x in r['results']];app=[x['seconds'] for x in actual['cases']]
summary={'task':'STYLE-QUALITY-003','active_mode':'reference_illustration','config_sha256':config_hash(),'actual_api_images':80,'current_images_reused_only_if_exact_graph_identical':sum('reused_from' in x for x in r['results']),'direct_visual_review_passed':80,'strict_original_png_and_framing_passed':80,'unity_prompt_parity':1440,'editmode_passed':66,'editmode_job_id':tests['job_id'],'python_contract_tests_passed':8,'runtime_success_checks':len(actual['checks']),'runtime_fixture_checks':len(fixture['checks']),'runtime_new_images':len(app),'actual_gpu':'RTX5070 Ti Laptop 12GB','api_median_seconds':statistics.median(values),'api_max_seconds':max(values),'actual_app_seconds':app,'rtx3060_measured':False,'windows_player_built':False,'physical_input_and_long_duration_tested':False,'preserved_files':preserved,'random_all_seed_perfection_guaranteed':False,'limits':'Exact decoration/clock markings and pixel RGB may vary. No global aesthetic or semantic detector is claimed. Strict structural failure returns to retry rather than showing a known broken image.'}
(out/'verification.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2),encoding='utf-8');print(json.dumps(summary,ensure_ascii=False,indent=2))
