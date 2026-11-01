# KOI Thé · ASP.NET Core MVC

Dựng lại giao diện [KOI Thé](https://koithe.com/en/main.php) bằng ASP.NET Core MVC và Razor View. Hoàn thành từng trang để duyệt trước khi làm trang kế tiếp.

## Chạy project

Yêu cầu .NET 10 SDK và PostgreSQL. Có sẵn Docker Compose dùng PostgreSQL 17; không cần Node.js/npm để chạy web.

```bash
dotnet restore Koi.sln
dotnet tool restore
# Lần đầu: sao chép .env.example thành .env và đặt POSTGRES_PASSWORD.
docker compose up -d --wait
# Tạo database mới một lần; không dùng database cũ.
docker compose exec postgres createdb -U koi koi_new
```

Cấu hình kết nối và tài khoản quản trị đầu tiên bằng User Secrets (thay các giá trị YOUR_ trước khi chạy):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=127.0.0.1;Port=55432;Database=koi_new;Username=koi;Password=YOUR_POSTGRES_PASSWORD" --project Koi.Web.csproj
dotnet user-secrets set "Admin:Email" "YOUR_ADMIN_EMAIL" --project Koi.Web.csproj
dotnet user-secrets set "Admin:Password" "YOUR_ADMIN_PASSWORD" --project Koi.Web.csproj
dotnet run --project Koi.Web.csproj
```

Mật khẩu quản trị cần ít nhất 12 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt. User Secrets và `.env` nằm ngoài Git. Tại máy đang làm việc, PostgreSQL và tài khoản quản trị đã được cấu hình bằng User Secrets ID `koi-web-development`.

Mở **http://localhost:5118** để xem trang bìa; chọn **TIẾNG VIỆT** hoặc **ENGLISH** để vào `/Home/Index`. Quản trị ở **http://localhost:5118/Admin**; người chưa đăng nhập sẽ được chuyển tới trang đăng nhập. Có thể mở `Koi.sln` trong Visual Studio hoặc dùng profile `http`.

```bash
dotnet build Koi.sln --configuration Release
dotnet publish Koi.Web.csproj --configuration Release --output ./artifacts/publish
```

Development tự áp dụng migration và tạo tài khoản được cấu hình nếu chưa có. Migration khởi tạo nạp dữ liệu mẫu News, Store, Menu và đồ uống một lần; dữ liệu chỉnh sửa không bị nạp lại khi khởi động. Bộ migration này chỉ dành cho database mới, không áp dụng lên database dùng chuỗi migration cũ. Production mặc định không khởi tạo database: cung cấp `ConnectionStrings__DefaultConnection`, áp dụng migration bằng `dotnet ef database update`, hoặc bật `Database__Initialize=true` trong lần khởi tạo có `Admin__Email`/`Admin__Password`. Sau đó tắt cờ khởi tạo. Các biến môi trường tương ứng được hỗ trợ khi triển khai; không đưa mật khẩu vào `appsettings.json`.

### Thiết lập đặt hàng cho chuỗi chi nhánh

1. Khởi động lại ứng dụng để áp dụng các migration mới trong môi trường Development, sau đó đăng nhập `/Admin`.
2. Website dùng thương hiệu KOI Thé và dữ liệu mẫu các chi nhánh tại Việt Nam.
3. Trên trang **Bán hàng**, tìm chi nhánh và bật nhận đơn; đặt giá chung cho các đồ uống muốn bán. Đồ uống và danh mục phải được xuất bản. Giá để trống có nghĩa là món chưa bán.
4. Khách đặt món tại `/Order/Index`, chọn nhận ở chi nhánh hoặc giao hàng và cung cấp số điện thoại. Trang **Đơn hàng** trong quản trị cho phép xem theo chi nhánh và chuyển trạng thái đơn.

Giá đang dùng đồng Việt Nam và dùng chung cho mọi chi nhánh. Đơn được lưu trong PostgreSQL; nhân viên cần theo dõi trang quản trị để liên hệ xác nhận, và phí giao hàng được thỏa thuận riêng. Chưa có cổng thanh toán trực tuyến hoặc gửi thông báo tự động. Giỏ hàng dùng session trong bộ nhớ, có thể mất khi ứng dụng khởi động lại; màn hình đặt hàng hiện dùng tiếng Việt. Bản FE tĩnh `fe-demo` không hỗ trợ đặt hàng.

### ID và database mới

Các khóa của nội dung, user và role dùng `Guid` (`uuid` trong PostgreSQL); ID riêng của user claim và role claim dùng `int` theo mặc định của ASP.NET Identity. Database mới được tạo từ migration `InitialCreate`; dữ liệu mẫu có ID cố định để liên kết nhất quán.

Chuỗi migration cũ đã được thay thế và không dùng để nâng cấp database cũ. Tạo database mới theo hướng dẫn ở trên; dữ liệu từ database cũ không được tự động chuyển sang.

## Deploy riêng FE lên Cloudflare Pages

Bản FE đã xuất sẵn tại [`fe-demo`](fe-demo/README.md), có giao diện VI/EN, animation và dữ liệu công khai hiện tại. Cloudflare Pages chỉ cần phục vụ thư mục này, không cần .NET hoặc PostgreSQL.

| Thiết lập | Giá trị |
| --- | --- |
| Production branch | `main` |
| Framework preset | `None` |
| Build command | `exit 0` |
| Build output directory | `fe-demo` |
| Root directory | Để trống |

Đây là dữ liệu tại thời điểm xuất; khi sửa nội dung trong MVC cần [chạy lại công cụ xuất](tools/Koi.StaticExport/README.md) rồi commit/push bản mới. Quản trị không nằm trong FE; Contact tiếp tục không gửi form.

## MVC và quản trị đã triển khai

- **Menu:** Quản trị thực đơn Việt Nam; trang Menu đọc database và mở ảnh trong tab mới.
- **News:** PostgreSQL + EF Core lưu tiêu đề, danh mục, ngày, HTML, ảnh/video, bản nháp/xuất bản, tin nổi bật và lựa chọn hiển thị ở trang chủ. News, chi tiết tin và carousel tin trang chủ đều đọc database.
- **Quản trị News:** đăng nhập/đăng xuất bằng ASP.NET Core Identity; tìm kiếm, phân trang, thêm/sửa/xóa, xem trước cả bản nháp và tải ảnh JPG/PNG/WebP tối đa 5 MB. Bản nháp chỉ quản trị viên xem được. Có kiểm tra dữ liệu phía server, chống CSRF, loại mã HTML không an toàn và phát hiện sửa đè từ bản cũ.
- **Chi nhánh:** Quản lý và nhận đơn cho các chi nhánh KOI Thé tại Việt Nam; các cửa hàng KOI Café Việt Nam cũ được lưu riêng.
- **Đồ uống:** PostgreSQL lưu 54 món, 7 danh mục và 4 mục New Drinks ban đầu. Quản trị thêm/sửa/xóa, mô tả/khu vực/ảnh, thứ tự, xuất bản, ảnh và biểu tượng danh mục, thẻ trang chủ và nền New Drinks. Our Tea và carousel danh mục trang chủ đọc database. Danh mục ẩn sẽ ẩn toàn bộ món bên trong và thẻ trang chủ.
- **Đặt hàng:** khách chọn chi nhánh, tùy chọn đồ uống và gửi đơn; quản trị viên cấu hình bán hàng và xử lý đơn. Thanh toán khi nhận hàng, chưa tích hợp thanh toán trực tuyến.
- **Contact:** giữ giao diện không gửi form theo yêu cầu.

Ảnh tải lên lưu trong `wwwroot/uploads`, không đưa vào Git hoặc bản publish. Khi triển khai cần giữ thư mục uploads riêng cùng với dữ liệu PostgreSQL. Xóa bài viết chỉ xóa bản ghi, giữ các ảnh đã lưu.

## Trang chủ

Các phần ghi chép và kiểm thử giao diện bên dưới phản ánh những phiên bản trước khi thu gọn phạm vi về Việt Nam; dữ liệu nước ngoài trong database đã được xóa theo yêu cầu.

- Giao diện có tiếng Việt và tiếng Anh; ngôn ngữ được chọn ở trang bìa hoặc nút VI/EN trong menu và chân trang.
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

Các trang nội dung đã dựng local, có bản tiếng Việt/tiếng Anh và ảnh/SVG lưu trong `wwwroot`:

| Trang | URL | Tương tác |
| --- | --- | --- |
| About | `/Home/About` | Cảnh chữ KOI/ly trà, ảnh xoay khi cuộn ngang, nguyên liệu chuyển mờ tự động, dải ảnh lịch sử xếp chéo và liên kết chi nhánh Việt Nam |
| Our Tea | `/Home/OurTea` | Dải ly và bốn nền chuyển động, New Drinks đồng bộ nền/ảnh, ảnh đồ uống đổi theo cuộn trên mobile |
| Recruit | `/Home/Recruit` | Chữ hiện lần lượt, bảng lộ trình phát triển, ảnh đội ngũ theo thiết bị và bốn pha cuộn Apply NOW |
| News | `/Home/News` | Tiêu đề xuất hiện theo mẫu, nút More chuyển động, danh sách responsive, lọc/phân trang và 13 bài chi tiết `/Home/NewsDetail/{id}` |
| Impact | `/Home/Impact` | Banner giữ màn hình, chữ chạy theo cuộn, mỗi lượt cuộn/vuốt chuyển một cam kết, hỗ trợ chấm chọn và quay lại đúng cam kết |
| Responsibility | `/Home/Responsibility` | Hai ảnh bìa trượt vào, chữ hiện theo thứ tự, bốn số liệu đếm cùng lúc, ảnh nguồn nguyên liệu hiện theo cuộn và video có Play/Back trên mobile |
| Precision | `/Home/Precision` | Banner giữ màn hình, nền tối theo cuộn, đoạn chữ chạy lên và video Play/Back trên mobile |
| People | `/Home/People` | Chữ hiện theo cuộn, năm thẻ Foundations cuộn vòng, ảnh Moments tự chạy/kéo/Pause và liên kết tuyển dụng |
| Connections | `/Home/Connections` | Ảnh bìa thu dần, OUR BELIEF hiện theo cuộn, ba nội dung cộng đồng chuyển mờ tự động và bảy thẻ IN ACTION cuộn vòng, căn giữa |
| Store | `/Home/Store` | Tiêu đề trượt lên, lọc Country/Location/Brand, chín cửa hàng mỗi trang và hộp ảnh tròn hỗ trợ bàn phím/cảm ứng |
| Contact | `/Home/Contact` | Thông tin liên hệ và biểu mẫu xem trước; chưa hỗ trợ gửi |
| Menu | `/Home/Menu` | Thực đơn Việt Nam mở trong tab mới |

Trang bìa ở `/`; trang chủ ở `/Home/Index`. Header/footer đã nối tới các trang local và hiển thị đúng tên trang đang xem. Các nút Menu trên Our Tea, footer và gấu nổi mở `/Home/Menu`; ảnh thực đơn cũng được lưu local. Contact chưa gửi form; News, Store, Menu, đồ uống và đơn hàng dùng PostgreSQL, các liên kết ứng tuyển giữ cổng tuyển dụng/email gốc.

Các trang nội dung trong bảng đã được duyệt, gồm cả Menu. Phần giao diện đã hoàn tất trong phạm vi các trang này; Contact giữ chế độ xem trước theo yêu cầu, không gửi form.

Nội dung News hiện lưu trong PostgreSQL và chỉnh sửa được qua trang quản trị; dữ liệu ban đầu được chuyển từ 13 bài của giai đoạn dựng giao diện. Video bài viết và chi tiết Impact dùng YouTube gốc. Poster của bài cuộc thi năm 2024 dùng lại ảnh của chính bài viết vì tên miền chứa ảnh nội dung cũ đã ngừng hoạt động.

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

Mã ứng dụng được gom vào `code/`; file khởi động, cấu hình dự án và tài nguyên tĩnh giữ ở gốc:

```text
code/
├── Controllers/
│   ├── HomeController.cs    # Các trang công khai
│   └── Admin/              # Tám controller quản trị
├── Views/
│   ├── Home/               # Giao diện công khai
│   ├── Admin/              # Giao diện quản trị
│   └── Shared/             # Layout, header, footer dùng chung
├── Models/
│   ├── Entities/           # Dữ liệu và cấu hình ánh xạ database
│   └── ViewModels/         # Dữ liệu và kiểm tra đầu vào cho View
├── Data/                   # DbContext, migration và dữ liệu ban đầu
├── Localization/           # Chọn nội dung theo ngôn ngữ
├── Resources/              # Tài nguyên tiếng Việt/tiếng Anh
└── Services/               # Truy vấn, nghiệp vụ quản trị và xử lý ảnh
wwwroot/                    # CSS, JavaScript, ảnh, font và video
Program.cs                  # Khởi động, routing và cấu hình View/localization
Koi.Web.csproj              # Cấu hình build và tài nguyên nhúng
```

Controller xử lý request, phản hồi và gọi service; service xử lý truy vấn, quy tắc nghiệp vụ và lưu dữ liệu. Entity giữ các trường và metadata cần cho EF Core; ViewModel phục vụ giao diện và kiểm tra đầu vào. Đăng nhập dùng trực tiếp `SignInManager` có sẵn của ASP.NET Core Identity. Dữ liệu catalog gốc dùng để khởi tạo migration nằm trong `Data/`.

Phần quản trị vẫn dùng cơ chế Area và giữ URL `/Admin/...`. `Program.cs` tìm View tại `code/Views`, tài nguyên ngôn ngữ tại `code/Resources`; các đường dẫn layout/View dùng chung đã được cập nhật. View quản trị kế thừa `_ViewImports.cshtml` chung và dùng layout qua `code/Views/Admin/_ViewStart.cshtml`. Tên tài nguyên JSON nhúng được giữ cố định trong `Koi.Web.csproj` để migration và các bản cài đặt mới tiếp tục đọc được dữ liệu. Lệnh build, chạy, test và publish ở gốc project không thay đổi. Kiểm tra sau khi chuyển vào `code/`: build/publish Release đạt, 59 test trên PostgreSQL kiểm thử mới đạt; bản publish Production kiểm tra 27 URL công khai, 32 trang quản trị, 85 liên kết local, 307 tài nguyên và chuyển VI/EN ở desktop/mobile. Không có thay đổi model cần migration. Khi thêm migration mới, dùng `dotnet ef migrations add TenMigration --output-dir code/Data/Migrations --namespace Koi.Web.Data.Migrations` để đặt file đúng thư mục.

- `code/Controllers/HomeController.cs`: các trang công khai và xử lý lỗi.
- `code/Controllers/Admin/`: đăng nhập và các chức năng quản trị nội dung.
- `code/Views/Admin/`: danh sách, form thêm/sửa/xóa và trang đăng nhập quản trị.
- `code/Views/Home/Cover.cshtml`, `code/Views/Shared/_CoverLayout.cshtml`, `wwwroot/css/cover.css`: trang bìa chọn ngôn ngữ.
- `code/Views/Home/Index.cshtml`, `_FeatureStage.cshtml`: trang chủ và phần cảnh thương hiệu dùng chung.
- `code/Views/Shared/_Layout.cshtml`: khung Razor chung.
- `code/Views/Shared/_Navigation.cshtml`, `_Footer.cshtml`: header/menu và footer.
- `wwwroot/css/site.css`, `wwwroot/js/site.js`: style và các tương tác giao diện.
- `wwwroot/js/home-animations.js`: timeline GSAP, các cảnh theo cuộn, intro và parallax.
- `wwwroot/images`, `wwwroot/videos`, `wwwroot/fonts`: tài nguyên local.

Dùng template MVC chính thức của .NET. Giai đoạn backend dùng Npgsql/EF Core cho PostgreSQL, ASP.NET Core Identity cho tài khoản và HtmlSanitizer cho nội dung HTML. GSAP 3.15.0 cùng ScrollTrigger/ScrollToPlugin được lưu trong `wwwroot/lib/gsap`, giữ thông tin bản quyền và giấy phép. Không cần npm để build hoặc chạy ứng dụng. Bỏ Bootstrap/jQuery mặc định vì trang này không sử dụng chúng.

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

Store tương ứng [STORE trên trang mẫu](https://koithe.com/en/store.php). Dữ liệu công khai chụp ngày 31/10/2026 được lưu trong `code/Data/Stores.json`, gồm 17 quốc gia, 126 lựa chọn địa điểm và sáu thương hiệu; nay được migration nhập một lần vào PostgreSQL và chỉnh sửa qua quản trị. Không cần API key. Giữ thứ tự danh sách riêng của từng quốc gia/thành phố, query `c/s/b/page` và chín mục mỗi trang. Dữ liệu không tự đồng bộ với website gốc; cập nhật trong quản trị. Bốn mục dùng chung một URL ảnh cũ không còn tải được đã chuyển sang ảnh mặc định của mẫu.

Store dùng GSAP có sẵn cho tiêu đề và dialog HTML gốc để xem ảnh. Ảnh, logo và font Noto Sans SC được lưu local; font chỉ chứa các ký tự cần cho bản dữ liệu này, kèm giấy phép SIL OFL trong `wwwroot/fonts/store/OFL.txt`. Menu/footer/gấu nổi và các liên kết quốc gia trong About đều mở trang Store local.

[Video chạy thử Store](docs/preview/store-animation.mp4): tiêu đề, chọn Vietnam/Ha Noi và mở/đóng ảnh. Ảnh xem trước: [desktop](docs/preview/store-desktop.png) · [mobile](docs/preview/store-mobile.png) · [Vietnam](docs/preview/store-vietnam.png) · [ảnh cửa hàng trên mobile](docs/preview/store-photo-mobile.png).

Store đã đối chiếu vị trí/kích thước tiêu đề, bộ lọc, thẻ đầu tiên và phân trang ở 1440/390/1024px. Kiểm tra bản publish Release gồm 154 danh sách lọc/phân trang theo dữ liệu mẫu, query không hợp lệ, liên kết giữ/xóa bộ lọc đúng cấp, hộp ảnh với Back/Escape/chạm nền và trả focus, chuyển mờ khi mở/đóng, giảm chuyển động, font CJK, không tràn ngang ở 360–1920px và tải lại bắt đầu ở đầu trang. Build/publish không có lỗi hoặc cảnh báo; các liên kết Store trong About cũng đã được kiểm tra.

Contact tương ứng [CONTACT trên trang mẫu](https://koithe.com/en/contact.php). Giữ bố cục, nội dung và hai nhóm trường; GSAP có sẵn xử lý tiêu đề trượt lên sau một giây và chuyển mờ 0,5 giây khi đổi form. Select, radio và ngày dùng điều khiển gốc của trình duyệt, hỗ trợ bàn phím/cảm ứng và giữ nội dung đã nhập khi chuyển nhóm. Menu/footer đã nối tới Contact local.

Theo yêu cầu, Contact chỉ hoàn thiện giao diện; không gửi email, không lưu dữ liệu và không giả lập thông báo gửi thành công. Nút gửi và ô Verify Code được vô hiệu hóa, kèm chú thích trên trang. Không thêm package NuGet, dịch vụ mail hoặc khóa CAPTCHA.

[Video chạy thử Contact](docs/preview/contact-animation.mp4): tiêu đề, đổi Contact/Business Partnership, chọn quốc gia/ngân sách và nhập ngày. Ảnh xem trước: [desktop](docs/preview/contact-desktop.png) · [mobile](docs/preview/contact-mobile.png) · [Business Partnership](docs/preview/contact-business.png) · [Business Partnership trên mobile](docs/preview/contact-business-mobile.png).

Contact đã đối chiếu tiêu đề, cột nội dung, thanh chọn form và vị trí các trường ở 1440/390/1024px. Kiểm tra bản publish Release gồm hai chế độ ở 360–2560px, dropdown/radio/ngày, giữ dữ liệu khi đổi nhóm, chuyển liên tục, bàn phím và cảm ứng thật, giảm chuyển động giữa lúc chuyển form, tải lại ở đầu trang kể cả JavaScript tải chậm và không phát sinh POST. Chữ mờ/chú thích được tăng độ tương phản; quét axe WCAG 2/2.1 A/AA cho cả hai chế độ trên desktop/mobile không phát hiện vi phạm tự động. Build/publish không có lỗi hoặc cảnh báo.

Menu tương ứng [MENU trên trang mẫu](https://koithe.com/en/menu.php), là trang chọn ảnh thực đơn theo quốc gia/thương hiệu thuộc Our Tea. Danh sách chụp ngày 31/10/2026 có 17 quốc gia và 22 thực đơn; giữ đúng nhãn và thứ tự, lưu ảnh gốc trong `wwwroot/menus` và mở ở tab mới như mẫu. Migration nhập danh sách ban đầu một lần vào PostgreSQL; thực đơn và ảnh chỉnh sửa qua quản trị, không tự đồng bộ giá từ trang gốc. Noto Sans SC Latin 500 được lưu local, dùng giấy phép tại `wwwroot/fonts/store/OFL.txt`.

Menu dùng lại GSAP và JavaScript trang nội dung: đường kẻ chạy trong một giây, tiêu đề bắt đầu sau 0,5 giây và danh sách hiện tiếp theo. Không thêm dependency. Nút Menu ở Our Tea/footer/gấu nổi và Back về Our Tea đều dùng URL MVC; header hiển thị OUR TEA (03/08) như trang mẫu.

[Video chạy thử Menu](docs/preview/menu-animation.mp4): hiệu ứng mở đầu, cuộn danh sách và mở thực đơn Việt Nam. Ảnh xem trước: [desktop](docs/preview/menu-desktop.png) · [mobile](docs/preview/menu-mobile.png) · [Vietnam trên mobile](docs/preview/menu-vietnam-mobile.png).

Menu đã đối chiếu bố cục ở 1440/390/1024px và lấy mẫu bảy khung hình của animation ở 1440/390px. Kiểm tra bản publish Release: đủ 17 quốc gia/22 liên kết, ảnh local trả đúng nội dung và giải mã được, mở tab thật bằng chuột/cảm ứng, bàn phím, Back về Our Tea, không tràn ngang ở 360–2560px, giảm chuyển động và tải lại bắt đầu ở đầu trang kể cả JavaScript tải chậm. Quét axe WCAG 2/2.1 A/AA không có vi phạm tự động; About, Our Tea, News, Recruit và Impact được kiểm tra lại sau khi bổ sung xử lý Menu trong JavaScript dùng chung. Build/publish không có lỗi hoặc cảnh báo.

Rà soát tổng thể ngày 31/10/2026 sau khi duyệt Menu: 27 URL gồm trang bìa, trang chủ, 12 trang nội dung và 13 bài News đều trả HTTP 200; 85 đích liên kết local hoạt động và 306 tài nguyên tham chiếu có nội dung. Kiểm tra trình duyệt cho 14 trang chính/nội dung ở 1440/390px, gồm đầu trang và footer, không phát hiện lỗi JavaScript, tài nguyên local lỗi hoặc tràn ngang. Header giữ đủ tám mục dẫn tới MVC; ID bài News không tồn tại trả 404. Build Release đạt, không có lỗi hoặc cảnh báo. Đây là lượt kiểm tra điều hướng/tài nguyên/bố cục tổng thể; các kiểm tra animation và tương tác riêng được ghi ở từng trang phía trên.

Bắt đầu giai đoạn MVC: Menu dùng `MenuViewModel`, `MenuCountry`, `MenuDocument`; dữ liệu 17 quốc gia/22 thực đơn nằm trong `code/Data/MenuCatalog.cs` và được `HomeController.Menu` truyền xuống Razor. View chỉ hiển thị dữ liệu từ Model. Đã build và đối chiếu danh sách với giao diện đã duyệt.

Giai đoạn MVC News đã kiểm tra: build/publish Release, 15 kiểm tra tích hợp trên PostgreSQL riêng (quyền truy cập, CSRF, bản nháp/xuất bản, ảnh, HTML, sửa đè và xóa), đối chiếu đủ nội dung/ảnh/video của 13 bài cũ và luồng trình duyệt đăng nhập → thêm/sửa/xem trước/xóa bản nháp → đăng xuất trên desktop/mobile. Bản publish Production đã chạy thử trang bìa, video người đọc sách, cảnh FRAGRANCE, News, Menu và đăng nhập quản trị. Ảnh quản trị: [danh sách](docs/preview/admin-news-desktop.png) · [sửa bài](docs/preview/admin-news-edit.png) · [mobile](docs/preview/admin-news-mobile.png) · [đăng nhập mobile](docs/preview/admin-login-mobile.png).

MVC Store đã kiểm tra: 26 test tích hợp đạt, đối chiếu 154 bộ lọc/phân trang với dữ liệu trang mẫu, điều hướng quản trị và form sửa trên mobile. Ảnh quản trị: [danh sách cửa hàng](docs/preview/admin-stores-desktop.png) · [sửa cửa hàng trên mobile](docs/preview/admin-store-edit-mobile.png).

MVC Menu đã kiểm tra: 32 test tích hợp đạt; trình duyệt xác nhận đủ 17 quốc gia/22 thực đơn và ảnh, mở tab mới bằng chuột/cảm ứng, bàn phím, bố cục 360–2560px, animation và chế độ giảm chuyển động. Ảnh quản trị: [danh sách](docs/preview/admin-menus-desktop.png) · [sửa trên mobile](docs/preview/admin-menu-edit-mobile.png).

MVC đồ uống đã kiểm tra: 43 test tích hợp đạt; so sánh đủ tên/mô tả/khu vực/ảnh của 54 món với giao diện trước khi chuyển, kiểm tra danh mục/thẻ trang chủ, ảnh/nền New Drinks, xuất bản, quan hệ, CSRF và sửa đè. Kiểm tra trình duyệt xác nhận animation, cảm ứng, đổi kích thước, giảm chuyển động, luồng thêm/sửa/xóa bản nháp thật và danh sách rỗng. Ảnh quản trị: [đồ uống](docs/preview/admin-drinks-desktop.png) · [sửa danh mục trên mobile](docs/preview/admin-category-edit-mobile.png).

## Cập nhật nội dung

Tên và logo thương hiệu trên trang chi nhánh được cấu hình bằng `Brand:Name` và `Brand:Logo` trong `appsettings.json`; có thể ghi đè qua biến môi trường `Brand__Name` và `Brand__Logo`.

| Nội dung | Đường dẫn quản trị |
| --- | --- |
| Tin tức | `/Admin/News` |
| Cửa hàng | `/Admin/Stores` |
| Quốc gia, thương hiệu và khu vực | `/Admin/Geography` (các tab trong trang) |
| Thực đơn | `/Admin/Menus` |
| Đồ uống | `/Admin/Drinks` |
| Danh mục và thẻ trang chủ | `/Admin/DrinkCategories` |
| New Drinks | `/Admin/FeaturedDrinks` |

Thêm thương hiệu → gán thương hiệu tại quốc gia → thêm khu vực nếu cần → thêm cửa hàng. Khu vực phải thuộc đúng quốc gia; không xóa được quốc gia/khu vực/thương hiệu hoặc danh mục còn dữ liệu sử dụng. Dùng tùy chọn hiển thị để tạm ẩn cửa hàng, thực đơn, món hoặc danh mục. Số thứ tự nhỏ hiển thị trước, ID phân xử khi trùng thứ tự. Cửa hàng giữ ba thứ tự riêng như trang mẫu.

Ảnh mới nhận JPG/PNG/WebP tối đa 5 MB mỗi tệp; giữ nguyên SVG gốc đã có. Chỉnh sửa không bắt buộc tải lại ảnh. Các trang nhập liệu có kiểm tra phía server, chống CSRF, yêu cầu quyền Admin và phát hiện bản sửa cũ. Khi gặp thông báo dữ liệu đã thay đổi, tải lại trang sửa trước khi lưu tiếp.

Kiểm tra nghiệm thu MVC ngày 31/10/2026: build/publish Release không có lỗi/cảnh báo; 43 test tích hợp đạt cả trên database kiểm thử đang dùng và một database mới tạo từ đầu. Khởi tạo migration và Identity được khóa bằng advisory lock của PostgreSQL để nhiều tiến trình không tạo trùng migration/role; không ghi đè mật khẩu tài khoản đã tồn tại. EF xác nhận không có thay đổi model thiếu migration.

Bản publish chạy ở môi trường Production đã kiểm tra 27 URL, 85 đích liên kết local, 306 tài nguyên có nội dung và 14 trang trên desktop/mobile; không phát hiện lỗi JavaScript, tài nguyên local lỗi hoặc tràn ngang. Trang bìa, video người đọc sách và cảnh FRAGRANCE được chạy thử lại; 13 bài News giữ nguyên nội dung/ảnh/video và luồng quản trị tin tức vẫn hoạt động. Dữ liệu website sau kiểm tra giữ 13 bài, 1.211 mục cửa hàng, 22 thực đơn, 7 danh mục, 54 món và 4 mục New Drinks; các bản nháp thử qua trình duyệt đã được xóa.

Các phần MVC và quản trị trong phạm vi website giới thiệu đã hoàn tất. Contact vẫn không gửi/lưu form theo yêu cầu trước đó. Dự án chạy local; chưa triển khai lên hosting/domain công khai.

## Tiếng Việt và tiếng Anh

Trang bìa thay lựa chọn tiếng Trung bằng **TIẾNG VIỆT**, giữ **ENGLISH**; cả hai mở website local. Chuyển ngôn ngữ trong menu hoặc chân trang sẽ giữ nguyên trang và các bộ lọc/phân trang hiện tại. Lựa chọn được lưu bằng cookie trong một năm. Website dùng localization và tài nguyên `.resx` có sẵn của ASP.NET Core, không cần dịch vụ dịch hoặc API key. Có thể mở trực tiếp một trang bằng `?culture=vi` hoặc `?culture=en` khi chưa có cookie lựa chọn.

- `code/Resources/Localization/SiteText.vi.resx`: bản dịch giao diện, nội dung các trang tĩnh và nhãn tương tác.
- `code/Resources/Localization/SiteText.resx`: nội dung tiếng Anh tương ứng; các khóa chỉ khác hoa/thường được phân biệt bằng hậu tố `__text`.
- `code/Localization/SiteText.cs`: xác định ngôn ngữ và định dạng ngày.
- Giao diện chuyển VI/EN; nội dung từ database dùng các trường gốc như `Name` và `Title` ở cả hai ngôn ngữ. Database mới không có các cột nội dung tiếng Việt riêng.

Tên riêng thương hiệu và ảnh thực đơn/poster gốc được giữ nguyên. Video và các hình nhận diện thương hiệu vẫn dùng tài nguyên đã duyệt. Contact tiếp tục không gửi form.

Kiểm tra phiên bản song ngữ: build/publish Release; 59 test tích hợp trên PostgreSQL mới, gồm chọn ngôn ngữ/CSRF, giữ URL và bộ lọc, chặn chuyển hướng ngoài, nội dung dịch từ máy chủ, lưu hai ngôn ngữ và HTML an toàn. Đã kiểm tra 13 trang ở 1440/390px, điều khiển animation và chuyển ngôn ngữ; ảnh xem trước: [trang chọn ngôn ngữ](docs/preview/languages-1440.png) · [Our Tea tiếng Việt](docs/preview/ourtea-vi-1440.png) · [Contact trên mobile](docs/preview/contact-vi-390.png).

## Cửa sổ laptop nhỏ và responsive

Phần bìa và các cảnh giữ màn hình tính kích thước theo chiều cao cửa sổ: nút điều khiển không lọt dưới màn hình, ảnh/chữ O thu cùng tỷ lệ, thẻ đồ uống giữ đủ tên và khoảng cách với nút chuyển. Menu vừa cả chiều rộng lẫn chiều cao. About và New Drinks tự giãn theo nội dung; mô tả lịch sử dùng cơ chế autoHeight có sẵn của Swiper. Bìa Precision vừa vùng nhìn khi được giữ trong lúc cuộn.

Ma trận kiểm tra gồm 15 URL, VI/EN và 11 kích thước: 360×640, 390×844, 844×390, 768×1024, 1024×600, 1093×550, 1280×600, 1366×650, 1536×720, 1440×900, 1920×1080. Kiểm tra hồi quy còn bổ sung 1280×800 và 1440×850, nội dung bị cắt, nút chuyển đồ uống, menu, chín slide lịch sử, thay đổi kích thước khi đang cuộn và giảm chuyển động.

Ảnh sau khi sửa: [đồ uống ở cửa sổ laptop 1366×650](docs/preview/laptop-drinks-1366x650.png) · [lịch sử About ở 844×390](docs/preview/about-history-844x390.png).
