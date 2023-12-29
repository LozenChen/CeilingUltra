# CeilingUltra

https://gamebanana.com/mods/485472

本文档的最后更新时间: v1.2.0, 请自行留意这是否是最新版本.

Mod 兼容性:

与 Mod 的各向 JumpThru 兼容.

与 GravityHelper 的重力反转内容不兼容, 但你仍然可以在常规重力下正确使用 GravityHelper.UpsideDownJumpThru.

机制的一些细节:

你只需要紧挨着天花板/左墙/右墙即可获得对应的狼时间, 没有速度要求. 不过 StDreamDash 中照例无法获得这些狼时间. 在 DreamDashEnd 你也无法获得地面狼跳时间以外的狼跳时间.

如果你紧挨着带冰的天花板/左墙/右墙, 那么你不能从这个墙面上恢复体力/冲刺. 但如果你在一个角落, 一面带冰而另一面不带, 你仍然能从不带冰的那一面恢复体力/冲刺.

你可以反向天花板 super/hyper, 反向竖直 hyper.

斜冲左右墙可以被墙跳打断竖直 ultra, 但斜冲天花板不能被蹬天花板跳打断天花板 ultra. 这是因为 DashUpdate 中不允许你在地面上进行狼跳, 因此我们也不允许你蹬天花板跳.

竖直向上 hyper 尽管只有 6 帧跳跃长度, 但此时撞到头并不会取消跳跃, 我们有机制保护这一点. 这使得你很容易绕过 1px 的凸出角落 (当然, 鉴于水平速度并不高, 你还需要按住远离墙面的方向).

斜上冲结尾保留竖直速度的机制: 我们以冲刺方向是正斜上为例 (除非你是 超冲 或 360度冲刺), 竖直速度大于 -169.71 则结束后变为 -84.85, 竖直速度小于 -325 则不变, 在两者之间则线性插值.

对于新增加的这些动作 (蹬天花板跳, 天花板 super/hyper, 竖直 hyper), 向下的速度分量并不会获得 LiftBoost.

蹬天花板跳会将 maxFall 设置为 240, 而速度为 +105. 如果蹬天花板时按了下, 那么 maxFall 320, 速度为 +280.

向下竖直 hyper 会将 maxFall 设置为 350.

斜向撞左右墙时, 如果你不是蹲姿 (或者 starFlyHurtbox), 那没什么需要检验的. 但如果是, 则需要检验碰撞箱变为挤压碰撞箱之后, 是否满足地形. 在变化碰撞箱前后, 如果是向左上撞墙 (这里的左上指速度方向而非 DashDir), 则保持碰撞箱左下不变; 向左下撞墙保持碰撞箱左上不变; 右上/下同理. 纵向速度为零的情况视作向上.

斜向撞天花板时, 需要进入蹲姿, 这个过程保持头顶高度不变 (即刚好顶着天花板). 如果原先是挤压碰撞箱, 在尝试进入蹲姿的时候, 水平方向上产生 0, 1, -1 的位移来试图避开固体 (优先顺序取决于速度方向). 如果不能进入蹲姿后成功避开固体, 那么撞天花板 ultra 失败.

天花板 super/hyper 时, 需要解除蹲姿, 这个过程保持头顶高度不变 (即刚好顶着天花板). 如果无法解除蹲姿, 那么不能天花板 super/hyper.

蹬天花板跳时, 会尝试解除蹲姿. 但即使不能解除蹲姿, 也依然能蹬天花板跳.

关于 forceMoveX 究竟何时会起作用, 目前的方案是仅蹬天花板跳会受 forceMoveX 影响 (原版中 super/hyper 也受影响). 但我还没有彻底拿定主意.

冲刺第 5 帧的几种 ultra 的优先级: 地面 ultra > 竖直 ultra > 天花板 ultra. 这保证你在地面靠墙时能顺利做出 reverse hyper.

不同于竖直上冲无法抓跳, 在冲刺并进入挤压态的情况下, 你依然可以抓跳.

我们有特殊的机制保护你仍然能做 delayed ultra: 当一个横向/纵向 ultra 产生时, 玩家会得到一个 OverrideUltra 量, 当你沿 OverrideUltra 所指墙面的法向撞击墙面时, 你的 DashDir 以 OverrideUltra 中记录的方向计算, 并因此能做出一个纵向/横向 ultra. 概况来说就是, (原生的) 横向 ultra 只能转化出纵向 ultra, 纵向 ultra 只能转化出横向 ultra, 转化出的 ultra 无法再次转化. 举例: 斜右下撞地面后, 获得 OverrideUltra(RightWall), 再撞右墙即可做出纵向 ultra. 斜右下撞右墙, 获得 OverrideUltra(Ground), 再撞地即可地面 ultra. 斜右上撞右墙, 获得 OverrideUltra(Ceiling), 再撞天花板即可天花板 ultra. 在所有会重置 DashDir 的值的场合 (e.g. 冲刺开始, ultra 产生), 这些 OverrideUltra 值都会消失, 正如同原版中你无法在这些情形下再次用原先的 DashDir 来获得 ultra. 碰撞墙面/地面/天花板时如果有法向的 CoyoteTime, 或者撞天花板时在 VerticalHyper 的 VarJumpTime 期间, 也会使得 OverrideUltra 值消失而不产生 Ultra (这也是为了增加墙角 reverse hyper 等的容错).

向上蹬墙加速: 仅在 StNormal 与 StClimb 起效, 当触发 WallJump 时, 如果按着上, 那么速度为 -105 与 (原先速度 - 20) 中的最小值再加上 LiftBoost.Y. 当得出的速度大于等于 -105 时, VarJumpTimer = 0.2f. 当速度小于等于 -325 时, VarJumpTimer = 0.1f. 之间则线性插值.

向下蹬墙加速: 仅在 StNormal 与 StClimb 起效, 当触发 WallJump 时, 如果按着下, 那么速度为 +40 与 (原先速度 + 40) 中的最大值. MaxFall = 速度 + 20.

