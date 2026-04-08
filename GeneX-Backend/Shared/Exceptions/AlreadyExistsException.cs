namespace GeneX_Backend.Shared.Exceptions
{
    public class AlreadyExistsException : Exception
    {
        public AlreadyExistsException(string message) : base(message){}
    }
}