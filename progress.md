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

## 2026-08-02 - Task: 向官方仓库提交资源链接功能并完成审查修复

### What was done

- 从官方最新 `upstream/main` 提交 `77d4c80` 建立独立分支 `feat/resource-junction-manager-official`，只保留功能、测试与用户文档，不包含预览发布工作流、发布标签和本进度日志。
- 创建官方 PR #71，并按维护者要求在正文披露模型、Codex harness、实际工具、skills 使用情况和用户提示词摘要。
- 将成立的自动审查反馈合并为单次修复提交 `8336763`：跳过嵌套重解析点、保持固定目录范围不可变、修复直接 `Sinmai_Data` 布局发现、保护自动选择 POST、修复操作后按钮状态，并从 Linux Swagger 重新生成客户端。
- 测试完成后重新读取维护者与全部审查评论；最新 Sourcery 和 Cubic 检查均成功，Cubic 对修复提交报告 0 个新问题，未再推送代码。

### Testing

- `dotnet test MaiChartManager.Tests/MaiChartManager.Tests.csproj -c Debug`：18 项通过，包含带空格和 `&` 的临时路径真实 Junction 测试。
- `dotnet test MaiChartManager.Tests/MaiChartManager.Tests.csproj -c LinuxDebugBackend`：18 项通过，确认测试项目在 Linux 配置下使用 `net10.0`。
- `pnpm build`：Vue 生产构建通过。
- `dotnet build MaiChartManager/MaiChartManager.csproj -c LinuxDebugBackend --no-restore`：通过。
- 本地 Swagger 确认自动选择端点为 POST，`apiGen.ts` 本地生成成功；随后访问无关的远端 AquaMai OpenAPI 时发生 `ECONNRESET`，未影响生成结果。
- 官方 PR 检查：Sourcery `SUCCESS`；Cubic `SUCCESS`，最新审查为 `0 issues found across 7 files`；PR 状态为 open、mergeable。

### Notes

- `progress.md`：追加官方 PR、审查处理、验证结果和回滚信息。
- 官方 PR：`https://github.com/MuNET-OSS/MaiChartManager/pull/71`；功能提交 `2cc64eb`，集中审查修复提交 `8336763`。
- 官方 PR 未改动真实 `F:\Game` 资源；测试只使用系统临时目录中的模拟游戏结构和 Junction。
- 回滚点：本记录前为 `1a0ebba`；提交后可使用 `git revert <本记录提交>` 仅撤销本次日志，不影响官方 PR 或功能代码。

## 2026-08-02 - Task: 处理官方 PR 最新审查并完成复审

### What was done

- 按 Sourcery 最新审查集中修复三项问题：前端显示后端目录校验详情、控制器集中执行 Export/本地操作头校验、原生源目录和目标目录选择标题接入三语本地化。
- 修复控制器辅助函数最初引入的可空泛型返回警告，保持原有 `Forbid` 与 `BadRequest` 行为。
- 在全部本地验证完成后再次读取维护者和自动审查评论，确认没有新增人工要求，再以单次提交 `5f932b1` 推送官方 PR。
- Sourcery 将原 inline 建议标记为已由 `5f932b1` 解决；Cubic 对本次 6 个文件报告 `0 issues`，未继续推送代码。

### Testing

- `dotnet test MaiChartManager.Tests/MaiChartManager.Tests.csproj -c Debug`：18 项通过，且 `ResourceJunctionController` 不再产生本轮引入的 `CS8604`。
- `dotnet test MaiChartManager.Tests/MaiChartManager.Tests.csproj -c LinuxDebugBackend`：18 项通过。
- `pnpm build`：Vue 生产构建通过；仅有仓库既有的 Sentry token、UnoCSS 重复引入和大 chunk 提示。
- `git diff --check`：通过；前端构建未留下 `wwwroot` 待提交变更。
- 官方 PR 检查：Sourcery `SUCCESS`；Cubic `SUCCESS`，最新审查为 `0 issues found across 6 files`。

### Notes

- `MaiChartManager/Controllers/Tools/ResourceJunctionController.cs`：集中 POST 可用性校验，并使用本地化的原生目录选择标题。
- `MaiChartManager/Front/src/views/Tools/ResourceJunctionModal.tsx`：从生成的 Fetch 客户端错误负载提取并展示后端校验详情。
- `MaiChartManager/Locale.resx`、`MaiChartManager/Locale.zh-Hans.resx`、`MaiChartManager/Locale.zh-Hant.resx`：增加源目录和目标目录选择标题的三语资源。
- `docs/resource-junction-manager.md`：补充目录校验错误展示行为。
- `progress.md`：追加本轮审查修复、验证和复审结果。
- 官方 PR：`https://github.com/MuNET-OSS/MaiChartManager/pull/71`；本轮集中修复提交 `5f932b1`。
- 回滚方式：官方 PR 代码使用 `git revert 5f932b1`；本功能分支日志使用 `git revert <本记录提交>`，两者均不涉及真实 `F:\Game` 资源。
