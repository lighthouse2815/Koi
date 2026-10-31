# Xuất FE tĩnh cho Cloudflare Pages

Công cụ dùng lại HTML do Razor MVC tạo ra và dữ liệu công khai hiện tại trong PostgreSQL. Kết nối database chỉ đọc; không chạy migration, không tạo tài khoản và không ghi dữ liệu. Không xuất Admin, bản nháp, cấu hình, mật khẩu hoặc token CSRF. Thư viện phân tích HTML AngleSharp đã có qua HtmlSanitizer của project; không thêm package NuGet.

## Xuất

Chạy website MVC bằng cấu hình PostgreSQL/User Secrets hiện có. Tại gốc repository:

```bash
dotnet run --project Koi.Web.csproj
```

Trong terminal khác, vẫn tại gốc repository:

```bash
dotnet run --project tools/Koi.StaticExport/Koi.StaticExport.csproj --configuration Release -- --source http://localhost:5118 --output fe-demo
python3 tools/Koi.StaticExport/verify_export.py
```

Công cụ đọc kết nối từ User Secrets của web, hoặc `ConnectionStrings__DefaultConnection`. Nguồn HTTP phải là website dùng cùng database. Xuất cả `en` và `vi`, đủ bài đã xuất bản, trang nội dung và dữ liệu lọc/phân trang. Nếu dữ liệu trên HTTP và database không khớp, quá trình dừng trước khi thay bản xuất cũ.

Đầu ra cố định là `fe-demo` ở gốc. Công cụ chỉ thay thư mục có `static-site.json` của chính nó; bản cũ được chuyển vào `artifacts/static-export` để có thể kiểm tra lại. Ảnh upload chỉ được sao chép khi có tham chiếu trong nội dung công khai; không sao chép cả thư mục uploads.

Hướng dẫn [Cloudflare.md](Cloudflare.md) được sao chép tự động thành `fe-demo/README.md` mỗi lần xuất. Cấu hình trên Cloudflare Pages: branch `main`, framework `None`, build `exit 0`, output `fe-demo`, root để trống. Bản xuất đã chứa `index.html`, dữ liệu và toàn bộ tài nguyên cần thiết; Cloudflare không cần build .NET hay kết nối database.

## Chạy thử bằng Cloudflare

```bash
npx wrangler@4 pages dev fe-demo --port 5119 --compatibility-date 2026-10-30
```

Mở `http://localhost:5119`. Chạy qua HTTP để trình duyệt tải được JSON lọc cửa hàng/tin tức. Không mở HTML trực tiếp bằng `file://`.

Các file kiểm tra được đặt ngoài output. `verify_export.py` dùng thư viện chuẩn Python, kiểm tra liên kết/tài nguyên, dữ liệu, giới hạn file của Pages và bảo đảm không có form POST hoặc mã server trong output. Những phần chuyển động và tương tác được kiểm tra thêm bằng trình duyệt.

## Cơ chế

- HTML cho các trang và chi tiết tin được chụp từ website đang chạy, nên giữ đúng Razor, CSS và nhãn VI/EN hiện tại.
- Thẻ cửa hàng và tin tức cũng được chụp từ MVC; JavaScript chỉ chọn/sắp xếp các thẻ đã xuất, giữ thứ tự riêng theo quốc gia và khu vực.
- Những script giao diện có sẵn chạy sau khi danh sách đã được dựng để sự kiện ảnh/modal và animation tiếp tục hoạt động.
- Tài nguyên thường được sao chép từ `wwwroot`; ảnh upload chỉ sao chép khi được trang hoặc thẻ công khai sử dụng.
- `fe-demo`, mã công cụ và các bản xuất tạm trong `artifacts` được loại khỏi nội dung publish MVC, nên hai bản chạy độc lập.
