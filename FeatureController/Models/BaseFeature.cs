using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatureController.Bases;

namespace FeatureController.Models
{
    /// <summary>
    /// 两个最关键的属性，FourBehaviorCountCollection，FourMinHourCountCollection
    /// </summary>
    public abstract class BaseFeature
    {
        /// <summary>
        /// 每多少小时作为一次统计单元
        /// </summary>
        protected int m_hourSpan;
        /// <summary>
        /// 统计前多少天的数据
        /// </summary>
        protected int m_relationDays;

        protected int m_defaultMinHourCount;

        public int Id { get; protected set; }

        /// <summary>
        /// 4种操作在各个时间段的数量统计
        /// </summary>
        public BehaviorCountCollection FourBehaviorCountCollection;
        public BehaviorCountCollection UniqueFourBehaviorCount;

        /// <summary>
        /// 4种操作距离现在最近的时间
        /// </summary>
        public HourCountCollection FourMinHourCountCollection;

        public DateTime PredictDate { get; protected set; }

        public BaseFeature()
        {
            m_hourSpan = Global.HourSpan;
            m_defaultMinHourCount = Global.DefaultMinHourCount;
            m_relationDays = Global.RelationDays;
            FourBehaviorCountCollection = new BehaviorCountCollection(4);
            FourMinHourCountCollection = new HourCountCollection(4);
            UniqueFourBehaviorCount = new BehaviorCountCollection(4);
        }

        public virtual void Write(StreamWriter writer)
        {
            FourBehaviorCountCollection.Write(writer);
            FourMinHourCountCollection.Write(writer);
            //writer.Write("0,0,0,0,"); 购买总数，这个不能在Batch运行模式时统计出来，先不输出了
        }

        public static void WriteHeaders(StreamWriter writer, string prefix = "")
        {
            string[] behaviors = new string[] { "{1}_click_{0}", "{1}_store_{0}", "{1}_car_{0}", "{1}_buy_{0}" };

            for (int i = 0; i < Global.RelationDays * 24 / Global.HourSpan; i++)
            {
                foreach (var behavior in behaviors)
                {
                    writer.Write(String.Format(behavior, i, prefix));
                    writer.Write(",");
                }
            }
            writer.Write("{0}_last_click,{0}_last_store,{0}_last_car,{0}_last_buy,", prefix);
            //writer.Write("{0}_total_click,{0}_total_store,{0}_total_car,{0}_total_buy,", prefix);
        }

        /// <summary>
        /// 为去重的两个统计准备的，比如统计一个用户购买的去重的商品个数
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="behaviors"></param>
        public static void WriteHeaders(StreamWriter writer, string[] behaviors)
        {
            List<int> hours = new List<int>();
            for (int i = Global.HourSpan; i <= Global.RelationDays * 24; i += Global.HourSpan)
            {
                hours.Add(i);
            }
            foreach (var behavior in behaviors)
            {
                foreach (var hour in hours)
                {
                    writer.Write(String.Format(behavior, hour));
                    writer.Write(",");
                }
            }
        }

        public void SetFourBehaviorCount(IGrouping<int, T_UserAction> data)
        {
            for (int behaviorType = 1; behaviorType < 5; behaviorType++)
            {
                for (int span = 24 * m_relationDays; span > 0; span -= m_hourSpan)
                {
                    DateTime dateTime = PredictDate.AddHours(-span);
                    int value = data.Count(d => d.behaviortype == behaviorType && d.actiondate >= dateTime);
                    if (value == 0)
                        break;
                    this.FourBehaviorCountCollection.SetValue(behaviorType - 1, span / m_hourSpan - 1, value);
                }
            }
        }

        public void SetFourMinHourCount(IGrouping<int, T_UserAction> data)
        {
            for (int behaviorType = 1; behaviorType < 5; behaviorType++)
            {
                var behaviorData = data.Where(d => d.behaviortype == behaviorType);
                int value = m_defaultMinHourCount;
                if (behaviorData.Count() > 0)
                    value = behaviorData.Min(d => PredictDate - d.actiondate).Hours;
                this.FourMinHourCountCollection.Items[behaviorType - 1] = value;
            }
        }

        public virtual void Update(IGrouping<int, T_UserAction> items)
        {
            this.SetFourBehaviorCount(items);
            this.SetFourMinHourCount(items);
        }

        public virtual void CatchMaxValue(BaseFeature item)
        {
            this.FourBehaviorCountCollection.CatchMaxValue(item.FourBehaviorCountCollection);
            this.FourMinHourCountCollection.CatchMaxValue(item.FourMinHourCountCollection);
        }
        public virtual void CatchMinValue(BaseFeature item)
        {
            this.FourBehaviorCountCollection.CatchMinValue(item.FourBehaviorCountCollection);
            this.FourMinHourCountCollection.CatchMinValue(item.FourMinHourCountCollection);
        }

        public virtual void Normalize(BaseFeature maxFeature, BaseFeature minFeature)
        {
            FourBehaviorCountCollection.Normalize(maxFeature.FourBehaviorCountCollection, minFeature.FourBehaviorCountCollection);
            FourMinHourCountCollection.Normalize(maxFeature.FourMinHourCountCollection, minFeature.FourMinHourCountCollection);
        }
    }

}
