using System;

namespace iotSensor
{
    class MainClass
    {
        static public int SYSgetSensorValue()
        {
            return 0;
        }

        static public void SYSprint(int i)
        {
            
        }

        static public void Main()
        {
				    String hayo = "abc";
					  while (true)
						{
                int a = SYSgetSensorValue();
                if (a > 200)
                {
                    SYSprint(1);
                }
                else if (a < 100)
                {
                    SYSprint(-1);
                }
                else
                {
                    SYSprint(0);
                }
						}
        }

    }
}
