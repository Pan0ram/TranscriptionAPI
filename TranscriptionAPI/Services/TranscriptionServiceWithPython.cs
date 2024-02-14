using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Reflection;

namespace TranscriptionAPI
{
    public class TranscriptionServiceWithPython : ITranscriptionService
    {
        public Transcription GetTranscriptionFromYoutubeURL(string youtubeURL)
        {
            string filename = "PythonScript\\main.py";
            string path = Assembly.GetExecutingAssembly().Location;
            string rootDir = Directory.GetParent(path).FullName;

            RunPythonFile(rootDir, filename);

            // Todo: Return Real Model
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

            return model;
        }

        public int RunPythonFile(string rootDir, string filename)
        {
            ScriptEngine engine = Python.CreateEngine();
            ScriptSource source;
            source = engine.CreateScriptSourceFromFile(rootDir + "\\" + filename);

            ScriptScope scope = engine.CreateScope();
            //Todo: Fix Invariant Culture Exception
            int result = source.ExecuteProgram();
            return result;
        }
    }

   
}
