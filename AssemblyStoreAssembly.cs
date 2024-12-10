using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using K4os.Compression.LZ4;
using StardewModdingAPI;
using xxHashSharp;


public static class Constants
{
    public const string ASSEMBLY_STORE_MAGIC = "XAMS";
    public const int ASSEMBLY_STORE_FORMAT_VERSION = 1;
    public const string COMPRESSED_DATA_MAGIC = "XALZ";
    public const string FILE_ASSEMBLIES_MANIFEST = "assemblies.manifest";
    public const string FILE_ASSEMBLIES_BLOB = "assemblies.blob";
    public const string FILE_ASSEMBLIES_JSON = "assemblies.json";
    public static readonly Dictionary<string, string> ARCHITECTURE_MAP = new Dictionary<string, string>
    {
        { "arm64", "assemblies.arm64.blob" },
        { "x86", "assemblies.x86.blob" },
        { "x86_64", "assemblies.x86_64.blob" }
    };
}





public class AssemblyStoreAssembly
{
    public int DataOffset { get; set; }
    public int DataSize { get; set; }
    public int DebugDataOffset { get; set; }
    public int DebugDataSize { get; set; }
    public int ConfigDataOffset { get; set; }
    public int ConfigDataSize { get; set; }
    public string DllName { get; internal set; }
}

public class AssemblyStoreHashEntry
{
    public string HashVal { get; set; }
    public int MappingIndex { get; set; }
    public int LocalStoreIndex { get; set; }
    public int StoreId { get; set; }
}

public class AssemblyStore
{
    public byte[] Raw { get; set; }
    public string FileName { get; set; }
    public ManifestList ManifestEntries { get; set; }
    public string HdrMagic { get; set; }
    public int HdrVersion { get; set; }
    public int HdrLec { get; set; }
    public int HdrGec { get; set; }
    public int HdrStoreId { get; set; }
    public List<AssemblyStoreAssembly> AssembliesList { get; set; }
    public List<AssemblyStoreHashEntry> GlobalHash32 { get; set; }
    public List<AssemblyStoreHashEntry> GlobalHash64 { get; set; }

