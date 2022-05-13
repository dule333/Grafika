using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ArrayPosition
    {
        private int x;
        private int y;
        private ArrayPosition parent;

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
        public ArrayPosition Parent { get => parent; set => parent = value; }
    }
}
