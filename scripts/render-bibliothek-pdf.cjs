const fs = require("fs");
const path = require("path");
const Module = require("module");

const workspace = path.resolve(__dirname, "..");
const bundledNodeModules =
  process.env.CODEX_NODE_MODULES ||
  "C:\\Users\\Mahi0\\.cache\\codex-runtimes\\codex-primary-runtime\\dependencies\\node\\node_modules";

Module.globalPaths.push(bundledNodeModules);

function requirePackage(name) {
  try {
    return require(name);
  } catch {
    const pnpmDir = path.join(bundledNodeModules, ".pnpm");
    if (fs.existsSync(pnpmDir)) {
      const escaped = name.replace("/", "+");
      const packageDir = fs
        .readdirSync(pnpmDir)
        .find((entry) => entry === name || entry.startsWith(`${escaped}@`) || entry.startsWith(`${name}@`));

      if (packageDir) {
        return require(path.join(pnpmDir, packageDir, "node_modules", name));
      }
    }

    return require(path.join(bundledNodeModules, name));
  }
}

const { marked } = requirePackage("marked");
const { chromium } = requirePackage("playwright");
const { PDFDocument } = requirePackage("pdf-lib");

const inputPath = path.join(workspace, "BIBLIOTHEK_PLUE_ANGABE.md");
const htmlPath = path.join(workspace, "BIBLIOTHEK_PLUE_ANGABE.html");
const pdfPath = path.join(workspace, "BIBLIOTHEK_PLUE_ANGABE.pdf");

const chromeCandidates = [
  "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
  "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe",
];

const executablePath = chromeCandidates.find((candidate) => fs.existsSync(candidate));
if (!executablePath) {
  throw new Error("No Chrome or Edge executable found for PDF rendering.");
}

marked.setOptions({
  gfm: true,
  mangle: false,
  headerIds: false,
});

const markdown = fs.readFileSync(inputPath, "utf8");
const bodyHtml = marked.parse(markdown);

const architectureDiagram = `
<section class="visual page-avoid">
  <div class="visual-title">Visualisierung: Architektur</div>
  <div class="pipeline">
    <div class="pipe-card"><strong>HTTP</strong><span>Request / Response</span></div>
    <div class="arrow">→</div>
    <div class="pipe-card"><strong>Controller</strong><span>Routing, Statuscodes</span></div>
    <div class="arrow">→</div>
    <div class="pipe-card"><strong>Service</strong><span>Businesslogik</span></div>
    <div class="arrow">→</div>
    <div class="pipe-card"><strong>Mapper</strong><span>Entity ↔ DTO</span></div>
    <div class="arrow">→</div>
    <div class="pipe-card"><strong>DbContext</strong><span>EF Core, LINQ</span></div>
    <div class="arrow">→</div>
    <div class="pipe-card"><strong>SQLite</strong><span>Persistenz / Tests</span></div>
  </div>
</section>`;

const erDiagram = `
<section class="visual page-avoid">
  <div class="visual-title">Visualisierung: Datenmodell</div>
  <div class="er-grid">
    <div class="entity">
      <h3>Author</h3>
      <p>Id, Name, Country, BirthYear</p>
      <small>1 Author hat n Books</small>
    </div>
    <div class="relation">1 → n</div>
    <div class="entity">
      <h3>Book</h3>
      <p>ISBN, Title, Genre, Copies</p>
      <small>1 Book hat n Loans</small>
    </div>
    <div class="entity">
      <h3>BookLoan</h3>
      <p>BorrowedBy, BorrowedAt, DueAt, ReturnedAt, Status</p>
      <small>n Loans gehoeren zu 1 Book</small>
    </div>
  </div>
</section>`;

const testDiagram = `
<section class="visual page-avoid">
  <div class="visual-title">Visualisierung: Teststrategie</div>
  <div class="test-stack">
    <div class="test-layer api"><strong>REST Integrationstests</strong><span>Controller, Routing, JSON, HTTP-Codes</span></div>
    <div class="test-layer service"><strong>Service Tests</strong><span>Businessregeln, Exceptions, DTOs</span></div>
    <div class="test-layer db"><strong>DbContext Tests</strong><span>EF Mapping, SQLite In-Memory, LINQ</span></div>
  </div>
</section>`;

const endpointDiagram = `
<section class="visual page-avoid">
  <div class="visual-title">Visualisierung: wichtigste Endpunkte</div>
  <div class="endpoint-grid">
    <span>GET</span><code>/api/books</code><em>alle Buecher</em>
    <span>POST</span><code>/api/books</code><em>Buch erstellen</em>
    <span>GET</span><code>/api/books/available</code><em>LINQ: verfuegbar</em>
    <span>GET</span><code>/api/books/by-author</code><em>LINQ: Author</em>
    <span>POST</span><code>/api/loans</code><em>ausleihen</em>
    <span>PUT</span><code>/api/loans/{id}/return</code><em>zurueckgeben</em>
    <span>GET</span><code>/api/loans/overdue</code><em>LINQ: ueberfaellig</em>
  </div>
</section>`;

const enhancedBody = bodyHtml
  .replace("<h2>Ausgangslage</h2>", `<h2>Ausgangslage</h2>${architectureDiagram}`)
  .replace("<h2>Aufgabe 1: OR-Mapping und DbContext</h2>", `<h2>Aufgabe 1: OR-Mapping und DbContext</h2>${erDiagram}`)
  .replace("<h2>Aufgabe 3: REST API</h2>", `<h2>Aufgabe 3: REST API</h2>${endpointDiagram}`)
  .replace("<h2>Aufgabe 5: Tests</h2>", `<h2>Aufgabe 5: Tests</h2>${testDiagram}`);

