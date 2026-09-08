from pathlib import Path
p=Path('outputs/udl-prototype.html')
s=p.read_text(encoding='utf-8')
start=s.index('<section class="hero">',s.index('function home()'))
end=s.index('<div class="home-grid">',start)
s=s[:start]+'''<section class="hero hero-intro" aria-label="Uruguay Demon List"><h1>Uruguay<br><span>Demon List</span></h1></section>'''+s[end:]
css='''
/* Navigation labels share the logo's vertical center. */
.navlinks a,.navlinks button{display:flex;align-items:center;justify-content:center}
/* Quiet opening: the name alone, with a slow ambient blue glow. */
.page:has(.hero-intro){padding-top:0}
.hero.hero-intro{position:relative;isolation:isolate;display:flex;align-items:center;justify-content:center;text-align:center;min-height:calc(100svh - 88px);padding:72px 0;border-bottom:0;overflow:hidden}
.hero-intro::before{content:"";position:absolute;z-index:-1;inset:5% -15%;pointer-events:none;background:radial-gradient(ellipse at 35% 45%,rgba(64,151,213,.13),transparent 58%),radial-gradient(ellipse at 72% 60%,rgba(47,99,156,.09),transparent 52%);filter:blur(28px);animation:ambient-drift 22s ease-in-out infinite alternate;will-change:transform}
.hero.hero-intro h1{margin:0;max-width:100%;font-size:clamp(58px,8.5vw,112px);line-height:1.08;letter-spacing:-.055em;font-weight:600}
.hero-intro+.home-grid{margin-top:36px}
@keyframes ambient-drift{from{transform:translate(-4%,-3%) scale(.95)}to{transform:translate(5%,4%) scale(1.08)}}
@media(max-width:760px){.navlinks a,.navlinks button{justify-content:flex-start}.hero.hero-intro{min-height:calc(100svh - 72px);padding:64px 0}.hero.hero-intro h1{font-size:clamp(48px,12vw,82px)}.hero-intro::before{inset:12% -10%;filter:blur(20px)}}
@media(prefers-reduced-motion:reduce){.hero-intro::before{animation:none;will-change:auto}}
'''
s=s.replace('</style>',css+'</style>')
p.write_text(s,encoding='utf-8')
Path('work/prototype-check.js').write_text(s.split('<script>')[1].split('</script>')[0],encoding='utf-8')
readme=Path('outputs/LEEME.md')
r=readme.read_text(encoding='utf-8-sig')
r+='\n\n## Ajuste de portada\n\nLa primera sección ocupa la altura disponible de la pantalla y muestra únicamente Uruguay Demon List. Se retiraron el subtítulo, los botones y las estadísticas laterales de esa sección. Los resúmenes siguen disponibles al desplazarse hacia abajo. La navegación queda centrada verticalmente con el logo. Un resplandor celeste de baja intensidad se desplaza lentamente detrás del título y queda estático si el dispositivo solicita reducir el movimiento.\n'
readme.write_text(r,encoding='utf-8')
