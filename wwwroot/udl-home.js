async function home() {
  main.innerHTML = '<div id="home-players" role="status" aria-busy="true">Cargando inicio…</div>';
  const loading = document.getElementById('home-players');
  try {
    const response = await fetch(new URL('home/content', document.baseURI), {cache: 'no-store'});
    if (!response.ok) throw new Error('Home unavailable');
    const html = await response.text();
    if (!loading.isConnected) return;
    main.innerHTML = html; // HTML Razor del mismo origen; los datos se codifican en el servidor.
    main.querySelectorAll('[data-image-fallback]').forEach(img => {
      img.addEventListener('error', () => img.remove(), {once: true});
      if (img.complete && !img.naturalWidth) img.remove();
    });
    if (location.hash === '#home-players') document.getElementById('home-players').scrollIntoView();
  } catch {
    if (!loading.isConnected) return;
    main.innerHTML = '<section class="empty"><h1>No pudimos cargar el inicio</h1><p role="alert">Intentá nuevamente en unos momentos.</p><button class="btn secondary" id="home-retry">Reintentar</button></section>';
    document.getElementById('home-retry').onclick = home;
  }
}
