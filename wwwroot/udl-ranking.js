// Ranking real. La demo de otras secciones no participa en estos datos.
let rankingPlayers = [];
const rankingPoints = new Intl.NumberFormat('en', {useGrouping: false, maximumFractionDigits: 1});

async function playersPage() {
  rankingPlayers = [];
  main.innerHTML = `<div class="page-intro"><div><div class="eyebrow">Comunidad uruguaya</div><h1>Jugadores<span class="blue">.</span></h1><p class="muted">El ranking de Uruguay, completación a completación.</p></div><span class="demo" id="ranking-total">Cargando…</span></div><div class="toolbar"><span class="small muted">Ordenados por puntos · De mayor a menor</span><label class="search"><span aria-hidden="true">⌕</span><input id="player-search" type="search" placeholder="Buscar jugador..." aria-label="Buscar jugador" autocomplete="off" disabled></label></div><div class="player-table-wrap"><table class="player-table" aria-busy="true"><caption class="sr-only">Ranking de jugadores uruguayos por puntos</caption><thead><tr><th scope="col">Puesto UY</th><th scope="col">Jugador</th><th scope="col">Puntos</th><th scope="col"><span class="desktop-label">Extreme Demons</span><span class="mobile-label">Extremes</span></th></tr></thead><tbody id="player-results"><tr><td colspan="4" role="status">Cargando jugadores…</td></tr></tbody></table></div><div id="player-empty" class="empty" hidden><h2>No encontramos jugadores</h2><p>Probá con otro nombre.</p><button class="btn secondary" id="player-clear">Limpiar búsqueda</button></div><div class="list-note"><span id="player-count" role="status"></span><span>Completaciones aprobadas al 100%.</span></div><p class="inline-style-1 muted small">Los puntos suman los niveles completados. A igual puntaje, se ordena por cantidad de completaciones y luego por nombre.</p>`;
  const body = document.getElementById('player-results');
  const input = document.getElementById('player-search');
  input.value = playerQuery;
  input.oninput = () => { playerQuery = input.value; playerRows(); };
  document.getElementById('player-clear').onclick = () => { playerQuery = ''; input.value = ''; playerRows(); input.focus(); };
  try {
    const response = await fetch(new URL('api/players/ranking', document.baseURI), {cache: 'no-store'});
    if (!response.ok) throw new Error('Ranking unavailable');
    const data = await response.json();
    if (!Array.isArray(data)) throw new Error('Invalid ranking');
    if (!body.isConnected) return; // Una respuesta vieja no debe reemplazar otra sección.
    rankingPlayers = data;
    document.getElementById('ranking-total').textContent = `${data.length} jugadores`;
    input.disabled = false;
    body.closest('table').setAttribute('aria-busy', 'false');
    playerRows();
  } catch {
    if (!body.isConnected) return;
    body.closest('table').setAttribute('aria-busy', 'false');
    body.innerHTML = '<tr><td colspan="4"><p role="alert">No pudimos cargar el ranking.</p><button class="btn secondary" id="ranking-retry">Reintentar</button></td></tr>';
    document.getElementById('ranking-total').textContent = 'Sin conexión';
    document.getElementById('ranking-retry').onclick = playersPage;
  }
}

function playerRows() {
  const body = document.getElementById('player-results');
  if (!body) return;
  const query = playerQuery.trim().toLocaleLowerCase('es');
  const matches = rankingPlayers.filter(p => p.nombreGD.toLocaleLowerCase('es').includes(query));
  const fragment = document.createDocumentFragment();
  for (const p of matches) {
    const row = document.createElement('tr');
    row.dataset.playerId = p.idJugador;
    const cell = (className, value) => { const td = document.createElement('td'); td.className = className; td.textContent = value; row.append(td); return td; };
    cell(`player-rank ${rankClass(p.posicion)}`, `#${String(p.posicion).padStart(2, '0')}`);
    const identity = document.createElement('span');
    identity.className = 'player-identity';
    const avatar = document.createElement('span');
    avatar.className = 'avatar ranking-avatar';
    avatar.setAttribute('aria-hidden', 'true');
    avatar.textContent = Array.from(p.nombreGD)[0]?.toLocaleUpperCase('es') || '?';
    if (p.avatarUrl) {
      try {
        const url = new URL(p.avatarUrl);
        if (['https:', 'http:'].includes(url.protocol)) {
          const img = document.createElement('img');
          img.alt = '';
          img.loading = 'lazy';
          img.referrerPolicy = 'no-referrer';
          img.onerror = () => img.remove(); // La inicial queda detrás de la imagen.
          img.src = url.href;
          avatar.append(img);
        }
      } catch { /* URL inválida: conservar inicial. */ }
    }
    const name = document.createElement('span');
    name.textContent = p.nombreGD;
    identity.append(avatar, name);
    cell('player-name', '').append(identity);
    const score = cell('player-score', rankingPoints.format(p.totalPuntos));
    const unit = document.createElement('span'); unit.className = 'muted small'; unit.textContent = ' pts'; score.append(unit);
    cell('mono muted', p.cantidadCompletions);
    fragment.append(row);
  }
  body.replaceChildren(fragment);
  document.getElementById('player-empty').hidden = matches.length > 0;
  document.getElementById('player-count').textContent = `${matches.length} de ${rankingPlayers.length} jugadores`;
}
