using System;

namespace SimpleExample
{
    class MainClass
    {
        struct Point2D
        {
            public int X;
            public int Y;
        }

        static void Main()
        {
            Point2D p;
            p.X = 15;
            p.Y = 100;
            MoveBy(ref p, 1, 1);
        }

        static void MoveBy(ref Point2D p, int x, int y)
        {
            p.X += x;
            p.Y += y;
        }

    }
}
