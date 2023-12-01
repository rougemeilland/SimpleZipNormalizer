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
            Console.WriteLine(new DateTime(0, DateTimeKind.Utc).Ticks);
            var list = new int [] {  };
            var r = list.Aggregate(-1, (x, y) => x + y);
            Console.WriteLine(r);
            Console.WriteLine("Completed.");
            _ = Console.ReadLine();
        }
    }
}
