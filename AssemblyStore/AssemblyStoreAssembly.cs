using System.IO;
using System.Runtime.CompilerServices;

namespace Xamarin.Android.AssemblyStore
{

	internal class AssemblyStoreAssembly
	{
		public uint DataOffset { get; }

		public uint DataSize { get; }

		public uint DebugDataOffset { get; }

		public uint DebugDataSize { get; }

		public uint ConfigDataOffset { get; }

		public uint ConfigDataSize { get; }

		public uint Hash32 { get; set; }

		public ulong Hash64 { get; set; }

		public string Name { get; set; } = string.Empty;


		public uint RuntimeIndex { get; set; }

		public AssemblyStoreReader Store { get; }

		public string DllName => MakeFileName("dll");

		public string PdbName => MakeFileName("pdb");

		public string ConfigName => MakeFileName("dll.config");

		internal AssemblyStoreAssembly(BinaryReader reader, AssemblyStoreReader store)
		{
			Store = store;
			DataOffset = reader.ReadUInt32();
			DataSize = reader.ReadUInt32();
			DebugDataOffset = reader.ReadUInt32();
			DebugDataSize = reader.ReadUInt32();
			ConfigDataOffset = reader.ReadUInt32();
			ConfigDataSize = reader.ReadUInt32();
		}

		public void ExtractImage(string outputDirPath,  string fileName = null)
		{
			Store.ExtractAssemblyImage(this, MakeOutputFilePath(outputDirPath, "dll", fileName));
		}

		public void ExtractImage(Stream output)
		{
			Store.ExtractAssemblyImage(this, output);
		}

		public void ExtractDebugData(string outputDirPath,string fileName = null)
		{
			Store.ExtractAssemblyDebugData(this, MakeOutputFilePath(outputDirPath, "pdb", fileName));
		}

		public void ExtractDebugData(Stream output)
		{
			Store.ExtractAssemblyDebugData(this, output);
		}

		public void ExtractConfig(string outputDirPath, string fileName = null)
		{
			Store.ExtractAssemblyConfig(this, MakeOutputFilePath(outputDirPath, "dll.config", fileName));
		}

		public void ExtractConfig(Stream output)
		{
			Store.ExtractAssemblyConfig(this, output);
		}

		private string MakeOutputFilePath(string outputDirPath, string extension, string fileName)
		{
			return Path.Combine(outputDirPath, MakeFileName(extension, fileName));
		}

		private string MakeFileName(string extension, string fileName = null)
		{
			if (string.IsNullOrEmpty(fileName))
			{
				fileName = Name;
				if (string.IsNullOrEmpty(fileName))
				{
					fileName = $"{Hash32:x}_{Hash64:x}";
				}
				fileName = fileName + "." + extension;
			}
			return fileName;
		}
	}
}
