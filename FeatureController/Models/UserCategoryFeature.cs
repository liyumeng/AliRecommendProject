using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureController.Models
{
    public class UserCategoryFeature : BaseFeature
    {
        public int CategoryId { get; private set; }
        public UserCategoryFeature(int categoryId,DateTime predictDate)
        {
            PredictDate = predictDate;
            CategoryId = categoryId;
        }

        public UserCategoryFeature() { }
    }
}
