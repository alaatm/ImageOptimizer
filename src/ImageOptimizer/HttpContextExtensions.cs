using System.Text;

namespace ImageOptimizer;

internal static class HttpContextExtensions
{
    public static bool TryGetQueryValue(this HttpContext context, string name, int defaultValue, out int value)
    {
        value = defaultValue;
        if (context.Request.Query.TryGetValue(name, out var strValue))
        {
            if (!int.TryParse(strValue, out var intValue))
            {
                return false;
            }

            value = intValue;
        }

        return true;
    }

    public static bool IsValidKey(this HttpContext context)
    {
        var config = context.RequestServices.GetRequiredService<IConfiguration>();
        if (!context.Request.Query.TryGetValue("key", out var key) || key != config["key"])
        {
            return false;
        }

        return true;
    }

    public static async Task SetErrorAsync(this HttpContext context, int status, string response)
    {
        context.Response.StatusCode = status;
        await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(response).AsMemory());
    }

    public static async Task<Stream> GetBodyStreamAsync(this HttpContext context)
    {
        var ms = new MemoryStream((int)context.Request.ContentLength!);
        await context.Request.Body.CopyToAsync(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }
}