using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace Core
{
    /// <summary>
    /// 이 앱의 핸들을 물려받지 않는 창 없는 자식 프로세스.
    /// <para>
    /// <c>Process.Start(UseShellExecute=false)</c>는 상속 가능한 핸들을 모두 자식에게 넘긴다.
    /// 앱보다 오래 사는 자식(ComfyUI 서버)이 RFID 수신 소켓을 물려받으면 앱을 껐다 켰을 때
    /// 포트가 계속 점유되어 수신을 시작하지 못한다. 그래서 상속 없이 직접 실행한다.
    /// </para>
    /// </summary>
    public sealed class DetachedProcess : IDisposable
    {
        private const uint CreateNoWindow = 0x08000000;
        private const uint CreateUnicodeEnvironment = 0x00000400;
        private const uint WaitObject0 = 0;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct StartupInfo
        {
            public int cb;
            public string lpReserved, lpDesktop, lpTitle;
            public int dwX, dwY, dwXSize, dwYSize, dwXCountChars, dwYCountChars, dwFillAttribute, dwFlags;
            public short wShowWindow, cbReserved2;
            public IntPtr lpReserved2, hStdInput, hStdOutput, hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ProcessInformation
        {
            public IntPtr hProcess, hThread;
            public int dwProcessId, dwThreadId;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessW(string applicationName, StringBuilder commandLine,
            IntPtr processAttributes, IntPtr threadAttributes, bool inheritHandles, uint creationFlags,
            IntPtr environment, string currentDirectory, ref StartupInfo startupInfo, out ProcessInformation processInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetExitCodeProcess(IntPtr process, out uint exitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetHandleInformation(IntPtr handle, uint mask, uint flags);

        private const uint HandleFlagInherit = 0x00000001;

        /// <summary>
        /// 핸들(소켓 등)을 자식 프로세스가 물려받지 못하게 표시한다. 이 클래스를 거치지 않는
        /// 다른 코드가 <c>Process.Start</c>로 자식을 만들어도 포트가 붙잡히지 않게 하는 이중 방어다.
        /// </summary>
        /// <returns>false면 표시에 실패했다(Windows가 아니거나 핸들이 유효하지 않음).</returns>
        public static bool PreventInheritance(IntPtr handle) =>
            Environment.OSVersion.Platform == PlatformID.Win32NT && SetHandleInformation(handle, HandleFlagInherit, 0);

        // 프로세스 핸들. Dispose 후에는 IntPtr.Zero.
        private IntPtr _handle;

        /// <summary>자식 프로세스 ID.</summary>
        public int Id { get; }

        private DetachedProcess(IntPtr handle, int id) { _handle = handle; Id = id; }

        /// <summary>true면 프로세스가 끝났다. Dispose 후에는 확인할 수 없어 true를 돌려준다.</summary>
        public bool HasExited => _handle == IntPtr.Zero || WaitForSingleObject(_handle, 0) == WaitObject0;

        /// <summary>종료 코드. 아직 실행 중이거나 Dispose 후면 -1.</summary>
        public int ExitCode =>
            HasExited && _handle != IntPtr.Zero && GetExitCodeProcess(_handle, out uint code) ? unchecked((int)code) : -1;

        /// <summary>
        /// 핸들 상속 없이 창 없는 프로세스를 시작한다. 환경 변수는 이 앱과 같다.
        /// </summary>
        /// <param name="executable">실행 파일의 경로.</param>
        /// <param name="arguments">이미 따옴표 처리가 끝난 인자 문자열.</param>
        /// <param name="workingDirectory">작업 폴더. 비어 있으면 이 앱의 현재 폴더.</param>
        /// <exception cref="Win32Exception">실행하지 못한 경우. NativeErrorCode에 Windows 오류 코드가 있다.</exception>
        public static DetachedProcess Start(string executable, string arguments, string workingDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(executable)) throw new ArgumentException("Executable path is empty.", nameof(executable));
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                throw new PlatformNotSupportedException("DetachedProcess supports Windows only.");
            // CreateProcessW가 버퍼를 고쳐 쓸 수 있어 수정 가능한 버퍼로 넘긴다.
            var commandLine = new StringBuilder("\"" + executable + "\"" + (string.IsNullOrEmpty(arguments) ? "" : " " + arguments));
            var startup = new StartupInfo { cb = Marshal.SizeOf(typeof(StartupInfo)) };
            if (!CreateProcessW(null, commandLine, IntPtr.Zero, IntPtr.Zero, false, CreateNoWindow | CreateUnicodeEnvironment,
                    IntPtr.Zero, string.IsNullOrWhiteSpace(workingDirectory) ? null : workingDirectory, ref startup, out var information))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            CloseHandle(information.hThread);
            return new DetachedProcess(information.hProcess, information.dwProcessId);
        }

        /// <summary>핸들만 닫는다. 프로세스는 끝내지 않는다. 여러 번 호출해도 된다.</summary>
        public void Dispose()
        {
            if (_handle == IntPtr.Zero) return;
            CloseHandle(_handle);
            _handle = IntPtr.Zero;
        }
    }
}
