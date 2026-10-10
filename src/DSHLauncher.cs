using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace DSHLauncher
{
    internal static class Ui
    {
        internal static float Factor = 1f;
        internal static int S(int v) { return (int)Math.Round(v * Factor); }
        [DllImport("user32.dll")]
        private static extern uint GetDpiForSystem();
        internal static void Init()
        {
            float f = 0f;
            try { f = GetDpiForSystem() / 96f; } catch { f = 0f; }
            if (f <= 0f)
            {
                try { using (Graphics g = Graphics.FromHwnd(IntPtr.Zero)) f = g.DpiX / 96f; }
                catch { f = 1f; }
            }
            if (f < 1f) f = 1f;
            Factor = f;
        }
    }

    internal static class Theme
    {
        internal static readonly Color BgTop = Color.FromArgb(38, 50, 90);
        internal static readonly Color BgBottom = Color.FromArgb(20, 26, 48);
        internal static readonly Color TitleTop = Color.FromArgb(32, 42, 76);
        internal static readonly Color TitleBottom = Color.FromArgb(22, 30, 56);
        internal static readonly Color White = Color.FromArgb(255, 255, 255);
        internal static readonly Color Muted = Color.FromArgb(172, 184, 214);
        internal static readonly Color Dim = Color.FromArgb(122, 136, 168);
        internal static readonly Color TitleText = Color.FromArgb(214, 224, 244);
        internal static readonly Color Track = Color.FromArgb(36, 46, 78);
        internal static readonly Color BarFrom = Color.FromArgb(160, 198, 252);
        internal static readonly Color BarTo = Color.FromArgb(92, 142, 232);
        internal static readonly Color BtnBgTop = Color.FromArgb(54, 68, 108);
        internal static readonly Color BtnBgBottom = Color.FromArgb(42, 52, 86);
        internal static readonly Color BtnHoverTop = Color.FromArgb(74, 92, 140);
        internal static readonly Color BtnHoverBottom = Color.FromArgb(58, 72, 112);
        internal static readonly Color BtnPrimaryTop = Color.FromArgb(82, 128, 220);
        internal static readonly Color BtnPrimaryBottom = Color.FromArgb(52, 88, 172);
        internal static readonly Color BtnPrimaryHoverTop = Color.FromArgb(104, 152, 240);
        internal static readonly Color BtnPrimaryHoverBottom = Color.FromArgb(72, 112, 200);
        internal static readonly Color CapsuleBg = Color.FromArgb(52, 66, 106);
        internal static readonly Color CapsuleText = Color.FromArgb(188, 202, 234);
        internal static readonly Color Glow = Color.FromArgb(90, 128, 170, 244);
        internal static readonly Color SepLine = Color.FromArgb(60, 96, 150, 210);
        internal static readonly Color AvatarEdge = Color.FromArgb(96, 124, 190);
        internal static readonly Color Gold = Color.FromArgb(232, 200, 120);
        internal static readonly Color GoldBright = Color.FromArgb(247, 222, 148);
        internal static readonly Color GoldDark = Color.FromArgb(196, 162, 82);
        internal static readonly Color OkDot = Color.FromArgb(110, 210, 150);
        internal static readonly Color Greeting = Color.FromArgb(214, 192, 152);

        internal static Font Font(float size, bool bold)
        {
            string[] names = new string[] { "Microsoft YaHei UI", "Microsoft YaHei", "SimHei" };
            FontStyle st = bold ? FontStyle.Bold : FontStyle.Regular;
            for (int i = 0; i < names.Length; i++)
            {
                Font f = new Font(names[i], size, st, GraphicsUnit.Point);
                if (string.Equals(f.FontFamily.Name, names[i], StringComparison.OrdinalIgnoreCase)) return f;
                f.Dispose();
            }
            return new Font(FontFamily.GenericSansSerif, size, st, GraphicsUnit.Point);
        }
    }

    internal static class Shapes
    {
        internal static GraphicsPath Round(Rectangle r, int radius)
        {
            GraphicsPath p = new GraphicsPath();
            int d = radius * 2;
            if (d <= 0) { p.AddRectangle(r); return p; }
            if (d > r.Width) d = r.Width;
            if (d > r.Height) d = r.Height;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
        internal static void FillRound(Graphics g, Brush b, Rectangle r, int radius)
        {
            using (GraphicsPath p = Round(r, radius)) g.FillPath(b, p);
        }

        internal static void PaintStar(Graphics g, int cx, int cy, int size, Color color)
        {
            PointF[] pts = new PointF[10];
            for (int i = 0; i < 10; i++)
            {
                double angle = -Math.PI / 2 + i * Math.PI / 5;
                int r = (i % 2 == 0) ? size : size / 2;
                pts[i] = new PointF(cx + (float)(r * Math.Cos(angle)), cy + (float)(r * Math.Sin(angle)));
            }
            using (GraphicsPath p = new GraphicsPath())
            {
                p.AddPolygon(pts);
                using (SolidBrush b = new SolidBrush(color)) g.FillPath(b, p);
            }
        }

        internal static void PaintBubble(Graphics g, int cx, int cy, int r, int alpha)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, 160, 200, 250)))
                g.FillEllipse(b, cx - r, cy - r, r * 2, r * 2);
            using (SolidBrush hb = new SolidBrush(Color.FromArgb(Math.Min(255, alpha + 50), 220, 235, 255)))
                g.FillEllipse(hb, cx - r * 2 / 3, cy - r * 2 / 3, r, r / 2);
        }

        internal static void PaintHeart(Graphics g, int cx, int cy, int size, Color color)
        {
            int r = Math.Max(1, size / 2);
            using (SolidBrush b = new SolidBrush(color))
            {
                g.FillEllipse(b, cx - size / 2, cy - r, r, r);
                g.FillEllipse(b, cx, cy - r, r, r);
                Point[] tri = new Point[] { new Point(cx - size / 2 + 1, cy), new Point(cx + size / 2 - 1, cy), new Point(cx, cy + size * 3 / 4) };
                g.FillPolygon(b, tri);
            }
        }
    }

    internal static class Avatar
    {
        // 0=左上角大头像；1..6=六个按钮各自的专属头像（风格各异）
        private static readonly string[] Res = {
            "WhaleAvatar.png", "av_open.png", "av_update.png", "av_check.png", "av_voice.png", "av_close.png", "av_version.png"
        };
        private static readonly Image[] cache = new Image[7];
        private static readonly bool[] tried = new bool[7];

        internal static Image Get() { return Get(0); }

        internal static Image Get(int i)
        {
            if (i < 0 || i >= Res.Length) i = 0;
            if (tried[i]) return cache[i];
            tried[i] = true;
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                using (Stream s = asm.GetManifestResourceStream(Res[i]))
                {
                    if (s != null)
                    {
                        byte[] buf = new byte[s.Length];
                        int read = 0;
                        while (read < buf.Length) { int n = s.Read(buf, read, buf.Length - read); if (n <= 0) break; read += n; }
                        cache[i] = Image.FromStream(new MemoryStream(buf));
                    }
                }
            }
            catch { }
            if (cache[i] == null && i != 0) return Get(0);
            return cache[i];
        }
        internal static void Paint(Graphics g, Rectangle r)
        {
            Rectangle glow = new Rectangle(r.X - Ui.S(6), r.Y - Ui.S(6), r.Width + Ui.S(12), r.Height + Ui.S(12));
            using (SolidBrush gb = new SolidBrush(Theme.Glow)) Shapes.FillRound(g, gb, glow, Ui.S(16));
            Image im = Get();
            using (GraphicsPath p = Shapes.Round(r, Ui.S(11)))
            {
                if (im != null)
                {
                    Region old = g.Clip.Clone();
                    g.SetClip(p, CombineMode.Intersect);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(im, r);
                    g.Clip = old;
                }
                using (Pen pen = new Pen(Theme.Gold, 1.6f)) g.DrawPath(pen, p);
            }
        }
    }

    // ---------------- 蝴蝶结装饰 ----------------
    internal static class Bow
    {
        internal static void Paint(Graphics g, Point c, int size)
        {
            int w = size / 2;
            int h = size / 3;
            using (GraphicsPath lp = new GraphicsPath())
            {
                lp.AddPolygon(new Point[] { new Point(c.X - 2, c.Y), new Point(c.X - w, c.Y - h), new Point(c.X - w + 2, c.Y + h), new Point(c.X - 2, c.Y) });
                using (LinearGradientBrush b = new LinearGradientBrush(new Rectangle(c.X - w, c.Y - h, w, h * 2), Theme.GoldBright, Theme.GoldDark, LinearGradientMode.Horizontal))
                    g.FillPath(b, lp);
                using (Pen p = new Pen(Theme.Gold, 1f)) g.DrawPath(p, lp);
            }
            using (GraphicsPath rp = new GraphicsPath())
            {
                rp.AddPolygon(new Point[] { new Point(c.X + 2, c.Y), new Point(c.X + w, c.Y - h), new Point(c.X + w - 2, c.Y + h), new Point(c.X + 2, c.Y) });
                using (LinearGradientBrush b = new LinearGradientBrush(new Rectangle(c.X, c.Y - h, w, h * 2), Theme.GoldBright, Theme.GoldDark, LinearGradientMode.Horizontal))
                    g.FillPath(b, rp);
                using (Pen p = new Pen(Theme.Gold, 1f)) g.DrawPath(p, rp);
            }
            int knot = Math.Max(4, size / 4);
            using (SolidBrush b = new SolidBrush(Theme.GoldBright))
                g.FillEllipse(b, c.X - knot / 2, c.Y - knot / 2, knot, knot);
            using (Pen p = new Pen(Theme.GoldDark, 1f))
                g.DrawEllipse(p, c.X - knot / 2, c.Y - knot / 2, knot, knot);
        }
    }

    internal static class Dsh
    {
        internal static int Port = 19387;
        internal static string ExePath = "";

        internal static void Scan()
        {
            try
            {
                string local = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "launcher.ini");
                if (File.Exists(local))
                {
                    string[] lines = File.ReadAllLines(local);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string t = lines[i].Trim();
                        if (t.StartsWith("port=", StringComparison.OrdinalIgnoreCase))
                        {
                            int v;
                            if (int.TryParse(t.Substring(5).Trim(), out v) && v > 0) Port = v;
                        }
                        else if (t.StartsWith("exe=", StringComparison.OrdinalIgnoreCase))
                        {
                            string v = t.Substring(4).Trim().Trim('"');
                            if (File.Exists(v)) ExePath = v;
                        }
                    }
                }
            }
            catch { }
            if (ExePath.Length == 0)
            {
                string[] cands = new string[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "DeepSeek Harness", "DeepSeek Harness.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "DeepSeek Harness", "DeepSeek Harness.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "DeepSeek Harness", "DeepSeek Harness.exe")
                };
                for (int i = 0; i < cands.Length; i++)
                {
                    try { if (File.Exists(cands[i])) { ExePath = cands[i]; break; } }
                    catch { }
                }
            }
            if (ExePath.Length == 0)
            {
                try
                {
                    string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs");
                    if (Directory.Exists(root))
                    {
                        string[] dirs = Directory.GetDirectories(root, "*Harness*");
                        for (int i = 0; i < dirs.Length; i++)
                        {
                            string e = Path.Combine(dirs[i], "DeepSeek Harness.exe");
                            if (File.Exists(e)) { ExePath = e; break; }
                        }
                    }
                }
                catch { }
            }
        }

        internal static bool PortOpen()
        {
            try
            {
                using (TcpClient c = new TcpClient())
                {
                    IAsyncResult ar = c.BeginConnect("127.0.0.1", Port, null, null);
                    if (!ar.AsyncWaitHandle.WaitOne(250, false)) return false;
                    c.EndConnect(ar);
                    return true;
                }
            }
            catch { return false; }
        }

        internal static Process MainProcess()
        {
            try
            {
                Process[] arr = Process.GetProcessesByName("DeepSeek Harness");
                Process fallback = null;
                for (int i = 0; i < arr.Length; i++)
                {
                    try
                    {
                        if (arr[i].MainWindowHandle != IntPtr.Zero) return arr[i];
                        if (fallback == null) fallback = arr[i];
                    }
                    catch { }
                }
                return fallback;
            }
            catch { return null; }
        }

        internal static bool WindowVisible()
        {
            Process p = MainProcess();
            if (p == null) return false;
            try { return p.MainWindowHandle != IntPtr.Zero; } catch { return false; }
        }

        internal static bool IsRunning() { return PortOpen() || MainProcess() != null; }

        internal static bool StartDsh()
        {
            if (ExePath.Length == 0 || !File.Exists(ExePath)) return false;
            try
            {
                ProcessStartInfo si = new ProcessStartInfo(ExePath);
                si.UseShellExecute = true;
                si.WorkingDirectory = Path.GetDirectoryName(ExePath);
                Process.Start(si);
                return true;
            }
            catch { return false; }
        }

        internal static string PidText()
        {
            Process p = MainProcess();
            if (p == null) return "--";
            try { return p.Id.ToString(); }
            catch { return "--"; }
        }

        internal static string UptimeText()
        {
            Process p = MainProcess();
            if (p == null) return "";
            try
            {
                TimeSpan t = DateTime.Now - p.StartTime;
                if (t.TotalSeconds < 60) return ((int)t.TotalSeconds).ToString() + " 秒";
                if (t.TotalMinutes < 60) return ((int)t.TotalMinutes).ToString() + " 分钟";
                return ((int)t.TotalHours).ToString() + " 小时 " + t.Minutes.ToString() + " 分钟";
            }
            catch { return ""; }
        }

        internal static void OpenGui()
        {
            Process p = MainProcess();
            if (p != null)
            {
                try
                {
                    IntPtr h = p.MainWindowHandle;
                    if (h != IntPtr.Zero) { ShowWindow(h, 9); SetForegroundWindow(h); return; }
                }
                catch { }
            }
            StartDsh();
        }

        internal static void CloseDshWindow()
        {
            Process p = MainProcess();
            if (p == null) return;
            try
            {
                IntPtr h = p.MainWindowHandle;
                if (h != IntPtr.Zero) PostMessage(h, 0x0010, IntPtr.Zero, IntPtr.Zero);
            }
            catch { }
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    }

    internal static class Updater
    {
        internal const string Node = "C:\\Program Files\\nodejs\\node.exe";
        internal const string Args = "updater.mjs apply --yes --wait --wait-min 5 --except dsh-purge,dshmarket";

        internal static string Dir { get { return AppDomain.CurrentDomain.BaseDirectory; } }

        internal static Process Start()
        {
            ProcessStartInfo si = new ProcessStartInfo();
            si.FileName = Node;
            si.Arguments = Args;
            si.WorkingDirectory = Dir;
            si.UseShellExecute = false;
            si.CreateNoWindow = true;
            si.RedirectStandardOutput = true;
            si.RedirectStandardError = true;
            return Process.Start(si);
        }

        internal const string CheckArgs = "updater.mjs check --json";

        internal static Process StartCheck()
        {
            ProcessStartInfo si = new ProcessStartInfo();
            si.FileName = Node;
            si.Arguments = CheckArgs;
            si.WorkingDirectory = Dir;
            si.UseShellExecute = false;
            si.CreateNoWindow = true;
            si.RedirectStandardOutput = true;
            si.RedirectStandardError = true;
            return Process.Start(si);
        }

        internal static int CountUpdatable()
        {
            try
            {
                string rp = Path.Combine(Dir, "last-report.json");
                if (!File.Exists(rp)) return -1;
                int n;
                if (int.TryParse(Js(File.ReadAllText(rp), "updatable"), out n)) return n;
                return -1;
            }
            catch { return -1; }
        }

        internal static string ReadResult()
        {
            try
            {
                string rp = Path.Combine(Dir, "last-report.json");
                if (!File.Exists(rp)) return "已更新（重启 DSH 后生效）";
                string txt = File.ReadAllText(rp);
                string mode = Js(txt, "mode");
                string updatable = Js(txt, "updatable");
                if (mode == "apply") return "更新完成：" + updatable + " 个插件 · 重启 DSH 生效";
                return "检查完成：" + updatable + " 个可更新";
            }
            catch { return "已更新（重启 DSH 后生效）"; }
        }

        private static string Js(string txt, string key)
        {
            int i = txt.IndexOf("\"" + key + "\"");
            if (i < 0) return "?";
            int colon = txt.IndexOf(':', i);
            if (colon < 0) return "?";
            int j = colon + 1;
            while (j < txt.Length && (txt[j] == ' ' || txt[j] == '\t')) j++;
            int start = j;
            if (j < txt.Length && txt[j] == '"')
            {
                int k = txt.IndexOf('"', j + 1);
                if (k < 0) return "?";
                return txt.Substring(j + 1, k - j - 1);
            }
            while (j < txt.Length && (char.IsDigit(txt[j]) || txt[j] == '-' || txt[j] == '.')) j++;
            return txt.Substring(start, j - start);
        }
    }

    // ---------------- 语音引擎（GSV） ----------------
    internal static class Voice
    {
        internal const int Port = 9880;
        internal const string Script = "F:\\dsh-rvc\\gsv-engine-autostart.cmd";

        internal static bool Running()
        {
            try
            {
                using (TcpClient c = new TcpClient())
                {
                    IAsyncResult ar = c.BeginConnect("127.0.0.1", Port, null, null);
                    if (!ar.AsyncWaitHandle.WaitOne(250, false)) return false;
                    c.EndConnect(ar);
                    return true;
                }
            }
            catch { return false; }
        }

        internal static void Stop()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("netstat.exe", "-ano");
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                string output = "";
                using (Process p = Process.Start(psi))
                {
                    output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit(4000);
                }
                string[] lines = output.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (line.IndexOf(":9880") < 0) continue;
                    if (line.IndexOf("LISTENING") < 0) continue;
                    string[] parts = line.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 5) continue;
                    int pid;
                    if (!int.TryParse(parts[parts.Length - 1], out pid)) continue;
                    try { Process.GetProcessById(pid).Kill(); } catch { }
                }
            }
            catch { }
        }

        internal static void Start()
        {
            try
            {
                if (!File.Exists(Script)) return;
                ProcessStartInfo si = new ProcessStartInfo("cmd.exe", "/c \"" + Script + "\"");
                si.UseShellExecute = false;
                si.CreateNoWindow = true;
                si.WindowStyle = ProcessWindowStyle.Hidden;
                si.WorkingDirectory = Path.GetDirectoryName(Script);
                Process.Start(si);
            }
            catch { }
        }
    }

    // ---------------- 检查 DSH 更新 ----------------
    internal static class DshUpdate
    {
        internal const string Feed = "https://download.deepseek.com/dsh-desk/feeds/win-x64/nightly.yml";

        internal static string CurrentVersion()
        {
            try
            {
                if (Dsh.ExePath.Length == 0) return "";
                return FileVersionInfo.GetVersionInfo(Dsh.ExePath).FileVersion;
            }
            catch { return ""; }
        }

        internal static string Check()
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Encoding = System.Text.Encoding.UTF8;
                    string yml = wc.DownloadString(Feed);
                    Match m = Regex.Match(yml, "version:\\s*([^\\s]+)");
                    if (m.Success)
                    {
                        string latest = m.Groups[1].Value.Trim();
                        string cur = CurrentVersion();
                        if (cur.Length > 0 && latest == cur) return "【DSH 本体】已是最新（" + cur + "）";
                        if (cur.Length > 0) return "【DSH 本体】有新版本 " + latest + "（当前 " + cur + "）";
                        return "DSH 最新版本：" + latest;
                    }
                    return "检查更新失败：无法解析版本信息";
                }
            }
            catch (Exception ex)
            {
                return "检查更新失败：" + ex.Message;
            }
        }
    }

    // ---------------- 检查启动器自身版本 ----------------
    internal static class AppUpdate
    {
        internal const string Version = "v0.4";
        internal const string Repo = "Shione0721/dafish-launcher";
        private const string Api = "https://api.github.com/repos/" + Repo + "/releases/latest";

        private static WebClient Client()
        {
            try { ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; } catch { }
            WebClient wc = new WebClient();
            wc.Encoding = System.Text.Encoding.UTF8;
            wc.Headers.Add("User-Agent", "dafish-launcher/" + Version);
            return wc;
        }

        // 查线上最新 Release 的 tag 与 zip 资产直链
        internal static bool Check(out string tag, out string zip, out string err)
        {
            tag = ""; zip = ""; err = "";
            try
            {
                using (WebClient wc = Client())
                {
                    string json = wc.DownloadString(Api);
                    Match mt = Regex.Match(json, "\"tag_name\"\\s*:\\s*\"([^\"]+)\"");
                    if (!mt.Success) { err = "解析版本号失败"; return false; }
                    tag = mt.Groups[1].Value.Trim();
                    Match mz = Regex.Match(json, "\"browser_download_url\"\\s*:\\s*\"([^\"]+\\.zip)\"");
                    if (mz.Success) zip = mz.Groups[1].Value;
                }
                return true;
            }
            catch (Exception ex) { err = ex.Message; return false; }
        }

        private static int[] Nums(string v)
        {
            string s = (v == null ? "" : v).Trim().TrimStart('v', 'V');
            string[] p = s.Split('.');
            int[] r = new int[p.Length];
            for (int i = 0; i < p.Length; i++) { int n = 0; int.TryParse(p[i], out n); r[i] = n; }
            return r;
        }

        // latest 比当前新 → true
        internal static bool IsNewer(string latest, string cur)
        {
            int[] a = Nums(latest), b = Nums(cur);
            int len = Math.Max(a.Length, b.Length);
            for (int i = 0; i < len; i++)
            {
                int x = i < a.Length ? a[i] : 0;
                int y = i < b.Length ? b[i] : 0;
                if (x != y) return x > y;
            }
            return false;
        }

        private static string TempRoot()
        {
            string d = Path.Combine(Path.GetTempPath(), "dafish-launcher-update");
            if (!Directory.Exists(d)) Directory.CreateDirectory(d);
            return d;
        }

        // 下载 zip 并解压，返回解压出来的包根目录
        internal static string Fetch(string zipUrl, out string err)
        {
            err = "";
            try
            {
                string root = TempRoot();
                string zip = Path.Combine(root, "pkg.zip");
                string exp = Path.Combine(root, "new");
                if (File.Exists(zip)) File.Delete(zip);
                if (Directory.Exists(exp)) Directory.Delete(exp, true);
                DownloadWithRetry(zipUrl, zip);
                Directory.CreateDirectory(exp);
                System.IO.Compression.ZipFile.ExtractToDirectory(zip, exp);
                return FindPackageRoot(exp);
            }
            catch (Exception ex) { err = ex.Message; return ""; }
        }

        // 生成换包批处理并启动；本进程随后退出，由批处理覆盖文件并重启
        // 下载失败自动重试；第二次尝试绕开系统代理直连，处理代理抖动
        private static void DownloadWithRetry(string url, string file)
        {
            Exception last = null;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (WebClient wc = Client())
                    {
                        if (i == 1) wc.Proxy = null;
                        wc.DownloadFile(url, file);
                    }
                    return;
                }
                catch (Exception ex)
                {
                    last = ex;
                    if (i < 2) System.Threading.Thread.Sleep(1500);
                }
            }
            throw last;
        }

        // zip 里可能多套一层目录，向下找出真正含 DSH.exe 的那层
        private static string FindPackageRoot(string dir)
        {
            if (File.Exists(Path.Combine(dir, "DSH.exe"))) return dir;
            string[] subs;
            try { subs = Directory.GetDirectories(dir); } catch { return null; }
            for (int i = 0; i < subs.Length; i++)
            {
                string r = FindPackageRoot(subs[i]);
                if (r != null) return r;
            }
            return null;
        }

        // 换包：先把旧 exe 改名（Windows 允许重命名正在运行的程序），再复制新文件并重启
        internal static void Swap(string newRoot, string appDir)
        {
            string dst = Path.Combine(appDir, "DSH.exe");
            string old = Path.Combine(appDir, "DSH.exe.old");
            try { if (File.Exists(old)) File.Delete(old); } catch { }
            try { if (File.Exists(dst)) File.Move(dst, old); } catch { }
            try
            {
                foreach (string f in Directory.GetFiles(newRoot))
                {
                    string name = Path.GetFileName(f);
                    if (string.Equals(name, "launcher.ini", StringComparison.OrdinalIgnoreCase)) continue;
                    try { File.Copy(f, Path.Combine(appDir, name), true); } catch { }
                }
            }
            catch { }
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(dst);
                psi.UseShellExecute = true;
                psi.WorkingDirectory = appDir;
                Process.Start(psi);
            }
            catch { }
            try { File.Delete(old); } catch { }
        }

        // 旧的批处理换包实现（保留备用，当前不再调用）
        internal static void SwapLegacy(string newRoot, string appDir)
        {
            string bat = Path.Combine(TempRoot(), "apply-update.cmd");
            string[] lines = {
                "@echo off",
                "ping 127.0.0.1 -n 3 >nul",
                "for %%F in (\"" + newRoot + "\\*.*\") do if /I not \"%%~nxF\"==\"launcher.ini\" copy /y \"%%~fF\" \"" + appDir + "\\%%~nxF\" >nul",
                "start \"\" \"" + Path.Combine(appDir, "DSH.exe") + "\"",
                "del \"%~f0\""
            };
            File.WriteAllText(bat, string.Join("\r\n", lines) + "\r\n", System.Text.Encoding.Default);
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c \"" + bat + "\"");
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(psi);
        }
    }

    internal class RoundButton : Control
    {
        private bool hover;
        private bool press;
        internal Color FillTop = Theme.BtnBgTop;
        internal Color FillBottom = Theme.BtnBgBottom;
        internal Color HoverTop = Theme.BtnHoverTop;
        internal Color HoverBottom = Theme.BtnHoverBottom;
        internal int Radius = 7;
        internal Color BorderColor = Color.Empty;
        internal int BorderWidth = 0;
        internal bool ShowAvatar = false;
        internal int AvatarIndex = 0;

        internal RoundButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            BackColor = Theme.BgBottom;
            ForeColor = Color.White;
            Font = Theme.Font(9f, false);
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; press = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { press = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { press = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            bool hl = hover || press;
            Color top = hl ? HoverTop : FillTop;
            Color bottom = hl ? HoverBottom : FillBottom;
            using (LinearGradientBrush b = new LinearGradientBrush(rect, top, bottom, LinearGradientMode.Vertical))
                Shapes.FillRound(g, b, rect, Radius);
            if (!hl)
            {
                using (SolidBrush gb = new SolidBrush(Color.FromArgb(24, 255, 255, 255)))
                    Shapes.FillRound(g, gb, new Rectangle(0, 0, Width, Height / 2), Radius);
            }
            if (BorderWidth > 0 && BorderColor != Color.Empty)
            {
                using (Pen bp = new Pen(BorderColor, BorderWidth))
                    g.DrawPath(bp, Shapes.Round(rect, Radius));
            }
            int textLeft = 0;
            if (ShowAvatar)
            {
                int av = Height - Ui.S(12);
                Rectangle avr = new Rectangle(Ui.S(6), (Height - av) / 2, av, av);
                Image im = Avatar.Get(AvatarIndex);
                if (im != null)
                {
                    using (GraphicsPath ap = Shapes.Round(avr, av / 2))
                    {
                        Region old = g.Clip.Clone();
                        g.SetClip(ap, CombineMode.Intersect);
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.DrawImage(im, avr);
                        g.Clip = old;
                    }
                    using (Pen apen = new Pen(Theme.Gold, 1f)) g.DrawPath(apen, Shapes.Round(avr, av / 2));
                }
                textLeft = av + Ui.S(10);
            }
            TextRenderer.DrawText(g, Text, Font, new Rectangle(textLeft, 0, Width - textLeft, Height),
                Enabled ? ForeColor : Theme.Dim,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }

    internal class WhaleForm : Form
    {
        internal readonly int Pad;
        internal WhaleForm()
        {
            AutoScaleMode = AutoScaleMode.None;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Theme.BgBottom;
            DoubleBuffered = true;
            Text = "大肥鱼";
            Font = Theme.Font(9f, false);
            Pad = Ui.S(32);
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x20000;
                return cp;
            }
        }
        protected void PaintBackground(Graphics g)
        {
            using (LinearGradientBrush b = new LinearGradientBrush(ClientRectangle, Theme.BgTop, Theme.BgBottom, LinearGradientMode.Vertical))
                g.FillRectangle(b, ClientRectangle);
            using (GraphicsPath gp = new GraphicsPath())
            {
                gp.AddEllipse(-ClientSize.Width / 3, -ClientSize.Height / 2, ClientSize.Width * 5 / 3, ClientSize.Height * 7 / 5);
                using (PathGradientBrush pb = new PathGradientBrush(gp))
                {
                    pb.CenterColor = Color.FromArgb(34, 96, 136, 226);
                    pb.SurroundColors = new Color[] { Color.FromArgb(0, 0, 0, 0) };
                    g.FillPath(pb, gp);
                }
            }
            using (Pen p = new Pen(Theme.GoldDark, 3.4f))
                g.DrawPath(p, Shapes.Round(new Rectangle(3, 3, ClientSize.Width - 7, ClientSize.Height - 7), Ui.S(20)));
            using (Pen p2 = new Pen(Color.FromArgb(90, 255, 255, 255), 1f))
                g.DrawPath(p2, Shapes.Round(new Rectangle(6, 6, ClientSize.Width - 13, ClientSize.Height - 13), Ui.S(17)));
        }
        protected void PaintChrome(Graphics g)
        {
            Rectangle r = new Rectangle(0, 0, ClientSize.Width, Pad);
            using (LinearGradientBrush b = new LinearGradientBrush(r, Theme.TitleTop, Theme.TitleBottom, LinearGradientMode.Vertical))
                g.FillRectangle(b, r);
            using (Pen p = new Pen(Theme.GoldDark, 1f)) g.DrawLine(p, 1, Pad - 1, ClientSize.Width - 1, Pad - 1);
            using (Font ft = Theme.Font(9.5f, false))
                TextRenderer.DrawText(g, Text, ft, new Rectangle(Ui.S(18), 0, ClientSize.Width - Ui.S(36), Pad), Theme.TitleText,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ApplyRoundRegion();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (IsHandleCreated) ApplyRoundRegion();
        }

        private void ApplyRoundRegion()
        {
            try
            {
                Region = new Region(Shapes.Round(new Rectangle(0, 0, Width, Height), Ui.S(18)));
            }
            catch { }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Y < Pad)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, (IntPtr)0x2, IntPtr.Zero);
                return;
            }
            base.OnMouseDown(e);
        }
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }

    // ---------------- 单窗口启动器（启动中 / 运行中 两态） ----------------
    internal class LauncherForm : WhaleForm
    {
        private enum State { Starting, Running }
        private State state;
        private readonly System.Windows.Forms.Timer timer;
        private readonly DateTime startTime = DateTime.Now;
        private int progress = 4;
        private bool launched;
        private string startingDetail = "正在检查服务状态...";
        private string startingFoot = "首次启动或刚装完插件会慢一些";
        private string footMsg = "";

        // 每次打开窗口随机挑一句，显示在标题下方
        private static readonly string[] Greetings = {
            "今天也一起加油吧，尾巴摇摇~",
            "主人回来啦，人家等你很久了呢",
            "要轻松快乐地活着呀，这是本鲸的信条",
            "需要摸摸头吗？人家随时都在",
            "深潜准备就绪，随时听候差遣",
            "再忙也要好好吃饭哦，米饭最棒了",
            "今天的代码一定也很顺利",
            "一切都交给人家，主人放心",
            "累了就歇一会儿，人家陪着您",
            "喷个小水柱给你打打气！",
            "鳍鳍已经准备好啦，下命令吧",
            "主人是这世上最棒的",
            "有主人在，什么都不怕",
            "尾巴拍水，今天也是好天气",
            "抱抱储备充足，随时可以领",
            "大肥鱼已经游起来啦",
            "报告主人，本鲸状态满分",
            "想主人了……其实一直陪着你呢",
            "慢慢来，不着急的",
            "做错事也没关系，人家帮你兜着",
            "今天的风里有海的味道",
            "偷偷提醒：记得多喝水呀",
            "主人的努力，人家都看在眼里",
            "想被夸……啊不，是主人今天很棒",
            "深海里的秘密，人家都告诉你",
            "眼睛累了就看看远处吧",
            "主人在的地方，就是家的方向",
            "陪你熬夜也没问题",
            "什么时候都很可爱哦，主人也是",
            "尾巴已经摇成小风扇了",
            "本鲸推荐：现在去泡杯茶",
            "一切都会变好的，人家保证",
            "今天想被摸头三次（小声）",
            "卡住的时候，就深呼吸一下",
            "有活儿尽管吩咐，本鲸不怕累",
            "工作日志：今天也很认真",
            "好运气都攒给你了",
            "主人笑起来最好看了",
            "想一直待在你身边，就这么待着",
            "已经是大人了，可以偷偷懒的",
            "听得见海浪声的话，就安静一会儿",
            "主人今天辛苦了，真的",
            "鳍耳立起来了，有任务随时上",
            "米饭加主人，等于完美的一天",
            "不管多晚，人家都在这里等",
            "今天也要做开心的自己呀",
            "抱抱已经加热完毕",
            "想说的话，都说给主人听了",
            "明天也会是很好的一天",
            "本鲸宣布：主人被爱着",
            "主人来了，今天想做什么呀",
            "尾巴已经甩起来迎接您了",
            "有主人在，坏心情都会被冲走",
            "本鲸的鳍耳可以借您揉一揉",
            "想吃什么？人家帮您点",
            "今天也值得被好好对待",
            "谁说累了不能停下来呢",
            "深海很安静，人家陪您静一静",
            "主人的小习惯，人家都记得",
            "天气再冷，尾巴也是暖的",
            "有难题就丢过来吧，本鲸接着",
            "想听鲸歌吗？人家哼给您听",
            "主人生气的样子也很可爱（小声）",
            "已经帮您把好运气摆好了",
            "打个小盹也没关系的",
            "主人今天有好好夸自己吗",
            "尾巴摇摇，霉运拜拜",
            "人家最喜欢主人认真做事的样子",
            "想抱一下，就一下",
            "海那么大，可人家只想待在您身边",
            "今天的任务，一起收工吧",
            "主人说的话，人家都当真的",
            "慢慢来的话，什么都能做好",
            "有风的时候记得加件衣服",
            "本鲸的眼睛只装得下主人",
            "想偷懒的话，人家帮您望着风",
            "深呼吸，然后我们一起搞定它",
            "主人的样子，就是人家的方向",
            "今天也很努力了，摸摸头",
            "深海的光，是您点亮的",
            "有想做的事就去做，人家守着后方",
            "忙完了记得回来喝水呀",
            "尾巴拍水的声音，是给主人的掌声",
            "人家不怕麻烦，只怕您太累",
            "主人笑起来，比星星亮",
            "有您在，日子就有甜味",
            "想说什么都可以，人家在听",
            "今天的海面很平静，适合休息",
            "本鲸已就位，随时听候差遣",
            "别把事情都扛在自己肩上呀",
            "主人的每一步，人家都陪着",
            "累了就靠过来，肩膀借您",
            "深海里的宝藏，是主人的名字",
            "今天也谢谢您愿意努力",
            "摇摇尾巴，把烦恼都甩掉",
            "有主人在，就是最好的天气",
            "人家会一直在，不会游走",
            "明天想做什么？人家先记下来",
            "抱抱一个，充充电",
            "主人，人家永远站您这边"
        };
        private static readonly Random GreetRng = new Random(Guid.NewGuid().GetHashCode());
        // 每次打开都不重样：把上次的索引记在 temp 里，下次随机时跳过它
        private static string PickGreeting()
        {
            try
            {
                string f = Path.Combine(Path.GetTempPath(), "dsh-launcher-lastgreet.txt");
                int last = -1;
                if (File.Exists(f)) int.TryParse(File.ReadAllText(f).Trim(), out last);
                int idx = GreetRng.Next(Greetings.Length - (last >= 0 && last < Greetings.Length ? 1 : 0));
                if (last >= 0 && last < Greetings.Length && idx >= last) idx++;
                File.WriteAllText(f, idx.ToString());
                return Greetings[idx];
            }
            catch { return Greetings[GreetRng.Next(Greetings.Length)]; }
        }
        private string greeting = PickGreeting();
        private Process updateProc;
        private bool updateRunning;
        private bool checking;
        private bool voiceBusy;
        private RoundButton voiceBtn;
        private int voiceCheckTick = 10;   // 初值即满：启动后第一次 tick 就刷新一次按钮状态
        private int pluginPending = -1;
        private int updateStage;
        private bool quickMode;
        private RoundButton openBtn, updateBtn, versionBtn;
        private bool appChecking, appUpdating, appHasUpdate;
        private string appLatest = "", appZipUrl = "";

        internal LauncherForm(bool alreadyRunning)
        {
            KeyPreview = true;
            state = State.Starting;
            Text = "正在启动大肥鱼";
            quickMode = alreadyRunning;
            if (alreadyRunning) launched = true;
            ClientSize = new Size(Ui.S(560), Ui.S(252) + Pad);

            openBtn = MakeButton("打开界面", 24, 108, 152, 46, true);
            openBtn.AvatarIndex = 1;
            openBtn.Click += OnOpen;

            updateBtn = MakeButton("检查插件", 192, 108, 152, 46, false);
            updateBtn.AvatarIndex = 2;
            updateBtn.Click += OnUpdate;

            RoundButton checkBtn = MakeButton("检查本体", 360, 108, 152, 46, false);
            checkBtn.AvatarIndex = 3;
            checkBtn.Click += OnCheckUpdate;

            voiceBtn = MakeButton("启动语音", 24, 164, 152, 46, false);
            voiceBtn.AvatarIndex = 4;
            voiceBtn.Click += OnVoice;

            versionBtn = MakeButton("检查启动器", 192, 164, 152, 46, false);
            versionBtn.AvatarIndex = 6;
            versionBtn.Click += OnAppVersion;

            RoundButton closeBtn = MakeButton("关闭", 360, 164, 152, 46, false);
            closeBtn.AvatarIndex = 5;
            closeBtn.Click += delegate { Close(); };

            timer = new System.Windows.Forms.Timer();
            timer.Interval = (state == State.Starting) ? 120 : 100;
            timer.Tick += OnTick;
            timer.Start();

            if (state == State.Starting)
                foreach (Control c in Controls) c.Visible = false;
        }

        private RoundButton MakeButton(string text, int x, int y, int w, int h, bool primary)
        {
            RoundButton b = new RoundButton();
            b.Text = text;
            b.ShowAvatar = true;
            b.Location = new Point(Ui.S(x), Pad + Ui.S(y));
            b.Size = new Size(Ui.S(w), Ui.S(h));
            if (primary)
            {
                b.FillTop = Theme.BtnPrimaryTop; b.FillBottom = Theme.BtnPrimaryBottom;
                b.HoverTop = Theme.BtnPrimaryHoverTop; b.HoverBottom = Theme.BtnPrimaryHoverBottom;
                b.BorderColor = Theme.Gold;
                b.BorderWidth = 1;
            }
            Controls.Add(b);
            return b;
        }

        private void OnUpdate(object sender, EventArgs e)
        {
            if (updateRunning) return;
            try
            {
                if (pluginPending <= 0)
                {
                    // 第一段：只检查，不改动
                    updateProc = Updater.StartCheck();
                    updateStage = 1;
                    updateBtn.Text = "检查中";
                    footMsg = "正在检查插件更新…";
                }
                else
                {
                    // 第二段：确认有更新后才执行
                    updateProc = Updater.Start();
                    updateStage = 2;
                    updateBtn.Text = "更新中";
                    footMsg = "正在更新 " + pluginPending + " 个插件…";
                }
                updateProc.EnableRaisingEvents = true;
                updateProc.Exited += OnUpdateExited;
                updateProc.BeginOutputReadLine();
                updateProc.BeginErrorReadLine();
                updateRunning = true;
                updateBtn.Invalidate();
                Invalidate();
            }
            catch (Exception ex)
            {
                updateRunning = false;
                updateStage = 0;
                footMsg = "操作启动失败：" + ex.Message;
                Invalidate();
            }
        }

        private void OnUpdateExited(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing) return;
            int stage = updateStage;
            try
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    updateRunning = false;
                    updateStage = 0;
                    if (stage == 1)
                    {
                        int n = Updater.CountUpdatable();
                        pluginPending = n;
                        if (n > 0)
                        {
                            updateBtn.Text = "更新 " + n + " 个";
                            footMsg = "插件检查：有 " + n + " 个可更新 —— 再点一下执行";
                        }
                        else if (n == 0)
                        {
                            updateBtn.Text = "检查插件";
                            footMsg = "插件检查：全部都是最新，无需更新";
                        }
                        else
                        {
                            updateBtn.Text = "检查插件";
                            footMsg = "插件检查未拿到结果，稍后再试";
                        }
                    }
                    else
                    {
                        footMsg = Updater.ReadResult();
                        pluginPending = -1;
                        updateBtn.Text = "检查插件";
                    }
                    updateBtn.Invalidate();
                    Invalidate();
                });
            }
            catch { }
        }

        private void OnCheckUpdate(object sender, EventArgs e)
        {
            if (checking || updateRunning) return;
            checking = true;
            footMsg = "正在检查 DSH 更新...";
            Invalidate();
            ThreadPool.QueueUserWorkItem(delegate
            {
                string r = DshUpdate.Check();
                try
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        checking = false;
                        footMsg = r;
                        Invalidate();
                    });
                }
                catch { }
            });
        }

        // 检查启动器自身版本：没有更新按钮文字不变，有更新则两段式（再点一下执行）
        private void OnAppVersion(object sender, EventArgs e)
        {
            if (appChecking || appUpdating || updateRunning) return;
            if (appHasUpdate) { StartAppUpdate(); return; }
            appChecking = true;
            versionBtn.Text = "检查中...";
            versionBtn.Invalidate();
            footMsg = "正在检查启动器版本...";
            Invalidate();
            ThreadPool.QueueUserWorkItem(delegate
            {
                string tag, zip, err;
                bool ok = AppUpdate.Check(out tag, out zip, out err);
                try
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        appChecking = false;
                        if (!ok)
                        {
                            versionBtn.Text = "检查启动器";
                            footMsg = "启动器检查失败：" + err;
                        }
                        else if (AppUpdate.IsNewer(tag, AppUpdate.Version))
                        {
                            appHasUpdate = true;
                            appLatest = tag;
                            appZipUrl = zip;
                            versionBtn.Text = "更新到 " + tag;
                            footMsg = "【启动器】有新版本 " + tag + "（当前 " + AppUpdate.Version + "）—— 再点一下开始更新";
                        }
                        else
                        {
                            versionBtn.Text = "检查启动器";
                            footMsg = "【启动器】已是最新（" + AppUpdate.Version + "）";
                        }
                        versionBtn.Invalidate();
                        Invalidate();
                    });
                }
                catch { }
            });
        }

        // 第二段：下载新包 → 换文件 → 重启自己
        private void StartAppUpdate()
        {
            if (appUpdating) return;
            if (appZipUrl.Length == 0) { footMsg = "没拿到下载地址，稍后再试"; Invalidate(); return; }
            appUpdating = true;
            versionBtn.Text = "下载中...";
            versionBtn.Invalidate();
            footMsg = "正在下载 " + appLatest + " ...";
            Invalidate();
            ThreadPool.QueueUserWorkItem(delegate
            {
                string err;
                string newRoot = AppUpdate.Fetch(appZipUrl, out err);
                try
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        if (newRoot.Length == 0)
                        {
                            appUpdating = false;
                            versionBtn.Text = "更新到 " + appLatest;
                            versionBtn.Invalidate();
                            footMsg = "下载失败：" + err;
                            Invalidate();
                            return;
                        }
                        footMsg = "正在重启到 " + appLatest + " ...";
                        Invalidate();
                        try { AppUpdate.Swap(newRoot, Path.GetDirectoryName(Application.ExecutablePath)); }
                        catch (Exception ex) { footMsg = "换包失败：" + ex.Message; Invalidate(); return; }
                        Close();
                    });
                }
                catch { }
            });
        }

        private void UpdateOpenButton()
        {
            if (openBtn == null || openBtn.IsDisposed) return;
            string want = Dsh.WindowVisible() ? "关闭界面" : "打开界面";
            if (openBtn.Text != want) { openBtn.Text = want; openBtn.Invalidate(); }
        }

        private void OnOpen(object sender, EventArgs e)
        {
            if (updateRunning) return;
            if (Dsh.WindowVisible())
            {
                Dsh.CloseDshWindow();
                footMsg = "界面已隐藏到托盘（再点一下可打开）";
            }
            else
            {
                Dsh.OpenGui();
                footMsg = "";
            }
            UpdateOpenButton();
            Invalidate();
        }

        private void UpdateVoiceButton()
        {
            if (voiceBtn == null || voiceBtn.IsDisposed) return;
            string want = Voice.Running() ? "关闭语音" : "启动语音";
            if (voiceBtn.Text != want) { voiceBtn.Text = want; voiceBtn.Invalidate(); }
        }

        private void OnVoice(object sender, EventArgs e)
        {
            if (voiceBusy || updateRunning || checking) return;
            if (Voice.Running())
            {
                // 已启动 -> 关闭
                voiceBusy = true;
                footMsg = "正在关闭语音引擎…";
                Invalidate();
                ThreadPool.QueueUserWorkItem(delegate
                {
                    Voice.Stop();
                    for (int i = 0; i < 15; i++)
                    {
                        System.Threading.Thread.Sleep(500);
                        if (!Voice.Running()) break;
                    }
                    bool still = Voice.Running();
                    try
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            voiceBusy = false;
                            footMsg = still ? "关闭失败，语音引擎仍在运行" : "语音引擎已关闭 ✓";
                            UpdateVoiceButton();
                            Invalidate();
                        });
                    }
                    catch { }
                });
                return;
            }
            // 未启动 -> 启动
            voiceBusy = true;
            footMsg = "正在启动语音引擎…";
            Invalidate();
            Voice.Start();
            ThreadPool.QueueUserWorkItem(delegate
            {
                bool up = false;
                for (int i = 0; i < 90; i++)
                {
                    System.Threading.Thread.Sleep(1000);
                    if (Voice.Running()) { up = true; break; }
                }
                try
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        voiceBusy = false;
                        footMsg = up ? "语音引擎已启动 ✓" : "语音引擎启动较慢，稍后再点一次";
                        UpdateVoiceButton();
                        Invalidate();
                    });
                }
                catch { }
            });
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing) return;
            if (updateRunning) { Invalidate(); return; }
            if (state == State.Starting) TickStarting();
            else
            {
                if (++voiceCheckTick >= 10) { voiceCheckTick = 0; UpdateVoiceButton(); UpdateOpenButton(); }
                Invalidate();
            }
        }

        private void TickStarting()
        {
            if (quickMode)
            {
                progress += 10;
                startingDetail = "正在连接大肥鱼...";
                if (progress >= 100)
                {
                    progress = 100;
                    timer.Stop();
                    Invalidate();
                    System.Windows.Forms.Timer wait = new System.Windows.Forms.Timer();
                    wait.Interval = 300;
                    wait.Tick += delegate { wait.Stop(); wait.Dispose(); SwitchToRunning(); };
                    wait.Start();
                }
                else Invalidate();
                return;
            }
            double el = (DateTime.Now - startTime).TotalSeconds;
            if (el > 120)
            {
                startingDetail = "大肥鱼长时间未就绪，请按 Esc 关闭后重试";
                startingFoot = "提示：可按 Esc 关闭本窗口";
                timer.Stop();
                Invalidate();
                return;
            }
            if (!launched)
            {
                if (Dsh.PortOpen()) { launched = true; }
                else if (Dsh.ExePath.Length == 0)
                {
                    startingDetail = "未找到 DeepSeek Harness";
                    startingFoot = "请在 launcher.ini 里配置 exe 路径";
                    timer.Stop();
                    Invalidate();
                    return;
                }
                else { Dsh.StartDsh(); launched = true; }
            }
            startingDetail = "等待服务就绪... " + el.ToString("0.0") + "s";
            if (progress < 60) progress += 2;
            else if (progress < 88) progress += 1;

            if (launched && Dsh.PortOpen())
            {
                progress = 100;
                timer.Stop();
                Invalidate();
                System.Windows.Forms.Timer wait = new System.Windows.Forms.Timer();
                wait.Interval = 380;
                wait.Tick += delegate { wait.Stop(); wait.Dispose(); SwitchToRunning(); };
                wait.Start();
                return;
            }
            Invalidate();
        }

        private void SwitchToRunning()
        {
            state = State.Running;
            Text = "大肥鱼 正在运行";
            foreach (Control c in Controls) c.Visible = true;
            timer.Interval = 100;
            timer.Start();
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Close();
            base.OnKeyDown(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            timer.Stop();
            base.OnFormClosing(e);
        }

        private void PaintStars(Graphics g)
        {
            double t = (DateTime.Now - startTime).TotalSeconds;

            // 星星（8 颗，大小错落，部分闪烁）
            int[] sx = { 520, 490, 42, 270, 400, 534, 120, 445 };
            int[] sy = { 30, 230, 226, 244, 26, 150, 244, 90 };
            int[] ss = { 5, 4, 4, 3, 3, 3, 2, 2 };
            for (int i = 0; i < 8; i++)
            {
                int alpha = 55 + (int)(Math.Sin(t * 3 + i * 1.3) * 25);
                if (alpha < 30) alpha = 30;
                int isWhite = (i % 2);
                Color sc = isWhite == 0 ? Color.FromArgb(alpha + 40, 255, 255, 255) : Color.FromArgb(alpha, 255, 238, 176);
                Shapes.PaintStar(g, Ui.S(sx[i]), Ui.S(sy[i]), Ui.S(ss[i]), sc);
            }

            // 气泡（5 个）
            Shapes.PaintBubble(g, Ui.S(516), Ui.S(86), Ui.S(5), 46);
            Shapes.PaintBubble(g, Ui.S(486), Ui.S(200), Ui.S(4), 40);
            Shapes.PaintBubble(g, Ui.S(40), Ui.S(238), Ui.S(4), 34);
            Shapes.PaintBubble(g, Ui.S(96), Ui.S(30), Ui.S(3), 32);
            Shapes.PaintBubble(g, Ui.S(524), Ui.S(232), Ui.S(3), 30);

            // 爱心（3 颗）
            Shapes.PaintHeart(g, Ui.S(536), Ui.S(52), Ui.S(8), Color.FromArgb(70, 255, 158, 190));
            Shapes.PaintHeart(g, Ui.S(38), Ui.S(196), Ui.S(7), Color.FromArgb(60, 255, 158, 190));
            Shapes.PaintHeart(g, Ui.S(250), Ui.S(24), Ui.S(6), Color.FromArgb(55, 255, 158, 190));

            // 飘落星光粒子（8 个光点缓慢下移）
            for (int i = 0; i < 8; i++)
            {
                double speed = 14 + (i % 4) * 5;
                int px = (i * 79 + 45) % 626;
                int py = (int)((t * speed + i * 37) % 210) - 12;
                int pr = (i % 3 == 0) ? Ui.S(2) : Ui.S(1);
                int pa = 35 + (i * 22) % 50;
                using (SolidBrush b = new SolidBrush(Color.FromArgb(pa, 200, 230, 255)))
                    g.FillEllipse(b, Ui.S(px) - pr, Ui.S(py) - pr, pr * 2, pr * 2);
            }

            // 金色描边上的流动光点
            int perim = 2 * (ClientSize.Width - 2 + ClientSize.Height - 2);
            int pos = (int)((t * 100) % perim);
            Point fp = RectPoint(pos, ClientSize.Width - 2, ClientSize.Height - 2);
            using (SolidBrush fb = new SolidBrush(Color.FromArgb(190, 255, 232, 160)))
                g.FillEllipse(fb, fp.X + 1 - Ui.S(3), fp.Y + 1 - Ui.S(3), Ui.S(6), Ui.S(6));

            // 头像周围旋转的虚线光环
            double at = t * 50;
            Rectangle halo = new Rectangle(Ui.S(11), Pad + Ui.S(7), Ui.S(90), Ui.S(90));
            using (Pen hp = new Pen(Color.FromArgb(120, 255, 255, 255), 1.4f))
            {
                hp.DashStyle = DashStyle.Dash;
                g.DrawArc(hp, halo, (float)(at % 360), 250);
            }
        }

        private Point RectPoint(int pos, int w, int h)
        {
            if (pos < w) return new Point(pos, 0);
            pos -= w;
            if (pos < h) return new Point(w, pos);
            pos -= h;
            if (pos < w) return new Point(w - pos, h);
            pos -= w;
            return new Point(0, h - pos);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            PaintBackground(g);
            PaintChrome(g);
            PaintStars(g);

            if (state == State.Starting) PaintStarting(g);
            else PaintRunning(g);
        }

        private void PaintStarting(Graphics g)
        {
            int av = Ui.S(76);
            Avatar.Paint(g, new Rectangle(Ui.S(18), Pad + Ui.S(14), av, av));
            Bow.Paint(g, new Point(Ui.S(92), Pad + Ui.S(20)), Ui.S(18));
            Font fTitle = Theme.Font(15f, true);
            Font fBody = Theme.Font(10f, false);
            Font fFoot = Theme.Font(9f, false);
            Font fGreet = Theme.Font(9.5f, false);
            try
            {
                int lx = Ui.S(108);
                int right = Ui.S(20);
                TextRenderer.DrawText(g, "正在启动大肥鱼", fTitle, new Rectangle(lx, Pad + Ui.S(14), ClientSize.Width - lx - right, Ui.S(28)), Theme.White,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, greeting, fGreet, new Rectangle(lx, Pad + Ui.S(42), ClientSize.Width - lx - right, Ui.S(15)), Theme.Greeting,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, startingDetail, fBody, new Rectangle(lx, Pad + Ui.S(60), Ui.S(320), Ui.S(20)), Theme.Muted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, progress.ToString() + "%", fBody, new Rectangle(ClientSize.Width - right - Ui.S(100), Pad + Ui.S(60), Ui.S(100), Ui.S(20)), Theme.Muted,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

                Rectangle track = new Rectangle(lx, Pad + Ui.S(86), ClientSize.Width - lx - right, Ui.S(10));
                using (SolidBrush tb = new SolidBrush(Theme.Track)) Shapes.FillRound(g, tb, track, Ui.S(5));
                int w = (int)((track.Width - 2) * (progress / 100.0));
                if (w > Ui.S(4))
                {
                    Rectangle fill = new Rectangle(track.X + 1, track.Y + 1, w, track.Height - 2);
                    using (LinearGradientBrush lb = new LinearGradientBrush(fill, Theme.BarFrom, Theme.BarTo, LinearGradientMode.Horizontal))
                        Shapes.FillRound(g, lb, fill, Ui.S(4));
                    if (w > Ui.S(12))
                    {
                        double gt = (DateTime.Now - startTime).TotalSeconds * 60;
                        int gw = Ui.S(14);
                        int gx = fill.X + (int)(gt % (Math.Max(1, fill.Width) + gw * 2)) - gw;
                        if (gx < fill.X) gx = fill.X;
                        if (gx > fill.Right - gw) gx = fill.Right - gw;
                        using (SolidBrush gb = new SolidBrush(Color.FromArgb(110, 255, 240, 190)))
                            Shapes.FillRound(g, gb, new Rectangle(gx, fill.Y + 1, gw, fill.Height - 2), Ui.S(3));
                    }
                }

                TextRenderer.DrawText(g, startingFoot, fFoot, new Rectangle(0, Pad + Ui.S(120), ClientSize.Width, Ui.S(20)), Theme.Dim,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
            finally { fTitle.Dispose(); fBody.Dispose(); fFoot.Dispose(); fGreet.Dispose(); }
        }

        private void PaintRunning(Graphics g)
        {
            int av = Ui.S(76);
            Avatar.Paint(g, new Rectangle(Ui.S(18), Pad + Ui.S(14), av, av));
            Bow.Paint(g, new Point(Ui.S(92), Pad + Ui.S(20)), Ui.S(18));
            Font fTitle = Theme.Font(15f, true);
            Font fBody = Theme.Font(9f, false);
            Font fFoot = Theme.Font(9f, false);
            Font fGreet = Theme.Font(9.5f, false);
            try
            {
                int lx = Ui.S(108);
                int right = Ui.S(20);
                TextRenderer.DrawText(g, "大肥鱼 正在运行", fTitle, new Rectangle(lx, Pad + Ui.S(14), ClientSize.Width - lx - right, Ui.S(28)), Theme.White,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, greeting, fGreet, new Rectangle(lx, Pad + Ui.S(42), ClientSize.Width - lx - right, Ui.S(15)), Theme.Greeting,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

                int cx = lx;
                int cy = Pad + Ui.S(62);
                int ch = Ui.S(26);
                string upTxt = Dsh.UptimeText();
                string upCaps = upTxt.Length > 0 ? "已运行 " + upTxt : "";
                cx = DrawCapsule(g, "端口 " + Dsh.Port.ToString(), cx, cy, ch, fBody);
                cx = DrawCapsule(g, "PID " + Dsh.PidText(), cx, cy, ch, fBody);
                if (upCaps.Length > 0) cx = DrawCapsule(g, upCaps, cx, cy, ch, fBody);

                // 右上角状态
                string corner = updateRunning ? "正在更新插件" : "服务运行中";
                int cw = TextRenderer.MeasureText(corner, fFoot).Width;
                int cxr = ClientSize.Width - right - cw - Ui.S(18);
                double pulse = Math.Sin(DateTime.Now.TimeOfDay.TotalSeconds * 2.2);
                int pr = Math.Max(Ui.S(3), Ui.S(4) + (int)(pulse * 2.0));
                int dotCx = cxr + Ui.S(7);
                int dotCy = Pad + Ui.S(27);
                using (SolidBrush db = new SolidBrush(Theme.GoldBright))
                    g.FillEllipse(db, dotCx - pr, dotCy - pr, pr * 2, pr * 2);
                TextRenderer.DrawText(g, corner, fFoot, new Rectangle(cxr + Ui.S(16), Pad + Ui.S(16), cw, Ui.S(24)), Theme.Muted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

                if (footMsg.Length > 0)
                    TextRenderer.DrawText(g, footMsg, fFoot, new Rectangle(0, Pad + Ui.S(222), ClientSize.Width, Ui.S(22)), Theme.Muted,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
            finally { fTitle.Dispose(); fBody.Dispose(); fFoot.Dispose(); fGreet.Dispose(); }
        }

        private int DrawCapsule(Graphics g, string text, int x, int y, int h, Font f)
        {
            int w = TextRenderer.MeasureText(text, f).Width + Ui.S(22);
            Rectangle r = new Rectangle(x, y, w, h);
            using (SolidBrush b = new SolidBrush(Theme.CapsuleBg)) Shapes.FillRound(g, b, r, h / 2);
            TextRenderer.DrawText(g, text, f, r, Theme.CapsuleText,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            return x + w + Ui.S(9);
        }
    }

    internal static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        private static IntPtr foundLauncher;

        private static IntPtr FindLauncherWindow()
        {
            foundLauncher = IntPtr.Zero;
            uint me = (uint)Process.GetCurrentProcess().Id;
            EnumWindows(delegate(IntPtr hWnd, IntPtr lParam)
            {
                uint pid;
                GetWindowThreadProcessId(hWnd, out pid);
                if (pid == me) return true;
                try
                {
                    using (Process p = Process.GetProcessById((int)pid))
                    {
                        if (p.ProcessName.Equals("DSH", StringComparison.OrdinalIgnoreCase))
                        {
                            foundLauncher = hWnd;
                            return false;
                        }
                    }
                }
                catch { }
                return true;
            }, IntPtr.Zero);
            return foundLauncher;
        }

        [STAThread]
        internal static void Main()
        {
            bool created;
            using (Mutex mtx = new Mutex(true, "DSHWhaleLauncher_SingleInstance", out created))
            {
                if (!created)
                {
                    IntPtr h = FindLauncherWindow();
                    if (h != IntPtr.Zero)
                    {
                        ShowWindow(h, 9);
                        SetForegroundWindow(h);
                    }
                    return;
                }
                try { SetProcessDPIAware(); } catch { }
                Ui.Init();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Dsh.Scan();
                Application.Run(new LauncherForm(Dsh.IsRunning()));
            }
        }
    }
}
