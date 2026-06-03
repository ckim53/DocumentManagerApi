using Amazon.S3;
using Amazon.S3.Model;

namespace DocumentManagerApi.Services;

public class R2Service
{
    private readonly IAmazonS3? _s3;
    private readonly string? _bucket;
    private readonly string? _publicUrl;

    public R2Service(IConfiguration config, IAmazonS3 s3 = null)
    {
        _s3 = s3;
        _bucket = config["R2:Bucket"]!;
        _publicUrl = config["R2:PublicUrl"]!;
    }

    public async Task<(string fileUrl, string fileName)> UploadAsync(IFormFile file)
    {
        if (_s3 is null) throw new InvalidOperationException("R2 is not configured.");
        var extension = Path.GetExtension(file.FileName);
        var key = $"{Guid.NewGuid()}{extension}";

        using var stream = file.OpenReadStream();

        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = stream,
            ContentType = file.ContentType,
            DisablePayloadSigning = true // required for R2
        };

        await _s3.PutObjectAsync(request);

        return ($"{_publicUrl}/{key}", file.FileName);
    }

    public async Task DeleteAsync(string fileUrl)
    {
        if (_s3 is null) return;
        var key = fileUrl.Split('/').Last();

        var request = new DeleteObjectRequest
        {
            BucketName = _bucket,
            Key = key
        };

        await _s3.DeleteObjectAsync(request);
    }
}