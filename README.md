# KOI Thé · ASP.NET Core MVC

Dựng lại giao diện [KOI Thé](https://koithe.com/en/main.php) bằng ASP.NET Core MVC và Razor View. Hoàn thành từng trang để duyệt trước khi làm trang kế tiếp.

## Chạy project

Yêu cầu .NET 10 SDK. Không cần Node.js, npm hoặc database.

```bash
dotnet restore
dotnet run
```

Mở **http://localhost:5118**. Có thể chạy bằng Visual Studio với profile `http`.

```bash
dotnet build --configuration Release
dotnet publish --configuration Release --output ./artifacts/publish
```

## Phần 1: trang chủ — chờ duyệt

- Giao diện tiếng Anh, theo bản tiếng Anh của website mẫu.
- Header cố định, điều hướng hình quạt, menu mở bằng hộp thoại.
- Hero chữ KOI lớn, video nền desktop/mobile, nút tạm dừng video.
- Các dòng chữ chuyển động, hình trà và bốn điểm nhấn thương hiệu.
- Đồ uống đề xuất, danh mục đồ uống kéo ngang hoặc bấm mũi tên.
- Tin tức chuyển giữa hai bài bằng nút chọn, footer và linh vật.
- Responsive, điều hướng bàn phím, hỗ trợ `prefers-reduced-motion`.

Hiện chỉ triển khai trang chủ tại `/` hoặc `/Home/Index`. Các trang About, Our Tea, News, Store, Recruit, Contact và Impact chưa được dựng local. Liên kết tới các trang này mở trang tương ứng trên website mẫu; các liên kết nội bộ `#drinks`, `#news`, `#our-story` dẫn tới nội dung của trang chủ. Chưa có đặt hàng hoặc xử lý dữ liệu nghiệp vụ.

Bố cục và tài nguyên bám website mẫu; hiệu ứng cuộn được rút gọn, không sao chép toàn bộ chuỗi hoạt ảnh GSAP và đoạn video giới thiệu trước khi vào trang. Phần tính năng thương hiệu được bố trí theo lưới để dễ xem trên desktop và điện thoại.

Ảnh xem trước: [desktop](docs/preview/trang-chu-desktop.jpg) · [điện thoại](docs/preview/trang-chu-mobile.jpg).

## Kiểm tra

Đã build/publish Release, kiểm tra cú pháp JavaScript và chạy kiểm tra trình duyệt bằng Playwright: menu/đóng bằng Escape, dừng/phát video, cuộn ngang hero, carousel hai chiều, chuyển tin tức, chế độ giảm chuyển động, route và tài nguyên local. Không tràn ngang ở 360, 390, 768, 1024, 1440 và 1920px. Quét axe WCAG 2/2.1 A/AA trên desktop và điện thoại không phát hiện vi phạm tự động.

## Cấu trúc

- `Controllers/HomeController.cs`: controller trang chủ và xử lý lỗi.
- `Views/Home/Index.cshtml`: nội dung trang chủ.
- `Views/Shared/_Layout.cshtml`: khung Razor chung.
- `Views/Shared/_Navigation.cshtml`, `_Footer.cshtml`: header/menu và footer.
- `wwwroot/css/site.css`, `wwwroot/js/site.js`: style và các tương tác giao diện.
- `wwwroot/images`, `wwwroot/videos`, `wwwroot/fonts`: tài nguyên local.

Dùng template MVC chính thức của .NET, không thêm NuGet hay thư viện frontend. Bỏ Bootstrap/jQuery mặc định vì trang này không sử dụng chúng.

## Tài nguyên tham chiếu

Hình ảnh, logo và video lấy từ trang chủ KOI Thé theo yêu cầu đối chiếu giao diện; quyền đối với những tài nguyên này thuộc chủ sở hữu tương ứng. Không hotlink tài nguyên và không đưa API key của website mẫu vào source. Font Red Hat Display dùng giấy phép SIL Open Font License, kèm tại `wwwroot/fonts/OFL.txt`.
