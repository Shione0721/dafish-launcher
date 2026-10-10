#!/usr/bin/env node
/**
 * 鲸鱼娘 · DSH 插件一键更新器 v1.1.1
 * 复用 dshmarket 公开「插件更新 API v1」（http://127.0.0.1:19387/dsh-market/api/v1/*）
 *
 * 用法：
 *   node updater.mjs check                   只检查（默认）
 *   node updater.mjs apply                   检查并更新全部（先问一次）
 *   node updater.mjs apply --yes             跳过确认
 *   node updater.mjs apply --yes --wait      有 agent 忙就等空闲再更新（默认最多等 60 分钟）
 *   node updater.mjs apply --only a,b        只更新列出的插件
 *   node updater.mjs apply --except a,b      跳过列出的插件
 *   node updater.mjs check --json            机器可读输出
 * 选项：--port 19387 | --base URL | --json | --yes | --wait | --wait-min 60 | --any
 * 默认只认主人自己的 DSH（profile=desktop）；要强制指别的实例才加 --any。
 * 说明：更新交给插件市场执行（自带兼容性校验与回滚点）。市场在「任一会话的 agent 正在跑」时会拒绝改动，--wait 会自己等到空闲。
 * 更新完成后重启 DeepSeek Harness 才会加载新版本（桌面「重启DSH生效更新」）。
 */
import fs from "node:fs";
import path from "node:path";
import readline from "node:readline";
import { fileURLToPath } from "node:url";

const HERE = path.dirname(fileURLToPath(import.meta.url));
const LOG_DIR = path.join(HERE, "logs");

const argv = process.argv.slice(2);
let mode = (argv[0] || "check").toLowerCase();
if (mode === "update") mode = "apply";
const has = (n) => argv.includes(n);
const opt = (n, d) => { const i = argv.indexOf(n); return i >= 0 && argv[i + 1] && !argv[i + 1].startsWith("--") ? argv[i + 1] : d; };
const PORT = opt("--port", "19387");
const BASE = String(opt("--base", "http://127.0.0.1:" + PORT)).replace(/[/]+$/, "");
const AS_JSON = has("--json");
const YES = has("--yes");
const WAIT = has("--wait");
const WAIT_MIN = Math.max(1, Number(opt("--wait-min", "60")) || 60);
const WAIT_DEADLINE = Date.now() + WAIT_MIN * 60 * 1000;
const ONLY = (opt("--only", "") || "").split(",").map((s) => s.trim()).filter(Boolean);
const EXCEPT = (opt("--except", "") || "").split(",").map((s) => s.trim()).filter(Boolean);
const ALLOW_ANY = has("--any");

const trace = (line) => { try { fs.mkdirSync(LOG_DIR, { recursive: true }); fs.appendFileSync(path.join(LOG_DIR, "waiter.log"), new Date().toLocaleString("zh-CN") + "  " + line + String.fromCharCode(10), "utf8"); } catch { } };
const say = (...a) => { const line = a.join(" "); if (!AS_JSON) console.log(line); if (WAIT) trace(line); };
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
const ts = () => new Date().toISOString().replace(/[:.]/g, "-").slice(0, 19);
const W = (s) => { let w = 0; for (const ch of String(s)) { const cp = ch.codePointAt(0); w += (cp >= 0x1100 && cp <= 0xFFFD) ? 2 : 1; } return w; };
const pad = (s, n) => { let out = String(s); while (W(out) < n) out += " "; return out; };
const short = (v) => { const s = String(v); return /^[0-9a-f]{40}$/i.test(s) ? s.slice(0, 7) : s; };

function bail(msg, code = 1) { console.error(msg); process.exitCode = code; }

async function api(p, opts) {
  const o = opts || {};
  const ctl = new AbortController();
  const t = setTimeout(() => ctl.abort(), o.timeout || 20000);
  try {
    const res = await fetch(BASE + p, {
      method: o.method || "GET", signal: ctl.signal,
      headers: { "Content-Type": "application/json" },
      body: o.body === undefined ? undefined : JSON.stringify(o.body),
    });
    const text = await res.text();
    let json = null; try { json = JSON.parse(text); } catch { }
    return { status: res.status, json, text };
  } finally { clearTimeout(t); }
}

async function confirm(q) {
  if (YES) return true;
  if (!process.stdin.isTTY) return false;
  const rl = readline.createInterface({ input: process.stdin, output: process.stdout });
  const a = await new Promise((r) => rl.question(q, r));
  rl.close();
  return /^y(es)?$/i.test(a.trim());
}

