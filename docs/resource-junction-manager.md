# 游戏资源链接

## 固定范围

只读源目录：

```text
F:\Game\maimai DX SDEZ 1.66\Package\Sinmai_Data\StreamingAssets\A000
```

目标目录：

```text
F:\Game\maimai DX SDGB 1.56\Package\Sinmai_Data\StreamingAssets\A000
```

仅处理 `AssetBundleImages`、`MovieData`、`SoundData`。

## 安全规则

- 不写入、移动或删除源目录中的任何内容。
- 建立操作只处理目标位置不存在的项目；普通目录、文件、符号链接和错误 Junction 均拒绝覆盖。
- 移除操作只处理正确指向固定源目录的 Windows Junction，并使用非递归目录删除。
- 每次写操作前重新检查目标类型和指向，操作后再次验证状态。
- 写操作仅限本地桌面模式，并要求专用请求头；远程导出模式返回拒绝。
- Linux 构建只返回不支持，不执行 Junction 操作。

## 使用

打开“工具”中的“游戏资源链接”。界面会先读取三项状态；确认状态后可建立缺失链接或移除正确链接。两个写操作都会弹出二次确认。
