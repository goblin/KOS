using System;
using kOS.Safe.Encapsulation;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace kOS.Suffixed
{
	public class Telnet : Structure
	{
		public string dest { get; set; }
		public int port { get; set; }
		private Socket sender;
		private SharedObjects shared;

		public Telnet (SharedObjects sharedObjs)
		{
			dest = "";
			port = 0;
			shared = sharedObjs;
		}

		private bool Connect ()
		{
			IPHostEntry ipHostInfo = Dns.Resolve (dest);
			IPAddress ipAddress = ipHostInfo.AddressList [0];
			IPEndPoint remoteEP = new IPEndPoint (ipAddress, port);

			sender = new Socket (AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp);

			bool result;
			try {
				sender.Connect (remoteEP);
				result = true;
			} catch (Exception e) {
				result = false;
			}
			return result;
		}

		private bool Send (string data)
		{
			sender.Send(Encoding.ASCII.GetBytes(data + "\n"));
			return true;
		}

		private string Receive ()
		{
			byte[] bytes = new byte[1024];
			int bytesRec = sender.Receive(bytes);
			return Encoding.ASCII.GetString(bytes, 0, bytesRec);
		}

		private bool Close()
		{
			sender.Shutdown(SocketShutdown.Both);
			sender.Close();
			return true;
		}

		private bool HasData()
		{
			return sender.Poll(0, SelectMode.SelectRead);
		}

		private void RunCommand(string command)
		{
			shared.Interpreter.RunCommand(command);
		}

		public override object GetSuffix (string suffixName)
		{
			switch (suffixName)
			{
				case "DEST":
					return dest;
				case "PORT":
					return port;
				case "CONNECT":
					return Connect();
				case "RECEIVE":
					return Receive();
				case "CLOSE":
					return Close();
				case "HASDATA":
					return HasData();
			}

			return base.GetSuffix (suffixName);
		}
			
		public override bool SetSuffix (string suffixName, object value)
		{
			switch (suffixName)
			{
				case "DEST":
					dest = (string)value;
					return true;
				case "PORT":
					port = (int)value;
					return true;
				case "SEND":
					return Send((string)value);
				case "EXEC":
					RunCommand((string)value);
					return true;
			}
			return base.SetSuffix (suffixName, value);
		}

		public override string ToString ()
		{
			return string.Format ("[Telnet {0}:{1}]", dest, port);
		}
	}
}

