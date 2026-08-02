# 项目结构

## 核心入口

- `MaiChartManager/Controllers/`：本地 REST API 控制器。
- `MaiChartManager/Services/`：跨控制器复用的业务服务；游戏资源 Junction 的源目录选择、固定范围检查与写操作位于此处。
- `MaiChartManager/Front/src/views/`：Vue 业务界面；资源链接入口位于 `Tools/`。
- `MaiChartManager.Tests/`：xUnit 测试，包含 Windows 临时目录 Junction 集成测试。
- `.github/workflows/verify.yml`：无需发布密钥的前端编译和 Windows 后端测试。
- `.github/workflows/release-preview.yml`：资源链接功能标签触发的 Windows 便携 ZIP 预览发布。

## 游戏资源链接模块

- `Services/ResourceJunctionService.cs`：从当前游戏确定目标，在历史与相邻游戏中按三类资源文件总数自动选源，并负责状态检查、安全建立和安全移除。
- `Controllers/Tools/ResourceJunctionController.cs`：仅本地模式可用的自动选择、手动目录选择、查询与写操作 API。
- `Front/src/views/Tools/ResourceJunctionModal.tsx`：源目录选择、文件计数、状态列表、二次确认和操作结果。
