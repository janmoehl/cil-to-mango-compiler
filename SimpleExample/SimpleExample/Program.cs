using System;

namespace SimpleExample
{
    class MainClass
    {
        static public int SYSgetTemperature()
        {
            return 0;
        }

        static public void SYSprint(int i)
        {
            
        }

        static public void Main()
        {
            int a = SYSgetTemperature();
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

            int maximum = Max(a, c, 0, 0, 0);
            //Console.WriteLine(maximum);
            SYSprint(maximum);
        }

        public static int Max(int a, int b, int c, int d, int e)
        {
            int result = c + d + e;
            if (a > b)
            {
                return a;
            }
            else
            {
                return b;
            }
        }

    }
}
