using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Xamarin.Android.AssemblyStore
{

	internal class AssemblyStoreManifestReader
	{
		private static readonly char[] fieldSplit = new char[1] { ' ' };

		public List<AssemblyStoreManifestEntry> Entries { get; } = new List<AssemblyStoreManifestEntry>();


		public Dictionary<uint, AssemblyStoreManifestEntry> EntriesByHash32 { get; } = new Dictionary<uint, AssemblyStoreManifestEntry>();


		public Dictionary<ulong, AssemblyStoreManifestEntry> EntriesByHash64 { get; } = new Dictionary<ulong, AssemblyStoreManifestEntry>();


		public AssemblyStoreManifestReader(Stream manifest)
		{
			manifest.Seek(0L, SeekOrigin.Begin);
			using (StreamReader sr = new StreamReader(manifest, Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
			{
				ReadManifest(sr);
			}
		}

		private void ReadManifest(StreamReader reader)
		{
			reader.ReadLine();
			while (!reader.EndOfStream)
			{
				string[] fields = reader.ReadLine()?.Split(fieldSplit, StringSplitOptions.RemoveEmptyEntries);
				if (fields != null)
				{
					AssemblyStoreManifestEntry entry = new AssemblyStoreManifestEntry(fields);
					Entries.Add(entry);
					if (entry.Hash32 != 0)
					{
						EntriesByHash32.Add(entry.Hash32, entry);
					}
					if (entry.Hash64 != 0L)
					{
						EntriesByHash64.Add(entry.Hash64, entry);
					}
				}
			}
		}
	}
}
