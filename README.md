# Koi Thé MVC

Ứng dụng website KOI Thé xây dựng bằng ASP.NET Core MVC, Razor Views, Entity Framework Core và PostgreSQL. Dự án gồm website công khai, trang quản trị nội dung, quản lý đồ uống, chi nhánh và đơn hàng.

## Yêu cầu

- .NET 10 SDK
- Docker và Docker Compose
- PostgreSQL 17 (có thể chạy bằng Docker Compose)

## Cách chạy

1. Khởi động PostgreSQL và tạo database mới:

   ```bash
   docker compose up -d --wait
   docker compose exec postgres createdb -U koi koi_new
   ```

2. Khôi phục package và chạy ứng dụng:

   ```bash
   dotnet restore Koi.sln
   dotnet run --project Koi.Web.csproj
   ```

Connection string và tài khoản quản trị Development đã có sẵn trong `appsettings.Development.json`. Đăng nhập quản trị bằng `admin@koi.local` / `KoiDev@2026!`. Mở `http://localhost:5118`; trang quản trị nằm tại `http://localhost:5118/Admin`.

Trong môi trường Development, ứng dụng tự chạy migration và nạp dữ liệu mẫu tiếng Việt. Không dùng cấu hình này cho production.

## Kiểm tra và phát hành

```bash
dotnet build Koi.sln --configuration Release
dotnet publish Koi.Web.csproj --configuration Release --output ./artifacts/publish
```