    public AssemblyStore(string inFileName, ManifestList manifestEntries, bool primary = true)
    {
        ManifestEntries = manifestEntries;
        FileName = Path.GetFileName(inFileName);
        Raw = File.ReadAllBytes(inFileName);

        using (var blobFile = new BinaryReader(File.OpenRead(inFileName)))
        {
            // Header Section
            HdrMagic = Encoding.ASCII.GetString(blobFile.ReadBytes(4));
            if (HdrMagic != Constants.ASSEMBLY_STORE_MAGIC)
            {
                throw new Exception($"Invalid Magic: {HdrMagic}");
            }

            HdrVersion = blobFile.ReadInt32();
            if (HdrVersion > Constants.ASSEMBLY_STORE_FORMAT_VERSION)
            {
                throw new Exception($"This version is higher than expected! Max = {Constants.ASSEMBLY_STORE_FORMAT_VERSION}, got {HdrVersion}");
            }

            HdrLec = blobFile.ReadInt32();
            HdrGec = blobFile.ReadInt32();
            HdrStoreId = blobFile.ReadInt32();

            Debug($"Local entry count: {HdrLec}");
            Debug($"Global entry count: {HdrGec}");

            AssembliesList = new List<AssemblyStoreAssembly>();

            Debug($"Entries start at: {blobFile.BaseStream.Position} (0x{blobFile.BaseStream.Position:X})");

            for (int i = 0; i < HdrLec; i++)
            {
                Debug($"Extracting Assembly: {blobFile.BaseStream.Position} (0x{blobFile.BaseStream.Position:X})");
                var entry = blobFile.ReadBytes(24);

                var assembly = new AssemblyStoreAssembly
                {
                    DataOffset = BitConverter.ToInt32(entry, 0),
                    DataSize = BitConverter.ToInt32(entry, 4),
                    DebugDataOffset = BitConverter.ToInt32(entry, 8),
                    DebugDataSize = BitConverter.ToInt32(entry, 12),
                    ConfigDataOffset = BitConverter.ToInt32(entry, 16),
                    ConfigDataSize = BitConverter.ToInt32(entry, 20)
                };

                AssembliesList.Add(assembly);

                Debug($"  Data Offset: {assembly.DataOffset} (0x{assembly.DataOffset:X})");
                Debug($"  Data Size: {assembly.DataSize} (0x{assembly.DataSize:X})");
                Debug($"  Config Offset: {assembly.ConfigDataOffset} (0x{assembly.ConfigDataOffset:X})");
                Debug($"  Config Size: {assembly.ConfigDataSize} (0x{assembly.ConfigDataSize:X})");
                Debug($"  Debug Offset: {assembly.DebugDataOffset} (0x{assembly.DebugDataOffset:X})");
                Debug($"  Debug Size: {assembly.DebugDataSize} (0x{assembly.DebugDataSize:X})");
            }

            if (!primary)
            {
                Debug("Skipping hash sections in non-primary store");
                return;
            }

            Debug($"Hash32 start at: {blobFile.BaseStream.Position} (0x{blobFile.BaseStream.Position:X})");
            GlobalHash32 = new List<AssemblyStoreHashEntry>();

            for (int i = 0; i < HdrLec; i++)
            {
                var entry = blobFile.ReadBytes(20);

                var hashEntry = new AssemblyStoreHashEntry
                {
                    HashVal = $"0x{BitConverter.ToUInt32(entry, 0):X8}",
                    MappingIndex = BitConverter.ToInt32(entry, 8),
                    LocalStoreIndex = BitConverter.ToInt32(entry, 12),
                    StoreId = BitConverter.ToInt32(entry, 16)
                };

                Debug("New Hash32 Section:");
                Debug($"   mapping index: {hashEntry.MappingIndex}");
                Debug($"   local store index: {hashEntry.LocalStoreIndex}");
                Debug($"   store id: {hashEntry.StoreId}");
                Debug($"   Hash32: {hashEntry.HashVal}");

                GlobalHash32.Add(hashEntry);
            }

            Debug($"Hash64 start at: {blobFile.BaseStream.Position} (0x{blobFile.BaseStream.Position:X})");
            GlobalHash64 = new List<AssemblyStoreHashEntry>();

            for (int i = 0; i < HdrLec; i++)
            {
                var entry = blobFile.ReadBytes(20);

                var hashEntry = new AssemblyStoreHashEntry
                {
                    HashVal = $"0x{BitConverter.ToUInt64(entry, 0):X16}",
                    MappingIndex = BitConverter.ToInt32(entry, 8),
                    LocalStoreIndex = BitConverter.ToInt32(entry, 12),
                    StoreId = BitConverter.ToInt32(entry, 16)
                };

                Debug("New Hash64 Section:");
                Debug($"   mapping index: {hashEntry.MappingIndex}");
                Debug($"   local store index: {hashEntry.LocalStoreIndex}");
                Debug($"   store id: {hashEntry.StoreId}");
                Debug($"   Hash64: {hashEntry.HashVal}");

                GlobalHash64.Add(hashEntry);
            }
        }
    }

