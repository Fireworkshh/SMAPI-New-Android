using SMAPI_Installation;

public class VerifyGame
{
    public static string CheckSMAPIInstallation()
    {
        string privateStoragePath = MainActivity.GetPrivateStoragePath();

        string[] directoriesToCheck = new string[]
        {
            Path.Combine(privateStoragePath, "Content"),
            Path.Combine(privateStoragePath, "Content/Animals"),
            Path.Combine(privateStoragePath, "Content/Buildings"),
            Path.Combine(privateStoragePath, "Content/Characters"),
            Path.Combine(privateStoragePath, "Content/Data"),
            Path.Combine(privateStoragePath, "Content/Effects"),
            Path.Combine(privateStoragePath, "dotnet"),
            Path.Combine(privateStoragePath, "smapi-internal")
        };

        // 检查必要的目录是否存在
        foreach (string directory in directoriesToCheck)
        {
            if (!Directory.Exists(directory))
            {
                return $"缺少目录: {directory}，请安装游戏.";
            }
        }

        // 如果所有目录都存在
        return null;
    }
}
