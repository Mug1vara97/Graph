using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private List<Point> circleCenters = new List<Point>();
        private List<string> vertexLabels = new List<string>();
        private List<Tuple<int, int, bool, int>> connections = new List<Tuple<int, int, bool, int>>();
        private Button createVertexButton;
        private Button deleteVertexButton;
        private Button changeLabelButton;
        private Button connectButton;
        private Button deleteEdgeButton;
        private TextBox weightTextBox;
        private RadioButton directedRadioButton;
        private RadioButton undirectedRadioButton;
        private Point pendingPoint;
        private bool isDragging = false;
        private int draggedCircleIndex = -1;
        private int selectedCircleIndex = -1;
        private int firstSelectedVertex = -1;
        private List<int> availableIds = new List<int>();
        private int nextId = 1;
        private bool isConnectingMode = false;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            createVertexButton = new Button();
            createVertexButton.Text = "Создать вершину";
            createVertexButton.Click += CreateVertexButton_Click;
            createVertexButton.Visible = false;
            this.Controls.Add(createVertexButton);

            deleteVertexButton = new Button();
            deleteVertexButton.Text = "Удалить вершину";
            deleteVertexButton.Click += DeleteVertexButton_Click;
            deleteVertexButton.Visible = false;
            this.Controls.Add(deleteVertexButton);

            changeLabelButton = new Button();
            changeLabelButton.Text = "Изменить метку";
            changeLabelButton.Click += ChangeLabelButton_Click;
            changeLabelButton.Visible = false;
            this.Controls.Add(changeLabelButton);

            deleteEdgeButton = new Button();
            deleteEdgeButton.Text = "Удалить ребро";
            deleteEdgeButton.Click += DeleteEdgeButton_Click;
            deleteEdgeButton.Visible = false;
            this.Controls.Add(deleteEdgeButton);

            weightTextBox = new TextBox() { Location = new Point(10, 10), Width = 50, PlaceholderText = "Вес" };
            connectButton = new Button() { Location = new Point(10, 40), Text = "Соединить" };
            connectButton.Click += ConnectButton_Click;

            directedRadioButton = new RadioButton() { Location = new Point(10, 70), Text = "Направленное", Checked = true };
            undirectedRadioButton = new RadioButton() { Location = new Point(10, 90), Text = "Ненаправленное" };

            this.Controls.Add(weightTextBox);
            this.Controls.Add(connectButton);
            this.Controls.Add(directedRadioButton);
            this.Controls.Add(undirectedRadioButton);

            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.MouseUp += Form1_MouseUp;

            Button saveButton = new Button();
            saveButton.Text = "Сохранить";
            saveButton.Click += SaveButton_Click;
            saveButton.Location = new Point(10, 110);
            this.Controls.Add(saveButton);

            // Добавляем кнопку "Загрузить"
            Button loadButton = new Button();
            loadButton.Text = "Загрузить";
            loadButton.Click += LoadButton_Click;
            loadButton.Location = new Point(10, 140);
            this.Controls.Add(loadButton);
        }
        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveGraph();
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            LoadGraph();
        }
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (IsOnEmptySpace(e.Location))
                {
                    pendingPoint = e.Location;
                    createVertexButton.Location = new Point(e.X, e.Y);
                    createVertexButton.Visible = true;
                }
                else
                {
                    createVertexButton.Visible = false;
                }

                selectedCircleIndex = -1;

                for (int i = 0; i < circleCenters.Count; i++)
                {
                    Point center = circleCenters[i];
                    int diameter = 60;

                    if (e.X >= center.X - diameter / 2 && e.X <= center.X + diameter / 2 &&
                        e.Y >= center.Y - diameter / 2 && e.Y <= center.Y + diameter / 2)
                    {
                        selectedCircleIndex = i;
                        break;
                    }
                }

                if (selectedCircleIndex >= 0)
                {
                    deleteVertexButton.Location = new Point(e.X, e.Y + 30);
                    deleteVertexButton.Visible = true;

                    changeLabelButton.Location = new Point(e.X, e.Y + 60);
                    changeLabelButton.Visible = true;
                }
                else
                {
                    selectedCircleIndex = -1; 
                    deleteVertexButton.Visible = false;
                    changeLabelButton.Visible = false;
                }

                if (isConnectingMode)
                {
                    for (int i = 0; i < connections.Count; i++)
                    {
                        var connection = connections[i];
                        Point p1 = circleCenters[connection.Item1];
                        Point p2 = circleCenters[connection.Item2];

                        if (IsPointOnLine(e.Location, p1, p2))
                        {
                            deleteEdgeButton.Location = new Point(e.X, e.Y + 10);
                            deleteEdgeButton.Visible = true;
                            createVertexButton.Visible = false;
                            deleteVertexButton.Visible = false;
                            changeLabelButton.Visible = false;
                            return;
                        }
                    }
                }


                deleteEdgeButton.Visible = false;
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (isConnectingMode)
                {
                    for (int i = 0; i < circleCenters.Count; i++)
                    {
                        Point center = circleCenters[i];
                        int diameter = 60;

                        if (e.X >= center.X - diameter / 2 && e.X <= center.X + diameter / 2 &&
                            e.Y >= center.Y - diameter / 2 && e.Y <= center.Y + diameter / 2)
                        {
                            if (firstSelectedVertex == -1)
                            {
                                firstSelectedVertex = i;
                                break;
                            }
                            else if (firstSelectedVertex != i)
                            {
                                ConnectVertices(firstSelectedVertex, i);
                                firstSelectedVertex = -1; 
                                break; 
                            }
                            break; 
                        }
                    }
                }
                else
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
        }

        private bool IsOnEmptySpace(Point p)
        {
            foreach (Point center in circleCenters)
            {
                int diameter = 60;
                if (p.X >= center.X - diameter / 2 && p.X <= center.X + diameter / 2 &&
                    p.Y >= center.Y - diameter / 2 && p.Y <= center.Y + diameter / 2)
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsPointOnLine(Point p, Point p1, Point p2)
        {
            const int tolerance = 5;
            double distance = Math.Abs((p2.Y - p1.Y) * p.X - (p2.X - p1.X) * p.Y + p2.X * p1.Y - p2.Y * p1.X) /
                               Math.Sqrt(Math.Pow(p2.Y - p1.Y, 2) + Math.Pow(p2.X - p1.X, 2));
            return distance <= tolerance;
        }

        private void DeleteEdgeButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                var connection = connections[i];
                Point p1 = circleCenters[connection.Item1];
                Point p2 = circleCenters[connection.Item2];

                if (IsPointOnLine(pendingPoint, p1, p2))
                {
                    connections.RemoveAt(i);
                    break; 
                }
            }

            deleteEdgeButton.Visible = false;
            this.Invalidate();
        }

        private void DeleteVertexButton_Click(object sender, EventArgs e)
        {
            if (selectedCircleIndex >= 0)
            {
                circleCenters.RemoveAt(selectedCircleIndex);
                vertexLabels.RemoveAt(selectedCircleIndex);
                availableIds.Add(selectedCircleIndex + 1);
                connections.RemoveAll(c => c.Item1 == selectedCircleIndex || c.Item2 == selectedCircleIndex);

                for (int i = 0; i < connections.Count; i++)
                {
                    var connection = connections[i];
                    int updatedVertex1 = connection.Item1 > selectedCircleIndex ? connection.Item1 - 1 : connection.Item1;
                    int updatedVertex2 = connection.Item2 > selectedCircleIndex ? connection.Item2 - 1 : connection.Item2;

                    connections[i] = new Tuple<int, int, bool, int>(updatedVertex1, updatedVertex2, connection.Item3, connection.Item4);
                }

                deleteVertexButton.Visible = false;
                changeLabelButton.Visible = false;
                selectedCircleIndex = -1;

                this.Invalidate();
            }
        }

        private void ChangeLabelButton_Click(object sender, EventArgs e)
        {
            if (selectedCircleIndex >= 0)
            {
                using (Form labelForm = new Form())
                {
                    TextBox labelTextBox = new TextBox() { Text = vertexLabels[selectedCircleIndex], Width = 200 };
                    Button okButton = new Button() { Text = "OK", DialogResult = DialogResult.OK };
                    labelForm.Controls.Add(labelTextBox);
                    labelForm.Controls.Add(okButton);
                    okButton.Dock = DockStyle.Bottom;
                    labelForm.AcceptButton = okButton;

                    if (labelForm.ShowDialog() == DialogResult.OK)
                    {
                        vertexLabels[selectedCircleIndex] = labelTextBox.Text;
                        this.Invalidate();
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
                deleteEdgeButton.Visible = false;
            }
        }

        private void CreateVertexButton_Click(object sender, EventArgs e)
        {
            int vertexId;
            if (availableIds.Count > 0)
            {
                vertexId = availableIds.First();
                availableIds.RemoveAt(0);
            }
            else
            {
                vertexId = nextId++;
            }

            circleCenters.Add(pendingPoint);
            vertexLabels.Add(vertexId.ToString());
            createVertexButton.Visible = false;
            this.Invalidate();
        }

        private void ConnectVertices(int vertex1Index, int vertex2Index)
        {
            bool isDirected = directedRadioButton.Checked;
            int weight = int.TryParse(weightTextBox.Text, out int parsedWeight) ? parsedWeight : 0;

            connections.Add(new Tuple<int, int, bool, int>(vertex1Index, vertex2Index, isDirected, weight));

            firstSelectedVertex = -1;

            isConnectingMode = false;
            connectButton.Text = "Соединить";
            connectButton.BackColor = SystemColors.Control;

            this.Invalidate();
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

                int weight = connection.Item4;
                Point weightPosition = new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
                g.DrawString(weight.ToString(), this.Font, Brushes.Black, weightPosition);
            }

            for (int i = 0; i < circleCenters.Count; i++)
            {
                int diameter = 60;
                Point center = circleCenters[i];
                g.DrawEllipse(Pens.Black, center.X - diameter / 2, center.Y - diameter / 2, diameter, diameter);
                g.DrawString(vertexLabels[i], this.Font, Brushes.Black, center.X - 10, center.Y - 10);
            }
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            isConnectingMode = !isConnectingMode;

            if (isConnectingMode)
            {
                connectButton.Text = "Закончить";
                connectButton.BackColor = Color.LightGreen;
                deleteVertexButton.Visible = false;
                changeLabelButton.Visible = false;
            }
            else
            {
                connectButton.Text = "Соединить";
                connectButton.BackColor = SystemColors.Control;
                firstSelectedVertex = -1;
            }
        }
        private void SaveGraph()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Graph Files (*.graph)|*.graph";
            saveFileDialog.Title = "Сохранить граф";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                var graphData = new
                {
                    x0 = 0,
                    y0 = 0,
                    vertices = circleCenters.Select((center, index) => new
                    {
                        x = center.X,
                        y = center.Y,
                        name = vertexLabels[index],
                        radius = 20,
                        background = "#ffffff",
                        fontSize = 18,
                        color = "#000000",
                        border = "#000000"
                    }).ToList(),
                    edges = connections.Select((connection, index) => new
                    {
                        vertex1 = connection.Item1,
                        vertex2 = connection.Item2,
                        weight = connection.Item4.ToString(),
                        isDirected = connection.Item3,
                        controlStep = 0,
                        fontSize = 18,
                        lineWidth = 2,
                        background = "#000000",
                        color = "#000000"
                    }).ToList(),
                    texts = new List<object>()
                };

                string jsonData = JsonSerializer.Serialize(graphData, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(filePath, jsonData);
            }
        }

        private void LoadGraph()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Graph Files (*.graph)|*.graph";
            openFileDialog.Title = "Загрузить граф";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    circleCenters.Clear();
                    vertexLabels.Clear();
                    connections.Clear();
                    availableIds.Clear();
                    nextId = 1;

                    string jsonData = File.ReadAllText(filePath);

                    var graphData = JsonSerializer.Deserialize<GraphData>(jsonData);

                    foreach (var vertex in graphData.vertices)
                    {
                        circleCenters.Add(new Point(vertex.x, vertex.y));
                        vertexLabels.Add(vertex.name);
                        nextId = Math.Max(nextId, int.Parse(vertex.name) + 1);
                    }

                    foreach (var edge in graphData.edges)
                    {
                        connections.Add(new Tuple<int, int, bool, int>(
                            edge.vertex1, edge.vertex2, edge.isDirected, int.Parse(edge.weight)));
                    }

                    for (int i = 1; i < nextId; i++)
                    {
                        if (!vertexLabels.Contains(i.ToString()))
                        {
                            availableIds.Add(i);
                        }
                    }

                    this.Invalidate();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке графа: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private class GraphData
        {
            public int x0 { get; set; }
            public int y0 { get; set; }
            public List<VertexData> vertices { get; set; }
            public List<EdgeData> edges { get; set; }
            public List<object> texts { get; set; }
        }

        private class VertexData
        {
            public int x { get; set; }
            public int y { get; set; }
            public string name { get; set; }
            public int radius { get; set; }
            public string background { get; set; }
            public int fontSize { get; set; }
            public string color { get; set; }
            public string border { get; set; }
        }

        private class EdgeData
        {
            public int vertex1 { get; set; }
            public int vertex2 { get; set; }
            public string weight { get; set; }
            public bool isDirected { get; set; }
            public int controlStep { get; set; }
            public int fontSize { get; set; }
            public int lineWidth { get; set; }
            public string background { get; set; }
            public string color { get; set; }
        }

    }
}
