using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Shape
{
    // 1. 위치(좌표) 클래스
    public class Position
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Position(float x, float y)
        {
            X = x;
            Y = y;
        }

        public PointF ToPointF() => new PointF(X, Y);
        public override string ToString() => $"({X:F0}, {Y:F0})";
    }

    // 2. 부모 추상 클래스 (도형)
    public abstract class ShapeBase
    {
        public string Name { get; protected set; }
        public Color FillColor { get; set; } = Color.FromArgb(190, 91, 155, 213); // 파워포인트 스타일 블루 (투명도 적용)
        public Color BorderColor { get; set; } = Color.FromArgb(46, 117, 182);

        public ShapeBase(string name)
        {
            Name = name;
        }

        // 다형성을 실현하는 핵심 추상 메서드
        public abstract void Draw(Graphics g);

        // 도형의 요약 정보 문자열
        public abstract string GetDetailString();

        // 노란색 꼭짓점 포인트/핀 그리기 공통 헬퍼
        protected void DrawPoint(Graphics g, Position pos, string label, Brush brush, Pen pen)
        {
            float radius = 5f;
            g.FillEllipse(brush, pos.X - radius, pos.Y - radius, radius * 2, radius * 2);
            g.DrawEllipse(pen, pos.X - radius, pos.Y - radius, radius * 2, radius * 2);

            if (!string.IsNullOrEmpty(label))
            {
                using var font = new Font("맑은 고딕", 9f, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.Black);
                g.DrawString(label, font, textBrush, pos.X + 6, pos.Y - 8);
            }
        }
    }

    // 3. 사각형 클래스
    public class RectangleShape : ShapeBase
    {
        public Position TopRight { get; set; }
        public Position BottomLeft { get; set; }

        public RectangleShape(Position topRight, Position bottomLeft) : base("사각형")
        {
            TopRight = topRight;
            BottomLeft = bottomLeft;
        }

        public override string GetDetailString() => $"TopRight:{TopRight}, BottomLeft:{BottomLeft}";

        public override void Draw(Graphics g)
        {
            float x = Math.Min(TopRight.X, BottomLeft.X);
            float y = Math.Min(TopRight.Y, BottomLeft.Y);
            float width = Math.Abs(TopRight.X - BottomLeft.X);
            float height = Math.Abs(TopRight.Y - BottomLeft.Y);

            // 사각형 본체 채우기 및 테두리
            using (var brush = new SolidBrush(FillColor))
            using (var pen = new Pen(BorderColor, 2f))
            {
                g.FillRectangle(brush, x, y, width, height);
                g.DrawRectangle(pen, x, y, width, height);
            }

            // 꼭짓점 포인트 및 레이블 표시
            using (var yellowBrush = new SolidBrush(Color.Gold))
            using (var blackPen = new Pen(Color.FromArgb(80, 80, 80), 1.5f))
            {
                Position topLeft = new Position(x, y);
                Position bottomRight = new Position(x + width, y + height);

                DrawPoint(g, topLeft, "TL", yellowBrush, blackPen);
                DrawPoint(g, TopRight, "TR", yellowBrush, blackPen);
                DrawPoint(g, BottomLeft, "BL", yellowBrush, blackPen);
                DrawPoint(g, bottomRight, "BR", yellowBrush, blackPen);
            }

            // 도형 중앙 라벨
            using var font = new Font("맑은 고딕", 10f, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.FromArgb(30, 30, 30));
            g.DrawString("사각형", font, textBrush, x + (width / 2) - 20, y + (height / 2) - 8);
        }
    }

    // 4. 원 클래스
    public class CircleShape : ShapeBase
    {
        public Position Center { get; set; }
        public float Radius { get; set; }

        public CircleShape(Position center, float radius) : base("원")
        {
            Center = center;
            Radius = radius;
        }

        public override string GetDetailString() => $"Center:{Center}, Radius:{Radius:F0}";

        public override void Draw(Graphics g)
        {
            float x = Center.X - Radius;
            float y = Center.Y - Radius;
            float diameter = Radius * 2;

            // 원 본체 채우기 및 테두리
            using (var brush = new SolidBrush(FillColor))
            using (var pen = new Pen(BorderColor, 2f))
            {
                g.FillEllipse(brush, x, y, diameter, diameter);
                g.DrawEllipse(pen, x, y, diameter, diameter);
            }

            // 반지름 화살표선 그리기
            using (var arrowPen = new Pen(Color.DarkOrange, 2f))
            {
                arrowPen.CustomEndCap = new AdjustableArrowCap(4, 4);
                g.DrawLine(arrowPen, Center.X, Center.Y, Center.X + Radius, Center.Y);
            }

            // 중심점 및 반지름 텍스트
            using (var yellowBrush = new SolidBrush(Color.Gold))
            using (var blackPen = new Pen(Color.FromArgb(80, 80, 80), 1.5f))
            {
                DrawPoint(g, Center, "Pos", yellowBrush, blackPen);
            }

            using var font = new Font("맑은 고딕", 8.5f, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.DarkOrange);
            g.DrawString($"radius ({Radius:F0})", font, textBrush, Center.X + 8, Center.Y + 4);
        }
    }

    // 5. 삼각형 클래스
    public class TriangleShape : ShapeBase
    {
        public Position A { get; set; }
        public Position B { get; set; }
        public Position C { get; set; }

        public TriangleShape(Position a, Position b, Position c) : base("삼각형")
        {
            A = a;
            B = b;
            C = c;
        }

        public override string GetDetailString() => $"A:{A}, B:{B}, C:{C}";

        public override void Draw(Graphics g)
        {
            PointF[] points = new PointF[] { A.ToPointF(), B.ToPointF(), C.ToPointF() };

            // 삼각형 본체 채우기 및 테두리
            using (var brush = new SolidBrush(FillColor))
            using (var pen = new Pen(BorderColor, 2f))
            {
                g.FillPolygon(brush, points);
                g.DrawPolygon(pen, points);
            }

            // 꼭짓점 A, B, C 포인트 표시
            using (var yellowBrush = new SolidBrush(Color.Gold))
            using (var blackPen = new Pen(Color.FromArgb(80, 80, 80), 1.5f))
            {
                DrawPoint(g, A, "A", yellowBrush, blackPen);
                DrawPoint(g, B, "B", yellowBrush, blackPen);
                DrawPoint(g, C, "C", yellowBrush, blackPen);
            }
        }
    }

    // 6. 더블 버퍼링 지원 캔버스 패널
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
        }
    }

    // 7. 메인 폼
    public class MainForm : Form
    {
        // ★ 다형성을 실현하는 부모 타입 컬렉션 (List<ShapeBase>)
        private readonly List<ShapeBase> _shapes = new List<ShapeBase>();
        private readonly DoubleBufferedPanel _canvasPanel;
        private readonly ListView _itemListView;
        private readonly Label _countStatusLabel;
        private readonly Label _summaryLabel;
        private readonly Random _rand = new Random();

        public MainForm()
        {
            Text = "C# 다형성(Polymorphism) 도형 관리 및 시각화 시스템";
            Size = new Size(1180, 750);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("맑은 고딕", 9f);
            BackColor = Color.FromArgb(245, 246, 250);

            // ================== 상단 툴바 패널 ==================
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                Padding = new Padding(10),
                BackColor = Color.White
            };

            var btnAddAll = CreateStyledButton("🎨 기본 세트(3종) 생성", Color.FromArgb(41, 128, 185));
            var btnAddRect = CreateStyledButton("➕ 사각형 추가", Color.FromArgb(52, 152, 219));
            var btnAddCircle = CreateStyledButton("➕ 원 추가", Color.FromArgb(46, 204, 113));
            var btnAddTriangle = CreateStyledButton("➕ 삼각형 추가", Color.FromArgb(155, 89, 182));
            var btnClear = CreateStyledButton("🗑️ 전체 지우기", Color.FromArgb(231, 76, 60));

            btnAddAll.Click += (s, e) => AddDefaultSet();
            btnAddRect.Click += (s, e) => AddRandomRectangle();
            btnAddCircle.Click += (s, e) => AddRandomCircle();
            btnAddTriangle.Click += (s, e) => AddRandomTriangle();
            btnClear.Click += (s, e) => ClearShapes();

            var flowToolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false
            };
            flowToolbar.Controls.AddRange(new Control[] { btnAddAll, btnAddRect, btnAddCircle, btnAddTriangle, btnClear });
            topPanel.Controls.Add(flowToolbar);

            // ================== 오른쪽 사이드바 (리스트 개수 및 아이템 정보 시각화) ==================
            var rightPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 420,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(250, 251, 253),
                BorderStyle = BorderStyle.FixedSingle
            };

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(10)
            };

            _countStatusLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 35,
                Text = "📦 리스트 총 개수: 0개",
                Font = new Font("맑은 고딕", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80)
            };

            _summaryLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                Text = "사각형: 0 | 원: 0 | 삼각형: 0",
                Font = new Font("맑은 고딕", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(127, 140, 141)
            };

            headerPanel.Controls.Add(_summaryLabel);
            headerPanel.Controls.Add(_countStatusLabel);

            // ListView: 각 인덱스별 어떤 아이템이 들어있는지 직관적으로 표로 출력
            _itemListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("맑은 고딕", 9f),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            _itemListView.Columns.Add("No", 45, HorizontalAlignment.Center);
            _itemListView.Columns.Add("도형명", 65, HorizontalAlignment.Center);
            _itemListView.Columns.Add("C# 실제 타입 (자식 클래스)", 135, HorizontalAlignment.Left);
            _itemListView.Columns.Add("보유 상세 멤버 데이터", 155, HorizontalAlignment.Left);

            rightPanel.Controls.Add(_itemListView);
            rightPanel.Controls.Add(headerPanel);

            // ================== 중앙 캔버스 패널 ==================
            _canvasPanel = new DoubleBufferedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            _canvasPanel.Paint += CanvasPanel_Paint;

            // 폼에 컨트롤 배치
            Controls.Add(_canvasPanel);
            Controls.Add(rightPanel);
            Controls.Add(topPanel);

            // 초기 기본 세트 생성
            AddDefaultSet();
        }

        private Button CreateStyledButton(string text, Color color)
        {
            return new Button
            {
                Text = text,
                Width = 150,
                Height = 40,
                Margin = new Padding(5, 2, 5, 2),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
        }

        private void CanvasPanel_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 모눈종이 그리드
            DrawGrid(g);

            // 좌측 상단 캔버스 실시간 다형성 실행 정보 오버레이
            using (var titleBrush = new SolidBrush(Color.FromArgb(44, 62, 80)))
            using (var titleFont = new Font("맑은 고딕", 11f, FontStyle.Bold))
            {
                g.DrawString($"🎨 캔버스 렌더링 중 (ShapeBase.Draw() 호출 대상: {_shapes.Count}개)", titleFont, titleBrush, 15, 15);
            }

            // ★ 다형성(Polymorphism) 호출:
            // List<ShapeBase> 순회하며 단 하나의 통일된 인터페이스 shape.Draw(g) 호출!
            foreach (ShapeBase shape in _shapes)
            {
                shape.Draw(g);
            }
        }

        private void DrawGrid(Graphics g)
        {
            using var gridPen = new Pen(Color.FromArgb(242, 244, 246), 1f);
            int step = 35;
            for (int x = 0; x < _canvasPanel.Width; x += step)
                g.DrawLine(gridPen, x, 0, x, _canvasPanel.Height);
            for (int y = 0; y < _canvasPanel.Height; y += step)
                g.DrawLine(gridPen, 0, y, _canvasPanel.Width, y);
        }

        public void AddShape(ShapeBase shape)
        {
            _shapes.Add(shape);
            UpdateShapeView();
            _canvasPanel.Invalidate();
        }

        private void AddDefaultSet()
        {
            _shapes.Clear();

            // 파워포인트 다이어그램 기본 구조
            _shapes.Add(new RectangleShape(new Position(320, 90), new Position(150, 260)));
            _shapes.Add(new CircleShape(new Position(370, 180), 60));
            _shapes.Add(new TriangleShape(new Position(235, 150), new Position(150, 310), new Position(320, 310)));

            UpdateShapeView();
            _canvasPanel.Invalidate();
        }

        private void AddRandomRectangle()
        {
            float x = _rand.Next(30, Math.Max(60, _canvasPanel.Width - 240));
            float y = _rand.Next(50, Math.Max(70, _canvasPanel.Height - 200));
            float w = _rand.Next(100, 200);
            float h = _rand.Next(80, 160);

            AddShape(new RectangleShape(new Position(x + w, y), new Position(x, y + h)));
        }

        private void AddRandomCircle()
        {
            float r = _rand.Next(35, 80);
            float cx = _rand.Next((int)r + 30, Math.Max((int)r + 50, _canvasPanel.Width - (int)r - 40));
            float cy = _rand.Next((int)r + 50, Math.Max((int)r + 60, _canvasPanel.Height - (int)r - 40));

            AddShape(new CircleShape(new Position(cx, cy), r));
        }

        private void AddRandomTriangle()
        {
            float ax = _rand.Next(80, Math.Max(100, _canvasPanel.Width - 80));
            float ay = _rand.Next(50, Math.Max(70, _canvasPanel.Height - 180));
            float bx = ax - _rand.Next(40, 90);
            float by = ay + _rand.Next(80, 150);
            float cx = ax + _rand.Next(40, 90);
            float cy = by;

            AddShape(new TriangleShape(new Position(ax, ay), new Position(bx, by), new Position(cx, cy)));
        }

        private void ClearShapes()
        {
            _shapes.Clear();
            UpdateShapeView();
            _canvasPanel.Invalidate();
        }

        // 리스트 내 아이템 개수 및 각 아이템의 상세 정보를 시각화하는 메서드
        private void UpdateShapeView()
        {
            int total = _shapes.Count;
            int rectCount = 0;
            int circleCount = 0;
            int triCount = 0;

            _itemListView.BeginUpdate();
            _itemListView.Items.Clear();

            for (int i = 0; i < _shapes.Count; i++)
            {
                ShapeBase s = _shapes[i];
                string typeName = s.GetType().Name;
                string detail = s.GetDetailString();

                if (s is RectangleShape) rectCount++;
                else if (s is CircleShape) circleCount++;
                else if (s is TriangleShape) triCount++;

                var item = new ListViewItem((i + 1).ToString());
                item.SubItems.Add(s.Name);
                item.SubItems.Add(typeName);
                item.SubItems.Add(detail);

                // 시각적 구분을 위한 행 배경색 지정
                if (s is RectangleShape) item.BackColor = Color.FromArgb(235, 245, 255);
                else if (s is CircleShape) item.BackColor = Color.FromArgb(235, 255, 240);
                else if (s is TriangleShape) item.BackColor = Color.FromArgb(250, 240, 255);

                _itemListView.Items.Add(item);
            }

            _itemListView.EndUpdate();

            // 개수 및 통계 레이블 갱신
            _countStatusLabel.Text = $"📦 리스트 총 개수: {total}개";
            _summaryLabel.Text = $"🟦 사각형: {rectCount}개 | 🟢 원: {circleCount}개 | 🟣 삼각형: {triCount}개";
        }
    }

    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
