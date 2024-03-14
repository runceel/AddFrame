using SkiaSharp;

if (args.Length != 2)
{
    PrintArgsError();
    return;
}

var input = args[0];
var output = args[1];

if (string.IsNullOrWhiteSpace(input) ||  string.IsNullOrWhiteSpace(output))
{
    PrintArgsError();
    return;
}

if (!Directory.Exists(input))
{
    Console.WriteLine("入力元のフォルダーが存在しません。");
    return;
}

var images = Directory.GetFiles(input, "*.*", new EnumerationOptions { RecurseSubdirectories = true });

var line = new SKPaint
{
    Color = SKColors.Black,
};
foreach (var image in images.Where(IsSupportedFileType))
{
    var relativePath = GetRelativePath(input, image);
    using var sourceImage = SKBitmap.Decode(image);
    using var distImage = new SKBitmap(sourceImage.Width + 2, sourceImage.Height + 2);
    using var canvas = new SKCanvas(distImage);
    canvas.DrawRect(0, 0, distImage.Width, distImage.Height, line);
    canvas.DrawBitmap(sourceImage, 1.0f, 1.0f);

    var fullOutputPath = Path.GetFullPath(output);
    var outputFilePath = Path.Combine(fullOutputPath, relativePath);
    var outputDir = Path.GetDirectoryName(outputFilePath);
    if (!Directory.Exists(outputDir))
    {
        Directory.CreateDirectory(outputDir);
    }
    var encodedData = Encode(outputFilePath, distImage);
    File.WriteAllBytes(outputFilePath, encodedData.ToArray());
}

static SKData Encode(string outputFilePath, SKBitmap bitmap)
{
    var encode = Path.GetExtension(outputFilePath).ToLowerInvariant() switch
    {
        ".png" => SKEncodedImageFormat.Png,
        ".jpg" => SKEncodedImageFormat.Jpeg,
        _ => throw new InvalidOperationException(),
    };

    return bitmap.Encode(encode, 100);
}

static void PrintArgsError() => Console.WriteLine("第一引数に入力元のフォルダーへのパス、第二引数に出力先のフォルダーへのパスを指定してください。");
static bool IsSupportedFileType(string path) => 
    Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".png" => true,
        ".jpg" => true,
        _ => false,
    };

static string GetRelativePath(string basePath, string fullPath)
{
    if (!fullPath.StartsWith(basePath))
    {
        throw new InvalidOperationException();
    }

    var relativePath = fullPath[basePath.Length..];
    if (relativePath[0] == Path.DirectorySeparatorChar)
    {
        relativePath = relativePath[1..];
    }

    return relativePath;
}