namespace GeneX_Backend.Shared.Exceptions
{
    public class UnauthroizedAccessException : Exception
    {
        public UnauthroizedAccessException(string message) : base(message) { }
    }
   
}