#!/usr/bin/env python3
import json
import sys
from pathlib import Path

from PIL import Image, ImageDraw

def load_data(path):
    with open(path, "r", encoding="utf-8") as f:
        data = json.load(f)

    segments = data["segments"]
    stray_indices = set(data.get("stray_segment_indices", []))
    pixels_per_unit = data.get("pixels_per_unit", None)

    return segments, stray_indices, pixels_per_unit

def compute_bbox(segments):
    xs = []
    ys = []
    for seg in segments:
        boundary = seg["boundary"]
        for x, y in boundary:
            xs.append(x)
            ys.append(y)
        for hole in seg.get("holes", []):
            for x, y in hole:
                xs.append(x)
                ys.append(y)

    if not xs or not ys:
        raise ValueError("No coordinates found in segments")

    return min(xs), min(ys), max(xs), max(ys)

def transform(point, xmin, ymin, ymax, scale):
    x, y = point
    # shift to origin
    x = (x - xmin) * scale
    y = (y - ymin) * scale
    return (x, y)

def main():
    if len(sys.argv) != 3:
        print("Usage: python3 visualize_segments.py input.json output.png")
        sys.exit(1)

    in_path = Path(sys.argv[1])
    out_path = Path(sys.argv[2])

    segments, stray_indices, pixels_per_unit = load_data(in_path)

    xmin, ymin, xmax, ymax = compute_bbox(segments)
    width_units = xmax - xmin
    height_units = ymax - ymin

    if width_units <= 0 or height_units <= 0:
        raise ValueError("Degenerate bounding box")

    if pixels_per_unit is None:
        # auto-scale so max dimension is around 2000 px
        max_dim_units = max(width_units, height_units)
        target_pixels = 2000.0
        pixels_per_unit = target_pixels / max_dim_units

    width_px = int(width_units * pixels_per_unit) + 20
    height_px = int(height_units * pixels_per_unit) + 20

    img = Image.new("RGB", (width_px, height_px), "white")
    draw = ImageDraw.Draw(img)

    # Colors
    normal_fill = (200, 200, 200)     # light gray
    normal_outline = (100, 100, 100)  # darker gray
    stray_fill = (255, 120, 120)      # light red
    stray_outline = (200, 0, 0)       # red

    for idx, seg in enumerate(segments):
        boundary = seg["boundary"]
        holes = seg.get("holes", [])

        is_stray = idx in stray_indices
        fill_color = stray_fill if is_stray else normal_fill
        outline_color = stray_outline if is_stray else normal_outline

        # Transform boundary
        b_points = [transform(pt, xmin, ymin, ymax, pixels_per_unit) for pt in boundary]
        # Draw boundary filled
        draw.polygon(b_points, fill=fill_color, outline=outline_color)

        # Draw holes as white cutouts
        for hole in holes:
            h_points = [transform(pt, xmin, ymin, ymax, pixels_per_unit) for pt in hole]
            draw.polygon(h_points, fill="white", outline=normal_outline)

    img.save(out_path)
    print(f"Saved {out_path}")

if __name__ == "__main__":
    main()

