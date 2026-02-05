using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

public class ProgressStreamContent : StreamContent
{
    private const int DefaultBufferSize = 4096;
    private readonly Stream _stream;
    private readonly int _bufferSize;
    private readonly Action<long, long> _progressCallback;

    public ProgressStreamContent(Stream stream, Action<long, long> progressCallback)
        : this(stream, DefaultBufferSize, progressCallback) { }

    public ProgressStreamContent(Stream stream, int bufferSize, Action<long, long> progressCallback)
        : base(stream)
    {
        _stream = stream;
        _bufferSize = bufferSize;
        _progressCallback = progressCallback;
    }

    protected override async Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext context)
    {
        var buffer = new byte[_bufferSize];
        long uploaded = 0;
        long total = _stream.Length;

        using (_stream) // Optional: dispose the source stream after use
        {
            int read;
            while ((read = await _stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await stream.WriteAsync(buffer, 0, read);
                uploaded += read;
                _progressCallback?.Invoke(uploaded, total);
            }
        }
    }

    public async Task UploadFileAsync(string filePath, string requestUri)
    {
        using (var httpClient = new HttpClient())
        using (var fileStream = File.OpenRead(filePath))
        {
            // Define the progress callback function
            Action<long, long> progress = (uploaded, total) =>
            {
                // Calculate progress percentage (0 to 100)
                int percent = (int)(((double)uploaded / total) * 100);
                Console.WriteLine($"Upload Progress: {percent}% ({uploaded}/{total} bytes)");
                // In a UI app, you would update a ProgressBar control here
            };

            var content = new ProgressStreamContent(fileStream, progress);
            content.Headers.Add("Content-Type", "application/octet-stream"); // Or other appropriate MIME type

            HttpResponseMessage response = await httpClient.PostAsync(requestUri, content);
            response.EnsureSuccessStatusCode();

            Console.WriteLine("File uploaded successfully.");
        }
    }
}


/////////
