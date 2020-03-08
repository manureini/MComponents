using System;
using System.Collections.Generic;
using System.Text;

namespace MComponents
{
    public struct BoundingBox
    {
        public double Width;
        public double Height;

        public double Top;
        public double Left;

        public double BorderTop;
        public double BorderRight;

        public double BorderSpace;

        public string BorderCollapse;
    }
}