    public Dictionary<string, object> ExtractAll(Dictionary<string, object> jsonConfig, string outPath = "out")
    {
        var storeJson = new Dictionary<string, object>
    {
        { FileName, new Dictionary<string, object>
            {
                { "header", new Dictionary<string, int>
                    {
                        { "version", HdrVersion },
                        { "lec", HdrLec },
                        { "gec", HdrGec },
                        { "store_id", HdrStoreId }
                    }
                }
            }
        }
    };

        // 确保 jsonConfig["stores"] 和 jsonConfig["assemblies"] 是 List<object> 类型
        if (jsonConfig["stores"] == null)
            jsonConfig["stores"] = new List<object>();
        if (jsonConfig["assemblies"] == null)
            jsonConfig["assemblies"] = new List<object>();

        var storesList = jsonConfig["stores"] as List<object>;
        var assembliesList = jsonConfig["assemblies"] as List<object>;

        for (int i = 0; i < AssembliesList.Count; i++)
        {
            var assembly = AssembliesList[i];
            var assemblyDict = new Dictionary<string, object>
        {
            { "lz4", false }
        };

            var entry = ManifestEntries.GetIdx(HdrStoreId, i);
            assemblyDict["name"] = entry.Name;
            assemblyDict["store_id"] = entry.BlobId;
            assemblyDict["blob_idx"] = entry.BlobIdx;
            assemblyDict["hash32"] = entry.Hash32;
            assemblyDict["hash64"] = entry.Hash64;

            var outFile = Path.Combine(outPath, $"{entry.Name}.dll");
            assemblyDict["file"] = outFile;

            var assemblyHeader = Raw.Skip(assembly.DataOffset).Take(4).ToArray();
            byte[] assemblyData;
            if (Encoding.ASCII.GetString(assemblyHeader) == Constants.COMPRESSED_DATA_MAGIC)
            {
                assemblyData = DecompressLz4(Raw.Skip(assembly.DataOffset).Take(assembly.DataSize).ToArray());
                assemblyDict["lz4"] = true;
                assemblyDict["lz4_desc_idx"] = BitConverter.ToInt32(Raw, assembly.DataOffset + 4);
            }
            else
            {
                assemblyData = Raw.Skip(assembly.DataOffset).Take(assembly.DataSize).ToArray();
            }

            Console.WriteLine($"Extracting {entry.Name}...");

            if (!Directory.Exists(Path.GetDirectoryName(outFile)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outFile));
            }

            File.WriteAllBytes(outFile, assemblyData);

            // 添加到 assembliesList
            assembliesList.Add(assemblyDict);
        }

        // 添加 storeJson 到 storesList
        storesList.Add(storeJson);

