from pathlib import Path

from PIL import Image, ImageOps


folder = Path(__file__).parent
pages = sorted(folder.glob("page-*.png"))
thumb_size = (620, 877)

for group_index, start in enumerate(range(0, len(pages), 6), 1):
    sheet = Image.new("RGB", (thumb_size[0] * 3, thumb_size[1] * 2), "#d0d0d0")
    for page_index, page_path in enumerate(pages[start : start + 6]):
        with Image.open(page_path) as source:
            thumb = ImageOps.contain(source.convert("RGB"), thumb_size)
        x = (page_index % 3) * thumb_size[0]
        y = (page_index // 3) * thumb_size[1]
        sheet.paste(thumb, (x, y))
    sheet.save(folder / f"contact-{group_index:02d}.jpg", quality=90)
