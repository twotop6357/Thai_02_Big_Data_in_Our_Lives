// Explicit Play-mode integration: four TCP clients, variable-length UID bursts, isolated JSON.
if(GameObject.Find("RfidUidBurstVerification"))return "Already running.";
var root=new GameObject("RfidUidBurstVerification");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var checks=new Newtonsoft.Json.Linq.JArray();var report=new Newtonsoft.Json.Linq.JObject{["task"]="RFID-STABLE-001",["checks"]=checks,["loopback_tcp"]=true};
 var hub=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidReaderHub>();var tcp=hub.GetComponent<Exhibition.Blocks.RfidTcpReaders>();
 var panel=hub.GetComponent<Exhibition.Blocks.RfidMappingPanel>();var clients=new System.Net.Sockets.TcpClient[4];
 var pathProperty=typeof(Exhibition.Blocks.RfidTcpReaders).GetProperty("ConfigPath");var presenceProperty=typeof(Exhibition.Blocks.RfidTcpReaders).GetProperty("Presence");
 Exhibition.Blocks.RfidConfiguration original=null;Exhibition.Blocks.RfidPresenceSettings originalPresence=null;string originalPath=null;
 bool background=Application.runInBackground,pumping=false;var held=new string[4];var copies=new[]{2,1,3,2};
 string directory="docs/validation/rfid_stable/burst";System.IO.Directory.CreateDirectory(directory);
 string temporary=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"Thai02-RFID-UidBurst-validation");System.IO.Directory.CreateDirectory(temporary);
 using var deadline=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(150));var token=deadline.Token;
 void Save()=>System.IO.File.WriteAllText(directory+"/runtime.json",report.ToString());
 void Check(bool ok,string name){if(!ok)throw new Exception(name);checks.Add(name);Save();}
 async Cysharp.Threading.Tasks.UniTask Wait(Func<bool> predicate)=>await Cysharp.Threading.Tasks.UniTask.WaitUntil(predicate,cancellationToken:token);
 async Cysharp.Threading.Tasks.UniTask Delay(int ms)=>await Cysharp.Threading.Tasks.UniTask.Delay(ms,ignoreTimeScale:true,cancellationToken:token);
 async Cysharp.Threading.Tasks.UniTask Write(int slot,string value,int split=0){
  var b=Exhibition.Blocks.RfidHex.Bytes(value);var stream=clients[slot].GetStream();
  if(split>0&&split<b.Length){await stream.WriteAsync(b,0,split,token);await Delay(25);await stream.WriteAsync(b,split,b.Length-split,token);}
  else await stream.WriteAsync(b,0,b.Length,token);
 }
 async Cysharp.Threading.Tasks.UniTaskVoid Pump(){
  int cycle=0;
  while(pumping&&!token.IsCancellationRequested){
   for(int i=0;i<4;i++)if(held[i]!=null&&clients[i]!=null){
    string burst=string.Concat(Enumerable.Repeat(held[i],copies[i]));
    try{await Write(i,burst,1+cycle%Math.Max(1,burst.Length/2-1));}catch(Exception ex)when(ex is System.IO.IOException||ex is ObjectDisposedException||ex is InvalidOperationException){}
   }
   cycle++;await Delay(160);
  }
 }
 async Cysharp.Threading.Tasks.UniTask Dial(int slot,int port){
  clients[slot]?.Close();clients[slot]=new System.Net.Sockets.TcpClient(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0."+(11+slot)),0));
  await clients[slot].ConnectAsync("127.0.0.1",port);await Wait(()=>tcp.State(slot).Connected);
 }
 try{
  Application.runInBackground=true;await Wait(()=>tcp.IsReady&&!tcp.IsBusy&&hub.IsArtworkReady);
  original=Newtonsoft.Json.JsonConvert.DeserializeObject<Exhibition.Blocks.RfidConfiguration>(Newtonsoft.Json.JsonConvert.SerializeObject(tcp.Configuration));originalPath=tcp.ConfigPath;originalPresence=tcp.Presence;
  var config=new Exhibition.Blocks.RfidConfiguration();var reservation=new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback,0);reservation.Start();config.listenPort=((System.Net.IPEndPoint)reservation.LocalEndpoint).Port;reservation.Stop();
  for(int i=0;i<4;i++)config.readers[i].ip="127.0.0."+(11+i);
  string[] ids={"0504A1736922D5","0001A2B3","00010203040506070809","000102030405060708090A0B0C0D0E0F"};
  config.cards=new[]{
   new Exhibition.Blocks.RfidCardMapping{uid=ids[0],kind="Group",id="child"},
   new Exhibition.Blocks.RfidCardMapping{uid=ids[1],kind="Product",id="bags"},
   new Exhibition.Blocks.RfidCardMapping{uid=ids[2],kind="Color",id="red"},
   new Exhibition.Blocks.RfidCardMapping{uid=ids[3],kind="Color",id="blue"},
   new Exhibition.Blocks.RfidCardMapping{uid="0504A1736922D6",kind="Group",id="student"}
  };
  pathProperty.SetValue(tcp,System.IO.Path.Combine(temporary,"RfidSettings.json"));presenceProperty.SetValue(tcp,new Exhibition.Blocks.RfidPresenceSettings{checkIntervalMilliseconds=1000});
  tcp.ApplyConfiguration(config);await Wait(()=>tcp.ListenerStatus=="Listening on TCP port "+config.listenPort);
  for(int i=0;i<4;i++)await Dial(i,config.listenPort);
  hub.ClearReaders(true);hub.Readers.RequireGroupFirst=false;panel.SetVisible(true);
  Check(panel.IsVisible,"F10 monitor remains available with variable UID framing");
  await Write(0,ids[0].Substring(0,6));await Delay(50);
  Check(tcp.State(0).Frames==0&&tcp.State(0).Uid==null,"A partial TCP receive is visible only as RX and cannot be selected as a UID");
  await Write(0,ids[0].Substring(6)+ids[0]);await Delay(130);
  await Wait(()=>tcp.State(0).Uid==ids[0]);
  Check(tcp.State(0).UidBytes==7&&hub.Readers[0]?.Id=="child","Two seven-byte copies coalesce to exactly one complete seven-byte UID");
  for(int i=0;i<4;i++)held[i]=ids[i];pumping=true;Pump().Forget();
  await Wait(()=>hub.Readers.HasRequiredComposition);
  Check(Enumerable.Range(0,4).All(i=>tcp.State(i).Uid==ids[i]),"Four real TCP clients retain 7/4/10/16-byte UIDs without forced padding or truncation");
  int revision=hub.Readers.Revision;var counts=Enumerable.Range(0,4).Select(i=>tcp.State(i).Frames).ToArray();await Delay(5500);
  Check(hub.Readers.Revision==revision&&Enumerable.Range(0,4).All(i=>tcp.State(i).Uid==ids[i]),"Repeated/split/coalesced reports stay identical across at least five one-second checks");
  Check(Enumerable.Range(0,4).All(i=>tcp.State(i).Frames>counts[i]+8),"Each reader actually sent more than eight further reports during the stability test");
  held[0]="0504A1736922D6";await Wait(()=>tcp.State(0).Uid==held[0]);
  Check(hub.Readers[0]?.Id=="student","A different seven-byte UID replaces the card instead of retaining the previous value");
  held[1]="0004A2B30004A2B3";await Wait(()=>tcp.State(1).Uid=="0004A2B3");
  Check(!hub.Readers[1].HasValue,"An unmapped repeated UID remains unmapped and never becomes a neighboring block");
  Check(await tcp.SaveMappingAsync(tcp.State(1).Uid,Exhibition.Blocks.BlockKind.Product,"shoes"),"Confirmed UID saves to isolated JSON through the actual mapping API");
  Check(tcp.Configuration.Find("0004A2B3")?.id=="shoes"&&hub.Readers[1]?.Id=="shoes","Only the canonical single UID is saved and applied");
  revision=hub.Readers.Revision;
  using(hub.Readers.FreezeInput()){
   held[0]=ids[0];held[2]=null;await Wait(()=>tcp.State(0).Uid==ids[0]&&tcp.State(2).Uid==null);
   Check(hub.Readers.Revision==revision&&hub.Readers[0]?.Id=="student"&&hub.Readers[2]?.Id=="red","Generation freeze keeps the captured four-block display while the monitor updates");
   Check(!panel.CanSave&&!await tcp.SaveMappingAsync("00112233",Exhibition.Blocks.BlockKind.Color,"green"),"Mappings remain locked during generation");
  }
  await Delay(1300);Check(hub.Readers.Revision==revision,"Ending generation does not replay ignored readings");
  held[2]=ids[2];await Wait(()=>tcp.State(2).Uid==ids[2]);held[2]=null;await Wait(()=>!hub.Readers[2].HasValue);
  Check(tcp.State(2).Connected,"Card removal clears the slot while retaining the TCP connection");
  held[3]=null;clients[3].Close();await Wait(()=>!tcp.State(3).Connected);await Dial(3,config.listenPort);
  await Write(3,ids[3]);await Delay(150);Check(tcp.State(3).Uid==null,"Reconnect discards prior confirmation; one single new report is not a selectable UID");
  await Write(3,ids[3]);await Wait(()=>tcp.State(3).Uid==ids[3]);held[3]=ids[3];
  Check(hub.Readers[3]?.Id=="blue","Two complete reports restore the same UID after reconnect");
  await Cysharp.Threading.Tasks.UniTask.WaitForEndOfFrame(tcp);ScreenCapture.CaptureScreenshot(directory+"/f10_panel.png");await Delay(300);
  report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{
  pumping=false;for(int i=0;i<4;i++)held[i]=null;
  if(panel&&panel.IsVisible)panel.SetVisible(false);
  foreach(var c in clients)c?.Close();
  if(tcp&&original!=null){pathProperty.SetValue(tcp,originalPath);presenceProperty.SetValue(tcp,originalPresence);tcp.ApplyConfiguration(original);}
  if(hub){hub.ClearReaders(true);hub.Readers.RequireGroupFirst=true;}
  Application.runInBackground=background;Save();UnityEngine.Object.Destroy(root);
 }
});return "RFID variable-UID integration verification started.";
