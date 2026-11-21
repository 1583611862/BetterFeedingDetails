# Better Feeding Details（更好的喂食详情）

《深空当铺（Probably Stolen）》独立 MOD 源码仓库。

在鱼类详情提示框只追加金色**当前屠宰价值**；在鱼图标左上角叠加蓝色 ♂ / 粉色 ♀ 性别标签，成年鱼额外显示黄色星星；在循环水养殖系统（RAS）详情追加**每日食物需要量**与**剩余饲料可维持天数**。纯统计与展示，不改动原版养殖、投喂、生长、繁殖、屠宰和收益机制。

- MOD ID：`better_feeding_details`
- 程序集：`BetterFeedingDetails.dll`（netstandard2.1 / C# 7.3 / Harmony 2）
- 依赖：无，不声明任何能力
- 游戏版本：`0.46`

> 本仓库只收录自研源码。游戏依赖 DLL（`Managed/`）、反编译工具、第三方 modding 文档与可安装交付产物（`mod/BetterFeedingDetails/release/`）均不在本仓库中。

## 特性

1. **价值（所有鱼即时可见）**：按原版屠宰方法计算当前鱼肉片 / 边角料 / 植皮片的基础价值，废品级 ×25%。仅依赖鱼的累计摄食热量与鱼种，**历史鱼打开详情即可显示，无需先喂一轮**。详情中不再显示花费、利润与食物消耗构成。
2. **每日食物需要量 / 可维持天数（RAS 详情）**：对缸内每条活鱼 / 每窝鱼苗调用原版 `AquacultureHelper.GetExpectedDrainTonight` 并求和；再读取分配器 `STORED_KCAL_TAG`，除以日需求量得到可维持天数。公式为 `max(5, 维持热量 + 1.2 × 本夜增长等效克数)`，老年维持减半、鱼苗群只算增长；详见 mod 包内 README 的 wiki 核对结论。
3. **鱼图标标记（所有鱼即时可见）**：读取 `FISH_SEX_TAG` 与 `FISH_AGE_TAG`，在图标左上角绘制运行时生成的像素徽标——雄性蓝色 ♂、雌性粉色 ♀，阶段达到 `Adult`（含 `Elder`）时再叠加黄色星星。徽标通过 `ItemSpriteModifier` 叠加，不替换原版鱼精灵，也不写入存档。
4. **账本层（保留但不展示）**：`AddHunger` / `GrowFry` 补丁继续把摄食数据写入鱼自身 tag，随存档 / 移动 / 销毁自动处理；不在详情中输出。

成本计价（工业鱼饲料 0.02/kcal、污泥饲料粒 0.01/kcal）仅保留在数据层，不参与展示。

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
- UI：鱼类价值行走官方钩子 `ModHook.OnCreateTooltipLate`；RAS 两行走 `MachineRAS.CreateTooltip` 的 Transpiler，插在原版分配器储粮行之后。补丁由加载器自动应用（无需手动 `PatchAll`）。
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
│   ├── GrowFryPatch.cs        鱼苗拆分成本分配
│   ├── FishIconPatch.cs       鱼图标徽标挂载点
│   └── RasTooltipPatch.cs     RAS 详情行插入位置（Transpiler）
└── UI/
    ├── FishIconBadges.cs      性别/成年像素徽标生成与叠加
    └── FeedingTooltip.cs      详情提示框注入 + 价值/日需/天数计算
```

## 日志

仅使用 info / error 两级：正常启停走 info，异常走 error；所有补丁处理函数都 catch 异常并回退到原版行为，MOD 不会中断游戏。
