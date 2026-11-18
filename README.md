# Better Feeding Details（更好的喂食详情）

《深空当铺（Probably Stolen）》独立 MOD 源码仓库。

在鱼类详情提示框中追加三块信息：**当前屠宰价值**、**累计饲料花费**、**当前利润与食物消耗构成**。纯统计与展示，不改动原版养殖、投喂、生长、繁殖、屠宰和收益机制。

- MOD ID：`better_feeding_details`
- 程序集：`BetterFeedingDetails.dll`（netstandard2.1 / C# 7.3 / Harmony 2）
- 依赖：无，不声明任何能力
- 游戏版本：`0.46`

> 本仓库只收录自研源码。游戏依赖 DLL（`Managed/`）、反编译工具、第三方 modding 文档与可安装交付产物（`mod/BetterFeedingDetails/release/`）均不在本仓库中。

## 特性

1. **价值（所有鱼即时可见）**：按原版屠宰方法计算当前鱼肉片 / 边角料 / 植皮片的基础价值，废品级 ×25%。仅依赖鱼的累计摄食热量与鱼种，**历史鱼打开详情即可显示，无需先喂一轮**。
2. **花费与食物构成（Mod 启用后记录）**：追踪每条鱼实际消耗的工业饲料、污泥饲料、肉类与其他食物热量，按类别展示消耗与吸收。
3. **利润（完整账本鱼）**：`价值 − 已知饲料成本`，盈利绿 / 亏损红；肉类与普通食物因游戏未保存单价，标记 `--`，利润加 `?` 表示部分统计。
4. **鱼苗拆分**：一窝拆成多尾时，账本按原版相同的尾数平分，避免成本被重复计算。
5. **存档安全**：账本写在鱼自身 tag 上，随存档 / 移动 / 销毁自动处理；不修改原版存档结构。

成本仅对有确定基础单价的两类饲料计价：工业鱼饲料 0.02/kcal（100/5000）、污泥饲料粒 0.01/kcal（40/4000）。

## 构建

依赖本机 `Managed/` 中的游戏程序集与 Roslyn 编译器（无需 .NET SDK 编译）：

```powershell
$env:PATH = "C:\Program Files\dotnet;$env:PATH"
& .\build.ps1 -CompilerDirectory <Roslyn bincore 目录>
```

产物 `BetterFeedingDetails.dll` 与 `manifest.xml` 输出到 `../release/`。`build.ps1` 也支持 `-GameManagedDir` 与 `-OutputDir`。

## 数据来源与限制

- 进食追踪：Harmony Patch `AquacultureHelper.AddHunger`，用进食前后 `FISH_KCAL_EATEN_TAG` 差值得到实际吸收热量。
- 拆分追踪：Patch `AquacultureHelper.GrowFry`。
- UI：官方钩子 `ModHook.OnCreateTooltipLate`，补丁由加载器自动应用（无需手动 `PatchAll`）。
- 原版活鱼 `unitValue = 0`，无缓存价值 tag，故价值走即时计算；体重无法反推历史花费（身体热量与累计摄食热量是两套计数，且插值取整不可逆）。完整调查结论见 mod 包内 `README.md`。

## 代码结构

```
src/
├── ModEntry.cs               入口 / 生命周期 / 日志
├── manifest.xml              模组清单
├── AssemblyInfo.cs           程序集版本
├── BetterFeedingDetails.csproj
├── build.ps1                 编译脚本
├── Data/
│   ├── FeedLedger.cs          账本数据结构 + 拆分
│   ├── FeedLedgerStore.cs     自定义序列化 + tag 存取
│   └── FeedPricing.cs         单价与成本估算
├── Patches/
│   ├── AddHungerPatch.cs      进食追踪（核心）
│   └── GrowFryPatch.cs        鱼苗拆分成本分配
└── UI/
    └── FeedingTooltip.cs      详情提示框注入 + 价值/利润计算
```

## 日志

仅使用 info / error 两级：正常启停走 info，异常走 error；所有补丁处理函数都 catch 异常并回退到原版行为，MOD 不会中断游戏。
