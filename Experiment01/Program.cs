using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utility;

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
            var fragments = new FragmentSet<int, int>();
            fragments.AddFragment(new FragmentSetElement<int, int>(0, 100));
            fragments.RemoveFragment(new FragmentSetElement<int, int>(10, 10));
            fragments.RemoveFragment(new FragmentSetElement<int, int>(30, 10));
            fragments.RemoveFragment(new FragmentSetElement<int, int>(20, 10));
            fragments.RemoveFragment(new FragmentSetElement<int, int>(40, 60));
            fragments.RemoveFragment(new FragmentSetElement<int, int>(0, 10));

            Console.WriteLine(fragments);

            Console.WriteLine("Completed.");
            Console.Beep();
            _ = Console.ReadLine();
        }
    }
}
