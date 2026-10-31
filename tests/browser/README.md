# Kiểm tra màn hình và chiều cao cửa sổ

`responsive.cjs` kiểm tra 13 kích thước cửa sổ, cả VI/EN, bằng Playwright và Chromium/Chrome. Kiểm tra cả nội dung bị cắt trong vùng giữ màn hình, nút bị che, cảnh Fragrance/ly trà bung, tên đồ uống chồng lên điều khiển và chín mô tả lịch sử; kiểm tra tràn ngang riêng lẻ không phát hiện được những lỗi này.

Chạy website MVC ở cổng 5118. Dùng Playwright đã cài trong môi trường; nếu chưa có, cài vào thư mục cache riêng:

```bash
npm install --prefix "$HOME/.cache/koi-browser" playwright
NODE_PATH="$HOME/.cache/koi-browser/node_modules" npx --prefix "$HOME/.cache/koi-browser" playwright install chromium
NODE_PATH="$HOME/.cache/koi-browser/node_modules" node tests/browser/responsive.cjs
```

Có thể đặt `KOI_TEST_URL` để đổi địa chỉ web và `KOI_BROWSER_EXECUTABLE` để dùng Chrome có sẵn. Kiểm tra không đăng nhập quản trị, không gửi form hoặc sửa dữ liệu.
