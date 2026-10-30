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
| Impact | `/Home/Impact` | Banner giữ màn hình, chữ chạy theo cuộn, mỗi lượt cuộn/vuốt chuyển một cam kết, hỗ trợ chấm chọn và quay lại đúng cam kết |
| Responsibility | `/Home/Responsibility` | Hai ảnh bìa trượt vào, chữ hiện theo thứ tự, bốn số liệu đếm cùng lúc, ảnh nguồn nguyên liệu hiện theo cuộn và video có Play/Back trên mobile |
| Precision | `/Home/Precision` | Banner giữ màn hình, nền tối theo cuộn, đoạn chữ chạy lên và video Play/Back trên mobile |
| People | `/Home/People` | Chữ hiện theo cuộn, năm thẻ Foundations cuộn vòng, ảnh Moments tự chạy/kéo/Pause và liên kết tuyển dụng |
| Connections | `/Home/Connections` | Ảnh bìa thu dần, OUR BELIEF hiện theo cuộn, ba nội dung cộng đồng chuyển mờ tự động và bảy thẻ IN ACTION cuộn vòng, căn giữa |
| Store | `/Home/Store` | Tiêu đề trượt lên, lọc Country/Location/Brand, chín cửa hàng mỗi trang và hộp ảnh tròn hỗ trợ bàn phím/cảm ứng |
| Contact | `/Home/Contact` | Tiêu đề trượt lên, chuyển mờ giữa Contact/Business Partnership, ảnh Overseas trên desktop và các trường nhập/chọn dùng điều khiển gốc của trình duyệt |

Trang bìa ở `/`; trang chủ ở `/Home/Index`. Header/footer đã nối tới các trang local và hiển thị đúng tên trang đang xem. Liên kết menu PDF vẫn mở trang mẫu. Chưa có đặt hàng, gửi form hoặc database; các liên kết ứng tuyển giữ cổng tuyển dụng/email gốc.

Trang About, Our Tea, News, Recruit, Impact, Responsibility, Precision, People, Connections và Store đã được duyệt. Contact được hoàn thiện tiếp để kiểm tra riêng trước khi chuyển sang trang khác.

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

About, Our Tea, People và Connections dùng Swiper 14.3.0 lưu local tại `wwwroot/lib/swiper` (MIT). About dùng cho ảnh/nội dung nguyên liệu và lịch sử; Our Tea dùng cho nền dải ly, ảnh/nền New Drinks và tên món trên mobile; People dùng cho Foundations và Moments; Connections dùng cho IN ACTION. Tự chuyển dừng khi phần tương ứng ra khỏi màn hình, tab bị ẩn, người dùng bấm Pause hoặc bật giảm chuyển động. Mobile xếp dọc bốn giá trị thương hiệu của About, dùng đúng nhóm ảnh bìa và hỗ trợ kéo dải ảnh lịch sử; New Drinks hỗ trợ vuốt và chấm chọn.

[Video chạy thử animation About](docs/preview/about-animation.mp4), gồm cuộn xuôi/ngược từ SELECTION OF FINE INGREDIENTS, chuyển nguyên liệu và lịch sử.

[Video chạy thử Our Tea](docs/preview/our-tea-animation.mp4), gồm dải ly, chuyển nền/ảnh New Drinks và chọn danh mục. Với các món trang mẫu không cung cấp ảnh riêng, phần xem trước giữ ảnh tổng hợp của nhóm.

[Video chạy thử Recruit](docs/preview/recruit-animation.mp4), gồm chữ mở đầu, hai ảnh đội ngũ và bốn pha Apply NOW theo cả hai chiều cuộn. Ảnh xem trước: [desktop](docs/preview/recruit-desktop.png) · [mobile](docs/preview/recruit-mobile.png) · [Apply NOW trên mobile](docs/preview/recruit-apply-mobile.png).

[Video chạy thử Impact](docs/preview/impact-animation.mp4), gồm banner, chữ chạy ngang và các lượt cuộn đổi cam kết theo cả hai chiều. Ảnh xem trước: [desktop](docs/preview/impact-desktop.png) · [bốn cam kết](docs/preview/impact-commitments.png) · [mobile](docs/preview/impact-mobile.png) · [cam kết trên mobile](docs/preview/impact-commitments-mobile.png).

[Video chạy thử Responsibility](docs/preview/responsibility-animation.mp4), gồm hai ảnh mở đầu, tiêu đề, số liệu và ba ảnh nguồn nguyên liệu. Ảnh xem trước: [desktop](docs/preview/responsibility-desktop.png) · [mobile](docs/preview/responsibility-mobile.png) · [số liệu trên mobile](docs/preview/responsibility-numbers-mobile.png).

