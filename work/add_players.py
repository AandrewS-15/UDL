from pathlib import Path
p=Path('outputs/udl-prototype.html')
s=p.read_text(encoding='utf-8')
s=s.replace('<a href="#home-players">Jugadores</a>','<a href="#players" data-nav="players">Jugadores</a>').replace('href="#home-players">Ver jugadores','href="#players">Ver jugadores')
s=s.replace('<span class="muted small">Primeros 3</span>','<a class="text-link" href="#players">Ver ranking →</a>')
pos=s.index('function home()')
s=s[:pos]+'''const playerNames=[...new Set(levels.flatMap(l=>l.records.filter(r=>r.country==='UY'&&r.approved).map(r=>r.player)))];
+const players=playerNames.map(name=>{const completed=uyLevels.filter(l=>victors(l).some(r=>r.player===name));return {name,points:completed.reduce((sum,l)=>sum+l.pts,0),extremes:completed.length};}).sort((a,b)=>b.points-a.points||a.name.localeCompare(b.name,'es'));
+players.forEach((p,i)=>p.rank=i&&p.points===players[i-1].points?players[i-1].rank:i+1);
+let playerQuery='';
+function playersPage(){main.innerHTML=`<div class="page-intro"><div><div class="eyebrow">Comunidad uruguaya</div><h1>Jugadores<span class="blue">.</span></h1><p class="muted">El ranking de Uruguay, completación a completación.</p></div><span class="demo">${players.length} jugadores</span></div><div class="toolbar"><span class="small muted">Ordenados por puntos · De mayor a menor</span><label class="search"><span aria-hidden="true">⌕</span><input id="player-search" type="search" placeholder="Buscar jugador..." aria-label="Buscar jugador" autocomplete="off"></label></div><div class="player-table-wrap"><table class="player-table"><caption class="sr-only">Ranking de jugadores uruguayos por puntos</caption><thead><tr><th scope="col">Puesto</th><th scope="col">Jugador</th><th scope="col">Puntos</th><th scope="col"><span class="desktop-label">Extreme Demons</span><span class="mobile-label">Extremes</span></th></tr></thead><tbody id="player-results"></tbody></table></div><div id="player-empty" class="empty" hidden><h2>No encontramos jugadores</h2><p>Probá con otro nombre.</p><button class="btn secondary" id="player-clear">Limpiar búsqueda</button></div><div class="list-note"><span id="player-count" role="status"></span><span>Puntos y récords de ejemplo.</span></div><p class="muted small" style="margin-top:14px">En este prototipo, los puntos suman las completaciones aceptadas al 100%, una vez por nivel. Los empates comparten puesto.</p>`;const input=document.getElementById('player-search');input.value=playerQuery;input.oninput=()=>{playerQuery=input.value;playerRows()};document.getElementById('player-clear').onclick=()=>{playerQuery='';input.value='';playerRows();input.focus()};playerRows()}
+function playerRows(){const matches=players.filter(p=>p.name.toLocaleLowerCase('es').includes(playerQuery.trim().toLocaleLowerCase('es')));document.getElementById('player-results').innerHTML=matches.map(p=>`<tr><td class="player-rank ${rankClass(p.rank)}">#${String(p.rank).padStart(2,'0')}</td><td class="player-name">${p.name}</td><td class="player-score">${p.points.toLocaleString('es-UY')} <span class="muted small">pts</span></td><td class="mono muted">${p.extremes}</td></tr>`).join('');document.getElementById('player-empty').hidden=matches.length>0;document.getElementById('player-count').textContent=`${matches.length} de ${players.length} jugadores`;}
+'''.replace('\n+','\n')+s[pos:]
s=s.replace("(isList?'list':'home')","(isList?'list':hash==='players'?'players':'home')")
s=s.replace("if(hash==='list')list();else if(hash.startsWith", "if(hash==='list')list();else if(hash==='players')playersPage();else if(hash.startsWith")
s=s.replace("hash==='list'?'Lista uruguaya':hash.startsWith", "hash==='list'?'Lista uruguaya':hash==='players'?'Jugadores':hash.startsWith")
s=s.replace('</style>','''
+.sr-only{position:absolute;width:1px;height:1px;padding:0;margin:-1px;overflow:hidden;clip:rect(0,0,0,0);white-space:nowrap;border:0}
+.player-table{width:100%;border-collapse:collapse;text-align:left}
+.player-table th{font-size:12px;font-weight:500;color:var(--muted);text-transform:uppercase;letter-spacing:.6px;padding:12px 20px}
+.player-table td{border-top:1px solid var(--line);padding:23px 20px}
+.player-table th:nth-child(n+3),.player-table td:nth-child(n+3){text-align:right}
+.player-table tbody tr:last-child td{border-bottom:1px solid var(--line)}
+.player-table tbody tr:hover{background:var(--surface)}
+.player-table .player-rank{font-size:24px;font-variant-numeric:tabular-nums;width:110px;color:var(--muted)}
+.player-name{font-weight:600}.player-score{font-size:18px;font-variant-numeric:tabular-nums}
+.mobile-label{display:none}.empty[hidden]{display:none}
+@media(max-width:760px){.player-table th{font-size:11px;letter-spacing:0;padding:12px 4px}.player-table td{padding:20px 4px;font-size:14px}.player-table .player-rank{font-size:18px;width:45px}.player-table .player-name{overflow-wrap:anywhere}.player-table .player-score{white-space:nowrap}.player-score .small{display:none}.desktop-label{display:none}.mobile-label{display:inline}}
+</style>'''.replace('\n+','\n'))
p.write_text(s,encoding='utf-8')
Path('work/prototype-check.js').write_text(s.split('<script>')[1].split('</script>')[0],encoding='utf-8')
