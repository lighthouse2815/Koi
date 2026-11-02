# Koi Thé MVC

Ứng dụng website KOI Thé xây dựng bằng ASP.NET Core MVC, Razor Views, Entity Framework Core và PostgreSQL. Dự án gồm website công khai, trang quản trị nội dung, quản lý đồ uống, chi nhánh và đơn hàng.

## Yêu cầu

- .NET 10 SDK
- Docker và Docker Compose
- PostgreSQL 17 (có thể chạy bằng Docker Compose)

## Cách chạy

1. Tạo file `.env` từ `.env.example` và đặt mật khẩu PostgreSQL:

   ```bash
   cp .env.example .env
   ```

2. Khởi động PostgreSQL và tạo database mới:

   ```bash
   docker compose up -d --wait
   docker compose exec postgres createdb -U koi koi_new
   ```

3. Cấu hình connection string và tài khoản quản trị bằng User Secrets:

   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=127.0.0.1;Port=55432;Database=koi_new;Username=koi;Password=YOUR_POSTGRES_PASSWORD" --project Koi.Web.csproj
   dotnet user-secrets set "Admin:Email" "admin@example.com" --project Koi.Web.csproj
   dotnet user-secrets set "Admin:Password" "YOUR_ADMIN_PASSWORD" --project Koi.Web.csproj
   ```

4. Khôi phục package và chạy ứng dụng:

   ```bash
   dotnet restore Koi.sln
   dotnet run --project Koi.Web.csproj
   ```

Mở `http://localhost:5118`. Trang quản trị nằm tại `http://localhost:5118/Admin`.

Trong môi trường Development, ứng dụng tự chạy migration và nạp dữ liệu mẫu tiếng Việt. Không đưa mật khẩu, User Secrets hoặc file `.env` vào Git.

## Kiểm tra và phát hành

```bash
dotnet build Koi.sln --configuration Release
dotnet publish Koi.Web.csproj --configuration Release --output ./artifacts/publish
```
