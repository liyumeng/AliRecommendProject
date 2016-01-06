using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatureController.Bases;

namespace FeatureController.Models
{
    public class HourCountCollection
    {
        private double[] m_items;
        public double[] Items
        {
            get
            {
                return m_items;
            }
            private set
            {
                m_items = value;
            }
        }

        public HourCountCollection(int count)
        {
            m_items = new double[count];
        }

        public void Write(StreamWriter writer)
        {
            foreach (var item in m_items)
            {
                writer.Write(item);
                writer.Write(",");
            }
        }

        /// <summary>
        /// 与item比较，将两者属性的最大值都赋予自己
        /// </summary>
        /// <param name="item"></param>
        public void CatchMaxValue(HourCountCollection item)
        {
            for (int i = 0; i < m_items.Length; i++)
            {
                this.m_items[i] = Math.Max(this.m_items[i], item.Items[i]);
            }
        }

        /// <summary>
        /// 与item比较，将两者属性的最小值都赋予自己
        /// </summary>
        /// <param name="item"></param>
        public void CatchMinValue(HourCountCollection item)
        {
            for (int i = 0; i < m_items.Length; i++)
            {
                this.m_items[i] = Math.Min(this.m_items[i], item.Items[i]);
            }
        }

        public void Normalize(HourCountCollection maxItem, HourCountCollection minItem)
        {
            for (int i = 0; i < m_items.Length; i++)
            {
                this.m_items[i] = Utils.Normalize(m_items[i], maxItem.m_items[i], minItem.m_items[i]);
            }
        }
    }
}
