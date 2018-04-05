using System;

namespace SimpleExample
{
    class MainClass
    {
        static public void Main()
        {
            int a = 3;
            a *= 2;
            a /= 1;
            a += 2;
            a -= 3;
            int c;
            if (a >= 0) {
                c = 1;
            }
            else
            {
                c = -1;
            }

            while(c == 1) {
                c = -1;
            }

            for (int i = 0; i < 12; i++)
            {
                a = i;
            }

            //int maximum = Max(a, b);
            //Console.WriteLine(maximum);
        }

        /*public static int Max(int a, int b)
        {
            if (a > b)
            {
                return a;
            }
            else
            {
                return b;
            }
        }*/

    }
}
