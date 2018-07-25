using System.Runtime.InteropServices;

namespace Enum
{
    enum Operations
    {
        Create,
        update,
        Delete
    }

    enum Status
    {
        success,
        error,
        exists,
        notexist,
        unknown,
    }

    enum Records
    {
        A,
        AAA,
        CNAME,
        MX,
        NS,
        PTR,
        SRV,
        TXT
    }

    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    };
}
