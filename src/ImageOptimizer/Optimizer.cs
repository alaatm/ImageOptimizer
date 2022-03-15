using ImageMagick;
using TurboJpegWrapper;

namespace ImageOptimizer;

public static class Optimizer
{
    public static async Task<MemoryStream?> OptimizeAsync(Stream input, int maxWidth, int quality)
    {
        try
        {
            return Compress(await ConvertToJpeg(input, maxWidth), quality);
        }
        catch
        {
            return null;
        }
    }

    public static async Task<MemoryStream?> GenerateThumbnail(Stream input, int maxWidth)
    {
        try
        {
            return await ConvertToJpeg(input, maxWidth);
        }
        catch
        {
            return null;
        }
    }

    private static async Task<MemoryStream> ConvertToJpeg(Stream stream, int maxWidth)
    {
        using var image = new MagickImage(stream);

        if (image.Width > maxWidth)
        {
            image.Resize(maxWidth, 0);
        }

        var mem = new MemoryStream();
        await image.WriteAsync(mem, MagickFormat.Jpeg);
        mem.Seek(0, SeekOrigin.Begin);
        return mem;
    }

    private static unsafe MemoryStream Compress(MemoryStream stream, int quality)
    {
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
}