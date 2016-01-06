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
    /// 本类可以存储某个特征在各个时间段的值
    /// </summary>
    public class BehaviorCountCollection
    {
        /// <summary>
        /// 每多少小时作为一次统计单元
        /// </summary>
        private int m_hourSpan;
        /// <summary>
        /// 统计前多少天的数据
        /// </summary>
        private int m_relationDays;

        public int BehaviorHour { get; private set; }

        public BehaviorCountCollection(int behaviorCount)
        {
            m_relationDays = Global.RelationDays;
            m_hourSpan = Global.HourSpan;
            BehaviorHour = behaviorCount;

            int capacity = 24 / m_hourSpan * m_relationDays;
            ActionData = new double[behaviorCount][];
            for (int i = 0; i < behaviorCount; i++)
            {
                ActionData[i] = new double[capacity];
            }
        }

        public double[][] ActionData;

        /// <summary>
        /// 传入数组索引ActionData[type ][hourSpan]
        /// </summary>
        /// <param name="type">范围0~BehaviorCount</param>
        /// <param name="hourSpan">范围0~TimeCount</param>
        /// <param name="value"></param>
        public void SetValue(int type, int hourSpan, int value)
        {
            ActionData[type][hourSpan] = value;
        }

        public void SetValue(int type, int hourSpan, double value)
        {
            ActionData[type][hourSpan] = value;
        }

        public void Write(StreamWriter writer)
        {
            if (ActionData.Length == 0) return;
            for (int i = 0; i < ActionData[0].Length; i++)
            {
                for (int j = 0; j < ActionData.Length; j++)
                {
                    writer.Write(ActionData[j][i]);
                    writer.Write(",");
                }
            }
        }

        /// <summary>
        /// {0}自动变成每个时间段从0~n，每个加1
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="names">例如i_click_{0},i_store_{0}</param>
        public void WriteHeaders(StreamWriter writer, string[] names)
        {
            if (ActionData.Length == 0) return;
            for (int i = 0; i < ActionData[0].Length; i++)//每个时间段
            {
                for (int j = 0; j < ActionData.Length; j++)//每总行为
                {
                    writer.Write(String.Format(names[j], i));
                    writer.Write(",");
                }
            }
        }

        /// <summary>
        /// 与item比较，将两者属性的最大值都赋予自己
        /// </summary>
        /// <param name="item"></param>
        public void CatchMaxValue(BehaviorCountCollection item)
        {
            for (int i = 0; i < ActionData.Length; i++)
            {
                for (int j = 0; j < ActionData[0].Length; j++)
                {
                    ActionData[i][j] = Math.Max(ActionData[i][j], item.ActionData[i][j]);
                }
            }
        }

        /// <summary>
        /// 与item比较，将两者属性的最小值都赋予自己
        /// </summary>
        /// <param name="item"></param>
        public void CatchMinValue(BehaviorCountCollection item)
        {
            for (int i = 0; i < ActionData.Length; i++)
            {
                for (int j = 0; j < ActionData[0].Length; j++)
                {
                    ActionData[i][j] = Math.Min(ActionData[i][j], item.ActionData[i][j]);
                }
            }
        }

        public void Normalize(BehaviorCountCollection maxItem, BehaviorCountCollection minItem)
        {
            for (int i = 0; i < ActionData.Length; i++)
            {
                for (int j = 0; j < ActionData[0].Length; j++)
                {
                    ActionData[i][j] = Utils.Normalize(ActionData[i][j], maxItem.ActionData[i][j], minItem.ActionData[i][j]);
                }
            }
        }

    }

}
