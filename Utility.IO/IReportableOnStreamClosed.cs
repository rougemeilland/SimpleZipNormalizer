using System;

namespace Utility.IO
{
    public interface IReportableOnStreamClosed<POSITION_T>
    {
        /// <summary>
        /// ストリームが閉じた際に発生するイベントです。
        /// </summary>
        event EventHandler<OnStreamClosedEventArgs<POSITION_T>>? OnStreamClosed;
    }
}
