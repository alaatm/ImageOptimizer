using ImageOptimizer;

var app = WebApplication.CreateBuilder(args).Build();
app.UseHttpsRedirection();

app.MapPost("/optimize", async context =>
{
    if (!context.IsValidKey())
    {
        await context.SetErrorAsync(401, "Missing or invalid key.");
        return;
    }

    if (!context.TryGetQueryValue("maxWidth", 960, out var maxWidth))
    {
        await context.SetErrorAsync(400, "Invalid maxWidth value. Must be an integer greater than zero.");
        return;
    }

    if (!context.TryGetQueryValue("quality", 70, out var quality) || quality is < 1 or > 100)
    {
        await context.SetErrorAsync(400, "Invalid quality value. Must be an integer between 1 and 100.");
        return;
    }

    using var ms = await context.GetBodyStreamAsync();
    using var output = await Optimizer.OptimizeAsync(ms, maxWidth, quality);

    if (output is not Stream optimizedImage)
    {
        await context.SetErrorAsync(400, "Could not read input image.");
        return;
    }

    context.Response.ContentType = "image/jpeg";
    await optimizedImage.CopyToAsync(context.Response.Body);
});

app.MapPost("/thumbnail", async context =>
{
    if (!context.IsValidKey())
    {
        await context.SetErrorAsync(401, "Missing or invalid key.");
        return;
    }

    if (!context.TryGetQueryValue("maxWidth", 150, out var maxWidth) || maxWidth is not (75 or 150 or 200))
    {
        await context.SetErrorAsync(400, "Invalid maxWidth value. Must be 75, 150 or 200.");
        return;
    }

    using var ms = await context.GetBodyStreamAsync();
    using var output = await Optimizer.GenerateThumbnail(ms, maxWidth);

    if (output is not Stream thumbImage)
    {
        await context.SetErrorAsync(400, "Could not read input image.");
        return;
    }

    context.Response.ContentType = "image/jpeg";
    await thumbImage.CopyToAsync(context.Response.Body);
});

await app.RunAsync();
