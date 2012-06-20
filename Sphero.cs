using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sphero.Communicator;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace HideAndSeek
{
	public class Sphero : IDisposable
	{
		private object locker = new object();

		public string ComPortName { get; set; }
		public string DeviceBTName { get; set; }

		private Color _curColor;
		public Color Color
		{
			get { return _curColor; }
			set
			{
				_curColor = value;
				this.SetColor(value);
			}
		}


		public string Name
		{
			get
			{
				if (string.IsNullOrWhiteSpace(DeviceBTName))
					return "[" + ComPortName + "]";
				else
					return "[" + ComPortName + "] " + DeviceBTName;
			}
		}
		private SpheroCommunicator com;

		public bool DebugMode { get; set; }

		public Sphero(string portName)
		{
			DebugMode = true;
			this.ComPortName = portName;
		}

		public void SetColor(Color color)
		{
			FailIfNotConnected();

			var p = PacketFactory.new_SetRGBLEDPacket(color.R, color.G, color.B, false);
			this.com.write(p);
		}

		public bool TestConnection()
		{
			FailIfNotConnected();

			var p = PacketFactory.new_GetBluetoothInfoPacket();

			try
			{
				var resp = com.writeAndWaitForResponse(p);

				if (resp.isValid())
				{
					this.DeviceBTName = resp.Message.Trim();
					Debug.WriteLine("Sphero " + this.Name + " seems to be working.");

					if (this.DebugMode)
					{
						SetColor(Color.Green);
					}

					return true;
				}
				else
				{
					Debug.WriteLine("Sphero " + this.Name + " isn't valid...");
					return false;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Sphero " + this.Name + " isn't working.  " + ex.ToString());
				return false;
			}



		}

		public bool IsConnected
		{
			get
			{
				return (com != null && com.IsOpen && com.IsConnected);
			}
		}

		public void FailIfNotConnected()
		{
			if (com != null && com.IsOpen && com.IsConnected)
				return;

			Debug.WriteLine("Sphero " + this.Name + " isn't opened when expected.");

			throw new InvalidOperationException("Sphero " + this.Name + " isn't opened when expected.");
		}

		public bool Connect()
		{
			Debug.WriteLine("About to open Sphero " + this.Name + ".");

			com = new SpheroCommunicator();

			bool connected = false;

			for (int i = 0; i < 10; i++)
			{
				try
				{
					com.openPort(this.ComPortName);
				}
				catch (Exception)
				{
					Debug.WriteLine("Opening Sphero " + this.Name + " failed on try " + (i + 1).ToString());
					continue;
				}

				connected = true;
				Debug.WriteLine("Sphero " + this.Name + " opened on try " + (i + 1).ToString());

				

				break;
			}

			if (connected)
			{
				if (this.DebugMode)
				{
					var p = PacketFactory.new_SetRGBLEDPacket((byte)200, (byte)200, (byte)0, false);
					this.com.write(p);
				}
			}
			else
			{
				Debug.WriteLine("Unable to open Sphero " + this.Name + "!");
			}

			return connected;
		}

		private void Send(Packet p)
		{
			lock (locker)
			{
				this.com.write(p);
			}
		}

		public void RollInCircle(bool clockwise, int speed, int steps, TimeSpan timeLength, float revolutions = float.NaN)
		{
			if (revolutions == float.NaN)
				revolutions = 1.0f;

			FailIfNotConnected();

			int msPerRevolution = (int)(timeLength.TotalMilliseconds / revolutions);

			int degsPerStep = 360 / steps;
			int msPer50Degrees = msPerRevolution / steps;
			int total50Degrees = (int)(revolutions * steps);

			int dirMod = 1;
			if (clockwise) dirMod = -1;

			//int speed = 180;

			System.Threading.Thread t = new System.Threading.Thread(state =>
			{
				Console.WriteLine("circle roll:");

				for (int i = 0; i < (int)(steps * revolutions); i++)
				{
					var p = PacketFactory.new_RollPacket((byte)speed, (ushort)(((dirMod * i * degsPerStep) + (360*1000)) % 360), true);

					this.com.writeAndWaitForResponse(p);

					Console.Write(i.ToString().PadLeft(3));
					
					//Send(p);
					Thread.Sleep(msPer50Degrees);
				}

				Console.WriteLine();
				Console.WriteLine("Done circle rolling");

				var stopper = PacketFactory.new_RollPacket((byte)0, (byte)0, true);
				Send(stopper);
			});

			t.Start();
					

		}

		#region IDisposable Members

		public void Dispose()
		{
			if (com != null)
			{
				Debug.WriteLine("Closing connection to Sphero " + this.Name);

				try
				{
					com.closePort();
					Debug.WriteLine("Connection to Sphero " + this.Name + " closed");
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Unable to close connection to Sphero " + this.Name + "!  " + ex.ToString());
				}
			}
		}

		#endregion
	}
}
