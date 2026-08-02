## 2026-08-02 - Task: 集成固定路径游戏资源 Junction 管理

### What was done

- 在工具页加入 SDGB 1.56 与 SDEZ 1.66 之间的固定资源链接管理，仅覆盖三个指定资源目录。
- 后端实现逐项检查、安全建立和只移除正确 Junction 的状态机，禁止远程模式写操作，并为写请求增加跨站误触保护。
- 增加无需发布密钥的 GitHub Actions 前端编译与 Windows 测试工作流。

### Testing

- `dotnet test MaiChartManager.Tests/MaiChartManager.Tests.csproj -c Debug`：8 项通过；临时目录真实 Junction 建立、读取、移除、错误目标拒绝和源文件保留均通过。
- `pnpm build`：Vue 生产构建通过。
- `GET /MaiChartManagerServlet/GetResourceJunctionStatusApi`：真实 SDGB 三项均识别为 `AlreadyLinked`，只读检查未改变游戏目录。

### Notes

- `.github/workflows/verify.yml`：新增 fork 可直接运行的前端与 Windows 后端验证。
- `MaiChartManager/Services/ResourceJunctionService.cs`：新增固定范围 Junction 业务逻辑。
- `MaiChartManager/Controllers/Tools/ResourceJunctionController.cs`：新增仅本地可用的资源链接 API。
- `MaiChartManager/ServerManager.cs`：注册资源链接服务。
- `MaiChartManager/Front/src/views/Tools/ResourceJunctionModal.tsx`、`index.tsx`：新增工具入口、状态和确认界面。
- `MaiChartManager/Front/src/locales/*.yaml`、`src/client/apiGen.ts`：新增三语文案和生成的 API 类型。
- `MaiChartManager.Tests/Services/ResourceJunctionServiceTests.cs`、`MaiChartManager.Tests.csproj`：新增 Windows Junction 测试并对齐测试目标框架。
- `docs/project-structure.md`、`docs/resource-junction-manager.md`：记录模块结构、固定路径和安全边界。
- 回滚方式：回退本任务提交；若已通过界面建立链接，应先在界面中确认并移除三个正确 Junction，再回退代码。
