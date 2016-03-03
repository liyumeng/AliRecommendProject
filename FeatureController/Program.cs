using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatureController.Bases;

namespace FeatureController
{
    class Program
    {
        static void InitialData()
        {
            Global.PrintConfig();

            Console.WriteLine("确认配置无误后，按任意键继续...");
            Console.ReadKey();
            DateTime startTime = new DateTime(2014, 11, 21);
            DateTime endTime = new DateTime(2014, 12, 18);
            for (DateTime date = startTime; date <= endTime; date = date.AddDays(1))
            {
                Console.WriteLine("---------------------");
                Console.WriteLine("正在处理{0}训练集。", date.ToString("yyyyMMdd"));
                var manager = new FeatureManager(date);
                manager.Run();
            }

            Global.OnlyOnline = true;
            Global.PrintConfig();
            for (DateTime date = endTime.AddDays(1); date >= endTime.AddDays(-5); date = date.AddDays(-1))
            {
                Console.WriteLine("---------------------");
                Console.WriteLine("正在处理{0}测试集。", date.ToString("yyyyMMdd"));
                var manager = new FeatureManager(date);
                manager.Run();
            }
        }

        static void MinInitialData()
        {
            Global.PrintConfig();
            Console.ReadKey();
            Console.WriteLine("确认配置无误后，按任意键继续...");
            Global.OnlyOnline = false;
            DateTime date = new DateTime(2014, 12, 17);
            Console.WriteLine("---------------------");
            Console.WriteLine("正在处理{0}训练集。", date.ToString("yyyyMMdd"));
            var manager = new FeatureManager(date);
            manager.Run();
            Global.OnlyOnline = true;
            for (int i = 16; i < 20; i++)
            {
                date = new DateTime(2014, 12, i);
                Console.WriteLine("---------------------");
                Console.WriteLine("正在处理{0}测试集。", date.ToString("yyyyMMdd"));
                manager = new FeatureManager(date);
                manager.Run();
            }
        }

        static void Main(string[] args)
        {
            Global.HourSpan = 8;
            Global.RelationDays = 3;
            Global.OnlyOnline = false;
            Global.NegativeSampleRate = 5;
            Global.Normalized = false;

            InitialData();

        }
    }
}


