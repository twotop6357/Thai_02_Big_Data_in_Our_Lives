"""Local synthetic responses for quality rejection/lifecycle tests. Never submits a GPU job."""
from http.server import BaseHTTPRequestHandler,ThreadingHTTPServer
from pathlib import Path
from PIL import Image,ImageDraw
import io,json,time,urllib.parse,uuid
ROOT=Path(__file__).resolve().parents[2]
state={'mode':'clipped','jobs':{},'requests':[]}
ref=next((ROOT/'Assets/StreamingAssets/BlockGeneration/StyleReferences').glob('style_user_*.png')).read_bytes()
def png(clipped):
 im=Image.new('RGBA',(480,480));d=ImageDraw.Draw(im);d.rectangle((0 if clipped else 140,140,240 if clipped else 340,340),fill=(236,104,104,255));stream=io.BytesIO();im.save(stream,format='PNG');return stream.getvalue()
images={'clipped.png':png(True),'valid.png':png(False)}
class Handler(BaseHTTPRequestHandler):
 def send(self,body,status=200,kind='application/json'):
  if not isinstance(body,bytes):body=json.dumps(body).encode()
  try:self.send_response(status);self.send_header('Content-Type',kind);self.send_header('Content-Length',str(len(body)));self.end_headers();self.wfile.write(body)
  except (BrokenPipeError,ConnectionResetError,ConnectionAbortedError):pass
 def do_GET(self):
  parsed=urllib.parse.urlparse(self.path);q=urllib.parse.parse_qs(parsed.query)
  if parsed.path=='/queue':return self.send({'queue_running':[],'queue_pending':[]})
  if parsed.path.startswith('/history/'):
   pid=parsed.path.rsplit('/',1)[1];job=state['jobs'].get(pid)
   if not job:return self.send({})
   if job['mode']=='delay' or time.monotonic()-job['started']<1:return self.send({})
   filename='clipped.png' if job['mode']=='clipped' else 'valid.png'
   return self.send({pid:{'status':{'status_str':'success','completed':True},'outputs':{'7':{'images':[{'filename':filename,'subfolder':'fixture','type':'output'}]}}}})
  if parsed.path=='/view':
   if q.get('type')==['input']:return self.send(ref,kind='image/png')
   return self.send(images[q['filename'][0]],kind='image/png')
  return self.send({},404)
 def do_POST(self):
  data=json.loads(self.rfile.read(int(self.headers.get('Content-Length',0))) or '{}')
  if self.path=='/fixture_mode':state['mode']=data['mode'];return self.send({'mode':state['mode']})
  if self.path=='/queue':return self.send(b'')
  if self.path=='/prompt':
   graph=data['prompt']
   if graph['5']['class_type']!='SamplerCustomAdvanced' or 'positive' in graph['5']['inputs'] or graph['36']['class_type']!='RandomNoise':return self.send({'error':'Incorrect runtime Klein bindings'},400)
   state['requests'].append({'mode':state['mode'],'prompt':graph})
   (ROOT/'docs/validation/comfy_style3/fixture_requests.json').write_text(json.dumps(state['requests'],indent=2))
   if state['mode']=='error':return self.send({'error':'Intentional fixture submission error'},400)
   pid=uuid.uuid4().hex;state['jobs'][pid]={'mode':state['mode'],'started':time.monotonic()};return self.send({'prompt_id':pid})
  return self.send({},404)
 def log_message(self,*_):pass
if __name__=='__main__':
 print('Quality fixture listening on 127.0.0.1:18190',flush=True)
 ThreadingHTTPServer(('127.0.0.1',18190),Handler).serve_forever()
