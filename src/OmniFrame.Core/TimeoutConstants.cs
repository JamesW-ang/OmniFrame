namespace OmniFrame.Core
{
    public static class TimeoutConstants
    {
        public const int SignalWaitDefault = 3000;
        public const int SignalWaitShort = 1000;
        public const int SignalWaitLong = 5000;
        public const int SignalWaitInstant = 500;

        public const int ThreadJoinTimeout = 5000;
        public const int ReconnectInterval = 5000;

        public const int WatchdogDefault = 5000;
        public const int WatchdogMin = 1000;
        public const int WatchdogMax = 60000;

        public const int PlaceDelay = 500;
        public const int CylinderWait = 2000;
        public const int TransferWait = 2000;
    }
}
