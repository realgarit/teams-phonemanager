#!/usr/bin/env python3
"""Generate app icon rasters (icon-512.png, icon.ico, icon.iconset/) from the
logo geometry. The geometry mirrors assets/icon.svg — keep both in sync.

Requires Pillow. The .icns is produced afterwards with:
    iconutil -c icns assets/icon.iconset -o assets/icon.icns
"""

import math
import os
from PIL import Image, ImageDraw

BASE = 1024  # master render size
SS = 4       # supersampling factor
S = BASE * SS

# Palette: matches the app's BrandGradientBrush (App.axaml), white glyph.
GRAD_TOP = (106, 111, 221)   # #6A6FDD
GRAD_BOTTOM = (75, 77, 158)  # #4B4D9E
WHITE = (255, 255, 255, 255)


def rounded_badge():
    img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    # vertical gradient
    grad = Image.new("RGBA", (1, S))
    for y in range(S):
        t = y / (S - 1)
        c = tuple(round(GRAD_TOP[i] + (GRAD_BOTTOM[i] - GRAD_TOP[i]) * t) for i in range(3))
        grad.putpixel((0, y), c + (255,))
    grad = grad.resize((S, S))
    mask = Image.new("L", (S, S), 0)
    d = ImageDraw.Draw(mask)
    r = round(S * 0.225)  # macOS-like corner radius
    d.rounded_rectangle([0, 0, S - 1, S - 1], radius=r, fill=255)
    img.paste(grad, (0, 0), mask)
    return img


def draw_glyph(img):
    """Dial-pad glyph: 3x3 dots plus a bottom bar. Legible at 16px."""
    d = ImageDraw.Draw(img)
    gap = S * 0.155              # spacing between dot centers
    r = S * 0.058                # dot radius
    # The glyph spans from (cy - gap - r) to (cy + 2*gap + r); solving for the
    # optical center at S/2 gives cy = S/2 - gap/2.
    cx, cy = S * 0.5, S * 0.5 - gap / 2

    for row in range(3):
        for col in range(3):
            x = cx + (col - 1) * gap
            y = cy + (row - 1) * gap
            d.ellipse([x - r, y - r, x + r, y + r], fill=WHITE)

    # bottom bar (the "0" row), rounded
    bar_y = cy + 2 * gap
    bar_half = gap * 1.0 + r
    d.rounded_rectangle([cx - bar_half, bar_y - r, cx + bar_half, bar_y + r],
                        radius=r, fill=WHITE)
    return img


def main():
    here = os.path.dirname(os.path.abspath(__file__))
    master = draw_glyph(rounded_badge()).resize((BASE, BASE), Image.LANCZOS)

    master.resize((512, 512), Image.LANCZOS).save(os.path.join(here, "icon-512.png"))

    # Windows .ico (multi-resolution)
    ico_src = master.resize((256, 256), Image.LANCZOS)
    ico_src.save(os.path.join(here, "icon.ico"),
                 sizes=[(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)])

    # macOS iconset
    iconset = os.path.join(here, "icon.iconset")
    os.makedirs(iconset, exist_ok=True)
    for pts in (16, 32, 128, 256, 512):
        for scale in (1, 2):
            px = pts * scale
            name = f"icon_{pts}x{pts}" + ("@2x" if scale == 2 else "") + ".png"
            master.resize((px, px), Image.LANCZOS).save(os.path.join(iconset, name))

    print("Wrote icon-512.png, icon.ico, icon.iconset/")


if __name__ == "__main__":
    main()
