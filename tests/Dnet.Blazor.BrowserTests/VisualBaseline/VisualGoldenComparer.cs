using System.Buffers.Binary;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

namespace Dnet.Blazor.BrowserTests.VisualBaseline;

/// <summary>
/// Golden storage and pixel comparison for the STY-002 visual baseline.
///
/// Playwright .NET 1.61 has no built-in snapshot comparison (toHaveScreenshot
/// only exists in the JS runner), so the comparison is implemented here in
/// plain C#: PNG decode + per-channel pixel diff.
///
/// Golden files are stored per platform because text rendering differs between
/// operating systems. The official goldens are frozen on the CI Linux/Chromium
/// runner; local development captures on macOS are a provisional baseline.
/// See VisualBaseline/README.md for the deliberate-update procedure.
/// </summary>
public static class VisualGoldenComparer
{
    /// <summary>Set to "true" to overwrite goldens instead of comparing.</summary>
    public const string UpdateEnvironmentVariable = "DNET_BLAZOR_UPDATE_GOLDENS";

    public static string Platform =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux"
        : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "darwin"
        : "win32";

    public static string GoldenPath(string testProjectRoot, string scenario, string state, string viewport)
        => Path.Combine(testProjectRoot, "VisualBaseline", "goldens", Platform, $"{scenario}-{state}-{viewport}.png");

    public static bool ShouldUpdate =>
        string.Equals(Environment.GetEnvironmentVariable(UpdateEnvironmentVariable), "true", StringComparison.OrdinalIgnoreCase);

    public static void Update(string path, byte[] png)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, png);
    }

    /// <param name="maxDiffPixels">Maximum number of pixels allowed to differ.</param>
    /// <param name="channelThreshold">Per-channel (R/G/B/A) difference that counts as a changed pixel.</param>
    public static GoldenComparisonResult Compare(byte[] actualPng, byte[] expectedPng, int maxDiffPixels = 0, int channelThreshold = 8)
    {
        var actual = PngImage.Decode(actualPng);
        var expected = PngImage.Decode(expectedPng);

        if (actual.Width != expected.Width || actual.Height != expected.Height)
        {
            return new GoldenComparisonResult(
                false,
                $"Screenshot size changed: actual {actual.Width}x{actual.Height}, golden {expected.Width}x{expected.Height}.",
                -1);
        }

        var diffCount = 0;
        var maxChannelDiff = 0;

        for (var i = 0; i < actual.Rgba.Length; i++)
        {
            var diff = Math.Abs(actual.Rgba[i] - expected.Rgba[i]);
            if (diff > maxChannelDiff)
            {
                maxChannelDiff = diff;
            }
        }

        for (var y = 0; y < actual.Height; y++)
        {
            for (var x = 0; x < actual.Width; x++)
            {
                var offset = (y * actual.Width + x) * 4;
                var changed = false;
                for (var channel = 0; channel < 4; channel++)
                {
                    if (Math.Abs(actual.Rgba[offset + channel] - expected.Rgba[offset + channel]) > channelThreshold)
                    {
                        changed = true;
                        break;
                    }
                }

                if (changed)
                {
                    diffCount++;
                }
            }
        }

        if (diffCount > maxDiffPixels)
        {
            var total = actual.Width * actual.Height;
            return new GoldenComparisonResult(
                false,
                $"Visual diff: {diffCount}/{total} pixels differ (threshold {channelThreshold}/channel, budget {maxDiffPixels}), " +
                $"max channel diff {maxChannelDiff}.",
                diffCount);
        }

        return new GoldenComparisonResult(true, string.Empty, diffCount);
    }

    public readonly record struct GoldenComparisonResult(bool Passed, string Message, int DiffPixels);
}

/// <summary>Minimal 8-bit PNG decoder sufficient for screenshots produced by Playwright (RGBA/RGB/gray).</summary>
internal sealed class PngImage
{
    private static readonly byte[] Signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public int Width { get; }

    public int Height { get; }

    public byte[] Rgba { get; }

    private PngImage(int width, int height, byte[] rgba)
    {
        Width = width;
        Height = height;
        Rgba = rgba;
    }

    public static PngImage Decode(byte[] data)
    {
        if (data.Length < Signature.Length || !data.AsSpan(0, Signature.Length).SequenceEqual(Signature))
        {
            throw new InvalidDataException("Not a PNG file.");
        }

        var width = 0;
        var height = 0;
        var bitDepth = 0;
        var colorType = 0;
        using var idat = new MemoryStream();

        var position = Signature.Length;
        while (position + 12 <= data.Length)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(position, 4));
            var type = Encoding.ASCII.GetString(data, position + 4, 4);

