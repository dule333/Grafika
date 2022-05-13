using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class CompareHelper : IComparer<LineEntity>
    {
        public int Compare(LineEntity x, LineEntity y)
        {
            Tuple<int, int> first1 = MapHandler.GetTuple(x.FirstEnd);
            Tuple<int, int> first2 = MapHandler.GetTuple(x.SecondEnd);
            Tuple<int, int> second1 = MapHandler.GetTuple(x.FirstEnd);
            Tuple<int, int> second2 = MapHandler.GetTuple(x.SecondEnd);
            if(first1 == null || first2 == null || second1 == null || second2 == null)
                return 0;
            return Calculate(second1, second2) - Calculate(first1, first2);
        }
        private int Calculate(Tuple<int, int> tuple1, Tuple<int, int> tuple2)
        {
            return (int)Math.Sqrt((tuple2.Item1 * tuple2.Item1 - tuple1.Item1 * tuple1.Item1) +
                (tuple2.Item2 * tuple2.Item2 - tuple1.Item2 * tuple1.Item2));
        }
    }
}
