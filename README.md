# Koi Thé MVC

Ứng dụng website KOI Thé xây dựng bằng ASP.NET Core MVC, Razor Views, Entity Framework Core và PostgreSQL. Dự án gồm website công khai, trang quản trị nội dung, quản lý đồ uống, chi nhánh và đơn hàng.

## Yêu cầu

- .NET 10 SDK
- Docker và Docker Compose
- PostgreSQL 17 (có thể chạy bằng Docker Compose)

## Cách chạy

Mở **Terminal** tại thư mục gốc của project, tức thư mục chứa `Koi.sln`, `Koi.Web.csproj` và `compose.yaml`. Chạy các lệnh sau trong chính thư mục đó:

1. Khởi động PostgreSQL và tạo database mới:

   ```bash
   docker compose up -d --wait
   docker compose exec postgres createdb -U koi koi_new
   ```

2. Khôi phục package và chạy ứng dụng:

   ```bash
   dotnet run
   ```

Để tải ảnh mới lên Cloudinary, cấu hình thông tin tài khoản bằng user secrets trước khi chạy ứng dụng:

```bash
dotnet user-secrets set "Cloudinary:CloudName" "<cloud-name>"
dotnet user-secrets set "Cloudinary:ApiKey" "<api-key>"
dotnet user-secrets set "Cloudinary:ApiSecret" "<api-secret>"
```

Trong môi trường triển khai, có thể dùng các biến môi trường `Cloudinary__CloudName`, `Cloudinary__ApiKey` và `Cloudinary__ApiSecret`. Ảnh đã lưu trước đó vẫn sử dụng đường dẫn hiện có; các file trong `wwwroot/uploads` cần được giữ lại.

Mở trình duyệt tại `http://localhost:5118`; trang quản trị nằm tại `http://localhost:5118/Admin`.

Đăng nhập quản trị bằng `admin@koi.local` / `KoiDev@2026!`.

