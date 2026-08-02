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

## 2026-08-02 - Task: 增加游戏资源源目录自动与手动选择

### What was done

- 将链接目标改为 MaiChartManager 当前配置的游戏目录，不再固定游戏版本路径。
- 打开工具时从游戏路径历史和当前游戏的相邻目录中查找候选，递归统计三类资源文件并自动选择总数最多的有效源；最高数量并列时要求手动选择。
- 增加手动源目录选择、选择方式和三类/合计文件数展示，手动结果仅在当前程序会话内生效。

### Testing

- `dotnet test MaiChartManager.Tests/MaiChartManager.Tests.csproj -c Debug --no-restore`：14 项通过，覆盖游戏根目录/Package 归一化、无效及重解析点候选拒绝、文件数排名、并列、手动覆盖、目标拒绝及临时真实 Junction 建立和移除。
- `pnpm build`：Vue 生产构建通过。
- 本地 API 只读检查：当前 SDGB 1.56 自动选中 SDEZ 1.66；计数为 AssetBundleImages 26562、MovieData 1501、SoundData 3055，合计 31118；未改动真实游戏目录。
- `pnpm genClient`：本地 Swagger 的 `apiGen.ts` 生成成功；随后访问既有远端 AquaMai OpenAPI 地址时连接被重置，因此命令最终返回非零。

### Notes

- `MaiChartManager/Services/ResourceJunctionService.cs`：增加动态目标、候选发现、文件计数、自动排名与会话内手动覆盖。
- `MaiChartManager/Controllers/Tools/ResourceJunctionController.cs`：增加自动选择和原生目录选择 API，并返回完整工具状态。
- `MaiChartManager/Front/src/views/Tools/ResourceJunctionModal.tsx`、`src/locales/*.yaml`、`src/client/apiGen.ts`：增加选择交互、计数展示、三语文案和生成的客户端类型。
- `MaiChartManager.Tests/Services/ResourceJunctionServiceTests.cs`：增加候选选择与路径校验测试。
- `docs/project-structure.md`、`docs/resource-junction-manager.md`：更新模块职责、目录发现规则和安全边界。
- 回滚方式：回退本任务提交即可恢复固定路径版本；本轮真实目录验证仅执行只读查询，无需文件系统回滚。
