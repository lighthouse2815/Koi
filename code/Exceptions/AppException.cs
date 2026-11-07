namespace Koi.Web.Exceptions;

public class AppException(ErrorCode error) : Exception(error.Message)
{
    public ErrorCode Error { get; } = error;
}