[Video chạy thử Precision](docs/preview/precision-animation.mp4), gồm cảnh bìa theo hai chiều cuộn, tiêu đề video và ba ảnh quy trình chất lượng. Ảnh xem trước: [desktop](docs/preview/precision-desktop.png) · [mobile](docs/preview/precision-mobile.png) · [chữ chạy trên mobile](docs/preview/precision-banner-mobile.png).

[Video chạy thử People](docs/preview/people-animation.mp4), gồm tiêu đề, Foundations, ba ảnh nội dung, Moments và JOIN OUR TEAM. Ảnh xem trước: [desktop](docs/preview/people-desktop.png) · [mobile](docs/preview/people-mobile.png) · [Foundations trên mobile](docs/preview/people-features-mobile.png) · [Moments](docs/preview/people-moments.png).

[Video chạy thử Connections](docs/preview/connections-animation.mp4), gồm ảnh bìa, OUR BELIEF, chuyển mờ nội dung cộng đồng, ba ảnh hoạt động và IN ACTION. Ảnh xem trước: [desktop](docs/preview/connections-desktop.png) · [mobile](docs/preview/connections-mobile.png) · [nội dung cộng đồng](docs/preview/connections-feature.png) · [IN ACTION trên mobile](docs/preview/connections-in-action-mobile.png).

## Tài nguyên tham chiếu

Hình ảnh, logo và video lấy từ trang chủ KOI Thé theo yêu cầu đối chiếu giao diện; quyền đối với những tài nguyên này thuộc chủ sở hữu tương ứng. Không hotlink tài nguyên và không đưa API key của website mẫu vào source. Font Red Hat Display dùng giấy phép SIL Open Font License, kèm tại `wwwroot/fonts/OFL.txt`.

## Kiểm tra các trang nội dung

Build/publish Release không có lỗi hoặc cảnh báo. Kiểm tra Playwright gồm tài nguyên CSS/JS/ảnh có nội dung, không tràn ngang ở 390/768/1024/1440px, chọn nguyên liệu/thương hiệu/lịch sử, đổi đồ uống và danh mục, lọc/phân trang News, đủ 13 trang chi tiết và ID không tồn tại trả 404, sao chép liên kết, chuyển bốn cam kết theo cả hai chiều, carousel nhân sự/cộng đồng và chế độ giảm chuyển động. Trình phát YouTube được kiểm tra URL và thao tác Play/Back; phát video vẫn phụ thuộc kết nối tới YouTube.

About đã đối chiếu vị trí các cột ly trà và góc xoay ảnh theo cả hai chiều cuộn với trang mẫu. Kiểm tra thêm chuyển mờ nguyên liệu, chọn đủ ba thành phần, tự chuyển/Pause, đủ chín mốc lịch sử và quay vòng, đổi liên tục 390/768/1024/1440/1920px không tạo lặp carousel, ảnh bìa rời màn hình trên mobile và chế độ giảm chuyển động. Our Tea và Recruit cũng được kiểm tra lại sau sửa quy tắc hiển thị tablet/mobile dùng chung.

Our Tea đã đối chiếu bố cục tiêu đề, ảnh tròn và danh sách New Drinks trên desktop. Kiểm tra bốn lựa chọn, chuyển mờ ảnh/nền đồng bộ, tự chuyển/Pause, bảy danh mục và phần xem trước của 54 món, vuốt/chấm chọn New Drinks trên điện thoại, đổi ảnh theo cuộn, chọn danh mục mobile, đổi kích thước liên tục và giảm chuyển động.

News đã đối chiếu vị trí/kích thước tiêu đề, tin nổi bật, năm hàng tin và phân trang ở 390/768/1440px. Khôi phục SVG More, cách xếp metadata/tiêu đề/ảnh theo từng màn hình, nhịp hiện chữ và màu hover/focus. Kiểm tra lại lọc tin, ba trang danh sách, 13 bài chi tiết, sao chép liên kết, responsive và giảm chuyển động. Ảnh xem trước: [desktop](docs/preview/news-desktop.png) · [mobile](docs/preview/news-mobile.png).

Recruit đã đối chiếu bố cục và độ mờ/vị trí của bốn pha ứng tuyển với mẫu ở 1440/390px, gồm cả cuộn ngược. Kiểm tra thứ tự hiện chữ, ảnh desktop/tablet/mobile, đổi kích thước liên tục không tạo lặp ScrollTrigger, hover/focus, chặn focus khi vùng ứng tuyển không hoạt động và bố cục tĩnh khi giảm chuyển động. Liên kết ứng tuyển được kiểm tra URL; không gửi hồ sơ.

