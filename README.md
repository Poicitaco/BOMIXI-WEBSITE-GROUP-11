# BOMIXI - Hệ thống Laptop E-Commerce Cyberpunk

Dự án được xây dựng trên nền tảng ASP.NET Core MVC 10, Entity Framework Core và SQLite.
Giao diện (UI) được thiết kế theo phong cách Cyberpunk/Neon hiện đại với màu chủ đạo là Đen (#050505) và Xanh Neon (#00FF66).

## Tính năng nổi bật
1. Đăng nhập / Đăng ký với Identity (Hỗ trợ Google Auth).
2. Lọc sản phẩm AJAX mượt mà không tải lại trang.
3. So sánh sản phẩm thông minh bằng AI (Groq API).
4. Trang chủ động với Hero Banner, Flash Sale và Lưới sản phẩm.
5. Gửi Email thông báo đặt hàng qua Resend API.
6. Admin Dashboard biểu đồ thời gian thực (Chart.js).

## Hướng dẫn chạy dự án (Dành cho người mới)

### Bước 1: Yêu cầu môi trường
*   Cài đặt **.NET SDK** (Phiên bản 8.0 trở lên).
*   Tải dự án về máy và mở thư mục `ShopLaptop-v1`.

### Bước 2: Khởi chạy dự án
1.  Mở **Terminal** (PowerShell hoặc Command Prompt) ngay tại thư mục `ShopLaptop-v1`.
2.  Gõ lệnh sau và nhấn Enter:
    ```bash
    dotnet run
    ```
3.  Chờ khoảng 5-10 giây cho đến khi thấy dòng chữ:
    `Now listening on: http://localhost:5143`
4.  Mở trình duyệt (Chrome/Edge) và truy cập địa chỉ: **http://localhost:5143**

### Bước 3: Tạo tài khoản Quản trị (Admin)
Thông tin quản trị không được lưu trực tiếp trong source code. Cấu hình tài khoản
admin cục bộ bằng .NET User Secrets trước lần chạy đầu tiên:

```bash
dotnet user-secrets set "SeedAdmin:Email" "admin@example.com"
dotnet user-secrets set "SeedAdmin:Password" "your-strong-password"
```

Trang quản trị: [http://localhost:5143/Admin](http://localhost:5143/Admin)

### Cấu hình dịch vụ ngoài
Các API key cũng phải được cấu hình bằng User Secrets:

```bash
dotnet user-secrets set "GroqApi:ApiKey" "your-groq-api-key"
dotnet user-secrets set "Resend:ApiKey" "your-resend-api-key"
```

### Build va chay test

Chay lenh sau truoc khi push de kiem tra ca web va cac logic quan trong:

```bash
dotnet build ShopLaptop-v1.slnx --no-restore
dotnet test ShopLaptop-v1.slnx --no-restore
```

---

## 💾 Vấn đề về Dữ liệu (Database)
Nếu bạn thấy trang web trống (không có sản phẩm):
*   **Hệ thống tự động Seed:** Dự án đã tích hợp sẵn `DbSeeder`. Khi bạn chạy lệnh `dotnet run` lần đầu tiên, hệ thống sẽ tự động tạo các danh mục (Apple, Dell, Asus...) và hơn 40 sản phẩm mẫu nếu cơ sở dữ liệu đang trống.
*   **Xử lý lỗi:** Nếu dữ liệu vẫn không hiển thị, hãy thử xóa file `shoplaptop.db` trong thư mục gốc và chạy lại lệnh `dotnet run`. Hệ thống sẽ khởi tạo lại toàn bộ dữ liệu gốc cho bạn.

---

## ✨ Tính năng chính
*   **Giao diện Cyberpunk:** Đen sâu, Neon Lime, hiệu ứng kính mờ (Glassmorphism).
*   **Bộ lọc thông minh:** Lọc theo hãng, giá, CPU, RAM, VGA trực tiếp trên trang chủ.
*   **Quản lý Flash Sale:** Admin có thể bật/tắt và đổi nội dung thanh Flash Sale xanh lá ngay trong trang Cài đặt.
*   **Trợ lý AI:** Tư vấn so sánh laptop sử dụng Groq API (Llama 3).
*   **Quản lý Profile:** Trang cá nhân Cyberpunk cực đẹp dành cho người dùng.

---
🚀 *Được tối ưu hóa bởi BOMIXI Engineering.*
