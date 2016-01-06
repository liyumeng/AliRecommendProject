using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatureController.Bases;

namespace FeatureController.Models
{
    public class FeatureCollection
    {
        public List<UserFeature> UserFeatureList { get; set; }
        public List<UserItemFeature> UserItemFeatureList { get; set; }

        public Dictionary<int, CategoryFeature> CategoryFeatureDict { get; set; }

        public Dictionary<int, ItemFeature> ItemFeatureDict { get; set; }

        public HashSet<string> PositiveSet { get; private set; }

        public HashSet<int> OnlineItemSet { get; private set; }

        public FeatureCollection()
        {
            UserFeatureList = new List<UserFeature>();
            UserItemFeatureList = new List<UserItemFeature>();
            CategoryFeatureDict = new Dictionary<int, CategoryFeature>();
            ItemFeatureDict = new Dictionary<int, ItemFeature>();
            PositiveSet = new HashSet<string>();
            OnlineItemSet = new HashSet<int>();
        }

        Random rand = new Random((int)DateTime.Now.Ticks);
        public void Write(string filename)
        {
            StreamWriter writer = new StreamWriter(filename);
            UserItemFeatureList[0].WriteHeaders(writer);
            writer.WriteLine();
            int rate = Global.NegativeSampleRate * 10;
            bool onlyOnline = Global.OnlyOnline;

            foreach (var item in UserItemFeatureList)
            {
                if (onlyOnline && item.ItemFeature.IsOnline == false)
                {
                    continue;
                }
                else if (onlyOnline == false && item.Label == false && rand.Next(1, 1000) > rate)
                {
                    continue;
                }
                item.Write(writer);
                writer.WriteLine();
            }
            writer.Close();
        }

        public bool CheckIsPositive(int userid, int itemid)
        {
            return PositiveSet.Contains(String.Format("{0},{1}", userid, itemid));
        }

        public bool CheckIsOnline(int itemid)
        {
            return OnlineItemSet.Contains(itemid);
        }

        public void UpdateOnlineItemSet(AliRecommend2DataEntities db)
        {
            OnlineItemSet.Clear();
            var onlineData = db.T_Item.Select(d => d.itemid).Distinct();
            foreach (var id in onlineData)
            {
                OnlineItemSet.Add(id);
            }
        }

        public void UpdatePositiveSet(AliRecommend2DataEntities db, DateTime predictDate)
        {
            DateTime tomorrow = predictDate.AddDays(1);
            var data = db.T_UserAction.Where(d => d.actiondate >= predictDate && d.actiondate < tomorrow && d.behaviortype == 4).Select(d => new { d.userid, d.itemid });

            PositiveSet.Clear();
            foreach (var record in data)
            {
                PositiveSet.Add(String.Format("{0},{1}", record.userid, record.itemid));
            }
        }

        public void Normalize()
        {
            foreach (var item in UserItemFeatureList)
            {
                item.Normalize(MaxUserItemFeature, MinUserItemFeature);
            }
            foreach (var item in UserFeatureList)
            {
                item.Normalize(MaxUserFeature, MinUserFeature);
                foreach (var pair in item.UserCategorieDict)
                {
                    pair.Value.Normalize(MaxUserCategoryFeature, MinUserCategoryFeature);
                }
            }
            foreach (var pair in ItemFeatureDict)
            {
                pair.Value.Normalize(MaxItemFeature, MinItemFeature);
            }
            foreach (var pair in CategoryFeatureDict)
            {
                pair.Value.Normalize(MaxCategoryFeature, MinCategoryFeature);
            }
        }

        public void AddCategoryFeature(CategoryFeature categoryFeature)
        {
            this.CategoryFeatureDict.Add(categoryFeature.Id, categoryFeature);
            MaxCategoryFeature.CatchMaxValue(categoryFeature);
            MinCategoryFeature.CatchMinValue(categoryFeature);
        }

        public void AddItemFeature(ItemFeature itemFeature)
        {
            this.ItemFeatureDict.Add(itemFeature.Id, itemFeature);
            MaxItemFeature.CatchMaxValue(itemFeature);
            MinItemFeature.CatchMinValue(itemFeature);
        }

        public void AddUserFeature(UserFeature userFeature)
        {
            this.UserFeatureList.Add(userFeature);
            MaxUserFeature.CatchMaxValue(userFeature);
            MinUserFeature.CatchMinValue(userFeature);

        }

        public void AddUserItemFeature(UserItemFeature userItemFeature)
        {
            this.UserItemFeatureList.Add(userItemFeature);
            MaxUserItemFeature.CatchMaxValue(userItemFeature);
            MinUserItemFeature.CatchMinValue(userItemFeature);
        }

        public void AddUserCategoryFeature(UserCategoryFeature userCategoryFeature)
        {
            MaxUserCategoryFeature.CatchMaxValue(userCategoryFeature);
            MinUserCategoryFeature.CatchMinValue(userCategoryFeature);
        }

        public CategoryFeature MaxCategoryFeature = new CategoryFeature();
        public CategoryFeature MinCategoryFeature = new CategoryFeature();

        public ItemFeature MaxItemFeature = new ItemFeature();
        public ItemFeature MinItemFeature = new ItemFeature();

        public UserFeature MaxUserFeature = new UserFeature();
        public UserFeature MinUserFeature = new UserFeature();

        public UserItemFeature MaxUserItemFeature = new UserItemFeature();
        public UserItemFeature MinUserItemFeature = new UserItemFeature();

        public UserCategoryFeature MaxUserCategoryFeature = new UserCategoryFeature();
        public UserCategoryFeature MinUserCategoryFeature = new UserCategoryFeature();


    }
}
