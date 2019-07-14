using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.Helper
{
    abstract class FileHelper
    {
        public static String GetFileLocation(String path)
        {
            path = path.Substring(0, path.LastIndexOf('\\') + 1);
            return path;
        }

        public static String GetFileName(String path)
        {
            int pos = path.LastIndexOf('\\');
            if (pos != -1)
                path = path.Substring(path.LastIndexOf('\\') + 1);
            return path;
        }
    }
}
