using System;
using System.IO;
using System.Threading.Tasks;

namespace KinectEx.DVR
{
    internal class DepthRecorder
    {
        readonly BinaryWriter _writer;

        public DepthRecorder(BinaryWriter writer)
        {
            this._writer = writer;
        }

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

                            for (int i = 0; i < frame.FrameData.Length; i++)
                            {
                                dataWriter.Write(frame.FrameData[i]);
                            }

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
