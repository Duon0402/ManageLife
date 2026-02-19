# ManageLife

<p align="center">
  <img src="https://raw.githubusercontent.com/FortAwesome/Font-Awesome/6.x/svgs/solid/rocket.svg" width="50" height="50" />
  <br />
  <strong>🚀 Ứng dụng quản lý cuộc sống cá nhân toàn diện</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-6.0-512bd4?style=for-the-badge&logo=.net" />
  <img src="https://img.shields.io/badge/MySQL-4479A1?style=for-the-badge&logo=mysql&logoColor=white" />
  <img src="https://img.shields.io/badge/TypeScript-3178C6?style=for-the-badge&logo=typescript&logoColor=white" />
  <img src="https://img.shields.io/badge/Bootstrap-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white" />
</p>

---

## 📖 Giới thiệu

**ManageLife** là một nền tảng quản lý cuộc sống cá nhân hiện đại, được xây dựng trên **ASP.NET Core 6.0**. Dự án tập trung vào việc tối ưu hóa hiệu suất làm việc hàng ngày thông qua các công cụ tự động hóa, quản lý dữ liệu linh hoạt và giao diện người dùng thân thiện.

---

## ✨ Tính năng nổi bật

### 🏗️ UI Builder Hệ thống (Mới)
- **Fluent Grid Builder**: Xây dựng các bảng dữ liệu (DataTables) mạnh mẽ chỉ với vài dòng code TypeScript.
- **Dynamic Form Builder**: Tự động tạo form và modal quản lý dữ liệu (CRUD) từ cấu hình.
- **ES Modules**: Kiến trúc Frontend hiện đại, dễ dàng bảo trì và mở rộng.

### 🔐 Bảo mật & Hệ thống
- **Xác thực JWT**: Hệ thống đăng ký/đăng nhập an toàn với JWT và Refresh Token.
- **Phân quyền linh hoạt**: Quản lý Roles và Permissions đến từng Action.
- **Đa ngôn ngữ**: Hệ thống quản lý bản dịch (Localization) đa quốc gia.

### 📧 Tiện ích thông minh
- **Daily Email Report**: Tự động tổng hợp và tạo nội dung email báo cáo công việc hàng ngày chuyên nghiệp.
- **Telegram File Storage**: Sử dụng Telegram API để lưu trữ file cloud không giới hạn dung lượng và hoàn toàn miễn phí.
- **QR Code Tools**: Tạo mã QR nhanh chóng cho nhiều mục đích.

---

## 🛠 Công nghệ sử dụng

| Lớp | Công nghệ |
|:---|:---|
| **Backend** | .NET 6, Entity Framework Core, Identity |
| **Database** | MySQL (Pomelo), Redis (Caching) |
| **Frontend Core** | TypeScript (ES Modules), jQuery, Bootstrap 5 |
| **UI Components** | DataTables.net, Flatpickr, Toastr, Select2 |
| **Integration** | Telegram Bot API, MailKit, EPPlus (Excel) |

---

## 📁 Cấu trúc dự án

```text
ManageLife/
├── Base/               # Kiến trúc cốt lõi (BaseService, RepositoryBase)
├── Clients/            # Mã nguồn Frontend (TypeScript)
│   ├── Core/           # Fluent Builders (Grid, Form), Common Services
│   └── Pages/          # Logic xử lý cho từng trang riêng biệt
├── Controllers/        # Xử lý Request (Admin, API, Web Client)
├── Entities/           # Database Models
├── Interfaces/         # Định nghĩa các dịch vụ (Dependency Injection)
├── Services/           # Logic nghiệp vụ chính
├── Views/              # Giao diện Razor (Server-side rendering)
├── wwwroot/            # Static assets (JS sau khi build, CSS, Libs)
└── README.md           # Tài liệu dự án
```

---

## 🚀 Cài đặt nhanh

### 1. Yêu cầu
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- MySQL Server & Redis

### 2. Cấu hình
Cập nhật chuỗi kết nối trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=managelife;user=root;password=..."
  },
  "TelegramSettings": {
    "BotToken": "your_bot_token",
    "ChatId": "your_chat_id"
  }
}
```

### 3. Khởi chạy
```bash
# Cập nhật database
dotnet ef database update

# Chạy ứng dụng
dotnet run
```

---

## 🗺 Lộ trình phát triển (Roadmap)

- [x] Chuyển đổi sang TypeScript ES Modules
- [x] Hệ thống Fluent Grid Builder
- [x] Tích hợp Daily Email Report
- [ ] Quản lý Tài chính cá nhân (Finance tracking)
- [ ] Chuyển đổi build system sang Vite
- [ ] Ứng dụng di động (MAUI hoặc PWA)

---

## 👨‍💻 Tác giả

**Đặng Trường Dương** - *Initial work* - [ManageLife](https://github.com/Duon0402

---

<p align="center">
  <i>⭐ Nếu bạn thấy dự án này thú vị, đừng quên nhấn Star nhé!</i>
</p>

