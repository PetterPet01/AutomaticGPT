using System.Speech.Synthesis;

namespace AutomaticGPTPupTest1
{
    public static class SystemSpeechExtension
    {
        const int PDefaultRate = 3;
        const int PErrorRate = 4;
        public static void Speak(this string text2speak)
        {
            text2speak.Speak(PDefaultRate);
        }
        static SpeechSynthesizer voice = new SpeechSynthesizer();
        public static void Speak(this string text2speak, int rate)
        {
            voice.SetOutputToDefaultAudioDevice();
            voice.Rate = rate;
            voice.Speak(text2speak);
        }

        public static void Stop()
        {
            voice.Pause();
            voice.SpeakAsyncCancelAll();
            voice.Resume();
        }
    }
}
