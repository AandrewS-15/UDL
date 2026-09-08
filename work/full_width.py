from pathlib import Path
p=Path('outputs/udl-prototype.html')
s=p.read_text(encoding='utf-8')
s=s.replace('</style>','''
/* Full-width home background; content retains its readable width. */
.page:has(.hero-intro){width:100%;max-width:none}
.hero.hero-intro{width:100%;margin-inline:0}
.hero-intro .hero-content{width:min(1120px,calc(100% - 64px));margin-inline:auto}
.page:has(.hero-intro)>.home-grid,.page:has(.hero-intro)>.changes{width:min(1120px,calc(100% - 64px));margin-inline:auto}
@media(max-width:900px){.hero-intro .hero-content,.page:has(.hero-intro)>.home-grid,.page:has(.hero-intro)>.changes{width:calc(100% - 40px)}}
@media(max-width:760px){.hero-intro .hero-content,.page:has(.hero-intro)>.home-grid,.page:has(.hero-intro)>.changes{width:calc(100% - 36px)}}
</style>''')
p.write_text(s,encoding='utf-8')