        return jsonConfig;
    }


    public static byte[] DecompressLz4(byte[] compressedData)
    {
        var packedPayloadLen = BitConverter.ToInt32(compressedData, 8);
        var compressedPayload = compressedData.Skip(12).ToArray();

        //666
        // 直接解压缩并返回解压后的字节数组
        return LZ4Pickler.Unpickle(compressedPayload); // 假设 Unpickle 返回 byte[]
    }


    public static byte[] Lz4Compress(byte[] fileData, int descIdx)
    {
        var packed = new List<byte>();
        packed.AddRange(Encoding.ASCII.GetBytes(Constants.COMPRESSED_DATA_MAGIC));
        packed.AddRange(BitConverter.GetBytes(descIdx));
        packed.AddRange(BitConverter.GetBytes(fileData.Length));

        var compressedData = LZ4Pickler.Pickle(fileData, LZ4Level.L12_MAX);
        packed.AddRange(compressedData);

        return packed.ToArray();
    }
  
    //666

    public static ManifestList ReadManifest(string inManifest)
    {
        var manifestList = new ManifestList();
        foreach (var line in File.ReadAllLines(inManifest))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("Hash"))
            {
                continue;
            }

            var splitLine = line.Split();

            // 将字符串转换为整数
            int blobId = int.Parse(splitLine[2]);   // 假设这是整数
            int blobIdx = int.Parse(splitLine[3]);  // 假设这是整数

            // 使用转换后的整数来创建 ManifestEntry
            manifestList.Add(new ManifestEntry(splitLine[0], splitLine[1], blobId, blobIdx, splitLine[4]));
        }

        return manifestList;
    }

    public class ManifestList : List<ManifestEntry>
    {
        public ManifestEntry GetIdx(int blobId, int blobIdx)
        {
            return this.FirstOrDefault(entry => entry.BlobIdx == blobIdx && entry.BlobId == blobId);
        }
    }
    public class ManifestEntry
    {
        public string Hash32 { get; set; }
        public string Hash64 { get; set; }
        public int BlobId { get; set; }
        public int BlobIdx { get; set; }
        public string Name { get; set; }

        public ManifestEntry(string hash32, string hash64, int blobId, int blobIdx, string name)
        {
            Hash32 = hash32;
            Hash64 = hash64;
            BlobId = blobId;
            BlobIdx = blobIdx;
            Name = name;
        }
    }
    public static int Usage()
    {
        Console.WriteLine("usage: pyxamstore MODE <args>");
        Console.WriteLine();
        Console.WriteLine("   MODES:");
        Console.WriteLine("\tunpack <args>  Unpack assembly blobs.");
        Console.WriteLine("\tpack <args>    Repackage assembly blobs.");
        Console.WriteLine("\thash file_name Generate xxHash values.");
        Console.WriteLine("\thelp           Print this message.");

        return 0;
    }

    public static int DoUnpack(string inDirectory, string inArch, bool force)
    {
        var archAssemblies = false;

        if (force && Directory.Exists("out/"))
        {
            Directory.Delete("out/", true);
        }

        if (Directory.Exists("out/"))
        {
            Console.WriteLine("Out directory already exists!");
            return 3;
        }

        var manifestPath = Path.Combine(inDirectory, Constants.FILE_ASSEMBLIES_MANIFEST);
        var assembliesPath = Path.Combine(inDirectory, Constants.FILE_ASSEMBLIES_BLOB);

        if (!File.Exists(manifestPath))
        {
            Console.WriteLine($"Manifest file '{manifestPath}' does not exist!");
            return 4;
        }
        else if (!File.Exists(assembliesPath))
        {
            Console.WriteLine($"Main assemblies blob '{assembliesPath}' does not exist!");
            return 4;
        }

        var manifestEntries = ReadManifest(manifestPath);
        if (manifestEntries == null)
        {
            Console.WriteLine("Unable to parse assemblies.manifest file!");
            return 5;
        }

        var jsonData = new Dictionary<string, object>
    {
        { "stores", new List<object>() },
        { "assemblies", new List<object>() }
    };

        Directory.CreateDirectory("out/");

        var assemblyStore = new AssemblyStore(assembliesPath, manifestEntries);

        if (assemblyStore.HdrLec != assemblyStore.HdrGec)
        {
            archAssemblies = true;
            Debug("There are more assemblies to unpack here!");
        }

        jsonData = assemblyStore.ExtractAll(jsonData);

        if (archAssemblies)
        {
            var archAssembliesPath = Path.Combine(inDirectory, Constants.ARCHITECTURE_MAP[inArch]);
            var archAssemblyStore = new AssemblyStore(archAssembliesPath, manifestEntries, primary: false);
            jsonData = archAssemblyStore.ExtractAll(jsonData);
        }

        File.WriteAllText(Constants.FILE_ASSEMBLIES_JSON, JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
    public static Tuple<string, string> GenXxhash(string name, bool raw = false)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(name);
            byte[] hash32 = sha256.ComputeHash(nameBytes);
            byte[] hash64 = new byte[8];

            // 取 SHA256 前8字节作为64位哈希
            Array.Copy(hash32, hash64, 8);

            // 结果根据 raw 参数返回
            if (raw)
            {
                Array.Reverse(hash32); // 模拟字节反转
                Array.Reverse(hash64); // 模拟字节反转
                return new Tuple<string, string>(BitConverter.ToString(hash32).Replace("-", ""), BitConverter.ToString(hash64).Replace("-", ""));
            }

            return new Tuple<string, string>(BitConverter.ToString(hash32).Replace("-", "").ToLower(), BitConverter.ToString(hash64).Replace("-", "").ToLower());
        }
    }
    public static int GetDoPack(string inJsonConfig)
    {
        // 检查配置文件是否存在
        if (!File.Exists(inJsonConfig))
        {
            Console.WriteLine($"Config file '{inJsonConfig}' does not exist!");
            return -1;
        }

        // 定义文件路径
        string manifestPath = Path.Combine(SMAPIMainActivity.externalFilesDir, "assemblies.manifest");
        string blobPath = Path.Combine(SMAPIMainActivity.externalFilesDir, "assemblies.blob");

        // 检查是否已有文件
        if (!File.Exists(manifestPath) || !File.Exists(blobPath))
        {
            Console.WriteLine($"Required files 'assemblies.manifest' or 'assemblies.blob' not found!");
            return -2;
        }

        // 读取 JSON 配置
        string jsonData = File.ReadAllText(inJsonConfig);
        JsonDocument jsonDoc = JsonDocument.Parse(jsonData);
        var assemblies = jsonDoc.RootElement.GetProperty("assemblies").EnumerateArray();

        // 读取原始 manifest 数据
        var manifestLines = File.ReadAllLines(manifestPath).ToList();
        int currentIdx = manifestLines.Count; // 根据 manifest 条目数确定 idx 起始值

        // 读取原始 blob 文件
        byte[] originalBlobData = File.ReadAllBytes(blobPath);
        int currentBlobDataOffset = originalBlobData.Length;

        // 准备追加的数据
        List<string> newManifestEntries = new List<string>();
        MemoryStream newBlobDataStream = new MemoryStream();

        foreach (JsonElement assembly in assemblies)
        {
            string assemblyName = assembly.GetProperty("name").GetString();
            string filePath = Path.Combine(SMAPIMainActivity.externalFilesDir, assemblyName);

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Assembly file '{filePath}' does not exist!");
                return -3;
            }

            // 计算哈希值
            string hash32 = HashUtils.ComputeHash32(filePath);
            string hash64 = HashUtils.ComputeHash64(filePath);

            // 读取 DLL 数据
            byte[] assemblyData = File.ReadAllBytes(filePath);

            // 压缩处理（如需要）
            if (assembly.GetProperty("lz4").GetBoolean())
            {
                int lz4DescIdx = assembly.GetProperty("lz4_desc_idx").GetInt32();
                assemblyData = Lz4Compress(assemblyData, lz4DescIdx);
            }

            // 准备 manifest 条目
            int storeId = assembly.GetProperty("store_id").GetInt32();
            string formattedStoreId = storeId.ToString("D3"); // 格式化为 3 位数
            string formattedIdx = currentIdx.ToString("D4"); // 格式化为 4 位数
            string manifestLine = $"0x{hash32,-8}  0x{hash64,-16}  {formattedStoreId}      {formattedIdx}      {assemblyName}";
            newManifestEntries.Add(manifestLine);

            // 写入 blob 数据
            using (BinaryWriter writer = new BinaryWriter(newBlobDataStream, System.Text.Encoding.Default, true))
            {
                // 写入条目元数据
                writer.Write(currentBlobDataOffset); // 偏移量
                writer.Write(assemblyData.Length);   // 数据长度
                writer.Write(new byte[16]);          // 预留字段

                // 写入实际数据
                currentBlobDataOffset += assemblyData.Length;
                writer.Write(assemblyData);
            }

            currentIdx++; // 递增 idx


        }

        // 更新 manifest 文件
        using (StreamWriter manifestWriter = new StreamWriter(manifestPath, append: true))
        {
            foreach (var entry in newManifestEntries)
            {
                manifestWriter.WriteLine(entry);
            }
        }

        // 更新 blob 文件
        using (FileStream blobFileStream = new FileStream(blobPath, FileMode.Append, FileAccess.Write))
        {
            newBlobDataStream.WriteTo(blobFileStream);
        }

        Console.WriteLine("Packing completed successfully!");
        return 0;
    }

    public static int DoPack(string inJsonConfig)
    {
        if (!File.Exists(inJsonConfig))
        {
            Console.WriteLine($"Config file '{inJsonConfig}' does not exist!");
            return -1;
        }

        if (File.Exists("assemblies.manifest.new"))
        {
            Console.WriteLine("Output manifest exists!");
            return -2;
        }

        if (File.Exists("assemblies.blob.new"))
        {
            Console.WriteLine("Output blob exists!");
            return -3;
        }

        // Read JSON configuration
        string jsonData = File.ReadAllText(inJsonConfig);
        JsonDocument jsonDoc = JsonDocument.Parse(jsonData);
        var assemblies = jsonDoc.RootElement.GetProperty("assemblies").EnumerateArray();
        var stores = jsonDoc.RootElement.GetProperty("stores").EnumerateArray();

        // Write new assemblies.manifest
        Console.WriteLine("Writing 'assemblies.manifest.new'...");
        string outputPath = Path.Combine(GameMainActivity.externalFilesDir,"assemblies.manifest");
        using (StreamWriter assembliesManifestF = new StreamWriter(outputPath))
        {
            assembliesManifestF.WriteLine("Hash 32     Hash 64             Blob ID  Blob idx  Name");

            foreach (JsonElement assembly in assemblies)
            {
                string assemblyName = assembly.GetProperty("name").GetString();

                // 假设 DLL 文件位于 "dlls" 目录下
                string filePath = Path.Combine(GameMainActivity.externalFilesDir, assemblyName);

                string hash32 = HashUtils.ComputeHash32(filePath);
                string hash64 = HashUtils.ComputeHash64(filePath);

                string line = $"0x{hash32,8}  0x{hash64,16}  {assembly.GetProperty("store_id").GetInt32(),3}      {assembly.GetProperty("blob_idx").GetInt32(),4}      {assemblyName}";
                assembliesManifestF.WriteLine(line);
            }
        }


        // Handle store logic
        int storeZeroLec = 0;
        foreach (JsonElement store in stores)
        {
            foreach (JsonProperty storeProp in store.EnumerateObject())
            {
                if (storeProp.Name == "assemblies.blob")
                {
                    storeZeroLec = storeProp.Value.GetProperty("header").GetProperty("lec").GetInt32();
                }
            }
        }

        // Now handle the blobs
        foreach (JsonElement store in stores)
        {
            foreach (JsonProperty storeProp in store.EnumerateObject())
            {
                string outStoreName = $"{storeProp.Name}";
                Console.WriteLine($"Writing '{outStoreName}'...");

                using (FileStream assembliesBlobF = new FileStream( GameMainActivity.externalFilesDir  +"/"+  outStoreName, FileMode.Create))
                using (BinaryWriter writer = new BinaryWriter(assembliesBlobF))
                {
                    JsonElement storeData = storeProp.Value;
                    JsonElement jsonHdr = storeData.GetProperty("header");

                    // Write header
                    writer.Write(Constants.ASSEMBLY_STORE_MAGIC);
                    writer.Write(jsonHdr.GetProperty("version").GetInt32());
                    writer.Write(jsonHdr.GetProperty("lec").GetInt32());
                    writer.Write(jsonHdr.GetProperty("gec").GetInt32());
                    writer.Write(jsonHdr.GetProperty("store_id").GetInt32());

                    bool isPrimaryStore = jsonHdr.GetProperty("store_id").GetInt32() == 0;

                    int nextEntryOffset = 20;
                    int nextDataOffset = 20 + (jsonHdr.GetProperty("lec").GetInt32() * 24) + (jsonHdr.GetProperty("gec").GetInt32() * 40);

                    if (!isPrimaryStore)
                    {
                        nextDataOffset = 20 + (jsonHdr.GetProperty("lec").GetInt32() * 24);
                    }

                    // First pass: Write the entries and DLL content
                    foreach (JsonElement assembly in assemblies)
                    {
                        if (assembly.GetProperty("store_id").GetInt32() != jsonHdr.GetProperty("store_id").GetInt32())
                        {
                            Console.WriteLine("Skipping assembly for another store");
                            continue;
                        }

                        string assemblyFile = assembly.GetProperty("file").GetString();
                        byte[] assemblyData = File.ReadAllBytes(assemblyFile);
                        if (assembly.GetProperty("lz4").GetBoolean())
                        {
                            int lz4DescIdx = assembly.GetProperty("lz4_desc_idx").GetInt32();
                            assemblyData = Lz4Compress(assemblyData, lz4DescIdx);
                        }

                        int dataSize = assemblyData.Length;

                        // Write the entry data
                        writer.Seek(nextEntryOffset, SeekOrigin.Begin);
                        writer.Write(nextDataOffset);
                        writer.Write(dataSize);
                        writer.Write(0);
                        writer.Write(0);
                        writer.Write(0);
                        writer.Write(0);

                        // Write binary data
                        writer.Seek(nextDataOffset, SeekOrigin.Begin);
                        writer.Write(assemblyData);

                        nextDataOffset += dataSize;
                        nextEntryOffset += 24;
                    }

                    // Handle hashes (hash32 and hash64)
                    if (!isPrimaryStore)
                    {
                        continue;
                    }

                    int nextHash32Offset = 20 + (jsonHdr.GetProperty("lec").GetInt32() * 24);
                    int nextHash64Offset = 20 + (jsonHdr.GetProperty("lec").GetInt32() * 24) + (jsonHdr.GetProperty("gec").GetInt32() * 20);

                    foreach (JsonElement assembly in assemblies.OrderBy(a => a.GetProperty("hash32").GetString()))
                    {
                        string hash32, hash64;
                        (hash32, hash64) = GenXxhash(assembly.GetProperty("name").GetString());

                        int mappingId = (assembly.GetProperty("store_id").GetInt32() == 0)
                            ? assembly.GetProperty("blob_idx").GetInt32()
                            : storeZeroLec + assembly.GetProperty("blob_idx").GetInt32();

                        // Write hash32
                        writer.Seek(nextHash32Offset, SeekOrigin.Begin);
                        writer.Write(Encoding.ASCII.GetBytes(hash32));
                        writer.Write(0);
                        writer.Write(mappingId);
                        writer.Write(assembly.GetProperty("blob_idx").GetInt32());
                        writer.Write(assembly.GetProperty("store_id").GetInt32());

                        nextHash32Offset += 20;
                    }

                    foreach (JsonElement assembly in assemblies.OrderBy(a => a.GetProperty("hash64").GetString()))
                    {
                        string hash32, hash64;
                        (hash32, hash64) = GenXxhash(assembly.GetProperty("name").GetString());

                        int mappingId = (assembly.GetProperty("store_id").GetInt32() == 0)
                            ? assembly.GetProperty("blob_idx").GetInt32()
                            : storeZeroLec + assembly.GetProperty("blob_idx").GetInt32();

                        // Write hash64
                        writer.Seek(nextHash64Offset, SeekOrigin.Begin);
                        writer.Write(Encoding.ASCII.GetBytes(hash64));
                        writer.Write(mappingId);
                        writer.Write(assembly.GetProperty("blob_idx").GetInt32());
                        writer.Write(assembly.GetProperty("store_id").GetInt32());

                        nextHash64Offset += 20;
                    }
                }
            }
        }

        return 0;
    }



    /* public static int UnpackStore(string[] args)
       {
           var parser = new CommandLine.Parser(with => with.HelpWriter = null);
           var result = parser.ParseArguments<UnpackOptions>(args);

           return result.MapResult(
               (UnpackOptions opts) => DoUnpack(opts.Directory, opts.Architecture, opts.Force),
               errs => 1);
       }

       public static int PackStore(string[] args)
       {
           var parser = new CommandLine.Parser(with => with.HelpWriter = null);
           var result = parser.ParseArguments<PackOptions>(args);

           return result.MapResult(
               (PackOptions opts) => DoPack(opts.ConfigJson),
               errs => 1);
       }
       */

    public static int GenHash(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Need to provide a string to hash!");
            return -1;
        }

        var fileName = args[0];
        var hashName = Path.GetFileNameWithoutExtension(fileName);

        Console.WriteLine($"Generating hashes for string '{fileName}' ({hashName})");
        var (hash32, hash64) = GenXxhash(hashName);

        Console.WriteLine($"Hash32: 0x{hash32}");
        Console.WriteLine($"Hash64: 0x{hash64}");

        return 0;
    }

  /*  public static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Mode is required!");
            Usage();
            return -1;
        }

        var mode = args[0];
        var modeArgs = args.Skip(1).ToArray();

        return mode switch
        {
            "unpack" => UnpackStore(modeArgs),
            "pack" => PackStore(modeArgs),
            "hash" => GenHash(modeArgs),
            _ => Usage()
        };
        
    }
        */
  
    public static void Debug(string message)
    {
        if (DEBUG)
        {
            Console.WriteLine($"[debug] {message}");
        }
    }

    public static bool DEBUG = false;
}

