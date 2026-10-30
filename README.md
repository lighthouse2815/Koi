# KOI Thé · ASP.NET Core MVC

Dựng lại giao diện [KOI Thé](https://koithe.com/en/main.php) bằng ASP.NET Core MVC và Razor View. Hoàn thành từng trang để duyệt trước khi làm trang kế tiếp.

## Chạy project

Yêu cầu .NET 10 SDK. Không cần Node.js, npm hoặc database.

```bash
dotnet restore
dotnet run
```

Mở **http://localhost:5118** để xem trang bìa chọn ngôn ngữ như URL gốc. Chọn **ENGLISH** để vào `/Home/Index`. Liên kết 中文 mở bản gốc tiếng Trung; nội dung MVC hiện dùng tiếng Anh. Có thể chạy bằng Visual Studio với profile `http`.

```bash
dotnet build --configuration Release
dotnet publish --configuration Release --output ./artifacts/publish
```

## Phần 1: trang chủ — chờ duyệt

- Giao diện tiếng Anh, theo bản tiếng Anh của website mẫu.
- Video mở đầu lần truy cập đầu tiên trong ngày: header xuất hiện ở giây 1, Skip ở giây 2, hero trượt lên từ giây 5,5; có Replay intro.
- Header cố định, menu hình quạt bung xoay khi hover/click, hỗ trợ bàn phím và Escape.
- Cảnh bìa cố định là người đọc sách, cầm ly trà; đồng bộ video desktop/tablet/mobile, ảnh trong vòng O và poster từ cùng một cảnh của trang mẫu.
- Hero giữ màn hình khi cuộn; chữ KOI trượt ngang và vòng chữ O xoay theo tiến độ. Dùng ảnh chữ, logo và đường cong header riêng cho mobile từ bản gốc.
- Ly trà xoay liên tục, đổi qua 23 khung hình theo cuộn, sau đó mở chuyển cảnh hình tròn.
- Chữ nền dùng SVG gốc, chạy liền mạch ở bốn tốc độ và dừng khi ra ngoài màn hình.
- Bốn cảnh thương hiệu; các cột hình tròn chạy ngược chiều. Mobile có cảnh phóng hình đầu tiên riêng, đủ bốn GIF ở cột giữa và khoảng chuyển cột dài hơn desktop. Chữ cùng hai đường cong có lớp thu/phóng riêng khi vào và rời cảnh. Mỗi lần đổi cột có vùng cuộn và đường tăng tốc riêng theo mẫu; các pha được cộng lại để cuộn nhanh hoặc đảo chiều không ghi đè vị trí nhau.
- Đồ uống, thành phần và nước bung theo hướng, tâm phóng và độ trễ của từng lớp trong bản gốc; parallax theo con trỏ. Cảnh được giữ qua lúc chuyển sang danh mục đồ uống, rồi đảo lại khi cuộn ngược.
- Danh mục đồ uống giữ màn hình và trượt ngang khi cuộn dọc; hỗ trợ nút và phím mũi tên, ảnh tròn xoay khi hover trên desktop và theo cuộn trên mobile.
- Tin tức trượt chuyển bài, tiêu đề xuất hiện theo cuộn. Footer dùng 13 chữ rời xếp lệch như mẫu; linh vật trượt cả thân lên, nâng ly và hiện Menu/Store khi hover hoặc focus.
- Responsive; `prefers-reduced-motion` chuyển sang bố cục tĩnh, tắt video/GIF và bỏ các cảnh giữ màn hình.

Các trang nội dung đã dựng local, dùng tiếng Anh và ảnh/SVG lưu trong `wwwroot`:

| Trang | URL | Tương tác |
| --- | --- | --- |
| About | `/Home/About` | Cảnh chữ KOI/ly trà, ảnh xoay khi cuộn ngang, nguyên liệu chuyển mờ tự động, dải ảnh lịch sử xếp chéo, bản đồ sáu thương hiệu |
| Our Tea | `/Home/OurTea` | Dải ly và bốn nền chuyển động, New Drinks đồng bộ nền/ảnh, ảnh đồ uống đổi theo cuộn trên mobile |
| Recruit | `/Home/Recruit` | Chữ hiện lần lượt, bảng lộ trình phát triển, ảnh đội ngũ theo thiết bị và bốn pha cuộn Apply NOW |
| News | `/Home/News` | Tiêu đề xuất hiện theo mẫu, nút More chuyển động, danh sách responsive, lọc/phân trang và 13 bài chi tiết `/Home/NewsDetail/{id}` |
| Impact | `/Home/Impact` | Banner giữ màn hình, chữ chạy theo cuộn, chuyển bốn cam kết bằng cuộn hoặc nút |
| Chi tiết Impact | `/Home/Responsibility`, `/Home/Precision`, `/Home/People`, `/Home/Connections` | Ảnh, video, số liệu đếm, nội dung nhân sự và hoạt động cộng đồng |

Trang bìa ở `/`; trang chủ ở `/Home/Index`. Header/footer đã nối tới các trang local và hiển thị đúng tên trang đang xem. Store, Contact và liên kết menu PDF vẫn mở trang mẫu. Chưa có đặt hàng, gửi form hoặc database; các liên kết ứng tuyển giữ cổng tuyển dụng/email gốc.

Trang About, Our Tea và News đã được duyệt. Recruit được hoàn thiện tiếp để kiểm tra riêng trước khi chuyển sang trang khác.

Nội dung News là bản tĩnh tại thời điểm dựng. Video bài viết và chi tiết Impact dùng YouTube gốc. Poster của bài cuộc thi năm 2024 dùng lại ảnh của chính bài viết vì tên miền chứa ảnh nội dung cũ đã ngừng hoạt động.

Chuỗi hoạt ảnh dùng GSAP, ScrollTrigger và ScrollToPlugin, dựa trên thứ tự cảnh, sprite và các mốc chuyển cảnh của website mẫu. Độ dài vùng cuộn và bố cục tự điều chỉnh theo màn hình; chế độ giảm chuyển động vẫn hiển thị đủ nội dung bằng bố cục tĩnh.

[Video chạy thử toàn bộ chuỗi animation](docs/preview/trang-chu-animation.mp4).

[Đối chiếu cuộn chuột: trang mẫu bên trái, MVC bên phải](docs/preview/doi-chieu-cuon-fragrance.mp4). Hai bên nhận cùng sự kiện cuộn, gồm cả dừng và cuộn ngược. Kiểm tra nhịp bằng nấc wheel 120px và lấy mẫu vị trí từng khung hình ở 1440×900, 2560×1325; kiểm tra thêm nhảy qua nhiều cảnh rồi đảo chiều.

[Clip riêng các lần đổi cảnh FRAGRANCE](docs/preview/tu-fragrance-animation.mp4) · [FRAGRANCE desktop](docs/preview/fragrance-desktop.jpg) · [FRAGRANCE mobile](docs/preview/fragrance-mobile.jpg).

Trang bìa: [desktop](docs/preview/trang-bia-desktop.png) · [điện thoại](docs/preview/trang-bia-mobile.png).

Ảnh xem trước: [desktop](docs/preview/trang-chu-desktop.jpg) · [điện thoại](docs/preview/trang-chu-mobile.jpg).

## Kiểm tra

Đối chiếu CSS gốc bằng trình duyệt: tọa độ/kích thước logo và vòng chọn ngôn ngữ ở 1440/390px; kích thước/vị trí chữ KOI, vòng O và quãng cuộn ngang ở 390/768/1024/1440/1920px khớp trong phạm vi 1px. Kiểm tra này xác nhận các phần được đo, chưa phải xác nhận toàn trang giống tuyệt đối.

Các kiểm tra sau đã đạt trên bản publish Release: build/publish .NET và cú pháp JavaScript. Kiểm tra trình duyệt bằng Playwright gồm intro/Skip/truy cập lại, sprite và chuyển cảnh, bốn trạng thái thương hiệu theo cả hai chiều cuộn, parallax, carousel bằng cuộn/nút/bàn phím, menu hover/rê chuột ra ngoài/bàn phím/Escape, chuyển tin tức và chế độ giảm chuyển động. Kiểm tra đổi giữa 360, 390, 768, 1024, 1440, 1920 và 2560px để phát hiện tràn ngang hoặc tạo lặp ScrollTrigger. Video intro lỗi không làm kẹt trang. Kiểm tra riêng phần FRAGRANCE xác nhận chữ/đường cong thu phóng độc lập, GIF thực sự đổi khung hình và mobile hiện đủ bốn GIF ở cột giữa. Quét axe WCAG 2/2.1 A/AA không phát hiện vi phạm tự động trên desktop/điện thoại.

## Cấu trúc

- `Controllers/HomeController.cs`: controller trang chủ và xử lý lỗi.
- `Views/Home/Cover.cshtml`, `Views/Shared/_CoverLayout.cshtml`, `wwwroot/css/cover.css`: trang bìa chọn ngôn ngữ.
- `Views/Home/Index.cshtml`, `_FeatureStage.cshtml`: trang chủ và phần cảnh thương hiệu dùng chung.
- `Views/Shared/_Layout.cshtml`: khung Razor chung.
- `Views/Shared/_Navigation.cshtml`, `_Footer.cshtml`: header/menu và footer.
- `wwwroot/css/site.css`, `wwwroot/js/site.js`: style và các tương tác giao diện.
- `wwwroot/js/home-animations.js`: timeline GSAP, các cảnh theo cuộn, intro và parallax.
- `wwwroot/images`, `wwwroot/videos`, `wwwroot/fonts`: tài nguyên local.

Dùng template MVC chính thức của .NET, không thêm NuGet. GSAP 3.15.0 cùng ScrollTrigger/ScrollToPlugin được lưu trong `wwwroot/lib/gsap`, giữ thông tin bản quyền và giấy phép. Không cần npm để build hoặc chạy ứng dụng. Bỏ Bootstrap/jQuery mặc định vì trang này không sử dụng chúng.

About và Our Tea dùng Swiper 14.3.0 lưu local tại `wwwroot/lib/swiper` (MIT). About dùng cho ảnh/nội dung nguyên liệu và lịch sử; Our Tea dùng cho nền dải ly, ảnh/nền New Drinks và tên món trên mobile. Tự chuyển dừng khi phần tương ứng ra khỏi màn hình, tab bị ẩn, người dùng bấm Pause hoặc bật giảm chuyển động. Mobile xếp dọc bốn giá trị thương hiệu của About, dùng đúng nhóm ảnh bìa và hỗ trợ kéo dải ảnh lịch sử; New Drinks hỗ trợ vuốt và chấm chọn.

[Video chạy thử animation About](docs/preview/about-animation.mp4), gồm cuộn xuôi/ngược từ SELECTION OF FINE INGREDIENTS, chuyển nguyên liệu và lịch sử.

[Video chạy thử Our Tea](docs/preview/our-tea-animation.mp4), gồm dải ly, chuyển nền/ảnh New Drinks và chọn danh mục. Với các món trang mẫu không cung cấp ảnh riêng, phần xem trước giữ ảnh tổng hợp của nhóm.

[Video chạy thử Recruit](docs/preview/recruit-animation.mp4), gồm chữ mở đầu, hai ảnh đội ngũ và bốn pha Apply NOW theo cả hai chiều cuộn. Ảnh xem trước: [desktop](docs/preview/recruit-desktop.png) · [mobile](docs/preview/recruit-mobile.png) · [Apply NOW trên mobile](docs/preview/recruit-apply-mobile.png).

## Tài nguyên tham chiếu

Hình ảnh, logo và video lấy từ trang chủ KOI Thé theo yêu cầu đối chiếu giao diện; quyền đối với những tài nguyên này thuộc chủ sở hữu tương ứng. Không hotlink tài nguyên và không đưa API key của website mẫu vào source. Font Red Hat Display dùng giấy phép SIL Open Font License, kèm tại `wwwroot/fonts/OFL.txt`.

## Kiểm tra các trang nội dung

Build/publish Release không có lỗi hoặc cảnh báo. Kiểm tra Playwright gồm tài nguyên CSS/JS/ảnh có nội dung, không tràn ngang ở 390/768/1024/1440px, chọn nguyên liệu/thương hiệu/lịch sử, đổi đồ uống và danh mục, lọc/phân trang News, đủ 13 trang chi tiết và ID không tồn tại trả 404, sao chép liên kết, chuyển bốn cam kết theo cả hai chiều, carousel nhân sự/cộng đồng và chế độ giảm chuyển động. Trình phát YouTube được kiểm tra URL và thao tác Play/Back; phát video vẫn phụ thuộc kết nối tới YouTube.

About đã đối chiếu vị trí các cột ly trà và góc xoay ảnh theo cả hai chiều cuộn với trang mẫu. Kiểm tra thêm chuyển mờ nguyên liệu, chọn đủ ba thành phần, tự chuyển/Pause, đủ chín mốc lịch sử và quay vòng, đổi liên tục 390/768/1024/1440/1920px không tạo lặp carousel, ảnh bìa rời màn hình trên mobile và chế độ giảm chuyển động. Our Tea và Recruit cũng được kiểm tra lại sau sửa quy tắc hiển thị tablet/mobile dùng chung.

Our Tea đã đối chiếu bố cục tiêu đề, ảnh tròn và danh sách New Drinks trên desktop. Kiểm tra bốn lựa chọn, chuyển mờ ảnh/nền đồng bộ, tự chuyển/Pause, bảy danh mục và phần xem trước của 54 món, vuốt/chấm chọn New Drinks trên điện thoại, đổi ảnh theo cuộn, chọn danh mục mobile, đổi kích thước liên tục và giảm chuyển động.

News đã đối chiếu vị trí/kích thước tiêu đề, tin nổi bật, năm hàng tin và phân trang ở 390/768/1440px. Khôi phục SVG More, cách xếp metadata/tiêu đề/ảnh theo từng màn hình, nhịp hiện chữ và màu hover/focus. Kiểm tra lại lọc tin, ba trang danh sách, 13 bài chi tiết, sao chép liên kết, responsive và giảm chuyển động. Ảnh xem trước: [desktop](docs/preview/news-desktop.png) · [mobile](docs/preview/news-mobile.png).

Recruit đã đối chiếu bố cục và độ mờ/vị trí của bốn pha ứng tuyển với mẫu ở 1440/390px, gồm cả cuộn ngược. Kiểm tra thứ tự hiện chữ, ảnh desktop/tablet/mobile, đổi kích thước liên tục không tạo lặp ScrollTrigger, hover/focus, chặn focus khi vùng ứng tuyển không hoạt động và bố cục tĩnh khi giảm chuyển động. Liên kết ứng tuyển được kiểm tra URL; không gửi hồ sơ.
