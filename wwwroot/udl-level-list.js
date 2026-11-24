// Datos de la lista real, aislados de los niveles de otras secciones de la demo.
let uruguayLevelItems = [];
const levelListPoints = new Intl.NumberFormat('en', {useGrouping: false, maximumFractionDigits: 1});

async function list() {
  uruguayLevelItems = [];
  main.innerHTML = `<div class="page-intro"><div><div class="eyebrow">Uruguay Demon List</div><h1>Lista uruguaya<span class="blue">.</span></h1><p class="muted">Extreme Demons completados por jugadores uruguayos, ordenados por dificultad.</p></div></div><div class="toolbar"><span class="small muted">Ordenados por dificultad · Referencia AREDL</span><label class="search"><span aria-hidden="true">⌕</span><input type="search" placeholder="Buscar nivel..." aria-label="Buscar nivel" id="search" autocomplete="off" disabled></label></div><div class="list-label"><span>Puesto UY / Nivel</span><span>Puntos</span></div><div id="results" role="list" aria-label="Niveles de la Lista Uruguay" aria-busy="true"><p role="status">Cargando niveles…</p></div><div class="list-note"><span id="count" role="status"></span><span>Solo completaciones aprobadas al 100%.</span></div>`;
  const results = document.getElementById('results');
  const search = document.getElementById('search');
  search.value = query;
  search.oninput = () => { query = search.value; rows(); };
  try {
    const response = await fetch(new URL('api/levels/uruguay', document.baseURI), {cache: 'no-store'});
    if (!response.ok) throw new Error('List unavailable');
    const data = await response.json();
    if (!Array.isArray(data)) throw new Error('Invalid list');
    if (!results.isConnected) return;
    uruguayLevelItems = data;
    search.disabled = false;
    results.setAttribute('aria-busy', 'false');
    rows();
  } catch {
    if (!results.isConnected) return;
    results.setAttribute('aria-busy', 'false');
    results.innerHTML = '<div class="empty"><p role="alert">No pudimos cargar la Lista Uruguay.</p><button class="btn secondary" id="level-list-retry">Reintentar</button></div>';
    document.getElementById('level-list-retry').onclick = list;
  }
}

function rows() {
  const results = document.getElementById('results');
  if (!results) return;
  const matches = uruguayLevelItems.filter(l => l.nombre.toLocaleLowerCase('es').includes(query.trim().toLocaleLowerCase('es')));
  const fragment = document.createDocumentFragment();
  const element = (tag, className, text) => { const e = document.createElement(tag); e.className = className; e.textContent = text; return e; };
  for (const level of matches) {
    const item = document.createElement('div');
    item.setAttribute('role', 'listitem');
    const row = element('a', 'levelrow level-banner-row', '');
    row.href = new URL(`levels/${level.idNivel}`, document.baseURI).href;
    row.dataset.levelId = level.idNivel;
    if (level.thumbnailUrl) {
      const img = document.createElement('img');
      img.className = 'level-banner-image'; img.alt = ''; img.loading = 'lazy';
      img.onerror = () => img.remove(); img.src = level.thumbnailUrl; row.append(img);
    }
    row.dataset.category = level.categoria;
    const rank = element('span', `levelrank ${rankClass(level.posicionUy)}`, String(level.posicionUy).padStart(2, '0'));
    rank.setAttribute('aria-label', `Puesto ${level.posicionUy} en Uruguay`);
    rank.prepend(element('small', '', '#'));
    const identity = document.createElement('div');
    identity.append(element('div', 'levelname', level.nombre), element('div', 'creator', level.categoria));
    const metadata = element('div', 'creator', '');
    metadata.append(element('span', 'blue', level.posicionAredl == null ? 'Sin posición AREDL' : `AREDL #${level.posicionAredl}`),
      document.createTextNode(` · ${level.victorCount} ${level.victorCount === 1 ? 'jugador UY' : 'jugadores UY'} al 100%`));
    identity.append(metadata);
    const points = element('div', 'levelpoints', levelListPoints.format(level.puntosAredl));
    points.append(element('small', '', 'pts'));
    const space = element('span', 'arrow', '↗');
    space.setAttribute('aria-hidden', 'true');
    row.append(rank, identity, points, space);
    item.append(row);
    fragment.append(item);
  }
  results.replaceChildren(fragment);
  if (!matches.length) {
    results.innerHTML = '<div class="empty"><h2>No encontramos niveles</h2><p>Probá con otro nombre entre los completados en Uruguay.</p><button class="btn secondary" id="clear">Limpiar búsqueda</button></div>';
    document.getElementById('clear').onclick = () => { query = ''; document.getElementById('search').value = ''; rows(); document.getElementById('search').focus(); };
  }
  document.getElementById('count').textContent = `${matches.length} de ${uruguayLevelItems.length} niveles completados en Uruguay`;
}
