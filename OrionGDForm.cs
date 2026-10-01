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

    internal class ExperienceCard
    {
        public RectangleF Bounds;
        public string     Company;
        public string     Role;
        public string     Detail;
        public string     Period;
        public bool       IsHovered;

        public ExperienceCard(string company, string role, string detail, string period)
        {
            Company = company;
            Role    = role;
            Detail  = detail;
            Period  = period;
        }
    }

    internal class BackgroundParticle
    {
        public float X;
        public float Y;
        public float Vx;
        public float Vy;
        public float Radius;
        public float BaseAlpha;
        public float Phase;
        public float PulseSpeed;
        public Color Color;
        public bool  IsHub;
    }

    // ══════════════════════════════════════════════════════════
    //  Main OrionGD Identity Dashboard Widget (32bpp Layered Window)
    // ══════════════════════════════════════════════════════════
    public class OrionGDForm : Form
    {
        // ── Base Canvas Dimensions (96 DPI Reference) ──────────
        private const float BaseCardW = 530f;
        private const float BaseCardH = 800f;
        private const float BaseCardR = 22f;
        private const float BasePad   = 22f;

        // ── Curated Futuristic Industrial Palette (Red + Charcoal + Black)
        private static readonly Color C_ENV_BLACK      = Color.FromArgb(255, 8,   9,   11);  // #08090B
        private static readonly Color C_BG_MAIN        = Color.FromArgb(255, 13,  15,  17);  // #0D0F11
        private static readonly Color C_CHARCOAL_MAIN  = Color.FromArgb(255, 17,  19,  21);  // #111315
        private static readonly Color C_CHARCOAL_SEC   = Color.FromArgb(255, 24,  26,  29);  // #181A1D
        private static readonly Color C_SURFACE_ELEV   = Color.FromArgb(255, 30,  33,  37);  // #1E2125
        private static readonly Color C_SURFACE_HL     = Color.FromArgb(255, 36,  39,  43);  // #24272B

        // ── Red Energy Accent System (10% Ratio)
        private static readonly Color C_RED_PRIMARY    = Color.FromArgb(255, 229, 57,  53);  // #E53935
        private static readonly Color C_RED_SIGNAL     = Color.FromArgb(255, 255, 59,  48);  // #FF3B30
        private static readonly Color C_RED_CRIMSON    = Color.FromArgb(255, 139, 30,  35);  // #8B1E23
        private static readonly Color C_RED_DARK       = Color.FromArgb(255, 53,  23,  26);  // #35171A
        private static readonly Color C_RED_BORDER     = Color.FromArgb(255, 111, 36,  40);  // #6F2428

        // ── Typography System
        private static readonly Color C_TEXT_PRIMARY   = Color.FromArgb(255, 242, 242, 242); // #F2F2F2
        private static readonly Color C_TEXT_SECOND    = Color.FromArgb(255, 167, 170, 174); // #A7AAAE
        private static readonly Color C_TEXT_CHIP      = Color.FromArgb(255, 213, 215, 218); // #D5D7DA
        private static readonly Color C_TEXT_MUTED     = Color.FromArgb(255, 104, 109, 115); // #686D73
        private static readonly Color C_TEXT_DISABLED  = Color.FromArgb(255, 69,  72,  76);  // #45484C

        // ── Industrial Borders
        private static readonly Color C_BORDER_NORMAL  = Color.FromArgb(255, 48,  51,  56);  // #303338
        private static readonly Color C_BORDER_SUBTLE  = Color.FromArgb(255, 37,  40,  44);  // #25282C
        private static readonly Color C_BORDER_ACTIVE  = Color.FromArgb(255, 111, 36,  40);  // #6F2428
        private static readonly Color C_BORDER_HL      = Color.FromArgb(255, 229, 57,  53);  // #E53935

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
        private readonly List<FocusCard>          _focusCards      = new();
        private readonly List<SkillPill>          _skillPills      = new();
        private readonly List<ExperienceCard>     _experienceCards = new();
        private readonly List<ConnectButton>      _connectButtons  = new();
        private readonly List<BackgroundParticle> _particles        = new();
        private readonly Random                   _rng             = new Random(1337);
        private readonly ToolTip                  _tooltip         = new ToolTip();
        private string                        _activeTooltip   = string.Empty;

        // Hitboxes
        private RectangleF _btnCloseBounds;
        private RectangleF _btnMinBounds;
        private RectangleF _availablePillBounds;
        private RectangleF _certCardBounds;
        private RectangleF _projectsCardBounds;

        private bool _btnCloseHovered;
        private bool _btnMinHovered;
        private bool _availablePillHovered;
        private bool _certCardHovered;
        private bool _projectsCardHovered;

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
        private Font? _fontExpCompany;
        private Font? _fontExpRole;
        private Font? _fontExpDetail;
        private Font? _fontExpPeriod;
        private Font? _fontCertHeader;
        private Font? _fontCertBody;
        private Font? _fontProjectsHeader;
        private Font? _fontProjectsBody;
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
            InitParticles();
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
            Text            = "TheOrionGD Identity Dashboard";
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
            _fontExpCompany?.Dispose();
            _fontExpRole?.Dispose();
            _fontExpDetail?.Dispose();
            _fontExpPeriod?.Dispose();
            _fontCertHeader?.Dispose();
            _fontCertBody?.Dispose();
            _fontProjectsHeader?.Dispose();
            _fontProjectsBody?.Dispose();
            _fontConnectHandle?.Dispose();
            _fontConnectHandleCompact?.Dispose();
            _fontConnectPlatform?.Dispose();
            _fontFooterQuote?.Dispose();
            _fontFooterTag?.Dispose();
            _fontMonogram?.Dispose();

            _fontTopHeader            = new Font("Segoe UI", S(11.2f), FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontTopSub               = new Font("Segoe UI", S(8.6f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontHeroTag              = new Font("Segoe UI", S(10.8f), FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontHeroSub              = new Font("Segoe UI", S(8.0f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontAvailStatus          = new Font("Segoe UI", S(9.2f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontAvailSub             = new Font("Segoe UI", S(8.0f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontBrand                = new Font("Segoe UI", S(23.5f), FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontName                 = new Font("Segoe UI", S(17.5f), FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontRolePill             = new Font("Segoe UI", S(10.0f), FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontBio                  = new Font("Segoe UI", S(10.6f), FontStyle.Regular, GraphicsUnit.Pixel);
            _fontSecHeader            = new Font("Segoe UI", S(10.0f), FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontFocusTitle           = new Font("Segoe UI", S(10.8f), FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontFocusSub             = new Font("Segoe UI", S(8.8f),  FontStyle.Regular, GraphicsUnit.Pixel);
            _fontSkillPill            = new Font("Segoe UI", S(9.5f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontExpCompany           = new Font("Segoe UI", S(9.8f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontExpRole              = new Font("Segoe UI", S(8.4f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontExpDetail            = new Font("Segoe UI", S(7.8f),  FontStyle.Regular, GraphicsUnit.Pixel);
            _fontExpPeriod            = new Font("Segoe UI", S(7.4f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontCertHeader           = new Font("Segoe UI", S(10.0f), FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontCertBody             = new Font("Segoe UI", S(8.4f),  FontStyle.Regular, GraphicsUnit.Pixel);
            _fontProjectsHeader       = new Font("Segoe UI", S(10.0f), FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontProjectsBody         = new Font("Segoe UI", S(8.4f),  FontStyle.Regular, GraphicsUnit.Pixel);
            _fontConnectHandle        = new Font("Segoe UI", S(9.5f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontConnectHandleCompact = new Font("Segoe UI", S(7.6f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontConnectPlatform      = new Font("Segoe UI", S(8.0f),  FontStyle.Regular, GraphicsUnit.Pixel);
            _fontFooterQuote          = new Font("Segoe UI", S(9.6f),  FontStyle.Regular, GraphicsUnit.Pixel);
            _fontFooterTag            = new Font("Segoe UI", S(9.4f),  FontStyle.Bold,    GraphicsUnit.Pixel);
            _fontMonogram             = new Font("Segoe UI", S(16.5f), FontStyle.Bold,    GraphicsUnit.Pixel);
        }

        // ──────────────────────────────────────────────────────
        private void InitElements()
        {
            // 6 Focus Areas (3x2 Grid) - Unified Red Energy Accent
            _focusCards.Clear();
            _focusCards.Add(new FocusCard("fullstack", "Java Full-Stack", "Spring Boot • React • JDBC",  C_RED_SIGNAL));
            _focusCards.Add(new FocusCard("ai-int",    "Multi-Agent AI",  "LLMs • RAG • Autonomous",     C_RED_SIGNAL));
            _focusCards.Add(new FocusCard("vision",    "Edge-AI & Vision","FastAPI • OpenCV • ONNX",     C_RED_SIGNAL));
            _focusCards.Add(new FocusCard("backend",   "Backend & APIs",  "REST APIs • Docker • Micro",  C_RED_SIGNAL));
            _focusCards.Add(new FocusCard("security",  "Security & Cloud","Zero Trust • JWT • Auth",     C_RED_SIGNAL));
            _focusCards.Add(new FocusCard("mobile",    "Web & Mobile",    "Next.js • React Native • UI", C_RED_SIGNAL));

            // 10 Skills & Technologies Pills (2 rows of 5)
            _skillPills.Clear();
            _skillPills.Add(new SkillPill("java",          "Java"));
            _skillPills.Add(new SkillPill("python",        "Python"));
            _skillPills.Add(new SkillPill("spring",        "Spring Boot"));
            _skillPills.Add(new SkillPill("react",         "React"));
            _skillPills.Add(new SkillPill("cpp",           "C / C++"));
            _skillPills.Add(new SkillPill("node",          "Node.js"));
            _skillPills.Add(new SkillPill("fastapi",       "FastAPI"));
            _skillPills.Add(new SkillPill("sql",           "SQL"));
            _skillPills.Add(new SkillPill("mongo",         "MongoDB"));
            _skillPills.Add(new SkillPill("cybersecurity", "Cybersecurity"));

            // 3 Industry Internship Experiences from Resume
            _experienceCards.Clear();
            _experienceCards.Add(new ExperienceCard("VDART Academy",    "Full Stack Intern (OJT)", "15+ APIs • OJT Delivery • +35% Spd", "Jan 2026"));
            _experienceCards.Add(new ExperienceCard("Prodigy InfoTech", "Web Development Intern", "Weather App • -30% Load • Vanilla JS", "Aug–Sep '24"));
            _experienceCards.Add(new ExperienceCard("Adaovi",           "CyberSecurity Intern",    "Pentest • Auth Hardening • Patching",  "Jun–Jul '24"));

            // 5 Connect Platform Buttons (Play Console removed, larger 95px touchpoints)
            _connectButtons.Clear();
            _connectButtons.Add(new ConnectButton("github",      "TheOrionGD",              "GitHub",       "https://github.com/TheOrionGD",              "GitHub / TheOrionGD"));
            _connectButtons.Add(new ConnectButton("linkedin",    "theoriongd",              "LinkedIn",     "https://www.linkedin.com/in/theoriongd/",    "LinkedIn / theoriongd"));
            _connectButtons.Add(new ConnectButton("portfolio",   "the-orion-gd.vercel.app", "Portfolio",    "https://the-orion-gd.vercel.app/",          "Official Portfolio Website"));
            _connectButtons.Add(new ConnectButton("hackerrank",  "OrionGD07",               "HackerRank",   "https://www.hackerrank.com/OrionGD07",      "HackerRank / OrionGD07"));
            _connectButtons.Add(new ConnectButton("youtube",     "theoriongd",              "YouTube",      "https://www.youtube.com/@theoriongd",       "YouTube / @theoriongd"));
        }

        // ──────────────────────────────────────────────────────
        private void BuildTrayAndMenu()
        {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add("Show / Hide Dashboard", null, (s, e) => ToggleVisibility());
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add("Open Portfolio Website", null, (s, e) => OpenUrl("https://the-orion-gd.vercel.app/"));
            _contextMenu.Items.Add("Open GitHub Profile",    null, (s, e) => OpenUrl("https://github.com/TheOrionGD"));
            _contextMenu.Items.Add("Open LinkedIn Profile",  null, (s, e) => OpenUrl("https://www.linkedin.com/in/theoriongd/"));
            _contextMenu.Items.Add("Open HackerRank Profile",null, (s, e) => OpenUrl("https://www.hackerrank.com/OrionGD07"));
            _contextMenu.Items.Add("Open YouTube Channel",   null, (s, e) => OpenUrl("https://www.youtube.com/@theoriongd"));
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add("About TheOrionGD Identity", null, (s, e) => ShowAboutDialog());
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add("Exit",                   null, (s, e) => Close());

            _contextMenu.BackColor       = C_CHARCOAL_MAIN;
            _contextMenu.ForeColor       = C_TEXT_PRIMARY;
            _contextMenu.ShowImageMargin = false;

            Icon trayIcon = CreateBrandIcon();
            _trayIcon = new NotifyIcon
            {
                Text             = "TheOrionGD Identity Dashboard",
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

                using var bgBrush = new SolidBrush(C_CHARCOAL_MAIN);
                g.FillEllipse(bgBrush, 1, 1, size - 2, size - 2);

                using var ringPen = new Pen(C_RED_SIGNAL, 1.5f);
                g.DrawEllipse(ringPen, 2, 2, size - 4, size - 4);

                using var font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
                using var textBrush = new SolidBrush(C_TEXT_PRIMARY);
                var sf = new StringFormat
                {
                    Alignment     = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString("OGD", font, textBrush, new RectangleF(0, 0, size, size), sf);
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

            UpdateParticles();

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
                    _projectsCardBounds.Contains(e.Location))
                    return;

                foreach (var fc in _focusCards)
                    if (fc.Bounds.Contains(e.Location)) return;

                foreach (var p in _skillPills)
                    if (p.Bounds.Contains(e.Location)) return;

                foreach (var exp in _experienceCards)
                    if (exp.Bounds.Contains(e.Location)) return;

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

            bool newProjects = _projectsCardBounds.Contains(pt);
            if (newProjects != _projectsCardHovered) { _projectsCardHovered = newProjects; repaint = true; }
            if (newProjects) { anyHover = true; hoveredTooltip = "Featured Projects: Connify App & FaceShield-Authentication"; }

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

            foreach (var exp in _experienceCards)
            {
                bool wasH = exp.IsHovered;
                exp.IsHovered = exp.Bounds.Contains(pt);
                if (exp.IsHovered != wasH) repaint = true;
                if (exp.IsHovered)
                {
                    anyHover = true;
                    hoveredTooltip = $"{exp.Company} ({exp.Period}) — {exp.Role}: {exp.Detail}";
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

            if (_availablePillBounds.Contains(pt) || _certCardBounds.Contains(pt) || _projectsCardBounds.Contains(pt))
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

            foreach (var exp in _experienceCards)
            {
                if (exp.Bounds.Contains(pt))
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
            InitParticles();
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
            dlg.TopMost = true;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
            BringToFront();
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

            // 3. Top Header Bar: "THEORION IDENTITY / BUILD — INNOVATE — IMPACT" & Window buttons
            DrawTopHeaderBar(g, card);

            // 4. Hero Section: Left Winged Logo + Center Holographic Crest + Right "AVAILABLE" pill
            float heroCY = card.Y + S(88f);
            DrawHeroSection(g, card, heroCY);

            // 5. Identity Titles: THEORIONGD, Godfrey T. R, [JAVA FULL STACK & SOFTWARE ENGINEER], Bio
            float idY = card.Y + S(142f);
            idY = DrawIdentityTitles(g, card, idY);

            // 6. Focus Areas 3x2 Grid
            float focusY = idY + S(14f);
            focusY = DrawFocusGrid(g, card, focusY);

            // 7. Skills & Technologies Section (2x5 Grid)
            float skillsY = focusY + S(14f);
            skillsY = DrawSkillsSection(g, card, skillsY);

            // 8. Internship Experience (3-Card Row from Resume)
            float expY = skillsY + S(14f);
            expY = DrawExperienceSection(g, card, expY);

            // 9. Certifications & Featured Projects Row
            float certY = expY + S(14f);
            certY = DrawCertAndProjectsRow(g, card, certY);

            // 10. Connect With Me 5 Platform Buttons
            float connectY = certY + S(14f);
            connectY = DrawConnectSection(g, card, connectY);

            // 11. Footer Bar: "> Always learning. Always building." and "/// THEORIONGD × OGD"
            DrawFooterBar(g, card, card.Bottom - S(26f));
        }

        // ──────────────────────────────────────────────────────
        // 1. Ambient Glow & Shadows
        // ──────────────────────────────────────────────────────
        private void DrawAmbientGlow(Graphics g, RectangleF card, float r)
        {
            // Subtle 0 20px 80px rgba(0,0,0,0.45) shadow
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

            // Subtle red ambient glow: 0 0 40px rgba(255, 59, 48, 0.08)
            float glowBright = (float)(0.06 + 0.02 * Math.Sin(_glowPulse));
            int ga = Math.Clamp((int)(glowBright * 255), 0, 255);
            using var glowBrush = new SolidBrush(Color.FromArgb(ga, C_RED_SIGNAL));
            float glowSpread = S(4.0f);
            var glowRect = new RectangleF(
                card.X - glowSpread,
                card.Y - glowSpread,
                card.Width + glowSpread * 2,
                card.Height + glowSpread * 2);
            using var glowPath = RoundedRect(glowRect, r + glowSpread);
            g.FillPath(glowBrush, glowPath);
        }

        // ──────────────────────────────────────────────────────
        // 2. Glassmorphic Card Surface & Industrial Red Border
        // ──────────────────────────────────────────────────────
        private void DrawCardSurface(Graphics g, RectangleF card, float r)
        {
            using var cardPath = RoundedRect(card, r);

            // Main Charcoal / Black Industrial background
            using var bgBrush = new LinearGradientBrush(
                new PointF(card.X, card.Y),
                new PointF(card.Right, card.Bottom),
                C_CHARCOAL_MAIN, C_BG_MAIN);
            g.FillPath(bgBrush, cardPath);

            // Dynamic Particle Constellation Field (Clipped within card bounds)
            var prevClip = g.Clip;
            g.SetClip(cardPath, CombineMode.Intersect);
            DrawParticleField(g, card);
            g.Clip = prevClip;

            // Very subtle specular top sheen
            var sheenRect = new RectangleF(card.X, card.Y, card.Width, S(36f));
            using var sheenPath = RoundedRect(sheenRect, r);
            using var sheenBrush = new LinearGradientBrush(
                new PointF(card.X, card.Y),
                new PointF(card.X, card.Y + S(36f)),
                Color.FromArgb(12, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255));
            g.FillPath(sheenBrush, sheenPath);

            // 1px solid #303338 base border
            using var baseBorderPen = new Pen(C_BORDER_NORMAL, S(1.0f));
            g.DrawPath(baseBorderPen, cardPath);

            // Precision HUD top laser accent line (#6F2428 with #FF3B30 center highlight)
            using var gradRimBrush = new LinearGradientBrush(
                new PointF(card.X, card.Y),
                new PointF(card.Right, card.Y),
                Color.Transparent, Color.Transparent);

            var rimCb = new ColorBlend(4)
            {
                Positions = new[] { 0.0f, 0.30f, 0.70f, 1.0f },
                Colors    = new[] {
                    Color.FromArgb(20, C_RED_BORDER),
                    Color.FromArgb(210, C_RED_PRIMARY),
                    Color.FromArgb(210, C_RED_SIGNAL),
                    Color.FromArgb(20, C_RED_BORDER)
                }
            };
            gradRimBrush.InterpolationColors = rimCb;
            using var rimPen = new Pen(gradRimBrush, S(1.4f));
            g.DrawLine(rimPen, card.X + r, card.Y, card.Right - r, card.Y);
        }

        // ──────────────────────────────────────────────────────
        // Particle Engine: Initialization, Physics & Constellation Field
        // ──────────────────────────────────────────────────────
        private void InitParticles()
        {
            _particles.Clear();
            var card = _cardBounds;
            if (card.Width <= 0 || card.Height <= 0)
            {
                card = new RectangleF(S(BasePad), S(BasePad), S(BaseCardW), S(BaseCardH));
            }

            int count = 46;
            for (int i = 0; i < count; i++)
            {
                bool isHub = (i % 6 == 0);
                float radius = isHub ? S(2.8f + (float)_rng.NextDouble() * 1.0f) : S(1.2f + (float)_rng.NextDouble() * 1.1f);
                float vx = (float)(_rng.NextDouble() * 0.44 - 0.22);
                float vy = (float)(_rng.NextDouble() * 0.38 - 0.26); // gentle upward drift
                if (Math.Abs(vx) < 0.05f) vx = 0.09f * (_rng.Next(2) == 0 ? 1 : -1);
                if (Math.Abs(vy) < 0.05f) vy = -0.14f;

                Color col;
                float baseAlpha;
                int typeRoll = _rng.Next(100);
                if (typeRoll < 55)
                {
                    col = C_RED_SIGNAL; // Crimson energy node
                    baseAlpha = isHub ? 140f : 85f;
                }
                else if (typeRoll < 80)
                {
                    col = C_RED_PRIMARY; // Warm ruby node
                    baseAlpha = isHub ? 115f : 65f;
                }
                else
                {
                    col = C_TEXT_CHIP; // High-tech silvery cyber spark
                    baseAlpha = isHub ? 95f : 50f;
                }

                _particles.Add(new BackgroundParticle
                {
                    X          = card.X + (float)_rng.NextDouble() * card.Width,
                    Y          = card.Y + (float)_rng.NextDouble() * card.Height,
                    Vx         = vx,
                    Vy         = vy,
                    Radius     = radius,
                    BaseAlpha  = baseAlpha,
                    Phase      = (float)(_rng.NextDouble() * Math.PI * 2),
                    PulseSpeed = 0.028f + (float)_rng.NextDouble() * 0.040f,
                    Color      = col,
                    IsHub      = isHub
                });
            }
        }

        private void UpdateParticles()
        {
            var card = _cardBounds;
            if (card.Width <= 0 || _particles.Count == 0) return;

            float pad = S(14f);
            float left = card.X - pad;
            float right = card.Right + pad;
            float top = card.Y - pad;
            float bottom = card.Bottom + pad;

            foreach (var p in _particles)
            {
                p.X += p.Vx;
                p.Y += p.Vy;
                p.Phase += p.PulseSpeed;

                if (p.X < left) p.X = right;
                else if (p.X > right) p.X = left;

                if (p.Y < top) p.Y = bottom;
                else if (p.Y > bottom) p.Y = top;
            }
        }

        private void DrawParticleField(Graphics g, RectangleF card)
        {
            if (_particles.Count == 0) InitParticles();

            // 1. Constellation network lines between nearby particles
            float maxDist = S(76f);
            float maxDistSq = maxDist * maxDist;

            for (int i = 0; i < _particles.Count; i++)
            {
                var p1 = _particles[i];
                for (int j = i + 1; j < _particles.Count; j++)
                {
                    var p2 = _particles[j];
                    float dx = p1.X - p2.X;
                    float dy = p1.Y - p2.Y;
                    float distSq = dx * dx + dy * dy;

                    if (distSq < maxDistSq)
                    {
                        float dist = (float)Math.Sqrt(distSq);
                        float norm = 1.0f - (dist / maxDist);
                        // Subtle, luminous energy connecting lines
                        int lineAlpha = (int)(norm * norm * 42f);
                        if (lineAlpha > 2)
                        {
                            using var linePen = new Pen(Color.FromArgb(lineAlpha, C_RED_SIGNAL), S(0.85f));
                            g.DrawLine(linePen, p1.X, p1.Y, p2.X, p2.Y);
                        }
                    }
                }
            }

            // 2. Render particle nodes with soft glow halos
            foreach (var p in _particles)
            {
                float shimmer = (float)(0.72 + 0.28 * Math.Sin(p.Phase));
                int currentAlpha = Math.Clamp((int)(p.BaseAlpha * shimmer), 15, 230);

                // Soft outer glowing aura
                int auraAlpha = Math.Max(4, currentAlpha / 4);
                float haloR = p.Radius * (p.IsHub ? 2.6f : 2.0f);
                using (var haloBrush = new SolidBrush(Color.FromArgb(auraAlpha, p.Color)))
                {
                    g.FillEllipse(haloBrush, p.X - haloR, p.Y - haloR, haloR * 2, haloR * 2);
                }

                // Core luminous particle
                using (var coreBrush = new SolidBrush(Color.FromArgb(currentAlpha, p.Color)))
                {
                    g.FillEllipse(coreBrush, p.X - p.Radius, p.Y - p.Radius, p.Radius * 2, p.Radius * 2);
                }

                // Center specular highlight for hub particles
                if (p.IsHub && currentAlpha > 75)
                {
                    float specR = S(0.95f);
                    using var specBrush = new SolidBrush(Color.FromArgb(Math.Min(255, currentAlpha + 65), 255, 245, 245));
                    g.FillEllipse(specBrush, p.X - specR, p.Y - specR, specR * 2, specR * 2);
                }
            }
        }

        // ──────────────────────────────────────────────────────
        // 3. Top Header Bar
        // ──────────────────────────────────────────────────────
        private void DrawTopHeaderBar(Graphics g, RectangleF card)
        {
            float y = card.Y + S(12f);
            float x = card.X + S(18f);

            // Small orbital/star symbol: #FF3B30
            DrawDiamondSparkle(g, x + S(4f), y + S(6f), S(4.5f), C_RED_SIGNAL);

            // THEORION IDENTITY: uppercase letter spacing bold #F2F2F2
            using var headBrush = new SolidBrush(C_TEXT_PRIMARY);
            g.DrawString("THEORION IDENTITY", _fontTopHeader!, headBrush, x + S(14f), y);

            // BUILD — INNOVATE — IMPACT: clean futuristic technical subtitle
            using var subBrush = new SolidBrush(C_TEXT_MUTED);
            g.DrawString("BUILD   —   INNOVATE   —   IMPACT", _fontTopSub!, subBrush, x + S(14f), y + S(14f));

            // Window Controls: Minimize [─] and Close [✕]
            float btnSz = S(18f);
            _btnCloseBounds = new RectangleF(card.Right - S(16f) - btnSz, y, btnSz, btnSz);
            _btnMinBounds   = new RectangleF(_btnCloseBounds.X - S(6f) - btnSz, y, btnSz, btnSz);

            DrawWindowButton(g, _btnMinBounds, false, _btnMinHovered);
            DrawWindowButton(g, _btnCloseBounds, true, _btnCloseHovered);

            // Header bottom border: #303338
            using var sepPen = new Pen(C_BORDER_NORMAL, S(1f));
            g.DrawLine(sepPen, card.X + S(8f), card.Y + S(36f), card.Right - S(8f), card.Y + S(36f));
        }

        private void DrawWindowButton(Graphics g, RectangleF r, bool isClose, bool isHovered)
        {
            using var path = RoundedRect(r, S(4f));

            Color bg = isHovered ? C_RED_DARK : C_SURFACE_HL;
            using var bgB = new SolidBrush(bg);
            g.FillPath(bgB, path);

            Color borderC = isHovered ? C_RED_BORDER : C_BORDER_NORMAL;
            using var bPen = new Pen(borderC, S(0.8f));
            g.DrawPath(bPen, path);

            float cx = r.X + r.Width / 2f;
            float cy = r.Y + r.Height / 2f;
            Color strokeC = isHovered ? C_RED_SIGNAL : C_TEXT_SECOND;
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

            using var bBrush = new SolidBrush(C_TEXT_PRIMARY);
            g.DrawString("THEORIONGD", _fontHeroTag!, bBrush, leftX + S(16f), cy - S(10f));

            using var subB = new SolidBrush(C_TEXT_MUTED);
            g.DrawString("CODE  ×  CREATE  ×  GROW", _fontHeroSub!, subB, leftX + S(16f), cy + S(3f));

            // Center Holographic Crest
            float cx = card.X + card.Width / 2f;
            DrawCenterHoloCrest(g, cx, cy);

            // Right Available Pill
            float pillW = S(126f);
            float pillH = S(34f);
            _availablePillBounds = new RectangleF(card.Right - S(20f) - pillW, cy - pillH / 2f, pillW, pillH);
            DrawAvailablePill(g, _availablePillBounds, _availablePillHovered);
        }

        private void DrawWingedBrandLogo(Graphics g, float cx, float cy)
        {
            float s = S(7f);
            using var brush = new SolidBrush(C_RED_SIGNAL);
            using var pen   = new Pen(C_RED_BORDER, S(1.2f)) { StartCap = LineCap.Round, EndCap = LineCap.Round };

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

            // Subtle red ambient bloom behind avatar
            using var radialPath = new GraphicsPath();
            radialPath.AddEllipse(cx - r * 1.6f, cy - r * 1.6f, r * 3.2f, r * 3.2f);
            using var pgb = new PathGradientBrush(radialPath)
            {
                CenterColor = Color.FromArgb(40, C_RED_SIGNAL),
                SurroundColors = new[] { Color.FromArgb(0, C_RED_SIGNAL) }
            };
            g.FillPath(pgb, radialPath);

            // Tilted Gyroscope Orbital Ellipse 1 with orbital node (rotating slowly 8-15s)
            var s1 = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(28f + MathF.Sin(_orbitRingAngle) * 6f);
            using (var ringPen1 = new Pen(C_RED_BORDER, S(1.1f)))
            {
                ringPen1.DashStyle = DashStyle.Custom;
                ringPen1.DashPattern = new[] { 6f, 3.5f };
                g.DrawEllipse(ringPen1, -(r + S(11f)), -S(13f), (r + S(11f)) * 2, S(26f));
            }

            // Orbital Node (tiny red light rotating on the orbit)
            float nodeAng = _orbitRingAngle * 2.0f;
            float nodeX = (r + S(11f)) * MathF.Cos(nodeAng);
            float nodeY = S(13f) * MathF.Sin(nodeAng);
            using (var nodeGlow = new SolidBrush(Color.FromArgb(70, C_RED_SIGNAL)))
                g.FillEllipse(nodeGlow, nodeX - S(4.5f), nodeY - S(4.5f), S(9f), S(9f));
            using (var nodeCore = new SolidBrush(C_RED_SIGNAL))
                g.FillEllipse(nodeCore, nodeX - S(2.0f), nodeY - S(2.0f), S(4f), S(4f));
            g.Restore(s1);

            // Tilted Gyroscope Orbital Ellipse 2
            var s2 = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(-36f - MathF.Cos(_orbitRingAngle) * 5f);
            using (var ringPen2 = new Pen(Color.FromArgb(60, C_RED_BORDER), S(1.0f)))
            {
                g.DrawEllipse(ringPen2, -(r + S(8f)), -S(9f), (r + S(8f)) * 2, S(18f));
            }
            g.Restore(s2);

            // Inner background: #08090B
            using var circBrush = new SolidBrush(C_ENV_BLACK);
            g.FillEllipse(circBrush, cx - r, cy - r, r * 2, r * 2);

            // Outer ring: #6F2428
            using var outerRingPen = new Pen(C_RED_BORDER, S(2.4f));
            g.DrawEllipse(outerRingPen, cx - r, cy - r, r * 2, r * 2);

            // Active ring: #E53935
            using var activeRingPen = new Pen(C_RED_PRIMARY, S(1.2f));
            g.DrawEllipse(activeRingPen, cx - r + S(1f), cy - r + S(1f), (r - S(1f)) * 2, (r - S(1f)) * 2);

            // Highlight: #FF3B30 top arc
            using var hlPen = new Pen(C_RED_SIGNAL, S(1.5f));
            g.DrawArc(hlPen, cx - r + S(1f), cy - r + S(1f), (r - S(1f)) * 2, (r - S(1f)) * 2, 210, 120);

            // Center Text "OGD" in #F2F2F2
            var sf = new StringFormat
            {
                Alignment     = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            using var textShadow = new SolidBrush(Color.FromArgb(180, 0, 0, 0));
            g.DrawString("OGD", _fontMonogram!, textShadow,
                new RectangleF(cx - r, cy - r + S(1.0f), r * 2, r * 2), sf);

            using var textBrush = new SolidBrush(C_TEXT_PRIMARY);
            g.DrawString("OGD", _fontMonogram!, textBrush,
                new RectangleF(cx - r, cy - r - S(0.5f), r * 2, r * 2), sf);

            // Tiny Red Light / Diamond Sparkle below "OGD": #FF3B30
            DrawDiamondSparkle(g, cx, cy + S(16.5f), S(3.5f), C_RED_SIGNAL);
        }

        private void DrawAvailablePill(Graphics g, RectangleF r, bool isHovered)
        {
            using var path = RoundedRect(r, r.Height / 2f);

            Color bg = isHovered ? C_CHARCOAL_SEC : C_CHARCOAL_MAIN;
            using var bBrush = new SolidBrush(bg);
            g.FillPath(bBrush, path);

            Color bc = isHovered ? C_RED_PRIMARY : C_RED_BORDER;
            using var bPen = new Pen(bc, S(1.1f));
            g.DrawPath(bPen, path);

            // Pulsing Red Radar Dot: #FF3B30 (NO GREEN)
            float dotX = r.X + S(12f);
            float dotY = r.Y + r.Height / 2f;
            float dotR = S(3.0f);

            float ripple = (float)(Math.Sin(_pulse * 1.5f) * 0.5 + 0.5);
            int ripAlpha = Math.Clamp((int)((1f - ripple) * 90), 0, 255);
            float ripR = dotR + ripple * S(4.5f);
            using var ripBrush = new SolidBrush(Color.FromArgb(ripAlpha, C_RED_SIGNAL));
            g.FillEllipse(ripBrush, dotX - ripR, dotY - ripR, ripR * 2, ripR * 2);

            using var dotCore = new SolidBrush(C_RED_SIGNAL);
            g.FillEllipse(dotCore, dotX - dotR, dotY - dotR, dotR * 2, dotR * 2);

            // Text: Line 1 "AVAILABLE" in #F2F2F2
            using var t1 = new SolidBrush(C_TEXT_PRIMARY);
            g.DrawString("AVAILABLE", _fontAvailStatus!, t1, r.X + S(22f), r.Y + S(4.5f));

            // Text: Line 2 "FOR COLLABORATION" in #A7AAAE
            using var t2 = new SolidBrush(C_TEXT_SECOND);
            g.DrawString("FOR COLLABORATION", _fontAvailSub!, t2, r.X + S(22f), r.Y + S(17.5f));
        }

        // ──────────────────────────────────────────────────────
        // 5. Identity Titles: THEORIONGD, Name, Pill, Bio
        // ──────────────────────────────────────────────────────
        private float DrawIdentityTitles(Graphics g, RectangleF card, float y)
        {
            float cx = card.X + card.Width / 2f;
            var sf = new StringFormat
            {
                Alignment     = StringAlignment.Center,
                LineAlignment = StringAlignment.Near
            };

            // THEORIONGD: Large Bold #F2F2F2
            using var brandBrush = new SolidBrush(C_TEXT_PRIMARY);
            g.DrawString("THEORIONGD", _fontBrand!, brandBrush, cx, y, sf);
            float h1 = g.MeasureString("THEORIONGD", _fontBrand!).Height;
            y += h1 + S(1f);

            // Godfrey T. R: Medium-large #F2F2F2
            using var nameBrush = new SolidBrush(C_TEXT_PRIMARY);
            g.DrawString("Godfrey T. R", _fontName!, nameBrush, cx, y, sf);
            float h2 = g.MeasureString("Godfrey T. R", _fontName!).Height;
            y += h2 + S(3f);

            // [ JAVA FULL STACK & SOFTWARE ENGINEER ] Capsule Pill
            float pillW = S(216f);
            float pillH = S(23f);
            var roleRect = new RectangleF(cx - pillW / 2f, y, pillW, pillH);
            using var rolePath = RoundedRect(roleRect, pillH / 2f);

            using var roleBg = new SolidBrush(C_CHARCOAL_SEC);
            g.FillPath(roleBg, rolePath);

            using var roleBorder = new Pen(C_RED_BORDER, S(1f));
            g.DrawPath(roleBorder, rolePath);

            var pillSf = new StringFormat
            {
                Alignment     = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            using var roleTxt = new SolidBrush(C_RED_SIGNAL);
            g.DrawString("JAVA FULL STACK & SOFTWARE ENGINEER", _fontRolePill!, roleTxt, roleRect, pillSf);
            y += pillH + S(6f);

            // Bio statement: #A7AAAE
            const string bio = "B.E. CSE (2027) • Java full-stack engineer building resilient systems, multi-agent AI assistants with RAG, zero-trust architectures, and edge-AI computer vision solutions.";
            var bioRect = new RectangleF(card.X + S(20f), y, card.Width - S(40f), S(32f));
            using var bioBrush = new SolidBrush(C_TEXT_SECOND);
            g.DrawString(bio, _fontBio!, bioBrush, bioRect, sf);
            y += S(32f);

            return y;
        }

        // ──────────────────────────────────────────────────────
        // 6. Capability Matrix 3x2 Grid (Industrial Charcoal + Red Accent)
        // ──────────────────────────────────────────────────────
        private float DrawFocusGrid(Graphics g, RectangleF card, float y)
        {
            float margin = S(16f);
            float gapX   = S(8f);
            float gapY   = S(8f);
            float totalW = card.Width - margin * 2;
            float cardW  = (totalW - gapX * 2f) / 3f;
            float cardH  = S(50f);

            for (int i = 0; i < _focusCards.Count; i++)
            {
                var fc = _focusCards[i];
                int col = i % 3;
                int row = i / 3;

                float bx = card.X + margin + col * (cardW + gapX);
                float by = y + row * (cardH + gapY);
                fc.Bounds = new RectangleF(bx, by, cardW, cardH);

                using var path = RoundedRect(fc.Bounds, S(8f));

                Color bg = fc.IsHovered ? C_SURFACE_ELEV : C_CHARCOAL_SEC;
                using var bgB = new SolidBrush(bg);
                g.FillPath(bgB, path);

                Color bc = fc.IsHovered ? C_RED_PRIMARY : C_BORDER_NORMAL;
                using var bPen = new Pen(bc, S(1f));
                g.DrawPath(bPen, path);

                if (fc.IsHovered)
                {
                    using var glowPen = new Pen(Color.FromArgb(40, C_RED_SIGNAL), S(2f));
                    g.DrawPath(glowPen, path);
                }

                float boxSz = S(34f);
                var boxRect = new RectangleF(bx + S(7f), by + (cardH - boxSz) / 2f, boxSz, boxSz);
                using var boxPath = RoundedRect(boxRect, S(6f));

                using var boxBg = new SolidBrush(C_CHARCOAL_MAIN);
                g.FillPath(boxBg, boxPath);

                using var boxBorder = new Pen(fc.IsHovered ? C_RED_BORDER : C_BORDER_NORMAL, S(1f));
                g.DrawPath(boxBorder, boxPath);

                DrawFocusVectorIcon(g, fc.Type, boxRect, C_RED_SIGNAL);

                float textX = boxRect.Right + S(8f);
                Color tc = fc.IsHovered ? Color.White : C_TEXT_PRIMARY;
                using var tBrush = new SolidBrush(tc);
                g.DrawString(fc.Title, _fontFocusTitle!, tBrush, textX, by + S(8f));

                using var subBrush = new SolidBrush(C_TEXT_SECOND);
                g.DrawString(fc.Subtitle, _fontFocusSub!, subBrush, textX, by + S(26f));
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

                case "vision": // Eye / Optical scanner
                    float er = S(4.8f);
                    g.DrawArc(pen, cx - er, cy - er * 0.7f, er * 2f, er * 1.4f, 20, 140);
                    g.DrawArc(pen, cx - er, cy - er * 0.7f, er * 2f, er * 1.4f, 200, 140);
                    g.FillEllipse(brush, cx - S(1.6f), cy - S(1.6f), S(3.2f), S(3.2f));
                    break;

                case "backend": // Stacked database / server nodes
                    float bsw = S(4.8f);
                    float bsh = S(2.2f);
                    g.DrawEllipse(pen, cx - bsw, cy - S(4.0f), bsw * 2f, bsh * 2f);
                    g.DrawArc(pen, cx - bsw, cy - S(1.0f), bsw * 2f, bsh * 2f, 0, 180);
                    g.DrawArc(pen, cx - bsw, cy + S(2.0f), bsw * 2f, bsh * 2f, 0, 180);
                    g.DrawLine(pen, cx - bsw, cy - S(1.8f), cx - bsw, cy + S(3.0f));
                    g.DrawLine(pen, cx + bsw, cy - S(1.8f), cx + bsw, cy + S(3.0f));
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

                case "mobile": // Smartphone outline with home dot
                    float mw = S(3.8f);
                    float mh = S(5.5f);
                    g.DrawRectangle(pen, cx - mw, cy - mh, mw * 2f, mh * 2f);
                    g.DrawLine(pen, cx - S(1.5f), cy + mh * 0.65f, cx + S(1.5f), cy + mh * 0.65f);
                    break;

                case "ux-user":
                    float ur = S(3.0f);
                    g.DrawEllipse(pen, cx - ur, cy - ur * 1.8f, ur * 2f, ur * 2f);
                    g.DrawArc(pen, cx - ur * 1.8f, cy - ur * 0.2f, ur * 3.6f, ur * 3.2f, 190, 160);
                    break;

                case "ai-llm":
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

                case "uiux":
                    float pr = S(5.2f);
                    g.DrawEllipse(pen, cx - pr, cy - pr, pr * 2f, pr * 1.8f);
                    g.FillEllipse(brush, cx - pr * 0.4f, cy - pr * 0.3f, S(2.2f), S(2.2f));
                    g.FillEllipse(brush, cx + pr * 0.1f, cy - pr * 0.4f, S(2.2f), S(2.2f));
                    g.FillEllipse(brush, cx + pr * 0.4f, cy - pr * 0.1f, S(2.2f), S(2.2f));
                    break;
            }
        }

        // ──────────────────────────────────────────────────────
        // 7. Skills & Technologies Section (Technical Chips)
        // ──────────────────────────────────────────────────────
        private float DrawSkillsSection(Graphics g, RectangleF card, float y)
        {
            float margin = S(16f);

            // Section Header: ⊞ SKILLS & TECHNOLOGIES
            float iconX = card.X + margin;
            Draw4SquaresIcon(g, iconX, y + S(5f), S(3.5f), C_RED_SIGNAL);

            using var secB = new SolidBrush(C_TEXT_SECOND);
            g.DrawString("SKILLS & TECHNOLOGIES", _fontSecHeader!, secB, iconX + S(14f), y);
            y += S(17f);

            // 10 Pills in 2 rows of 5
            float totalW = card.Width - margin * 2;
            int cols = 5;
            float gapX = S(6f);
            float gapY = S(6f);
            float pillW = (totalW - (cols - 1) * gapX) / cols;
            float pillH = S(28f);

            for (int i = 0; i < _skillPills.Count; i++)
            {
                int col = i % cols;
                int row = i / cols;
                var sp = _skillPills[i];
                float px = card.X + margin + col * (pillW + gapX);
                float py = y + row * (pillH + gapY);
                sp.Bounds = new RectangleF(px, py, pillW, pillH);
                DrawSkillPillItem(g, sp);
            }

            return y + (pillH * 2f) + gapY;
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

            Color bg = sp.IsHovered ? C_SURFACE_ELEV : C_CHARCOAL_MAIN;
            using var bgB = new SolidBrush(bg);
            g.FillPath(bgB, path);

            Color bc = sp.IsHovered ? C_RED_PRIMARY : C_BORDER_NORMAL;
            using var bPen = new Pen(bc, S(1f));
            g.DrawPath(bPen, path);

            if (sp.IsHovered)
            {
                using var glowPen = new Pen(Color.FromArgb(40, C_RED_SIGNAL), S(2f));
                g.DrawPath(glowPen, path);
            }

            // Center icon + text as a group inside the pill
            SizeF textSize = g.MeasureString(sp.Name, _fontSkillPill!);
            float iconRadius = S(4.5f);
            float iconGap = S(5f);
            float totalContentW = iconRadius * 2f + iconGap + textSize.Width;
            float startX = sp.Bounds.X + (sp.Bounds.Width - totalContentW) / 2f;
            if (startX < sp.Bounds.X + S(4f)) startX = sp.Bounds.X + S(4f);

            float iconX = startX + iconRadius;
            float iconY = sp.Bounds.Y + sp.Bounds.Height / 2f;
            Color iconColor = sp.IsHovered ? C_RED_SIGNAL : C_RED_PRIMARY;
            DrawSkillBrandIcon(g, sp.Type, iconX, iconY, iconColor);

            // Text Label
            Color tc = sp.IsHovered ? Color.White : C_TEXT_CHIP;
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

        private void DrawSkillBrandIcon(Graphics g, string type, float cx, float cy, Color iconColor)
        {
            float s = S(4.5f);
            using var icBrush = new SolidBrush(iconColor);
            using var icPen = new Pen(iconColor, S(1.1f)) { StartCap = LineCap.Round, EndCap = LineCap.Round };

            switch (type)
            {
                case "java":
                    g.DrawArc(icPen, cx - s * 0.8f, cy - s * 0.5f, s * 1.4f, s * 1.4f, 0, 180);
                    g.DrawLine(icPen, cx - s * 0.8f, cy - s * 0.5f, cx + s * 0.6f, cy - s * 0.5f);
                    g.DrawArc(icPen, cx + s * 0.5f, cy - s * 0.5f, s * 0.7f, s * 0.7f, -90, 180);
                    break;

                case "python":
                    g.DrawArc(icPen, cx - s * 0.8f, cy - s * 0.8f, s * 1.2f, s * 1.2f, 90, 270);
                    g.DrawArc(icPen, cx - s * 0.4f, cy - s * 0.4f, s * 1.2f, s * 1.2f, 270, 270);
                    g.FillEllipse(icBrush, cx - s * 0.3f, cy - s * 0.6f, S(1.6f), S(1.6f));
                    g.FillEllipse(icBrush, cx + s * 0.1f, cy + s * 0.4f, S(1.6f), S(1.6f));
                    break;

                case "spring":
                    PointF[] spLeaf = {
                        new PointF(cx - s * 0.7f, cy + s * 0.7f),
                        new PointF(cx - s * 0.3f, cy - s * 0.7f),
                        new PointF(cx + s * 0.7f, cy - s * 0.7f),
                        new PointF(cx + s * 0.3f, cy + s * 0.7f)
                    };
                    g.FillPolygon(icBrush, spLeaf);
                    g.DrawLine(icPen, cx - s * 0.5f, cy + s * 0.6f, cx + s * 0.5f, cy - s * 0.6f);
                    break;

                case "react":
                    g.DrawEllipse(icPen, cx - s, cy - s * 0.45f, s * 2f, s * 0.9f);
                    g.DrawEllipse(icPen, cx - s * 0.45f, cy - s, s * 0.9f, s * 2f);
                    g.FillEllipse(icBrush, cx - S(1.2f), cy - S(1.2f), S(2.4f), S(2.4f));
                    break;

                case "cpp":
                case "c":
                    g.FillEllipse(icBrush, cx - s, cy - s, s * 2, s * 2);
                    using (var f = new Font("Segoe UI", S(5.5f), FontStyle.Bold, GraphicsUnit.Pixel))
                    using (var w = new SolidBrush(C_CHARCOAL_MAIN))
                        g.DrawString("C++", f, w, cx - S(4.5f), cy - S(4.5f));
                    break;

                case "node":
                    float nh = s * 0.9f;
                    PointF[] nHex = {
                        new PointF(cx, cy - nh),
                        new PointF(cx + nh * 0.85f, cy - nh * 0.5f),
                        new PointF(cx + nh * 0.85f, cy + nh * 0.5f),
                        new PointF(cx, cy + nh),
                        new PointF(cx - nh * 0.85f, cy + nh * 0.5f),
                        new PointF(cx - nh * 0.85f, cy - nh * 0.5f)
                    };
                    g.DrawPolygon(icPen, nHex);
                    g.FillEllipse(icBrush, cx - S(1.2f), cy - S(1.2f), S(2.4f), S(2.4f));
                    break;

                case "fastapi":
                    PointF[] fa = {
                        new PointF(cx + s * 0.2f, cy - s * 0.9f),
                        new PointF(cx - s * 0.6f, cy + s * 0.1f),
                        new PointF(cx, cy + s * 0.1f),
                        new PointF(cx - s * 0.2f, cy + s * 0.9f),
                        new PointF(cx + s * 0.6f, cy - s * 0.1f),
                        new PointF(cx, cy - s * 0.1f)
                    };
                    g.FillPolygon(icBrush, fa);
                    break;

                case "sql":
                    g.DrawEllipse(icPen, cx - s, cy - s * 0.8f, s * 2f, s * 0.7f);
                    g.DrawArc(icPen, cx - s, cy - s * 0.2f, s * 2f, s * 0.7f, 0, 180);
                    g.DrawArc(icPen, cx - s, cy + s * 0.4f, s * 2f, s * 0.7f, 0, 180);
                    g.DrawLine(icPen, cx - s, cy - s * 0.5f, cx - s, cy + s * 0.75f);
                    g.DrawLine(icPen, cx + s, cy - s * 0.5f, cx + s, cy + s * 0.75f);
                    break;

                case "mongo":
                    PointF[] mLeaf = {
                        new PointF(cx, cy - s * 0.9f),
                        new PointF(cx + s * 0.6f, cy),
                        new PointF(cx, cy + s * 0.9f),
                        new PointF(cx - s * 0.4f, cy)
                    };
                    g.FillPolygon(icBrush, mLeaf);
                    break;

                case "cybersecurity":
                    PointF[] sh = {
                        new PointF(cx - s * 0.8f, cy - s * 0.9f),
                        new PointF(cx + s * 0.8f, cy - s * 0.9f),
                        new PointF(cx + s * 0.8f, cy + s * 0.1f),
                        new PointF(cx, cy + s),
                        new PointF(cx - s * 0.8f, cy + s * 0.1f)
                    };
                    g.DrawPolygon(icPen, sh);
                    break;
            }
        }

        // ──────────────────────────────────────────────────────
        // 8. Internship Experience Section (3 Industry Roles from Resume)
        // ──────────────────────────────────────────────────────
        private float DrawExperienceSection(Graphics g, RectangleF card, float y)
        {
            float margin = S(16f);

            // Section Header: 💼 INTERNSHIP EXPERIENCE
            float iconX = card.X + margin;
            DrawBriefcaseIcon(g, iconX + S(3f), y + S(6f), S(4f), C_RED_SIGNAL);

            using var secB = new SolidBrush(C_TEXT_SECOND);
            g.DrawString("INTERNSHIP EXPERIENCE", _fontSecHeader!, secB, iconX + S(14f), y);
            y += S(17f);

            float totalW = card.Width - margin * 2;
            int count = _experienceCards.Count;
            float gapX = S(8f);
            float cardW = (totalW - (count - 1) * gapX) / count;
            float cardH = S(62f);

            for (int i = 0; i < count; i++)
            {
                var ec = _experienceCards[i];
                float bx = card.X + margin + i * (cardW + gapX);
                ec.Bounds = new RectangleF(bx, y, cardW, cardH);

                using var path = RoundedRect(ec.Bounds, S(8f));

                Color bg = ec.IsHovered ? C_SURFACE_ELEV : C_CHARCOAL_MAIN;
                using var bgB = new SolidBrush(bg);
                g.FillPath(bgB, path);

                Color bc = ec.IsHovered ? C_RED_PRIMARY : C_BORDER_NORMAL;
                using var bPen = new Pen(bc, S(1f));
                g.DrawPath(bPen, path);

                if (ec.IsHovered)
                {
                    using var glowPen = new Pen(Color.FromArgb(40, C_RED_SIGNAL), S(2f));
                    g.DrawPath(glowPen, path);
                }

                // Top: Company Name (Left) & Period (Right)
                float innerPadX = S(9f);
                float topY = y + S(7f);
                Color compColor = ec.IsHovered ? Color.White : C_TEXT_PRIMARY;
                using (var compBrush = new SolidBrush(compColor))
                {
                    g.DrawString(ec.Company, _fontExpCompany!, compBrush, bx + innerPadX, topY);
                }

                using (var perBrush = new SolidBrush(C_RED_SIGNAL))
                {
                    var perSf = new StringFormat
                    {
                        Alignment     = StringAlignment.Far,
                        LineAlignment = StringAlignment.Near
                    };
                    g.DrawString(ec.Period, _fontExpPeriod!, perBrush, bx + cardW - innerPadX, topY + S(1.2f), perSf);
                }

                // Middle: Role Title (Bold White)
                float roleY = topY + S(15.5f);
                using (var roleBrush = new SolidBrush(Color.White))
                {
                    g.DrawString(ec.Role, _fontExpRole!, roleBrush, bx + innerPadX, roleY);
                }

                // Bottom: Metric Highlights / Details
                float detY = roleY + S(15f);
                using (var detBrush = new SolidBrush(C_TEXT_MUTED))
                {
                    var detRect = new RectangleF(bx + innerPadX, detY, cardW - innerPadX * 2f, S(16f));
                    var detSf = new StringFormat
                    {
                        FormatFlags = StringFormatFlags.NoWrap,
                        Trimming    = StringTrimming.EllipsisCharacter
                    };
                    g.DrawString(ec.Detail, _fontExpDetail!, detBrush, detRect, detSf);
                }
            }

            return y + cardH;
        }

        private static void DrawBriefcaseIcon(Graphics g, float cx, float cy, float s, Color c)
        {
            using var pen = new Pen(c, 1.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawRectangle(pen, cx - s * 1.1f, cy - s * 0.5f, s * 2.2f, s * 1.5f);
            g.DrawLine(pen, cx - s * 0.5f, cy - s * 0.5f, cx - s * 0.5f, cy - s * 0.9f);
            g.DrawLine(pen, cx - s * 0.5f, cy - s * 0.9f, cx + s * 0.5f, cy - s * 0.9f);
            g.DrawLine(pen, cx + s * 0.5f, cy - s * 0.9f, cx + s * 0.5f, cy - s * 0.5f);
            g.DrawLine(pen, cx, cy - s * 0.5f, cx, cy - s * 0.1f);
        }

        // ──────────────────────────────────────────────────────
        // 9. Credentials & Featured Projects Row (Horizontal Panel)
        // ──────────────────────────────────────────────────────
        private float DrawCertAndProjectsRow(Graphics g, RectangleF card, float y)
        {
            float margin = S(16f);
            float totalW = card.Width - margin * 2;
            float cardH  = S(62f);

            var panelBounds = new RectangleF(card.X + margin, y, totalW, cardH);
            using var path = RoundedRect(panelBounds, S(16f));

            // Background #111315
            using var bgB = new SolidBrush(C_CHARCOAL_MAIN);
            g.FillPath(bgB, path);

            // Border #303338
            using var bPen = new Pen(C_BORDER_NORMAL, S(1f));
            g.DrawPath(bPen, path);

            // Proportional split: Left 49%, Right 51%
            float certW = totalW * 0.49f;
            float projW = totalW - certW;
            _certCardBounds     = new RectangleF(panelBounds.X, panelBounds.Y, certW, cardH);
            _projectsCardBounds = new RectangleF(panelBounds.X + certW, panelBounds.Y, projW, cardH);

            // Vertical divider line: #303338
            float divX = panelBounds.X + certW;
            using var divPen = new Pen(C_BORDER_NORMAL, S(1f));
            g.DrawLine(divPen, divX, panelBounds.Y + S(6f), divX, panelBounds.Bottom - S(6f));

            // Hover state overlays
            if (_certCardHovered)
            {
                using var hBrush = new SolidBrush(Color.FromArgb(20, C_RED_SIGNAL));
                using var hPath = RoundedRect(new RectangleF(_certCardBounds.X + S(2f), _certCardBounds.Y + S(2f), _certCardBounds.Width - S(4f), _certCardBounds.Height - S(4f)), S(14f));
                g.FillPath(hBrush, hPath);
            }
            if (_projectsCardHovered)
            {
                using var hBrush = new SolidBrush(Color.FromArgb(20, C_RED_SIGNAL));
                using var hPath = RoundedRect(new RectangleF(_projectsCardBounds.X + S(2f), _projectsCardBounds.Y + S(2f), _projectsCardBounds.Width - S(4f), _projectsCardBounds.Height - S(4f)), S(14f));
                g.FillPath(hBrush, hPath);
            }

            DrawCertificationsCard(g, _certCardBounds, _certCardHovered);
            DrawProjectsCard(g, _projectsCardBounds, _projectsCardHovered);

            return y + cardH;
        }

        private void DrawCertificationsCard(Graphics g, RectangleF r, bool isHovered)
        {
            float iconBoxSz = S(32f);
            var iconBox = new RectangleF(r.X + S(8f), r.Y + (r.Height - iconBoxSz) / 2f, iconBoxSz, iconBoxSz);
            DrawGoldMedalIcon(g, iconBox, isHovered);

            float textX = iconBox.Right + S(8f);
            using var headB = new SolidBrush(C_TEXT_PRIMARY);
            g.DrawString("CERTIFICATIONS & HONORS", _fontCertHeader!, headB, textX, r.Y + S(8f));

            using var bodyB = new SolidBrush(C_TEXT_SECOND);
            g.DrawString("Diploma in Programming (C, C++, Java)  •  GenAI", _fontCertBody!, bodyB, textX, r.Y + S(24f));
            g.DrawString("AI Agents (Google & Kaggle)  •  MSME Hackathon", _fontCertBody!, bodyB, textX, r.Y + S(39f));
        }

        private void DrawGoldMedalIcon(Graphics g, RectangleF r, bool isHovered)
        {
            float cx = r.X + r.Width / 2f;
            float cy = r.Y + r.Height / 2f;
            float mr = S(11.5f);

            Color circleBg = isHovered ? Color.FromArgb(40, C_RED_DARK) : C_CHARCOAL_SEC;
            Color circleBorder = isHovered ? C_RED_PRIMARY : C_RED_BORDER;
            using (var goldBg = new SolidBrush(circleBg))
            using (var goldCirclePen = new Pen(circleBorder, S(1f)))
            {
                g.FillEllipse(goldBg, cx - mr, cy - mr, mr * 2, mr * 2);
                g.DrawEllipse(goldCirclePen, cx - mr, cy - mr, mr * 2, mr * 2);
            }

            float inR = S(5.5f);
            float medalCy = cy - S(1.5f);
            Color medalC = isHovered ? C_RED_SIGNAL : C_RED_PRIMARY;
            using var goldPen = new Pen(medalC, S(1.3f));
            g.DrawEllipse(goldPen, cx - inR, medalCy - inR, inR * 2, inR * 2);
            DrawDiamondSparkle(g, cx, medalCy, S(2.5f), medalC);

            using var ribPen = new Pen(medalC, S(1.3f));
            g.DrawLine(ribPen, cx - inR * 0.5f, medalCy + inR * 0.7f, cx - inR * 0.8f, medalCy + inR * 1.5f);
            g.DrawLine(ribPen, cx + inR * 0.5f, medalCy + inR * 0.7f, cx + inR * 0.8f, medalCy + inR * 1.5f);
        }

        private void DrawProjectsCard(Graphics g, RectangleF r, bool isHovered)
        {
            float iconBoxSz = S(32f);
            var iconBox = new RectangleF(r.X + S(8f), r.Y + (r.Height - iconBoxSz) / 2f, iconBoxSz, iconBoxSz);
            DrawTerminalProjectIcon(g, iconBox, isHovered);

            float textX = iconBox.Right + S(8f);
            using var headB = new SolidBrush(C_TEXT_PRIMARY);
            g.DrawString("FEATURED PROJECTS", _fontProjectsHeader!, headB, textX, r.Y + S(8f));

            using var p1B = new SolidBrush(C_TEXT_SECOND);
            g.DrawString("Connify — Zero-Trust React Native & Next.js", _fontProjectsBody!, p1B, textX, r.Y + S(24f));
            g.DrawString("FaceShield — Edge-AI Face Auth (FastAPI • ONNX)", _fontProjectsBody!, p1B, textX, r.Y + S(39f));
        }

        private void DrawTerminalProjectIcon(Graphics g, RectangleF r, bool isHovered)
        {
            float cx = r.X + r.Width / 2f;
            float cy = r.Y + r.Height / 2f;
            float mr = S(11.5f);

            Color circleBg = isHovered ? Color.FromArgb(40, C_RED_DARK) : C_CHARCOAL_SEC;
            Color circleBorder = isHovered ? C_RED_PRIMARY : C_RED_BORDER;
            using (var pBg = new SolidBrush(circleBg))
            using (var pCirclePen = new Pen(circleBorder, S(1f)))
            {
                g.FillEllipse(pBg, cx - mr, cy - mr, mr * 2, mr * 2);
                g.DrawEllipse(pCirclePen, cx - mr, cy - mr, mr * 2, mr * 2);
            }

            float s = S(4.5f);
            Color iconC = isHovered ? C_RED_SIGNAL : C_RED_PRIMARY;
            using var pen = new Pen(iconC, S(1.2f)) { StartCap = LineCap.Round, EndCap = LineCap.Round };

            // Prompt chevron >_
            g.DrawLines(pen, new[] {
                new PointF(cx - s * 0.7f, cy - s * 0.6f),
                new PointF(cx - s * 0.1f, cy),
                new PointF(cx - s * 0.7f, cy + s * 0.6f)
            });
            g.DrawLine(pen, cx + s * 0.1f, cy + s * 0.6f, cx + s * 0.7f, cy + s * 0.6f);
        }

        // ──────────────────────────────────────────────────────
        // 9. Connect With Me (5 Platform Buttons)
        // ──────────────────────────────────────────────────────
        private float DrawConnectSection(Graphics g, RectangleF card, float y)
        {
            float margin = S(16f);

            // Section Header: 🔗 CONNECT WITH ME
            float iconX = card.X + margin;
            DrawChainLinkIcon(g, iconX, y + S(5f), S(3.5f), C_RED_SIGNAL);

            using var secB = new SolidBrush(C_TEXT_SECOND);
            g.DrawString("CONNECT WITH ME", _fontSecHeader!, secB, iconX + S(14f), y);
            y += S(17f);

            // 5 Buttons side-by-side
            float totalW = card.Width - margin * 2;
            int count = _connectButtons.Count;
            float gapX = S(6f);
            float btnW = (totalW - (count - 1) * gapX) / count;
            float btnH = S(54f);

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

                Color bg = b.IsHovered ? C_CHARCOAL_SEC : C_CHARCOAL_MAIN;
                using var bgB = new SolidBrush(bg);
                g.FillPath(bgB, path);

                Color bc = b.IsHovered ? C_RED_PRIMARY : C_BORDER_NORMAL;
                using var bPen = new Pen(bc, S(1f));
                g.DrawPath(bPen, path);

                if (b.IsHovered)
                {
                    using var glowPen = new Pen(Color.FromArgb(40, C_RED_SIGNAL), S(2f));
                    g.DrawPath(glowPen, path);
                }

                // Top Vector Platform Icon
                var iconRect = new RectangleF(bx, y + S(6f), btnW, S(16f));
                DrawConnectPlatformIcon(g, b.Type, iconRect, b.IsHovered);

                // Handle Label (e.g. TheOrionGD or theoriongd)
                Font handleFont = (b.Handle.Length > 15) ? _fontConnectHandleCompact! : _fontConnectHandle!;
                Color hc = b.IsHovered ? Color.White : C_TEXT_PRIMARY;
                using var hb = new SolidBrush(hc);
                var handleRect = new RectangleF(bx + S(2f), y + S(24f), btnW - S(4f), S(14f));
                g.DrawString(b.Handle, handleFont, hb, handleRect, sf);

                // Platform Sublabel (e.g. GitHub, LinkedIn)
                using var pb = new SolidBrush(C_TEXT_MUTED);
                var platRect = new RectangleF(bx, y + S(38f), btnW, S(12f));
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

        private void DrawConnectPlatformIcon(Graphics g, string type, RectangleF r, bool isHovered)
        {
            float cx = r.X + r.Width / 2f;
            float cy = r.Y + r.Height / 2f;
            float s = S(4.5f);
            Color ic = isHovered ? C_RED_SIGNAL : C_TEXT_SECOND;

            switch (type)
            {
                case "github":
                    using (var pen = new Pen(ic, S(1.2f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    {
                        g.DrawLines(pen, new[] { new PointF(cx - s * 0.4f, cy - s * 0.8f), new PointF(cx - s * 1.2f, cy), new PointF(cx - s * 0.4f, cy + s * 0.8f) });
                        g.DrawLine(pen, cx - s * 0.2f, cy + s * 0.9f, cx + s * 0.2f, cy - s * 0.9f);
                        g.DrawLines(pen, new[] { new PointF(cx + s * 0.4f, cy - s * 0.8f), new PointF(cx + s * 1.2f, cy), new PointF(cx + s * 0.4f, cy + s * 0.8f) });
                    }
                    break;

                case "linkedin":
                    float inW = S(12f);
                    float inH = S(12f);
                    using (var inBorder = new Pen(isHovered ? C_RED_BORDER : C_BORDER_NORMAL, S(1f)))
                    using (var inBg = new SolidBrush(C_SURFACE_HL))
                    {
                        using var inPath = RoundedRect(new RectangleF(cx - inW / 2, cy - inH / 2, inW, inH), S(2.5f));
                        g.FillPath(inBg, inPath);
                        g.DrawPath(inBorder, inPath);
                    }
                    using (var inFont = new Font("Segoe UI", S(7.5f), FontStyle.Bold, GraphicsUnit.Pixel))
                    using (var wBrush = new SolidBrush(ic))
                    {
                        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString("in", inFont, wBrush, new RectangleF(cx - inW / 2, cy - inH / 2, inW, inH), sf);
                    }
                    break;

                case "hackerrank":
                    float hs = S(5.2f);
                    using (var hBg = new SolidBrush(C_SURFACE_HL))
                    using (var hBorder = new Pen(isHovered ? C_RED_BORDER : C_BORDER_NORMAL, S(1f)))
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
                        g.DrawPolygon(hBorder, hex);
                    }
                    using (var hPen = new Pen(ic, S(1.1f)))
                    {
                        g.DrawLine(hPen, cx - hs * 0.45f, cy - hs * 0.5f, cx - hs * 0.45f, cy + hs * 0.5f);
                        g.DrawLine(hPen, cx + hs * 0.45f, cy - hs * 0.5f, cx + hs * 0.45f, cy + hs * 0.5f);
                        g.DrawLine(hPen, cx - hs * 0.45f, cy, cx + hs * 0.45f, cy);
                    }
                    break;

                case "portfolio":
                    using (var globePen = new Pen(ic, S(1.1f)))
                    {
                        g.DrawEllipse(globePen, cx - s, cy - s, s * 2f, s * 2f);
                        g.DrawEllipse(globePen, cx - s * 0.45f, cy - s, s * 0.9f, s * 2f);
                        g.DrawLine(globePen, cx - s, cy, cx + s, cy);
                    }
                    break;

                case "youtube":
                    float ytW = S(14f);
                    float ytH = S(10f);
                    using (var ytBorder = new Pen(isHovered ? C_RED_BORDER : C_BORDER_NORMAL, S(1f)))
                    using (var ytBg = new SolidBrush(C_SURFACE_HL))
                    {
                        using var ytPath = RoundedRect(new RectangleF(cx - ytW / 2f, cy - ytH / 2f, ytW, ytH), S(2.5f));
                        g.FillPath(ytBg, ytPath);
                        g.DrawPath(ytBorder, ytPath);
                    }
                    PointF[] ytTri = {
                        new PointF(cx - S(2.0f), cy - S(2.8f)),
                        new PointF(cx + S(2.8f), cy),
                        new PointF(cx - S(2.0f), cy + S(2.8f))
                    };
                    using (var wB = new SolidBrush(ic))
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

            // Horizontal separator above footer: #303338
            using var sepPen = new Pen(C_BORDER_NORMAL, S(1f));
            g.DrawLine(sepPen, card.X + S(8f), y - S(6f), card.Right - S(8f), y - S(6f));

            // Left: > Always learning. Always building.
            using var chevB = new SolidBrush(C_RED_SIGNAL);
            g.DrawString(">", _fontFooterQuote!, chevB, card.X + margin, y);

            using var quoteB = new SolidBrush(C_TEXT_SECOND);
            g.DrawString("Always learning. Always building.", _fontFooterQuote!, quoteB, card.X + margin + S(12f), y);

            // Right: /// THEORIONGD × OGD
            float rightX = card.Right - margin;
            var sf = new StringFormat
            {
                Alignment     = StringAlignment.Far,
                LineAlignment = StringAlignment.Near
            };

            // OGD
            float ogdW = g.MeasureString("OGD", _fontFooterTag!).Width;
            using var ogdB = new SolidBrush(C_TEXT_MUTED);
            g.DrawString("OGD", _fontFooterTag!, ogdB, rightX, y, sf);

            // ×
            float crossW = g.MeasureString(" × ", _fontFooterTag!).Width;
            float crossX = rightX - ogdW;
            using var crossB = new SolidBrush(C_TEXT_MUTED);
            g.DrawString(" × ", _fontFooterTag!, crossB, crossX, y, sf);

            // THEORIONGD
            float orionW = g.MeasureString("THEORIONGD", _fontFooterTag!).Width;
            float orionX = crossX - crossW;
            using var orionB = new SolidBrush(C_TEXT_PRIMARY);
            g.DrawString("THEORIONGD", _fontFooterTag!, orionB, orionX, y, sf);

            // /// Red accent slashes
            float stripeStartX = orionX - orionW - S(16f);
            using var stripePen = new Pen(C_RED_PRIMARY, S(1.4f)) { StartCap = LineCap.Round, EndCap = LineCap.Round };
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
                _fontProjectsHeader?.Dispose();
                _fontProjectsBody?.Dispose();
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
            StartPosition   = FormStartPosition.CenterScreen;
            TopMost         = true;
            Width           = 390;
            Height          = 370;
            BackColor       = Color.FromArgb(255, 13, 15, 17);
            ForeColor       = Color.FromArgb(255, 241, 245, 249);
            ShowInTaskbar   = false;
            DoubleBuffered  = true;
            Font            = new Font("Segoe UI", 9f);
            KeyPreview      = true;

            var panel = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            panel.Paint += PaintAbout;
            Controls.Add(panel);

            var closeBtn = new Button
            {
                Text         = "Close",
                Width        = 110,
                Height       = 32,
                FlatStyle    = FlatStyle.Flat,
                BackColor    = Color.FromArgb(255, 24, 26, 29),
                ForeColor    = Color.FromArgb(255, 255, 59, 48),
                Font         = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor       = Cursors.Hand,
                DialogResult = DialogResult.OK,
                Anchor       = AnchorStyles.Bottom
            };
            closeBtn.FlatAppearance.BorderColor = Color.FromArgb(255, 111, 36, 40);
            closeBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 53, 23, 26);
            closeBtn.Click += (s, e) => Close();
            closeBtn.Location = new Point((Width - closeBtn.Width) / 2, Height - closeBtn.Height - 48);

            panel.Controls.Add(closeBtn);
            closeBtn.BringToFront();

            AcceptButton = closeBtn;
            CancelButton = closeBtn;

            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter)
                    Close();
            };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            TopMost = true;
            BringToFront();
            Activate();
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
            using var b1 = new SolidBrush(Color.FromArgb(255, 255, 59, 48));
            g.DrawString("THEORIONGD", f1, b1, new RectangleF(0, y, w, 32), sf);
            y += 32f;

            using var f2 = new Font("Segoe UI", 12f, FontStyle.Bold);
            using var b2 = new SolidBrush(Color.FromArgb(255, 242, 242, 242));
            g.DrawString("Godfrey T. R", f2, b2, new RectangleF(0, y, w, 24), sf);
            y += 24f;

            using var f3 = new Font("Segoe UI", 9.5f);
            using var b3 = new SolidBrush(Color.FromArgb(255, 167, 170, 174));
            g.DrawString("Java Full Stack Developer | Software Engineer", f3, b3, new RectangleF(0, y, w, 20), sf);
            y += 24f;

            using var sepPen = new Pen(Color.FromArgb(180, 111, 36, 40), 1.2f);
            g.DrawLine(sepPen, 40, y, w - 40, y);
            y += 12f;

            using var f4 = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            using var b4 = new SolidBrush(Color.FromArgb(255, 229, 57, 53));
            g.DrawString("Java Full-Stack  •  Multi-Agent AI  •  Edge-AI  •  Security", f4, b4,
                new RectangleF(0, y, w, 20), sf);
            y += 24f;

            using var f5 = new Font("Segoe UI", 8.5f);
            using var b5 = new SolidBrush(Color.FromArgb(255, 167, 170, 174));
            g.DrawString("Official TheOrionGD Desktop Identity Dashboard.\nCrafted with hardware-accelerated 32bpp per-pixel transparency.",
                f5, b5, new RectangleF(24, y, w - 48, 48), sf);
            y += 48f;

            string versionStr = $"v{Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "2.0.0"}";
            using var f6 = new Font("Segoe UI", 8f);
            using var b6 = new SolidBrush(Color.FromArgb(255, 104, 109, 115));
            g.DrawString($"Version {versionStr}  •  TheOrionGD Ecosystem", f6, b6,
                new RectangleF(0, y, w, 20), sf);
        }
    }
}
