using Cyotek.Collections.Generic;
using NAudio.Utils;
using NAudio.Wave;
using Newtonsoft.Json;
using PetterPet.CNNVAD.Time;
using PetterPet.FFTSSharp;
using System.Diagnostics;
using System.Threading.Channels;
using Vosk;

namespace VoskAutomationTest1.VoskAutomation
{
    public class Result
    {
        public double conf { get; set; }
        public double end { get; set; }
        public double start { get; set; }
        public string word { get; set; }
    }

    public class SpeechResult
    {
        public List<Result>? result { get; set; }
        public string? text { get; set; }
    }

    public class PartialSpeechResult
    {
        public string? partial { get; set; }
    }

    internal class AutomaticVosk : IDisposable
    {
        public event EventHandler<string>? SpeechEnded;
        public event EventHandler<string>? SpeechRecognized;
        public event EventHandler<bool>? SpeechDetected;

        AudioRecorder VAD;
        VoskRecognizer rec;

        private Stream memoryStream; //for save 
        public WaveFileWriter waveFile;
        ChannelReader<FrameProcessedEventArgs> reader;
        CircularBuffer<bool> predictions = new CircularBuffer<bool>(8);
        float secsOfSilence;
        CircularBuffer<bool> arbitraryPredicionsBuffer;
        string currentSpeech;
        bool speechStopped;
        static readonly int deviceSamplingRate = 48000;
        static readonly float appendSecs = 0.2f;
        static readonly int samplesPerFrame = (int)(deviceSamplingRate * 0.0125);

        private bool disposedValue;

        protected virtual void OnSpeechRecognized(string e)
        {
            SpeechRecognized?.Invoke(this, e);
        }

        protected virtual void OnSpeechEnded(string e)
        {
            SpeechEnded?.Invoke(this, e);
        }

        protected virtual void OnSpeechDetected(bool e)
        {
            SpeechDetected?.Invoke(this, e);
        }

        static string? GetSpeechText(VoskRecognizer rec, bool isFull = true)
        {
            if (isFull)
            {
                SpeechResult? sr = JsonConvert.DeserializeObject<SpeechResult>(rec.Result());
                if (sr == null || sr.text == null || sr.text.Trim() == "")
                    return null;

                return sr.text;
            }
            else
            {
                PartialSpeechResult? sr = JsonConvert.DeserializeObject<PartialSpeechResult>(rec.PartialResult());
                if (sr == null || sr.partial == null || sr.partial.Trim() == "")
                    return null;

                return sr.partial;
            }
        }

        void ProcessSpeech(byte[] data)
        {
            string? speech = GetSpeechText(rec, rec.AcceptWaveform(data, data.Length));
            if (speech == null)
                return;
            OnSpeechRecognized(speech);
        }

        private static async Task ForceAsync<T>(Func<Task> func)
        {
            await Task.Yield();
            await func();
        }

        void ProcessFrame(FrameProcessedEventArgs e)
        {
            if (waveFile != null)
            {
                waveFile.Write(e.Data, 0, e.Data.Length);
                waveFile.Flush();
            }

            if (predictions.Size == 0)
            {
                predictions.Put(e.IsSpeaking);

                for (int i = 0; i < (e.Data.Length / samplesPerFrame); i++)
                    arbitraryPredicionsBuffer.Put(e.IsSpeaking);
                return;
            }

            int count = 0;
            bool[] preds = predictions.Peek(predictions.Size);
            foreach (bool b in preds)
                if (b) count++;

            //Finalize a speech part after a certain amount of silence
            if (!speechStopped && count == 0)
            {
                string? speech = GetSpeechText(rec, false);
                if (speech != null)
                    currentSpeech += speech;
                Debug.WriteLine("Resetting VoskRecognizer");
                rec.Reset();
                rec.FinalResult();
                speechStopped = true;
            }
            else if (arbitraryPredicionsBuffer.Size < predictions.Size)
            {
                string? speech = GetSpeechText(rec, false);
                if (speech != null)
                    currentSpeech = speech;
            }

            //Recognize speech
            bool isSpeaking;
            if (count > predictions.Size / 2)
                isSpeaking = true;
            else
                isSpeaking = e.IsSpeaking;
            if (isSpeaking)
            {
                if (speechStopped && count == 0)
                {
                    Debug.WriteLine("appended");
                    if (waveFile != null)
                    {
                        //waveFile.Write(e.Append, 0, e.Append.Length);
                        //waveFile.Flush();
                    }
                    ProcessSpeech(e.Append);
                }
                if (waveFile != null)
                {
                    //waveFile.Write(e.Data, 0, e.Data.Length);
                    //waveFile.Flush();
                }
                ProcessSpeech(e.Data);
                speechStopped = false;
            }
            OnSpeechDetected(isSpeaking);

            //Call SpeechEnded event after a specified amount of silience
            bool[] arbPreds = arbitraryPredicionsBuffer.Peek(arbitraryPredicionsBuffer.Size);
            if (currentSpeech != "" && arbPreds.All(x => !x))
            {
                OnSpeechEnded(currentSpeech);
                currentSpeech = "";
            }

            predictions.Put(e.IsSpeaking);
            for (int i = 0; i < (e.Data.Length / samplesPerFrame); i++)
                arbitraryPredicionsBuffer.Put(e.IsSpeaking);

            //Debug.WriteLine("finished processing");
        }

        async Task ConsumeAsync()
        {
            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out FrameProcessedEventArgs? e))
                {
                    //Stopwatch watch = new Stopwatch();
                    //watch.Start();
                    await Task.Run(() => ProcessFrame(e));
                    //watch.Stop();
                    //if (watch.ElapsedMilliseconds > 5)
                    //Debug.WriteLine(watch.ElapsedMilliseconds);
                }
            }
        }

        public AutomaticVosk(string cnnvadModelPath, string voskModelPath, float secsOfSilence = 0.2f)
        {
            FFTSManager.LoadAppropriateDll();

            this.secsOfSilence = secsOfSilence;
            arbitraryPredicionsBuffer = new CircularBuffer<bool>((int)(secsOfSilence / 0.0125f));

            Model voskModel = new Model(voskModelPath);
            rec = new VoskRecognizer(voskModel, deviceSamplingRate);
            rec.SetMaxAlternatives(0);
            rec.SetWords(false);

            VAD = new AudioRecorder();
            VAD.start(cnnvadModelPath, deviceSamplingRate, ProcessFrame, appendSecs);
            //ConsumeAsync();

            memoryStream = new MemoryStream();
            //WaveFileWriter with ignoredisposesream memorystream
            waveFile = new WaveFileWriter(new IgnoreDisposeStream(memoryStream), new WaveFormat(48000, 1));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    VAD.Dispose();
                    rec.FinalResult();
                    if (waveFile != null)
                    {
                        waveFile.Close();
                        waveFile = null;
                    }
                    var fileStream = File.Create(@"D:\testaudio.wav");
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                    fileStream.Close();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
