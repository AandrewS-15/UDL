from pathlib import Path
import re
p=Path('outputs/index.html');s=p.read_text(encoding='utf-8')
js=re.search(r'<script>(.*?)</script>',s,re.S).group(1)
js=js.replace('const uyLevels=','let uyLevels=').replace('const players=','let players=')
js=js.replace('function route(){','function publicRoute(){')
# Defer startup to the account extension, keeping existing public views.
a=js.index("document.querySelector('.menu').onclick=")
js=js[:a]
# Let the summary follow approval-driven ranking changes.
js=js.replace("[['PlayerB','PB','890'],['PlayerA','PA','650'],['AndrewS-15','AS','440']]", "players.slice(0,3).map(p=>[p.name,p.name.slice(0,2).toUpperCase(),p.points])")
s=re.sub(r'<script>.*?</script>','<script src="udl-app.js"></script>\n<script src="udl-account.js"></script>',s,flags=re.S)
s=s.replace('<button disabled title="Disponible en la próxima etapa de diseño">Clasificación</button>','<a href="#classification" data-nav="classification">Clasificación</a>').replace('<button disabled title="Disponible en la próxima etapa de diseño">Acerca de</button>','<a href="#about" data-nav="about">Acerca de</a>')
s=s.replace('<button class="login" disabled title="Disponible en la próxima etapa de diseño">Ingresar</button>','<div id="account-nav" class="account-nav"></div>')
s=s.replace('<span class="demo">Prototipo · Datos de ejemplo</span>','<a href="#rules">Reglas</a><span class="demo">Prototipo · Datos de ejemplo</span>')
p.write_text(s,encoding='utf-8');Path('outputs/udl-app.js').write_text(js,encoding='utf-8')
