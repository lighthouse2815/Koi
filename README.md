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
- Video mở đầu lần truy cập đầu tiên trong ngày, nút Skip và Replay intro.
- Header cố định, menu hình quạt bung xoay khi hover/click, hỗ trợ bàn phím và Escape.
- Hero giữ màn hình khi cuộn; chữ KOI trượt ngang và vòng chữ O xoay theo tiến độ.
- Ly trà xoay liên tục, đổi qua 23 khung hình theo cuộn, sau đó mở chuyển cảnh hình tròn.
- Bốn cảnh thương hiệu; các cột hình tròn chạy ngược chiều và thu/phóng theo cuộn.
- Đồ uống, thành phần và nước bung ra từ tâm; từng lớp parallax theo con trỏ.
- Danh mục đồ uống giữ màn hình và trượt ngang khi cuộn dọc; hỗ trợ nút và phím mũi tên, ảnh tròn xoay khi hover.
- Tin tức trượt chuyển bài, tiêu đề xuất hiện theo cuộn, chữ footer và linh vật chuyển động.
- Responsive; `prefers-reduced-motion` chuyển sang bố cục tĩnh, tắt video/GIF và bỏ các cảnh giữ màn hình.

Hiện chỉ triển khai trang chủ tại `/` hoặc `/Home/Index`. Các trang About, Our Tea, News, Store, Recruit, Contact và Impact chưa được dựng local. Liên kết tới các trang này mở trang tương ứng trên website mẫu; các liên kết nội bộ `#drinks`, `#news`, `#our-story` dẫn tới nội dung của trang chủ. Chưa có đặt hàng hoặc xử lý dữ liệu nghiệp vụ.

Chuỗi hoạt ảnh dùng GSAP, ScrollTrigger và ScrollToPlugin, dựa trên thứ tự cảnh, sprite và các mốc chuyển cảnh của website mẫu. Độ dài vùng cuộn và bố cục tự điều chỉnh theo màn hình; chế độ giảm chuyển động vẫn hiển thị đủ nội dung bằng bố cục tĩnh.

[Video chạy thử toàn bộ chuỗi animation](docs/preview/trang-chu-animation.mp4).

Ảnh xem trước: [desktop](docs/preview/trang-chu-desktop.jpg) · [điện thoại](docs/preview/trang-chu-mobile.jpg).

## Kiểm tra

Các kiểm tra sau đã đạt trên bản publish Release: build/publish .NET và cú pháp JavaScript. Kiểm tra trình duyệt bằng Playwright gồm intro/Skip/truy cập lại, sprite và chuyển cảnh, bốn trạng thái thương hiệu theo cả hai chiều cuộn, parallax, carousel bằng cuộn/nút/bàn phím, menu hover/rê chuột ra ngoài/bàn phím/Escape, chuyển tin tức và chế độ giảm chuyển động. Kiểm tra đổi giữa 360, 390, 768, 1024, 1440 và 1920px để phát hiện tràn ngang hoặc tạo lặp ScrollTrigger. Video intro lỗi không làm kẹt trang. Quét axe WCAG 2/2.1 A/AA không phát hiện vi phạm tự động trên desktop/điện thoại.

## Cấu trúc

- `Controllers/HomeController.cs`: controller trang chủ và xử lý lỗi.
- `Views/Home/Index.cshtml`: nội dung trang chủ.
- `Views/Shared/_Layout.cshtml`: khung Razor chung.
- `Views/Shared/_Navigation.cshtml`, `_Footer.cshtml`: header/menu và footer.
- `wwwroot/css/site.css`, `wwwroot/js/site.js`: style và các tương tác giao diện.
- `wwwroot/js/home-animations.js`: timeline GSAP, các cảnh theo cuộn, intro và parallax.
- `wwwroot/images`, `wwwroot/videos`, `wwwroot/fonts`: tài nguyên local.

Dùng template MVC chính thức của .NET, không thêm NuGet. GSAP 3.15.0 cùng ScrollTrigger/ScrollToPlugin được lưu trong `wwwroot/lib/gsap`, giữ thông tin bản quyền và giấy phép. Không cần npm để build hoặc chạy ứng dụng. Bỏ Bootstrap/jQuery mặc định vì trang này không sử dụng chúng.

## Tài nguyên tham chiếu

Hình ảnh, logo và video lấy từ trang chủ KOI Thé theo yêu cầu đối chiếu giao diện; quyền đối với những tài nguyên này thuộc chủ sở hữu tương ứng. Không hotlink tài nguyên và không đưa API key của website mẫu vào source. Font Red Hat Display dùng giấy phép SIL Open Font License, kèm tại `wwwroot/fonts/OFL.txt`.
