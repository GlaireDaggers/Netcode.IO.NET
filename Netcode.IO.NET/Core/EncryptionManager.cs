using System;
using System.Net;

using NetcodeIO.NET.Utils;

namespace NetcodeIO.NET
{
	internal class EncryptionManager
	{
		internal struct encryptionMapEntry
		{
			public double ExpireTime;
			public double LastAccessTime;
			public EndPoint Address;
			public byte[] SendKey;
			public byte[] ReceiveKey;

			public void Reset()
			{
				ExpireTime = -1.0;
				LastAccessTime = -1000.0;
				Address = null;

				Array.Clear(SendKey, 0, SendKey.Length);
				Array.Clear(ReceiveKey, 0, ReceiveKey.Length);
			}
		}

		internal int numEncryptionMappings;
		internal encryptionMapEntry[] encryptionMappings;

		public EncryptionManager( int maxClients )
		{
			encryptionMappings = new encryptionMapEntry[maxClients * 4];
			for (int i = 0; i < encryptionMappings.Length; i++)
			{
				encryptionMappings[i].SendKey = new byte[32];
				encryptionMappings[i].ReceiveKey = new byte[32];
			}

			Reset();
		}

		public void Reset()
		{
			numEncryptionMappings = 0;
			for (int i = 0; i < encryptionMappings.Length; i++)
			{
				encryptionMappings[i].Reset();
			}
		}

		public bool AddEncryptionMapping(EndPoint address, byte[] sendKey, byte[] receiveKey, double time, double expireTime)
		{
			for (int i = 0; i < numEncryptionMappings; i++)
			{
				if ( MiscUtils.AddressEqual( encryptionMappings[i].Address, address )
					&& encryptionMappings[i].LastAccessTime + Defines.NETCODE_TIMEOUT_SECONDS >= time)
				{
					encryptionMappings[i].ExpireTime = expireTime;
					encryptionMappings[i].LastAccessTime = time;

					Buffer.BlockCopy(sendKey, 0, encryptionMappings[i].SendKey, 0, 32);
					Buffer.BlockCopy(receiveKey, 0, encryptionMappings[i].ReceiveKey, 0, 32);
					return true;
				}
			}

			for (int i = 0; i < encryptionMappings.Length; i++)
			{
				if (encryptionMappings[i].LastAccessTime + Defines.NETCODE_TIMEOUT_SECONDS < time ||
					(encryptionMappings[i].ExpireTime >= 0.0 && encryptionMappings[i].ExpireTime < time))
				{
					encryptionMappings[i].Address = address;
					encryptionMappings[i].ExpireTime = expireTime;
					encryptionMappings[i].LastAccessTime = time;

					Buffer.BlockCopy(sendKey, 0, encryptionMappings[i].SendKey, 0, 32);
					Buffer.BlockCopy(receiveKey, 0, encryptionMappings[i].ReceiveKey, 0, 32);

					if (i + 1 > numEncryptionMappings)
						numEncryptionMappings = i + 1;

					return true;
				}
			}

			return false;
		}

		public bool RemoveEncryptionMapping(EndPoint address, double time)
		{
			for (int i = 0; i < numEncryptionMappings; i++)
			{
				if (MiscUtils.AddressEqual(encryptionMappings[i].Address, address))
				{
					encryptionMappings[i].Reset();

					if (i + 1 == numEncryptionMappings)
					{
						int index = i - 1;
						while (index >= 0)
						{
							if (encryptionMappings[index].LastAccessTime + Defines.NETCODE_TIMEOUT_SECONDS >= time &&
								(encryptionMappings[index].ExpireTime < 0 || encryptionMappings[index].ExpireTime > time))
								break;
							index--;
						}
						numEncryptionMappings = index + 1;
					}

					return true;
				}
			}

			return false;
		}

		public byte[] GetSendKey(int idx)
		{
			if (idx == -1 || idx >= encryptionMappings.Length) return null;
			return encryptionMappings[idx].SendKey;
		}

		public byte[] GetReceiveKey(int idx)
		{
			if (idx == -1 || idx >= encryptionMappings.Length) return null;
			return encryptionMappings[idx].ReceiveKey;
		}

		public bool Touch(int index, EndPoint address, double time)
		{
			if (index < 0 || index >= numEncryptionMappings)
				throw new IndexOutOfRangeException();

			if (!MiscUtils.AddressEqual(encryptionMappings[index].Address, address))
				return false;

			encryptionMappings[index].LastAccessTime = time;
			encryptionMappings[index].ExpireTime = time + Defines.NETCODE_TIMEOUT_SECONDS;
			return true;
		}

		public void SetExpireTime(int index, double expireTime)
		{
			if (index < 0 || index >= numEncryptionMappings)
				throw new IndexOutOfRangeException();

			encryptionMappings[index].ExpireTime = expireTime;
		}

		public int FindEncryptionMapping(EndPoint address, double time)
		{
			for (int i = 0; i < numEncryptionMappings; i++)
			{
				if (MiscUtils.AddressEqual(encryptionMappings[i].Address, address) &&
					encryptionMappings[i].LastAccessTime + Defines.NETCODE_TIMEOUT_SECONDS >= time &&
					(encryptionMappings[i].ExpireTime < 0.0 || encryptionMappings[i].ExpireTime >= time))
				{
					encryptionMappings[i].LastAccessTime = time;
					return i;
				}
			}

			return -1;
		}
	}
}
