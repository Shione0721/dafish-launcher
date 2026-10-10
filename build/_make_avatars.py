
from PIL import Image, ImageDraw, ImageOps, ImageFilter
import math, os

SRC = r"C:\Users\29245\.dsh\pets\ds-whale-girl\previews\idle.png"
OUT = r"C:\Users\29245\Documents\dsh-launcher\build"
SIZE = 160

base = Image.open(SRC).convert("RGBA").resize((SIZE, SIZE), Image.LANCZOS)

def tint(img, rgb, strength):
    r, g, b, a = img.split()
    b3 = Image.merge("RGB", (r, g, b))
    layer = Image.new("RGB", b3.size, rgb)
    t = Image.blend(b3, layer, strength)
    return Image.merge("RGBA", (*t.split(), a))

def star_pts(cx, cy, R, r, n=5):
    pts = []
    for i in range(n * 2):
        ang = -math.pi / 2 + i * math.pi / n
        rad = R if i % 2 == 0 else r
        pts.append((cx + rad * math.cos(ang), cy + rad * math.sin(ang)))
    return pts

def draw_badge(img, kind, accent):
    d = ImageDraw.Draw(img)
    S = img.size[0]
    br = int(S * 0.26)
    cx, cy = int(S * 0.76), int(S * 0.76)
    # white round plate (so it pops on any tint)
    d.ellipse([cx - br, cy - br, cx + br, cy + br], fill=(255, 255, 255, 235),
              outline=(255, 255, 255, 255), width=max(2, S // 48))
    a = accent
    w = max(2, S // 40)
    if kind == "star":
        pts = star_pts(cx, cy + 1, br * 0.62, br * 0.26)
        d.polygon(pts, fill=a)
    elif kind == "gear":
        for i in range(8):
            ang = i * math.pi / 4
            x1 = cx + math.cos(ang) * br * 0.42; y1 = cy + math.sin(ang) * br * 0.42
            x2 = cx + math.cos(ang) * br * 0.66; y2 = cy + math.sin(ang) * br * 0.66
            d.line([x1, y1, x2, y2], fill=a, width=w + 1)
        d.ellipse([cx - br * 0.40, cy - br * 0.40, cx + br * 0.40, cy + br * 0.40], fill=a)
        d.ellipse([cx - br * 0.14, cy - br * 0.14, cx + br * 0.14, cy + br * 0.14], fill=(255, 255, 255, 255))
    elif kind == "search":
        rr = br * 0.40
        ox, oy = cx - br * 0.10, cy - br * 0.10
        d.ellipse([ox - rr, oy - rr, ox + rr, oy + rr], outline=a, width=w + 1)
        d.line([ox + rr * 0.72, oy + rr * 0.72, cx + br * 0.62, cy + br * 0.62], fill=a, width=w + 2)
    elif kind == "note":
        rr = br * 0.36
        d.ellipse([cx - br * 0.34 - rr, cy + br * 0.20 - rr, cx - br * 0.34 + rr, cy + br * 0.20 + rr], fill=a)
        d.line([cx - br * 0.34 + rr * 0.86, cy + br * 0.20, cx - br * 0.34 + rr * 0.86, cy - br * 0.52], fill=a, width=w + 1)
        d.line([cx - br * 0.34 + rr * 0.86, cy - br * 0.52, cx + br * 0.30, cy - br * 0.34], fill=a, width=w + 1)
        d.line([cx + br * 0.30, cy - br * 0.34, cx + br * 0.30, cy - br * 0.02], fill=a, width=w + 1)
    elif kind == "moon":
        d.ellipse([cx - br * 0.62, cy - br * 0.62, cx + br * 0.62, cy + br * 0.62], fill=a)
        d.ellipse([cx - br * 0.16, cy - br * 0.80, cx + br * 1.08, cy + br * 0.34], fill=(255, 255, 255, 255))
    return img

# (文件名, 色调RGB, 强度, 翻转, 角标, 角标色)
variants = [
    ("av_open.png",  (0x3A, 0x6E, 0xC8), 0.06, False, "star",   (0xF7, 0xDE, 0x94)),
    ("av_update.png",(0x2E, 0x7A, 0x9E), 0.16, True,  "gear",   (0x5F, 0xC8, 0xE8)),
    ("av_check.png", (0x8A, 0x4A, 0x9E), 0.16, False, "search", (0xC9, 0x8A, 0xF0)),
    ("av_voice.png", (0x2E, 0x8A, 0x6E), 0.16, True,  "note",   (0x6E, 0xE8, 0xB4)),
    ("av_close.png", (0x55, 0x5F, 0x7A), 0.22, False, "moon",   (0xB8, 0xC4, 0xE0)),
]

if not os.path.isdir(OUT):
    os.makedirs(OUT)

for name, rgb, strength, flip, kind, accent in variants:
    im = tint(base, rgb, strength)
    if flip:
        im = ImageOps.mirror(im)
    im = draw_badge(im, kind, accent)
    p = os.path.join(OUT, name)
    im.save(p, "PNG", optimize=True)
    print(name, im.size, os.path.getsize(p) // 1024, "KB")
