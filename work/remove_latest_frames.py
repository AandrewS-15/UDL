from pathlib import Path
p=Path('outputs/udl-styles.css')
s=p.read_text(encoding='utf-8-sig')
marker='/* The opening title remains spacious, with a subtle opaque reading surface. */'
assert marker in s
s=s[:s.index(marker)]+'''/* Preserve the softer mobile watermark without the added text frames. */
@media(max-width:760px){body::before{opacity:.16}body:has(.hero-intro)::before{opacity:.22}}
'''
p.write_text(s,encoding='utf-8')
