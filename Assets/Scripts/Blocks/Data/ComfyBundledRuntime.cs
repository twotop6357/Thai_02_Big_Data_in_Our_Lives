using System;
using System.IO;
using UnityEngine;

namespace Exhibition.Blocks
{
    /// <summary>
    /// 실행 파일 옆 <c>ComfyRuntime</c> 폴더(동봉 Python·ComfyUI·모델)의 실행 정보.
    /// 폴더 위치가 바뀌어도 실행 파일 기준 상대 위치로 찾으므로 설정 파일을 고칠 필요가 없다.
    /// </summary>
    public sealed class ComfyBundledRuntime
    {
        /// <summary>실행 파일과 같은 폴더에 두는 런타임 폴더 이름.</summary>
        public const string FolderName = "ComfyRuntime";
        /// <summary>동봉 서버의 최소 준비 대기 시간(초). 첫 실행의 모델 로딩을 고려한다.</summary>
        public const int MinimumStartupTimeoutSeconds = 300;

        // 서버가 실행 중에 쓰는 폴더. 배포본에 없으면 실행 전에 만든다.
        private static readonly string[] WorkFolders = { "input", "output", "user", "temp", "logs" };

        [Serializable] private sealed class Manifest { public Model[] models; }
        // path: 런타임 폴더 기준 상대 경로, bytes: 모델 파일 크기(바이트).
        [Serializable] private sealed class Model { public string path; public long bytes; }

        /// <summary>런타임 폴더의 절대 경로. 서버의 작업 폴더로도 쓴다.</summary>
        public string Root { get; }
        /// <summary>동봉 Python 실행 파일의 절대 경로.</summary>
        public string Executable { get; }
        /// <summary>서버 실행 인자. 경로는 모두 따옴표로 감싼 절대 경로다.</summary>
        public string Arguments { get; }
        /// <summary>서버가 생성 이미지를 저장하는 폴더의 절대 경로.</summary>
        public string OutputDirectory { get; }

        private ComfyBundledRuntime(string root, int port)
        {
            Root = root;
            Executable = Path.Combine(root, "python", "python.exe");
            OutputDirectory = Path.Combine(root, "output");
            Arguments = "-s " + Quote(Path.Combine(root, "portable_server.py")) +
                " --listen 127.0.0.1 --port " + port +
                " --disable-auto-launch --disable-all-custom-nodes --offline" +
                " --models-directory " + Quote(Path.Combine(root, "models")) +
                " --input-directory " + Quote(Path.Combine(root, "input")) +
                " --output-directory " + Quote(OutputDirectory) +
                " --user-directory " + Quote(Path.Combine(root, "user")) +
                " --temp-directory " + Quote(Path.Combine(root, "temp"));
        }

        /// <summary>
        /// <paramref name="applicationRoot"/>(실행 파일이 있는 폴더) 아래의 동봉 런타임을 찾는다.
        /// </summary>
        /// <param name="port">서버가 수신할 로컬 TCP 포트.</param>
        /// <returns>런타임 폴더가 없으면 null.</returns>
        public static ComfyBundledRuntime Find(string applicationRoot, int port)
        {
            if (string.IsNullOrEmpty(applicationRoot)) return null;
            string root = Path.Combine(Path.GetFullPath(applicationRoot), FolderName);
            return Directory.Exists(root) ? new ComfyBundledRuntime(root, port) : null;
        }

        /// <summary>
        /// 서버 실행 직전에 호출한다. 실행 파일·서버 스크립트·모델이 온전한지 확인하고 작업 폴더를 만든다.
        /// </summary>
        /// <exception cref="InvalidOperationException">복사가 덜 됐거나 파일이 빠진 경우. 메시지에 빠진 파일을 적는다.</exception>
        public void EnsureReady()
        {
            RequireFile(Executable);
            RequireFile(Path.Combine(Root, "portable_server.py"));
            RequireFile(Path.Combine(Root, "ComfyUI", "main.py"));
            string manifestPath = Path.Combine(Root, "package_manifest.json");
            RequireFile(manifestPath);
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(manifestPath));
            if (manifest?.models == null || manifest.models.Length == 0)
                throw new InvalidOperationException("Bundled ComfyRuntime manifest lists no models: " + manifestPath);
            foreach (var model in manifest.models)
            {
                var file = new FileInfo(Path.Combine(Root, model.path));
                if (!file.Exists || file.Length != model.bytes)
                    throw new InvalidOperationException("Bundled ComfyRuntime model is missing or incomplete: " + file.FullName);
            }
            foreach (string folder in WorkFolders) Directory.CreateDirectory(Path.Combine(Root, folder));
        }

        private static void RequireFile(string path)
        {
            if (!File.Exists(path)) throw new InvalidOperationException("Bundled ComfyRuntime file is missing: " + path);
        }

        // Windows 경로에는 큰따옴표가 올 수 없고 여기서 넘기는 경로는 역슬래시로 끝나지 않는다.
        private static string Quote(string path) => "\"" + path + "\"";
    }
}
