using System;

namespace SimpleExample
{
    class MainClass
    {
        struct Point2D
        {
            public int x;
            public int y;
        }

        /*
         * struct Line2D
         * {
         *      public Point2D start;
         *      public Point2D end;
         * }
         * 
         * */

        static void Main()
        {
            Point2D p;
            p.x = 15;
            p.y = 100;
            MoveBy(ref p, 1, 1);
            bool x = BooleanFunction();
            if (x) {
                p.x = 20;
            }
        }

        static void MoveBy(ref Point2D p, int x, int y)
        {
            p.x += x;
            p.y += y;
        }

        static bool BooleanFunction() {
            return true;
        }

    }
} 
