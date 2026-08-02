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

## 2026-08-02 - Task: 使用 GitHub Actions 发布资源链接功能预览版

### What was done

- 增加仅由 `resource-junction-manager-*` 标签触发的预览发布工作流。
- 工作流在 GitHub 托管 runner 上构建前端和 self-contained Windows x64 程序，生成便携 ZIP 与 SHA-256 校验文件，并上传到 prerelease。

### Testing

- 待标签推送后，以对应 GitHub Actions 运行和 Release 资产下载信息作为正式验证证据。

### Notes

- `.github/workflows/release-preview.yml`：新增功能预览版编译、归档、校验和 Release 上传流程。
- `docs/project-structure.md`：登记预览发布工作流职责。
- `progress.md`：追加本轮发布施工和验证记录。
- 回滚方式：删除本轮创建的 prerelease 与标签，并回退本任务提交；不会影响此前的功能提交。

## 2026-08-02 - Task: 验证并记录资源链接预览版发布结果

### What was done

- 以标签 `resource-junction-manager-preview-20260802` 触发 GitHub Actions，并完成资源链接功能的 Windows x64 预览版发布。
- 核对标签实际指向、Release 状态、附件大小及 ZIP 校验值。

### Testing

- GitHub Actions `30744205967`：`Build Frontend` 与 `Build Windows Preview and Release` 均成功。
- Release 为非草稿 prerelease，标签最终指向提交 `d913c14`。
- `MaiChartManager-win-x64.zip` 已上传，大小 288799852 字节；GitHub 资产摘要与 `.sha256` 文件均为 `4661c9f8d5e312de07e6623ce2b6296db42404c4ced05a216f9bcbff0ef2ee09`。

### Notes

- `progress.md`：追加 Actions、Release、标签和附件校验结果。
- 发布地址：`https://github.com/UranusNo7/MaiChartManager/releases/tag/resource-junction-manager-preview-20260802`。
- 回滚方式：删除 GitHub prerelease 和远端/本地 `resource-junction-manager-preview-20260802` 标签；该操作不回退功能代码。

## 2026-08-02 - Task: 增加独立目标目录选择并调整目录按钮布局

### What was done

- 将源目录和目标目录选择按钮分别移动到对应目录信息右侧。
- 目标目录默认使用 MaiChartManager 当前游戏，但允许在资源链接工具会话内独立切换，不修改全局游戏配置，也不触发游戏数据重新加载。
- 新目标与当前源重合时清空源选择，阻止同目录自链接。

### Testing

- `dotnet test MaiChartManager.Tests/MaiChartManager.Tests.csproj -c Debug --no-restore`：16 项通过；新增覆盖会话内目标覆盖、不重新读取当前游戏提供器和源目标重合保护。
- `pnpm build`：Vue 生产构建通过。
- `pnpm genClient`：本地 Swagger 客户端生成成功；随后既有远端 AquaMai OpenAPI 地址连接重置，命令最终返回非零。

### Notes

- `MaiChartManager/Services/ResourceJunctionService.cs`：增加会话内目标目录状态与源目标重合保护。
- `MaiChartManager/Controllers/Tools/ResourceJunctionController.cs`：增加本地原生目标目录选择 API。
- `MaiChartManager/Front/src/views/Tools/ResourceJunctionModal.tsx`、`src/locales/*.yaml`、`src/client/apiGen.ts`：调整两个目录按钮布局并接入目标选择。
- `MaiChartManager.Tests/Services/ResourceJunctionServiceTests.cs`：增加独立目标选择测试。
- `docs/resource-junction-manager.md`：更新目标目录选择、会话边界和界面说明。
- 回滚方式：回退本任务提交；本轮没有修改真实游戏目录，无需文件系统回滚。

## 2026-08-02 - Task: 验证并记录独立目标目录版本发布结果

### What was done

- 以标签 `resource-junction-manager-preview-20260802-2` 触发 GitHub Actions，完成独立目标目录选择版本的 Windows x64 预览版发布。
- 核对标签实际指向、Release 状态、附件大小以及 ZIP 的 GitHub 摘要和校验文件内容。

### Testing

- GitHub Actions `30744707325`：`Build Frontend` 与 `Build Windows Preview and Release` 均成功。
- GitHub Actions `30744707312` 和 `30744695523`：标签与功能分支在提交 `599395bcdcda83db6c346e2cd1566baba03f26b8` 上的前端构建、Windows 构建和测试均成功。
- Release 为 prerelease，标签最终指向提交 `599395bcdcda83db6c346e2cd1566baba03f26b8`。
- `MaiChartManager-win-x64.zip` 已上传，大小 288801934 字节；GitHub 资产摘要与 `.sha256` 文件均为 `b6967812464e45e8fec80e4b5706becbe8e9c1e3fdaa3eb9401b621cf3706a93`。

### Notes

- `progress.md`：追加本轮 Actions、Release、标签和附件校验结果。
- 发布地址：`https://github.com/UranusNo7/MaiChartManager/releases/tag/resource-junction-manager-preview-20260802-2`。
- 回滚方式：删除 GitHub prerelease 和远端/本地 `resource-junction-manager-preview-20260802-2` 标签；该操作不回退功能代码。
