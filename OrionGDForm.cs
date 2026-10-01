using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OrionGDWidget
{
    // ══════════════════════════════════════════════════════════
    //  Interactive Item Descriptors
    // ══════════════════════════════════════════════════════════
    internal class FocusCard
    {
        public RectangleF Bounds;
        public string     Type;
        public string     Title;
        public string     Subtitle;
        public Color      AccentColor;
        public bool       IsHovered;

        public FocusCard(string type, string title, string subtitle, Color accentColor)
        {
            Type        = type;
            Title       = title;
            Subtitle    = subtitle;
            AccentColor = accentColor;
        }
    }

    internal class SkillPill
    {
        public RectangleF Bounds;
        public string     Type;
        public string     Name;
        public bool       IsHovered;

        public SkillPill(string type, string name)
        {
            Type = type;
            Name = name;
        }
    }

    internal class ConnectButton
    {
        public RectangleF Bounds;
        public string     Type;
        public string     Handle;
        public string     Platform;
        public string     Url;
        public string     Tooltip;
        public bool       IsHovered;

        public ConnectButton(string type, string handle, string platform, string url, string tooltip)
        {
            Type     = type;
            Handle   = handle;
            Platform = platform;
            Url      = url;
            Tooltip  = tooltip;
        }
    }

    // ══════════════════════════════════════════════════════════
    //  Main OrionGD Identity Dashboard Widget (32bpp Layered Window)
    // ══════════════════════════════════════════════════════════
    public class OrionGDForm : Form
    {
        // ── Base Canvas Dimensions (96 DPI Reference) ──────────
        private const float BaseCardW = 500f;
        private const float BaseCardH = 744f;
        private const float BaseCardR = 20f;
        private const float BasePad   = 22f;

        // ── Curated Futuristic Palette ─────────────────────────
        private static readonly Color C_BG_TOP        = Color.FromArgb(255, 6,   9,   19);
        private static readonly Color C_BG_MID        = Color.FromArgb(255, 10,  15,  30);
        private static readonly Color C_BG_BOTTOM     = Color.FromArgb(255, 5,   8,   17);
        private static readonly Color C_BORDER        = Color.FromArgb(255, 26,  36,  54);
        private static readonly Color C_CYAN          = Color.FromArgb(255, 56,  189, 248);
        private static readonly Color C_CYAN_BRIGHT   = Color.FromArgb(255, 186, 230, 253);
        private static readonly Color C_EMERALD       = Color.FromArgb(255, 16,  185, 129);
        private static readonly Color C_PURPLE        = Color.FromArgb(255, 168, 85,  247);
        private static readonly Color C_AMBER         = Color.FromArgb(255, 245, 158, 11);
        private static readonly Color C_CRIMSON       = Color.FromArgb(255, 239, 68,  68);
        private static readonly Color C_INDIGO        = Color.FromArgb(255, 99,  102, 241);
        private static readonly Color C_TEXT_PRIMARY  = Color.FromArgb(255, 248, 250, 252);
        private static readonly Color C_TEXT_SECOND   = Color.FromArgb(255, 148, 163, 184);
        private static readonly Color C_TEXT_MUTED    = Color.FromArgb(255, 100, 116, 139);
        private static readonly Color C_TILE_BG       = Color.FromArgb(255, 12,  18,  34);
        private static readonly Color C_TILE_BORDER   = Color.FromArgb(255, 28,  39,  62);

        // ── Scaling & Bounds ───────────────────────────────────
        private float      _dpiScale = 1.0f;
        private RectangleF _cardBounds;

        // ── Motion & Animation State ───────────────────────────
        private System.Windows.Forms.Timer? _animTimer;
        private float _pulse          = 0f;
        private float _glowPulse      = 0f;
        private float _orbitRingAngle = 0f;

        // ── Dragging State ─────────────────────────────────────
        private bool  _dragging = false;
        private Point _dragStart;
        private Point _winStart;

        // ── Interactive UI Components ──────────────────────────
        private readonly List<FocusCard>     _focusCards     = new();
        private readonly List<SkillPill>     _skillPills     = new();
        private readonly List<ConnectButton> _connectButtons = new();
        private readonly ToolTip             _tooltip        = new ToolTip();
        private string                       _activeTooltip  = string.Empty;

        // Hitboxes
        private RectangleF _btnCloseBounds;
        private RectangleF _btnMinBounds;
        private RectangleF _availablePillBounds;
        private RectangleF _certCardBounds;
        private RectangleF _patentCardBounds;

        private bool _btnCloseHovered;
        private bool _btnMinHovered;
        private bool _availablePillHovered;
        private bool _certCardHovered;
        private bool _patentCardHovered;

        // ── System Tray & Context Menu ─────────────────────────
        private NotifyIcon?       _trayIcon;
        private ContextMenuStrip? _contextMenu;

        // ── Scaled Typography ──────────────────────────────────
        private Font? _fontTopHeader;
        private Font? _fontTopSub;
        private Font? _fontHeroTag;
        private Font? _fontHeroSub;
        private Font? _fontAvailStatus;
        private Font? _fontAvailSub;
        private Font? _fontBrand;
        private Font? _fontName;
        private Font? _fontRolePill;
        private Font? _fontBio;
        private Font? _fontSecHeader;
        private Font? _fontFocusTitle;
        private Font? _fontFocusSub;
        private Font? _fontSkillPill;
        private Font? _fontCertHeader;
        private Font? _fontCertBody;
        private Font? _fontPatentTitle;
        private Font? _fontPatentApp;
        private Font? _fontPatentDesc;
        private Font? _fontConnectHandle;
        private Font? _fontConnectHandleCompact;
        private Font? _fontConnectPlatform;
        private Font? _fontFooterQuote;
        private Font? _fontFooterTag;
        private Font? _fontMonogram;

        // ── 32bpp Layered Window DIB Surface ───────────────────
        private IntPtr    _screenDc   = IntPtr.Zero;
        private IntPtr    _memDc      = IntPtr.Zero;
        private IntPtr    _hBitmap    = IntPtr.Zero;
        private IntPtr    _oldBitmap  = IntPtr.Zero;
        private Bitmap?   _dibBitmap;
        private Graphics? _dibGraphics;
        private int       _bmpW = 0;
        private int       _bmpH = 0;

        // ══════════════════════════════════════════════════════
        //  Constructor
        // ══════════════════════════════════════════════════════
        public OrionGDForm()
        {
            UpdateDpiScale();
            InitForm();
            InitFonts();
            InitElements();
            BuildTrayAndMenu();
            StartAnimation();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_LAYERED; // Hardware composited 32bpp ARGB
                return cp;
            }
        }

        private void UpdateDpiScale()
        {
            _dpiScale = DeviceDpi / 96.0f;
            if (_dpiScale < 0.5f) _dpiScale = 1.0f;
        }

        private float S(float baseValue) => baseValue * _dpiScale;

        // ──────────────────────────────────────────────────────
        private void InitForm()
        {
            Text            = "OrionGD Identity Dashboard";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition   = FormStartPosition.Manual;
            TopMost         = true;
            ShowInTaskbar   = false;
            DoubleBuffered  = true;

            int totalW = (int)Math.Ceiling(S(BaseCardW + BasePad * 2));
            int totalH = (int)Math.Ceiling(S(BaseCardH + BasePad * 2));
            Width  = totalW;
            Height = totalH;

            _cardBounds = new RectangleF(
                S(BasePad),
                S(BasePad),
                S(BaseCardW),
                S(BaseCardH));

            var screen = Screen.PrimaryScreen?.WorkingArea ?? Screen.AllScreens[0].WorkingArea;
            Left = screen.Right - Width - (int)S(25);
            Top  = screen.Top   + (int)S(25);

            MouseDown  += OnMouseDown;
            MouseMove  += OnMouseMove;
            MouseUp    += OnMouseUp;
            MouseClick += OnMouseClick;

            _tooltip.InitialDelay = 250;
            _tooltip.ReshowDelay  = 100;
            _tooltip.AutoPopDelay = 5000;
        }

        // ──────────────────────────────────────────────────────
        private void InitFonts()
        {
            _fontTopHeader?.Dispose();
            _fontTopSub?.Dispose();
            _fontHeroTag?.Dispose();
            _fontHeroSub?.Dispose();
            _fontAvailStatus?.Dispose();
            _fontAvailSub?.Dispose();
            _fontBrand?.Dispose();
            _fontName?.Dispose();
            _fontRolePill?.Dispose();
            _fontBio?.Dispose();
            _fontSecHeader?.Dispose();
            _fontFocusTitle?.Dispose();
            _fontFocusSub?.Dispose();
            _fontSkillPill?.Dispose();
            _fontCertHeader?.Dispose();
            _fontCertBody?.Dispose();
            _fontPatentTitle?.Dispose();
            _fontPatentApp?.Dispose();
            _fontPatentDesc?.Dispose();
            _fontConnectHandle?.Dispose();
            _fontConnectHandleCompact?.Dispose();
            _fontConnectPlatform?.Dispose();
            _fontFooterQuote?.Dispose();
            _fontFooterTag?.Dispose();
            _fontMonogram?.Dispose();

            _fontTopHeader            = new Font("Segoe UI", S(9.0f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontTopSub               = new Font("Segoe UI", S(7.0f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontHeroTag              = new Font("Segoe UI", S(8.5f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontHeroSub              = new Font("Segoe UI", S(6.8f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontAvailStatus          = new Font("Segoe UI", S(7.8f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontAvailSub             = new Font("Segoe UI", S(6.8f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontBrand                = new Font("Segoe UI", S(20.5f), FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontName                 = new Font("Segoe UI", S(15.5f), FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontRolePill             = new Font("Segoe UI", S(8.5f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontBio                  = new Font("Segoe UI", S(9.2f),  FontStyle.Regular, GraphicsUnit.Pixel);
            _fontSecHeader            = new Font("Segoe UI", S(8.2f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontFocusTitle           = new Font("Segoe UI", S(9.2f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontFocusSub             = new Font("Segoe UI", S(7.2f),  FontStyle.Regular, GraphicsUnit.Pixel);
            _fontSkillPill            = new Font("Segoe UI", S(8.0f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontCertHeader           = new Font("Segoe UI", S(8.0f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontCertBody             = new Font("Segoe UI", S(6.8f),  FontStyle.Regular, GraphicsUnit.Pixel);
            _fontPatentTitle          = new Font("Segoe UI", S(9.5f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontPatentApp            = new Font("Segoe UI", S(7.5f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontPatentDesc           = new Font("Segoe UI", S(7.2f),  FontStyle.Regular, GraphicsUnit.Pixel);
            _fontConnectHandle        = new Font("Segoe UI", S(7.8f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontConnectHandleCompact = new Font("Segoe UI", S(5.8f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontConnectPlatform      = new Font("Segoe UI", S(6.8f),  FontStyle.Regular, GraphicsUnit.Pixel);
            _fontFooterQuote          = new Font("Segoe UI", S(8.2f),  FontStyle.Regular, GraphicsUnit.Pixel);
            _fontFooterTag            = new Font("Segoe UI", S(8.0f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontMonogram             = new Font("Segoe UI", S(18.0f), FontStyle.Bold,    GraphicsUnit.Pixel);
        }

        // ──────────────────────────────────────────────────────
        private void InitElements()
        {
            // 6 Focus Areas (3x2 Grid)
            _focusCards.Clear();
            _focusCards.Add(new FocusCard("fullstack", "Full-Stack",      "Web • MERN • APIs",          C_EMERALD));
            _focusCards.Add(new FocusCard("ai-int",    "AI Integration",  "LLMs • RAG • Automation",    C_PURPLE));
            _focusCards.Add(new FocusCard("ux-user",   "User-Centric UX", "Design • Accessibility",     C_AMBER));
            _focusCards.Add(new FocusCard("ai-llm",    "AI & LLMs",       "Gemini • Groq • HuggingFace", C_CYAN));
            _focusCards.Add(new FocusCard("security",  "Security & Cloud","Zero Trust • Azure",         C_CRIMSON));
            _focusCards.Add(new FocusCard("uiux",      "UI / UX Design",  "Modern • Responsive",        Color.FromArgb(255, 192, 132, 252)));

            // 10 Skills & Technologies Pills
            _skillPills.Clear();
            _skillPills.Add(new SkillPill("c",             "C"));
            _skillPills.Add(new SkillPill("cpp",           "C++"));
            _skillPills.Add(new SkillPill("java",          "Java"));
            _skillPills.Add(new SkillPill("python",        "Python"));
            _skillPills.Add(new SkillPill("sql",           "SQL"));
            _skillPills.Add(new SkillPill("react",         "React"));
            _skillPills.Add(new SkillPill("mern",          "MERN"));
            _skillPills.Add(new SkillPill("uiux",          "UI/UX"));
            _skillPills.Add(new SkillPill("3d",            "3D/XR"));
            _skillPills.Add(new SkillPill("cybersecurity", "Cybersecurity"));

            // 6 Connect Platform Buttons
            _connectButtons.Clear();
            _connectButtons.Add(new ConnectButton("github",      "TheOrionGD",              "GitHub",       "https://github.com/OrionGD",                             "GitHub / OrionGD"));
            _connectButtons.Add(new ConnectButton("linkedin",    "godfrey-1823lw",          "LinkedIn",     "https://www.linkedin.com/in/godfrey-1823lw/",            "LinkedIn / Godfrey T. R"));
            _connectButtons.Add(new ConnectButton("hackerrank",  "OrionGD07",               "HackerRank",   "https://www.hackerrank.com/OrionGD07",                   "HackerRank / OrionGD07"));
            _connectButtons.Add(new ConnectButton("portfolio",   "the-orion-gd.vercel.app", "Portfolio",    "https://the-orion-gd.vercel.app/",                       "Official Portfolio Website"));
            _connectButtons.Add(new ConnectButton("play",        "TheOrionGD",              "Play Console", "https://play.google.com/store/apps/developer?id=TheOrionGD", "Google Play Developer Profile"));
            _connectButtons.Add(new ConnectButton("youtube",     "theoriongd",              "YouTube",      "https://www.youtube.com/@theoriongd",                    "YouTube Channel / @theoriongd"));
        }

        // ──────────────────────────────────────────────────────
        private void BuildTrayAndMenu()
        {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add("Show / Hide Dashboard", null, (s, e) => ToggleVisibility());
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add("Open Portfolio Website", null, (s, e) => OpenUrl("https://the-orion-gd.vercel.app/"));
            _contextMenu.Items.Add("Open GitHub Profile",    null, (s, e) => OpenUrl("https://github.com/OrionGD"));
            _contextMenu.Items.Add("Open LinkedIn Profile",  null, (s, e) => OpenUrl("https://www.linkedin.com/in/godfrey-1823lw/"));
            _contextMenu.Items.Add("Open HackerRank Profile",null, (s, e) => OpenUrl("https://www.hackerrank.com/OrionGD07"));
            _contextMenu.Items.Add("Open Google Play Store", null, (s, e) => OpenUrl("https://play.google.com/store/apps/developer?id=TheOrionGD"));
            _contextMenu.Items.Add("Open YouTube Channel",   null, (s, e) => OpenUrl("https://www.youtube.com/@theoriongd"));
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add("About OrionGD Identity", null, (s, e) => ShowAboutDialog());
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add("Exit",                   null, (s, e) => Close());

            _contextMenu.BackColor       = Color.FromArgb(255, 12, 18, 34);
            _contextMenu.ForeColor       = Color.FromArgb(255, 241, 245, 249);
            _contextMenu.ShowImageMargin = false;

            Icon trayIcon = CreateBrandIcon();
            _trayIcon = new NotifyIcon
            {
                Text             = "OrionGD Identity Dashboard",
                Icon             = trayIcon,
                Visible          = true,
                ContextMenuStrip = _contextMenu
            };
            _trayIcon.DoubleClick += (s, e) => ToggleVisibility();
        }

        private Icon CreateBrandIcon()
        {
            int size = 32;
            using var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using var bgBrush = new SolidBrush(Color.FromArgb(255, 10, 14, 26));
                g.FillEllipse(bgBrush, 1, 1, size - 2, size - 2);

                using var ringPen = new Pen(C_CYAN, 1.5f);
                g.DrawEllipse(ringPen, 2, 2, size - 4, size - 4);

                using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.White);
                var sf = new StringFormat
                {
                    Alignment     = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString("GD", font, textBrush, new RectangleF(0, 0, size, size), sf);
            }
            return Icon.FromHandle(bmp.GetHicon());
        }

        private void ToggleVisibility()
        {
            Visible = !Visible;
            if (Visible)
            {
                BringToFront();
                _animTimer?.Start();
                RenderLayeredWindow();
            }
            else
            {
                _animTimer?.Stop();
            }
        }

        private void StartAnimation()
        {
            _animTimer       = new System.Windows.Forms.Timer { Interval = 33 };
            _animTimer.Tick += OnTick;
            _animTimer.Start();
        }

        private void OnTick(object? sender, EventArgs e)
        {
            if (!Visible) return;

            _pulse          += 0.040f;
            _glowPulse      += 0.025f;
            _orbitRingAngle += 0.012f;

            RenderLayeredWindow();
        }

        // ══════════════════════════════════════════════════════
        //  MOUSE & HIT TESTING
        // ══════════════════════════════════════════════════════
        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_btnCloseBounds.Contains(e.Location) ||
                    _btnMinBounds.Contains(e.Location) ||
                    _availablePillBounds.Contains(e.Location) ||
                    _certCardBounds.Contains(e.Location) ||
                    _patentCardBounds.Contains(e.Location))
                    return;

                foreach (var fc in _focusCards)
                    if (fc.Bounds.Contains(e.Location)) return;

                foreach (var p in _skillPills)
                    if (p.Bounds.Contains(e.Location)) return;

                foreach (var b in _connectButtons)
                    if (b.Bounds.Contains(e.Location)) return;

                _dragging  = true;
                _dragStart = Cursor.Position;
                _winStart  = Location;
            }
            else if (e.Button == MouseButtons.Right)
            {
                _contextMenu?.Show(this, e.Location);
            }
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                var cur  = Cursor.Position;
                Location = new Point(
                    _winStart.X + (cur.X - _dragStart.X),
                    _winStart.Y + (cur.Y - _dragStart.Y));
                return;
            }

            Point pt = e.Location;
            bool repaint = false;
            bool anyHover = false;
            string hoveredTooltip = string.Empty;

            bool newClose = _btnCloseBounds.Contains(pt);
            if (newClose != _btnCloseHovered) { _btnCloseHovered = newClose; repaint = true; }
            if (newClose) { anyHover = true; hoveredTooltip = "Close Widget"; }

            bool newMin = _btnMinBounds.Contains(pt);
            if (newMin != _btnMinHovered) { _btnMinHovered = newMin; repaint = true; }
            if (newMin) { anyHover = true; hoveredTooltip = "Minimize to System Tray"; }

            bool newAvail = _availablePillBounds.Contains(pt);
            if (newAvail != _availablePillHovered) { _availablePillHovered = newAvail; repaint = true; }
            if (newAvail) { anyHover = true; hoveredTooltip = "Godfrey T. R is currently open for engineering collaboration"; }

            bool newCert = _certCardBounds.Contains(pt);
            if (newCert != _certCardHovered) { _certCardHovered = newCert; repaint = true; }
            if (newCert) { anyHover = true; hoveredTooltip = "Professional Certifications & Accreditations"; }

            bool newPatent = _patentCardBounds.Contains(pt);
            if (newPatent != _patentCardHovered) { _patentCardHovered = newPatent; repaint = true; }
            if (newPatent) { anyHover = true; hoveredTooltip = "Indian Patent App. No. 202441033032: IR-based Android TV control"; }

            foreach (var fc in _focusCards)
            {
                bool wasH = fc.IsHovered;
                fc.IsHovered = fc.Bounds.Contains(pt);
                if (fc.IsHovered != wasH) repaint = true;
                if (fc.IsHovered)
                {
                    anyHover = true;
                    hoveredTooltip = $"{fc.Title} — {fc.Subtitle}";
                }
            }

            foreach (var p in _skillPills)
            {
                bool wasH = p.IsHovered;
                p.IsHovered = p.Bounds.Contains(pt);
                if (p.IsHovered != wasH) repaint = true;
                if (p.IsHovered)
                {
                    anyHover = true;
                    hoveredTooltip = $"Skill / Technology: {p.Name}";
                }
            }

            foreach (var b in _connectButtons)
            {
                bool wasH = b.IsHovered;
                b.IsHovered = b.Bounds.Contains(pt);
                if (b.IsHovered != wasH) repaint = true;
                if (b.IsHovered)
                {
                    anyHover = true;
                    hoveredTooltip = b.Tooltip;
                }
            }

            Cursor = anyHover ? Cursors.Hand : Cursors.Default;

            if (hoveredTooltip != _activeTooltip)
            {
                _activeTooltip = hoveredTooltip;
                if (!string.IsNullOrEmpty(_activeTooltip))
                    _tooltip.SetToolTip(this, _activeTooltip);
                else
                    _tooltip.SetToolTip(this, null);
            }

            if (repaint) RenderLayeredWindow();
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _dragging = false;
        }

        private void OnMouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            Point pt = e.Location;

            if (_btnCloseBounds.Contains(pt))
            {
                Close();
                return;
            }

            if (_btnMinBounds.Contains(pt))
            {
                ToggleVisibility();
                return;
            }

            if (_availablePillBounds.Contains(pt) || _certCardBounds.Contains(pt) || _patentCardBounds.Contains(pt))
            {
                OpenUrl("https://the-orion-gd.vercel.app/");
                return;
            }

            foreach (var fc in _focusCards)
            {
                if (fc.Bounds.Contains(pt))
                {
                    OpenUrl("https://the-orion-gd.vercel.app/");
                    return;
                }
            }

            foreach (var b in _connectButtons)
            {
                if (b.Bounds.Contains(pt))
                {
                    OpenUrl(b.Url);
                    return;
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST)
            {
                int lp = m.LParam.ToInt32();
                short x = (short)(lp & 0xFFFF);
                short y = (short)((lp >> 16) & 0xFFFF);
                Point clientPt = PointToClient(new Point(x, y));

                if (_cardBounds.Contains(clientPt))
                {
                    m.Result = (IntPtr)HTCLIENT;
                    return;
                }
                else
                {
                    m.Result = (IntPtr)HTTRANSPARENT;
                    return;
                }
            }

            base.WndProc(ref m);
        }

        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);
            UpdateDpiScale();
            InitFonts();

            int totalW = (int)Math.Ceiling(S(BaseCardW + BasePad * 2));
            int totalH = (int)Math.Ceiling(S(BaseCardH + BasePad * 2));
            Width  = totalW;
            Height = totalH;

            _cardBounds = new RectangleF(
                S(BasePad),
                S(BasePad),
                S(BaseCardW),
                S(BaseCardH));

            RecreateDib(Width, Height);
            RenderLayeredWindow();
        }

        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch { /* browser launcher unavailable */ }
        }

        private void ShowAboutDialog()
        {
            using var dlg = new AboutDialog();
            dlg.ShowDialog(this);
        }

        // ══════════════════════════════════════════════════════
        //  LAYERED WINDOW DIB ENGINE
        // ══════════════════════════════════════════════════════
        private void EnsureDib(int width, int height)
        {
            if (_dibBitmap != null && _bmpW == width && _bmpH == height) return;
            RecreateDib(width, height);
        }

        private void RecreateDib(int width, int height)
        {
            CleanupDib();

            _bmpW = Math.Max(1, width);
            _bmpH = Math.Max(1, height);

            _screenDc = GetDC(IntPtr.Zero);
            _memDc    = CreateCompatibleDC(_screenDc);

            var bi = new BITMAPINFO();
            bi.bmiHeader.biSize        = (uint)Marshal.SizeOf<BITMAPINFOHEADER>();
            bi.bmiHeader.biWidth       = _bmpW;
            bi.bmiHeader.biHeight      = -_bmpH;
            bi.bmiHeader.biPlanes      = 1;
            bi.bmiHeader.biBitCount    = 32;
            bi.bmiHeader.biCompression = 0;

            _hBitmap   = CreateDIBSection(_screenDc, ref bi, 0, out IntPtr pBits, IntPtr.Zero, 0);
            _oldBitmap = SelectObject(_memDc, _hBitmap);

            _dibBitmap   = new Bitmap(_bmpW, _bmpH, _bmpW * 4, PixelFormat.Format32bppPArgb, pBits);
            _dibGraphics = Graphics.FromImage(_dibBitmap);
            _dibGraphics.SmoothingMode     = SmoothingMode.AntiAlias;
            _dibGraphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            _dibGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        }

        private void CleanupDib()
        {
            _dibGraphics?.Dispose();
            _dibGraphics = null;

            _dibBitmap?.Dispose();
            _dibBitmap = null;

            if (_memDc != IntPtr.Zero && _oldBitmap != IntPtr.Zero)
            {
                SelectObject(_memDc, _oldBitmap);
                _oldBitmap = IntPtr.Zero;
            }

            if (_hBitmap != IntPtr.Zero)
            {
                DeleteObject(_hBitmap);
                _hBitmap = IntPtr.Zero;
            }

            if (_memDc != IntPtr.Zero)
            {
                DeleteDC(_memDc);
                _memDc = IntPtr.Zero;
            }

            if (_screenDc != IntPtr.Zero)
            {
                ReleaseDC(IntPtr.Zero, _screenDc);
                _screenDc = IntPtr.Zero;
            }
        }

        private void RenderLayeredWindow()
        {
            if (!IsHandleCreated || IsDisposed || Disposing) return;

            int w = Width;
            int h = Height;
            if (w <= 0 || h <= 0) return;

            EnsureDib(w, h);
            if (_dibGraphics == null || _memDc == IntPtr.Zero) return;

            _dibGraphics.Clear(Color.FromArgb(0, 0, 0, 0));
            PaintWidget(_dibGraphics);
            _dibGraphics.Flush();

            var ptDst = new POINT { X = Left, Y = Top };
            var szDst = new SIZE  { CX = w,    CY = h };
            var ptSrc = new POINT { X = 0,    Y = 0 };

            var blend = new BLENDFUNCTION
            {
                BlendOp             = AC_SRC_OVER,
                BlendFlags          = 0,
                SourceConstantAlpha = 255,
                AlphaFormat         = AC_SRC_ALPHA
            };

            UpdateLayeredWindow(Handle, _screenDc, ref ptDst, ref szDst, _memDc, ref ptSrc, 0, ref blend, ULW_ALPHA);
        }

        public Bitmap RenderToBitmap()
        {
            var bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode     = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.Clear(Color.Transparent);
                PaintWidget(g);
            }
            return bmp;
        }

        // ══════════════════════════════════════════════════════
        //  MASTER PAINT PIPELINE
        // ══════════════════════════════════════════════════════
        private void PaintWidget(Graphics g)
        {
            var card = _cardBounds;
            float r = S(BaseCardR);

            // 1. Ambient back-glow & deep drop shadow
            DrawAmbientGlow(g, card, r);

            // 2. Glassmorphic card body with vibrant cyan-emerald gradient laser rim
            DrawCardSurface(g, card, r);

            // 3. Top Header Bar: "ORION IDENTITY / BUILD • SOLVE • IMPACT" & Window buttons
            DrawTopHeaderBar(g, card);

            // 4. Hero Section: Left Winged Logo + Center Holographic Crest + Right "AVAILABLE" pill
            float heroCY = card.Y + S(86f);
            DrawHeroSection(g, card, heroCY);

            // 5. Identity Titles: ORIONGD, Godfrey T. R, [SOFTWARE ENGINEER], Bio
            float idY = card.Y + S(138f);
            idY = DrawIdentityTitles(g, card, idY);

            // 6. Focus Areas 3x2 Grid
            float focusY = idY + S(10f);
            focusY = DrawFocusGrid(g, card, focusY);

            // 7. Skills & Technologies Section
            float skillsY = focusY + S(12f);
            skillsY = DrawSkillsSection(g, card, skillsY);

            // 8. Certifications & Indian Patent Row
            float certY = skillsY + S(12f);
            certY = DrawCertAndPatentRow(g, card, certY);

            // 9. Connect With Me 6 Platform Buttons
            float connectY = certY + S(12f);
            connectY = DrawConnectSection(g, card, connectY);

            // 10. Footer Bar: "> Always learning. Always building." and "/// ORIONGD × CSE 23 A"
            DrawFooterBar(g, card, card.Bottom - S(26f));
        }

        // ──────────────────────────────────────────────────────
        // 1. Ambient Glow & Shadows
        // ──────────────────────────────────────────────────────
        private void DrawAmbientGlow(Graphics g, RectangleF card, float r)
        {
            for (int i = 4; i >= 1; i--)
            {
                float spread = S(i * 3.5f);
                var shadowRect = new RectangleF(
                    card.X - spread + S(1f),
                    card.Y - spread + S(4f),
                    card.Width + spread * 2,
                    card.Height + spread * 2);

                int alpha = (int)(24 / (i * 0.85f));
                using var sb = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0));
                using var sp = RoundedRect(shadowRect, r + spread);
                g.FillPath(sb, sp);
            }

            float glowBright = (float)(0.40 + 0.25 * Math.Sin(_glowPulse));
            int ga = Math.Clamp((int)(glowBright * 55), 0, 255);
            using var glowBrush = new SolidBrush(Color.FromArgb(ga, 56, 189, 248));
            float glowSpread = S(3.5f);
            var glowRect = new RectangleF(
                card.X - glowSpread,
                card.Y - glowSpread,
                card.Width + glowSpread * 2,
                card.Height + glowSpread * 2);
            using var glowPath = RoundedRect(glowRect, r + glowSpread);
            g.FillPath(glowBrush, glowPath);
        }

        // ──────────────────────────────────────────────────────
        // 2. Glassmorphic Card Surface & Dual Gradient Border
        // ──────────────────────────────────────────────────────
        private void DrawCardSurface(Graphics g, RectangleF card, float r)
        {
            using var cardPath = RoundedRect(card, r);

            // Deep cosmic background gradient
            using var bgBrush = new LinearGradientBrush(
                new PointF(card.X, card.Y),
                new PointF(card.Right, card.Bottom),
                C_BG_TOP, C_BG_BOTTOM);

            var cb = new ColorBlend(3)
            {
                Positions = new[] { 0.0f, 0.50f, 1.0f },
                Colors    = new[] { C_BG_TOP, C_BG_MID, C_BG_BOTTOM }
            };
            bgBrush.InterpolationColors = cb;
            g.FillPath(bgBrush, cardPath);

            // Specular top glass curvature sheen
            var sheenRect = new RectangleF(card.X, card.Y, card.Width, S(48f));
            using var sheenPath = RoundedRect(sheenRect, r);
            using var sheenBrush = new LinearGradientBrush(
                new PointF(card.X, card.Y),
                new PointF(card.X, card.Y + S(48f)),
                Color.FromArgb(24, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255));
            g.FillPath(sheenBrush, sheenPath);

            // Subtle base border
            using var baseBorderPen = new Pen(C_BORDER, S(1.2f));
            g.DrawPath(baseBorderPen, cardPath);

            // Top-edge radiant cyan-to-emerald laser highlight
            using var gradRimBrush = new LinearGradientBrush(
                new PointF(card.X, card.Y),
                new PointF(card.Right, card.Y),
                Color.Transparent, Color.Transparent);

            var rimCb = new ColorBlend(4)
            {
                Positions = new[] { 0.0f, 0.25f, 0.85f, 1.0f },
                Colors    = new[] {
                    Color.FromArgb(40, 56, 189, 248),
                    Color.FromArgb(220, 56, 189, 248),
                    Color.FromArgb(200, 16, 185, 129),
                    Color.FromArgb(40, 16, 185, 129)
                }
            };
            gradRimBrush.InterpolationColors = rimCb;
            using var rimPen = new Pen(gradRimBrush, S(1.5f));
            g.DrawLine(rimPen, card.X + r, card.Y, card.Right - r, card.Y);
        }

        // ──────────────────────────────────────────────────────
        // 3. Top Header Bar
        // ──────────────────────────────────────────────────────
        private void DrawTopHeaderBar(Graphics g, RectangleF card)
        {
            float y = card.Y + S(12f);
            float x = card.X + S(18f);

            // 4-point Diamond Star
            DrawDiamondSparkle(g, x + S(4f), y + S(6f), S(4.5f), C_CYAN);

            // ORION IDENTITY
            using var headBrush = new SolidBrush(Color.White);
            g.DrawString("ORION IDENTITY", _fontTopHeader!, headBrush, x + S(14f), y);

            // BUILD  •  SOLVE  •  IMPACT
            using var subBrush = new SolidBrush(C_TEXT_MUTED);
            g.DrawString("BUILD   •   SOLVE   •   IMPACT", _fontTopSub!, subBrush, x + S(14f), y + S(13f));

            // Window Controls: Minimize [─] and Close [✕]
            float btnSz = S(18f);
            _btnCloseBounds = new RectangleF(card.Right - S(16f) - btnSz, y, btnSz, btnSz);
            _btnMinBounds   = new RectangleF(_btnCloseBounds.X - S(6f) - btnSz, y, btnSz, btnSz);

            DrawWindowButton(g, _btnMinBounds, false, _btnMinHovered);
            DrawWindowButton(g, _btnCloseBounds, true, _btnCloseHovered);
        }

        private void DrawWindowButton(Graphics g, RectangleF r, bool isClose, bool isHovered)
        {
            using var path = RoundedRect(r, S(4f));

            Color bg = isHovered
                ? (isClose ? Color.FromArgb(180, 239, 68, 68) : Color.FromArgb(160, 30, 41, 59))
                : Color.FromArgb(40, 255, 255, 255);
            using var bgB = new SolidBrush(bg);
            g.FillPath(bgB, path);

            Color borderC = isHovered
                ? (isClose ? Color.FromArgb(255, 248, 113, 113) : C_CYAN)
                : Color.FromArgb(30, 255, 255, 255);
            using var bPen = new Pen(borderC, S(0.8f));
            g.DrawPath(bPen, path);

            float cx = r.X + r.Width / 2f;
            float cy = r.Y + r.Height / 2f;
            Color strokeC = isHovered ? Color.White : Color.FromArgb(200, 148, 163, 184);
            using var pen = new Pen(strokeC, S(1.2f))
            {
                StartCap = LineCap.Round,
                EndCap   = LineCap.Round
            };

            if (isClose)
            {
                float d = S(3.5f);
                g.DrawLine(pen, cx - d, cy - d, cx + d, cy + d);
                g.DrawLine(pen, cx + d, cy - d, cx - d, cy + d);
            }
            else
            {
                float w = S(4.5f);
                g.DrawLine(pen, cx - w, cy, cx + w, cy);
            }
        }

        // ──────────────────────────────────────────────────────
        // 4. Hero Section: Left Logo + Center Crest + Right Pill
        // ──────────────────────────────────────────────────────
        private void DrawHeroSection(Graphics g, RectangleF card, float cy)
        {
            // Left Hero Brand Block
            float leftX = card.X + S(22f);
            DrawWingedBrandLogo(g, leftX, cy - S(3f));

            using var bBrush = new SolidBrush(Color.White);
            g.DrawString("ORIONGD", _fontHeroTag!, bBrush, leftX + S(16f), cy - S(10f));

            using var subB = new SolidBrush(Color.FromArgb(180, 148, 163, 184));
            g.DrawString("CODE  ×  CREATE  ×  GROW", _fontHeroSub!, subB, leftX + S(16f), cy + S(3f));

            // Center Holographic Crest
            float cx = card.X + card.Width / 2f;
            DrawCenterHoloCrest(g, cx, cy);

            // Right Available Pill
            float pillW = S(116f);
            float pillH = S(32f);
            _availablePillBounds = new RectangleF(card.Right - S(20f) - pillW, cy - pillH / 2f, pillW, pillH);
            DrawAvailablePill(g, _availablePillBounds, _availablePillHovered);
        }

        private void DrawWingedBrandLogo(Graphics g, float cx, float cy)
        {
            float s = S(7f);
            using var brush = new SolidBrush(C_CYAN);
            using var pen   = new Pen(C_CYAN_BRIGHT, S(1.1f)) { StartCap = LineCap.Round, EndCap = LineCap.Round };

            // Central 4-point diamond
            PointF[] diamond = {
                new PointF(cx, cy - s),
                new PointF(cx + s * 0.45f, cy),
                new PointF(cx, cy + s),
                new PointF(cx - s * 0.45f, cy)
            };
            g.FillPolygon(brush, diamond);

            // Angled chevron wings
            g.DrawLine(pen, cx - s * 0.9f, cy - s * 0.6f, cx - s * 0.4f, cy);
            g.DrawLine(pen, cx - s * 0.4f, cy, cx - s * 0.9f, cy + s * 0.6f);

            g.DrawLine(pen, cx + s * 0.9f, cy - s * 0.6f, cx + s * 0.4f, cy);
            g.DrawLine(pen, cx + s * 0.4f, cy, cx + s * 0.9f, cy + s * 0.6f);
        }

        private void DrawCenterHoloCrest(Graphics g, float cx, float cy)
        {
            float r = S(32f);

            // Radial ambient bloom
            using var radialPath = new GraphicsPath();
            radialPath.AddEllipse(cx - r * 1.8f, cy - r * 1.8f, r * 3.6f, r * 3.6f);
            using var pgb = new PathGradientBrush(radialPath)
            {
                CenterColor = Color.FromArgb(55, 56, 189, 248),
                SurroundColors = new[] { Color.FromArgb(0, 56, 189, 248) }
            };
            g.FillPath(pgb, radialPath);

            // Tilted Gyroscope Orbital Ring 1
            var s1 = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(28f + MathF.Sin(_orbitRingAngle) * 6f);
            using var ringPen1 = new Pen(Color.FromArgb(90, 56, 189, 248), S(1.2f));
            ringPen1.DashStyle = DashStyle.Custom;
            ringPen1.DashPattern = new[] { 6f, 3.5f };
            g.DrawEllipse(ringPen1, -(r + S(12f)), -S(14f), (r + S(12f)) * 2, S(28f));
            g.Restore(s1);

            // Tilted Gyroscope Orbital Ring 2
            var s2 = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(-36f - MathF.Cos(_orbitRingAngle) * 5f);
            using var ringPen2 = new Pen(Color.FromArgb(65, 99, 102, 241), S(1.0f));
            g.DrawEllipse(ringPen2, -(r + S(9f)), -S(10f), (r + S(9f)) * 2, S(20f));
            g.Restore(s2);

            // Center Dark Disk
            using var circBrush = new LinearGradientBrush(
                new PointF(cx - r, cy - r),
                new PointF(cx + r, cy + r),
                Color.FromArgb(255, 20, 28, 48),
                Color.FromArgb(255, 8, 12, 22));
            g.FillEllipse(circBrush, cx - r, cy - r, r * 2, r * 2);

            // Vibrant Multi-Color Glowing Rim (Cyan -> Emerald)
            using var rimBrush = new LinearGradientBrush(
                new PointF(cx - r, cy - r),
                new PointF(cx + r, cy + r),
                C_CYAN, C_EMERALD);
            using var rimPen = new Pen(rimBrush, S(2.0f));
            g.DrawEllipse(rimPen, cx - r, cy - r, r * 2, r * 2);

            // Inner specular rim arc
            using var innerPen = new Pen(Color.FromArgb(85, 255, 255, 255), S(1.0f));
            g.DrawArc(innerPen, cx - r + S(2f), cy - r + S(2f), (r - S(2f)) * 2, (r - S(2f)) * 2, 200, 140);

            // Monogram "GD"
            var sf = new StringFormat
            {
                Alignment     = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            using var textShadow = new SolidBrush(Color.FromArgb(140, 0, 0, 0));
            g.DrawString("GD", _fontMonogram!, textShadow,
                new RectangleF(cx - r, cy - r + S(1.0f), r * 2, r * 2), sf);

            using var textBrush = new LinearGradientBrush(
                new PointF(cx, cy - S(14f)),
                new PointF(cx, cy + S(14f)),
                Color.White,
                C_CYAN_BRIGHT);
            g.DrawString("GD", _fontMonogram!, textBrush,
                new RectangleF(cx - r, cy - r - S(0.5f), r * 2, r * 2), sf);

            // Cyan 4-point Diamond Starburst below "GD"
            DrawDiamondSparkle(g, cx, cy + S(16.5f), S(3.5f), C_CYAN);
        }

        private void DrawAvailablePill(Graphics g, RectangleF r, bool isHovered)
        {
            using var path = RoundedRect(r, r.Height / 2f);

            Color bg = isHovered ? Color.FromArgb(45, 16, 185, 129) : Color.FromArgb(22, 16, 185, 129);
            using var bBrush = new SolidBrush(bg);
            g.FillPath(bBrush, path);

            Color bc = isHovered ? C_EMERALD : Color.FromArgb(100, 16, 185, 129);
            using var bPen = new Pen(bc, S(1.1f));
            g.DrawPath(bPen, path);

            // Pulsing Emerald Radar Dot
            float dotX = r.X + S(12f);
            float dotY = r.Y + r.Height / 2f;
            float dotR = S(3.0f);

            float ripple = (float)(Math.Sin(_pulse * 1.5f) * 0.5 + 0.5);
            int ripAlpha = Math.Clamp((int)((1f - ripple) * 110), 0, 255);
            float ripR = dotR + ripple * S(4.5f);
            using var ripBrush = new SolidBrush(Color.FromArgb(ripAlpha, C_EMERALD));
            g.FillEllipse(ripBrush, dotX - ripR, dotY - ripR, ripR * 2, ripR * 2);

            using var dotCore = new SolidBrush(C_EMERALD);
            g.FillEllipse(dotCore, dotX - dotR, dotY - dotR, dotR * 2, dotR * 2);

            // Text: Line 1 "AVAILABLE"
            using var t1 = new SolidBrush(Color.FromArgb(255, 167, 243, 208));
            g.DrawString("AVAILABLE", _fontAvailStatus!, t1, r.X + S(22f), r.Y + S(4.5f));

            // Text: Line 2 "FOR COLLABORATION"
            using var t2 = new SolidBrush(Color.FromArgb(220, 110, 231, 183));
            g.DrawString("FOR COLLABORATION", _fontAvailSub!, t2, r.X + S(22f), r.Y + S(16.5f));
        }

        // ──────────────────────────────────────────────────────
        // 5. Identity Titles: ORIONGD, Name, Pill, Bio
        // ──────────────────────────────────────────────────────
        private float DrawIdentityTitles(Graphics g, RectangleF card, float y)
        {
            float cx = card.X + card.Width / 2f;
            var sf = new StringFormat
            {
                Alignment     = StringAlignment.Center,
                LineAlignment = StringAlignment.Near
            };

            // ORIONGD
            using var brandBrush = new LinearGradientBrush(
                new PointF(cx - S(80f), y),
                new PointF(cx + S(80f), y),
                Color.White,
                C_CYAN);
            g.DrawString("ORIONGD", _fontBrand!, brandBrush, cx, y, sf);
            float h1 = g.MeasureString("ORIONGD", _fontBrand!).Height;
            y += h1 + S(1f);

            // Godfrey T. R
            using var nameBrush = new SolidBrush(C_TEXT_PRIMARY);
            g.DrawString("Godfrey T. R", _fontName!, nameBrush, cx, y, sf);
            float h2 = g.MeasureString("Godfrey T. R", _fontName!).Height;
            y += h2 + S(3f);

            // [ SOFTWARE ENGINEER ] Capsule Pill
            float pillW = S(168f);
            float pillH = S(21f);
            var roleRect = new RectangleF(cx - pillW / 2f, y, pillW, pillH);
            using var rolePath = RoundedRect(roleRect, pillH / 2f);

            using var roleBg = new SolidBrush(Color.FromArgb(28, 56, 189, 248));
            g.FillPath(roleBg, rolePath);

            using var roleBorder = new Pen(Color.FromArgb(90, 56, 189, 248), S(1f));
            g.DrawPath(roleBorder, rolePath);

            var pillSf = new StringFormat
            {
                Alignment     = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            using var roleTxt = new SolidBrush(C_CYAN);
            g.DrawString("SOFTWARE ENGINEER", _fontRolePill!, roleTxt, roleRect, pillSf);
            y += pillH + S(6f);

            // Bio statement
            const string bio = "Architecting resilient full-stack systems, intelligent AI platforms, security frameworks, and next-generation UI/UX.";
            var bioRect = new RectangleF(card.X + S(24f), y, card.Width - S(48f), S(26f));
            using var bioBrush = new SolidBrush(C_TEXT_SECOND);
            g.DrawString(bio, _fontBio!, bioBrush, bioRect, sf);
            y += S(26f);

            return y;
        }

        // ──────────────────────────────────────────────────────
        // 6. Focus Areas 3x2 Grid
        // ──────────────────────────────────────────────────────
        private float DrawFocusGrid(Graphics g, RectangleF card, float y)
        {
            float margin = S(16f);
            float gapX   = S(8f);
            float gapY   = S(7f);
            float totalW = card.Width - margin * 2;
            float cardW  = (totalW - gapX * 2f) / 3f;
            float cardH  = S(44f);

            for (int i = 0; i < _focusCards.Count; i++)
            {
                var fc = _focusCards[i];
                int col = i % 3;
                int row = i / 3;

                float bx = card.X + margin + col * (cardW + gapX);
                float by = y + row * (cardH + gapY);
                fc.Bounds = new RectangleF(bx, by, cardW, cardH);

                using var path = RoundedRect(fc.Bounds, S(8f));

                Color bg = fc.IsHovered ? Color.FromArgb(32, fc.AccentColor) : C_TILE_BG;
                using var bgB = new SolidBrush(bg);
                g.FillPath(bgB, path);

                Color bc = fc.IsHovered ? fc.AccentColor : C_TILE_BORDER;
                using var bPen = new Pen(bc, S(1f));
                g.DrawPath(bPen, path);

                float boxSz = S(30f);
                var boxRect = new RectangleF(bx + S(7f), by + (cardH - boxSz) / 2f, boxSz, boxSz);
                using var boxPath = RoundedRect(boxRect, S(6f));

                using var boxBg = new SolidBrush(Color.FromArgb(35, fc.AccentColor));
                g.FillPath(boxBg, boxPath);

                using var boxBorder = new Pen(Color.FromArgb(120, fc.AccentColor), S(1f));
                g.DrawPath(boxBorder, boxPath);

                DrawFocusVectorIcon(g, fc.Type, boxRect, fc.AccentColor);

                float textX = boxRect.Right + S(8f);
                Color tc = fc.IsHovered ? Color.White : C_TEXT_PRIMARY;
                using var tBrush = new SolidBrush(tc);
                g.DrawString(fc.Title, _fontFocusTitle!, tBrush, textX, by + S(7f));

                using var subBrush = new SolidBrush(C_TEXT_MUTED);
                g.DrawString(fc.Subtitle, _fontFocusSub!, subBrush, textX, by + S(22f));
            }

            return y + (cardH * 2f) + gapY;
        }

        private void DrawFocusVectorIcon(Graphics g, string type, RectangleF r, Color c)
        {
            float cx = r.X + r.Width / 2f;
            float cy = r.Y + r.Height / 2f;
            using var pen = new Pen(c, S(1.3f)) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            using var brush = new SolidBrush(c);

            switch (type)
            {
                case "fullstack": // < / >
                    float s = S(4.2f);
                    g.DrawLines(pen, new[] { new PointF(cx - s * 0.4f, cy - s * 0.8f), new PointF(cx - s * 1.2f, cy), new PointF(cx - s * 0.4f, cy + s * 0.8f) });
                    g.DrawLine(pen, cx - s * 0.2f, cy + s * 0.9f, cx + s * 0.2f, cy - s * 0.9f);
                    g.DrawLines(pen, new[] { new PointF(cx + s * 0.4f, cy - s * 0.8f), new PointF(cx + s * 1.2f, cy), new PointF(cx + s * 0.4f, cy + s * 0.8f) });
                    break;

                case "ai-int": // Brain / Neural network lobes
                    float br = S(4.5f);
                    g.DrawArc(pen, cx - br * 1.1f, cy - br * 0.9f, br * 1.1f, br * 1.2f, 120, 240);
                    g.DrawArc(pen, cx, cy - br * 0.9f, br * 1.1f, br * 1.2f, 180, 240);
                    g.DrawLine(pen, cx, cy - br * 0.8f, cx, cy + br * 0.8f);
                    g.DrawLine(pen, cx - br * 0.8f, cy, cx + br * 0.8f, cy);
                    break;

                case "ux-user": // User profile silhouette
                    float ur = S(3.0f);
                    g.DrawEllipse(pen, cx - ur, cy - ur * 1.8f, ur * 2f, ur * 2f);
                    g.DrawArc(pen, cx - ur * 1.8f, cy - ur * 0.2f, ur * 3.6f, ur * 3.2f, 190, 160);
                    break;

                case "ai-llm": // Lightning bolt
                    float ls = S(4.5f);
                    PointF[] bolt = {
                        new PointF(cx + ls * 0.2f, cy - ls),
                        new PointF(cx - ls * 0.7f, cy + ls * 0.1f),
                        new PointF(cx, cy + ls * 0.1f),
                        new PointF(cx - ls * 0.2f, cy + ls),
                        new PointF(cx + ls * 0.7f, cy - ls * 0.1f),
                        new PointF(cx, cy - ls * 0.1f)
                    };
                    g.FillPolygon(brush, bolt);
                    break;

                case "security": // Cyber shield
                    float sw = S(4.2f);
                    float sh = S(5.0f);
                    PointF[] shield = {
                        new PointF(cx - sw, cy - sh),
                        new PointF(cx + sw, cy - sh),
                        new PointF(cx + sw, cy + sh * 0.1f),
                        new PointF(cx, cy + sh),
                        new PointF(cx - sw, cy + sh * 0.1f)
                    };
                    g.DrawPolygon(pen, shield);
                    g.DrawLine(pen, cx, cy - sh * 0.6f, cx, cy + sh * 0.4f);
                    break;

                case "uiux": // Artist color palette
                    float pr = S(5.2f);
                    g.DrawEllipse(pen, cx - pr, cy - pr, pr * 2f, pr * 1.8f);
                    g.FillEllipse(brush, cx - pr * 0.4f, cy - pr * 0.3f, S(2.2f), S(2.2f));
                    g.FillEllipse(brush, cx + pr * 0.1f, cy - pr * 0.4f, S(2.2f), S(2.2f));
                    g.FillEllipse(brush, cx + pr * 0.4f, cy - pr * 0.1f, S(2.2f), S(2.2f));
                    break;
            }
        }

        // ──────────────────────────────────────────────────────
        // 7. Skills & Technologies Section (2 Rows)
        // ──────────────────────────────────────────────────────
        private float DrawSkillsSection(Graphics g, RectangleF card, float y)
        {
            float margin = S(16f);

            // Section Header: ⊞ SKILLS & TECHNOLOGIES
            float iconX = card.X + margin;
            Draw4SquaresIcon(g, iconX, y + S(5f), S(3.5f), C_CYAN);

            using var secB = new SolidBrush(C_CYAN);
            g.DrawString("SKILLS & TECHNOLOGIES", _fontSecHeader!, secB, iconX + S(14f), y);
            y += S(17f);

            // Row 1: 7 Pills (C, C++, Java, Python, SQL, React, MERN)
            float totalW = card.Width - margin * 2;
            int row1Count = 7;
            float gapX = S(6f);
            float pillW = (totalW - (row1Count - 1) * gapX) / row1Count;
            float pillH = S(24f);

            for (int i = 0; i < row1Count; i++)
            {
                var sp = _skillPills[i];
                float px = card.X + margin + i * (pillW + gapX);
                sp.Bounds = new RectangleF(px, y, pillW, pillH);
                DrawSkillPillItem(g, sp);
            }

            y += pillH + S(6f);

            // Row 2: 3 Pills (UI/UX, 3D/XR, Cybersecurity) centered
            int row2Count = 3;
            float r2PillW = S(88f);
            float r2TotalW = row2Count * r2PillW + (row2Count - 1) * gapX;
            float r2StartX = card.X + (card.Width - r2TotalW) / 2f;

            for (int i = 0; i < row2Count; i++)
            {
                var sp = _skillPills[row1Count + i];
                float px = r2StartX + i * (r2PillW + gapX);
                sp.Bounds = new RectangleF(px, y, r2PillW, pillH);
                DrawSkillPillItem(g, sp);
            }

            return y + pillH;
        }

        private static void Draw4SquaresIcon(Graphics g, float cx, float cy, float s, Color c)
        {
            using var b = new SolidBrush(c);
            g.FillRectangle(b, cx - s, cy - s, s * 0.85f, s * 0.85f);
            g.FillRectangle(b, cx + s * 0.15f, cy - s, s * 0.85f, s * 0.85f);
            g.FillRectangle(b, cx - s, cy + s * 0.15f, s * 0.85f, s * 0.85f);
            g.FillRectangle(b, cx + s * 0.15f, cy + s * 0.15f, s * 0.85f, s * 0.85f);
        }

        private void DrawSkillPillItem(Graphics g, SkillPill sp)
        {
            using var path = RoundedRect(sp.Bounds, sp.Bounds.Height / 2f);

            Color bg = sp.IsHovered ? Color.FromArgb(40, 56, 189, 248) : Color.FromArgb(16, 255, 255, 255);
            using var bgB = new SolidBrush(bg);
            g.FillPath(bgB, path);

            Color bc = sp.IsHovered ? C_CYAN : Color.FromArgb(45, 56, 189, 248);
            using var bPen = new Pen(bc, S(1f));
            g.DrawPath(bPen, path);

            // Center icon + text as a group inside the pill
            SizeF textSize = g.MeasureString(sp.Name, _fontSkillPill!);
            float iconRadius = S(4.5f);
            float iconGap = S(5f);
            float totalContentW = iconRadius * 2f + iconGap + textSize.Width;
            float startX = sp.Bounds.X + (sp.Bounds.Width - totalContentW) / 2f;
            if (startX < sp.Bounds.X + S(4f)) startX = sp.Bounds.X + S(4f);

            float iconX = startX + iconRadius;
            float iconY = sp.Bounds.Y + sp.Bounds.Height / 2f;
            DrawSkillBrandIcon(g, sp.Type, iconX, iconY);

            // Text Label
            Color tc = sp.IsHovered ? Color.White : C_TEXT_PRIMARY;
            using var tb = new SolidBrush(tc);
            var sf = new StringFormat
            {
                Alignment     = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                FormatFlags   = StringFormatFlags.NoWrap
            };
            float textX = startX + iconRadius * 2f + iconGap;
            var textRect = new RectangleF(textX, sp.Bounds.Y, sp.Bounds.Right - textX, sp.Bounds.Height);
            g.DrawString(sp.Name, _fontSkillPill!, tb, textRect, sf);
        }

        private void DrawSkillBrandIcon(Graphics g, string type, float cx, float cy)
        {
            float s = S(4.5f);
            using var cyanBrush = new SolidBrush(C_CYAN);

            switch (type)
            {
                case "c":
                    using (var cBrush = new SolidBrush(Color.FromArgb(255, 59, 130, 246)))
                        g.FillEllipse(cBrush, cx - s, cy - s, s * 2, s * 2);
                    using (var f = new Font("Segoe UI", S(6.0f), FontStyle.Bold, GraphicsUnit.Pixel))
                    using (var w = new SolidBrush(Color.White))
                        g.DrawString("C", f, w, cx - S(3f), cy - S(4.5f));
                    break;

                case "cpp":
                    using (var cBrush = new SolidBrush(Color.FromArgb(255, 37, 99, 235)))
                        g.FillEllipse(cBrush, cx - s, cy - s, s * 2, s * 2);
                    using (var f = new Font("Segoe UI", S(5.5f), FontStyle.Bold, GraphicsUnit.Pixel))
                    using (var w = new SolidBrush(Color.White))
                        g.DrawString("C+", f, w, cx - S(4f), cy - S(4.5f));
                    break;

                case "java":
                    using (var jPen = new Pen(Color.FromArgb(255, 249, 115, 22), S(1.1f)))
                    {
                        g.DrawArc(jPen, cx - s * 0.8f, cy - s * 0.5f, s * 1.4f, s * 1.4f, 0, 180);
                        g.DrawLine(jPen, cx - s * 0.8f, cy - s * 0.5f, cx + s * 0.6f, cy - s * 0.5f);
                        g.DrawArc(jPen, cx + s * 0.5f, cy - s * 0.5f, s * 0.7f, s * 0.7f, -90, 180);
                    }
                    break;

                case "python":
                    using (var pyB = new SolidBrush(Color.FromArgb(255, 59, 130, 246)))
                        g.FillEllipse(pyB, cx - s * 0.8f, cy - s * 0.8f, s * 1.1f, s * 1.1f);
                    using (var pyY = new SolidBrush(Color.FromArgb(255, 234, 179, 8)))
                        g.FillEllipse(pyY, cx - s * 0.3f, cy - s * 0.3f, s * 1.1f, s * 1.1f);
                    break;

                case "sql":
                    using (var sqlPen = new Pen(C_CYAN, S(1.1f)))
                    {
                        g.DrawEllipse(sqlPen, cx - s, cy - s * 0.8f, s * 2f, s * 0.7f);
                        g.DrawArc(sqlPen, cx - s, cy - s * 0.2f, s * 2f, s * 0.7f, 0, 180);
                        g.DrawArc(sqlPen, cx - s, cy + s * 0.4f, s * 2f, s * 0.7f, 0, 180);
                        g.DrawLine(sqlPen, cx - s, cy - s * 0.5f, cx - s, cy + s * 0.75f);
                        g.DrawLine(sqlPen, cx + s, cy - s * 0.5f, cx + s, cy + s * 0.75f);
                    }
                    break;

                case "react":
                    using (var rPen = new Pen(Color.FromArgb(255, 97, 218, 251), S(1.0f)))
                    {
                        g.DrawEllipse(rPen, cx - s, cy - s * 0.45f, s * 2f, s * 0.9f);
                        g.DrawEllipse(rPen, cx - s * 0.45f, cy - s, s * 0.9f, s * 2f);
                        g.FillEllipse(cyanBrush, cx - S(1.2f), cy - S(1.2f), S(2.4f), S(2.4f));
                    }
                    break;

                case "mern":
                    using (var leafB = new SolidBrush(Color.FromArgb(255, 34, 197, 94)))
                    {
                        PointF[] leaf = {
                            new PointF(cx, cy - s),
                            new PointF(cx + s * 0.8f, cy),
                            new PointF(cx, cy + s),
                            new PointF(cx - s * 0.4f, cy)
                        };
                        g.FillPolygon(leafB, leaf);
                    }
                    break;

                case "uiux":
                    DrawDiamondSparkle(g, cx, cy, s, Color.FromArgb(255, 236, 72, 153));
                    break;

                case "3d":
                    using (var cubePen = new Pen(C_CYAN, S(1.0f)))
                    {
                        g.DrawRectangle(cubePen, cx - s * 0.8f, cy - s * 0.8f, s * 1.6f, s * 1.6f);
                        g.DrawLine(cubePen, cx - s * 0.8f, cy - s * 0.8f, cx + s * 0.8f, cy + s * 0.8f);
                    }
                    break;

                case "cybersecurity":
                    using (var sPen = new Pen(Color.FromArgb(255, 56, 189, 248), S(1.1f)))
                    {
                        PointF[] sh = {
                            new PointF(cx - s * 0.8f, cy - s * 0.9f),
                            new PointF(cx + s * 0.8f, cy - s * 0.9f),
                            new PointF(cx + s * 0.8f, cy + s * 0.1f),
                            new PointF(cx, cy + s),
                            new PointF(cx - s * 0.8f, cy + s * 0.1f)
                        };
                        g.DrawPolygon(sPen, sh);
                    }
                    break;
            }
        }

        // ──────────────────────────────────────────────────────
        // 8. Certifications & Indian Patent Split Row (Proportional)
        // ──────────────────────────────────────────────────────
        private float DrawCertAndPatentRow(Graphics g, RectangleF card, float y)
        {
            float margin = S(16f);
            float gapX   = S(8f);
            float totalW = card.Width - margin * 2;
            float certW  = (totalW - gapX) * 0.58f;
            float patW   = totalW - certW - gapX;
            float cardH  = S(58f);

            _certCardBounds   = new RectangleF(card.X + margin, y, certW, cardH);
            _patentCardBounds = new RectangleF(card.X + margin + certW + gapX, y, patW, cardH);

            DrawCertificationsCard(g, _certCardBounds, _certCardHovered);
            DrawPatentCard(g, _patentCardBounds, _patentCardHovered);

            return y + cardH;
        }

        private void DrawCertificationsCard(Graphics g, RectangleF r, bool isHovered)
        {
            using var path = RoundedRect(r, S(8f));

            Color bg = isHovered ? Color.FromArgb(32, 245, 158, 11) : C_TILE_BG;
            using var bgB = new SolidBrush(bg);
            g.FillPath(bgB, path);

            Color bc = isHovered ? C_AMBER : C_TILE_BORDER;
            using var bPen = new Pen(bc, S(1f));
            g.DrawPath(bPen, path);

            // Gold Medal Icon in left box
            float iconBoxSz = S(32f);
            var iconBox = new RectangleF(r.X + S(8f), r.Y + (r.Height - iconBoxSz) / 2f, iconBoxSz, iconBoxSz);
            DrawGoldMedalIcon(g, iconBox);

            // Title: CERTIFICATIONS
            float textX = iconBox.Right + S(8f);
            using var headB = new SolidBrush(C_CYAN);
            g.DrawString("CERTIFICATIONS", _fontCertHeader!, headB, textX, r.Y + S(7f));

            // Body line 1: Python • AI Tools • Data Science • Power BI • Azure • Generative AI
            using var bodyB = new SolidBrush(C_TEXT_SECOND);
            g.DrawString("Python  •  AI Tools  •  Data Science  •  Power BI  •  Azure  •  Generative AI", _fontCertBody!, bodyB, textX, r.Y + S(22f));

            // Body line 2: Google Ads • WordPress
            g.DrawString("Google Ads  •  WordPress", _fontCertBody!, bodyB, textX, r.Y + S(36f));
        }

        private void DrawGoldMedalIcon(Graphics g, RectangleF r)
        {
            float cx = r.X + r.Width / 2f;
            float cy = r.Y + r.Height / 2f;
            float mr = S(11.5f);

            using (var goldBg = new SolidBrush(Color.FromArgb(30, 245, 158, 11)))
            using (var goldCirclePen = new Pen(Color.FromArgb(90, 245, 158, 11), S(1f)))
            {
                g.FillEllipse(goldBg, cx - mr, cy - mr, mr * 2, mr * 2);
                g.DrawEllipse(goldCirclePen, cx - mr, cy - mr, mr * 2, mr * 2);
            }

            float inR = S(5.5f);
            float medalCy = cy - S(1.5f);
            using var goldPen = new Pen(C_AMBER, S(1.3f));
            g.DrawEllipse(goldPen, cx - inR, medalCy - inR, inR * 2, inR * 2);
            DrawDiamondSparkle(g, cx, medalCy, S(2.5f), C_AMBER);

            using var ribPen = new Pen(C_AMBER, S(1.3f));
            g.DrawLine(ribPen, cx - inR * 0.5f, medalCy + inR * 0.7f, cx - inR * 0.8f, medalCy + inR * 1.5f);
            g.DrawLine(ribPen, cx + inR * 0.5f, medalCy + inR * 0.7f, cx + inR * 0.8f, medalCy + inR * 1.5f);
        }

        private void DrawPatentCard(Graphics g, RectangleF r, bool isHovered)
        {
            using var path = RoundedRect(r, S(8f));

            Color bg = isHovered ? Color.FromArgb(32, 56, 189, 248) : C_TILE_BG;
            using var bgB = new SolidBrush(bg);
            g.FillPath(bgB, path);

            Color bc = isHovered ? C_CYAN : C_TILE_BORDER;
            using var bPen = new Pen(bc, S(1f));
            g.DrawPath(bPen, path);

            // Cyan Lightbulb Icon in left box
            float iconBoxSz = S(32f);
            var iconBox = new RectangleF(r.X + S(8f), r.Y + (r.Height - iconBoxSz) / 2f, iconBoxSz, iconBoxSz);
            DrawLightbulbIcon(g, iconBox);

            // Title: Indian Patent
            float textX = iconBox.Right + S(8f);
            using var headB = new SolidBrush(Color.White);
            g.DrawString("Indian Patent", _fontPatentTitle!, headB, textX, r.Y + S(7f));

            // App No: App. No. 202441033032
            using var appB = new SolidBrush(C_CYAN);
            g.DrawString("App. No. 202441033032", _fontPatentApp!, appB, textX, r.Y + S(22f));

            // Description: IR-based Android TV control
            using var descB = new SolidBrush(C_TEXT_MUTED);
            g.DrawString("IR-based Android TV control", _fontPatentDesc!, descB, textX, r.Y + S(36f));
        }

        private void DrawLightbulbIcon(Graphics g, RectangleF r)
        {
            float cx = r.X + r.Width / 2f;
            float cy = r.Y + r.Height / 2f;
            float mr = S(11.5f);

            // Circular cyan badge container matching gold medal
            using (var bulbBg = new SolidBrush(Color.FromArgb(30, 56, 189, 248)))
            using (var bulbCirclePen = new Pen(Color.FromArgb(90, 56, 189, 248), S(1f)))
            {
                g.FillEllipse(bulbBg, cx - mr, cy - mr, mr * 2, mr * 2);
                g.DrawEllipse(bulbCirclePen, cx - mr, cy - mr, mr * 2, mr * 2);
            }

            float lr = S(5.5f);
            using var bulbPen = new Pen(C_CYAN, S(1.3f)) { StartCap = LineCap.Round, EndCap = LineCap.Round };

            // Bulb top dome
            g.DrawArc(bulbPen, cx - lr, cy - lr * 1.15f, lr * 2f, lr * 2f, 150, 240);

            // Base screw threads
            g.DrawLine(bulbPen, cx - lr * 0.45f, cy + lr * 0.55f, cx + lr * 0.45f, cy + lr * 0.55f);
            g.DrawLine(bulbPen, cx - lr * 0.35f, cy + lr * 0.85f, cx + lr * 0.35f, cy + lr * 0.85f);

            // Internal filament
            g.DrawLine(bulbPen, cx, cy - lr * 0.6f, cx, cy + lr * 0.2f);
        }

        // ──────────────────────────────────────────────────────
        // 9. Connect With Me (6 Platform Buttons)
        // ──────────────────────────────────────────────────────
        private float DrawConnectSection(Graphics g, RectangleF card, float y)
        {
            float margin = S(16f);

            // Section Header: 🔗 CONNECT WITH ME
            float iconX = card.X + margin;
            DrawChainLinkIcon(g, iconX, y + S(5f), S(3.5f), C_CYAN);

            using var secB = new SolidBrush(C_CYAN);
            g.DrawString("CONNECT WITH ME", _fontSecHeader!, secB, iconX + S(14f), y);
            y += S(17f);

            // 6 Buttons side-by-side
            float totalW = card.Width - margin * 2;
            int count = _connectButtons.Count;
            float gapX = S(6f);
            float btnW = (totalW - (count - 1) * gapX) / count;
            float btnH = S(48f);

            var sf = new StringFormat
            {
                Alignment     = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                FormatFlags   = StringFormatFlags.NoWrap
            };

            for (int i = 0; i < count; i++)
            {
                var b = _connectButtons[i];
                float bx = card.X + margin + i * (btnW + gapX);
                b.Bounds = new RectangleF(bx, y, btnW, btnH);

                using var path = RoundedRect(b.Bounds, S(8f));

                Color bg = b.IsHovered ? Color.FromArgb(35, 56, 189, 248) : C_TILE_BG;
                using var bgB = new SolidBrush(bg);
                g.FillPath(bgB, path);

                Color bc = b.IsHovered ? C_CYAN : C_TILE_BORDER;
                using var bPen = new Pen(bc, S(1f));
                g.DrawPath(bPen, path);

                // Top Vector Platform Icon
                var iconRect = new RectangleF(bx, y + S(4f), btnW, S(16f));
                DrawConnectPlatformIcon(g, b.Type, iconRect);

                // Handle Label (e.g. TheOrionGD or the-orion-gd.vercel.app)
                Font handleFont = (b.Handle.Length > 15) ? _fontConnectHandleCompact! : _fontConnectHandle!;
                Color hc = b.IsHovered ? Color.White : C_TEXT_PRIMARY;
                using var hb = new SolidBrush(hc);
                var handleRect = new RectangleF(bx + S(1f), y + S(21f), btnW - S(2f), S(12f));
                g.DrawString(b.Handle, handleFont, hb, handleRect, sf);

                // Platform Sublabel (e.g. GitHub)
                using var pb = new SolidBrush(C_TEXT_MUTED);
                var platRect = new RectangleF(bx, y + S(33f), btnW, S(11f));
                g.DrawString(b.Platform, _fontConnectPlatform!, pb, platRect, sf);
            }

            return y + btnH;
        }

        private static void DrawChainLinkIcon(Graphics g, float cx, float cy, float s, Color c)
        {
            using var p = new Pen(c, 1.2f);
            g.DrawEllipse(p, cx - s * 0.9f, cy - s * 0.5f, s * 1.1f, s * 1.0f);
            g.DrawEllipse(p, cx - s * 0.1f, cy - s * 0.5f, s * 1.1f, s * 1.0f);
        }

        private void DrawConnectPlatformIcon(Graphics g, string type, RectangleF r)
        {
            float cx = r.X + r.Width / 2f;
            float cy = r.Y + r.Height / 2f;
            float s = S(4.5f);

            switch (type)
            {
                case "github":
                    using (var pen = new Pen(Color.White, S(1.2f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    {
                        g.DrawLines(pen, new[] { new PointF(cx - s * 0.4f, cy - s * 0.8f), new PointF(cx - s * 1.2f, cy), new PointF(cx - s * 0.4f, cy + s * 0.8f) });
                        g.DrawLine(pen, cx - s * 0.2f, cy + s * 0.9f, cx + s * 0.2f, cy - s * 0.9f);
                        g.DrawLines(pen, new[] { new PointF(cx + s * 0.4f, cy - s * 0.8f), new PointF(cx + s * 1.2f, cy), new PointF(cx + s * 0.4f, cy + s * 0.8f) });
                    }
                    break;

                case "linkedin":
                    float inW = S(12f);
                    float inH = S(12f);
                    using (var inBg = new SolidBrush(Color.FromArgb(255, 14, 118, 168)))
                    {
                        using var inPath = RoundedRect(new RectangleF(cx - inW / 2, cy - inH / 2, inW, inH), S(2.5f));
                        g.FillPath(inBg, inPath);
                    }
                    using (var inFont = new Font("Segoe UI", S(7.5f), FontStyle.Bold, GraphicsUnit.Pixel))
                    using (var wBrush = new SolidBrush(Color.White))
                    {
                        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString("in", inFont, wBrush, new RectangleF(cx - inW / 2, cy - inH / 2, inW, inH), sf);
                    }
                    break;

                case "hackerrank":
                    float hs = S(5.2f);
                    using (var hBg = new SolidBrush(Color.FromArgb(255, 34, 197, 94)))
                    {
                        PointF[] hex = {
                            new PointF(cx, cy - hs),
                            new PointF(cx + hs * 0.85f, cy - hs * 0.5f),
                            new PointF(cx + hs * 0.85f, cy + hs * 0.5f),
                            new PointF(cx, cy + hs),
                            new PointF(cx - hs * 0.85f, cy + hs * 0.5f),
                            new PointF(cx - hs * 0.85f, cy - hs * 0.5f)
                        };
                        g.FillPolygon(hBg, hex);
                    }
                    using (var hPen = new Pen(Color.White, S(1.1f)))
                    {
                        g.DrawLine(hPen, cx - hs * 0.45f, cy - hs * 0.5f, cx - hs * 0.45f, cy + hs * 0.5f);
                        g.DrawLine(hPen, cx + hs * 0.45f, cy - hs * 0.5f, cx + hs * 0.45f, cy + hs * 0.5f);
                        g.DrawLine(hPen, cx - hs * 0.45f, cy, cx + hs * 0.45f, cy);
                    }
                    break;

                case "portfolio":
                    using (var globePen = new Pen(C_CYAN, S(1.1f)))
                    {
                        g.DrawEllipse(globePen, cx - s, cy - s, s * 2f, s * 2f);
                        g.DrawEllipse(globePen, cx - s * 0.45f, cy - s, s * 0.9f, s * 2f);
                        g.DrawLine(globePen, cx - s, cy, cx + s, cy);
                    }
                    break;

                case "play":
                    PointF[] playPts = {
                        new PointF(cx - s * 0.8f, cy - s),
                        new PointF(cx + s * 1.1f, cy),
                        new PointF(cx - s * 0.8f, cy + s)
                    };
                    using (var playBrush = new LinearGradientBrush(
                        new PointF(cx - s, cy - s),
                        new PointF(cx + s, cy + s),
                        Color.FromArgb(255, 99, 102, 241),
                        Color.FromArgb(255, 56, 189, 248)))
                    {
                        g.FillPolygon(playBrush, playPts);
                    }
                    break;

                case "youtube":
                    float ytW = S(14f);
                    float ytH = S(10f);
                    using (var ytBg = new SolidBrush(Color.FromArgb(255, 239, 68, 68)))
                    {
                        using var ytPath = RoundedRect(new RectangleF(cx - ytW / 2f, cy - ytH / 2f, ytW, ytH), S(2.5f));
                        g.FillPath(ytBg, ytPath);
                    }
                    PointF[] ytTri = {
                        new PointF(cx - S(2.0f), cy - S(2.8f)),
                        new PointF(cx + S(2.8f), cy),
                        new PointF(cx - S(2.0f), cy + S(2.8f))
                    };
                    using (var wB = new SolidBrush(Color.White))
                    {
                        g.FillPolygon(wB, ytTri);
                    }
                    break;
            }
        }

        // ──────────────────────────────────────────────────────
        // 10. Footer Bar
        // ──────────────────────────────────────────────────────
        private void DrawFooterBar(Graphics g, RectangleF card, float y)
        {
            float margin = S(18f);

            // Left: > Always learning. Always building.
            using var chevB = new SolidBrush(C_CYAN);
            g.DrawString(">", _fontFooterQuote!, chevB, card.X + margin, y);

            using var quoteB = new SolidBrush(C_TEXT_MUTED);
            g.DrawString("Always learning. Always building.", _fontFooterQuote!, quoteB, card.X + margin + S(12f), y);

            // Right: /// ORIONGD × CSE 23 A
            float rightX = card.Right - margin;
            var sf = new StringFormat
            {
                Alignment     = StringAlignment.Far,
                LineAlignment = StringAlignment.Near
            };
            using var tagB = new SolidBrush(Color.FromArgb(210, 56, 189, 248));
            g.DrawString("ORIONGD   ×   CSE 23 A", _fontFooterTag!, tagB, rightX, y, sf);

            float strW = g.MeasureString("ORIONGD   ×   CSE 23 A", _fontFooterTag!).Width;
            float stripeStartX = rightX - strW - S(24f);

            using var stripePen = new Pen(C_CYAN, S(1.4f)) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            for (int i = 0; i < 3; i++)
            {
                float sx = stripeStartX + i * S(4.5f);
                g.DrawLine(stripePen, sx, y + S(9f), sx + S(3.5f), y + S(1f));
            }
        }

        // ──────────────────────────────────────────────────────
        private static GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            var path = new GraphicsPath();
            path.AddArc(r.X,              r.Y,              d, d, 180, 90);
            path.AddArc(r.Right - d,      r.Y,              d, d, 270, 90);
            path.AddArc(r.Right - d,      r.Bottom - d,     d, d,   0, 90);
            path.AddArc(r.X,              r.Bottom - d,     d, d,  90, 90);
            path.CloseFigure();
            return path;
        }

        private static void DrawDiamondSparkle(Graphics g, float cx, float cy, float r, Color c)
        {
            using var b = new SolidBrush(c);
            PointF[] pts = {
                new PointF(cx, cy - r),
                new PointF(cx + r * 0.3f, cy - r * 0.3f),
                new PointF(cx + r, cy),
                new PointF(cx + r * 0.3f, cy + r * 0.3f),
                new PointF(cx, cy + r),
                new PointF(cx - r * 0.3f, cy + r * 0.3f),
                new PointF(cx - r, cy),
                new PointF(cx - r * 0.3f, cy - r * 0.3f)
            };
            g.FillPolygon(b, pts);
        }

        // ══════════════════════════════════════════════════════
        //  CLEANUP
        // ══════════════════════════════════════════════════════
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animTimer?.Stop();
                _animTimer?.Dispose();
                _trayIcon?.Dispose();
                _contextMenu?.Dispose();
                _tooltip.Dispose();

                _fontTopHeader?.Dispose();
                _fontTopSub?.Dispose();
                _fontHeroTag?.Dispose();
                _fontHeroSub?.Dispose();
                _fontAvailStatus?.Dispose();
                _fontAvailSub?.Dispose();
                _fontBrand?.Dispose();
                _fontName?.Dispose();
                _fontRolePill?.Dispose();
                _fontBio?.Dispose();
                _fontSecHeader?.Dispose();
                _fontFocusTitle?.Dispose();
                _fontFocusSub?.Dispose();
                _fontSkillPill?.Dispose();
                _fontCertHeader?.Dispose();
                _fontCertBody?.Dispose();
                _fontPatentTitle?.Dispose();
                _fontPatentApp?.Dispose();
                _fontPatentDesc?.Dispose();
                _fontConnectHandle?.Dispose();
                _fontConnectHandleCompact?.Dispose();
                _fontConnectPlatform?.Dispose();
                _fontFooterQuote?.Dispose();
                _fontFooterTag?.Dispose();
                _fontMonogram?.Dispose();

                CleanupDib();
            }
            base.Dispose(disposing);
        }

        // ══════════════════════════════════════════════════════
        //  WIN32 INTEROP
        // ══════════════════════════════════════════════════════
        private const int WS_EX_LAYERED  = 0x00080000;
        private const int WM_NCHITTEST   = 0x0084;
        private const int HTCLIENT       = 1;
        private const int HTTRANSPARENT  = -1;

        private const byte AC_SRC_OVER   = 0x00;
        private const byte AC_SRC_ALPHA  = 0x01;
        private const uint ULW_ALPHA     = 0x00000002;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SIZE
        {
            public int CX;
            public int CY;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public uint   biSize;
            public int    biWidth;
            public int    biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint   biCompression;
            public uint   biSizeImage;
            public int    biXPelsPerMeter;
            public int    biYPelsPerMeter;
            public uint   biClrUsed;
            public uint   biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
            public uint             bmiColors;
        }

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool UpdateLayeredWindow(
            IntPtr hwnd,
            IntPtr hdcDst,
            ref POINT pptDst,
            ref SIZE psize,
            IntPtr hdcSrc,
            ref POINT pptSrc,
            uint crKey,
            [In] ref BLENDFUNCTION pblend,
            uint dwFlags);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true)]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateDIBSection(
            IntPtr hdc,
            [In] ref BITMAPINFO pbmi,
            uint iUsage,
            out IntPtr ppvBits,
            IntPtr hSection,
            uint dwOffset);
    }

    // ══════════════════════════════════════════════════════════
    //  Modern Glassmorphic About Dialog
    // ══════════════════════════════════════════════════════
    internal class AboutDialog : Form
    {
        public AboutDialog()
        {
            Text            = "About OrionGD Identity";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            Width           = 380;
            Height          = 360;
            BackColor       = Color.FromArgb(255, 6, 9, 19);
            ForeColor       = Color.FromArgb(255, 241, 245, 249);
            ShowInTaskbar   = false;
            DoubleBuffered  = true;
            Font            = new Font("Segoe UI", 9f);

            var panel = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            panel.Paint += PaintAbout;
            Controls.Add(panel);

            var closeBtn = new Button
            {
                Text      = "Close",
                Width     = 100,
                Height    = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 12, 18, 34),
                ForeColor = Color.FromArgb(255, 56, 189, 248),
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                DialogResult = DialogResult.OK
            };
            closeBtn.FlatAppearance.BorderColor = Color.FromArgb(255, 30, 58, 95);
            closeBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 30, 48, 75);
            closeBtn.Click += (s, e) => Close();
            closeBtn.Location = new Point((Width - closeBtn.Width) / 2, Height - closeBtn.Height - 34);
            Controls.Add(closeBtn);
        }

        private void PaintAbout(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            var w = ((Control)sender!).Width;
            var sf = new StringFormat
            {
                Alignment     = StringAlignment.Center,
                LineAlignment = StringAlignment.Near
            };

            float y = 20f;

            using var f1 = new Font("Segoe UI", 18f, FontStyle.Bold);
            using var b1 = new SolidBrush(Color.FromArgb(255, 56, 189, 248));
            g.DrawString("ORIONGD", f1, b1, new RectangleF(0, y, w, 32), sf);
            y += 32f;

            using var f2 = new Font("Segoe UI", 12f, FontStyle.Bold);
            using var b2 = new SolidBrush(Color.FromArgb(255, 248, 250, 252));
            g.DrawString("Godfrey T. R", f2, b2, new RectangleF(0, y, w, 24), sf);
            y += 24f;

            using var f3 = new Font("Segoe UI", 9.5f);
            using var b3 = new SolidBrush(Color.FromArgb(255, 148, 163, 184));
            g.DrawString("Software Engineer  •  CSE 23 A", f3, b3, new RectangleF(0, y, w, 20), sf);
            y += 24f;

            using var sepPen = new Pen(Color.FromArgb(120, 56, 189, 248), 1.2f);
            g.DrawLine(sepPen, 40, y, w - 40, y);
            y += 12f;

            using var f4 = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            using var b4 = new SolidBrush(Color.FromArgb(255, 186, 230, 253));
            g.DrawString("Full-Stack  •  AI & LLMs  •  Cybersecurity  •  UI/UX", f4, b4,
                new RectangleF(0, y, w, 20), sf);
            y += 24f;

            using var f5 = new Font("Segoe UI", 8.5f);
            using var b5 = new SolidBrush(Color.FromArgb(255, 148, 163, 184));
            g.DrawString("Official OrionGD Desktop Identity Dashboard.\nIndian Patent App. No. 202441033032.\nCrafted with hardware-accelerated 32bpp per-pixel transparency.",
                f5, b5, new RectangleF(24, y, w - 48, 48), sf);
            y += 48f;

            string versionStr = $"v{Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "2.0.0"}";
            using var f6 = new Font("Segoe UI", 8f);
            using var b6 = new SolidBrush(Color.FromArgb(255, 100, 116, 139));
            g.DrawString($"Version {versionStr}  •  TheOrionGD Ecosystem", f6, b6,
                new RectangleF(0, y, w, 20), sf);
        }
    }
}
