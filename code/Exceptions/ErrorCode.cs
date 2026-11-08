namespace Koi.Web.Exceptions;

public record ErrorCode(string Code, string Message, int StatusCode)
{
    public static readonly ErrorCode DATA_CONFLICT =
        new("DATA_CONFLICT", "Dữ liệu đã thay đổi. Vui lòng tải lại và thử lại.", StatusCodes.Status409Conflict);

    public static readonly ErrorCode INVALID_INPUT =
        new("INVALID_INPUT", "Dữ liệu không hợp lệ.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode INVALID_IMAGE =
        new("INVALID_IMAGE", "Ảnh tải lên không hợp lệ.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode DRINK_NOT_FOUND =
        new("DRINK_NOT_FOUND", "Không tìm thấy đồ uống.", StatusCodes.Status404NotFound);

    public static readonly ErrorCode DRINK_CATEGORY_INVALID =
        new("DRINK_CATEGORY_INVALID", "Chọn danh mục hợp lệ.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode FEATURED_DRINK_NOT_FOUND =
        new("FEATURED_DRINK_NOT_FOUND", "Không tìm thấy mục New Drinks.", StatusCodes.Status404NotFound);

    public static readonly ErrorCode FEATURED_DRINK_IMAGE_REQUIRED =
        new("FEATURED_DRINK_IMAGE_REQUIRED", "Chọn ảnh đồ uống.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode FEATURED_DRINK_BACKGROUND_REQUIRED =
        new("FEATURED_DRINK_BACKGROUND_REQUIRED", "Chọn ảnh nền.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode MENU_NOT_FOUND =
        new("MENU_NOT_FOUND", "Không tìm thấy thực đơn.", StatusCodes.Status404NotFound);

    public static readonly ErrorCode MENU_IMAGE_REQUIRED =
        new("MENU_IMAGE_REQUIRED", "Chọn ảnh thực đơn.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode CATEGORY_NOT_FOUND =
        new("CATEGORY_NOT_FOUND", "Không tìm thấy danh mục.", StatusCodes.Status404NotFound);

    public static readonly ErrorCode CATEGORY_NAME_EXISTS =
        new("CATEGORY_NAME_EXISTS", "Tên danh mục đã tồn tại.", StatusCodes.Status409Conflict);

    public static readonly ErrorCode CATEGORY_INVALID =
        new("CATEGORY_INVALID", "Dữ liệu danh mục không hợp lệ.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode CATEGORY_IMAGE_REQUIRED =
        new("CATEGORY_IMAGE_REQUIRED", "Chọn ảnh chung cho danh mục.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode CATEGORY_ICON_REQUIRED =
        new("CATEGORY_ICON_REQUIRED", "Chọn biểu tượng thẻ trang chủ.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode CATEGORY_HAS_DRINKS =
        new("CATEGORY_HAS_DRINKS", "Không thể xóa danh mục đang có đồ uống.", StatusCodes.Status409Conflict);

    public static readonly ErrorCode CATEGORY_CHANGED =
        new("CATEGORY_CHANGED", "Danh mục đã thay đổi. Vui lòng tải lại và thử lại.", StatusCodes.Status409Conflict);
}
