
from PIL import Image, ImageDraw, ImageOps
import math, os

SRC = r"C:\Users\29245\.dsh\pets\ds-whale-girl\previews\idle.png"
OUT = r"C:\Users\29245\Documents\dsh-launcher\build"
TMP = r"C:\Users\29245\AppData\Local\Temp\dswhale"
S = 200

src = Image.open(SRC).convert("RGBA")
# 裁切：聚焦「头饰 + 完整脸」，x 到 186 完全避开右下角桌宠道具
crop = src.crop((0, 40, 186, 226)).resize((S, S), Image.LANCZOS)

def tint(img, rgb, strength):
    r, g, b, a = img.split()
    b3 = Image.merge("RGB", (r, g, b))
    t = Image.blend(b3, Image.new("RGB", b3.size, rgb), strength)
    return Image.merge("RGBA", (*t.split(), a))

def badge(img, kind, accent):
    d = ImageDraw.Draw(img, "RGBA")
    S0 = img.size[0]
    br = int(S0 * 0.125)
    cx, cy = int(S0 * 0.80), int(S0 * 0.80)
    ow = max(2, S0 // 60)
    if kind == "star":
        pts = [(cx + (br if i % 2 == 0 else br*0.42)*math.cos(-math.pi/2 + i*math.pi/5),
                cy + (br if i % 2 == 0 else br*0.42)*math.sin(-math.pi/2 + i*math.pi/5)) for i in range(10)]
        d.polygon([(cx+(x-cx)*1.30, cy+(y-cy)*1.30) for x, y in pts], fill=(255,255,255,255))
        d.polygon(pts, fill=accent)
    elif kind == "gear":
        for w2, r1, r2, col in ((ow+5, br*0.34, br*0.98, (255,255,255,255)), (ow+1, br*0.40, br*0.84, accent)):
            for i in range(8):
                ang = i*math.pi/4
                d.line([cx+math.cos(ang)*r1, cy+math.sin(ang)*r1, cx+math.cos(ang)*r2, cy+math.sin(ang)*r2], fill=col, width=w2)
        d.ellipse([cx-br*0.68, cy-br*0.68, cx+br*0.68, cy+br*0.68], fill=(255,255,255,255))
        d.ellipse([cx-br*0.56, cy-br*0.56, cx+br*0.56, cy+br*0.56], fill=accent)
        d.ellipse([cx-br*0.18, cy-br*0.18, cx+br*0.18, cy+br*0.18], fill=(255,255,255,255))
    elif kind == "search":
        rr = br*0.50
        ox, oy = cx-br*0.18, cy-br*0.18
        d.line([ox+rr*0.70, oy+rr*0.70, cx+br*0.92, cy+br*0.92], fill=(255,255,255,255), width=ow+6)
        d.line([ox+rr*0.70, oy+rr*0.70, cx+br*0.86, cy+br*0.86], fill=accent, width=ow+2)
        d.ellipse([ox-rr, oy-rr, ox+rr, oy+rr], outline=(255,255,255,255), width=ow+5)
        d.ellipse([ox-rr*0.86, oy-rr*0.86, ox+rr*0.86, oy+rr*0.86], outline=accent, width=ow+2)
    elif kind == "note":
        rr = br*0.42
        def note(col, k):
            d.ellipse([cx-br*0.34-rr, cy+br*0.26-rr, cx-br*0.34+rr, cy+br*0.26+rr], fill=col)
            d.line([cx-br*0.34+rr*0.85, cy+br*0.26, cx-br*0.34+rr*0.85, cy-br*0.62], fill=col, width=ow+3)
            d.line([cx-br*0.34+rr*0.85, cy-br*0.62, cx+br*0.46, cy-br*0.40], fill=col, width=ow+3)
            d.line([cx+br*0.46, cy-br*0.40, cx+br*0.46, cy-br*0.02], fill=col, width=ow+3)
        note((255,255,255,255), 1.24); note(accent, 1.0)
    elif kind == "moon":
        d.ellipse([cx-br*0.90, cy-br*0.90, cx+br*0.90, cy+br*0.90], fill=(255,255,255,255))
        d.ellipse([cx-br*0.62, cy-br*0.62, cx+br*0.62, cy+br*0.62], fill=accent)
        cut = Image.new("RGBA", img.size, (0,0,0,0))
        ImageDraw.Draw(cut).ellipse([cx-br*0.20, cy-br*1.04, cx+br*1.10, cy+br*0.34], fill=(255,255,255,255))
        img.alpha_composite(cut)
    return img

variants = [
    ("av_open.png",   (0x30,0x60,0xB8), 0.05, False, "star",   (0xFF,0xD1,0x4A)),
    ("av_update.png", (0x22,0x6E,0x96), 0.15, True,  "gear",   (0x49,0xB8,0xE0)),
    ("av_check.png",  (0x7A,0x40,0x92), 0.15, False, "search", (0xC0,0x76,0xEE)),
    ("av_voice.png",  (0x22,0x7E,0x62), 0.15, True,  "note",   (0x54,0xE0,0xA0)),
    ("av_close.png",  (0x44,0x50,0x6C), 0.22, False, "moon",   (0xA8,0xB6,0xD8)),
]
for name, rgb, st, flip, kind, accent in variants:
    im = tint(crop, rgb, st)
    if flip: im = ImageOps.mirror(im)
    im = badge(im, kind, accent)
    im.save(os.path.join(OUT, name), "PNG", optimize=True)

big = crop.copy()
ImageDraw.Draw(big, "RGBA").rounded_rectangle([1, 1, S-2, S-2], radius=int(S*0.16), outline=(232,200,120,255), width=max(3, S//38))
big.save(os.path.join(OUT, "avatar.png"), "PNG", optimize=True)

sheet = Image.new("RGBA", (160*6, 160), (24,32,52,255))
for i, n in enumerate(["avatar.png","av_open.png","av_update.png","av_check.png","av_voice.png","av_close.png"]):
    im = Image.open(os.path.join(OUT, n)).convert("RGBA").resize((150,150), Image.LANCZOS)
    sheet.paste(im, (i*160+5, 5), im)
sheet.save(os.path.join(TMP, "sheet3.png"))
print("ok")
