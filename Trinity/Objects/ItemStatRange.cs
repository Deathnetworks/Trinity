using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Objects
{
    public class ItemStatRange
    {
        public double AncientMax;
        public double AncientMin;
        public double Max;
        public double Min;

        public double AbsMax
        {
            get { return Math.Max(AncientMax, Max); }
        }

        public double AbsMin
        {
            get { return Math.Min(AncientMin, Min); }
        }

        public double AbsStep
        {
            get { return GetStep(AbsMin, AbsMax); }
        }

        /// <summary>
        /// Friendly Increment amount between maximum and minimum
        /// </summary>
        public int GetStep(double min, double max)
        {
            var result = 1;
            var range = max - min;

            if (range > 0 && range > 10)
                result = (int)Math.Round(Math.Ceiling((range / 10) * 100) / 100, 0);

            return result;
        }
    }
}
