from pathlib import Path
import xml.etree.ElementTree as E
ns='http://www.w3.org/2000/svg';E.register_namespace('',ns);E.register_namespace('xlink','http://www.w3.org/1999/xlink')
root=E.fromstring(Path('outputs/assets/sol-uruguay.svg').read_text(encoding='utf-8-sig'))
root.find('{'+ns+'}title').text='Sol de Mayo — contorno celeste UDL'
groups=root.findall('{'+ns+'}g');rays,face=groups
# Remove painted shading, retaining the original outlines and facial geometry.
for parent in list(rays.iter()):
 for child in list(parent):
  if child.tag=='{'+ns+'}path' and child.get('fill')=='#7b3f00':parent.remove(child)
for g,width in [(rays,'.14'),(face,'1.25')]:
 for e in g.iter():
  for key in ['fill','stroke','stroke-width','stroke-miterlimit','stroke-linecap']:
   e.attrib.pop(key,None)
 g.set('fill','none');g.set('stroke','#72c5fc');g.set('stroke-width',width);g.set('stroke-linejoin','round');g.set('stroke-linecap','round')
# Mask ray geometry behind the face instead of filling the central disk.
defs=E.SubElement(root,'{'+ns+'}defs');mask=E.SubElement(defs,'{'+ns+'}mask',{'id':'ray-outline-mask','maskUnits':'userSpaceOnUse','x':'-40','y':'-40','width':'80','height':'80'})
E.SubElement(mask,'{'+ns+'}rect',{'x':'-40','y':'-40','width':'80','height':'80','fill':'white'})
E.SubElement(mask,'{'+ns+'}circle',{'r':'11','fill':'black'})
wrapper=E.Element('{'+ns+'}g',{'mask':'url(#ray-outline-mask)'})
for c in list(rays):
 if c.tag!='{'+ns+'}circle':rays.remove(c);wrapper.append(c)
rays.insert(0,wrapper)
E.ElementTree(root).write('outputs/assets/sol-uruguay-contorno.svg',encoding='utf-8',xml_declaration=True)
