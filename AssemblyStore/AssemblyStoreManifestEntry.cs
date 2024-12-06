using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Xamarin.Android.AssemblyStore
{

	internal class AssemblyStoreManifestEntry
	{
		private const int NumberOfFields = 5;

		private const int Hash32FieldIndex = 0;

		private const int Hash64FieldIndex = 1;

		private const int StoreIDFieldIndex = 2;

		private const int StoreIndexFieldIndex = 3;

		private const int NameFieldIndex = 4;

		public uint Hash32 { get; }

		public ulong Hash64 { get; }

		public uint StoreID { get; }

		public uint IndexInStore { get; }

		public string Name { get; }

		public AssemblyStoreManifestEntry(string[] fields)
		{
			if (fields.Length != 5)
			{
				throw new ArgumentOutOfRangeException("fields", "Invalid number of fields");
			}
			Hash32 = GetUInt32(fields[0]);
			Hash64 = GetUInt64(fields[1]);
			StoreID = GetUInt32(fields[2]);
			IndexInStore = GetUInt32(fields[3]);
			Name = fields[4].Trim();
		}

		private uint GetUInt32(string value)
		{
			if (uint.TryParse(PrepHexValue(value), NumberStyles.HexNumber, null, out var hash))
			{
				return hash;
			}
			return 0u;
		}

		private ulong GetUInt64(string value)
		{
			if (ulong.TryParse(PrepHexValue(value), NumberStyles.HexNumber, null, out var hash))
			{
				return hash;
			}
			return 0uL;
		}

		private string PrepHexValue(string value)
		{
			if (value.StartsWith("0x", StringComparison.Ordinal))
			{
				return value.Substring(2);
			}
			return value;
		}
	}
}
