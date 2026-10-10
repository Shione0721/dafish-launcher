
from PIL import Image, ImageDraw, ImageOps, ImageFilter
import math, os

SRC = r"C:\Users\29245\.dsh\pets\ds-whale-girl\previews\idle.png"
OUT = r"C:\Users\29245\Documents\dsh-launcher\build"
TMP = r"C:\Users\29245\AppData\Local\Temp\dswhale"
S = 200

src = Image.open(SRC).convert("RGBA")
W, H = src.size

# --- 1) 抹掉右下角桌宠道具（鼠标）：用左侧对称头发做镜像填补 ---
box = (186, 128, 268, 258)          # 鼠标所在区域
bw = box[2] - box[0]
patch = src.crop((box[0] - bw, box[1], box[0], box[3]))
patch = ImageOps.mirror(patch)
src.paste(patch, (box[0], box[1]))
# 轻微模糊接缝
seam = src.crop(box).filter(ImageFilter.GaussianBlur(2))
src.paste(seam, (box[0], box[1]))
src.save(os.path.join(TMP, "cleaned.png"))

# --- 2) 聚焦构图：小鲸鱼 + 脸 ---
crop = src.crop((26, 26, 240, 240)).resize((S, S), Image.LANCZOS)

def tint(img, rgb, strength):
    r, g, b, a = img.split()
    b3 = Image.merge("RGB", (r, g, b))
    layer = Image.new("RGB", b3.size, rgb)
    t = Image.blend(b3, layer, strength)
    return Image.merge("RGBA", (*t.split(), a))

def corner_badge(img, kind, accent):
    d = ImageDraw.Draw(img, "RGBA")
    S0 = img.size[0]
    br = int(S0 * 0.13)
    cx, cy = int(S0 * 0.815), int(S0 * 0.815)
    ow = max(2, S0 // 64)
    if kind == "star":
        pts = []
        for i in range(10):
            ang = -math.pi/2 + i*math.pi/5
            rad = br if i % 2 == 0 else br*0.42
            pts.append((cx+rad*math.cos(ang), cy+rad*math.sin(ang)))
        d.polygon(pts, fill=(255,255,255,255))
        d.polygon([(cx+(x-cx)*0.76, cy+(y-cy)*0.76) for x, y in pts], fill=accent)
    elif kind == "gear":
        for w2, col in ((ow+4, (255,255,255,255)), (ow+1, accent)):
            for i in range(8):
                ang = i*math.pi/4
                r1 = br*0.40 if col == accent else br*0.36
                r2 = br*0.86 if col == accent else br*0.95
                d.line([cx+math.cos(ang)*r1, cy+math.sin(ang)*r1,
                        cx+math.cos(ang)*r2, cy+math.sin(ang)*r2], fill=col, width=w2)
        d.ellipse([cx-br*0.64, cy-br*0.64, cx+br*0.64, cy+br*0.64], fill=(255,255,255,255))
        d.ellipse([cx-br*0.54, cy-br*0.54, cx+br*0.54, cy+br*0.54], fill=accent)
        d.ellipse([cx-br*0.17, cy-br*0.17, cx+br*0.17, cy+br*0.17], fill=(255,255,255,255))
    elif kind == "search":
        rr = br*0.54
        ox, oy = cx-br*0.14, cy-br*0.14
        d.line([ox+rr*0.72, oy+rr*0.72, cx+br*0.86, cy+br*0.86], fill=(255,255,255,255), width=ow+5)
        d.line([ox+rr*0.72, oy+rr*0.72, cx+br*0.80, cy+br*0.80], fill=accent, width=ow+2)
        d.ellipse([ox-rr, oy-rr, ox+rr, oy+rr], outline=(255,255,255,255), width=ow+4)
        d.ellipse([ox-rr*0.84, oy-rr*0.84, ox+rr*0.84, oy+rr*0.84], outline=accent, width=ow+2)
    elif kind == "note":
        rr = br*0.46
        def note(col):
            d.ellipse([cx-br*0.32-rr, cy+br*0.24-rr, cx-br*0.32+rr, cy+br*0.24+rr], fill=col)
            d.line([cx-br*0.32+rr*0.85, cy+br*0.24, cx-br*0.32+rr*0.85, cy-br*0.60], fill=col, width=ow+3)
            d.line([cx-br*0.32+rr*0.85, cy-br*0.60, cx+br*0.44, cy-br*0.38], fill=col, width=ow+3)
            d.line([cx+br*0.44, cy-br*0.38, cx+br*0.44, cy-br*0.02], fill=col, width=ow+3)
        note((255,255,255,255)); note(accent)
    elif kind == "moon":
        d.ellipse([cx-br*0.84, cy-br*0.84, cx+br*0.84, cy+br*0.84], fill=(255,255,255,255))
        d.ellipse([cx-br*0.30, cy-br*1.04, cx+br*1.06, cy+br*0.30], fill=(0,0,0,0))
        d.ellipse([cx-br*0.60, cy-br*0.60, cx+br*0.60, cy+br*0.60], fill=accent)
        # 挖出月牙：用透明圆覆盖
        cut = Image.new("RGBA", img.size, (0,0,0,0))
        cd = ImageDraw.Draw(cut)
        cd.ellipse([cx-br*0.28, cy-br*1.02, cx+br*1.08, cy+br*0.32], fill=(255,255,255,255))
        img.alpha_composite(cut)
    return img

variants = [
    ("av_open.png",   (0x30,0x60,0xB8), 0.05, False, "star",   (0xFF,0xD1,0x4A)),
    ("av_update.png", (0x22,0x6E,0x96), 0.14, True,  "gear",   (0x49,0xB8,0xE0)),
    ("av_check.png",  (0x7A,0x40,0x92), 0.14, False, "search", (0xC0,0x76,0xEE)),
    ("av_voice.png",  (0x22,0x7E,0x62), 0.14, True,  "note",   (0x54,0xE0,0xA0)),
    ("av_close.png",  (0x48,0x54,0x70), 0.20, False, "moon",   (0xA8,0xB6,0xD8)),
]
for name, rgb, st, flip, kind, accent in variants:
    im = tint(crop, rgb, st)
    if flip: im = ImageOps.mirror(im)
    im = corner_badge(im, kind, accent)
    im.save(os.path.join(OUT, name), "PNG", optimize=True)

big = crop.copy()
d = ImageDraw.Draw(big, "RGBA")
d.rounded_rectangle([1, 1, S-2, S-2], radius=int(S*0.16), outline=(232,200,120,255), width=max(3, S//40))
big.save(os.path.join(OUT, "avatar.png"), "PNG", optimize=True)
print("done")

sheet = Image.new("RGBA", (160*6, 160), (24,32,52,255))
for i, n in enumerate(["avatar.png","av_open.png","av_update.png","av_check.png","av_voice.png","av_close.png"]):
    im = Image.open(os.path.join(OUT, n)).convert("RGBA").resize((150,150), Image.LANCZOS)
    sheet.paste(im, (i*160+5, 5), im)
sheet.save(os.path.join(TMP, "sheet2.png"))
print("sheet2 ok")
