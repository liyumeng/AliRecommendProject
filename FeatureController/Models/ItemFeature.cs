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

        public ItemFeature() { }
        public ItemFeature(int itemId, DateTime predictDate)
        {
            PredictDate = predictDate;
            Id = itemId;
            TransferRateCollection = new BehaviorCountCollection(3);
        }

        public override void Update(IGrouping<int, T_UserAction> items)
        {
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
            CategoryFeature.Write(writer);

            TransferRateCollection.Write(writer);
        }
        public void WriteHeaders(System.IO.StreamWriter writer)
        {
            BaseFeature.WriteHeaders(writer, "i");
            CategoryFeature.WriteHeaders(writer, "c");

            TransferRateCollection.WriteHeaders(writer, new string[] { "i_click_tranfer_{0}", "i_store_tranfer_{0}", "i_car_tranfer_{0}" });
        }
    }
}
