# MiniStore 小店数字化系统（前后端分离版）

MiniStore 是面向美甲、美睫、美容工作室等预约制小店的轻量数字化模板系统。采用 **.NET 8 Web API** 与 **Vue 3 + TypeScript + Vite** 前后端分离架构，支持多租户数据隔离、顾客端 H5 扫码选项目预约与商家移动/PC管理后台。

---

## 🛠️ 技术架构

- **后端**：ASP.NET Core 8 Web API
  - 数据访问：Entity Framework Core（SQLite / PostgreSQL 兼容）
  - 多租户机制：EF Core 全局查询过滤器（Global Query Filters）+ `TenantMiddleware` 租户上下文注入
  - 安全鉴权：JWT Bearer Token + BCrypt 密码加密
  - 接口文档：Swagger / OpenAPI UI
- **前端**：Vue 3 + TypeScript + Vite + Pinia + Vue Router
  - 顾客端 H5：移动端视口适配、微信扫码友好、预约时段生成算法、一键拨号与地址复制
  - 商家管理台：响应式侧边抽屉导航、经营看板、预约流转、客户档案、服务上架、会员次卡快捷核销、**高清台卡二维码动态生成**

---

## 🚀 快速启动指南

### 1. 启动后端 API（.NET 8）
```bash
# 位于项目根目录下
dotnet run --project src/MiniStore.Api --launch-profile http
```
- 后端服务地址：`http://localhost:5033`
- Swagger 接口文档：`http://localhost:5033/swagger`

### 2. 启动前端 SPA（Vue 3）
```bash
cd src/ministore-web
npm install
npm run dev
```
- 前端服务地址：`http://localhost:5173`
- 顾客端 H5 演示体验：`http://localhost:5173/s/muse-nail`
- 商家管理后台：`http://localhost:5173/admin/login`

---

## 🔑 内置演示账号

| 角色 | 访问地址 | 账号 | 密码 | 预置数据 |
| :--- | :--- | :--- | :--- | :--- |
| **示范商家** | `/admin/login` | `admin` | `123456` | MUSE NAIL（缪斯日式美睫美甲店）、8款精选服务项目、真实客户档案、预约单与次卡 |
| **顾客端体验** | `/s/muse-nail` | 扫码即用 | 免登录 | 在线浏览分类、选时段预约、到店核销次卡 |

---

## 📁 目录结构

```
小店模板项目/
├── MiniStore.sln               # .NET 8 解决方案
├── DEVELOPMENT_PLAN.md         # 14天前后端分离落地执行计划
├── README.md                   # 快速上手与架构说明文档
└── src/
    ├── MiniStore.Api/          # .NET 8 Web API 后端项目
    │   ├── Controllers/        # 控制器（公开 H5 API、商家管理 API、上传等）
    │   ├── Data/               # AppDbContext (多租户过滤器)、DbInitializer (种子数据)
    │   ├── DTOs/               # 前后端传输数据模型
    │   ├── Entities/           # 10张核心实体定义 (IMustHaveTenant 强约束)
    │   ├── Middleware/         # 租户上下文解析中间件
    │   └── Services/           # JWT 认证服务、租户作用域服务
    └── ministore-web/          # Vue 3 前端工程
        ├── src/
        │   ├── api/            # Axios 请求封装与 JWT 拦截器
        │   ├── assets/         # 全局样式系统
        │   ├── layouts/        # 商家后台响应式侧边布局
        │   ├── router/         # 路由配置与全局前置守卫
        │   ├── stores/         # Pinia 状态管理
        │   └── views/
        │       ├── customer/   # 顾客端 H5 门店页面 (StoreView.vue)
        │       └── admin/      # 商家登录、看板、预约、客户、服务、次卡、设置
        └── public/_redirects   # Cloudflare Pages SPA 防 404 规则
```
