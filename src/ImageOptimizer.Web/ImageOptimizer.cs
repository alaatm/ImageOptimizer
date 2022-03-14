extern alias MagickNET8;
extern alias MagickNET16;
extern alias MagickNET32;

using MagickImage8 = MagickNET8.ImageMagick.MagickImage;
using MagickImage16 = MagickNET16.ImageMagick.MagickImage;
using MagickImage32 = MagickNET32.ImageMagick.MagickImage;

using ImageMagick;
using TurboJpegWrapper;

namespace ImageOptimizer.Web;

public static class Optimizer
{
    public static async Task<MemoryStream?> OptimizeAsync(int bpp, bool resize, Stream input)
        => await ConvertToJpeg(bpp, resize, input) is MemoryStream mem
            ? Compress(mem)
            : null;

    private static async Task<MemoryStream?> ConvertToJpeg(int bpp, bool resize, Stream stream)
    {
        try
        {
            return bpp switch
            {
                8 => await ConvertToJpeg8(resize, stream),
                16 => await ConvertToJpeg16(resize, stream),
                32 => await ConvertToJpeg32(resize, stream),
                _ => null,
            };
        }
        catch
        {
            return null;
        }

        static async Task<MemoryStream> ConvertToJpeg8(bool resize, Stream stream)
        {
            using var image = new MagickImage8(stream);

            if (resize && image.Width > 960)
            {
                image.Resize(960, 0);
            }

            using var mem = new MemoryStream();
            await image.WriteAsync(mem, MagickFormat.Jpeg);
            mem.Seek(0, SeekOrigin.Begin);
            return mem;
        }

        static async Task<MemoryStream> ConvertToJpeg16(bool resize, Stream stream)
        {
            using var image = new MagickImage16(stream);

            if (resize && image.Width > 960)
            {
                image.Resize(960, 0);
            }

            using var mem = new MemoryStream();
            await image.WriteAsync(mem, MagickFormat.Jpeg);
            mem.Seek(0, SeekOrigin.Begin);
            return mem;
        }

        static async Task<MemoryStream> ConvertToJpeg32(bool resize, Stream stream)
        {
            using var image = new MagickImage32(stream);

            if (resize && image.Width > 960)
            {
                image.Resize(960, 0);
            }

            using var mem = new MemoryStream();
            await image.WriteAsync(mem, MagickFormat.Jpeg);
            mem.Seek(0, SeekOrigin.Begin);
            return mem;
        }
    }

    private static unsafe MemoryStream? Compress(MemoryStream stream)
    {
        try
        {
            var quality = 70;
            var flags = TJFlags.None;
            var pixelFormat = TJPixelFormat.RGB;
            var subsampling = TJSubsamplingOption.Chrominance420;

            using var decr = new TJDecompressor();
            var raw = decr.Decompress(stream.GetBuffer(), pixelFormat, flags, out var width, out var height, out var stride);

            fixed (byte* pRaw = raw)
            {
                using var compr = new TJCompressor();
                var compressed = compr.Compress(new IntPtr(pRaw), stride, width, height, pixelFormat, subsampling, quality, flags);
                return new MemoryStream(compressed);
            }
        }
        catch
        {
            return null;
        }
    }
}