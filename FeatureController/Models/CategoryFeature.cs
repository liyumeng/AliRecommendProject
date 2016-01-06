using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureController.Models
{
    //类别特征处理类
    public class CategoryFeature : BaseFeature
    {
        public CategoryFeature() { }
        public CategoryFeature(int id, DateTime predictDate)
        {
            PredictDate = predictDate;
            Id = id;
        }

    }
}