/** 等所有会话空闲（市场在任一 agent 忙时拒绝改动插件文件）。空闲返回 true。 */
async function waitIdle() {
  let said = false;
  for (;;) {
    if (Date.now() > WAIT_DEADLINE) return false;
    let ra = null;
    try { const st = await api("/dsh-market/status", { timeout: 8000 }); ra = st.json && Array.isArray(st.json.runningAgents) ? st.json.runningAgents : null; } catch { }
    if (ra && ra.length === 0) { if (said) say("      空闲了，继续～"); return true; }
    if (!said) { said = true; say("     ⏳ 现在有 agent 在运行，先等它空闲（每 20 秒看一眼）：" + (ra ? ra.join(", ") : "状态未知")); }
    await sleep(20000);
  }
}

function finish(code, obj) {
  let f = null;
  try {
    fs.mkdirSync(LOG_DIR, { recursive: true });
    const rec = JSON.stringify({ at: new Date().toISOString(), base: BASE, ...obj }, null, 2);
    f = path.join(LOG_DIR, "update-" + ts() + ".json");
    fs.writeFileSync(f, rec, "utf8");
    fs.writeFileSync(path.join(HERE, "last-report.json"), rec, "utf8");
  } catch { }
  if (f) say("   （日志：" + f + "）");
  if (AS_JSON) console.log(JSON.stringify(obj, null, 2));
  process.exitCode = code;
}

