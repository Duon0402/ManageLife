# ManageLife

<p align="center">
  <strong>🚀 Ứng dụng quản lý cuộc sống cá nhân toàn diện</strong>
</p>

---

## 📖 Giới thiệu

**ManageLife** là một dự án cá nhân được xây dựng trên nền tảng **ASP.NET Core 6.0**, cung cấp các tiện ích hỗ trợ quản lý cuộc sống hàng ngày. Ứng dụng được thiết kế với kiến trúc rõ ràng, dễ mở rộng và tích hợp nhiều dịch vụ hiện đại.

---

## ✨ Tính năng chính

### 🔐 Hệ thống xác thực & phân quyền
- Đăng ký, đăng nhập với JWT Token
- Quản lý người dùng, vai trò (Role) và quyền hạn (Permission)
- Hỗ trợ Refresh Token

### 📋 Quản lý công việc (Todo)
- Tạo và quản lý danh sách công việc
- Theo dõi tiến độ hoàn thành

### 📁 Lưu trữ tệp (File Storage)
- Upload file thông qua Telegram Bot
- Lưu trữ và quản lý file cloud miễn phí
- Hỗ trợ nhiều định dạng: ảnh, video, audio, tài liệu

### 📊 Tiện ích khác
- **Tạo mã QR**: Hỗ trợ tạo QR code
- **Đa ngôn ngữ**: Hệ thống quản lý ngôn ngữ và bản dịch
- **Email Report**: Gửi báo cáo tổng hợp qua email
- **Cron Jobs**: Lên lịch và quản lý tác vụ tự động

### 🛠 Trang quản trị (Admin)
- Dashboard tổng quan
- Quản lý ngôn ngữ & bản dịch
- Quản lý Cron Jobs

---

## 🛠 Công nghệ sử dụng

| Công nghệ | Mô tả |
|-----------|-------|
| **ASP.NET Core 6.0** | Framework web chính |
| **Entity Framework Core** | ORM cho Database |
| **MySQL** | Cơ sở dữ liệu (Pomelo.EntityFrameworkCore.MySql) |
| **Redis** | Caching (StackExchange.Redis) |
| **JWT** | Xác thực người dùng |
| **Telegram Bot API** | Lưu trữ file & thông báo |
| **AutoMapper** | Object mapping |
| **EPPlus** | Xuất Excel |
| **QRCoder** | Tạo mã QR |
| **Bootstrap 5** | UI Framework |

---

## 📁 Cấu trúc dự án

```
ManageLife/
├── Base/               # Các class cơ sở (BaseEntity, ServiceBase, ...)
├── Commons/            # Hằng số, Enum, Helpers dùng chung
├── Contexts/           # DbContext configuration
├── Controllers/        # API & MVC Controllers
│   ├── Admin/          # Controllers cho trang quản trị
│   ├── API/            # RESTful API endpoints
│   └── Client/         # Controllers cho client
├── Data/               # Seed data
├── Entities/           # Entity models (EF Core)
│   ├── Auth/           # User, Role, Permission entities
│   └── Todo/           # Todo entities
├── Extensions/         # Extension methods
├── Helpers/            # Utility helpers
├── Interfaces/         # Service interfaces
├── Middlewares/        # Custom middlewares (JWT Auth)
├── Migrations/         # EF Core migrations
├── Models/             # DTOs và Request/Response models
├── Repositories/       # Data access layer
├── Services/           # Business logic layer
├── ViewModels/         # View models cho Razor Views
├── Views/              # Razor Views (MVC)
│   ├── Admin/          # Views trang quản trị
│   ├── Client/         # Views trang client
│   └── Shared/         # Shared layouts & partials
└── wwwroot/            # Static files (CSS, JS, images)
```

---

## 🚀 Hướng dẫn cài đặt

### Yêu cầu hệ thống
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- MySQL Server
- Redis Server (tuỳ chọn)

### Các bước cài đặt

1. **Clone repository**
   ```bash
   git clone https://github.com/your-username/ManageLife.git
   cd ManageLife
   ```

2. **Cấu hình kết nối**
   
   Chỉnh sửa file `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Your_MySQL_Connection_String"
     },
     "Redis": {
       "EndPoints": "Your_Redis_Endpoint",
       "Password": "Your_Redis_Password"
     },
     "Jwt": {
       "Key": "Your_Secret_Key",
       "Issuer": "ManageLife",
       "Audience": "ManageLifeUsers"
     },
     "TelegramSettings": {
       "BotToken": "Your_Telegram_Bot_Token",
       "ChatId": "Your_Chat_Id"
     }
   }
   ```

3. **Chạy Migration**
   ```bash
   dotnet ef database update
   ```

4. **Khởi chạy ứng dụng**
   ```bash
   dotnet run
   ```

5. **Truy cập ứng dụng**
   - Client: `https://localhost:5001`
   - Admin: `https://localhost:5001/Admin`

---

## 📡 API Endpoints

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| POST | `/api/auth/login` | Đăng nhập |
| POST | `/api/auth/register` | Đăng ký |
| POST | `/api/auth/refresh-token` | Refresh token |
| POST | `/api/file/upload` | Upload file |
| GET | `/api/file/{id}` | Lấy file URL |

---

## 🗺 Roadmap

- [x] Hệ thống xác thực JWT
- [x] Quản lý file với Telegram
- [x] Tạo mã QR
- [x] Đa ngôn ngữ
- [ ] Quản lý tài chính (Thu chi)
- [ ] Quản lý thời gian (Calendar)


---

## 👨‍💻 Tác giả

**Đặng Trường Dương**

---

## 📄 License

Dự án này được phát triển cho mục đích cá nhân.

---

<p align="center">
  <i>⭐ Nếu thấy hữu ích, hãy cho dự án một sao nhé!</i>
</p>
