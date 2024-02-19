using FFmpeg.NET;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using Whisper.net;
using Whisper.net.Ggml;
using System.Diagnostics;
using YoutubeExplode;

namespace TranscriptionAPI
{
    public class TranscriptionService : ITranscriptionService
    {
        public async Task<Transcription> GetTranscriptionFromYoutubeURL(string youtubeURL)
        {
            // 1.Take YoutubeURL and extract audio from Youtube with ffmpg.net
            var videoFilePath = await DownloadYouTubeVideo(youtubeURL);

            // 2.Convert Video to Audio
            var audioFilePath = await ConvertToAudio(videoFilePath);

            // 2. Use OpenAI's Whisper language model to transcript the video (async)
            var model = await GetTranscriptionFromAudio(audioFilePath);

            // 3. Return Transcription
            return model;
        }

        private static async Task<string> DownloadYouTubeVideo(string videoUrl, string outputDirectory = "Output")
        {
            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(videoUrl);

            // Sanitize the video title to remove invalid characters from the file name
            string sanitizedTitle = string.Join("_", video.Title.Split(Path.GetInvalidFileNameChars()));

            // Get all available muxed streams
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
            var muxedStreams = streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality).ToList();

            if (muxedStreams.Any())
            {
                var streamInfo = muxedStreams.First();
                using var httpClient = new HttpClient();
                var stream = await httpClient.GetStreamAsync(streamInfo.Url);
                var datetime = DateTime.Now;

                string outputFilePath = Path.Combine(outputDirectory, $"{sanitizedTitle}.{streamInfo.Container}");
                using var outputStream = File.Create(outputFilePath);
                await stream.CopyToAsync(outputStream);

                Console.WriteLine("Download completed!");
                Console.WriteLine($"Video saved as: {outputFilePath}{datetime}");

                return outputFilePath;
            }
            else
            {
                Console.WriteLine($"No suitable video stream found for {video.Title}.");
                throw new Exception("Fehler bei der Videoumwandlung");
            }
        }

        private async Task<string> ConvertToAudio(string videoFilePath)
        {
            // Specify the input and output file paths
            string outputFile = $"{videoFilePath.Substring(0, videoFilePath.Length - 1)}3";

            // Check if the input file exists
            if (!File.Exists(videoFilePath))
            {
                Console.WriteLine("Input file not found.");
                throw new Exception("Fehler bei der Audiodateigenerierung.");
            }

            // Configure FFmpeg.NET
            var ffmpeg = new Engine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg\\bin\\ffmpeg.exe"));

            var conversionOptions = new ConversionOptions();

            var ffmpegIn = new FFmpeg.NET.InputFile(videoFilePath);
            var ffmpegOut = new OutputFile(outputFile);
            await ffmpeg.ConvertAsync(ffmpegIn, ffmpegOut, default);

            return outputFile;
        }

        private async Task<Transcription> GetTranscriptionFromAudio(string mp3FileName)
        {
            // We declare three variables which we will use later, ggmlType, modelFileName and mp3FileName
            var ggmlType = GgmlType.Base;
            var modelFileName = "ggml-base.bin";

            // Check if the input file exists
            if (!File.Exists(mp3FileName))
            {
                Console.WriteLine("Input file not found.");
                throw new Exception("Fehler bei der Transkriptgenerierung.");
            }

            // This section detects whether the "ggml-base.bin" file exists in our project disk. If it doesn't, it downloads it from the internet
            if (!File.Exists(modelFileName))
            {
                await DownloadModel(modelFileName, ggmlType);
            }

            // This section creates the whisperFactory object which is used to create the processor object.
            using var whisperFactory = WhisperFactory.FromPath("ggml-base.bin");

            // This section creates the processor object which is used to process the audio file, it uses language auto to detect the language of the audio file.
            using var processor = whisperFactory.CreateBuilder()
                .WithLanguage("auto")
                .Build();

            // This section opens the mp3 file and converts it to a wav file with 16Khz sample rate.
            using var fileStream = File.OpenRead(mp3FileName);

            using var wavStream = new MemoryStream();

            using var reader = new Mp3FileReader(fileStream);
            var resampler = new WdlResamplingSampleProvider(reader.ToSampleProvider(), 16000);
            WaveFileWriter.WriteWavFileToStream(wavStream, resampler.ToWaveProvider16());

            // This section sets the wavStream to the beginning of the stream. (This is required because the wavStream was written to in the previous section)
            wavStream.Seek(0, SeekOrigin.Begin);

            // This section processes the audio file and prints the results (start time, end time and text) to the console.
            var model = new Transcription();
            await foreach (var result in processor.ProcessAsync(wavStream))
            {
                Debug.WriteLine($"{result.Start}->{result.End}: {result.Text}");
                model.TranscriptionLines.Add(new TranscriptionData()
                {
                    StartSeconds = result.Start,
                    EndSeconds = result.End,
                    Transcript = result.Text
                });
            }

            return model;
        }

        private static async Task DownloadModel(string fileName, GgmlType ggmlType)
        {
            Console.WriteLine($"Downloading Model {fileName}");
            using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
            using var fileWriter = File.OpenWrite(fileName);
            await modelStream.CopyToAsync(fileWriter);
        }     
    }
}