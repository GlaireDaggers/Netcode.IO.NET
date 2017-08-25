using System;
using System.IO;
using System.Net;
using System.Threading;

using NetcodeIO.NET;
using NetcodeIO.NET.Utils.IO;

using NetcodeIO.NET.Tests;

namespace Test
{
	class Program
	{
		static readonly byte[] _privateKey = new byte[]
		{
			0x60, 0x6a, 0xbe, 0x6e, 0xc9, 0x19, 0x10, 0xea,
			0x9a, 0x65, 0x62, 0xf6, 0x6f, 0x2b, 0x30, 0xe4,
			0x43, 0x71, 0xd6, 0x2c, 0xd1, 0x99, 0x27, 0x26,
			0x6b, 0x3c, 0x60, 0xf4, 0xb7, 0x15, 0xab, 0xa1,
		};
		
		private static Client client;

		static void Main(string[] args)
		{
			//startClient();
			startServer();

			//Tests.TestClientServerMultipleClients();
			Console.ReadLine();
		}

		private static void runTest()
		{
		}

		private static void startClient()
		{
			// get token
			/*var webRequest = WebRequest.Create("http://127.0.0.1:8080/token");
			webRequest.Credentials = CredentialCache.DefaultCredentials;
			HttpWebResponse response;

			try
			{
				response = (HttpWebResponse)webRequest.GetResponse();
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to get token: " + e.Message);
				Console.ReadLine();
				return;
			}

			if (response.StatusCode != HttpStatusCode.OK)
			{
				Console.WriteLine("Failed to get token: " + response.StatusDescription);
				Console.ReadLine();
				return;
			}

			Stream dataStream = response.GetResponseStream();
			StreamReader reader = new StreamReader(dataStream);

			string responseStr = reader.ReadToEnd();

			reader.Close();
			dataStream.Close();
			response.Close();

			byte[] connectToken = System.Convert.FromBase64String(responseStr);*/
			TokenFactory factory = new TokenFactory(0x1122334455667788L, _privateKey);
			byte[] connectToken = factory.GenerateConnectToken(new IPEndPoint[] { new IPEndPoint(IPAddress.Parse("127.0.0.1"), 40000) },
				30,
				5,
				1UL,
				1UL,
				new byte[256]);

			testPacket = new byte[256];
			using (var testPacketWriter = ByteArrayReaderWriter.Get(testPacket))
			{
				testPacketWriter.Write((uint)0xAABBCCDD);
			}

			client = new Client();

			client.OnStateChanged += Client_OnStateChanged;
			client.OnMessageReceived += Client_OnMessageReceived;

			Console.WriteLine("Connecting...");
			client.Connect(connectToken);

			Console.ReadLine();
			client.Disconnect();
		}

		private static void startServer()
		{
			Server server = new Server(
				256,
				"127.0.0.1", 40000,
				0x1122334455667788L,
				_privateKey
				);
			server.LogLevel = NetcodeLogLevel.Debug;
			server.Start();
			Console.WriteLine("Server started");

			server.OnClientConnected += Server_OnClientConnected;
			server.OnClientDisconnected += Server_OnClientDisconnected;
			server.OnClientMessageReceived += Server_OnClientMessageReceived;

			Console.ReadLine();
			server.Stop();
		}

		private static byte[] testPacket;

		private static bool isRunningClient = false;
		private static void doClientSendStuff()
		{
			while (isRunningClient)
			{
				Console.WriteLine("Sent packet");
				client.Send(testPacket, 256);

				Thread.Sleep(100);
			}
		}

		private static void Client_OnMessageReceived(byte[] payload, int payloadSize)
		{
			Console.WriteLine("Got packet!");
		}

		private static void Client_OnStateChanged(ClientState state)
		{
			Console.WriteLine("Client state changed: " + state.ToString());
			if (state == ClientState.Connected)
			{
				// connected! start sending stuff.
				isRunningClient = true;
				var workThread = new Thread(doClientSendStuff);
				workThread.Start();
			}
			else
			{
				if (isRunningClient)
				{
					isRunningClient = false;
				}
			}
		}

		private static void Server_OnClientMessageReceived(RemoteClient sender, byte[] payload, int payloadSize)
		{
			Console.WriteLine("Received message from " + sender.ClientID);

			// just send it back to them
			sender.SendPayload(payload, payloadSize);
		}

		private static void Server_OnClientDisconnected(RemoteClient client)
		{
			Console.WriteLine("Client " + client.ClientID + " disconnected");
		}

		private static void Server_OnClientConnected(RemoteClient client)
		{
			Console.WriteLine("Client " + client.ClientID + " connected");
		}
	}
}
