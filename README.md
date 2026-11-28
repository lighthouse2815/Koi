# Koi Thé Blazor

Phiên bản KOI Thé dùng Blazor và PostgreSQL. Dự án nằm trên nhánh `blazor` và có thể chạy từ một thư mục riêng.

## Yêu cầu

- .NET 10 SDK
- Docker và Docker Compose
- PostgreSQL 17 (có thể chạy bằng Docker Compose)

## Cách chạy

Clone nhánh Blazor và mở terminal trong thư mục chứa `Koi.Blazor.sln`, `Koi.Blazor.csproj` và `compose.yaml`.

   ```bash
   git clone --branch blazor https://github.com/lighthouse2815/Koi.git Koi.Blazor
   cd Koi.Blazor
   ```

1. Tạo `.env` từ mẫu và điền `POSTGRES_PASSWORD` cùng `Admin__Password`. Mật khẩu quản trị cần có ít nhất 12 ký tự, chữ hoa, chữ thường, số và ký tự đặc biệt.

   ```bash
   cp .env.example .env
   ```

2. Khởi động PostgreSQL. Docker Compose tạo database `koi_blazor` trên cổng `55433`.

   ```bash
   docker compose up -d --wait
   ```

3. Chạy ứng dụng. Ứng dụng áp dụng migration và tạo tài khoản quản trị khi khởi động ở môi trường Development.

   ```bash
   dotnet run
   ```

Để tải ảnh mới lên Cloudinary khi phát triển, điền `Cloudinary__CloudName`, `Cloudinary__ApiKey` và `Cloudinary__ApiSecret` vào `.env` ở thư mục gốc theo mẫu `.env.example`. Dùng một dòng `KEY=value` cho mỗi giá trị và khởi động lại ứng dụng sau khi sửa file.

Có thể dùng user secrets để cấu hình Cloudinary. Giá trị từ user secrets hoặc biến môi trường được ưu tiên hơn `.env`.

```bash
dotnet user-secrets set "Cloudinary:CloudName" "<cloud-name>"
dotnet user-secrets set "Cloudinary:ApiKey" "<api-key>"
dotnet user-secrets set "Cloudinary:ApiSecret" "<api-secret>"
```

Trong môi trường triển khai, có thể dùng các biến môi trường `Cloudinary__CloudName`, `Cloudinary__ApiKey` và `Cloudinary__ApiSecret`. Ảnh đã lưu trước đó vẫn sử dụng đường dẫn hiện có; các file trong `wwwroot/uploads` cần được giữ lại.

Các biểu mẫu quản trị yêu cầu JavaScript để tải ảnh trực tiếp từ trình duyệt lên Cloudinary. Server cấp chữ ký. Khi tải ảnh thành công, trình duyệt gửi URL HTTPS do Cloudinary trả về để server lưu. Ảnh đã tải lên nhưng biểu mẫu không được lưu có thể còn trong Cloudinary và cần được dọn dẹp khi phù hợp.

Mở trình duyệt tại `http://localhost:5119`. Đăng nhập tại `http://localhost:5119/Admin` bằng `admin@koi.local` và mật khẩu quản trị đã đặt trong `.env`.

