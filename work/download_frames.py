import urllib.request,ssl
from pathlib import Path
assets={'acu.jpg':'https://i.ytimg.com/vi/z6l74Mkoxm8/hq1.jpg','bloodbath.jpg':'https://i.ytimg.com/vi/twTw4fjT0ik/hq1.jpg','cataclysm.jpg':'https://i.ytimg.com/vi/UtQnr47L7Q0/hq1.jpg','zodiac.jpg':'https://i.ytimg.com/vi/rVMzHiyp9oE/hq1.jpg','sol-uruguay.svg':'https://upload.wikimedia.org/wikipedia/commons/9/92/Sol_de_Mayo-Bandera_de_Uruguay.svg'}
for name,url in assets.items():
 try:
  data=urllib.request.urlopen(urllib.request.Request(url,headers={'User-Agent':'UDL-prototype/1.0'}),timeout=20,context=ssl._create_unverified_context()).read()
  Path('outputs/assets',name).write_bytes(data)
  print(name,len(data))
 except Exception as e: print(name,str(e))

