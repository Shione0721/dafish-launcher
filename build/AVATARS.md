# 大肥鱼启动器 - 头像素材说明

## 生成方式
素材来源：桌宠 ds-whale-girl 的 previews\idle.png（268x268，本机唯一成品图，Live2D 图集无法切图）
生成脚本：_make_avatars4.py（Pillow 12.3.0）+ _make_avatars5.py（v0.4 补第七个）
另：_make_avatar_full.py 生成左上角完整立绘 WhaleAvatar.png

思路：
1. 裁切 (0, 40, 186, 226) —— 聚焦「头饰 + 完整脸」，右边留 186 完全避开右下角那个桌宠道具图标
2. 每个变体先做色调偏向（Image.blend），update/voice 额外水平镜像
3. 右下角贴一个白色粗描边 + 彩色内芯的小角标（星/齿轮/放大镜/音符/月牙/箭头）

## 产物（均 200x200）
- WhaleAvatar.png 左上大头像（完整立绘 + 金色圆角描边）
- av_open.png    打开/关闭界面  原色   + 金色星星
- av_update.png  检查插件      冷蓝   + 齿轮（镜像）
- av_check.png   检查本体      紫     + 放大镜
- av_voice.png   启动/关闭语音 绿     + 音符（镜像）
- av_close.png   关闭          灰蓝   + 月牙
- av_version.png 检查启动器    暖橙   + 向上箭头（v0.4 新增）

## 重新生成
& "<Pillow 的 python>" build\_make_avatars4.py
& "<Pillow 的 python>" build\_make_avatars5.py
然后重新编译（七个 /resource 都要带，逗号写进引号内）
