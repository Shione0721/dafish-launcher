
from PIL import Image, ImageDraw
import os
SRC = r"C:\Users\29245\.dsh\pets\ds-whale-girl\previews\idle.png"
OUT = r"C:\Users\29245\Documents\dsh-launcher\build"
S = 200
src = Image.open(SRC).convert("RGBA")
W, H = src.size
crop = src.crop((0, 0, 184, H))
scale = S / float(H)
nw = int(round(184 * scale))
art = crop.resize((nw, S), Image.LANCZOS)

canvas = Image.new("RGBA", (S, S), (0, 0, 0, 0))
d = ImageDraw.Draw(canvas)
d.rounded_rectangle([0, 0, S - 1, S - 1], radius=int(S * 0.16), fill=(26, 36, 68, 255))
ox = (S - nw) // 2
# 左右补同色深蓝渐变，和立绘边缘自然衔接
for x in range(nw, S):
    t = (x - nw) / float(max(1, S - nw))
    d.line([(x, 0), (x, S - 1)], fill=(int(26 - 6 * t), int(36 - 9 * t), int(68 - 16 * t), 255))
canvas.paste(art, (ox, 0), art)
d.rounded_rectangle([1, 1, S - 2, S - 2], radius=int(S * 0.16), outline=(232, 200, 120, 255), width=max(3, S // 40))
canvas.save(os.path.join(OUT, "avatar.png"), "PNG", optimize=True)
print("done", nw)
