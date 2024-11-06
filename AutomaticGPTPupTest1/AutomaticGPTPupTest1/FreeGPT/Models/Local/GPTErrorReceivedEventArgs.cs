using PetterPet.FreeGPT.API;

namespace AutomaticGPTPupTest1.FreeGPT.Models.Local
{
    public class GPTErrorReceivedEventArgs : EventArgs
    {
        public GPTError Error { get; set; }
        public string ErrorMessage { get; set; }

        public GPTErrorReceivedEventArgs(GPTError error, string errorMessage)
        {
            Error = error;
            ErrorMessage = errorMessage;
        }
    }
}
