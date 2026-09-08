from pathlib import Path
p=Path('outputs/udl-prototype.html')
s=p.read_text(encoding='utf-8')
a='<section class="hero hero-intro" aria-label="Uruguay Demon List"><h1>Uruguay<br><span>Demon List</span></h1></section>'
b='<section class="hero hero-intro" aria-label="Uruguay Demon List"><div class="hero-content"><div class="eyebrow"><span class="dot"></span>Geometry Dash · Comunidad uruguaya</div><h1>Uruguay<br><span>Demon List</span></h1><div class="actions"><a class="btn" href="#list">Explorar niveles <span aria-hidden="true">↗</span></a><a class="btn secondary" href="#home-players">Ver jugadores <span aria-hidden="true">→</span></a></div></div></section>'
assert a in s
s=s.replace(a,b)
s=s.replace('</style>','''
.hero-content{width:100%;position:relative;z-index:1}
.hero-content .eyebrow{justify-content:center;margin-bottom:26px;flex-wrap:wrap}
.hero-content .actions{justify-content:center;margin-top:36px}
.hero-intro::before{inset:-10% -30%;background:radial-gradient(ellipse at 35% 45%,rgba(64,151,213,.26),transparent 48%),radial-gradient(ellipse at 72% 60%,rgba(47,99,156,.18),transparent 45%);filter:blur(32px);animation:ambient-drift 8s ease-in-out infinite alternate}
.hero-intro::after{content:"";position:absolute;z-index:-1;pointer-events:none;inset:5% -25%;background:radial-gradient(ellipse at 65% 55%,rgba(91,184,244,.12),transparent 45%);filter:blur(24px);animation:ambient-counter 11s ease-in-out infinite alternate;will-change:transform}
@keyframes ambient-drift{from{transform:translate(-15%,-8%) scale(.9)}to{transform:translate(15%,8%) scale(1.12)}}
@keyframes ambient-counter{from{transform:translate(12%,8%)}to{transform:translate(-16%,-10%)}}
@media(max-width:760px){.hero-content .eyebrow{font-size:11px;letter-spacing:1.4px;margin-bottom:22px}.hero-content .actions{margin-top:28px}.hero-intro::before{inset:-10% -35%}}
@media(prefers-reduced-motion:reduce){.hero-intro::before,.hero-intro::after{animation:none;will-change:auto}}
</style>''')
p.write_text(s,encoding='utf-8')
Path('work/prototype-check.js').write_text(s.split('<script>')[1].split('</script>')[0],encoding='utf-8')
p=Path('outputs/LEEME.md')
s=p.read_text(encoding='utf-8')
s+='\n\nLa portada recupera la leyenda Geometry Dash · Comunidad uruguaya y los botones Explorar niveles y Ver jugadores, centrados. El fondo utiliza dos resplandores con desplazamiento más visible en ciclos de 8 y 11 segundos. Se respeta la preferencia de movimiento reducido.\n'
p.write_text(s,encoding='utf-8')
