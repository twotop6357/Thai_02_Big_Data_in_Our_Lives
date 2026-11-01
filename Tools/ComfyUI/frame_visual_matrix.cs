// Execute in the stopped Unity Editor after a matrix batch (or for the completed images of a partial batch).
var folder = "docs/validation/comfy_blocks_v3";
var report = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText(folder + "/report.json"));
System.IO.Directory.CreateDirectory(folder + "/framed");
var results = new Newtonsoft.Json.Linq.JArray();
var seen = new System.Collections.Generic.HashSet<string>();
foreach (var item in (Newtonsoft.Json.Linq.JArray)report["results"])
{
    string name = (string)item["local_image"];
    if (!seen.Add(name)) continue;
    Texture2D texture = new Texture2D(2,2,TextureFormat.RGBA32,false);
    try
    {
        if (!ImageConversion.LoadImage(texture,System.IO.File.ReadAllBytes(folder + "/" + name),false)) throw new Exception("Could not decode " + name);
        var centered = Exhibition.Blocks.GeneratedImageFraming.Center(texture.GetPixels32(),texture.width,texture.height);
        texture.SetPixels32(centered);texture.Apply();
        int l=480,r=-1,b=480,t=-1,transparent=0;
        for(int y=0;y<480;y++)for(int x=0;x<480;x++)
        {
            if(centered[y*480+x].a==0)transparent++;
            if(centered[y*480+x].a<8)continue;
            l=Math.Min(l,x);r=Math.Max(r,x);b=Math.Min(b,y);t=Math.Max(t,y);
        }
        double cx=(l+r+1)*.5,cy=(b+t+1)*.5;
        if(Math.Abs(cx-240)>1 || Math.Abs(cy-240)>1) throw new Exception("Off-center final image: " + name);
        if(l<24 || b<24 || r>=456 || t>=456) throw new Exception("Unsafe image margin: " + name);
        System.IO.File.WriteAllBytes(folder + "/framed/" + name,texture.EncodeToPNG());
        results.Add(new Newtonsoft.Json.Linq.JObject{["file"]=name,["center"]=new Newtonsoft.Json.Linq.JArray(cx,cy),
            ["bounds"]=new Newtonsoft.Json.Linq.JArray(l,b,r+1,t+1),["transparent_pixels"]=transparent});
    }
    finally{UnityEngine.Object.DestroyImmediate(texture);}
}
System.IO.File.WriteAllText(folder + "/framing.json",results.ToString());
return new { count=results.Count, centered=true };
