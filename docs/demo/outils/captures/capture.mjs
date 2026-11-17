// Captures et vidéo de démo du Lycée Horizon Démo sur smallapp.org.
// Usage :
//   node capture.mjs login  <etat.json>          -> ouvre Chromium, l'utilisateur se connecte lui-même
//   node capture.mjs shots  <etat.json> <dossier> -> captures 1440x900 de chaque écran
//   node capture.mjs video  <etat.json> <dossier> -> visite filmée 1920x1080 (webm)
// Le script ne fait que naviguer et défiler : aucune saisie, aucun enregistrement.
import { chromium } from 'playwright';
import fs from 'node:fs';
import path from 'node:path';

const BASE = 'https://smallapp.org';
const [mode, statePath, outDir] = process.argv.slice(2);

const ECRANS = [
  ['tableau-de-bord', '/'],
  ['eleves', '/eleves'],
  ['inscriptions', '/inscriptions'],
  ['parents', '/parents'],
  ['enseignants', '/enseignants'],
  ['matieres', '/matieres'],
  ['epreuves', '/epreuves'],
  ['listes-classes', '/listes-classes'],
  ['emploi-du-temps', '/emploi-du-temps'],
  ['bulletins', '/bulletins'],
  ['proces-verbal', '/proces-verbal'],
  ['paiements', '/paiements'],
  ['tresorerie', '/tresorerie'],
  ['configuration', '/configuration'],
  // Journal exclu : il affiche l'auteur SuperAdmin des imports.
];

// Textes qui ne doivent jamais apparaître à l'écran (console SuperAdmin).
const INTERDITS = ['SuperAdmin', "Vue d'ensemble du réseau", 'Super administration', 'Comptes SuperAdmin', 'Tout le réseau'];

const CSS_DEMO = `
  /* pas de barre de défilement ni de curseur clignotant à l'image */
  ::-webkit-scrollbar { display: none; }
  html { scrollbar-width: none; scroll-behavior: auto !important; }
  #demo-legende {
    position: fixed; left: 50%; bottom: 48px; transform: translateX(-50%);
    z-index: 2147483647; max-width: 80%; padding: 18px 34px; border-radius: 14px;
    background: rgba(20, 22, 40, .86); color: #fff; font: 600 30px/1.35 'Public Sans', system-ui, sans-serif;
    text-align: center; box-shadow: 0 10px 40px rgba(0,0,0,.25); opacity: 0; transition: opacity .45s ease;
    pointer-events: none;
  }
  #demo-legende.visible { opacity: 1; }
`;

async function preparer(page) {
  await page.addStyleTag({ content: CSS_DEMO }).catch(() => {});
  // Masque le pied de page « Thème Sneat (ThemeSelection, MIT) ».
  await page.evaluate(() => {
    for (const el of document.querySelectorAll('footer, .content-footer, .footer')) {
      if (/Sneat|ThemeSelection/i.test(el.textContent)) el.style.display = 'none';
    }
  }).catch(() => {});
}

async function attendre(page) {
  await page.waitForLoadState('networkidle', { timeout: 20000 }).catch(() => {});
  // Attend la fin des indicateurs de chargement éventuels.
  await page.waitForFunction(() =>
    !document.querySelector('.spinner-border, .spinner-grow, .loading, .placeholder, .placeholder-glow, [class*="skeleton"]')
    && [...document.querySelectorAll('tbody')].every((tb) => tb.innerText.trim().length > 0),
    null, { timeout: 8000 }).catch(() => {});
  await page.waitForTimeout(1200);
}

async function verifier(page, nom) {
  const texte = await page.evaluate(() => document.body.innerText);
  const trouve = INTERDITS.filter((t) => texte.includes(t));
  if (trouve.length) throw new Error(`Écran « ${nom} » : texte SuperAdmin visible (${trouve.join(', ')}). Arrêt.`);
  if (!texte.includes('Lycée Horizon Démo')) throw new Error(`Écran « ${nom} » : pas sur le Lycée Horizon Démo. Arrêt.`);
}

async function aller(page, route, nom) {
  await page.goto(BASE + route, { waitUntil: 'domcontentloaded' });
  await attendre(page);
  if (page.url().includes('login')) throw new Error('Session expirée : relancer « login ».');
  await preparer(page);
  await verifier(page, nom);
}

async function login() {
  const browser = await chromium.launch({ headless: false, args: ['--start-maximized'] });
  const context = await browser.newContext({ viewport: null, locale: 'fr-FR' });
  const page = await context.newPage();
  await page.goto(BASE + '/');
  console.log('Connectez-vous dans la fenêtre Chromium (Connexion à votre établissement, code puis PIN).');
  await page.waitForFunction(
    () => {
      const t = document.body?.innerText || '';
      return t.includes('Élèves inscrits') && t.includes('Lycée Horizon Démo') && !t.includes("Vue d'ensemble du réseau");
    },
    null,
    { timeout: 15 * 60 * 1000, polling: 1000 },
  );
  await page.waitForTimeout(2000);
  await context.storageState({ path: statePath });
  console.log('Connexion détectée, session enregistrée.');
  await browser.close();
}

