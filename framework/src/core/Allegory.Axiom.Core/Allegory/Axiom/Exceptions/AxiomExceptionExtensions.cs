namespace Allegory.Axiom.Exceptions;

public static class AxiomExceptionExtensions
{
    extension(AxiomException exception)
    {
        public AxiomException AddData(object key, object value)
        {
            exception.Data[key] = value;
            return exception;
        }
    }
}