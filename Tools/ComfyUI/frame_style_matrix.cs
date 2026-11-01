// Validate every original PNG with the actual production Unity helper before composing sheets.
var dir="C:/UnityProjects/Thai_02_Big_Data_in_Our_Lives/docs/validation/comfy_style3/final_accepted/";
System.IO.Directory.CreateDirectory(dir+"framed");
var original=Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText(dir+"report.json"));
var checks=new Newtonsoft.Json.Linq.JArray();int passed=0;
foreach(var row in original["results"]){
 string name=(string)row["local_image"];var texture=new Texture2D(480,480,TextureFormat.RGBA32,false);
 var check=new Newtonsoft.Json.Linq.JObject{["file"]=name};
 try{
  if(!ImageConversion.LoadImage(texture,System.IO.File.ReadAllBytes(dir+name),false))throw new Exception("PNG decode failed");
  var pixels=Exhibition.Blocks.GeneratedImageFraming.Center(texture.GetPixels32(),480,480,requireComplete:true);
  int l=480,r=-1,b=480,t=-1;for(int y=0;y<480;y++)for(int x=0;x<480;x++)if(pixels[y*480+x].a>=8){l=Math.Min(l,x);r=Math.Max(r,x);b=Math.Min(b,y);t=Math.Max(t,y);}
  if(l<24||b<24||r>=456||t>=456||Math.Abs((l+r+1)*.5-240)>1||Math.Abs((b+t+1)*.5-240)>1)throw new Exception("Invalid final bounds");
  texture.SetPixels32(pixels);texture.Apply();System.IO.File.WriteAllBytes(dir+"framed/"+name,texture.EncodeToPNG());check["passed"]=true;passed++;
 }catch(Exception ex){check["passed"]=false;check["error"]=ex.ToString();}finally{UnityEngine.Object.DestroyImmediate(texture);}checks.Add(check);
}
var report=new Newtonsoft.Json.Linq.JObject{["actualUnity"]=Application.unityVersion,["strictRequireComplete"]=true,["passed"]=passed,["total"]=checks.Count,["checks"]=checks};
System.IO.File.WriteAllText(dir+"strict_unity_framing.json",report.ToString());
return passed+"/"+checks.Count+" final original PNGs passed strict source validation and centered framing.";
