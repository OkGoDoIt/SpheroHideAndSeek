using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sphero.Communicator;
using System.Drawing;
using System.Threading;

namespace HideAndSeek
{
	class Program
	{
		static void Main(string[] args)
		{
			var comm = new global::Sphero.Communicator.SpheroCommunicator();
			int numRequiredConnections = 4;
			bool enoughConnected = false;
			var ports = comm.getPortNames().OrderBy(p => int.Parse(p.Substring(3)));

			List<Sphero> spheros = new List<Sphero>();

			List<Thread> threads = new List<Thread>();

			foreach (var port in ports)
			{
				Thread t = new Thread(state =>
				{
					var s = new Sphero((string)state);

					if (s.Connect())
					{
						if (enoughConnected)
						{
							s.Dispose();
							return;
						}

						int count = -1;

						lock (spheros)
						{
							spheros.Add(s);
							count = spheros.Count;
						}

						Console.WriteLine(s.Name + " connected! Now " + count.ToString() + " Sphero" + (count == 1 ? " is" : "s are") + " connected.");

						lock (spheros)
						{
							if (enoughConnected)
							{
								return;
							}

							if (count >= numRequiredConnections)
							{
								enoughConnected = true;

								Console.WriteLine("Met connected Sphero quota, good to go!");

								foreach (var sp in spheros)
								{
									sp.SetColor(Color.Green);
								}
							}
						}

						for (int i = 0; i < 10 * 60; i++)
						{
							Thread.Sleep(1000);
						}

					}
				});
				
				t.Start(port);
			}

			while (!enoughConnected)
			{
				Thread.Sleep(1000);
			}

			foreach (var sp in spheros)
			{
				sp.SetColor(Color.FromArgb(0, 0, 255));
			}

			Thread.Sleep(2000);

			for (int i = 10; i >= 0; i--)
			{
				foreach (var sp in spheros)
				{
					sp.SetColor(Color.FromArgb(0, 0, i * 25));
				}

				Thread.Sleep(20);
			}

			Console.WriteLine("Starting");

			Random rand = new Random();

			var all = spheros.OrderBy(s => rand.NextDouble()).ToList();
			var theone = spheros.OrderBy(s => rand.NextDouble()).First();

			Console.WriteLine("shimmer");


			for (int i = 0; i < 10; i++)
			{
				var allThis = spheros.OrderBy(s => rand.NextDouble()).ToList();

				foreach (var sp in allThis)
				{
					var thisColor = Color.Black;

					thisColor = Color.FromArgb(rand.Next(25) * 10, rand.Next(25) * 10, rand.Next(25) * 10);
					sp.SetColor(thisColor);
				
					Thread.Sleep(50);
				}

			}					


			foreach (var sp in spheros)
			{
				sp.SetColor(Color.Black);
			}

			Console.WriteLine("shimmer complete");


			Thread.Sleep(2000);

			Console.WriteLine("show target");


			for (int j = 0; j < 3; j++)
			{
				for (int i = 0; i < 25; i++)
				{
					var baseColor = Color.Gold;
					var thisColor = Color.FromArgb((int)(baseColor.R * i / 25.0), (int)(baseColor.G * i / 25.0), (int)(baseColor.B * i / 25.0));

					theone.SetColor(thisColor);

					Thread.Sleep(20);
				}
				for (int i = 25; i >= 0; i--)
				{
					var baseColor = Color.Gold;
					var thisColor = Color.FromArgb((int)(baseColor.R * i / 25.0), (int)(baseColor.G * i / 25.0), (int)(baseColor.B * i / 25.0));

					theone.SetColor(thisColor);

					Thread.Sleep(20);
				}
			}

			Console.WriteLine("done showing target");


			Thread.Sleep(rand.Next(2000, 5000));

			Console.WriteLine("start chaos");

			foreach (var thisOne in all)
			{
				var thisColor = Color.FromArgb(rand.Next(25) * 10, rand.Next(25) * 10, rand.Next(25) * 10);
				thisOne.SetColor(thisColor);
				thisOne.RollInCircle(rand.NextDouble() > 0.8, rand.Next(150, 254), 10, TimeSpan.FromSeconds(rand.NextDouble() * 4.0), rand.Next(5, 50) * 0.1f);


			}


			for (int i = 0; i < 100 + rand.Next(150); i++)
			{
				var thisOne = spheros.OrderBy(s => rand.NextDouble()).First();

				var thisColor = Color.FromArgb(rand.Next(25) * 10, rand.Next(25) * 10, rand.Next(25) * 10);

				if (rand.NextDouble() > 0.3)
					thisOne.SetColor(thisColor);

				thisOne = spheros.OrderBy(s => rand.NextDouble()).First();

				if (rand.NextDouble() > 0.6)
					thisOne.RollInCircle(rand.NextDouble() > 0.8, rand.Next(50, 254), 10, TimeSpan.FromSeconds(rand.NextDouble() * 4.0), rand.Next(5, 20) * 0.1f);

				Thread.Sleep(rand.Next(10, 500));
			}

			Console.WriteLine("done with chaos");


			Thread.Sleep(4000);

			Console.WriteLine("guess time");


			for (int i = 0; i < 25; i++)
			{
				var baseColor = Color.White;
				var thisColor = Color.FromArgb((int)(baseColor.R * i / 25.0), (int)(baseColor.G * i / 25.0), (int)(baseColor.B * i / 25.0));

				foreach (var sp in all)
				{
					sp.SetColor(thisColor);
				}
				Thread.Sleep(20);
			}
			for (int i = 25; i >= 0; i--)
			{
				var baseColor = Color.White;
				var thisColor = Color.FromArgb((int)(baseColor.R * i / 25.0), (int)(baseColor.G * i / 25.0), (int)(baseColor.B * i / 25.0));

				foreach (var sp in all)
				{
					sp.SetColor(thisColor);
				}

				Thread.Sleep(20);
			}

			for (int i = 0; i < 25; i++)
			{
				var baseColor = Color.Red;
				var thisColor = Color.FromArgb((int)(baseColor.R * i / 25.0), (int)(baseColor.G * i / 25.0), (int)(baseColor.B * i / 25.0));

				foreach (var sp in all)
				{
					sp.SetColor(thisColor);
				}

				Thread.Sleep(20);
			}

			Console.WriteLine("guess time over");


			for (int i = 25; i >= 0; i--)
			{
				var baseColor = Color.Red;
				var thisColor = Color.FromArgb((int)(baseColor.R * i / 25.0), (int)(baseColor.G * i / 25.0), (int)(baseColor.B * i / 25.0));

				foreach (var sp in all)
				{
					sp.SetColor(thisColor);
				}

				Thread.Sleep(20);
			}

			Console.WriteLine("show correct");


			for (int j = 0; j < 5; j++)
			{
				for (int i = 0; i < 25; i++)
				{
					var baseColor = Color.Gold;
					var thisColor = Color.FromArgb((int)(baseColor.R * i / 25.0), (int)(baseColor.G * i / 25.0), (int)(baseColor.B * i / 25.0));

					theone.SetColor(thisColor);

					Thread.Sleep(20);
				}
				for (int i = 25; i >= 0; i--)
				{
					var baseColor = Color.RoyalBlue;
					var thisColor = Color.FromArgb((int)(baseColor.R * i / 25.0), (int)(baseColor.G * i / 25.0), (int)(baseColor.B * i / 25.0));

					theone.SetColor(thisColor);

					Thread.Sleep(20);
				}
				for (int i = 25; i >= 0; i--)
				{
					var baseColor = Color.Gold;
					var thisColor = Color.FromArgb((int)(baseColor.R * i / 25.0), (int)(baseColor.G * i / 25.0), (int)(baseColor.B * i / 25.0));

					theone.SetColor(thisColor);

					Thread.Sleep(20);
				}
			}


			Console.WriteLine("done showing correct");


			//spheros[0].RollInCircle(true, 200, 10, TimeSpan.FromSeconds(6), 2.0f);
			//spheros[1].RollInCircle(false, 200, 10, TimeSpan.FromSeconds(6), 2.0f);


			foreach (var sp in spheros)
			{
				sp.Dispose();
			}


			Console.WriteLine("finished");



			//using (var s1 = new Sphero("COM6"))
			//using (var s2 = new Sphero("COM8"))
			//{
			//    bool connected = false;
			//    connected |= s1.Connect();
			//    //		connected |= s2.Connect();

			//    if (connected)
			//    {
			//        Console.WriteLine("CONNECTED!");
			//    }
			//    else
			//    {
			//        Console.WriteLine("not connected...");
			//    }


			//    bool working = false;
			//    working |= s1.TestConnection();
			//    //		working |= s2.TestConnection();

			//    if (working)
			//    {
			//        Console.WriteLine("WORKING!");
			//    }
			//    else
			//    {
			//        Console.WriteLine("not working...");
			//    }

			//    s1.Color = Color.Blue;

			//    s1.RollInCircle(50, TimeSpan.FromSeconds(20), 1.0f);
			//    //	s2.RollInCircle(100, TimeSpan.FromSeconds(20), 1.0f);

			//    Console.ReadLine();

			//    Console.WriteLine("cleaning up...");

			//}

			Console.WriteLine("cleaned up");

			Console.ReadLine();

		}
	}
}
