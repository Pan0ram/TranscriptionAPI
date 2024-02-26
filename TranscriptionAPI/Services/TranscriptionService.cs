using FFmpeg.NET;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using Whisper.net;
using Whisper.net.Ggml;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using YoutubeExplode;

namespace TranscriptionAPI
{
    public class TranscriptionService : ITranscriptionService
    {
        public async Task<Transcription> GetTranscriptionFromYoutubeURL(string youtubeURL)
        {
            // Check and ensure that FFmpeg is present
            await EnsureFFmpegExists();

            // 1. Take Youtube URL and extract audio from Youtube using FFmpeg.NET
            var videoFilePath = await DownloadYouTubeVideo(youtubeURL);

            // 2. Convert video to audio
            var audioFilePath = await ConvertToAudio(videoFilePath);

            // 3. Use OpenAI's Whisper speech model to transcribe the audio (asynchronously)
            var model = await GetTranscriptionFromAudio(audioFilePath);

            // 4. Save transcription
            await SaveTranscriptionToFile(model, youtubeURL);

            // 5. Return transcription
            return model;
        }

        private static async Task<string> DownloadYouTubeVideo(string videoUrl, string outputDirectory = "Output")
        {
            // Ensure the output folder exists
            EnsureOutputFolderExists(outputDirectory);

            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(videoUrl);

            // Clean video title to remove invalid characters from the filename
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
                throw new Exception("Error during video conversion");
            }
        }

        private static async Task<string> ConvertToAudio(string videoFilePath)
        {
            // Set input and output file paths
            string outputFile = $"{videoFilePath.Substring(0, videoFilePath.Length - 1)}3";

            // Check if the input file exists
            if (!File.Exists(videoFilePath))
            {
                Console.WriteLine("Input file not found.");
                throw new Exception("Error during audio file generation.");
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
            // Declare three variables that we will use later: ggmlType, modelFileName, and mp3FileName
            var ggmlType = GgmlType.Base;
            var modelFileName = "ggml-base.bin";

            // Check if the input file exists
            if (!File.Exists(mp3FileName))
            {
                Console.WriteLine("Input file not found.");
                throw new Exception("Error during transcript generation.");
            }

            // Check if the file "ggml-base.bin" is present on our project's disk. If not, download it from the internet
            if (!File.Exists(modelFileName))
            {
                await DownloadModel(modelFileName, ggmlType);
            }

            // Create a WhisperFactory object used to create the Processor object
            using var whisperFactory = WhisperFactory.FromPath(modelFileName);

            // Create a Processor object used to process the audio file. It uses language auto to detect the language of the audio file.
            using var processor = whisperFactory.CreateBuilder()
                .WithLanguage("auto")
                .Build();

            // Open MP3 file and convert it to a WAV file with 16KHz Sample Rate
            using var fileStream = File.OpenRead(mp3FileName);

            using var wavStream = new MemoryStream();

            using var reader = new Mp3FileReader(fileStream);
            var resampler = new WdlResamplingSampleProvider(reader.ToSampleProvider(), 16000);
            WaveFileWriter.WriteWavFileToStream(wavStream, resampler.ToWaveProvider16());

            // Ensure wavStream is at the beginning of the stream (this is necessary because wavStream was described in the previous section)
            wavStream.Seek(0, SeekOrigin.Begin);

            // Process the audio file and display the results (start time, end time, and text) in the console
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
            Console.WriteLine($"Downloading model {fileName}");
            using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
            using var fileWriter = File.OpenWrite(fileName);
            await modelStream.CopyToAsync(fileWriter);
        }
         //  Check ob ffmpeg vorhanden ist
         // herunterladen, downloaden, extrahieren
         // Ordner löschen und Ordner Struktur herstellen
        private static async Task EnsureFFmpegExists()
        {
            string ffmpegDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg\\bin");
            string ffmpegPath = Path.Combine(ffmpegDirectory, "ffmpeg.exe");
            string ffmpegZipPath = Path.Combine(ffmpegDirectory, "ffmpeg.zip");

            
            if (!Directory.Exists(ffmpegDirectory))
            {
                Console.WriteLine($"Creating ffmpeg directory: {ffmpegDirectory}");
                Directory.CreateDirectory(ffmpegDirectory);
            }

             
            if (!File.Exists(ffmpegPath))
            {
                Console.WriteLine("ffmpeg.exe not found. Downloading...");

                // Download ffmpeg
                using (var httpClient = new HttpClient())
                {
                    var zipFileUrl = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip";
                    var zipFilePath = Path.Combine(ffmpegDirectory, "ffmpeg.zip");

                    using (var zipStream = await httpClient.GetStreamAsync(zipFileUrl))
                    {
                        using (var fileStream = File.Create(zipFilePath))
                        {
                            await zipStream.CopyToAsync(fileStream);
                        }
                    }

                    // Extrahieren
                    System.IO.Compression.ZipFile.ExtractToDirectory(zipFilePath, ffmpegDirectory);

                    
                    string destinationBinDirectory = Path.Combine(ffmpegDirectory, "");

                    
                    if (!Directory.Exists(destinationBinDirectory))
                    {
                        Console.WriteLine($"Creating destination directory: {destinationBinDirectory}");
                        Directory.CreateDirectory(destinationBinDirectory);
                    }

                 
                    MoveFFmpegBinContents(Path.Combine(ffmpegDirectory, "ffmpeg-master-latest-win64-gpl", "bin"), destinationBinDirectory);

                    Console.WriteLine("ffmpeg.exe successfully downloaded and extracted.");

                    // Delete the Zip file and the extracted folder
                    DeleteFFmpegFolderAndZip(ffmpegZipPath, Path.Combine(ffmpegDirectory, "ffmpeg-master-latest-win64-gpl"));
                }
            }
            else
            {
                Console.WriteLine("ffmpeg.exe found.");
            }
        }
       
        private static void DeleteFFmpegFolderAndZip(string zipFilePath, string extractedFolderPath)
        {
            // Löschen der Zip
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
                Console.WriteLine($"Zip file deleted: {zipFilePath}");
            }

            // löschen des extrahierten Ordners
            if (Directory.Exists(extractedFolderPath))
            {
                Directory.Delete(extractedFolderPath, true);
                Console.WriteLine($"Extracted folder deleted: {extractedFolderPath}");
            }
        }

