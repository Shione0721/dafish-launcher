
from PIL import Image, ImageDraw, ImageOps, ImageFilter
import math, os

SRC = r"C:\Users\29245\.dsh\pets\ds-whale-girl\previews\idle.png"
OUT = r"C:\Users\29245\Documents\dsh-launcher\build"
S = 200

src = Image.open(SRC).convert("RGBA")
# 原图右下角有个桌宠道具（鼠标），裁掉它：只取左上 3/4 区域再放大
crop = src.crop((0, 0, 196, 190))
base = crop.resize((S, S), Image.LANCZOS)

def tint(img, rgb, strength):
    r, g, b, a = img.split()
    b3 = Image.merge("RGB", (r, g, b))
    layer = Image.new("RGB", b3.size, rgb)
    t = Image.blend(b3, layer, strength)
    return Image.merge("RGBA", (*t.split(), a))

def corner_badge(img, kind, accent):
    d = ImageDraw.Draw(img, "RGBA")
    S0 = img.size[0]
    br = int(S0 * 0.14)              # 小角标，不遮脸
    cx, cy = int(S0 * 0.80), int(S0 * 0.80)
    ow = max(2, S0 // 64)
    # 白色描边的彩色图形（不铺白底盘）
    if kind == "star":
        pts = []
        for i in range(10):
            ang = -math.pi/2 + i*math.pi/5
            rad = br if i % 2 == 0 else br*0.42
            pts.append((cx+rad*math.cos(ang), cy+rad*math.sin(ang)))
        d.polygon(pts, fill=(255,255,255,255), outline=None)
        pts2 = [(cx+(x-cx)*0.78, cy+(y-cy)*0.78) for x, y in pts]
        d.polygon(pts2, fill=accent)
    elif kind == "gear":
        for i in range(8):
            ang = i*math.pi/4
            d.line([cx+math.cos(ang)*br*0.42, cy+math.sin(ang)*br*0.42,
                    cx+math.cos(ang)*br*0.95, cy+math.sin(ang)*br*0.95], fill=(255,255,255,255), width=ow+3)
        for i in range(8):
            ang = i*math.pi/4
            d.line([cx+math.cos(ang)*br*0.42, cy+math.sin(ang)*br*0.42,
                    cx+math.cos(ang)*br*0.88, cy+math.sin(ang)*br*0.88], fill=accent, width=ow+1)
        d.ellipse([cx-br*0.62, cy-br*0.62, cx+br*0.62, cy+br*0.62], fill=(255,255,255,255))
        d.ellipse([cx-br*0.52, cy-br*0.52, cx+br*0.52, cy+br*0.52], fill=accent)
        d.ellipse([cx-br*0.18, cy-br*0.18, cx+br*0.18, cy+br*0.18], fill=(255,255,255,255))
    elif kind == "search":
        rr = br*0.55
        ox, oy = cx-br*0.12, cy-br*0.12
        d.line([ox+rr*0.7, oy+rr*0.7, cx+br*0.85, cy+br*0.85], fill=(255,255,255,255), width=ow+4)
        d.line([ox+rr*0.7, oy+rr*0.7, cx+br*0.78, cy+br*0.78], fill=accent, width=ow+1)
        d.ellipse([ox-rr, oy-rr, ox+rr, oy+rr], outline=(255,255,255,255), width=ow+3)
        d.ellipse([ox-rr*0.86, oy-rr*0.86, ox+rr*0.86, oy+rr*0.86], outline=accent, width=ow+1)
    elif kind == "note":
        rr = br*0.48
        d.ellipse([cx-br*0.30-rr, cy+br*0.22-rr, cx-br*0.30+rr, cy+br*0.22+rr], fill=(255,255,255,255))
        d.line([cx-br*0.30+rr*0.85, cy+br*0.22, cx-br*0.30+rr*0.85, cy-br*0.62], fill=(255,255,255,255), width=ow+3)
        d.line([cx-br*0.30+rr*0.85, cy-br*0.62, cx+br*0.42, cy-br*0.40], fill=(255,255,255,255), width=ow+3)
        d.line([cx+br*0.42, cy-br*0.40, cx+br*0.42, cy-br*0.02], fill=(255,255,255,255), width=ow+3)
        rr2 = rr*0.74
        d.ellipse([cx-br*0.30-rr2, cy+br*0.22-rr2, cx-br*0.30+rr2, cy+br*0.22+rr2], fill=accent)
        d.line([cx-br*0.30+rr*0.85, cy+br*0.22, cx-br*0.30+rr*0.85, cy-br*0.62], fill=accent, width=ow+1)
        d.line([cx-br*0.30+rr*0.85, cy-br*0.62, cx+br*0.42, cy-br*0.40], fill=accent, width=ow+1)
        d.line([cx+br*0.42, cy-br*0.40, cx+br*0.42, cy-br*0.02], fill=accent, width=ow+1)
    elif kind == "moon":
        d.ellipse([cx-br*0.82, cy-br*0.82, cx+br*0.82, cy+br*0.82], fill=(255,255,255,255))
        d.ellipse([cx-br*0.58, cy-br*0.58, cx+br*0.58, cy+br*0.58], fill=accent)
        d.ellipse([cx-br*0.30, cy-br*1.02, cx+br*1.06, cy+br*0.30], fill=None)
    return img

variants = [
    ("av_open.png",   (0x30,0x60,0xB8), 0.05, False, "star",   (0xFF,0xD1,0x4A)),
    ("av_update.png", (0x22,0x6E,0x96), 0.15, True,  "gear",   (0x49,0xB8,0xE0)),
    ("av_check.png",  (0x7A,0x40,0x92), 0.15, False, "search", (0xC0,0x76,0xEE)),
    ("av_voice.png",  (0x22,0x7E,0x62), 0.15, True,  "note",   (0x54,0xE0,0xA0)),
    ("av_close.png",  (0x48,0x54,0x70), 0.20, False, "moon",   (0xA8,0xB6,0xD8)),
]
for name, rgb, st, flip, kind, accent in variants:
    im = tint(base, rgb, st)
    if flip: im = ImageOps.mirror(im)
    im = corner_badge(im, kind, accent)
    im.save(os.path.join(OUT, name), "PNG", optimize=True)
    print(name, im.size)

# 大头像也用干净底图（去掉鼠标），加金色描边底
big = base.copy()
d = ImageDraw.Draw(big, "RGBA")
d.rounded_rectangle([0, 0, S-1, S-1], radius=int(S*0.16), outline=(232,200,120,255), width=max(3, S//40))
big.save(os.path.join(OUT, "avatar.png"), "PNG", optimize=True)
print("avatar.png", big.size)