            if (type == "IHDR")
            {
                width = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(position + 8, 4));
                height = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(position + 12, 4));
                bitDepth = data[position + 16];
                colorType = data[position + 17];
                if (data[position + 20] != 0)
                {
                    throw new NotSupportedException("Interlaced PNG is not supported.");
                }
            }
            else if (type == "IDAT")
            {
                idat.Write(data, position + 8, length);
            }
            else if (type == "IEND")
            {
                break;
            }

            position += 12 + length;
        }

        if (bitDepth != 8)
        {
            throw new NotSupportedException($"Only 8-bit PNG is supported, got bit depth {bitDepth}.");
        }

        var channels = colorType switch
        {
            0 => 1, // grayscale
            2 => 3, // RGB
            4 => 2, // grayscale + alpha
            6 => 4, // RGBA
            _ => throw new NotSupportedException($"Unsupported PNG color type {colorType}."),
        };

        idat.Position = 0;
        using var inflater = new ZLibStream(idat, CompressionMode.Decompress);
        using var rawStream = new MemoryStream();
        inflater.CopyTo(rawStream);
        var raw = rawStream.ToArray();

        return DecodeScanlines(raw, width, height, channels);
    }

    private static PngImage DecodeScanlines(byte[] raw, int width, int height, int channels)
    {
        var stride = width * channels;
        var rgba = new byte[width * height * 4];
        var previous = new byte[stride];
        var expected = (stride + 1) * height;

        if (raw.Length < expected)
        {
            throw new InvalidDataException($"PNG scanlines truncated: expected {expected} bytes, got {raw.Length}.");
        }

        for (var y = 0; y < height; y++)
        {
            var filter = raw[y * (stride + 1)];
            var rowStart = y * (stride + 1) + 1;
            var row = new byte[stride];
            Array.Copy(raw, rowStart, row, 0, stride);

            ApplyFilter(filter, row, previous, channels, stride);

            for (var x = 0; x < width; x++)
            {
                var source = x * channels;
                var target = (y * width + x) * 4;

                switch (channels)
                {
                    case 1:
                        rgba[target] = row[source];
                        rgba[target + 1] = row[source];
                        rgba[target + 2] = row[source];
                        rgba[target + 3] = 255;
                        break;
                    case 2:
                        rgba[target] = row[source];
                        rgba[target + 1] = row[source];
                        rgba[target + 2] = row[source];
                        rgba[target + 3] = row[source + 1];
                        break;
                    case 3:
                        rgba[target] = row[source];
                        rgba[target + 1] = row[source + 1];
                        rgba[target + 2] = row[source + 2];
                        rgba[target + 3] = 255;
                        break;
                    default:
                        rgba[target] = row[source];
                        rgba[target + 1] = row[source + 1];
                        rgba[target + 2] = row[source + 2];
                        rgba[target + 3] = row[source + 3];
                        break;
                }
            }

            Array.Copy(row, previous, stride);
        }

        return new PngImage(width, height, rgba);
    }

    private static void ApplyFilter(byte filter, byte[] row, byte[] previous, int bpp, int stride)
    {
        switch (filter)
        {
            case 0: // None
                break;
            case 1: // Sub
                for (var i = bpp; i < stride; i++)
                {
                    row[i] = (byte)(row[i] + row[i - bpp]);
                }
                break;
            case 2: // Up
                for (var i = 0; i < stride; i++)
                {
                    row[i] = (byte)(row[i] + previous[i]);
                }
                break;
            case 3: // Average
                for (var i = 0; i < stride; i++)
                {
                    var left = i >= bpp ? row[i - bpp] : 0;
                    var up = previous[i];
                    row[i] = (byte)(row[i] + ((left + up) >> 1));
                }
                break;
            case 4: // Paeth
                for (var i = 0; i < stride; i++)
                {
                    var left = i >= bpp ? row[i - bpp] : 0;
                    var up = previous[i];
                    var upLeft = i >= bpp ? previous[i - bpp] : 0;
                    row[i] = (byte)(row[i] + PaethPredictor(left, up, upLeft));
                }
                break;
            default:
                throw new InvalidDataException($"Unknown PNG filter {filter}.");
        }
    }

    private static int PaethPredictor(int a, int b, int c)
    {
        var p = a + b - c;
        var pa = Math.Abs(p - a);
        var pb = Math.Abs(p - b);
        var pc = Math.Abs(p - c);

        return (pa <= pb && pa <= pc) ? a : (pb <= pc ? b : c);
    }
}
