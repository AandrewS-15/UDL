from pathlib import Path
import re
p=Path('outputs/index.html')
s=p.read_text(encoding='utf-8')
blocks=re.findall(r'<style>(.*?)</style>',s,re.S)
assert len(blocks)==1
css=blocks[0].strip()+'\n'
s=re.sub(r'<style>.*?</style>','<link rel="stylesheet" href="udl-styles.css">',s,count=1,flags=re.S)
# Move inline presentation into reusable classes as well.
inline={}
def replace_style(m):
    tag,style=m.group(1),m.group(2)
    if style not in inline: inline[style]='inline-style-'+str(len(inline)+1)
    cls=inline[style]
    if 'class="' in tag:
        tag=tag.replace('class="','class="'+cls+' ',1)
        return tag
    return tag+' class="'+cls+'"'
s=re.sub(r'(<[^>]*?) style="([^"]*)"',replace_style,s)
for style,cls in inline.items(): css+='.'+cls+'{'+style+'}\n'
Path('outputs/udl-styles.css').write_text(css,encoding='utf-8')
p.write_text(s,encoding='utf-8')
assert '<style>' not in s and ' style="' not in s
assert Path('outputs/udl-styles.css').is_file()
print('HTML vinculado a CSS externo; estilos inline migrados:',len(inline))

