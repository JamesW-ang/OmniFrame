using System;

namespace OmniFrame.Common
{
    public class ValueChangedEventArgs<T> : EventArgs
    {
        public T OldValue { get; }
        public T NewValue { get; }

        public ValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    public class StateChangedEventArgs : EventArgs
    {
        public int OldState { get; }
        public int NewState { get; }
        public string Message { get; }

        public StateChangedEventArgs(int oldState, int newState, string message = "")
        {
            OldState = oldState;
            NewState = newState;
            Message = message;
        }
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; }
        public int Length { get; }
        public DateTime Timestamp { get; }

        public DataReceivedEventArgs(byte[] data, int length)
        {
            Data = data;
            Length = length;
            Timestamp = DateTime.Now;
        }
    }
}
