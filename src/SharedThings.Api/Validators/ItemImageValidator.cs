namespace SharedThings.Api.Validators;

public static class ItemImageValidator
{
    public const long MaximumFileSizeBytes =
        5 * 1024 * 1024;

    private static readonly byte[] PngSignature =
    [
        0x89,
        0x50,
        0x4E,
        0x47,
        0x0D,
        0x0A,
        0x1A,
        0x0A,
    ];

    public static async Task<ItemImageValidationResult> ValidateAsync(
        IFormFile image,
        CancellationToken cancellationToken)
    {
        if (image.Length == 0)
        {
            return ItemImageValidationResult.Failure(
                "Select a non-empty image.");
        }

        if (image.Length > MaximumFileSizeBytes)
        {
            return ItemImageValidationResult.Failure(
                "The image must be no larger than 5 MB.");
        }

        var declaredContentType = NormaliseContentType(
            image.ContentType);

        if (!IsSupportedContentType(declaredContentType))
        {
            return ItemImageValidationResult.Failure(
                "The image must be a JPEG, PNG or WebP file.");
        }

        var header = new byte[12];

        await using var stream = image.OpenReadStream();

        var bytesRead = await ReadHeaderAsync(
            stream,
            header,
            cancellationToken);

        var detectedImage = DetectImageFormat(
            header,
            bytesRead);

        if (detectedImage is null)
        {
            return ItemImageValidationResult.Failure(
                "The uploaded file is not a valid JPEG, PNG or WebP image.");
        }

        if (!string.Equals(
                detectedImage.ContentType,
                declaredContentType,
                StringComparison.OrdinalIgnoreCase))
        {
            return ItemImageValidationResult.Failure(
                "The image content does not match its declared file type.");
        }

        return ItemImageValidationResult.Success(
            detectedImage);
    }

    private static string NormaliseContentType(
        string contentType)
    {
        return contentType
            .Split(';', 2)[0]
            .Trim()
            .ToLowerInvariant();
    }

    private static bool IsSupportedContentType(
        string contentType)
    {
        return contentType is
            "image/jpeg" or
            "image/png" or
            "image/webp";
    }

    private static async Task<int> ReadHeaderAsync(
        Stream stream,
        byte[] buffer,
        CancellationToken cancellationToken)
    {
        var totalBytesRead = 0;

        while (totalBytesRead < buffer.Length)
        {
            var bytesRead = await stream.ReadAsync(
                buffer.AsMemory(
                    totalBytesRead,
                    buffer.Length - totalBytesRead),
                cancellationToken);

            if (bytesRead == 0)
            {
                break;
            }

            totalBytesRead += bytesRead;
        }

        return totalBytesRead;
    }

    private static ValidatedItemImage? DetectImageFormat(
        byte[] header,
        int bytesRead)
    {
        if (
            bytesRead >= PngSignature.Length &&
            header
                .AsSpan(0, PngSignature.Length)
                .SequenceEqual(PngSignature)
        )
        {
            return new ValidatedItemImage(
                ContentType: "image/png",
                FileExtension: "png");
        }

        if (
            bytesRead >= 3 &&
            header[0] == 0xFF &&
            header[1] == 0xD8 &&
            header[2] == 0xFF
        )
        {
            return new ValidatedItemImage(
                ContentType: "image/jpeg",
                FileExtension: "jpg");
        }

        if (
            bytesRead >= 12 &&
            HeaderContainsAscii(header, 0, "RIFF") &&
            HeaderContainsAscii(header, 8, "WEBP")
        )
        {
            return new ValidatedItemImage(
                ContentType: "image/webp",
                FileExtension: "webp");
        }

        return null;
    }

    private static bool HeaderContainsAscii(
        byte[] header,
        int offset,
        string expected)
    {
        if (header.Length < offset + expected.Length)
        {
            return false;
        }

        for (var index = 0;
             index < expected.Length;
             index++)
        {
            if (header[offset + index] != expected[index])
            {
                return false;
            }
        }

        return true;
    }
}

public sealed record ValidatedItemImage(
    string ContentType,
    string FileExtension);

public sealed record ItemImageValidationResult(
    ValidatedItemImage? Image,
    string? Error)
{
    public bool IsValid =>
        Image is not null && Error is null;

    public static ItemImageValidationResult Success(
        ValidatedItemImage image)
    {
        return new ItemImageValidationResult(
            image,
            Error: null);
    }

    public static ItemImageValidationResult Failure(
        string error)
    {
        return new ItemImageValidationResult(
            Image: null,
            error);
    }
}