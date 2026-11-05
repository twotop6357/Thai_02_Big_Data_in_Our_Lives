// Execute through Unity in Edit mode. Builds the actual exhibit without development network discovery.
if (UnityEditor.EditorApplication.isPlaying || UnityEditor.BuildPipeline.isBuildingPlayer) return "Stop Play/build before building.";
var preloaded = UnityEditor.PlayerSettings.GetPreloadedAssets();
new Action(() => {
 try {
  var options = new UnityEditor.BuildPlayerOptions {
   scenes = UnityEditor.EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
   locationPathName = "Builds/ComfyAutostart/NSM_DNA_02.exe",
   target = UnityEditor.BuildTarget.StandaloneWindows64,
   options = UnityEditor.BuildOptions.None
  };
  var report = UnityEditor.BuildPipeline.BuildPlayer(options);
  var result = new {
   result = report.summary.result.ToString(), errors = report.summary.totalErrors, warnings = report.summary.totalWarnings,
   durationSeconds = report.summary.totalTime.TotalSeconds, bytes = report.summary.totalSize,
   outputPath = report.summary.outputPath, buildOptions = report.summary.options.ToString(),
   diagnostics = report.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Warning || m.type==LogType.Error).Select(m=>m.content).ToArray()
  };
  System.IO.Directory.CreateDirectory("docs/validation/comfy_autostart");
  System.IO.File.WriteAllText("docs/validation/comfy_autostart/build.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));
 } catch (Exception ex) {
  System.IO.File.WriteAllText("docs/validation/comfy_autostart/build.json",ex.ToString());Debug.LogException(ex);
 } finally {
  // Input System temporarily adds the build's action asset. Preserve the project's prior list.
  UnityEditor.PlayerSettings.SetPreloadedAssets(preloaded);
 }
})();
return "Non-development Windows build completed.";
