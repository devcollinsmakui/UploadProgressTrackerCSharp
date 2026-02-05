public class UploadProgressTracker
{
    /// <summary>
    /// Tracks upload progress.
    /// </summary>
    /// <param name="stream">takes a file stream</param>
    /// <returns>an interger</returns>
    public static void TrackUploadProgress(Stream stream)
    {
        int previousPosition = -1;
        while (previousPosition < 100)
        {
            Thread.Sleep(100);
            int position = (int)Math.Round(100 * (stream.Position/(double)stream.Length));
            if(position != previousPosition)
            {
                previousPosition = position;
                Console.WriteLine(previousPosition);
            }

        }
    }
}
