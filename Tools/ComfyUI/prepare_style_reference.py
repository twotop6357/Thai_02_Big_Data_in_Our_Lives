"""Verify and upload the packaged, immutable user style reference to a local ComfyUI server."""
import argparse,hashlib,json,urllib.request,urllib.parse,urllib.error,uuid
from pathlib import Path
from build_workflow import read_json
ROOT=Path(__file__).resolve().parents[2]
def ensure(server):
 model=read_json('model.json')
 if model.get('active_mode')!='reference_illustration':return None
 ref=model['style_reference'];name=ref['filename'];folder=ref['subfolder']
 if Path(name).name!=name or '..' in name or ':' in name or folder!='Thai02Style3':raise ValueError('Invalid immutable reference path')
 data=(ROOT/'Assets/StreamingAssets/BlockGeneration/StyleReferences'/name).read_bytes()
 if hashlib.sha256(data).hexdigest()!=ref['sha256']:raise ValueError('Packaged style reference hash mismatch')
 url=server.rstrip('/')+'/view?'+urllib.parse.urlencode({'filename':name,'subfolder':folder,'type':'input'})
 try:
  with urllib.request.urlopen(url,timeout=20) as response:old=response.read()
  if hashlib.sha256(old).digest()!=hashlib.sha256(data).digest():raise ValueError('Server reference differs; refusing to overwrite')
 except urllib.error.HTTPError as ex:
  if ex.code!=404:raise
  boundary=uuid.uuid4().hex
  body=b''.join(f'--{boundary}\r\nContent-Disposition: form-data; name="{k}"\r\n\r\n{v}\r\n'.encode() for k,v in [('subfolder',folder),('type','input'),('overwrite','false')])
  body+=f'--{boundary}\r\nContent-Disposition: form-data; name="image"; filename="{name}"\r\nContent-Type: image/png\r\n\r\n'.encode()+data+f'\r\n--{boundary}--\r\n'.encode()
  req=urllib.request.Request(server.rstrip('/')+'/upload/image',data=body,headers={'Content-Type':'multipart/form-data; boundary='+boundary})
  with urllib.request.urlopen(req,timeout=20) as response:result=json.load(response)
  if result.get('name')!=name or result.get('subfolder')!=folder:raise ValueError('Unexpected reference upload response')
 return folder+'/'+name
if __name__=='__main__':
 p=argparse.ArgumentParser();p.add_argument('--server',default='http://127.0.0.1:8188');print(ensure(p.parse_args().server))
