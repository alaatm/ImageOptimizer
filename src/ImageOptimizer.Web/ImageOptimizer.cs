using ImageMagick;
using TurboJpegWrapper;

namespace ImageOptimizer.Web;

public static class Optimizer
{
    public static async Task<MemoryStream?> OptimizeAsync(bool resize, Stream input)
        => await ConvertToJpeg(resize, input) is MemoryStream mem
            ? Compress(mem)
            : null;

    private static async Task<MemoryStream?> ConvertToJpeg(bool resize, Stream stream)
    {
        try
        {
            using var image = new MagickImage(stream);

            if (resize && image.Width > 960)
            {
                image.Resize(960, 0);
            }

            using var mem = new MemoryStream();
            await image.WriteAsync(mem, MagickFormat.Jpeg);
            mem.Seek(0, SeekOrigin.Begin);
            return mem;
        }
        catch
        {
            return null;
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