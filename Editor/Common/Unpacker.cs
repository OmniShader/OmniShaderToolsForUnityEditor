using System;
using System.IO;
using System.Text;
using OmniShader.Common.UI;
using UnityEngine;

namespace OmniShader.Common
{
    public static class Unpacker
    {
        public const byte SEED = 131;
        
        public static string GetDataFolder()
        {
            var dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(dataFolder, "OmniShaderTools");
        }

        public static string GetOmniShaderFolder()
        {
            return Path.Combine(GetDataFolder(), ".omnishader");
        }

        public static bool UnpackInNeeds(int version)
        {
            var stylePath = UIExtensions.GetAppStylePath();
            var packageFile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(stylePath), "..", "Data", "os.dat"));
            if (!File.Exists(packageFile))
            {
                return false;
            }

            var targetFolder = GetOmniShaderFolder();
            var vesrionFile = Path.Combine(targetFolder, "VERSION");
            if (File.Exists(vesrionFile))
            {
                var versionFileText = File.ReadAllText(vesrionFile);
                if (int.TryParse(versionFileText, out int localVersion))
                {
                    if (localVersion >= version)
                    {
                        return false;
                    }
                }
            }

            if (Directory.Exists(targetFolder))
            {
                Directory.Delete(targetFolder, true);
            }

            Directory.CreateDirectory(targetFolder);
            File.WriteAllText(vesrionFile, version.ToString());

            var bytes = File.ReadAllBytes(packageFile);
            bytes = UnPackBytes(bytes, SEED);
            int index = 0;

            while (index < bytes.Length)
            {
                var relativePathLength = BitConverter.ToInt32(bytes.GetRange(index, 4));
                index += 4;

                var relativePathBytes = bytes.GetRange(index, relativePathLength);
                var relativePath = Encoding.UTF8.GetString(relativePathBytes);
                index += relativePathLength;

                var contentLength = BitConverter.ToInt32(bytes.GetRange(index, 4));
                index += 4;
                var contentBytes = bytes.GetRange(index, contentLength);
                index += contentLength;

                var saveFile = Path.Combine(targetFolder, relativePath);
                var saveFolder = Path.GetDirectoryName(saveFile);
                Directory.CreateDirectory(saveFolder);
                File.WriteAllBytes(saveFile, contentBytes);
            }

            return true;
        }

        private static byte[] GetRange(this byte[] bytes, int start, int length)
        {
            byte[] results = new byte[length];
            Buffer.BlockCopy(bytes, start, results, 0, length);
            return results;
        }

        private static byte[] UnPackBytes(byte[] bytes, byte flag)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] ^= flag;
            }

            return bytes;
        }
    }
}