using FeatureController.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureController.Models
{
    public class ItemFeature : BaseFeature
    {
        public int CategoryId { get; set; }
        public bool IsOnline { get; set; }
        public CategoryFeature CategoryFeature { get; set; }

        //商品的点击，收藏及加入购物车的转化率
        public BehaviorCountCollection TransferRateCollection { get; set; }

        public ItemFeature()
        {
            TransferRateCollection = new BehaviorCountCollection(3);

        }
        public ItemFeature(int itemId, DateTime predictDate)
        {
            PredictDate = predictDate;
            Id = itemId;
            TransferRateCollection = new BehaviorCountCollection(3);
        }

        public override void Update(IGrouping<int, T_UserAction> items)
        {
            this.SetUniqueScanAndBuyCount(items);
            base.Update(items);
            var item = items.First();
            CategoryId = item.category;

            UpdateTransferRate(items);
        }

        //更新商品转化率
        private void UpdateTransferRate(IGrouping<int, T_UserAction> items)
        {
            for (int span = 24 * m_relationDays; span > 0; span -= m_hourSpan)
            {
                var buyCount = this.FourBehaviorCountCollection.ActionData[3][span / m_hourSpan - 1];

                DateTime dateTime = PredictDate.AddHours(-span);

                var data =
                    items.GroupBy(d => d.userid)
                        .Where(d => d.Any(r => r.actiondate >= dateTime));

                for (int i = 1; i <= 3; i++)
                {
                    var count = data.Count(d => d.Any(r => r.behaviortype == i) && d.Any(r => r.behaviortype == 4));//收藏且购买的
                    var total = data.Count(d => d.Any(r => r.behaviortype == i));//收藏总人数
                    double value = 1.0 * (1 + count) / (2 + total);//进行了一次平滑，假设如果没有记录，购买转化率是0.5

                    TransferRateCollection.SetValue(i - 1, span / m_hourSpan - 1, value);
                }
            }
        }

        public override void Write(System.IO.StreamWriter writer)
        {
            base.Write(writer);
            UniqueFourBehaviorCount.Write(writer);    //独立用户的统计

            CategoryFeature.Write(writer);
            TransferRateCollection.Write(writer);
        }
        public void WriteHeaders(System.IO.StreamWriter writer)
        {
            BaseFeature.WriteHeaders(writer, "i");

            #region 输出表头，浏览该商品的去重用户的数量

            string[] behaviors = new string[] { "item_unique_user_click_in_{0}_hours", "item_unique_user_store_in_{0}_hours", "item_unique_user_car_in_{0}_hours", "item_unique_user_buy_in_{0}_hours" };
            BaseFeature.WriteHeaders(writer, behaviors);

            #endregion

            CategoryFeature.WriteHeaders(writer);

            TransferRateCollection.WriteHeaders(writer, new string[] { "i_click_tranfer_{0}", "i_store_tranfer_{0}", "i_car_tranfer_{0}" });
        }

        //去重用户后的统计


        /// <summary>
        /// 点击该商品的用户去重后的数量
        /// </summary>
        /// <param name="data"></param>
        public void SetUniqueScanAndBuyCount(IGrouping<int, T_UserAction> data)
        {
            int spanCount = 24 / m_hourSpan * m_relationDays;
            int[] behaviorTypes = new int[] { 1, 2, 3, 4 };

            for (int i = 0; i < behaviorTypes.Length; i++)
            {
                for (int span = spanCount; span > 0; span--)
                {
                    DateTime dateTime = PredictDate.AddHours(-span * m_hourSpan);
                    int value = data.Where(d => d.behaviortype == behaviorTypes[i] && d.actiondate >= dateTime).GroupBy(d => d.userid).Count();
                    if (value == 0)
                        break;
                    this.UniqueFourBehaviorCount.SetValue(i, span - 1, value);
                }
            }
        }
    }
}
