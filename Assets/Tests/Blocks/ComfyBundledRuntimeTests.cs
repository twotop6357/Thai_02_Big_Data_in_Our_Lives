using System;
using System.IO;
using Exhibition.Blocks;
using NUnit.Framework;

public sealed class ComfyBundledRuntimeTests
{
    private string _root, _runtime;

    [SetUp] public void CreateIsolatedApplicationFolder()
    {
        // 공백과 한글이 있는 경로에서도 인자가 깨지지 않는지 함께 확인한다.
        _root = Path.Combine(Path.GetTempPath(), "Thai02 번들 Tests_" + Guid.NewGuid().ToString("N"));
        _runtime = Path.Combine(_root, ComfyBundledRuntime.FolderName);
        Directory.CreateDirectory(_root);
    }

    [TearDown] public void RemoveIsolatedApplicationFolder()
    {
        // 이 테스트가 만든 폴더만 지운다.
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }

    private void Write(string relative, int bytes = 1)
    {
        string path = Path.Combine(_runtime, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllBytes(path, new byte[bytes]);
    }

    private void CreateCompleteRuntime(int modelBytes = 8, int manifestBytes = 8)
    {
        Write("python/python.exe");
        Write("portable_server.py");
        Write("ComfyUI/main.py");
        Write("models/vae/model.safetensors", modelBytes);
        File.WriteAllText(Path.Combine(_runtime, "package_manifest.json"),
            "{\"player\":\"NSM_DNA_02.exe\",\"models\":[{\"path\":\"models\\\\vae\\\\model.safetensors\",\"bytes\":" + manifestBytes + ",\"sha256\":\"x\"}]}");
    }

    [Test] public void ReturnsNullWithoutRuntimeFolder()
    {
        Assert.IsNull(ComfyBundledRuntime.Find(_root, 8188));
        Assert.IsNull(ComfyBundledRuntime.Find(null, 8188));
    }

    [Test] public void BuildsQuotedAbsolutePathsForTheRequestedPort()
    {
        CreateCompleteRuntime();
        var runtime = ComfyBundledRuntime.Find(_root, 8190);
        Assert.AreEqual(_runtime, runtime.Root);
        Assert.AreEqual(Path.Combine(_runtime, "python", "python.exe"), runtime.Executable);
        Assert.AreEqual(Path.Combine(_runtime, "output"), runtime.OutputDirectory);
        StringAssert.Contains("--port 8190 ", runtime.Arguments);
        StringAssert.Contains("-s \"" + Path.Combine(_runtime, "portable_server.py") + "\"", runtime.Arguments);
        StringAssert.Contains("--models-directory \"" + Path.Combine(_runtime, "models") + "\"", runtime.Arguments);
        StringAssert.Contains("--output-directory \"" + runtime.OutputDirectory + "\"", runtime.Arguments);
    }

    [Test] public void EnsureReadyCreatesWorkFoldersForACompleteRuntime()
    {
        CreateCompleteRuntime();
        var runtime = ComfyBundledRuntime.Find(_root, 8188);
        runtime.EnsureReady();
        runtime.EnsureReady(); // 반복 호출해도 안전해야 한다.
        foreach (string folder in new[] { "input", "output", "user", "temp", "logs" })
            Assert.IsTrue(Directory.Exists(Path.Combine(_runtime, folder)), folder);
    }

    [Test] public void EnsureReadyRejectsAPartiallyCopiedModel()
    {
        CreateCompleteRuntime(modelBytes: 4, manifestBytes: 8);
        var error = Assert.Throws<InvalidOperationException>(() => ComfyBundledRuntime.Find(_root, 8188).EnsureReady());
        StringAssert.Contains("model.safetensors", error.Message);
    }

    [TestCase("python/python.exe")]
    [TestCase("portable_server.py")]
    [TestCase("ComfyUI/main.py")]
    [TestCase("package_manifest.json")]
    [TestCase("models/vae/model.safetensors")]
    public void EnsureReadyNamesTheMissingFile(string relative)
    {
        CreateCompleteRuntime();
        File.Delete(Path.Combine(_runtime, relative));
        var error = Assert.Throws<InvalidOperationException>(() => ComfyBundledRuntime.Find(_root, 8188).EnsureReady());
        StringAssert.Contains(Path.GetFileName(relative), error.Message);
    }
}
