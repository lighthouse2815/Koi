namespace Koi.Web.Exceptions;

public record ErrorCode(string Code, string Message, int StatusCode)
{
    public static readonly ErrorCode CATEGORY_NOT_FOUND =
        new("CATEGORY_NOT_FOUND", "Không tìm thấy danh mục.", StatusCodes.Status404NotFound);

    public static readonly ErrorCode CATEGORY_NAME_EXISTS =
        new("CATEGORY_NAME_EXISTS", "Tên danh mục đã tồn tại.", StatusCodes.Status409Conflict);

    public static readonly ErrorCode CATEGORY_INVALID =
        new("CATEGORY_INVALID", "Dữ liệu danh mục không hợp lệ.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode CATEGORY_HAS_DRINKS =
        new("CATEGORY_HAS_DRINKS", "Không thể xóa danh mục đang có đồ uống.", StatusCodes.Status409Conflict);

    public static readonly ErrorCode CATEGORY_CHANGED =
        new("CATEGORY_CHANGED", "Danh mục đã thay đổi. Vui lòng tải lại và thử lại.", StatusCodes.Status409Conflict);
}
