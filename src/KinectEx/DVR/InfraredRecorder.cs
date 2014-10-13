using System;
using System.IO;
using System.Threading.Tasks;

namespace KinectEx.DVR
{
    /// <summary>
    /// Internal class that provides the services necessary to encode and store
    /// a <c>InfraredFrame</c>.
    /// </summary>
    internal class InfraredRecorder
    {
        private static byte[] _staticBytes = null;

        readonly BinaryWriter _writer;

        public InfraredRecorder(BinaryWriter writer)
        {
            this._writer = writer;
        }

        public async Task RecordAsync(ReplayInfraredFrame frame)
        {
            await Task.Run(() =>
            {
                try
                {
                    // Header
                    _writer.Write((int)frame.FrameType);
                    _writer.Write(frame.RelativeTime.TotalMilliseconds);

                    // Data
                    using (var dataStream = new MemoryStream())
                    {
                        using (var dataWriter = new BinaryWriter(dataStream))
                        {
                            dataWriter.Write(frame.Width);
                            dataWriter.Write(frame.Height);
                            dataWriter.Write(frame.BytesPerPixel);

                            if (_staticBytes == null)
                                _staticBytes = new byte[frame.FrameData.Length * 2];

                            System.Buffer.BlockCopy(frame.FrameData, 0, _staticBytes, 0, _staticBytes.Length);

                            dataWriter.Write(_staticBytes);

                            // Reset frame data stream
                            dataWriter.Flush();
                            dataStream.Position = 0;

                            // Write FrameSize
                            _writer.Write(dataStream.Length);

                            // Write actual frame data
                            dataStream.CopyTo(_writer.BaseStream);

                            // Write end of frame marker
                            _writer.Write(ReplayFrame.EndOfFrameMarker);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // TODO: Change to log the error
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            });
        }
    }
}
