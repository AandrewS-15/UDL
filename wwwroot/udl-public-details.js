// Las imágenes ausentes dejan visible el fondo o la inicial, sin iconos rotos.
document.querySelectorAll('[data-image-fallback]').forEach(img => {
  img.addEventListener('error', () => img.remove(), {once: true});
  if (img.complete && !img.naturalWidth) img.remove();
});
const publicMenu = document.querySelector('.menu');
const publicNav = document.getElementById('navlinks');
publicMenu.addEventListener('click', () => {
  publicMenu.setAttribute('aria-expanded', String(publicNav.classList.toggle('open')));
});
document.addEventListener('keydown', event => {
  if (event.key === 'Escape') {
    publicNav.classList.remove('open');
    publicMenu.setAttribute('aria-expanded', 'false');
  }
});
const publicSection = location.pathname.includes('/players/') ? 'players' : location.pathname.includes('/levels/') ? 'list' : '';
document.querySelector(`[data-nav="${publicSection}"]`)?.classList.add('active');
document.querySelector(`[data-nav="${publicSection}"]`)?.setAttribute('aria-current', 'page');
