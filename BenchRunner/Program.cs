using System;
using GlutenFree.OddJob.PerfTests;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
           /* Console.WriteLine("Press enter to start SQLite Store Test");
            Console.ReadLine();
            System.Console.WriteLine(new Perftest1().Perftest_Sqlite_Store(1000));
            */
           Console.WriteLine("Press enter to start Serialize-Fast Test");
           Console.ReadLine();
           System.Console.WriteLine(new Perftest1().JobSerialzierPerfTestSlow(100000));
           System.Console.WriteLine(new Perftest1().JobSerialzierPerfTestSlow(100000));
           Console.WriteLine("Press enter to start Serialize-Slow Test");
            Console.ReadLine();
            System.Console.WriteLine(new Perftest1().JobSerialzierPerfTest_Fast(100000));
            System.Console.WriteLine(new Perftest1().JobSerialzierPerfTest_Fast(100000));
            Console.WriteLine("Press enter to start the MT Serialize Test");
            Console.ReadLine();
            Console.WriteLine(new Perftest1().PerfTest_MultiThread_Serialized(4,500000));
            Console.WriteLine("Press enter to start Base Test");
            Console.ReadLine();
            System.Console.WriteLine(new Perftest1().PerfTest(500000));
            Console.WriteLine("Press enter to start the MT Test");
            Console.ReadLine();
            Console.WriteLine(new Perftest1().PerfTest_MultiThread_Base(4,500000));
            
            Console.WriteLine("Press Enter to Exit");
            Console.ReadLine();
        }
    }
}