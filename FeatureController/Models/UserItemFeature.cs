using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureController.Models
{
    public class UserItemFeature : BaseFeature
    {
        public UserFeature UserFeature { get; private set; }
        public ItemFeature ItemFeature { get; set; }

        public UserCategoryFeature UserCategoryFeature { get; set; }

        public int UserId { get; private set; }
        public int ItemId { get; private set; }

        public bool Label { get; set; }

        public UserItemFeature() { }
        public UserItemFeature(UserFeature userFeature, int itemId)
        {
            UserFeature = userFeature;
            PredictDate = userFeature.PredictDate;
            ItemId = itemId;
            UserId = userFeature.Id;
        }

        public override void Write(System.IO.StreamWriter writer)
        {
            writer.Write("{0},{1},{2},{3},", UserId, ItemId, Label ? 1 : 0, ItemFeature.IsOnline ? 1 : 0);
            //writer.Write(String.Format("{0},{1},{2},", UserId, ItemId, Label ? 1 : 0));
            base.Write(writer);
            UserFeature.Write(writer);
            ItemFeature.Write(writer);
            UserCategoryFeature.Write(writer);
        }

        public void WriteHeaders(System.IO.StreamWriter writer)
        {
            writer.Write("userid,itemid,label,is_online,");
            //writer.Write("user_id,item_id,label,");
            BaseFeature.WriteHeaders(writer, "ui");
            UserFeature.WriteHeaders(writer);
            ItemFeature.WriteHeaders(writer);
            UserCategoryFeature.WriteHeaders(writer, "uc");
        }

    }
}
