using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace FoodRestoresHealth
{
	public class WavUtility
	{
		public static AudioClip ToAudioClip(string filePath)
		{
			bool flag = !filePath.StartsWith(Application.persistentDataPath) && !filePath.StartsWith(Application.dataPath);
			AudioClip result;
			if (flag)
			{
				Debug.LogWarning("This only supports files that are stored using Unity's Application data path. \nTo load bundled resources use 'Resources.Load(\"filename\") typeof(AudioClip)' method. \nhttps://docs.unity3d.com/ScriptReference/Resources.Load.html");
				result = null;
			}
			else
			{
				byte[] fileBytes = File.ReadAllBytes(filePath);
				result = WavUtility.ToAudioClip(fileBytes, 0, "wav");
			}
			return result;
		}

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

		public static byte[] FromAudioClip(AudioClip audioClip)
		{
			string text;
			return WavUtility.FromAudioClip(audioClip, out text, false, "recordings");
		}

		public static byte[] FromAudioClip(AudioClip audioClip, out string filepath, bool saveAsFile = true, string dirname = "recordings")
		{
			MemoryStream memoryStream = new MemoryStream();
			ushort bitDepth = 16;
			int fileSize = audioClip.samples * 2 + 44;
			WavUtility.WriteFileHeader(ref memoryStream, fileSize);
			WavUtility.WriteFileFormat(ref memoryStream, audioClip.channels, audioClip.frequency, bitDepth);
			WavUtility.WriteFileData(ref memoryStream, audioClip, bitDepth);
			byte[] array = memoryStream.ToArray();
			if (saveAsFile)
			{
				filepath = string.Format("{0}/{1}/{2}.{3}", new object[]
				{
					Application.persistentDataPath,
					dirname,
					DateTime.UtcNow.ToString("yyMMdd-HHmmss-fff"),
					"wav"
				});
				Directory.CreateDirectory(Path.GetDirectoryName(filepath));
				File.WriteAllBytes(filepath, array);
			}
			else
			{
				filepath = null;
			}
			memoryStream.Dispose();
			return array;
		}

		private static int WriteFileHeader(ref MemoryStream stream, int fileSize)
		{
			int num = 0;
			byte[] bytes = Encoding.ASCII.GetBytes("RIFF");
			num += WavUtility.WriteBytesToMemoryStream(ref stream, bytes, "ID");
			int value = fileSize - 8;
			num += WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(value), "CHUNK_SIZE");
			byte[] bytes2 = Encoding.ASCII.GetBytes("WAVE");
			return num + WavUtility.WriteBytesToMemoryStream(ref stream, bytes2, "FORMAT");
		}

		private static int WriteFileFormat(ref MemoryStream stream, int channels, int sampleRate, ushort bitDepth)
		{
			int num = 0;
			byte[] bytes = Encoding.ASCII.GetBytes("fmt ");
			num += WavUtility.WriteBytesToMemoryStream(ref stream, bytes, "FMT_ID");
			int value = 16;
			num += WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(value), "SUBCHUNK_SIZE");
			ushort value2 = 1;
			num += WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(value2), "AUDIO_FORMAT");
			ushort value3 = Convert.ToUInt16(channels);
			num += WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(value3), "CHANNELS");
			num += WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(sampleRate), "SAMPLE_RATE");
			int value4 = sampleRate * channels * WavUtility.BytesPerSample(bitDepth);
			num += WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(value4), "BYTE_RATE");
			ushort value5 = Convert.ToUInt16(channels * WavUtility.BytesPerSample(bitDepth));
			num += WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(value5), "BLOCK_ALIGN");
			return num + WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(bitDepth), "BITS_PER_SAMPLE");
		}

		private static int WriteFileData(ref MemoryStream stream, AudioClip audioClip, ushort bitDepth)
		{
			int num = 0;
			float[] data = new float[audioClip.samples * audioClip.channels];
			audioClip.GetData(data, 0);
			byte[] bytes = WavUtility.ConvertAudioClipDataToInt16ByteArray(data);
			byte[] bytes2 = Encoding.ASCII.GetBytes("data");
			num += WavUtility.WriteBytesToMemoryStream(ref stream, bytes2, "DATA_ID");
			int value = Convert.ToInt32(audioClip.samples * 2);
			num += WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(value), "SAMPLES");
			return num + WavUtility.WriteBytesToMemoryStream(ref stream, bytes, "DATA");
		}

		private static byte[] ConvertAudioClipDataToInt16ByteArray(float[] data)
		{
			MemoryStream memoryStream = new MemoryStream();
			int count = 2;
			short maxValue = short.MaxValue;
			for (int i = 0; i < data.Length; i++)
			{
				memoryStream.Write(BitConverter.GetBytes(Convert.ToInt16(data[i] * (float)maxValue)), 0, count);
			}
			byte[] result = memoryStream.ToArray();
			memoryStream.Dispose();
			return result;
		}

		private static int WriteBytesToMemoryStream(ref MemoryStream stream, byte[] bytes, string tag = "")
		{
			int num = bytes.Length;
			stream.Write(bytes, 0, num);
			return num;
		}

		public static ushort BitDepth(AudioClip audioClip)
		{
			return Convert.ToUInt16((float)(audioClip.samples * audioClip.channels) * audioClip.length / (float)audioClip.frequency);
		}

		private static int BytesPerSample(ushort bitDepth)
		{
			return (int)(bitDepth / 8);
		}

		private static int BlockSize(ushort bitDepth)
		{
			int result;
			if (bitDepth != 8)
			{
				if (bitDepth != 16)
				{
					if (bitDepth != 32)
					{
						throw new Exception(bitDepth + " bit depth is not supported.");
					}
					result = 4;
				}
				else
				{
					result = 2;
				}
			}
			else
			{
				result = 1;
			}
			return result;
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

		private const int BlockSize_16Bit = 2;
	}
}
