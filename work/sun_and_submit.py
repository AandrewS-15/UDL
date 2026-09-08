from pathlib import Path
import xml.etree.ElementTree as E
ns='http://www.w3.org/2000/svg';E.register_namespace('',ns);E.register_namespace('xlink','http://www.w3.org/1999/xlink')
p=Path('outputs/assets/sol-uruguay-limpio.svg');root=E.fromstring(p.read_text());g=root.find('{'+ns+'}g')
# Preserve separated rays and circular disk, replace the simplified face with the
# original public-domain Sun of May features, without double outline artifacts.
children=list(g)
for el in children[17:]:g.remove(el)
original=E.parse('outputs/assets/sol-uruguay.svg').getroot();face=original.findall('{'+ns+'}g')[-1]
face.set('transform','translate(500 500) scale(1.48)');face.set('fill','#1c3d56');face.set('stroke','none')
for el in face.iter():
 if el.get('fill'):el.set('fill','#1c3d56')
root.append(face)
E.ElementTree(root).write(p,encoding='utf-8',xml_declaration=True)
p=Path('outputs/udl-account.js');s=p.read_text(encoding='utf-8')
s=s.replace('<a class="text-link submit-nav" href="#submit">Enviar récord</a>','')
s=s.replace('function navAccount(){', '''function navAccount(){const oldSubmit=document.getElementById('submit-navigation');if(!session){oldSubmit?.remove()}else if(!oldSubmit){const link=document.createElement('a');link.id='submit-navigation';link.href='#submit';link.dataset.nav='submit';link.textContent='Submit a record';document.getElementById('navlinks').append(link)}''')
s=s.replace("hash==='classification'?'classification':''", "hash==='classification'?'classification':hash==='submit'?'submit':''")
p.write_text(s,encoding='utf-8')
