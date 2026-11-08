namespace Koi.Web.Exceptions;

public class CheckoutException(IReadOnlyList<(string Field, ErrorCode Error)> errors)
    : AppException(errors[0].Error)
{
    public IReadOnlyList<(string Field, ErrorCode Error)> Errors { get; } = errors;
}
