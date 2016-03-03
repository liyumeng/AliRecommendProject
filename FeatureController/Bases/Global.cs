using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureController.Bases
{
    public static class Global
    {
        /// <summary>
        /// 每多少小时作为一次统计单元
        /// </summary>
        public static int HourSpan = 8;
        /// <summary>
        /// 统计前多少天的数据
        /// </summary>
        public static int RelationDays = 3;

        //public readonly static int DefaultMinHourCount = 2 * 24 * RelationDays / HourSpan;
        public static readonly int DefaultMinHourCount = 24 * RelationDays + 1;
        /// <summary>
        /// 是否只输出线上产品，也意味着这是在输出测试集
        /// </summary>
        public static bool OnlyOnline = false;

        public static bool Normalized = false;
        public static readonly string MainDirName = @"F:\AliRecommendData\434维特征搜索作业数据";
        public static readonly string DirName = MainDirName + @"\{0}days_{1}hourspan_{2}\";

        /// <summary>
        /// 负例抽样比例，如果只输出线上产品，该值不起作用
        /// </summary>
        public static int NegativeSampleRate = 5;

        static Global()
        {
            if (Directory.Exists(MainDirName) == false)
                Directory.CreateDirectory(MainDirName);
        }

        public static void PrintConfig()
        {
            Console.WriteLine("------------------------------------");
            Console.WriteLine("相关天数:{0}, 时间每片：{1}小时", Global.RelationDays, Global.HourSpan);
            Console.WriteLine("只输出线上商品：{0}（用作测试集）", Global.OnlyOnline);
            Console.WriteLine("负例抽样率:{0}%", Global.NegativeSampleRate);
            Console.WriteLine("是否进行归一化：{0}", Global.Normalized);
            Console.WriteLine("输出目录：{0}", Global.GetDirectoryName());
            Console.WriteLine("------------------------------------");
        }

        public static string GetDirectoryName()
        {
            return String.Format(DirName, Global.RelationDays, Global.HourSpan, Global.Normalized ? "normal" : "unnormal");
        }
        public static string GetFileName(DateTime dateTime)
        {
            if (OnlyOnline)
                return $"testset_{dateTime.ToString("yyyyMMdd")}_{Global.RelationDays}_{Global.HourSpan}.csv";
            else
                return
                    $"trainingset_{dateTime.ToString("yyyyMMdd")}_{Global.RelationDays}_{Global.HourSpan}_{Global.NegativeSampleRate}.csv";
        }
    }
}
