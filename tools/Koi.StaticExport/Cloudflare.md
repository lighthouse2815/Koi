# KOI Thé · FE tĩnh

Thư mục này đã sẵn sàng đưa lên **Cloudflare Pages**. Không cần .NET, PostgreSQL, biến môi trường hoặc API key để chạy bản demo.

| Thiết lập Cloudflare Pages | Giá trị |
| --- | --- |
| Repository | `lighthouse2815/Koi` |
| Production branch | `main` |
| Framework preset | `None` / không chọn framework |
| Build command | `exit 0` |
| Build output directory | `fe-demo` |
| Root directory | Để trống (gốc repository) |

Hướng dẫn chính thức: https://developers.cloudflare.com/pages/framework-guides/deploy-anything/

Các trang tiếng Anh nằm trong `/en/Home/.../`; tiếng Việt trong `/vi/Home/.../`. Trang `/` chọn ngôn ngữ. URL MVC cũ `/Home/...` chuyển về bản tiếng Anh nhờ `_redirects`. Có `404.html` riêng để URL không tồn tại trả đúng 404.

Bản xuất giữ giao diện, animation, ảnh/video, chuyển VI/EN, 13 bài News, 54 đồ uống, 7 danh mục, 4 mục New Drinks, 22 thực đơn và 1.211 mục cửa hàng. Lọc quốc gia/khu vực/thương hiệu, phân trang và xem ảnh cửa hàng chạy bằng JavaScript với dữ liệu trong `data/`. Dữ liệu chụp tại thời điểm xuất, thể hiện trong `static-site.json`.

Phần quản trị không được xuất. Contact vẫn không gửi form. Khi cần cập nhật nội dung, sửa trong MVC rồi xuất lại bằng công cụ tại `tools/Koi.StaticExport`; bản demo không tự đồng bộ database. Ảnh thực đơn/poster và địa chỉ nước ngoài giữ nội dung gốc.

Không sửa tay các file HTML/JSON hoặc tài nguyên trong thư mục này: lần xuất tiếp theo sẽ tạo lại chúng.
