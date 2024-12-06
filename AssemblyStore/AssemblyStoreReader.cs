using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Xamarin.Android.AssemblyStore
{

	internal class AssemblyStoreReader
	{
		private const uint ASSEMBLY_STORE_MAGIC = 1094861144u;

		private const uint ASSEMBLY_STORE_FORMAT_VERSION = 1u;


		private MemoryStream storeData;

		public uint Version { get; private set; }

		public uint LocalEntryCount { get; private set; }

		public uint GlobalEntryCount { get; private set; }

		public uint StoreID { get; private set; }

		public List<AssemblyStoreAssembly> Assemblies { get; }

		public List<AssemblyStoreHashEntry> GlobalIndex32 { get; } = new List<AssemblyStoreHashEntry>();


		public List<AssemblyStoreHashEntry> GlobalIndex64 { get; } = new List<AssemblyStoreHashEntry>();


		public string Arch { get; }

		public bool HasGlobalIndex => StoreID == 0;

		public AssemblyStoreReader(Stream store, string arch = null, bool keepStoreInMemory = false)
		{
			Arch = arch ?? string.Empty;
			store.Seek(0L, SeekOrigin.Begin);
			if (keepStoreInMemory)
			{
				storeData = new MemoryStream();
				store.CopyTo(storeData);
				storeData.Flush();
				store.Seek(0L, SeekOrigin.Begin);
			}
			using (BinaryReader reader = new BinaryReader(store, Encoding.UTF8, leaveOpen: true))
			{
				ReadHeader(reader);
				Assemblies = new List<AssemblyStoreAssembly>();
				ReadLocalEntries(reader, Assemblies);
				if (HasGlobalIndex)
				{
					ReadGlobalIndex(reader, GlobalIndex32, GlobalIndex64);
				}
			}
		}

		internal void ExtractAssemblyImage(AssemblyStoreAssembly assembly, string outputFilePath)
		{
			SaveDataToFile(outputFilePath, assembly.DataOffset, assembly.DataSize);
		}

		internal void ExtractAssemblyImage(AssemblyStoreAssembly assembly, Stream output)
		{
			SaveDataToStream(output, assembly.DataOffset, assembly.DataSize);
		}

		internal void ExtractAssemblyDebugData(AssemblyStoreAssembly assembly, string outputFilePath)
		{
			if (assembly.DebugDataOffset != 0 && assembly.DebugDataSize != 0)
			{
				SaveDataToFile(outputFilePath, assembly.DebugDataOffset, assembly.DebugDataSize);
			}
		}

		internal void ExtractAssemblyDebugData(AssemblyStoreAssembly assembly, Stream output)
		{
			if (assembly.DebugDataOffset != 0 && assembly.DebugDataSize != 0)
			{
				SaveDataToStream(output, assembly.DebugDataOffset, assembly.DebugDataSize);
			}
		}

		internal void ExtractAssemblyConfig(AssemblyStoreAssembly assembly, string outputFilePath)
		{
			if (assembly.ConfigDataOffset != 0 && assembly.ConfigDataSize != 0)
			{
				SaveDataToFile(outputFilePath, assembly.ConfigDataOffset, assembly.ConfigDataSize);
			}
		}

		internal void ExtractAssemblyConfig(AssemblyStoreAssembly assembly, Stream output)
		{
			if (assembly.ConfigDataOffset != 0 && assembly.ConfigDataSize != 0)
			{
				SaveDataToStream(output, assembly.ConfigDataOffset, assembly.ConfigDataSize);
			}
		}

		private void SaveDataToFile(string outputFilePath, uint offset, uint size)
		{
			EnsureStoreDataAvailable();
			using (FileStream fs = File.Open(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				SaveDataToStream(fs, offset, size);
			}
		}

		private void SaveDataToStream(Stream output, uint offset, uint size)
		{
			EnsureStoreDataAvailable();
			ArrayPool<byte> pool = ArrayPool<byte>.Shared;
			storeData.Seek(offset, SeekOrigin.Begin);
			byte[] buf = pool.Rent(16384);
			long toRead = size;
			int nread;
			while (toRead > 0 && (nread = storeData.Read(buf, 0, buf.Length)) > 0)
			{
				if (nread > toRead)
				{
					nread = (int)toRead;
				}
				output.Write(buf, 0, nread);
				toRead -= nread;
			}
			output.Flush();
			pool.Return(buf);
		}

		private void EnsureStoreDataAvailable()
		{
			if (storeData != null)
			{
				return;
			}
			throw new InvalidOperationException("Store data not available. AssemblyStore/AssemblyStoreExplorer must be instantiated with the `keepStoreInMemory` argument set to `true`");
		}

		public bool HasIdenticalContent(AssemblyStoreReader other)
		{
			if (other.Version == Version && other.LocalEntryCount == LocalEntryCount && other.GlobalEntryCount == GlobalEntryCount && other.StoreID == StoreID && other.Assemblies.Count == Assemblies.Count && other.GlobalIndex32.Count == GlobalIndex32.Count)
			{
				return other.GlobalIndex64.Count == GlobalIndex64.Count;
			}
			return false;
		}

		private void ReadHeader(BinaryReader reader)
		{
			if (reader.ReadUInt32() != 1094861144)
			{
				throw new InvalidOperationException("Invalid header magic number");
			}
			Version = reader.ReadUInt32();
			if (Version == 0)
			{
				throw new InvalidOperationException("Invalid version number: 0");
			}
			if (Version > 1)
			{
				throw new InvalidOperationException($"Store format version {Version} is higher than the one understood by this reader, {1u}");
			}
			LocalEntryCount = reader.ReadUInt32();
			GlobalEntryCount = reader.ReadUInt32();
			StoreID = reader.ReadUInt32();
		}

		private void ReadLocalEntries(BinaryReader reader, List<AssemblyStoreAssembly> assemblies)
		{
			for (uint i = 0u; i < LocalEntryCount; i++)
			{
				assemblies.Add(new AssemblyStoreAssembly(reader, this));
			}
		}

		private void ReadGlobalIndex(BinaryReader reader, List<AssemblyStoreHashEntry> index32, List<AssemblyStoreHashEntry> index64)
		{
			ReadIndex(is32Bit: true, index32);
			ReadIndex(is32Bit: true, index64);
			void ReadIndex(bool is32Bit, List<AssemblyStoreHashEntry> index)
			{
				for (uint i = 0u; i < GlobalEntryCount; i++)
				{
					index.Add(new AssemblyStoreHashEntry(reader, is32Bit));
				}
			}
		}

	}
}