async function main() {
  say("🐋 鲸鱼娘 · DSH 插件一键更新器");
  say("   时间 " + new Date().toLocaleString("zh-CN") + (WAIT ? " · 等待模式（最多 " + WAIT_MIN + " 分钟）" : ""));
  let caps;
  try { caps = await api("/dsh-market/api/v1/capabilities", { timeout: 8000 }); }
  catch { return bail("❌ 连不上 DSH（" + BASE + "）——先把 DeepSeek Harness 桌面端打开再跑人家啦～"); }
  if (!caps.json || caps.json.schema !== "dsh-market/update-api/v1") return bail("❌ 这个 DSH 里没有插件市场更新接口（市场未安装或版本太老）。");
  const mk = caps.json;
  if (!ALLOW_ANY && (mk.profile !== "desktop" || mk.runtime !== "desktop")) {
    return bail("❌ 这个实例不是主人自己的 DeepSeek Harness（profile=" + mk.profile + "，runtime=" + mk.runtime + "）。人家只给自己那套更新；确实要指过去就加 --any。");
  }
  say("   目标：主人自己的 DeepSeek Harness（profile=" + mk.profile + " · 市场 v" + mk.marketVersion + " · " + BASE + "）");
  say("   正在检查全部插件（force 深度检查，稍等一下）…");

  const s = await api("/dsh-market/api/v1/updates/summary?force=1", { timeout: 240000 });
  if (!s.json || typeof s.json.updatable !== "number") return bail("❌ 检查失败（HTTP " + s.status + "）：" + String(s.text || "").slice(0, 300));
  const checked = s.json.checked;
  const pkgs = s.json.packages || [];

  say("");
  say("   检查了 " + checked + " 个插件，发现 " + pkgs.length + " 个可更新。");
  if (!pkgs.length) {
    say("   🎉 全部都是最新的，不用更新啦～");
    return finish(0, { mode: "check", checked, updatable: 0, packages: [] });
  }
  say("");
  say("   " + pad("#", 4) + pad("插件", 42) + pad("当前", 12) + pad("最新", 12) + "来源");
  pkgs.forEach((p, i) => say("   " + pad(String(i + 1), 4) + pad(p.name, 42) + pad(short(p.installedVersion), 12) + pad(short(p.latestVersion), 12) + "[" + p.source + "]"));
  say("");
  if (EXCEPT.length) say("   （跳过：" + EXCEPT.join("、") + "；要更新它们可在插件市场里自己点）");
  say("   👉 想一键全部更新：双击桌面「一键更新全部插件」；或命令行加 apply。");

  if (mode !== "apply") return finish(0, { mode: "check", checked, updatable: pkgs.length, packages: pkgs });

  const todo = pkgs.filter((p) => (ONLY.length ? ONLY.includes(p.name) : true) && !EXCEPT.includes(p.name));
  if (!todo.length) { say("   （按 --only/--except 过滤后，没有要更新的了）"); return finish(0, { mode: "apply", checked, updatable: pkgs.length, results: [] }); }

  const go = await confirm("   现在更新这 " + todo.length + " 个插件？（由插件市场执行，自带兼容性校验和回滚点）[y/N] ");
  if (!go) { say("   好的，人家先不动它们～"); return finish(0, { mode: "apply", cancelled: true, checked, updatable: pkgs.length }); }

  const results = [];
  for (const p of todo) {
    say("");
    say("   ▶ " + p.name + "  " + p.installedVersion + " → " + p.latestVersion);
    let attempt = 0;
    let forceNext = false;
    for (;;) {
      if (WAIT) {
        const free = await waitIdle();
        if (!free) { say("     ⏰ 等太久了（超过 " + WAIT_MIN + " 分钟仍在忙），这个先跳过。"); results.push({ name: p.name, state: "skipped-timeout" }); break; }
      }
      attempt++;
      let r;
      try { r = await api("/dsh-market/api/v1/updates", { method: "POST", body: forceNext ? { packageName: p.name, force: true } : { packageName: p.name }, timeout: 30000 }); }
      catch (e) { say("     ✗ 请求失败：" + (e && e.message)); results.push({ name: p.name, state: "request-failed", message: String(e && e.message) }); break; }
      const opId = r.json && (r.json.operationId || (r.json.operation && (r.json.operation.operationId || r.json.operation.id)));
      if (!opId) {
        const msg = (r.json && (r.json.message || r.json.error)) || r.text;
        say("     ✗ 未受理（HTTP " + r.status + "）：" + String(msg).slice(0, 220));
        results.push({ name: p.name, state: "rejected", http: r.status, message: String(msg).slice(0, 400) });
        break;
      }
      let op = null, last = "";
      const deadline = Date.now() + 20 * 60 * 1000;
      while (Date.now() < deadline) {
        await sleep(1200);
        let o;
        try { o = await api("/dsh-market/api/v1/operations?operationId=" + encodeURIComponent(opId), { timeout: 20000 }); } catch { continue; }
        const root = o.json || null;
        op = (root && (root.operation || root)) || op;
        const st = (op && (op.state || op.status || op.phase)) || "?";
        if (st !== last) { last = st; if (st === "queued" || st === "running") say("     · " + st + "…"); }
        if (st === "succeeded" || st === "failed" || st === "cancelled" || st === "rolled-back") break;
      }
      const state = (op && (op.state || op.status || op.phase)) || "timeout";
      const fcode = op && op.failure && op.failure.code;
      const fmsg = op && op.failure && op.failure.message;
      if (state === "succeeded") {
        const inst = op && (op.installedVersion || (op.after && op.after.version) || op.toVersion);
        say("     ✓ 完成" + (inst ? "（已装 " + inst + "）" : "") + (op && op.outcome && op.outcome.restartRequired === true ? " · 重启后生效" : ""));
        results.push({ name: p.name, from: p.installedVersion, to: p.latestVersion, operationId: opId, state: "succeeded", installedVersion: inst || null, op: op });
        break;
      }
      if (state === "failed" && fcode === "AGENTS_RUNNING" && WAIT) {
        say("     ↻ 市场说现在有 agent 在跑，拒绝改动；等空闲后自动重试这个包（第 " + attempt + " 次）");
        await sleep(15000);
        continue;
      }
      if (state === "failed" && fcode === "RELEASE_TOO_FRESH" && !forceNext) {
        say("     · 新版本发布还太新（release-age 保护），用 force 再来一次…");
        forceNext = true;
        continue;
      }
      if (state === "failed" && fcode === "VERSION_UNCHANGED") {
        say("     · 版本没有变化（可能镜像还没刷新），先跳过。");
        results.push({ name: p.name, from: p.installedVersion, to: p.latestVersion, operationId: opId, state: "unchanged", op: op });
        break;
      }
      say("     ✗ " + state + (fcode ? "（" + fcode + "）" : "") + (fmsg ? "：" + String(fmsg).slice(0, 200) : ""));
      results.push({ name: p.name, from: p.installedVersion, to: p.latestVersion, operationId: opId, state, code: fcode || null, op: op });
      break;
    }
  }

  say("");
  const okN = results.filter((x) => x.state === "succeeded").length;
  const badN = results.length - okN;
  say("   ═════════════ 总结 ═════════════");
  say("   成功 " + okN + " 个" + (badN ? "，未成功 " + badN + " 个" : "") + "（共 " + results.length + " 个）");
  if (okN) say("   ↻ 重启 DeepSeek Harness 后新版本才生效（桌面「重启DSH生效更新」）。");
  return finish(badN ? 1 : 0, { mode: "apply", checked, updatable: pkgs.length, results });
}

main().catch((e) => { console.error("❌ 出错了：" + ((e && e.stack) || e)); process.exitCode = 1; });
