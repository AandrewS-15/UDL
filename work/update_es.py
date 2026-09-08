from pathlib import Path
p=Path('outputs/udl-prototype.html')
s=p.read_text(encoding='utf-8')
translations={
'lang="en"':'lang="es"','Skip to content':'Saltar al contenido','Main navigation':'Navegación principal','UDL Home':'UDL Inicio','>Home<':'>Inicio<','>Demon List</a>':'>Lista uruguaya</a>','>Players<':'>Jugadores<','Planned for the next design stage':'Disponible en la próxima etapa de diseño','>Rankings<':'>Clasificación<','>About<':'>Acerca de<','>Login<':'>Ingresar<','Open navigation':'Abrir navegación','Made for the climb.':'La comunidad uruguaya, nivel a nivel.','Design preview · Sample data':'Prototipo · Datos de ejemplo',"Uruguay's extreme demon ranking":'Geometry Dash · Comunidad uruguaya','Demon List.</span>':'Demon List</span>','The Extreme Demon ranking for Uruguay.':'Los Extreme Demons completados por jugadores uruguayos.','Explore Demon List':'Explorar niveles','View Players':'Ver jugadores','Community statistics':'Estadísticas de la comunidad','Uruguayan players':'Jugadores uruguayos','Accepted records':'Récords aceptados','Ranked extremes':'Niveles completados en UY','Top Uruguay Players':'Jugadores destacados','Latest Records':'Últimos récords','Recent activity':'Actividad reciente',' completed ':' completó ','2 hours ago':'Hace 2 horas','5 hours ago':'Hace 5 horas',' made progress on ':' avanzó en ','Yesterday':'Ayer','Latest List Changes':'Cambios en la lista uruguaya','Explore list':'Ver lista','Moved up · Today':'Subió · Hoy','Moved down · Ayer':'Bajó · Ayer','Added · Sep 4':'Agregado · 4 sep.','Level not found':'Nivel no encontrado','Back to Demon List':'Volver a la lista uruguaya','>by <':'>por <','Verification video mockup':'Maqueta de video de verificación','VERIFICATION':'VERIFICACIÓN','Preview verification video':'Vista previa del video de verificación','Verification video':'Video de verificación','Video preview · Sample content':'Vista previa · Contenido de ejemplo','Level information':'Información del nivel','Geometry Dash ID':'ID de Geometry Dash','Pending':'Pendiente','>Creator<':'>Creador<','>Verifier<':'>Verificador<','Completion points':'Puntos por completar','Level tags':'Etiquetas del nivel','Uruguayan Records':'Récords uruguayos','Progress Records':'Récords de progreso','Position history':'Historial en Uruguay','Placed at ':'Puesto actual: ','September 6, 2026':'6 de septiembre de 2026','September 2, 2026':'2 de septiembre de 2026','August 28, 2026':'28 de agosto de 2026','Illustrative history and records.':'Historial y récords de ejemplo.','Video placeholder — a YouTube link will be connected in the next stage.':'Video de ejemplo: el enlace de YouTube se conectará en una próxima etapa.',"?'Demon List'":"?'Lista uruguaya'","||'Level':'Home'":"||'Nivel':'Inicio'"
}
for a,b in translations.items(): s=s.replace(a,b)
start=s.index('const levels=[')
end=s.index('const rankClass=',start)
s=s[:start]+'''// Datos ficticios para validar el diseño, no completaciones ni puestos oficiales.
const levels=[
{name:'Bloodbath',creator:'Riot',pts:450,aredlRank:420,tags:['Nave','Precisión'],records:[{player:'PlayerA',pct:100,country:'UY',approved:true},{player:'PlayerB',pct:100,country:'UY',approved:true},{player:'PlayerD',pct:87,country:'UY',approved:true}]},
{name:'Acu',creator:'neigefeu',pts:240,aredlRank:980,tags:['Precisión','Ritmo rápido'],records:[{player:'PlayerB',pct:100,country:'UY',approved:true},{player:'AndrewS-15',pct:100,country:'UY',approved:true}]},
{name:'Cataclysm',creator:'Ggb0y',pts:200,aredlRank:1050,tags:['Nave','Wave'],records:[{player:'PlayerA',pct:100,country:'UY',approved:true},{player:'PlayerB',pct:100,country:'UY',approved:true},{player:'AndrewS-15',pct:100,country:'UY',approved:true},{player:'PlayerE',pct:72,country:'UY',approved:true}]},
{name:'Zodiac',creator:'Bianox',pts:460,aredlRank:70,tags:['XL','Wave'],records:[{player:'PlayerC',pct:87,country:'UY',approved:true}]}
];
const victors=l=>l.records.filter(r=>r.country==='UY'&&r.approved&&r.pct===100);
const uyLevels=levels.map((l,id)=>({...l,id})).filter(l=>victors(l).length>0).sort((a,b)=>a.aredlRank-b.aredlRank).map((l,i)=>({...l,rank:i+1}));
let query='';const main=document.getElementById('main');
''' +s[end:]
start=s.index('function list()')
end=s.index('function detail(',start)
s=s[:start]+'''function list(){main.innerHTML=`<div class="page-intro"><div><div class="eyebrow">Uruguay Demon List</div><h1>Completados en Uruguay<span class="blue">.</span></h1><p class="muted">Solo niveles con al menos una completación uruguaya aceptada al 100%.</p></div></div><div class="toolbar"><span class="small muted">Ordenados por dificultad · Referencia AREDL</span><label class="search"><span aria-hidden="true">⌕</span><input type="search" placeholder="Buscar nivel..." aria-label="Buscar nivel" id="search" autocomplete="off"></label></div><div class="list-label"><span>Puesto UY / Nivel</span><span>Puntos</span></div><div id="results"></div><div class="list-note"><span id="count" role="status"></span><span>Puestos AREDL ilustrativos, no verificados.</span></div>`;const search=document.getElementById('search');search.value=query;search.addEventListener('input',()=>{query=search.value;rows()});rows()}
function rows(){const matches=uyLevels.filter(l=>l.name.toLowerCase().includes(query.trim().toLowerCase()));document.getElementById('results').innerHTML=matches.length?matches.map(l=>`<a class="levelrow" href="#level/${l.id}"><span class="levelrank ${rankClass(l.rank)}" aria-label="Puesto ${l.rank} en Uruguay"><small>#</small>${String(l.rank).padStart(2,'0')}</span><div><div class="levelname">${l.name}</div><div class="creator">por ${l.creator}</div><div class="creator"><span class="blue">AREDL #${l.aredlRank}</span> · ${victors(l).length} jugadores UY al 100%</div></div><div class="levelpoints">${l.pts}<small>pts</small></div><span class="arrow" aria-hidden="true">↗</span></a>`).join(''):`<div class="empty"><h2>No encontramos niveles</h2><p>Probá con otro nombre entre los completados en Uruguay.</p><button class="btn secondary" id="clear">Limpiar búsqueda</button></div>`;document.getElementById('count').textContent=`${matches.length} de ${uyLevels.length} niveles completados en Uruguay · Datos de ejemplo`;document.getElementById('clear')?.addEventListener('click',()=>{query='';document.getElementById('search').value='';rows();document.getElementById('search').focus()})}
''' +s[end:]
s=s.replace('const l=levels[id];if(!l)', 'const l=uyLevels.find(level=>level.id===id);if(!l)')
s=s.replace('${l.name} by ${l.creator}. Explore its placement, completion points and records from the Uruguayan community.', '${l.name}, de ${l.creator}. Completado por ${victors(l).length} jugadores uruguayos. El puesto UY lo ubica entre los niveles completados en el país; AREDL indica su posición global de referencia.')
s=s.replace('<span>Placement</span><span>#${l.rank} · ${l.group} List</span>', '<span>Puesto en Uruguay</span><span>#${l.rank}</span></div><div class="data-line"><span>Puesto en AREDL</span><span>#${l.aredlRank} <small class="muted">(ejemplo)</small></span>')
s=s.replace('3 Victors','${victors(l).length} completaciones')
s=s.replace("${record('PlayerA',100,1)+record('PlayerB',100,2)+record('AndrewS-15',100,3)}", "${victors(l).map((r,i)=>record(r.player,r.pct,i+1)).join('')}")
s=s.replace("${record('PlayerD',87,'—')+record('PlayerE',72,'—')}", "${l.records.filter(r=>r.country==='UY'&&r.approved&&r.pct<100).map(r=>record(r.player,r.pct,'—')).join('')||'<p class=\"muted small\">No hay récords de progreso aceptados.</p>'}")
a=s.index('<div class="history"><div class="history-item">')
b=s.index('</div><p class="muted small">Historial',a)
s=s[:a]+'''<div class="history"><div class="history-item">Puesto actual: <span class="blue">#${l.rank} en Uruguay</span><small>6 de septiembre de 2026</small></div><div class="history-item">Primera completación uruguaya aceptada<small>28 de agosto de 2026 · ${victors(l)[0].player}</small></div>'''+s[b:]
# Home links must point to qualifying local levels; progress-only Zodiac is excluded.
s=s.replace('href="#level/5"','href="#level/0"').replace('href="#level/6"','href="#level/1"')
s=s.replace('href="#level/4"','href="#level/2"').replace('href="#level/3"','href="#level/0"')
s=s.replace('>Zodiac<','>Cataclysm<').replace('>SOCIETY<','>Bloodbath<').replace('>Amethyst<','>Acu<')
s=s.replace('<strong>PlayerC</strong> avanzó','<strong>PlayerE</strong> avanzó').replace('>87% <span','>72% <span')
s=s.replace('<strong class="mono">128</strong>','<strong class="mono">5</strong>').replace('<strong class="mono">1,406</strong>','<strong class="mono">9</strong>').replace('<strong class="mono">150</strong>','<strong class="mono">${uyLevels.length}</strong>')
s=s.replace("'8,420'","'650'").replace("'7,950'","'890'").replace("'6,830'","'440'")
s=s.replace("[['PlayerA','PA','650'],['PlayerB','PB','890']", "[['PlayerB','PB','890'],['PlayerA','PA','650']")
s=s.replace('<small>Subió · Hoy</small>','<small>Primer puesto UY · Hoy</small>').replace('#6 → <span class="blue">#4</span>','<span class="blue">#1 UY</span>')
s=s.replace('<small>Bajó · Ayer</small>','<small>Puesto local · Ayer</small>').replace('#4 → #5','#3 UY')
s=s.replace('>Top 3<','>Primeros 3<')
s=s.replace('font-size:48px;letter-spacing:-2px;margin-top:19px','font-size:clamp(36px,10vw,48px);letter-spacing:-2px;margin-top:19px')
s=s.replace('.navlinks{gap:18px}.brand-name', '.navlinks{gap:14px}.brand-name')
s=s.replace('@media(max-width:640px)', '@media(max-width:760px)')
p.write_text(s,encoding='utf-8')
Path('work/prototype-check.js').write_text(s.split('<script>')[1].split('</script>')[0],encoding='utf-8')
