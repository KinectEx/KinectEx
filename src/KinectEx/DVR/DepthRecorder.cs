using System;
using System.IO;
using System.Threading.Tasks;

namespace KinectEx.DVR
{
    /// <summary>
    /// Internal class that provides the services necessary to encode and store
    /// a <c>DepthFrame</c>.
    /// </summary>
    internal class DepthRecorder
    {
        private static byte[] _staticBytes = null;

        private readonly BinaryWriter _writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthRecorder"/> class.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public DepthRecorder(BinaryWriter writer)
        {
            this._writer = writer;
        }

        /// <summary>
        /// Records a <c>ReplayDepthFrame</c>.
        /// </summary>
        /// <param name="frame">The frame.</param>
        public async Task RecordAsync(ReplayDepthFrame frame)
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
                            dataWriter.Write(frame.DepthMinReliableDistance);
                            dataWriter.Write(frame.DepthMaxReliableDistance);

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
