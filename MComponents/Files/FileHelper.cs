using System.Globalization;

namespace MComponents.Files
{
    public static class FileHelper
    {
        public static string GetFormattedBytes(long pSize)
        {
            long absolute_i = (pSize < 0 ? -pSize : pSize);

            string suffix;
            double readable;

            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (pSize >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (pSize >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (pSize >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (pSize >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (pSize >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = pSize;
            }
            else
            {
                return pSize.ToString("0 B"); // Byte
            }

            readable /= 1024;

            return readable.ToString("0.## ", CultureInfo.InvariantCulture) + suffix;
        }

    }
}
