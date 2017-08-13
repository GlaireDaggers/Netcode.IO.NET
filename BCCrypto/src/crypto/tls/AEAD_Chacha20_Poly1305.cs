using System;

using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Crypto.Tls;

namespace Org.BouncyCastle.Crypto.TlsExt
{
	public class AEAD_Chacha20_Poly1305
	{
		private static readonly byte[] Zeroes = new byte[15];

		public static int Encrypt(byte[] plaintext, int offset, int len, byte[] additionalData, byte[] nonce, byte[] key, byte[] outBuffer)
		{
			var cipher = new ChaCha7539Engine();

			var encryptKey = new KeyParameter(key);
			cipher.Init(true, new ParametersWithIV(encryptKey, nonce));

			byte[] firstBlock = BufferPool.GetBuffer(64);
			KeyParameter macKey = GenerateRecordMacKey(cipher, firstBlock);
			
			cipher.ProcessBytes(plaintext, offset, len, outBuffer, 0);

			byte[] mac = BufferPool.GetBuffer(16);
			int macsize = CalculateRecordMac(macKey, additionalData, outBuffer, 0, len, mac);
			Array.Copy(mac, 0, outBuffer, len, macsize);

			BufferPool.ReturnBuffer(mac);
			BufferPool.ReturnBuffer(firstBlock);

			return len + 16;
		}

		public static int Decrypt(byte[] ciphertext, int offset, int len, byte[] additionalData, byte[] nonce, byte[] key, byte[] outBuffer)
		{
			var cipher = new ChaCha7539Engine();

			var decryptKey = new KeyParameter(key);
			cipher.Init(false, new ParametersWithIV(decryptKey, nonce));

			byte[] firstBlock = BufferPool.GetBuffer(64);
			KeyParameter macKey = GenerateRecordMacKey(cipher, firstBlock);

			int plaintextLength = len - 16;

			byte[] calculatedMac = BufferPool.GetBuffer(16);
			CalculateRecordMac(macKey, additionalData, ciphertext, offset, plaintextLength, calculatedMac);

			byte[] receivedMac = BufferPool.GetBuffer(16);
			Array.Copy(ciphertext, offset + plaintextLength, receivedMac, 0, receivedMac.Length);

			if (!Arrays.ConstantTimeAreEqual(calculatedMac, receivedMac))
			{
				BufferPool.ReturnBuffer(calculatedMac);
				BufferPool.ReturnBuffer(receivedMac);
				BufferPool.ReturnBuffer(firstBlock);

				throw new TlsFatalAlert(AlertDescription.bad_record_mac);
			}

			BufferPool.ReturnBuffer(calculatedMac);
			BufferPool.ReturnBuffer(receivedMac);
			BufferPool.ReturnBuffer(firstBlock);
			
			cipher.ProcessBytes(ciphertext, offset, plaintextLength, outBuffer, 0);
			return plaintextLength;
		}

		protected static KeyParameter GenerateRecordMacKey(IStreamCipher cipher, byte[] firstBlock)
		{
			cipher.ProcessBytes(firstBlock, 0, firstBlock.Length, firstBlock, 0);

			KeyParameter macKey = new KeyParameter(firstBlock, 0, 32);
			Arrays.Fill(firstBlock, (byte)0);
			return macKey;
		}

		protected static int CalculateRecordMac(KeyParameter macKey, byte[] additionalData, byte[] buf, int off, int len, byte[] outMac)
		{
			IMac mac = new Poly1305();
			mac.Init(macKey);

			UpdateRecordMacText(mac, additionalData, 0, additionalData.Length);
			UpdateRecordMacText(mac, buf, off, len);
			UpdateRecordMacLength(mac, additionalData.Length);
			UpdateRecordMacLength(mac, len);

			return MacUtilities.DoFinalOut(mac, outMac);
		}

		protected static void UpdateRecordMacLength(IMac mac, int len)
		{
			byte[] longLen = BufferPool.GetBuffer(8);
			Pack.UInt64_To_LE((ulong)len, longLen);
			mac.BlockUpdate(longLen, 0, longLen.Length);
			BufferPool.ReturnBuffer(longLen);
		}

		protected static void UpdateRecordMacText(IMac mac, byte[] buf, int off, int len)
		{
			mac.BlockUpdate(buf, off, len);

			int partial = len % 16;
			if (partial != 0)
			{
				mac.BlockUpdate(Zeroes, 0, 16 - partial);
			}
		}
	}
}
