using System;
using Utility;

namespace ZipUtility
{
    /// <summary>
    /// ZIP アーカイブの検証結果を示すオブジェクトのクラスです。
    /// </summary>
    public class ZipArchiveValidationResult
    {
        internal ZipArchiveValidationResult(ZipArchiveValidationResultId resultId, String message, Exception? catchedException)
        {
            ResultId = resultId;
            Message = message;
            CatchedException = catchedException;
        }

        /// <summary>
        /// 検証結果の概要を示す値を取得します。
        /// </summary>
        public ZipArchiveValidationResultId ResultId { get; }

        /// <summary>
        /// 検証結果のメッセージ文字列を取得します。
        /// </summary>
        public String Message { get; }

        /// <summary>
        /// 検証中に発生した例外オブジェクトを取得します。例外が発生していなければ null が返ります。
        /// </summary>
        public Exception? CatchedException { get; }

        /// <summary>
        /// 検証中に発生した例外の詳細な情報を示す文字列を取得します。例外が発生していなければ null が返ります。
        /// </summary>
        public String? CatchedExceptionLongMessage => CatchedException?.GetFullExceptionMessage();
    }
}
