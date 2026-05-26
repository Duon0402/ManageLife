# ManageLife

<div align="center">

<img src="https://raw.githubusercontent.com/FortAwesome/Font-Awesome/6.x/svgs/solid/rocket.svg" width="60" height="60" />

### Ứng dụng quản lý cuộc sống cá nhân toàn diện

[![.NET](https://img.shields.io/badge/.NET-8.0-512bd4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com)
[![MySQL](https://img.shields.io/badge/MySQL-8.0-4479A1?style=for-the-badge&logo=mysql&logoColor=white)](https://mysql.com)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.x-3178C6?style=for-the-badge&logo=typescript&logoColor=white)](https://typescriptlang.org)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.x-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)](https://getbootstrap.com)
[![Redis](https://img.shields.io/badge/Redis-Cache-DC382D?style=for-the-badge&logo=redis&logoColor=white)](https://redis.io)

</div>

---

## Giới thiệu

**ManageLife** là nền tảng quản lý cuộc sống cá nhân xây dựng trên **ASP.NET Core 8.0 MVC**, tập trung vào tự động hóa các tác vụ lặp lại hàng ngày, quản lý dữ liệu linh hoạt và giao diện người dùng nhất quán.

Điểm đặc trưng của dự án là **hệ thống UI Builder** phía client — tập hợp các Fluent Builder TypeScript cho phép xây dựng giao diện phức tạp (bảng dữ liệu, form CRUD, date picker, gallery...) từ cấu hình, không cần viết lặp HTML.

---

## Kiến trúc

```
Browser ──► ASP.NET Core 8 MVC ──► Services ──► EF Core ──► MySQL
                    │                   │
                    │               Redis Cache
                    │
              TypeScript Namespace Monolith
              (ClientSrc/ → wwwroot/js/app.js)
```

**Patterns:** Repository + UnitOfWork · Result\<T\> response wrapper · Soft Delete · Permission-based auth · Request-scoped Contexts · Fluent Builder (UI)

---

## Tính năng

<table>
<tr>
<td width="50%" valign="top">

**Hệ thống**
- JWT Authentication + Refresh Token
- Phân quyền Role/Permission đến từng Action
- Đa ngôn ngữ: server `T("key")` + client `TranslationService.t("key")`
- Background jobs (Hangfire)
- Realtime chat (SignalR)
- Structured logging (Serilog)

</td>
<td width="50%" valign="top">

**Tiện ích**
- Daily Email Report — tự động tổng hợp và tạo nội dung email báo cáo công việc
- Telegram File Storage — lưu trữ file cloud qua Telegram Bot API
- QR Code Generator
- Import/Export Excel (EPPlus)

</td>
</tr>
</table>

---

## UI Builder System

Tất cả builder nằm trong `ClientSrc/Core/`, theo pattern Fluent API `return this`, kết hợp qua `.build()`.

```typescript
// Ví dụ: bảng dữ liệu có CRUD đầy đủ
new GridBuilder('#container', { url: '/Admin/User/GetList' })
    .addColumn(new GridColumnBuilder('fullName', 'Họ tên').build())
    .addColumn(new GridColumnBuilder('email', 'Email').build())
    .addColumn(new GridColumnBuilder('createdTime', 'Ngày tạo').asDate().build())
    .addToolbarButton({ label: 'Thêm mới', onClick: () => form.showCreate() })
    .build();

// Ví dụ: date picker với validate nhập tay
new DatePickerBuilder('#dobContainer')
    .withId('dob')
    .setDefaultDate(new Date())
    .enableTyping()   // bật nhập tay, tự validate format dd/mm/yyyy
    .onChange(date => console.log(date))
    .build();
```

| Builder | Thư viện tích hợp | Tính năng nổi bật |
|:---|:---|:---|
| **GridBuilder** | DataTables.net | Toolbar, action buttons, AJAX, reload, clear |
| **GridColumnBuilder** | — | Format: date, currency, boolean, badge, custom template |
| **GridFormBuilder** | Bootstrap Modal | Modal CRUD tự động, multi-column layout, validation |
| **DatePickerBuilder** | Flatpickr | Calendar popup, nhập tay + validate format, minDate/maxDate |
| **FileUploaderBuilder** | — | Drag & drop, progress bar, extension/size validation |
| **PopupBuilder** | Bootstrap Modal | Title, body HTML, footer, scrollable, size variants |
| **GalleryBuilder** | PhotoSwipe | Responsive grid, lightbox, delete/download |

---

## Client Services

| Service | Mô tả |
|:---|:---|
| `ApiService` | jQuery AJAX wrapper với loading, toast, progress upload |
| `TranslationService` | Load + cache translation dict từ API, `t("key", ...args)` sync |
| `ToastService` | Toast notification (success / error / warning / info) |
| `MessageService` | Modal confirm/alert |
| `LoadingService` | Global loading overlay |

---

## Cấu trúc dự án

```text
ManageLife/
│
├── ClientSrc/                  # TypeScript source → wwwroot/js/app.js
│   ├── Core/
│   │   ├── Grid/               # GridBuilder, GridColumnBuilder, GridFormBuilder
│   │   ├── Gallary/            # GalleryBuilder
│   │   ├── ApiService.ts
│   │   ├── BasePage.ts         # Abstract base cho tất cả page classes
│   │   ├── DatePickerBuilder.ts
│   │   ├── FileUploaderBuilder.ts
│   │   ├── LoadingService.ts
│   │   ├── MessageService.ts
│   │   ├── PopupBuilder.ts
│   │   ├── ToastService.ts
│   │   └── TranslationService.ts
│   ├── Models/
│   └── Pages/
│       ├── Admin/              # AdminUserPage, AdminRolePage, AdminTranslationPage ...
│       └── Client/             # ClientHeader, UtilityEmailPage, ChatPage ...
│
├── Commons/                    # App-wide: Cache, Constants, Enums, TranslationKey
├── Contexts/                   # Request-scoped: UserContext, LanguageContext, TranslationContext
├── Controllers/                # Endpoints: Admin/, Client/, API/
├── Core/                       # Infrastructure: BaseController, Result<T>, Attributes, Mapping
├── Data/                       # AppDbContext, Migrations, Seed
├── Entities/                   # EF Core domain entities
├── Hubs/                       # SignalR ChatHub
├── Interfaces/                 # IService, IRepository interfaces
├── Middleware/                 # JwtAuthenticationMiddleware
├── Models/                     # Request/Response DTOs
├── Repositories/               # EF Core implementations
├── Services/                   # Business logic
├── Views/                      # Razor views: Admin/, Client/, Shared/
└── wwwroot/                    # Static assets: compiled JS, CSS, lib/
```

---

## Cài đặt

**Yêu cầu:** .NET 8.0 SDK · MySQL 8 · Redis

```bash
# 1. Clone
git clone https://github.com/Duon0402/ManageLife.git
cd ManageLife

# 2. Cấu hình secrets (không commit file này)
cp appsettings.json appsettings.Development.json
# Sửa ConnectionStrings, Redis, TelegramSettings trong file Development

# 3. Migrate database
dotnet ef database update

# 4. Chạy
dotnet run
```

<details>
<summary>Cấu hình mẫu <code>appsettings.Development.json</code></summary>

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=managelife;user=root;password=your_password"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "TelegramSettings": {
    "BotToken": "your_bot_token",
    "ChatId": "your_chat_id"
  },
  "MailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your@email.com",
    "Password": "your_app_password"
  }
}
```

</details>

---

## Công nghệ

| Nhóm | Công nghệ |
|:---|:---|
| Backend | ASP.NET Core 8.0 · Entity Framework Core · Identity |
| Database | MySQL (Pomelo) · Redis · Hangfire |
| Frontend | TypeScript · jQuery · Bootstrap 5 |
| UI Libraries | DataTables.net · Flatpickr · PhotoSwipe · Font Awesome |
| Integration | Telegram Bot API · MailKit · EPPlus · QRCoder |
| DevOps | Serilog · AutoMapper · LinqKit |

---

## Lộ trình

- [x] Fluent UI Builder System (Grid, Form, DatePicker, Gallery, Popup, FileUploader)
- [x] Phân quyền Role/Permission
- [x] Đa ngôn ngữ + client-side TranslationService
- [x] Daily Email Report · Telegram File Storage · QR Code
- [x] Performance: Redis cache · AsNoTracking · DB indexes · DataProtection keys
- [ ] Quản lý tài chính cá nhân
- [ ] PWA / Mobile

---

<div align="center">

**Đặng Trường Dương** — [github.com/Duon0402](https://github.com/Duon0402)

</div>
