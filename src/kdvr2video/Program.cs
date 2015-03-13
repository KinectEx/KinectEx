using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectEx.DVR;
using System.IO;
using System.Diagnostics;

namespace KinectEx.kdvr2video
{
    class Program
    {
        static void Main(string[] args)
        {
            TranscodeVideo(args[0], args[1]);
        }

        private static async void TranscodeVideo(string kdvrFile, string videoFile)
        {
            string tempPath = Path.GetTempPath() + Guid.NewGuid();
            try
            {
                Directory.CreateDirectory(tempPath);

                using (Stream kdvrStream = File.Open(kdvrFile, FileMode.Open, FileAccess.Read))
                {
                    if (kdvrStream == null)
                    {
                        Console.Error.WriteLine("Error opening file {0}", kdvrFile);
                        return;
                    }

                    using (KinectReplay replay = new KinectReplay(kdvrStream))
                    {
                        if (replay == null || !replay.HasColorFrames)
                        {
                            Console.Error.WriteLine("Error with KDVR file (does it have color frames?)");
                            return;
                        }


                        Console.WriteLine("Converting {0} to {1}", kdvrFile, videoFile);

                        Console.WriteLine("EXPORTING JPEG IMAGES FROM {0}...", kdvrFile);
                        await replay.ExportColorFramesAsync(tempPath);

                        Console.WriteLine("ENCODING {0}...", videoFile);
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.UseShellExecute = false;
                        startInfo.FileName = "ffmpeg.exe";
                        startInfo.Arguments = "-framerate 30 -i \"" + tempPath + "\\%06d.jpeg\" \"" + videoFile + "\"";
                        using (Process exeProcess = Process.Start(startInfo))
                        {
                            exeProcess.WaitForExit();
                        }
                        Console.WriteLine("SUCCESS!!!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            finally
            {
                Directory.Delete(tempPath, true);
            }

        }
    }
}
