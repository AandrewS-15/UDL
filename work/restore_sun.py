from pathlib import Path
p=Path('outputs/assets/sol-uruguay.svg')
s=p.read_text(encoding='utf-8-sig')
s=s.replace('#fcd116','#0b0f17').replace('#7b3f00','#1c3d56')
s=s.replace('<title>Flag of Uruguay</title>','<title>Sol de Mayo — trazado original en azul oscuro</title>')
# Retain source fills: they occlude ray intersections and preserve the face.
Path('outputs/assets/sol-uruguay-original-azul.svg').write_text(s,encoding='utf-8')
