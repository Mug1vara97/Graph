using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private List<Point> circleCenters = new List<Point>();
        private List<Tuple<int, int, bool>> connections = new List<Tuple<int, int, bool>>();
        private Button createVertexButton;
        private Button connectButton;
        private TextBox vertex1TextBox;
        private TextBox vertex2TextBox;
        private RadioButton directedRadioButton;
        private RadioButton undirectedRadioButton;
        private Point pendingPoint;
        private bool isDragging = false;
        private int draggedCircleIndex = -1;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            createVertexButton = new Button();
            createVertexButton.Text = "Создать вершину";
            createVertexButton.Click += CreateVertexButton_Click;
            createVertexButton.Visible = false;
            this.Controls.Add(createVertexButton);

            vertex1TextBox = new TextBox() { Location = new Point(10, 10), Width = 50 };
            vertex2TextBox = new TextBox() { Location = new Point(10, 40), Width = 50 };
            connectButton = new Button() { Location = new Point(10, 110), Text = "Соединить" };
            connectButton.Click += ConnectButton_Click;

            directedRadioButton = new RadioButton() { Location = new Point(10, 75), Text = "Направленное", Checked = true };
            undirectedRadioButton = new RadioButton() { Location = new Point(10, 95), Text = "Ненаправленное" };

            this.Controls.Add(vertex1TextBox);
            this.Controls.Add(vertex2TextBox);
            this.Controls.Add(connectButton);
            this.Controls.Add(directedRadioButton);
            this.Controls.Add(undirectedRadioButton);

            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.MouseUp += Form1_MouseUp;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                pendingPoint = e.Location;
                createVertexButton.Location = new Point(e.X, e.Y);
                createVertexButton.Visible = true;
            }
            else if (e.Button == MouseButtons.Left)
            {
                for (int i = 0; i < circleCenters.Count; i++)
                {
                    Point center = circleCenters[i];
                    int diameter = 60;

                    if (e.X >= center.X - diameter / 2 && e.X <= center.X + diameter / 2 &&
                        e.Y >= center.Y - diameter / 2 && e.Y <= center.Y + diameter / 2)
                    {
                        isDragging = true;
                        draggedCircleIndex = i;
                        break;
                    }
                }
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && draggedCircleIndex >= 0)
            {
                circleCenters[draggedCircleIndex] = e.Location;
                this.Invalidate();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                draggedCircleIndex = -1;
            }
        }

        private void CreateVertexButton_Click(object sender, EventArgs e)
        {
            circleCenters.Add(pendingPoint);
            createVertexButton.Visible = false;
            this.Invalidate();
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (int.TryParse(vertex1TextBox.Text, out int vertex1) &&
                int.TryParse(vertex2TextBox.Text, out int vertex2) &&
                vertex1 > 0 &&
                vertex2 > 0 &&
                vertex1 <= circleCenters.Count &&
                vertex2 <= circleCenters.Count)
            {
                bool isDirected = directedRadioButton.Checked;
                connections.Add(new Tuple<int, int, bool>(vertex1 - 1, vertex2 - 1, isDirected));
                this.Invalidate();
            }
        }

        private void DrawArrow(Graphics g, Point p1, Point p2)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            int radius = 30;
            Point startPoint = new Point(
                p1.X + (int)(dx * radius / distance),
                p1.Y + (int)(dy * radius / distance)
            );

            Point endPoint = new Point(
                p2.X - (int)(dx * radius / distance),
                p2.Y - (int)(dy * radius / distance)
            );

            using (Pen arrowPen = new Pen(Color.Black, 2))
            {

                g.DrawLine(arrowPen, startPoint, endPoint);

                float arrowLength = 10f;
                float arrowAngle = 45f;
                double angle = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);

                PointF arrowPoint1 = new PointF(
                    endPoint.X - arrowLength * (float)Math.Cos(angle - arrowAngle * Math.PI / 180),
                    endPoint.Y - arrowLength * (float)Math.Sin(angle - arrowAngle * Math.PI / 180));

                PointF arrowPoint2 = new PointF(
                    endPoint.X - arrowLength * (float)Math.Cos(angle + arrowAngle * Math.PI / 180),
                    endPoint.Y - arrowLength * (float)Math.Sin(angle + arrowAngle * Math.PI / 180));

                g.DrawLine(arrowPen, endPoint, arrowPoint1);
                g.DrawLine(arrowPen, endPoint, arrowPoint2);
            }
        }

        private void DrawLine(Graphics g, Point p1, Point p2, float thickness)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            int radius = 30;

            Point startPoint = new Point(
                p1.X + (int)(dx * radius / distance),
                p1.Y + (int)(dy * radius / distance)
            );

            Point endPoint = new Point(
                p2.X - (int)(dx * radius / distance),
                p2.Y - (int)(dy * radius / distance)
            );

            using (Pen linePen = new Pen(Color.Black, 2))
            {
                g.DrawLine(linePen, startPoint, endPoint);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            foreach (var connection in connections)
            {
                Point p1 = circleCenters[connection.Item1];
                Point p2 = circleCenters[connection.Item2];

                if (connection.Item3)
                {
                    DrawArrow(g, p1, p2);
                }
                else
                {
                    DrawLine(g, p1, p2, 5);
                }
            }

            for (int i = 0; i < circleCenters.Count; i++)
            {
                int diameter = 60;
                Point center = circleCenters[i];
                g.DrawEllipse(Pens.Black, center.X - diameter / 2, center.Y - diameter / 2, diameter, diameter);
                g.DrawString((i + 1).ToString(), this.Font, Brushes.Black, center.X - 10, center.Y - 10);
            }
        }


    }
}
