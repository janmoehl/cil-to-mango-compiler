class MainClass
{
		static public int SYSgetSensorValue()
		{
				return 0;
		}

		static public void SYSsetLight(int i)
		{
				
		}

		static public void Main()
		{
				while (true)
				{
						int a = SYSgetSensorValue();
						if (a > 150)
						{
								SYSsetLight(0);
						}
						else
						{
								SYSsetLight(1);
						}
				}
		}
}
