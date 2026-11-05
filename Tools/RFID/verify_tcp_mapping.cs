// Explicit Play-mode test. Four real loopback TCP clients bound to distinct 127.0.0.x source IPs; never writes production RFID settings.
if(GameObject.Find("RfidTcpMappingVerification"))return "Already running.";
var root=new GameObject("RfidTcpMappingVerification");UnityEngine.Object.DontDestroyOnLoad(root);
Cysharp.Threading.Tasks.UniTask.Void(async()=>{
 var checks=new Newtonsoft.Json.Linq.JArray();var report=new Newtonsoft.Json.Linq.JObject{["task"]="RFID-STABLE-001-legacy-regression",["checks"]=checks,["loopback_tcp"]=true};
 var hub=UnityEngine.Object.FindFirstObjectByType<Exhibition.Blocks.RfidReaderHub>();var tcp=hub.GetComponent<Exhibition.Blocks.RfidTcpReaders>();var panel=hub.GetComponent<Exhibition.Blocks.RfidMappingPanel>();
 var manager=UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();var sim=hub.GetComponent<Exhibition.Blocks.RfidSimulator>();
 var oldKeyboard=UnityEngine.InputSystem.Keyboard.current;UnityEngine.InputSystem.Keyboard keyboard=null;
 var oldFocus=UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior;bool background=Application.runInBackground;
 var clients=new System.Net.Sockets.TcpClient[4];int port=0;System.Net.Sockets.TcpListener blocker=null;System.Net.Sockets.TcpClient stranger=null;
 var held=new string[4];bool pumping=true;
 string ReaderIp(int slot)=>"127.0.0."+(11+slot);
 Exhibition.Blocks.RfidConfiguration original=null;Exhibition.Blocks.RfidPresenceSettings originalPresence=null;string originalPath=null,originalPresencePath=null;
 string tempDirectory=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"Thai02-RFID-TCP-validation");string tempPath=System.IO.Path.Combine(tempDirectory,"RfidSettings.json");string tempPresencePath=System.IO.Path.Combine(tempDirectory,"RfidPresenceSettings.json");
 var pathProperty=typeof(Exhibition.Blocks.RfidTcpReaders).GetProperty("ConfigPath");var presencePathProperty=typeof(Exhibition.Blocks.RfidTcpReaders).GetProperty("PresencePath");var presenceProperty=typeof(Exhibition.Blocks.RfidTcpReaders).GetProperty("Presence");
 using var deadline=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(240));var token=deadline.Token;
 string directory="docs/validation/rfid_stable/legacy";System.IO.Directory.CreateDirectory(directory);
 void Save()=>System.IO.File.WriteAllText(directory+"/runtime.json",report.ToString());
 void Check(bool ok,string name){if(!ok)throw new Exception(name);checks.Add(name);Save();}
 async Cysharp.Threading.Tasks.UniTask Wait(Func<bool> predicate)=>await Cysharp.Threading.Tasks.UniTask.WaitUntil(predicate,cancellationToken:token);
 async Cysharp.Threading.Tasks.UniTask Frames(int n=4){for(int i=0;i<n;i++)await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update,token);}
 async Cysharp.Threading.Tasks.UniTask Delay(int ms)=>await Cysharp.Threading.Tasks.UniTask.Delay(ms,ignoreTimeScale:true,cancellationToken:token);
 async Cysharp.Threading.Tasks.UniTask Send(int slot,string hex){var bytes=Exhibition.Blocks.RfidHex.Bytes(hex);await clients[slot].GetStream().WriteAsync(bytes,0,bytes.Length,token);}
 // A real reader keeps reporting a resting card, so a held value is re-sent until released.
 void Hold(int slot,string hex)=>held[slot]=hex;
 void ReleaseAll(){for(int i=0;i<4;i++)held[i]=null;}
 async Cysharp.Threading.Tasks.UniTaskVoid Pump(){
  while(pumping&&!token.IsCancellationRequested){
   for(int i=0;i<4;i++){
    if(held[i]==null||clients[i]==null)continue;
    try{var bytes=Exhibition.Blocks.RfidHex.Bytes(held[i]);clients[i].GetStream().Write(bytes,0,bytes.Length);}
    catch(Exception ex) when(ex is System.IO.IOException||ex is ObjectDisposedException||ex is InvalidOperationException){}
   }
   await Cysharp.Threading.Tasks.UniTask.Delay(30,ignoreTimeScale:true);
  }
 }
 async Cysharp.Threading.Tasks.UniTask Key(UnityEngine.InputSystem.Key key,bool hold=false){UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(key));await Frames();if(!hold){UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());await Frames();}}
 async Cysharp.Threading.Tasks.UniTask<System.Net.Sockets.TcpClient> Dial(string ip){
  while(!token.IsCancellationRequested){
   var client=new System.Net.Sockets.TcpClient(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip),0));
   try{await client.ConnectAsync("127.0.0.1",port);return client;}
   catch(System.Net.Sockets.SocketException){client.Close();await Delay(50);}
  }
  throw new OperationCanceledException(token);
 }
 async Cysharp.Threading.Tasks.UniTask DialAll(){
  ReleaseAll();
  for(int i=0;i<4;i++){clients[i]?.Close();clients[i]=await Dial(ReaderIp(i));}
  await Wait(()=>Enumerable.Range(0,4).All(i=>tcp.State(i).Connected));await Frames();
 }
 async Cysharp.Threading.Tasks.UniTask Connect(Exhibition.Blocks.RfidConfiguration config){
  ReleaseAll();for(int i=0;i<4;i++){clients[i]?.Close();clients[i]=null;}
  tcp.ApplyConfiguration(config);await DialAll();
 }
 void WritePresence(string json)=>System.IO.File.WriteAllText(tempPresencePath,json);
 try{
  Application.runInBackground=true;
  await Wait(()=>tcp.IsReady&&!tcp.IsBusy&&hub.IsArtworkReady&&manager.IsInputReady&&!manager.IsTransitioning&&!manager.IsPresentationLocked);
  original=Newtonsoft.Json.JsonConvert.DeserializeObject<Exhibition.Blocks.RfidConfiguration>(Newtonsoft.Json.JsonConvert.SerializeObject(tcp.Configuration));originalPath=tcp.ConfigPath;
  originalPresence=tcp.Presence;originalPresencePath=tcp.PresencePath;
  Check(tcp.Presence.checkIntervalMilliseconds==1000&&originalPresencePath.Replace('\\','/').EndsWith("StreamingAssets/RFID/RfidPresenceSettings.json"),"Shipped check interval is one second in its own StreamingAssets JSON");
  Check(original.readers.OrderBy(r=>r.slot).Select(r=>r.ip).SequenceEqual(new[]{"192.168.0.121","192.168.0.122","192.168.0.123","192.168.0.124"}),"Shipped IP order matches top-left/top-right/bottom-left/bottom-right");
  Check(original.listenPort==10123,"Production configuration listens on TCP port 10123");
  await Wait(()=>tcp.ListenerStatus=="Listening on TCP port 10123");Check(true,"Unity actually opens the production port without any reader connected");
  using(var probe=new System.Net.Sockets.TcpClient()){await probe.ConnectAsync("127.0.0.1",10123);await Wait(()=>tcp.RejectedAddress=="127.0.0.1");}
  Check(Enumerable.Range(0,4).All(i=>!tcp.State(i).Connected),"An unconfigured source IP reaches port 10123 but attaches to no reader");
  Check(original.format.framing=="UidBurst"&&!original.format.asciiHex&&original.format.uidOffsetBytes==0&&original.format.uidLengthBytes==0,"Shipped format uses complete variable-length binary UID bursts rather than fixed 16-byte slices");
  blocker=new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any,0);blocker.Start();port=((System.Net.IPEndPoint)blocker.LocalEndpoint).Port;
  var config=new Exhibition.Blocks.RfidConfiguration{listenPort=port};config.format.framing="FixedLength";config.format.fixedFrameBytes=2;config.format.removedValues=new[]{"FFFF"};
  for(int i=0;i<4;i++)config.readers[i].ip=ReaderIp(i);
  config.cards=new[]{
   new Exhibition.Blocks.RfidCardMapping{uid="0001",kind="Group",id="child"},new Exhibition.Blocks.RfidCardMapping{uid="0002",kind="Product",id="bags"},
   new Exhibition.Blocks.RfidCardMapping{uid="0003",kind="Color",id="red"},new Exhibition.Blocks.RfidCardMapping{uid="0004",kind="Color",id="blue"},
   new Exhibition.Blocks.RfidCardMapping{uid="0005",kind="Group",id="student"}
  };
  System.IO.Directory.CreateDirectory(tempDirectory);System.IO.File.WriteAllText(tempPath,Newtonsoft.Json.JsonConvert.SerializeObject(config));
  System.IO.File.Copy(originalPresencePath,tempPresencePath,true);
  pathProperty.SetValue(tcp,tempPath);presencePathProperty.SetValue(tcp,tempPresencePath);
  Pump().Forget();
  await tcp.ReloadAsync();await Wait(()=>tcp.ListenerStatus.StartsWith("Listen failed"));Check(!tcp.State(0).Connected,"An occupied port is reported without clearing or inventing reader state");
  blocker.Stop();blocker=null;await Wait(()=>tcp.ListenerStatus=="Listening on TCP port "+port);Check(true,"The listener recovers automatically once the port is free");
  hub.ClearReaders(true);hub.Readers.RequireGroupFirst=true;
  Check(hub.RecognizeReader(0,Exhibition.Blocks.BlockKind.Group,"child"),"Simulator path can place a block with no reader connected");
  await Delay(3000);Check(hub.Readers[0]?.Id=="child","A placed block survives while no reader is connected");
  clients[0]=await Dial(ReaderIp(0));await Wait(()=>tcp.State(0).Connected);await Delay(1500);
  Check(hub.Readers[0]?.Id=="child"&&!tcp.State(1).Connected,"A connected reader that has reported nothing attaches only its slot and keeps the placed block");
  clients[0].Close();clients[0]=null;await Wait(()=>!tcp.State(0).Connected);await Frames();
  Check(!hub.Readers[0].HasValue,"Losing a connected reader clears its slot");
  await DialAll();Check(Enumerable.Range(0,4).All(i=>tcp.State(i).Connected),"Four readers connect in and attach automatically by source IP");
  stranger=await Dial("127.0.0.99");await Wait(()=>tcp.RejectedAddress=="127.0.0.99");stranger.Close();stranger=null;
  Check(Enumerable.Range(0,4).All(i=>tcp.State(i).Connected),"An unconfigured source IP does not disturb attached readers");
  // Presence judgement at the shipped one-second interval.
  hub.ClearReaders(true);hub.Readers.RequireGroupFirst=false;
  var clock=System.Diagnostics.Stopwatch.StartNew();Hold(0,"0001");await Wait(()=>hub.Readers[0]?.Id=="child");
  report["first_recognition_ms"]=clock.ElapsedMilliseconds;Check(clock.ElapsedMilliseconds<=1300,"A new value is recognized at the next one-second check");
  int steady=hub.Readers.Revision;await Delay(3500);Check(hub.Readers.Revision==steady&&hub.Readers[0]?.Id=="child","The same repeated value keeps the block across checks with no flicker");
  bool cleared=false;Action<int,Exhibition.Blocks.RfidBlock?,Exhibition.Blocks.RfidBlock?> watch=(slot,before,after)=>{if(slot==0&&!after.HasValue)cleared=true;};
  hub.ReaderChanged+=watch;clock.Restart();Hold(0,"0005");await Wait(()=>hub.Readers[0]?.Id=="student");hub.ReaderChanged-=watch;
  report["replace_ms"]=clock.ElapsedMilliseconds;Check(!cleared&&clock.ElapsedMilliseconds<=1300,"A different value replaces the block at the next check without an empty state");
  held[0]=null;clock.Restart();await Wait(()=>!hub.Readers[0].HasValue);
  report["removal_ms"]=clock.ElapsedMilliseconds;Check(clock.ElapsedMilliseconds<=2300&&tcp.State(0).Connected&&tcp.State(0).Uid==null,"A missing value removes the block within two checks and keeps the connection");
  // The interval comes from the JSON file. Remaining checks use a short interval.
  WritePresence("{\"checkIntervalMilliseconds\":200}");await tcp.ReloadAsync();await DialAll();
  Check(tcp.Presence.checkIntervalMilliseconds==200,"The JSON interval is applied on reload");
  hub.Readers.RequireGroupFirst=false;Hold(0,"0001");await Wait(()=>hub.Readers[0]?.Id=="child");
  await Delay(100);held[0]=null;clock.Restart();await Wait(()=>!hub.Readers[0].HasValue);
  report["removal_ms_at_200"]=clock.ElapsedMilliseconds;Check(clock.ElapsedMilliseconds<=650,"Changing the JSON interval changes actual removal timing");
  var keep=tcp.Configuration;
  WritePresence("{\"checkIntervalMilliseconds\":50}");await tcp.ReloadAsync();Check(ReferenceEquals(keep,tcp.Configuration)&&tcp.Presence.checkIntervalMilliseconds==200&&!string.IsNullOrEmpty(tcp.Error),"An out-of-range interval is rejected and the working settings stay");
  WritePresence("{}");await tcp.ReloadAsync();Check(ReferenceEquals(keep,tcp.Configuration)&&tcp.Presence.checkIntervalMilliseconds==200&&!string.IsNullOrEmpty(tcp.Error),"A missing interval is rejected and the working settings stay");
  presencePathProperty.SetValue(tcp,tempPresencePath+".missing");await tcp.ReloadAsync();Check(ReferenceEquals(keep,tcp.Configuration)&&!string.IsNullOrEmpty(tcp.Error),"A missing interval file is reported and the working settings stay");
  presencePathProperty.SetValue(tcp,tempPresencePath);WritePresence("{\"checkIntervalMilliseconds\":200}");await tcp.ReloadAsync();await DialAll();
  Check(tcp.Error=="","A corrected interval file clears the error");
  hub.ClearReaders(true);hub.Readers.RequireGroupFirst=true;
  Hold(1,"0002");await Wait(()=>tcp.State(1).Uid=="0002");Check(!hub.Readers[1].HasValue,"Product before first group is ignored while raw UID remains visible");
  await Send(0,"00");await Delay(60);Check(tcp.State(0).Frames==0&&tcp.State(0).LastChunkHex=="00","Partial TCP bytes are visible in F10 without becoming a card");
  await Send(0,"01");Hold(0,"0001");await Wait(()=>hub.Readers[0]?.Id=="child");Check(tcp.State(0).Uid=="0001","Fragmented HEX bytes combine without losing leading zeros");
  await Delay(500);Check(!hub.Readers[1].HasValue,"Repeated reports do not replay a product ignored before group");
  Hold(1,"FFFF");await Wait(()=>tcp.State(1).Uid==null);int frames=tcp.State(1).Frames;
  held[1]=null;await Send(1,"FFFF0002");Hold(1,"0002");await Wait(()=>hub.Readers[1]?.Id=="bags");Check(tcp.State(1).Frames>=frames+2,"Coalesced removal and card reports split into separate frames");
  Hold(2,"0003");Hold(3,"0004");await Wait(()=>hub.Readers.HasRequiredComposition);
  Check(hub.Readers[0]?.Id=="child"&&hub.Readers[1]?.Id=="bags"&&hub.Readers[2]?.Id=="red"&&hub.Readers[3]?.Id=="blue","All four reader positions drive the matching shared screen slots");
  Check(hub.Readers.Color1Id=="red"&&hub.Readers.Color2Id=="blue","Physical slot order preserves primary/secondary color order");
  UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
  keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>("RFID TCP verification keyboard");
  await Key(UnityEngine.InputSystem.Key.F9);Check(sim.IsVisible,"Existing F9 simulator still opens");
  await Key(UnityEngine.InputSystem.Key.F10);Check(panel.IsVisible&&!sim.IsVisible&&Cursor.visible&&manager.IsPresentationLocked,"F10 opens monitor, closes F9, exposes cursor and locks exhibition button");
  await Key(UnityEngine.InputSystem.Key.Digit1,true);await Delay(160);Check(manager.IsTitleScreen,"Typing digit 1 in setup does not start a session");
  await Key(UnityEngine.InputSystem.Key.F9);Check(!sim.IsVisible&&panel.IsVisible,"F9 cannot overlap F10 and steal cursor ownership");
  int revision=hub.Readers.Revision;
  using(hub.Readers.FreezeInput()){
   Hold(0,"0005");Hold(1,"FFFF");Hold(2,"00AA");held[3]=null;clients[3].Close();await Wait(()=>tcp.State(0).Uid=="0005"&&tcp.State(1).Uid==null&&tcp.State(2).Uid=="00AA"&&!tcp.State(3).Connected);
   Check(hub.Readers.Revision==revision&&hub.Readers.HasRequiredComposition,"Generation freeze ignores TCP replacement, loss, unknown card and disconnection");
   Check(!panel.CanSave&&!await tcp.SaveMappingAsync("00AA",Exhibition.Blocks.BlockKind.Product,"shoes"),"F10 remains live but cannot save mappings during generation");
   clients[3]=await Dial(ReaderIp(3));await Wait(()=>tcp.State(3).Connected);
  }
  await Delay(500);Check(hub.Readers.Revision==revision,"Releasing generation lock does not replay discarded network reports");
  Hold(2,"FFFF");await Wait(()=>!hub.Readers[2].HasValue);Check(!hub.Readers[2].HasValue,"A fresh removal after generation updates the program");
  Hold(1,"00AA");await Wait(()=>tcp.State(1).Uid=="00AA");Check(!hub.Readers[1].HasValue,"Unknown card is displayed without inventing a block");
  Check(await tcp.SaveMappingAsync("00 aa",Exhibition.Blocks.BlockKind.Product,"shoes"),"Mapping saves canonical HEX using an atomic JSON replacement");
  Check(hub.Readers[1]?.Id=="shoes"&&tcp.Configuration.Find("00AA")?.id=="shoes","Saving maps the currently held card immediately");
  await Delay(500);Check(hub.Readers[1]?.Id=="shoes","A saved held card stays through later checks");
  Check(System.IO.File.Exists(tempPath+".bak")&&!System.IO.File.Exists(tempPath+".tmp"),"Saving retains prior JSON backup and removes temporary file");
  Check(await tcp.SaveMappingAsync("00AA",Exhibition.Blocks.BlockKind.Product,"hats"),"Existing UID can be remapped");
  Check(tcp.Configuration.cards.Count(c=>c.uid=="00AA")==1&&hub.Readers[1]?.Id=="hats","Replacing a mapping produces one UID entry and updates its block");
  Check(await tcp.SaveMappingAsync("00AA",Exhibition.Blocks.BlockKind.Product,"hats",true),"Mapping can be deleted and saved");
  Check(tcp.Configuration.Find("00AA")==null&&!hub.Readers[1].HasValue,"Deleting a held-card mapping clears the semantic block");
  string good=System.IO.File.ReadAllText(tempPath);Check(!await tcp.SaveMappingAsync("ZZ",Exhibition.Blocks.BlockKind.Product,"bags"),"Invalid HEX is rejected with an operator-visible error");
  Check(System.IO.File.ReadAllText(tempPath)==good,"Rejected mapping preserves the previous JSON bytes");
  Check(!await tcp.SaveMappingAsync("00BB",Exhibition.Blocks.BlockKind.Product,"not_a_product"),"Unknown block ID cannot be saved");
  System.IO.File.WriteAllText(tempPath,"{bad");var before=tcp.Configuration;
  await tcp.ReloadAsync();Check(ReferenceEquals(before,tcp.Configuration)&&tcp.IsReady&&!string.IsNullOrEmpty(tcp.Error),"Malformed JSON reload retains last working configuration");
  System.IO.File.WriteAllText(tempPath,good);
  await tcp.ReloadAsync();await DialAll();
  Check(tcp.Configuration.Find("0001")?.id=="child"&&tcp.Error=="","Reload JSON restores mappings and readers can connect again");
  // Remote disconnect/reconnect, without applying another configuration.
  clients[3].Close();await Wait(()=>!tcp.State(3).Connected);
  clients[3]=await Dial(ReaderIp(3));await Wait(()=>tcp.State(3).Connected);
  Check(tcp.State(0).Connected&&tcp.State(1).Connected&&tcp.State(2).Connected,"One reader reconnects automatically without restarting the other three");
  // A rebooted reader connects again while its old socket still looks open.
  hub.Readers.RequireGroupFirst=false;Hold(3,"0004");await Wait(()=>hub.Readers[3]?.Id=="blue");
  held[3]=null;var halfOpen=clients[3];clients[3]=await Dial(ReaderIp(3));await Wait(()=>!hub.Readers[3].HasValue&&tcp.State(3).Uid==null);
  Hold(3,"0004");await Wait(()=>hub.Readers[3]?.Id=="blue");await Delay(400);
  Check(tcp.State(3).Connected&&hub.Readers[3]?.Id=="blue","The newest connection replaces a half-open one and the old session cannot clear it");
  halfOpen.Close();await Delay(400);Check(tcp.State(3).Connected&&hub.Readers[3]?.Id=="blue","Closing the replaced socket leaves the new session intact");
  Hold(3,"FFFF");await Wait(()=>!hub.Readers[3].HasValue);Check(tcp.State(3).Uid==null,"A configured removal value is judged as a missing value");
  Hold(3,"0004");await Wait(()=>hub.Readers[3]?.Id=="blue");
  Check(hub.Readers[3]?.Id=="blue","Reconnected reader resumes recognition");
  config.format.framing="FixedLength";config.format.fixedFrameBytes=16;
  const string sample="0841FF5433014B3130323000003938EF";
  config.cards=config.cards.Concat(new[]{new Exhibition.Blocks.RfidCardMapping{uid=sample,kind="Group",id="senior"}}).ToArray();
  await Connect(config);hub.Readers.RequireGroupFirst=false;
  await Send(0,sample.Substring(0,12));await Delay(50);Check(tcp.State(0).Frames==0,"Partial 16-byte example remains raw telemetry until complete");
  await Send(0,sample.Substring(12));Hold(0,sample);await Wait(()=>tcp.State(0).Uid==sample);
  Check(hub.Readers[0]?.Id=="senior"&&tcp.State(0).RawHex=="08 41 FF 54 33 01 4B 31 30 32 30 00 00 39 38 EF","Complete example HEX maps as an opaque 16-byte card value with no field interpretation");
  config.format.fixedFrameBytes=2;
  config.format.framing="IdleGap";await Connect(config);hub.Readers.RequireGroupFirst=false;
  await Send(0,"00");await Delay(30);await Send(0,"01");await Wait(()=>tcp.State(0).Uid=="0001");
  Check(hub.Readers[0]?.Id=="child","Actual TCP idle-gap framing combines a fragmented binary burst");
  config.format.framing="Delimiter";config.format.asciiHex=true;config.format.delimiterHex="0D0A";await Connect(config);hub.Readers.RequireGroupFirst=false;
  await Send(0,"3030");await Delay(30);await Send(0,"30310D0A303030350D0A");await Wait(()=>tcp.State(0).Uid=="0005");
  Check(tcp.State(0).Frames==2&&hub.Readers[0]?.Id=="student","Actual TCP ASCII-HEX delimiter reports survive fragmentation and coalescing");
  config.format.framing="FixedLength";config.format.fixedFrameBytes=2;config.format.asciiHex=false;await Connect(config);hub.Readers.RequireGroupFirst=false;
  for(int i=0;i<4;i++)Hold(i,"000"+(i+1));await Wait(()=>hub.Readers.HasRequiredComposition);
  typeof(Exhibition.Blocks.RfidMappingPanel).GetField("_uid",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(panel,"0001");
  await Frames();await Cysharp.Threading.Tasks.UniTask.WaitForEndOfFrame(tcp);
  ScreenCapture.CaptureScreenshot(directory+"/f10_panel.png");await Delay(400);
  await Key(UnityEngine.InputSystem.Key.F10);Check(!panel.IsVisible&&!manager.IsPresentationLocked,"F10 closes and releases its modal lock");
  await Frames();Check(manager.IsTitleScreen,"Closing setup discards a pending exhibition-button press");
  await Key(UnityEngine.InputSystem.Key.F9);Check(sim.IsVisible,"F9 operates normally after closing setup");await Key(UnityEngine.InputSystem.Key.F9);
  panel.SetVisible(true);panel.enabled=false;Check(!panel.IsVisible&&!manager.IsPresentationLocked,"Disabling the setup component restores cursor/input ownership");panel.enabled=true;
  int count=UnityEngine.Object.FindObjectsByType<Exhibition.Blocks.RfidTcpReaders>(FindObjectsSortMode.None).Length;
  Check(count==1,"Exactly one persistent TCP adapter exists");
  report["passed"]=true;
 }catch(Exception ex){report["passed"]=false;report["error"]=ex.ToString();Debug.LogException(ex);}
 finally{
  pumping=false;
  if(panel&&panel.IsVisible)panel.SetVisible(false);
  if(keyboard!=null)UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);if(oldKeyboard!=null&&oldKeyboard.added)oldKeyboard.MakeCurrent();
  UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=oldFocus;
  if(tcp&&original!=null){pathProperty.SetValue(tcp,originalPath);presencePathProperty.SetValue(tcp,originalPresencePath);presenceProperty.SetValue(tcp,originalPresence);tcp.ApplyConfiguration(original);}
  foreach(var client in clients)client?.Close();stranger?.Close();blocker?.Stop();
  if(hub){hub.ClearReaders(true);hub.Readers.RequireGroupFirst=true;}
  manager?.ResetButtonInput();Application.runInBackground=background;Save();UnityEngine.Object.Destroy(root);
 }
});return "RFID TCP/JSON/F10 verification started.";
