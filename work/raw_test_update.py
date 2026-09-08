from pathlib import Path
p=Path('work/check_stage1.cjs');s=p.read_text(encoding='utf-8-sig')
s=s.replace("el('raw-url').value='';el('device')", "el('raw-url').value='https://example.com/raw-footage';el('device')")
s=s.replace("console.log('OK:", "run('globalThis.bounds=[0,1,250,251].map(n=>rawRequired({aredlRank:n}))');assert.deepEqual(Array.from(c.bounds),[false,true,true,false]);\nrun('session=demoUsers[0];submitPage();selectedLevel=3');el('percentage').value='100';el('fps').value='240';el('video-url').value='https://youtube.com/watch?v=demo';el('raw-url').value='';el('submission-form').onsubmit({preventDefault(){}});assert.ok(el('raw-url-error').textContent.includes('obligatorio'));\nconsole.log('OK:")
Path('work/check_stage1_raw.cjs').write_text(s,encoding='utf-8')