        private static void EnsureOutputFolderExists(string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
            {
                Console.WriteLine($"Creating output folder: {outputDirectory}");
                Directory.CreateDirectory(outputDirectory);
            }
        }

        // Methode um Ordnerstruktur für ffmpeg zu korrigieren
        // Sollte eigentlich überflüssig sein, bisher jedoch keine besser Lösung finden können
        private static void MoveFFmpegBinContents(string sourceBinDirectory, string destinationBinDirectory)
        {
            
            if (!Directory.Exists(sourceBinDirectory))
            {
                Console.WriteLine($"The source directory {sourceBinDirectory} does not exist.");
                return;
            }

            
            if (!Directory.Exists(destinationBinDirectory))
            {
                Console.WriteLine($"Creating destination directory: {destinationBinDirectory}");
                Directory.CreateDirectory(destinationBinDirectory);
            }

            foreach (var sourceFile in Directory.GetFiles(sourceBinDirectory))
            {
                string destinationFile = Path.Combine(destinationBinDirectory, Path.GetFileName(sourceFile));

               
                if (File.Exists(destinationFile))
                {
                    Console.WriteLine($"File {destinationFile} already exists. Skipping.");
                    continue;
                }

              
                try
                {
                    File.Move(sourceFile, destinationFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error moving file from {sourceFile} to {destinationFile}: {ex.Message}");
                }
            }

            foreach (var sourceSubDirectory in Directory.GetDirectories(sourceBinDirectory))
            {
                string destinationSubDirectory = Path.Combine(destinationBinDirectory, Path.GetFileName(sourceSubDirectory));
                MoveFilesRecursively(sourceSubDirectory, destinationSubDirectory);
            }
        }

        // Methode zum verschieben der Datein
        private static void MoveFilesRecursively(string sourceDirectory, string destinationDirectory)
        {
            
            if (!Directory.Exists(sourceDirectory))
            {
                Console.WriteLine($"The source directory {sourceDirectory} does not exist.");
                return;
            }

            
            if (!Directory.Exists(destinationDirectory))
            {
                Console.WriteLine($"Creating destination directory: {destinationDirectory}");
                Directory.CreateDirectory(destinationDirectory);
            }

            foreach (var sourceFile in Directory.GetFiles(sourceDirectory))
            {
                string destinationFile = Path.Combine(destinationDirectory, Path.GetFileName(sourceFile));

                
                if (File.Exists(destinationFile))
                {
                    Console.WriteLine($"File {destinationFile} already exists. Skipping.");
                    continue;
                }

                
                try
                {
                    File.Move(sourceFile, destinationFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error moving file from {sourceFile} to {destinationFile}: {ex.Message}");
                }
            }

            foreach (var sourceSubDirectory in Directory.GetDirectories(sourceDirectory))
            {
                string destinationSubDirectory = Path.Combine(destinationDirectory, Path.GetFileName(sourceSubDirectory));
                MoveFilesRecursively(sourceSubDirectory, destinationSubDirectory);
            }
        }
        // Bennenung der Transcript.json -> Sollte auch anders gelöst werden
        // viel zu umständlich -> Lösung wird gesucht
        private static async Task SaveTranscriptionToFile(Transcription model, string youtubeURL)
        {
            
            var videoTitleWords = youtubeURL.Split('/').Last().Split('-').Take(2);
            var sanitizedTitle = string.Join("_", videoTitleWords);
            sanitizedTitle = string.Join("_", sanitizedTitle.Split(Path.GetInvalidFileNameChars()));

            
            var fileName = "transcript.json";

            var filePath = Path.Combine("Output", sanitizedTitle, fileName);

            var jsonContent = JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            await File.WriteAllTextAsync(filePath, jsonContent);

            Console.WriteLine($"Transkription erfolgreich in JSON-Datei gespeichert: {filePath}");
        }
    }
}