Impact dùng Observer có sẵn trong ScrollTrigger để xử lý wheel/touch. Chữ, ảnh và chấm chọn đồng bộ theo nhịp chuyển của mẫu; cuộn bình thường được trả lại ở đầu/cuối vùng giữ màn hình. Đối chiếu vị trí chữ ngang, độ mờ banner và lớp phủ ở 1440/390/1024px theo cả hai chiều. Kiểm tra thêm sáu lượt chuyển cam kết xuôi/ngược, chấm chọn, cử chỉ vuốt và mở More thật trên thiết bị cảm ứng, liên kết Back giữ cam kết, đổi kích thước, màu header và bố cục tĩnh đủ bốn thẻ khi giảm chuyển động.

Responsibility đã đối chiếu bố cục và mốc kích hoạt hiệu ứng ở 1440/390/1024px. Vị trí chữ, độ mờ phụ đề/mũi tên và thứ tự hai dòng OUR BELIEF được lấy mẫu theo thời gian trên desktop/mobile. Kiểm tra bốn số liệu khởi chạy cùng lúc, ba ảnh nguồn nguyên liệu hiện theo cuộn, chạm Play/Back thật, căn giữa và khóa/mở cuộn video trên mobile, đổi kích thước khi video đang mở, giảm chuyển động và các trang chi tiết dùng chung CSS/JavaScript. Nút Back từ Responsibility đưa desktop về đúng cam kết đầu tiên trên Impact thay vì đầu trang. Video dùng trình phát YouTube gốc; việc phát nội dung phụ thuộc kết nối tới YouTube.

Responsibility bắt đầu ở ảnh bìa khi mở lại hoặc tải lại, kể cả lúc JavaScript tải chậm. Đã kiểm tra vào từ More, lịch sử Back, cuộn bình thường, giảm chuyển động và liên kết có fragment; các trang khác vẫn giữ cách khôi phục vị trí cuộn của trình duyệt.

