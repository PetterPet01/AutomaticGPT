namespace AutomaticGPTPupTest1.FreeGPT.Models.Exceptions
{
    [Serializable]
    class InternalAPIException : Exception
    {
        public InternalAPIException() { }

        public InternalAPIException(string message)
            : base($"An exception occurred in the API library: {message}")
        {

        }
    }
}
