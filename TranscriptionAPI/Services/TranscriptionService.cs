using System;
using System.Collections.Generic;
using TranscriptionAPI.Modul;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;


namespace TranscriptionAPI.Services
{
  
 }
    public class dynamic_demo
    {
        static void Main()
        {
            var ipy = Python.CreateRuntime();
            dynamic test = ipy.UseFile("main.py");
            test.Simple();
        }
    }

    public interface ITranscriptionService
    {
        Transcription GetTranscriptionFromYoutubeURL(string youtubeURL);
        string GetTranscriptionTextWithTimestampsFromYoutubeURL(string youtubeURL);
    }
}
