using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureController.Bases
{
    public class Utils
    {
        public static double Normalize(double value, double max, double min)
        {
            if (max == min)
                return 0;
            else
                return 1.0 * (value - min) / (max - min);
        }

    }
}
