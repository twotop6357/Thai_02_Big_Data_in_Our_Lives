"""Upload project control guides to ComfyUI. Never overwrite a differing existing input."""
import argparse,hashlib,json,urllib.request,urllib.error,urllib.parse,uuid
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--server',default='http://127.0.0.1:8188');a=p.parse_args()
guides=sorted((Path(__file__).resolve().parents[2]/'Assets/StreamingAssets/BlockGeneration/ControlGuides').glob('guide_*.png'))
if len(guides)!=14:raise RuntimeError('Expected 14 bundled product control guides.')
for path in guides:
 data=path.read_bytes();query=urllib.parse.urlencode({'filename':path.name,'subfolder':'Thai02Quality','type':'input'})
 try:
  with urllib.request.urlopen(a.server+'/view?'+query,timeout=15) as r:existing=r.read()
 except urllib.error.HTTPError as ex:
  if ex.code!=404:raise
 else:
  if hashlib.sha256(existing).digest()!=hashlib.sha256(data).digest():raise RuntimeError('Existing input differs; refusing overwrite: '+path.name)
  print('Already installed:',path.name);continue
 boundary='Thai02'+uuid.uuid4().hex
 body=(f'--{boundary}\r\nContent-Disposition: form-data; name="subfolder"\r\n\r\nThai02Quality\r\n--{boundary}\r\nContent-Disposition: form-data; name="type"\r\n\r\ninput\r\n--{boundary}\r\nContent-Disposition: form-data; name="image"; filename="{path.name}"\r\nContent-Type: image/png\r\n\r\n'.encode()+data+f'\r\n--{boundary}--\r\n'.encode())
 req=urllib.request.Request(a.server+'/upload/image',data=body,headers={'Content-Type':'multipart/form-data; boundary='+boundary})
 with urllib.request.urlopen(req,timeout=30) as r:reply=json.load(r)
 if reply.get('name')!=path.name or reply.get('subfolder')!='Thai02Quality':raise RuntimeError('Unexpected upload path: '+str(reply))
 print('Installed:',path.name)
