namespace OmniFrame.Core
{
    public static class SignalNames
    {
        public static class Robot1
        {
            public const string MoveToPick = "Robot1_MoveToPick";
            public const string AtPick = "Robot1_AtPick";
            public const string Pick = "Robot1_Pick";
            public const string PickDone = "Robot1_PickDone";
            public const string MoveToPlace = "Robot1_MoveToPlace";
            public const string AtPlace = "Robot1_AtPlace";
            public const string Place = "Robot1_Place";
        }

        public static class Robot2
        {
            public const string MoveToPick = "Robot2_MoveToPick";
            public const string AtPick = "Robot2_AtPick";
            public const string Pick = "Robot2_Pick";
            public const string PickDone = "Robot2_PickDone";
            public const string MoveToPlace = "Robot2_MoveToPlace";
            public const string AtPlace = "Robot2_AtPlace";
            public const string Place = "Robot2_Place";
        }

        public static class Robot3
        {
            public const string MoveToPick = "Robot3_MoveToPick";
            public const string AtPick = "Robot3_AtPick";
            public const string Pick = "Robot3_Pick";
            public const string PickDone = "Robot3_PickDone";
            public const string MoveToPlace = "Robot3_MoveToPlace";
            public const string AtPlace = "Robot3_AtPlace";
            public const string Place = "Robot3_Place";
        }

        public static class Camera1
        {
            public const string Trigger = "Camera1_Trigger";
            public const string Done = "Camera1_Done";
        }

        public static class Camera2
        {
            public const string Trigger = "Camera2_Trigger";
            public const string Done = "Camera2_Done";
        }

        public static class Buffer
        {
            public const string ClampOff = "BufferClamp_Off";
            public const string ClampOn = "BufferClamp_On";
            public const string ConveyorForward = "BufferConveyor_Forward";
            public const string ConveyorStop = "BufferConveyor_Stop";
            public const string PosArrived = "BufferPos_Arrived";
            public const string MaterialPresent = "MaterialPresent";
        }

        public static class Transfer
        {
            public const string MoveToPick = "Transfer_MoveToPick";
            public const string AtPick = "Transfer_AtPick";
            public const string MoveToPlace = "Transfer_MoveToPlace";
            public const string AtPlace = "Transfer_AtPlace";
        }

        public static class Cylinder1
        {
            public const string Extend = "Cylinder1_Extend";
            public const string Extended = "Cylinder1_Extended";
            public const string Retract = "Cylinder1_Retract";
            public const string Retracted = "Cylinder1_Retracted";
        }

        public static class Cylinder2
        {
            public const string Extend = "Cylinder2_Extend";
            public const string Extended = "Cylinder2_Extended";
            public const string Retract = "Cylinder2_Retract";
            public const string Retracted = "Cylinder2_Retracted";
        }

        public static class Gripper
        {
            public const string Close = "Gripper_Close";
            public const string Open = "Gripper_Open";
        }

        public static class UnloadGripper
        {
            public const string Close = "UnloadGripper_Close";
            public const string Open = "UnloadGripper_Open";
        }

        public static class UnloadTransfer
        {
            public const string MoveToPick = "UnloadTransfer_MoveToPick";
            public const string AtPick = "UnloadTransfer_AtPick";
            public const string MoveToPlace = "UnloadTransfer_MoveToPlace";
            public const string AtPlace = "UnloadTransfer_AtPlace";
        }

        public static class Lighting1
        {
            public const string On = "Lighting1_On";
            public const string Ready = "Lighting1_Ready";
        }

        public static class Lighting2
        {
            public const string On = "Lighting2_On";
            public const string Ready = "Lighting2_Ready";
        }
    }
}
