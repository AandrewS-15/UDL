from pathlib import Path
p=Path('outputs/udl-prototype.html')
s=p.read_text(encoding='utf-8')
s=s.replace('</style>','''
/* Brief entrance on route changes; searches remain immediate. */
+.section-enter{animation:section-enter 260ms cubic-bezier(.2,.7,.3,1) both}
+@keyframes section-enter{from{opacity:0;transform:translateY(9px)}to{opacity:1;transform:translateY(0)}}
+@media(prefers-reduced-motion:reduce){.section-enter{animation:none}}
+</style>'''.replace('\n+','\n'))
old="else home();document.title="
new="else home();main.classList.remove('section-enter');void main.offsetWidth;main.classList.add('section-enter');document.title="
assert old in s
s=s.replace(old,new)
p.write_text(s,encoding='utf-8')
Path('work/prototype-check.js').write_text(s.split('<script>')[1].split('</script>')[0],encoding='utf-8')
