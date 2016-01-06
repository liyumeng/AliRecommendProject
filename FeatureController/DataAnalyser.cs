using FeatureController.Bases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatureController.Models;

namespace FeatureController
{
    public class DataAnalyser
    {
        /// <summary>
        /// 将生成answers下所有文件，用于统计每天的线上购买的答案
        /// </summary>
        public static void Run()
        {
            BuyOnlineCount();
        }

        private static void BuyOnlineCount()
        {
            string dir = Global.MainDirName + @"\answers";
            if (Directory.Exists(dir) == false)
                Directory.CreateDirectory(dir);

            string filename = dir + @"\{0}.csv";
            string totalFile = dir + @"\count.csv";
            Console.WriteLine("本程序用来自动提取标准答案");
            Console.WriteLine("将输出至：{0}", dir);
            Console.WriteLine("正在尝试读取数据库...");
            using (AliRecommend2DataEntities db = new AliRecommend2DataEntities())
            {
                var itemids = db.T_Item.Select(d => d.itemid).Distinct();
                HashSet<int> itemIdSet = new HashSet<int>();
                foreach (var id in itemids)
                {
                    itemIdSet.Add(id);
                }
                Console.WriteLine("o2o数据集读取成功");

                DateTime firstDate = db.T_UserAction.Min(d => d.actiondate).Date;
                DateTime lastDate = db.T_UserAction.Max(d => d.actiondate).Date;

                StreamWriter totalWriter = new StreamWriter(totalFile);
                totalWriter.WriteLine("date,count");
                for (DateTime date = firstDate; date <= lastDate; date = date.AddDays(1))
                {
                    DateTime tommorrow = date.AddDays(1);
                    Console.WriteLine("---------------------");
                    Console.WriteLine("正在获取{0}的购买记录。", date.ToString("yyyyMMdd"));
                    var data = db.T_UserAction.Where(d => d.behaviortype == 4).Where(d => d.actiondate >= date && d.actiondate < tommorrow).Select(d => new { d.userid, d.itemid }).Distinct();
                    var items = data.ToList();
                    Console.WriteLine("正在输出...");
                    int count = 0;
                    using (StreamWriter writer = new StreamWriter(String.Format(filename, date.ToString("yyyyMMdd"))))
                    {
                        writer.WriteLine("userid,itemid");
                        foreach (var item in items)
                        {
                            if (itemIdSet.Contains(item.itemid) == false)
                                continue;
                            writer.WriteLine("{0},{1}", item.userid, item.itemid);
                            count++;
                        }
                    }
                    totalWriter.WriteLine("{0},{1}", date.ToString("yyyyMMdd"), count);
                    Console.WriteLine("{0}输出完毕.", date.ToString("yyyyMMdd"));
                }
                totalWriter.Close();
                Console.WriteLine("BuyOnlineCount 所有数据输出完毕.");
            }
        }
    }
}
