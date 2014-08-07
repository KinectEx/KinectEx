using System.IO;
using System.Threading.Tasks;

#if NETFX_CORE
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows.Media.Imaging;
#endif

namespace KinectEx.DVR
{
    public interface IColorCodec
    {
        int CodecId { get; }
        int Width { get; set; }
        int Height { get; set; }
        int OutputWidth { get; set; }
        int OutputHeight { get; set; }
#if !NOSDK
        Task EncodeAsync(byte[] bytes, BinaryWriter writer);
#endif
        void ReadHeader(BinaryReader reader, ReplayFrame frame);
        Task<BitmapSource> DecodeAsync(byte[] bytes);
    }
}