async function shots() {
  fs.mkdirSync(path.join(outDir, 'pleine-page'), { recursive: true });
  const browser = await chromium.launch();
  const context = await browser.newContext({
    storageState: statePath, viewport: { width: 1440, height: 900 }, deviceScaleFactor: 2, locale: 'fr-FR',
  });
  const page = await context.newPage();
  for (const [nom, route] of ECRANS) {
    try {
      await aller(page, route, nom);
      await page.screenshot({ path: path.join(outDir, `${nom}.png`) });
      await page.screenshot({ path: path.join(outDir, 'pleine-page', `${nom}.png`), fullPage: true });
      console.log('ok', nom);
    } catch (e) {
      console.log('ÉCHEC', nom, e.message.split('\n')[0]);
      if (/SuperAdmin|Session expirée|Horizon/.test(e.message)) break;
    }
  }
  // Écrans de détail : on ne fait que cliquer pour ouvrir une vue, rien n'est enregistré.
  const details = [
    ['alertes', '/', async () => { await versTexte(page, 'Top 5 élèves', 40, 10); }],
    ['activite-recente', '/', async () => { await versTexte(page, 'Activité récente', 40, 10); }],
    ['bulletins-3eA', '/bulletins', async () => { await ouvrirBulletin(page); }],
    ['proces-verbal-3eA', '/proces-verbal', async () => { await page.getByText('3e A', { exact: true }).first().click(); await attendre(page); }],
  ];
  for (const [nom, route, action] of details) {
    try {
      await aller(page, route, nom);
      await action();
      await preparer(page);
      await verifier(page, nom);
      await page.screenshot({ path: path.join(outDir, `${nom}.png`) });
      await page.screenshot({ path: path.join(outDir, 'pleine-page', `${nom}.png`), fullPage: true });
      console.log('ok', nom, page.url());
    } catch (e) {
      console.log('ÉCHEC', nom, e.message.split('\n')[0]);
    }
  }
  await context.storageState({ path: statePath }).catch(() => {});
  await browser.close();
}

// ---- Vidéo -------------------------------------------------------------
async function legende(page, texte) {
  await page.evaluate((t) => {
    let el = document.getElementById('demo-legende');
    if (!el) { el = document.createElement('div'); el.id = 'demo-legende'; document.body.appendChild(el); }
    el.textContent = t;
    requestAnimationFrame(() => el.classList.add('visible'));
  }, texte);
}

// Défilement doux de la fenêtre (ou du conteneur qui défile) jusqu'à y, en ms.
async function defiler(page, y, ms = 2500) {
  await page.evaluate(async ({ y, ms }) => {
    const sc = document.scrollingElement;
    const debut = sc.scrollTop, fin = Math.min(y, sc.scrollHeight - innerHeight), t0 = performance.now();
    await new Promise((ok) => {
      const pas = (t) => {
        const k = Math.min(1, (t - t0) / ms), e = k < .5 ? 2 * k * k : 1 - Math.pow(-2 * k + 2, 2) / 2;
        sc.scrollTop = debut + (fin - debut) * e;
        k < 1 ? requestAnimationFrame(pas) : ok();
      };
      requestAnimationFrame(pas);
    });
  }, { y, ms });
}

async function versTexte(page, texte, marge = 140, ms = 2500) {
  const y = await page.evaluate(({ texte, marge }) => {
    const w = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
    let n;
    while ((n = w.nextNode())) if (n.textContent.trim().startsWith(texte)) {
      return n.parentElement.getBoundingClientRect().top + scrollY - marge;
    }
    return null;
  }, { texte, marge });
  if (y != null) await defiler(page, y, ms);
}

async function survoler(page, texte) {
  const el = page.getByText(texte, { exact: false }).first();
  const box = await el.boundingBox().catch(() => null);
  if (box) await page.mouse.move(box.x + box.width / 2, box.y + box.height / 2, { steps: 25 });
}

// Ouvre la 3e A, trie par rang et affiche le bulletin du premier élève (lecture seule).
async function ouvrirBulletin(page, lent = false) {
  if (lent) { await survoler(page, '3e A'); await page.waitForTimeout(800); }
  await page.getByText('3e A', { exact: true }).first().click();
  await page.locator('.eleve-item').first().waitFor({ timeout: 20000 });
  await attendre(page);
  await page.locator('select').filter({ hasText: 'Rang (classement)' }).selectOption({ label: 'Rang (classement)' });
  await page.waitForTimeout(lent ? 1500 : 800);
  const premier = page.locator('.eleve-item').first();
  if (lent) { await premier.hover(); await page.waitForTimeout(600); }
  await premier.click();
  await attendre(page);
}

