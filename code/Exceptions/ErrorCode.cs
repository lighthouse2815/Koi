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

    public static readonly ErrorCode STORE_NOT_FOUND =
        new("STORE_NOT_FOUND", "Không tìm thấy chi nhánh.", StatusCodes.Status404NotFound);

    public static readonly ErrorCode STORE_CITY_INVALID =
        new("STORE_CITY_INVALID", "Chọn khu vực hợp lệ.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode STORE_LINK_INVALID =
        new("STORE_LINK_INVALID", "Nhập liên kết HTTPS hoặc HTTP hợp lệ.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode STORE_CITY_NOT_FOUND =
        new("STORE_CITY_NOT_FOUND", "Không tìm thấy khu vực.", StatusCodes.Status404NotFound);

    public static readonly ErrorCode ORDER_FULFILLMENT_INVALID =
        new("ORDER_FULFILLMENT_INVALID", "Chọn cách nhận hàng hợp lệ.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode ORDER_ADDRESS_REQUIRED =
        new("ORDER_ADDRESS_REQUIRED", "Nhập địa chỉ giao hàng.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode ORDER_BRANCH_UNAVAILABLE =
        new("ORDER_BRANCH_UNAVAILABLE", "Chi nhánh không còn nhận đơn. Vui lòng chọn chi nhánh khác.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode ORDER_CART_EMPTY =
        new("ORDER_CART_EMPTY", "Giỏ hàng đang trống.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode ORDER_DRINK_UNAVAILABLE =
        new("ORDER_DRINK_UNAVAILABLE", "Có món không còn được bán. Vui lòng xóa món đó khỏi giỏ hàng.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode ORDER_OPTIONS_INVALID =
        new("ORDER_OPTIONS_INVALID", "Các lựa chọn cũ không còn được hỗ trợ. Vui lòng xóa món cũ và chọn lại.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode ORDER_PRICE_CHANGED =
        new("ORDER_PRICE_CHANGED", "Giá đã thay đổi. Vui lòng kiểm tra lại giỏ hàng và xác nhận đặt hàng lần nữa.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode NEWS_NOT_FOUND =
        new("NEWS_NOT_FOUND", "Không tìm thấy bài viết.", StatusCodes.Status404NotFound);

    public static readonly ErrorCode NEWS_FEATURED_UNPUBLISHED =
        new("NEWS_FEATURED_UNPUBLISHED", "Tin nổi bật cần được xuất bản.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode NEWS_VIDEO_INVALID =
        new("NEWS_VIDEO_INVALID", "Nhập liên kết video YouTube hợp lệ.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode NEWS_BODY_REQUIRED =
        new("NEWS_BODY_REQUIRED", "Nhập nội dung bài viết.", StatusCodes.Status400BadRequest);

    public static readonly ErrorCode NEWS_IMAGE_REQUIRED =
        new("NEWS_IMAGE_REQUIRED", "Chọn ảnh cho bài viết.", StatusCodes.Status400BadRequest);

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
