using FeatureController.Bases;
using FeatureController.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureController
{
    public class FeatureManager
    {
        public DateTime PredictDate { get; private set; }

        public DateTime StartDate { get; private set; }

        private int m_relationDays;

        public FeatureManager(DateTime dateToPredict)
        {
            PredictDate = dateToPredict;
            m_relationDays = Global.RelationDays;
            StartDate = PredictDate.AddDays(-m_relationDays);
        }

        public void Run()
        {
            using (AliRecommend2DataEntities db = new AliRecommend2DataEntities())
            {
                ((IObjectContextAdapter)db).ObjectContext.CommandTimeout = 600;
                long startTime = DateTime.Now.Ticks;
                 var data = db.T_UserAction.Where(d => d.actiondate >= StartDate && d.actiondate < PredictDate).ToList();
                Console.WriteLine("数据库数据读取完毕，正在进行处理中...");
                FeatureCollection features = new FeatureCollection();

                features.UpdateOnlineItemSet(db);
                Console.WriteLine("正在读取正例数据...");
                features.UpdatePositiveSet(db, PredictDate);

                var categories = data.GroupBy(d => d.category);
                Console.WriteLine("正在添加类别特征...");
                foreach (var categoryData in categories)
                {
                    CategoryFeature categoryFeature = new CategoryFeature(categoryData.Key, PredictDate);
                    categoryFeature.Update(categoryData);
                    features.AddCategoryFeature(categoryFeature);
                }

                
                var items = data.GroupBy(d => d.itemid);
                Console.WriteLine("正在添加商品特征...");
                //按商品分组，itemData是每个商品的所有记录
                foreach (var itemData in items)
                {
                    ItemFeature itemFeature = new ItemFeature(itemData.Key, PredictDate);
                    itemFeature.Update(itemData);
                    itemFeature.IsOnline = features.CheckIsOnline(itemFeature.Id);
                    //向ItemFeature绑定CategoryFeature
                    itemFeature.CategoryFeature = features.CategoryFeatureDict[itemFeature.CategoryId];
                    features.AddItemFeature(itemFeature);
                }

                var users = data.GroupBy(d => d.userid);
                Console.WriteLine("正在添加用户相关特征...");
                foreach (var userData in users)
                {
                    UserFeature userFeature = new UserFeature(userData.Key, PredictDate);
                    userFeature.Update(userData);
                    features.AddUserFeature(userFeature);

                    var userCategories = userData.GroupBy(d => d.category);
                    foreach (var userCategoryData in userCategories)
                    {
                        UserCategoryFeature userCategoryFeature = new UserCategoryFeature(userCategoryData.Key, PredictDate);
                        userCategoryFeature.Update(userCategoryData);
                        userFeature.UserCategorieDict.Add(userCategoryFeature.CategoryId, userCategoryFeature);
                        features.AddUserCategoryFeature(userCategoryFeature);
                    }

                    var userItems = userData.GroupBy(d => d.itemid);
                    foreach (var userItemData in userItems)
                    {
                        UserItemFeature userItemFeature = new UserItemFeature(userFeature, userItemData.Key);
                        userItemFeature.Update(userItemData);
                        //是否是正例
                        userItemFeature.Label = features.CheckIsPositive(userItemFeature.UserId, userItemFeature.ItemId);
                        //向UserItemFeature 绑定ItemFeature
                        userItemFeature.ItemFeature = features.ItemFeatureDict[userItemFeature.ItemId];
                        //向UserItemFeature绑定UserCategoryFeature
                        userItemFeature.UserCategoryFeature = userItemFeature.UserFeature.UserCategorieDict[userItemFeature.ItemFeature.CategoryId];
                        features.AddUserItemFeature(userItemFeature);
                    }
                }
                if (Global.Normalized)
                {
                    features.Normalize();
                }
                string dirname = Global.GetDirectoryName();
                if (Directory.Exists(dirname) == false)
                    Directory.CreateDirectory(dirname);
                string filename = dirname + Global.GetFileName(PredictDate);
                features.Write(filename);

                Console.WriteLine("运行完毕,耗时{0}s", (DateTime.Now.Ticks - startTime) / 10000000);
                Console.WriteLine("{0} 输出完毕。", filename);
            }
            //Console.ReadKey();
        }

    }
}