const html = `<!doctype html>
<html lang="de">
<head>
  <meta charset="utf-8">
  <title>POS PLUE Uebungsangabe - Bibliothek</title>
  <style>
    @page {
      size: A4;
      margin: 16mm 15mm 17mm;
    }

    * {
      box-sizing: border-box;
    }

    body {
      margin: 0;
      color: #172033;
      background: #ffffff;
      font-family: "Segoe UI", Arial, sans-serif;
      font-size: 10.4pt;
      line-height: 1.5;
    }

    h1 {
      margin: 0 0 14px;
      padding: 18px 20px 20px;
      color: #ffffff;
      background: linear-gradient(135deg, #24466f, #2f6f73);
      border-radius: 8px;
      font-size: 23pt;
      line-height: 1.15;
      letter-spacing: 0;
    }

    h2 {
      margin: 22px 0 8px;
      padding-bottom: 5px;
      color: #24466f;
      border-bottom: 2px solid #d8e4ef;
      font-size: 15.5pt;
      break-after: avoid;
    }

    h3 {
      margin: 14px 0 5px;
      color: #2f4f5f;
      font-size: 12.5pt;
      break-after: avoid;
    }

    p {
      margin: 0 0 8px;
    }

    ul {
      margin: 5px 0 10px 20px;
      padding: 0;
    }

    li {
      margin: 2px 0;
    }

    code {
      padding: 1px 4px;
      border-radius: 4px;
      background: #edf3f8;
      color: #16314a;
      font-family: Consolas, "Courier New", monospace;
      font-size: 9.5pt;
    }

    strong {
      color: #122137;
    }

    .visual {
      margin: 12px 0 16px;
      padding: 12px;
      border: 1px solid #c7d7e5;
      border-radius: 8px;
      background: #f8fbfd;
    }

    .page-avoid {
      break-inside: avoid;
    }

    .visual-title {
      margin-bottom: 9px;
      color: #24466f;
      font-weight: 700;
      font-size: 11.5pt;
    }

    .pipeline {
      display: grid;
      grid-template-columns: 1fr 18px 1fr 18px 1fr;
      gap: 7px;
      align-items: center;
    }

    .pipeline .pipe-card:nth-of-type(n + 4),
    .pipeline .arrow:nth-of-type(n + 4) {
      margin-top: 7px;
    }

    .pipe-card {
      min-height: 58px;
      padding: 8px;
      border: 1px solid #b8ccdc;
      border-radius: 6px;
      background: #ffffff;
    }

    .pipe-card strong,
    .pipe-card span,
    .test-layer strong,
    .test-layer span {
      display: block;
    }

    .pipe-card span,
    .test-layer span,
    .entity small {
      color: #506275;
      font-size: 8.6pt;
    }

    .arrow,
    .relation {
      color: #2f6f73;
      font-weight: 700;
      text-align: center;
    }

    .er-grid {
      display: grid;
      grid-template-columns: 1.1fr 52px 1.1fr;
      gap: 10px;
      align-items: center;
    }

    .entity {
      min-height: 84px;
      padding: 9px 10px;
      border: 1px solid #b8ccdc;
      border-radius: 6px;
      background: #ffffff;
    }

    .entity h3 {
      margin: 0 0 3px;
      font-size: 11pt;
    }

    .entity p {
      margin: 0 0 4px;
      font-size: 9.3pt;
    }

    .wide {
      border-color: #c9b77f;
      background: #fffdf7;
    }

    .endpoint-grid {
      display: grid;
      grid-template-columns: 54px 1.25fr 1fr;
      gap: 6px 8px;
      align-items: center;
    }

    .endpoint-grid span {
      padding: 3px 6px;
      border-radius: 4px;
      color: #ffffff;
      background: #2f6f73;
      font-weight: 700;
      text-align: center;
      font-size: 8.4pt;
    }

    .endpoint-grid em {
      color: #506275;
      font-style: normal;
      font-size: 9pt;
    }

    .test-stack {
      display: grid;
      gap: 7px;
    }

    .test-layer {
      padding: 9px 12px;
      border-radius: 6px;
      color: #152334;
      border: 1px solid transparent;
    }

    .api {
      margin-left: 60px;
      margin-right: 60px;
      background: #f1f6fb;
      border-color: #bed2e4;
    }

    .service {
      margin-left: 32px;
      margin-right: 32px;
      background: #f5faf8;
      border-color: #b8d9cf;
    }

    .db {
      background: #fffaf0;
      border-color: #dbc88e;
    }
  </style>
</head>
<body>
  ${enhancedBody}
</body>
</html>`;

fs.writeFileSync(htmlPath, html, "utf8");

(async () => {
  const browser = await chromium.launch({
    executablePath,
    headless: true,
    args: ["--disable-gpu", "--no-sandbox"],
  });
  const page = await browser.newPage({ viewport: { width: 1240, height: 1754 } });
  await page.goto(`file://${htmlPath.replaceAll("\\", "/")}`, { waitUntil: "load" });
  await page.pdf({
    path: pdfPath,
    format: "A4",
    printBackground: true,
    preferCSSPageSize: true,
  });
  await browser.close();

  const pdfBytes = fs.readFileSync(pdfPath);
  const pdf = await PDFDocument.load(pdfBytes);
  console.log(JSON.stringify({
    htmlPath,
    pdfPath,
    pages: pdf.getPageCount(),
    bytes: pdfBytes.length,
    executablePath,
  }, null, 2));
})().catch((error) => {
  console.error(error);
  process.exit(1);
});
