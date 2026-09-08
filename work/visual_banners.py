from pathlib import Path
p=Path('outputs/udl-app.js');s=p.read_text(encoding='utf-8')
s=s.replace("name:'Bloodbath',", "name:'Bloodbath',image:'assets/bloodbath.jpg',").replace("name:'Acu',", "name:'Acu',image:'assets/acu.jpg',").replace("name:'Cataclysm',", "name:'Cataclysm',image:'assets/cataclysm.jpg',").replace("name:'Zodiac',", "name:'Zodiac',image:'assets/zodiac.jpg',")
s=s.replace('<a class="levelrow" href="#level/${l.id}">','<a class="levelrow level-banner-row" href="#level/${l.id}"><img class="level-banner-image" src="${l.image}" alt="" loading="lazy">')
s=s.replace('<div class="video" aria-label="Maqueta de video de verificación">','<div class="video level-video" aria-label="Maqueta de video de verificación"><img class="level-banner-image" src="${l.image}" alt="Vista del nivel ${l.name}">')
p.write_text(s,encoding='utf-8')
p=Path('outputs/udl-account.js');s=p.read_text(encoding='utf-8')
pos=s.index('function submitPage()')
s=s[:pos]+'''const rawRequired=level=>Boolean(level&&level.aredlRank>=1&&level.aredlRank<=250);
function updateRawRequirement(){const required=rawRequired(levels[selectedLevel]);const input=document.getElementById('raw-url');input.required=required;input.setAttribute('aria-required',String(required));document.querySelector('label[for="raw-url"]').textContent=required?'Raw footage (obligatorio)':'Raw footage (opcional)';document.getElementById('raw-help').textContent=required?'Este nivel está en el top 1–250 de AREDL: necesitás raw footage para enviar completaciones o progresos.':'Raw footage obligatorio para niveles del top 1–250 de AREDL; opcional fuera de ese rango.';}
''' +s[pos:]
s=s.replace("${field('raw-url','Raw footage (opcional)','url','https://...')}","${field('raw-url','Raw footage (opcional)','url','https://...')}<p class=\"raw-help small\" id=\"raw-help\">Raw footage obligatorio para niveles del top 1–250 de AREDL; opcional fuera de ese rango.</p>")
s=s.replace('selectedLevel=id;input.value=levels[id].name;', 'selectedLevel=id;updateRawRequirement();input.value=levels[id].name;')
s=s.replace('selectedLevel=null;show()', 'selectedLevel=null;updateRawRequirement();show()')
s=s.replace("if(value('raw-url')&&!validURL(value('raw-url')))","if(rawRequired(levels[selectedLevel])&&!value('raw-url'))ok=error('raw-url','El raw footage es obligatorio para el top 1–250 de AREDL.');if(value('raw-url')&&!validURL(value('raw-url')))")
s=s.replace('Los moderadores podrán solicitar raw footage cuando sea necesario.','El raw footage es obligatorio en completaciones y progresos de niveles ubicados entre los puestos 1 y 250 de AREDL, inclusive. Fuera de ese rango es opcional; los moderadores podrán solicitarlo cuando sea necesario.')
s=s.replace("s.status=approve?'Aprobado':'Rechazado';", "if(approve&&rawRequired(levels[s.level])&&!validURL(s.raw)){notify('No se puede aprobar: falta raw footage válido para el top 1–250 de AREDL.');return false}s.status=approve?'Aprobado':'Rechazado';")
s=s.replace('applyDecision(id,approve,reason);dialog.close();','if(!applyDecision(id,approve,reason))return;dialog.close();')
s=s.replace('<h3 class="spaced">Notas</h3>','<p class="raw-help small spaced">${rawRequired(levels[s.level])?\'Raw footage obligatorio · Top 1–250 AREDL\':\'Raw footage opcional · Fuera del top 250 AREDL\'}</p><h3 class="spaced">Notas</h3>')
p.write_text(s,encoding='utf-8')
