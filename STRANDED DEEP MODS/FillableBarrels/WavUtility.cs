using System;
using UnityEngine;

namespace FillableBarrels
{
    internal class WavUtility
    {
		public static AudioClip ToAudioClip(byte[] fileBytes, int offsetSamples = 0, string name = "wav")
		{
			int num = BitConverter.ToInt32(fileBytes, 16);
			ushort code = BitConverter.ToUInt16(fileBytes, 20);
			string str = WavUtility.FormatCode(code);
			ushort channels = BitConverter.ToUInt16(fileBytes, 22);
			int frequency = BitConverter.ToInt32(fileBytes, 24);
			ushort num2 = BitConverter.ToUInt16(fileBytes, 34);
			int num3 = 20 + num + 4;
			int dataSize = BitConverter.ToInt32(fileBytes, num3);
			ushort num4 = num2;
			float[] array;
			if (num4 <= 16)
			{
				if (num4 == 8)
				{
					array = WavUtility.Convert8BitByteArrayToAudioClipData(fileBytes, num3, dataSize);
					goto IL_D2;
				}
				if (num4 == 16)
				{
					array = WavUtility.Convert16BitByteArrayToAudioClipData(fileBytes, num3, dataSize);
					goto IL_D2;
				}
			}
			else
			{
				if (num4 == 24)
				{
					array = WavUtility.Convert24BitByteArrayToAudioClipData(fileBytes, num3, dataSize);
					goto IL_D2;
				}
				if (num4 == 32)
				{
					array = WavUtility.Convert32BitByteArrayToAudioClipData(fileBytes, num3, dataSize);
					goto IL_D2;
				}
			}
			throw new Exception(num2 + " bit depth is not supported.");
		IL_D2:
			AudioClip audioClip = AudioClip.Create(name, array.Length, (int)channels, frequency, false);
			audioClip.SetData(array, 0);
			return audioClip;
		}

		private static float[] Convert8BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
		{
			int num = BitConverter.ToInt32(source, headerOffset);
			headerOffset += 4;
			float[] array = new float[num];
			sbyte maxValue = sbyte.MaxValue;
			for (int i = 0; i < num; i++)
			{
				array[i] = (float)source[i] / (float)maxValue;
			}
			return array;
		}

		private static float[] Convert16BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
		{
			int num = BitConverter.ToInt32(source, headerOffset);
			headerOffset += 4;
			int num2 = 2;
			int num3 = num / num2;
			float[] array = new float[num3];
			short maxValue = short.MaxValue;
			for (int i = 0; i < num3; i++)
			{
				int startIndex = i * num2 + headerOffset;
				array[i] = (float)BitConverter.ToInt16(source, startIndex) / (float)maxValue;
			}
			return array;
		}

		private static float[] Convert24BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
		{
			int num = BitConverter.ToInt32(source, headerOffset);
			headerOffset += 4;
			int num2 = 3;
			int num3 = num / num2;
			int maxValue = int.MaxValue;
			float[] array = new float[num3];
			byte[] array2 = new byte[4];
			for (int i = 0; i < num3; i++)
			{
				int srcOffset = i * num2 + headerOffset;
				Buffer.BlockCopy(source, srcOffset, array2, 1, num2);
				array[i] = (float)BitConverter.ToInt32(array2, 0) / (float)maxValue;
			}
			return array;
		}

		private static float[] Convert32BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
		{
			int num = BitConverter.ToInt32(source, headerOffset);
			headerOffset += 4;
			int num2 = 4;
			int num3 = num / num2;
			int maxValue = int.MaxValue;
			float[] array = new float[num3];
			for (int i = 0; i < num3; i++)
			{
				int startIndex = i * num2 + headerOffset;
				array[i] = (float)BitConverter.ToInt32(source, startIndex) / (float)maxValue;
			}
			return array;
		}

		private static string FormatCode(ushort code)
		{
			switch (code)
			{
				case 1:
					return "PCM";
				case 2:
					return "ADPCM";
				case 3:
					return "IEEE";
				case 4:
				case 5:
				case 6:
					break;
				case 7:
					return "μ-law";
				default:
					if (code == 65534)
					{
						return "WaveFormatExtensable";
					}
					break;
			}
			Debug.LogWarning("Unknown wav code format:" + code);
			return "";
		}
	}
}
