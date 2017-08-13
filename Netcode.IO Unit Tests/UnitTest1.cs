using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NetcodeIO.NET;

using System.Diagnostics;

using NetcodeIO.NET.Tests;

namespace Netcode_IO_Unit_Tests
{
	[TestClass]
	public class UnitTest1
	{
		[TestCategory("Misc"), TestMethod]
		public void TestSequence()
		{
			Tests.TestSequence();
		}

		[TestCategory("Misc"), TestMethod]
		public void TestConnectToken()
		{
			Tests.TestConnectToken();
		}

		[TestCategory("Misc"), TestMethod]
		public void TestChallengeToken()
		{
			Tests.TestChallengeToken();
		}

		[TestCategory("Misc"), TestMethod]
		public void TestEncryptionManager()
		{
			Tests.TestEncryptionManager();
		}

		[TestCategory("Misc"), TestMethod]
		public void TestReplayProtection()
		{
			Tests.TestReplayProtection();
		}

		[TestCategory("Packets"), TestMethod]
		public void TestConnectionRequestPacket()
		{
			Tests.TestConnectionRequestPacket();
		}

		[TestCategory("Packets"), TestMethod]
		public void TestConnectionDeniedPacket()
		{
			Tests.TestConnectionDeniedPacket();
		}

		[TestCategory("Packets"), TestMethod]
		public void TestConnectionKeepAlivePacket()
		{
			Tests.TestConnectionKeepAlivePacket();
		}

		[TestCategory("Packets"), TestMethod]
		public void TestConnectionChallengePacket()
		{
			Tests.TestConnectionChallengePacket();
		}

		[TestCategory("Packets"), TestMethod]
		public void TestConnectionPayloadPacket()
		{
			Tests.TestConnectionPayloadPacket();
		}

		[TestCategory("Packets"), TestMethod]
		public void TestConnectionDisconnectPacket()
		{
			Tests.TestConnectionDisconnectPacket();
		}

		[TestCategory("Connection"), TestMethod]
		public void TestClientServerConnection()
		{
			Tests.TestClientServerConnection();
		}

		[TestCategory("Connection"), TestMethod]
		public void TestClientServerKeepAlive()
		{
			Tests.TestClientServerKeepAlive();
		}

		[TestCategory("Connection"), TestMethod]
		public void TestClientServerMultipleClients()
		{
			Tests.TestClientServerMultipleClients();
		}

		[TestCategory("Connection"), TestMethod]
		public void TestClientServerMultipleServers()
		{
			Tests.TestClientServerMultipleServers();
		}

		[TestCategory("Connection"), TestMethod]
		public void TestConnectTokenExpired()
		{
			Tests.TestConnectTokenExpired();
		}

		[TestCategory("Connection"), TestMethod]
		public void TestInvalidConnectToken()
		{
			Tests.TestClientInvalidConnectToken();
		}

		[TestCategory("Connection"), TestMethod]
		public void TestConnectionTimeout()
		{
			Tests.TestConnectionTimeout();
		}

		[TestCategory("Connection"), TestMethod]
		public void TestChallengeResponseTimeout()
		{
			Tests.TestChallengeResponseTimeout();
		}

		[TestCategory("Connection"), TestMethod]
		public void TestConnectionRequestTimeout()
		{
			Tests.TestConnectionRequestTimeout();
		}

		[TestCategory("Connection"), TestMethod]
		public void TestConnectionDenied()
		{
			Tests.TestConnectionDenied();
		}

		[TestCategory("Connection"), TestMethod]
		public void TestClientSideDisconnect()
		{
			Tests.TestClientSideDisconnect();
		}

		[TestCategory("Connection"), TestMethod]
		public void TestServerSideDisconnect()
		{
			Tests.TestServerSideDisconnect();
		}

		[TestCategory("Connection"), TestMethod]
		public void TestClientReconnect()
		{
			Tests.TestReconnect();
		}

		[TestCategory("Soak Connection"), TestMethod]
		public void SoakConnectionTests()
		{
			const int soakTime = 1000 * 60 * 10;

			Stopwatch sw = new Stopwatch();
			sw.Start();

			int iterations = 0;
			while (sw.ElapsedMilliseconds < soakTime)
			{
				Console.WriteLine("=== RUN " + iterations + " ===");

				Tests.TestClientServerConnection();
				Tests.TestClientServerKeepAlive();
				Tests.TestClientServerKeepAlive();
				Tests.TestClientServerMultipleClients();
				Tests.TestClientServerMultipleServers();
				Tests.TestConnectTokenExpired();
				Tests.TestClientInvalidConnectToken();
				Tests.TestConnectionTimeout();
				Tests.TestChallengeResponseTimeout();
				Tests.TestConnectionRequestTimeout();
				Tests.TestConnectionDenied();
				Tests.TestClientSideDisconnect();
				Tests.TestServerSideDisconnect();
				Tests.TestReconnect();

				iterations++;
			}

			sw.Stop();
		}

		[TestCategory("Soak Connection"), TestMethod]
		public void SoakClientServerRandomConnection()
		{
			Tests.SoakTestClientServerConnection(30);
		}
	}
}
