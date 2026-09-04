using Amazon.S3;
using Amazon.S3.Model;
using SharedThings.Api.Interfaces;

namespace SharedThings.Api.Storage;

public sealed class S3ItemImageStorage(
    IAmazonS3 s3,
    string bucketName)
    : IItemImageStorage
{
    public async Task UploadAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
        };

        await s3.PutObjectAsync(request, cancellationToken);
    }

    public async Task DeleteAsync(
        string key,
        CancellationToken cancellationToken)
    {
        await s3.DeleteObjectAsync(
            bucketName,
            key,
            cancellationToken);
    }
}
