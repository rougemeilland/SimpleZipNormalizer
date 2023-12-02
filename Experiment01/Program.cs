using System;
using System.Linq;
using System.Text;

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
            var minimumDateTime = new DateTime(DateTime.MinValue.Ticks, DateTimeKind.Utc);
            var maximumDateTime = new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Utc);
            Console.WriteLine($"DateTime.MinValue={minimumDateTime}");
            Console.WriteLine($"DateTime.MaxValue={maximumDateTime}");
            Console.WriteLine($"DateTime.FromFileTime(long.MinValue)={DateTime.FromFileTime(0)}");
            Console.WriteLine($"DateTime.FromFileTime(long.MaxValue)={DateTime.FromFileTime(long.MaxValue)}");
            Console.WriteLine("Completed.");
            _ = Console.ReadLine();
        }
    }
}
