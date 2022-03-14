using ImageOptimizer.Web;
using System.Text;

var app = WebApplication.CreateBuilder(args).Build();
app.UseHttpsRedirection();

app.MapPost("/optimize", async context =>
{
    var resize = false;

    if (context.Request.Query.TryGetValue("resize", out var resizeQueryString))
    {
        if (resizeQueryString != "0" && resizeQueryString != "1")
        {
            context.Response.StatusCode = 400;
            await context.Response.Body.WriteAsync(GetTextBytes("Invalid resize query string value. Must be either 1 or 0."));
            return;
        }

        resize = resizeQueryString != "0";
    }

    if (context.Request.Form.Files.Count != 1)
    {
        context.Response.StatusCode = 400;
        await context.Response.Body.WriteAsync(GetTextBytes($"Must upload a single file. You uploaded {context.Request.Form.Files.Count}."));
        return;
    }

    var stopwatch = ValueStopwatch.StartNew();

    if (await Optimizer.OptimizeAsync(resize, context.Request.Form.Files[0].OpenReadStream()) is not Stream optimizedImage)
    {
        context.Response.StatusCode = 400;
        await context.Response.Body.WriteAsync(GetTextBytes("Could not read input image."));
        return;
    }

    context.Response.ContentType = "image/jpeg";
    context.Response.Headers.Add("X-ElapsedMS", $"{stopwatch.GetElapsedTime().TotalMilliseconds}");
    await optimizedImage.CopyToAsync(context.Response.Body);

    static Memory<byte> GetTextBytes(string text)
        => Encoding.UTF8.GetBytes(text).AsMemory();
});

app.Run();
