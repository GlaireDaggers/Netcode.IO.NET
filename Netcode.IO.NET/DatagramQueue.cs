using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace NetcodeIO.NET.Utils
{
	internal struct Datagram
	{
		public byte[] payload;
		public int payloadSize;
		public EndPoint sender;

		public void Release()
		{
			BufferPool.ReturnBuffer(payload);
		}
	}

	internal class DatagramQueue
	{
		protected Queue<Datagram> datagramQueue = new Queue<Datagram>();
		protected Queue<EndPoint> endpointPool = new Queue<EndPoint>();

		public int Count
		{
			get { return datagramQueue.Count; }
		}

		public void Clear()
		{
			datagramQueue.Clear();
			endpointPool.Clear();
		}

		public void ReadFrom(Socket socket)
		{
			EndPoint sender;
			if (endpointPool.Count > 0)
			{
				lock (endpointPool)
					sender = endpointPool.Dequeue();
			}
			else
				sender = new IPEndPoint(IPAddress.Any, 0);

			byte[] receiveBuffer = BufferPool.GetBuffer(2048);
			int recv = socket.ReceiveFrom(receiveBuffer, ref sender);

			Datagram packet = new Datagram();
			packet.sender = sender;
			packet.payload = receiveBuffer;
			packet.payloadSize = recv;

			lock (datagramQueue)
				datagramQueue.Enqueue(packet);
		}

		public Datagram Dequeue()
		{
			lock(datagramQueue)
				return datagramQueue.Dequeue();
		}
	}
}
