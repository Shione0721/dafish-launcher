# -*- coding: utf-8 -*-
# _make_avatars5.py —— 追加生成第 7 个按钮头像：av_version.png（检查/更新启动器）
from PIL import Image, ImageDraw
import os

SRC = r"C:\Users\29245\.dsh\pets\ds-whale-girl\previews\idle.png"
OUT = r"C:\Users\29245\Documents\dsh-launcher\build"
S = 200

src = Image.open(SRC).convert("RGBA")
# 与 _make_avatars4.py 同样的裁切：聚焦「头饰 + 完整脸」，避开右下角桌宠道具
crop = src.crop((0, 40, 186, 226)).resize((S, S), Image.LANCZOS)


def tint(img, rgb, strength):
    r, g, b, a = img.split()
    b3 = Image.merge("RGB", (r, g, b))
    t = Image.blend(b3, Image.new("RGB", b3.size, rgb), strength)
    return Image.merge("RGBA", (*t.split(), a))


def badge_up(img, accent):
    """右下角角标：向上箭头（升级）—— 白色粗描边 + 彩色内芯"""
    d = ImageDraw.Draw(img, "RGBA")
    S0 = img.size[0]
    br = int(S0 * 0.125)
    cx, cy = int(S0 * 0.80), int(S0 * 0.80)
    ow = max(2, S0 // 60)
    w2 = ow + 3
    d.line([cx, cy + br * 0.80, cx, cy - br * 0.45], fill=(255, 255, 255, 255), width=w2 + 5)
    d.line([cx, cy + br * 0.80, cx, cy - br * 0.45], fill=accent, width=w2)
    d.polygon([(cx, cy - br * 1.00), (cx - br * 0.60, cy - br * 0.28), (cx + br * 0.60, cy - br * 0.28)], fill=(255, 255, 255, 255))
    d.polygon([(cx, cy - br * 0.82), (cx - br * 0.42, cy - br * 0.36), (cx + br * 0.42, cy - br * 0.36)], fill=accent)
    return img


im = tint(crop, (0x9A, 0x5A, 0x1E), 0.18)   # 暖橙
im = badge_up(im, (0xFF, 0xB0, 0x4A))
im.save(os.path.join(OUT, "av_version.png"), "PNG", optimize=True)
print("ok " + os.path.join(OUT, "av_version.png"))
