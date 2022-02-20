using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MComponents.Files
{
    public class ProgressableStreamContent : StreamContent
    {
        private const int BUFFER_SIZE = 1024 * 1024;

        private readonly Action<UploadProgress> mProgress;

        private readonly Stream mStream;

        public ProgressableStreamContent(Stream pStream, Action<UploadProgress> pProgress) : base(pStream, BUFFER_SIZE)
        {
            mStream = pStream;
            mProgress = pProgress;
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var buffer = new byte[BUFFER_SIZE];
            var size = mStream.Length;
            var uploaded = 0;

            using (mStream)
            {
                while (true)
                {
                    var length = await mStream.ReadAsync(buffer.AsMemory(0, buffer.Length));
                    if (length <= 0)
                    {
                        break;
                    }

                    uploaded += length;
                    mProgress.Invoke(new UploadProgress(uploaded, size));

                    await Task.Yield(); //We don't need this, but blazor
                    await stream.WriteAsync(buffer.AsMemory(0, length));
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                mStream.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    public class UploadProgress
    {
        public UploadProgress(long bytesTransfered, long? totalBytes)
        {
            BytesTransfered = bytesTransfered;
            TotalBytes = totalBytes;
            if (totalBytes.HasValue)
            {
                ProgressPercentage = (float)bytesTransfered / totalBytes.Value * 100;
            }
        }

        public long BytesTransfered { get; private set; }

        public float ProgressPercentage { get; private set; }

        public long? TotalBytes { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}% ({1} / {2})", ProgressPercentage, BytesTransfered, TotalBytes);
        }
    }
}