Precision tương ứng cam kết thứ hai “PRECISION BUILDS TRUST” trên [trang mẫu](https://koithe.com/en/Precision.php). Đã đối chiếu bố cục, độ mờ nền/tiêu đề và vị trí chữ theo cả hai chiều cuộn ở 1440/390/1024px. Kiểm tra đổi kích thước không tạo lặp cảnh giữ màn hình, ba ảnh hiện theo cuộn, chạm Play/Back, khóa/mở cuộn video và tiếp tục animation sau khi đóng, đổi kích thước khi video đang mở, Back về đúng cam kết và mở lại trang ở ảnh bìa kể cả khi JavaScript tải chậm. Khi giảm chuyển động, bỏ vùng giữ màn hình và hiển thị tĩnh đủ sáu đoạn chữ cùng ba ảnh. Responsibility được kiểm tra lại sau khi dùng chung xử lý video và hiện ảnh. Video vẫn dùng trình phát YouTube gốc.

People tương ứng cam kết thứ ba “PEOPLE BEHIND THE CRAFT” trên [trang mẫu](https://koithe.com/en/people.php). Đã đối chiếu bố cục ở 1440/390/1024px và nhịp hiện tiêu đề/phụ đề trên desktop/mobile. Kiểm tra đủ năm thẻ Foundations theo cả hai chiều và quay vòng, chấm chọn, vuốt/kéo bằng cảm ứng thật, Moments tự chạy với Pause/Play, dừng khi ra khỏi màn hình hoặc giảm chuyển động, đổi liên tục 390–2560px không tạo lặp carousel hay tràn ngang, liên kết Recruit và Back về cam kết People. Mở mới/tải lại luôn bắt đầu ở ảnh bìa, kể cả khi JavaScript tải chậm. Precision được kiểm tra lại sau thay đổi JavaScript dùng chung.

Connections tương ứng cam kết thứ tư “CONNECTIONS THAT GO BEYOND THE CUP” trên [trang mẫu](https://koithe.com/en/connections.php). Đã đối chiếu bố cục ở 1440/390/1024px, nhịp hiện tiêu đề và độ mờ ảnh cộng đồng tại bốn khung hình. GSAP xử lý chuyển mờ 1,15 giây, thanh thời gian 3 giây và cử chỉ qua Observer; Swiper xử lý bảy thẻ IN ACTION căn giữa, quay vòng và vuốt. Kiểm tra Pause/Play, ảnh/chữ/chấm chọn đồng bộ, cảm ứng thật trong cả hai chế độ chuyển động, đổi 390–2560px không tràn ngang/tạo lặp carousel, chuyển sang giảm chuyển động giữa lúc đang đổi ảnh, Back đúng cam kết và mở lại trang ở ảnh bìa khi JavaScript tải chậm. People được kiểm tra lại sau thay đổi JavaScript dùng chung.

Store tương ứng [STORE trên trang mẫu](https://koithe.com/en/store.php). Dữ liệu công khai chụp ngày 31/10/2026 được lưu trong `Data/Stores.json`, gồm 17 quốc gia, 126 lựa chọn địa điểm và sáu thương hiệu; không cần database hay API key. Giữ thứ tự danh sách riêng của từng quốc gia/thành phố, query `c/s/b/page` và chín mục mỗi trang. Dữ liệu này là bản tĩnh, không tự đồng bộ với website gốc. Bốn mục dùng chung một URL ảnh cũ không còn tải được đã chuyển sang ảnh mặc định của mẫu.

Store dùng GSAP có sẵn cho tiêu đề và dialog HTML gốc để xem ảnh. Ảnh, logo và font Noto Sans SC được lưu local; font chỉ chứa các ký tự cần cho bản dữ liệu này, kèm giấy phép SIL OFL trong `wwwroot/fonts/store/OFL.txt`. Menu/footer/gấu nổi và các liên kết quốc gia trong About đều mở trang Store local.

[Video chạy thử Store](docs/preview/store-animation.mp4): tiêu đề, chọn Vietnam/Ha Noi và mở/đóng ảnh. Ảnh xem trước: [desktop](docs/preview/store-desktop.png) · [mobile](docs/preview/store-mobile.png) · [Vietnam](docs/preview/store-vietnam.png) · [ảnh cửa hàng trên mobile](docs/preview/store-photo-mobile.png).

Store đã đối chiếu vị trí/kích thước tiêu đề, bộ lọc, thẻ đầu tiên và phân trang ở 1440/390/1024px. Kiểm tra bản publish Release gồm 154 danh sách lọc/phân trang theo dữ liệu mẫu, query không hợp lệ, liên kết giữ/xóa bộ lọc đúng cấp, hộp ảnh với Back/Escape/chạm nền và trả focus, chuyển mờ khi mở/đóng, giảm chuyển động, font CJK, không tràn ngang ở 360–1920px và tải lại bắt đầu ở đầu trang. Build/publish không có lỗi hoặc cảnh báo; các liên kết Store trong About cũng đã được kiểm tra.

Contact tương ứng [CONTACT trên trang mẫu](https://koithe.com/en/contact.php). Giữ bố cục, nội dung và hai nhóm trường; GSAP có sẵn xử lý tiêu đề trượt lên sau một giây và chuyển mờ 0,5 giây khi đổi form. Select, radio và ngày dùng điều khiển gốc của trình duyệt, hỗ trợ bàn phím/cảm ứng và giữ nội dung đã nhập khi chuyển nhóm. Menu/footer đã nối tới Contact local.

Theo yêu cầu, Contact chỉ hoàn thiện giao diện; không gửi email, không lưu dữ liệu và không giả lập thông báo gửi thành công. Nút gửi và ô Verify Code được vô hiệu hóa, kèm chú thích trên trang. Không thêm package NuGet, dịch vụ mail hoặc khóa CAPTCHA.

[Video chạy thử Contact](docs/preview/contact-animation.mp4): tiêu đề, đổi Contact/Business Partnership, chọn quốc gia/ngân sách và nhập ngày. Ảnh xem trước: [desktop](docs/preview/contact-desktop.png) · [mobile](docs/preview/contact-mobile.png) · [Business Partnership](docs/preview/contact-business.png) · [Business Partnership trên mobile](docs/preview/contact-business-mobile.png).

Contact đã đối chiếu tiêu đề, cột nội dung, thanh chọn form và vị trí các trường ở 1440/390/1024px. Kiểm tra bản publish Release gồm hai chế độ ở 360–2560px, dropdown/radio/ngày, giữ dữ liệu khi đổi nhóm, chuyển liên tục, bàn phím và cảm ứng thật, giảm chuyển động giữa lúc chuyển form, tải lại ở đầu trang kể cả JavaScript tải chậm và không phát sinh POST. Chữ mờ/chú thích được tăng độ tương phản; quét axe WCAG 2/2.1 A/AA cho cả hai chế độ trên desktop/mobile không phát hiện vi phạm tự động. Build/publish không có lỗi hoặc cảnh báo.
