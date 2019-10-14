using System;
using System.IO;

namespace GZipTest.Helpers
{
    public static class IOHelper
    {
        public static FileInfo GetFileFromStream(Func<Stream> getStreamFunc)
        {
            FileInfo file = null;
            using (Stream stream = getStreamFunc())
            {
                if (stream is FileStream fs) file = new FileInfo(fs.Name);
            }          
            return file;
        }
    }
}