// Navigation par le menu latéral : pas de rechargement complet de l'application.
// Texte qui prouve que les données de la page sont arrivées (et pas seulement le squelette).
const MARQUEURS = { '/': 'Inès NKOLO', '/enseignants': 'TAMBA', '/eleves': 'DEMO26', '/inscriptions': 'DEMO26' };

async function menu(page, route, nom) {
  await page.evaluate(() => document.getElementById('demo-legende')?.classList.remove('visible'));
  const lien = page.locator(`aside a[href="${route}"], nav a[href="${route}"], a.menu-link[href="${route}"]`).first();
  await lien.scrollIntoViewIfNeeded();
  const box = await lien.boundingBox();
  if (box) await page.mouse.move(box.x + box.width / 2, box.y + box.height / 2, { steps: 20 });
  await page.waitForTimeout(350);
  await lien.click();
  await attendre(page);
  if (MARQUEURS[route]) {
    await page.waitForFunction((m) => document.body.innerText.includes(m), MARQUEURS[route], { timeout: 30000 }).catch(() => console.log('marqueur absent', route));
    await page.waitForTimeout(800);
  }
  await preparer(page);
  await verifier(page, nom);
  await page.evaluate(() => document.scrollingElement.scrollTo(0, 0));
}

async function scene(page, route, nom, texte, actions, tenue = 3500) {
  await menu(page, route, nom);
  await legende(page, texte);
  await page.waitForTimeout(800);
  if (actions) await actions();
  await page.waitForTimeout(tenue);
}

async function video() {
  fs.mkdirSync(outDir, { recursive: true });
  const browser = await chromium.launch();
  const context = await browser.newContext({
    storageState: statePath, viewport: { width: 1920, height: 1080 }, locale: 'fr-FR',
    recordVideo: { dir: outDir, size: { width: 1920, height: 1080 } },
  });
  const page = await context.newPage();
  const tDebutVideo = Date.now();
  await aller(page, '/', 'tableau-de-bord');
  await page.waitForTimeout(1500);
  const debut = (Date.now() - tDebutVideo) / 1000; // tout ce qui précède est coupé au montage

  await legende(page, 'Le chef d’établissement voit l’essentiel d’un coup d’œil.');
  await page.waitForTimeout(1500);
  for (const t of ['Élèves inscrits', 'Classes actives', 'Enseignants', 'Notes saisies']) {
    await survoler(page, t); await page.waitForTimeout(900);
  }
  await versTexte(page, 'Évolution des inscriptions', 120, 3000);
  await page.waitForTimeout(2500);
  await legende(page, 'Les alertes utiles remontent toutes seules.');
  await versTexte(page, 'Top 5 élèves', 160, 3000);
  await page.waitForTimeout(4000);

  await scene(page, '/eleves', 'eleves', 'Tous les élèves, leurs classes et leurs parents.', async () => {
    await defiler(page, 500, 3500);
  });
  await scene(page, '/inscriptions', 'inscriptions', 'Inscriptions et effectifs suivis en temps réel.', async () => {
    await defiler(page, 400, 3000);
  });
  await scene(page, '/enseignants', 'enseignants', 'Les enseignants et leurs matières par classe.', null, 3500);
  await scene(page, '/bulletins', 'bulletins', 'Les bulletins de toute la classe, prêts en un clic.', async () => {
    await page.waitForTimeout(1200);
    await ouvrirBulletin(page, true);
    await preparer(page);
    await legende(page, 'Classement par rang et aperçu du bulletin de chaque élève.');
    await page.waitForTimeout(2500);
    await defiler(page, 700, 4000);
  }, 3000);
  await scene(page, '/proces-verbal', 'proces-verbal', 'Procès-verbal d’évaluation imprimable en PDF.', async () => {
    await page.waitForTimeout(1200);
    await survoler(page, '3e A'); await page.waitForTimeout(800);
    await page.getByText('3e A', { exact: true }).first().click();
    await attendre(page); await preparer(page);
    await legende(page, 'Procès-verbal d’évaluation imprimable en PDF.');
    await page.waitForTimeout(1500);
    await defiler(page, 600, 4000);
  }, 3000);
  await scene(page, '/', 'fin', 'Toute votre école dans un seul outil.', null, 4500);

  const duree = ((Date.now() - tDebutVideo) / 1000 - debut).toFixed(0);
  await context.storageState({ path: statePath }).catch(() => {});
  const v = page.video();
  await context.close();
  const brut = await v.path();
  fs.renameSync(brut, path.join(outDir, 'visite-brute.webm'));
  fs.writeFileSync(path.join(outDir, 'debut.txt'), debut.toFixed(2));
  console.log('vidéo ok', `début coupé à ${debut.toFixed(1)}s, durée utile ~${duree}s`);
  await browser.close();
}

const modes = { login, shots, video };
if (!modes[mode] || !statePath) {
  console.log('usage: node capture.mjs <login|shots|video> <etat.json> [dossier]');
  process.exit(1);
}
await modes[mode]();
