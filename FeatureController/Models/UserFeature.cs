using FeatureController.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureController.Models
{
    /// <summary>
    /// 用户基本特征，用户购买去重商品的量，用户购买去重商品类别的量
    /// </summary>
    public class UserFeature : BaseFeature
    {
        public BehaviorCountCollection UniqueScanAndBuyCount { get; set; }
        public BehaviorCountCollection UniqueScanAndBuyCategoryCount { get; set; }
        public Dictionary<int, UserCategoryFeature> UserCategorieDict { get; private set; }


        //用户的点击，收藏及加入购物车的转化率
        public BehaviorCountCollection TransferRateCollection { get; set; }

        public UserFeature() {
            UniqueScanAndBuyCount = new BehaviorCountCollection(2);
            UniqueScanAndBuyCategoryCount = new BehaviorCountCollection(2);
        }
        public UserFeature(int userId, DateTime predictDate)
        {
            UniqueScanAndBuyCount = new BehaviorCountCollection(2);
            UniqueScanAndBuyCategoryCount = new BehaviorCountCollection(2);
            UserCategorieDict = new Dictionary<int, UserCategoryFeature>();

            PredictDate = predictDate;
            Id = userId;
            TransferRateCollection = new BehaviorCountCollection(3);
        }


        public override void Update(IGrouping<int, T_UserAction> items)
        {
            this.SetUniqueScanAndBuyCount(items);
            this.SetUniqueScanAndBuyCategoryCount(items);
            base.Update(items);

            UpdateTransferRate(items);
        }

        /// <summary>
        /// 用户点击的商品去重后的数量
        /// </summary>
        /// <param name="data"></param>
        public void SetUniqueScanAndBuyCount(IGrouping<int, T_UserAction> data)
        {
            int spanCount = 24 / m_hourSpan * m_relationDays;
            int[] behaviorTypes = new int[] { 1, 4 };

            for (int i = 0; i < behaviorTypes.Length; i++)
            {
                for (int span = spanCount; span > 0; span--)
                {
                    DateTime dateTime = PredictDate.AddHours(-span * m_hourSpan);
                    int value = data.Where(d => d.behaviortype == behaviorTypes[i] && d.actiondate >= dateTime).GroupBy(d => d.itemid).Count();
                    if (value == 0)
                        break;
                    this.UniqueScanAndBuyCount.SetValue(i, span - 1, value);
                }
            }
        }

        public void SetUniqueScanAndBuyCategoryCount(IGrouping<int, T_UserAction> data)
        {
            int spanCount = 24 / m_hourSpan * m_relationDays;
            int[] behaviorTypes = new int[] { 1, 4 };

            for (int i = 0; i < behaviorTypes.Length; i++)
            {
                for (int span = spanCount; span > 0; span--)
                {
                    DateTime dateTime = PredictDate.AddHours(-span * m_hourSpan);
                    //注意这里是按category分组
                    int value = data.Where(d => d.behaviortype == behaviorTypes[i] && d.actiondate >= dateTime).GroupBy(d => d.category).Count();
                    if (value == 0)
                        break;
                    this.UniqueScanAndBuyCategoryCount.SetValue(i, span - 1, value);
                }
            }
        }

        public override void Write(System.IO.StreamWriter writer)
        {
            base.Write(writer); //先写入基本的统计信息
            UniqueScanAndBuyCount.Write(writer);    //用户点击独立商品的统计
            UniqueScanAndBuyCategoryCount.Write(writer);    //用户点击独立类别的统计
            TransferRateCollection.Write(writer);   //转化率统计
        }

        public void WriteHeaders(System.IO.StreamWriter writer)
        {
            BaseFeature.WriteHeaders(writer, "u");

            TransferRateCollection.WriteHeaders(writer, new string[] { "u_click_tranfer_{0}", "u_store_tranfer_{0}", "u_car_tranfer_{0}" });


            #region 输出表头，用户浏览、购买的去重商品的数量

            string[] behaviors = new string[] { "user_unique_item_scan_in_{0}_hours", "user_unique_item_buy_in_{0}_hours" };
            BaseFeature.WriteHeaders(writer, behaviors);

            #endregion

            #region 输出表头，用户浏览、购买的去重商品类别的数量

            behaviors = new string[] { "user_unique_category_scan_in_{0}_hours", "user_unique_category_buy_in_{0}_hours" };

            BaseFeature.WriteHeaders(writer, behaviors);

            #endregion
        }

        public override void CatchMaxValue(BaseFeature item)
        {
            base.CatchMaxValue(item);
            var userFeature = (UserFeature)item;
            UniqueScanAndBuyCount.CatchMaxValue(userFeature.UniqueScanAndBuyCount);
        }

        public override void CatchMinValue(BaseFeature item)
        {
            base.CatchMinValue(item);
            var userFeature = (UserFeature)item;
            UniqueScanAndBuyCount.CatchMinValue(userFeature.UniqueScanAndBuyCount);
        }


        //更新用户转化率
        private void UpdateTransferRate(IGrouping<int, T_UserAction> items)
        {
            for (int span = 24 * m_relationDays; span > 0; span -= m_hourSpan)
            {
                var buyCount = this.FourBehaviorCountCollection.ActionData[3][span / m_hourSpan - 1];

                DateTime dateTime = PredictDate.AddHours(-span);

                var data =
                    items.GroupBy(d => d.itemid)
                        .Where(d => d.Any(r => r.actiondate >= dateTime));

                for (int i = 1; i <= 3; i++)
                {
                    var count = data.Count(d => d.Any(r => r.behaviortype == i) && d.Any(r => r.behaviortype == 4));//收藏且购买的
                    var total = data.Count(d => d.Any(r => r.behaviortype == i));//收藏总商品数

                    double value = 1.0 * (1 + count) / (2 + total);//进行了一次平滑，假设如果没有记录，购买转化率是0.5

                    TransferRateCollection.SetValue(i - 1, span / m_hourSpan - 1, value);
                }
            }
        }
    }
}
