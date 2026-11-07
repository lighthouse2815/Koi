namespace Koi.Web.Exceptions;

public record ErrorCode(string Code, string Message, int StatusCode)
{
    public static readonly ErrorCode CATEGORY_NOT_FOUND =
        new("CATEGORY_NOT_FOUND", "Không tìm thấy danh mục.", StatusCodes.Status404NotFound);
}
