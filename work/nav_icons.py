from pathlib import Path
from urllib.parse import quote
icons={
'home':'<path d="m3 10 9-7 9 7v10a1 1 0 0 1-1 1h-5v-7H9v7H4a1 1 0 0 1-1-1Z"/>',
'list':'<path d="M9 5h12M9 12h12M9 19h12M3 5h1M3 12h1M3 19h1"/>',
'players':'<circle cx="9" cy="8" r="3"/><path d="M3 21v-3a6 6 0 0 1 12 0v3M16 5a3 3 0 0 1 0 6m2 4a5 5 0 0 1 3 5"/>',
'classification':'<path d="M8 3h8v6a4 4 0 0 1-8 0ZM8 5H4v2a4 4 0 0 0 4 4m8-6h4v2a4 4 0 0 1-4 4m-4 2v5m-4 3h8m-7-3h6"/>',
'about':'<circle cx="12" cy="12" r="9"/><path d="M12 11v6m0-10v.5"/>',
'me':'<circle cx="12" cy="8" r="4"/><path d="M4 21v-2a8 8 0 0 1 16 0v2"/>',
'submissions':'<path d="M6 3h9l4 4v14H6ZM14 3v5h5M9 12h7m-7 4h7"/>',
'moderation':'<path d="m12 3 8 3v6c0 5-8 9-8 9s-8-4-8-9V6Zm-4 9 3 3 5-6"/>',
'admin':'<path d="M4 7h16M4 17h16"/><circle cx="9" cy="7" r="3"/><circle cx="15" cy="17" r="3"/>',
'me/account':'<path d="M4 7h16M4 17h16"/><circle cx="9" cy="7" r="3"/><circle cx="15" cy="17" r="3"/>',
'me/completed':'<circle cx="12" cy="12" r="9"/><path d="m7 12 3 3 7-7"/>',
'me/progress':'<path d="M4 20h16M6 16v-4m6 4V8m6 8V4"/>',
'login':'<path d="M14 3h6v18h-6M3 12h12m-4-4 4 4-4 4"/>',
'submit':'<path d="M12 16V3m-5 5 5-5 5 5M4 15v6h16v-6"/>',
'admin/moderators':'<path d="m12 3 8 3v6c0 5-8 9-8 9s-8-4-8-9V6Zm-4 9 3 3 5-6"/>',
'admin/submissions':'<path d="M6 3h9l4 4v14H6ZM14 3v5h5M9 12h7m-7 4h7"/>'}
css='''
/* Small monochrome navigation icons, matching the existing accent palette. */
:is(.navlinks,.private-tabs,.user-dropdown,.account-nav) a::before{content:"";display:inline-block;width:16px;height:16px;flex:0 0 16px;background:currentColor;mask:var(--nav-icon) center/contain no-repeat;-webkit-mask:var(--nav-icon) center/contain no-repeat;vertical-align:-2px;margin-right:7px;opacity:.8;transition:color .18s,opacity .18s,transform .18s}
.navlinks a.active::before,.private-tabs a[aria-current=page]::before{color:var(--blue);opacity:1}
.user-dropdown>a{display:flex;align-items:center}.account-nav .submit-nav{display:flex;align-items:center}
'''
for name,path in icons.items():
 svg='<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="black" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round">'+path+'</svg>'
 css+='a[href="#'+name+'"]{--nav-icon:url("data:image/svg+xml,'+quote(svg,safe='')+'")}\n'
css+='''
/* Buttons respond gently to hover, keyboard focus and press. */
.btn,.login,.watch,.evidence-button,.play,.menu{transition:transform .18s ease,background .18s ease,border-color .18s ease,color .18s ease,box-shadow .18s ease;transform:translateY(0)}
.btn span[aria-hidden=true]{display:inline-block;transition:transform .18s ease}
@media(hover:hover){.btn:not(:disabled):hover,.login:hover,.menu:hover{transform:translateY(-2px);box-shadow:0 4px 12px #0003}.btn:hover span[aria-hidden=true]{transform:translateX(3px)}.watch:not(:disabled):hover,.evidence-button:hover{transform:translateY(-1px)}.play:hover{transform:scale(1.06)}.navlinks a:hover::before,.private-tabs a:hover::before{transform:translateY(-1px);opacity:1}}
.btn:not(:disabled):active,.login:active,.watch:not(:disabled):active,.evidence-button:active,.play:active,.menu:active{transform:translateY(1px) scale(.98);box-shadow:none}
.btn:disabled{transform:none;box-shadow:none}
@media(min-width:761px) and (max-width:1150px){.navlinks{gap:10px}.navlinks a::before{width:14px;height:14px;flex-basis:14px;margin-right:5px}.account-nav .submit-nav{max-width:90px}.nav{gap:14px}}
@media(max-width:760px){.private-tabs{gap:12px}.private-tabs a::before{width:14px;height:14px;margin-right:5px}.account-nav .submit-nav{max-width:68px}.account-nav .submit-nav::before{width:13px;height:13px;flex-basis:13px;margin-right:5px}}
@media(prefers-reduced-motion:reduce){.btn,.login,.watch,.evidence-button,.play,.menu,.btn span[aria-hidden=true],.navlinks a::before,.private-tabs a::before{transition:none!important;transform:none!important}}
'''
p=Path('outputs/udl-styles.css');p.write_text(p.read_text(encoding='utf-8-sig')+css,encoding='utf-8')
