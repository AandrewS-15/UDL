from pathlib import Path
import math
# Redraw the existing decorative sun as a clean, symmetric outline.
def pt(r,a):
 t=math.radians(a);return f'{500+r*math.cos(t):.2f},{500+r*math.sin(t):.2f}'
parts=['<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1000 1000" width="1000" height="1000"><title>Sol de Mayo UDL, contorno simplificado</title><g fill="none" stroke="#1c3d56" stroke-width="2.3" stroke-linecap="round" stroke-linejoin="round">']
for i in range(16):
 a=-90+i*22.5
 if i%2==0:d=f'M {pt(179,a-6)} L {pt(464,a)} L {pt(179,a+6)}'
 else:d=f'M {pt(179,a-6)} C {pt(265,a-6)} {pt(338,a+6)} {pt(437,a+5)} C {pt(361,a-3)} {pt(282,a+7)} {pt(179,a+6)}'
 parts.append(f'<path d="{d}"/>')
parts.append('<circle cx="500" cy="500" r="170"/>')
# Face uses single, clean paths rather than outlining the filled source shapes twice.
parts.append('<path d="M390 462 Q429 435 468 459 M532 459 Q571 435 610 462"/>')
parts.append('<path d="M394 481 Q430 459 466 481 Q430 504 394 481 Z M534 481 Q570 459 606 481 Q570 504 534 481 Z"/>')
parts.append('<circle cx="430" cy="481" r="10"/><circle cx="570" cy="481" r="10"/>')
parts.append('<path d="M490 478 C490 503 487 516 478 530 Q486 539 500 533 Q514 539 522 530 C513 516 510 503 510 478 M454 572 Q477 558 500 563 Q523 558 546 572 Q500 600 454 572 Z M461 573 Q500 579 539 573 M484 617 Q500 623 516 617"/>')
parts.append('</g></svg>')
Path('outputs/assets/sol-uruguay-limpio.svg').write_text(''.join(parts),encoding='utf-8')
p=Path('outputs/udl-app.js');s=p.read_text(encoding='utf-8');old='<h1>Uruguay<br><span>Demon List</span></h1>';assert old in s
s=s.replace(old,'<h1 class="game-title"><span class="title-country">URUGUAY</span><span class="title-game">DEMONLIST</span></h1>');p.write_text(s,encoding='utf-8')
