using FFmpeg.NET;
using FFmpeg.NET.Enums;
using System;
using System.IO;
using Whisper;

namespace TranscriptionAPI
{
    public class TranscriptionService : ITranscriptionService
    {
    public Transcription GetTranscriptionFromYoutubeURL(string youtubeURL)
     {
            //1. Take YoutubeURL and extract audio from Youtube with ffmpg.net
            string audioFileName = "audio.mp3";                                                                                 // Pfad zum heruntergeladenen Audio relativ zum übergeordneten Verzeichnis
            string outputPath = Path.Combine(Directory.GetParent(youtubeURL).FullName, audioFileName);                            
            var ffmpeg = new Engine("ffmpeg\\bin\\ffmpeg.exe");                                                                 // Konfigurieren FFmpeg.NET
            var inputPath = youtubeURL;                                                                                         // Hier sollte der YouTube-URL stehen
            var outputFileInfo = new FileInfo(outputPath);                                                                      // Erstellen Sie den Eingabe- und Ausgabepfad
            var conversion = ffmpeg.Convert(inputPath, outputFileInfo);                                                         // Extrahiert die Audio Datei
            conversion.AddStreamSpecifier(new StreamSpecifier(StreamType.Audio));
            conversion.SetOutput(outputFileInfo)
                conversion.Start();                                                                                             // Starten Sie die Konvertierung
                conversion.Wait();                                                                                              // Warten Sie auf den Abschluss

            //2. Use OpenAI's Whisper language model to transcript the video (async)

            //2.5 Optional Translate

            //3. Return Transcription

            Transcription model = new Transcription()
            {
                Date = DateTime.Now,
                TranscriptionLines = new List<TranscriptionData>
                {
                   new TranscriptionData() { StartSeconds = 0, EndSeconds = 2, Transcript = "Lorem ipsum dolor sit amet, consetetur sad" },
                   new TranscriptionData() { StartSeconds = 2, EndSeconds = 5, Transcript = "cusam et justo duo dolores et ea rebum. Stet clita kasd" },
                   new TranscriptionData() { StartSeconds = 5, EndSeconds = 2, Transcript = "dolor sit amet. Lorem ipsum dolor sit am" },
                   new TranscriptionData() { StartSeconds = 0, EndSeconds = 8, Transcript = "erat, sed diam voluptua. At vero e" }
                }
            };

            return outputFileInfo.FullName;
        }
    }
}
