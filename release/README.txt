Spriteman 1.4.0 / Spriteman Smoking 1.2.1
========================================

这是 Spriteman 的预编译发布包。它包含 Content Patcher 内容包和 SMAPI
功能模块，安装后不需要 .NET SDK，也不需要编译源码。

一、必要前提
------------

1. Stardew Valley 1.6 或更高版本。
2. SMAPI 4.0 或更高版本。开发测试环境为 SMAPI 4.5.2。
3. Content Patcher。开发测试环境为 Content Patcher 2.9.1。

SMAPI 和 Content Patcher 不包含在本压缩包中，请从官方渠道单独安装。
Console Commands 和 Save Backup 不是 Spriteman 的运行前提，只是可选的
测试/存档工具。

二、安装方法
------------

1. 关闭 Stardew Valley 和 SMAPI。
2. 打开本压缩包，将其中的全部内容解压到 Stardew Valley 的 Mods 文件夹，
   不是解压到 Mods 文件夹的上一层。
3. Windows 默认路径示例：

   E:\Steam\steamapps\common\Stardew Valley\Mods\

   如果你的游戏安装在其他盘符，请使用自己的 Stardew Valley\Mods 路径。
4. 通过 SMAPI 启动游戏。

安装完成后，文件夹结构应为：

Mods\
|-- [CP] Spriteman\
|   |-- assets\
|   |-- i18n\
|   |-- content.json
|   `-- manifest.json
`-- [SMAPI] Spriteman Smoking\
    |-- i18n\
    |-- manifest.json
    `-- Spriteman.Smoking.dll

不要把 `[CP] Spriteman` 或 `[SMAPI] Spriteman Smoking` 再套一层同名文件夹。
如果 SMAPI 的 Mods 目录中已经有旧版 Spriteman，请直接覆盖这两个同名文件夹
中的文件，不要同时保留两个不同 UniqueID 的 Spriteman 副本。

三、主要内容
------------

- Betel Nut 和 Tobacco 作物、脱水机加工、造纸机和卷烟配方。
- 基础 Vape Juice Pod 和可重复使用的 E-Cigarette。
- Apple、Blueberry、Melon、Strawberry 四种水果味烟弹。
- 每个烟弹 20 次使用，并分别提供对应的临时增益。
- 电子烟不可堆叠，每支设备独立保存烟弹口味和剩余次数。

四、首次测试建议
----------------

进入游戏后，确认 SMAPI 日志显示以下两个 Mod：

- (CP) Spriteman 1.4.0
- Spriteman Smoking 1.2.1

手持空电子烟按正常交互键即可装入背包中找到的第一个兼容烟弹。装入后
会显示 20 次，之后每次吸食减少 1 次；烟弹用完后可以再次装入新的烟弹。

五、卸载方法
------------

关闭游戏后，从 Mods 文件夹删除以下两个文件夹即可：

- [CP] Spriteman
- [SMAPI] Spriteman Smoking

本包不修改 Stardew Valley 原版文件。删除 Mod 前请备份存档。

六、项目与许可
--------------

项目地址：https://github.com/JuhaoChen666/Spriteman
许可证：Apache License 2.0
