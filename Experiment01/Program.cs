using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace Experiment01
{
    internal class Program
    {
        static Program()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:未使用のパラメーターを削除します", Justification = "<保留中>")]
        private static void Main(string[] args)
        {
            // TODO: コンソールで進捗を表示している最中にスクロールが発生するとカーソルが正しい位置に戻らない問題の修正方法の検討
            while (true)
            {
                for (var count = 0; count < 40; ++count)
                    PrintMessage($"row {count}");
                PrintMessage("このメッセージが消えてはならない。");
                PrintProgress($"progress: {new string('*', 300)}進捗メッセージはここまで");
                PrintProgress($"progress: {new string('*', 300)}進捗メッセージはここまで");
                PrintProgress($"progress: {new string('*', 300)}進捗メッセージはここまで");
                PrintProgress($"progress: {new string('*', 300)}進捗メッセージはここまで");
                PrintProgress($"progress: {new string('*', 300)}進捗メッセージはここまで");
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
            //_ = Console.ReadLine();
        }

        private static void PrintMessage(string message)
        {
            Console.Write("\u001b[0J");
            Console.WriteLine(message);
        }

        private static void PrintProgress(string message)
        {
#if true
            var length = message.Length * 2;
            var rows = (length + Console.WindowWidth - 1) / Console.WindowWidth;
            Console.Write($"\x1b[0J{new string('\n', rows)}\x1b[{rows}A");
            var (cursorLeft, cursorTop) = Console.GetCursorPosition();
            Console.Write($"{message}\x1b[{cursorTop + 1};{cursorLeft + 1}H");
            //Console.SetCursorPosition(cursorPosition.Left, cursorPosition.Top);
#else
            var cursorPosition = Console.GetCursorPosition();
            Console.Write("\u001b[0J");
            Console.Write(message);
            Console.SetCursorPosition(cursorPosition.Left, cursorPosition.Top);
#endif
        }
    }
}
