using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CompGraph
{
    public partial class Form1 : Form
    {
        Graphics graphColored, graphMainImage;
        Bitmap bitmap;

        // Выбранный цвет
        Color currentColor;
        // Выбранный регион
        Point currentRegion;
        bool isPaint = false;

        public Form1()
        {
            InitializeComponent();
            bitmap = new Bitmap(pictureBox1.Image);
            graphMainImage = Graphics.FromImage(bitmap);
            currentColor = Color.Black;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Построить гистограмму
            isPaint = false;
            int[] brightly = new int[bitmap.Width * bitmap.Height];
            Bitmap bipmapGist = new Bitmap(256, bitmap.Width * bitmap.Height);
            Graphics graphBrightly = Graphics.FromImage(bipmapGist);
            pictureBox2.Image = bipmapGist;
            Dictionary<int, List<int>> dictRed = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> dictGreen = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> dictBlue = new Dictionary<int, List<int>>();

            Pen pen = new Pen(Color.Black);
            int x = 0, y = 0;
            for (int i = 0; i < bitmap.Width; i++)
            {
                List<int> red = new List<int>();
                List<int> green = new List<int>();
                List<int> blue = new List<int>();
                for (int j = 0; j < bitmap.Height; j++)
                {
                    int r = Convert.ToInt32(bitmap.GetPixel(i, j).R);
                    int g = Convert.ToInt32(bitmap.GetPixel(i, j).G);
                    int b = Convert.ToInt32(bitmap.GetPixel(i, j).B);
                    red.Add(r);
                    green.Add(g);
                    blue.Add(b);

                    brightly[y] = (int)(0.34 * r + 0.5 * g + 0.16 * b); // получили яркость
                    graphBrightly.DrawLine(pen, new Point(x, 0), new Point(x, brightly[y]));
                    y++;

                }
                dictRed.Add(x, red);
                dictBlue.Add(x, blue);
                dictGreen.Add(x, green);
                x++;
            }
            Bitmap gist1 = new Bitmap(256, bitmap.Width * bitmap.Height);
            graphColored = Graphics.FromImage(gist1);
            pictureBox3.Image = gist1;
            Drawing(dictRed, new Pen(Color.Red));
            Drawing(dictGreen, new Pen(Color.Green));
            Drawing(dictBlue, new Pen(Color.Blue));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // повысить яркость +
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    int r = AddRight(Convert.ToInt32(bitmap.GetPixel(i, j).R));
                    int g = AddRight(Convert.ToInt32(bitmap.GetPixel(i, j).G));
                    int b = AddRight(Convert.ToInt32(bitmap.GetPixel(i, j).B));
                    Color color = Color.FromArgb(r, g, b);
                    bitmap.SetPixel(i, j, color);
                }
            }

            graphMainImage = Graphics.FromImage(bitmap);
            pictureBox1.Image = bitmap;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // снизить яркость -
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    int r = MinusRight(Convert.ToInt32(bitmap.GetPixel(i, j).R));
                    int g = MinusRight(Convert.ToInt32(bitmap.GetPixel(i, j).G));
                    int b = MinusRight(Convert.ToInt32(bitmap.GetPixel(i, j).B));
                    Color color = Color.FromArgb(r, g, b);
                    bitmap.SetPixel(i, j, color);
                }
            }

            graphMainImage = Graphics.FromImage(bitmap);
            pictureBox1.Image = bitmap;
        }


        private void button4_Click(object sender, EventArgs e)
        {
            // закрасить область
            DialogResult dialog = colorDialog1.ShowDialog();
            if (dialog == DialogResult.OK)
            {
                currentColor = colorDialog1.Color;
                isPaint = true;
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // выбор области
            if (isPaint)
            {
                currentRegion = e.Location;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isPaint)
            {
                PainRegion();
                pictureBox1.Image = bitmap;
                graphMainImage = Graphics.FromImage(bitmap);
            }
        }

        /// <summary>
        /// Получить цвет пикселя
        /// </summary>
        private Color GetColor(int x, int y)
        {
            int r = Convert.ToInt32(bitmap.GetPixel(x, y).R);
            int g = Convert.ToInt32(bitmap.GetPixel(x, y).G);
            int b = Convert.ToInt32(bitmap.GetPixel(x, y).B);
            Color color = Color.FromArgb(r, g, b);

            return color;
        }

        /// <summary>
        /// Проверка вышения яркости
        /// </summary>
        private int AddRight(int color)
        {
            if (color == 255)
                return 255;

            int newColor = color + 10;
            if (newColor > 255)
                return 255;

            return newColor;
        }

        /// <summary>
        /// Проверка вычитания яркости
        /// </summary>
        private int MinusRight(int color)
        {
            if (color == 0)
                return 0;
            int newColor = color - 10;
            if (newColor < 0)
                return 0;

            return newColor;
        }

        /// <summary>
        /// Закрасить регион с одинаковым цветом в выбранный цвет
        /// </summary>
        private void PainRegion()
        {
            Color color = GetColor(currentRegion.X, currentRegion.Y);

            Color currentColorRegion = color;
            Stack<Point> points = new Stack<Point>();
            points.Push(currentRegion);
            Point currentPoint;

            while (points.Count != 0)
            {
                currentPoint = points.Pop();
                bitmap.SetPixel(currentPoint.X, currentPoint.Y, currentColor);

                int x, y;
                // Ищем соседей
                // Верхний пиксель
                x = currentPoint.X;
                y = currentPoint.Y + 1;
                CheckBorderImage(ref x, ref y);
                CheckBorder(ref points, color, x, y);
                // Правый пиксель
                x = currentPoint.X + 1;
                y = currentPoint.Y;
                CheckBorderImage(ref x, ref y);
                CheckBorder(ref points, color, x, y);
                // Нижний пиксель
                x = currentPoint.X;
                y = currentPoint.Y - 1;
                CheckBorderImage(ref x, ref y);
                CheckBorder(ref points, color, x, y);
                // Левый пиксель
                x = currentPoint.X - 1;
                y = currentPoint.Y;
                CheckBorderImage(ref x, ref y);
                CheckBorder(ref points, color, x, y);
            }
        }

        /// <summary>
        /// Проверить границы заливки
        /// </summary>
        private void CheckBorder(ref Stack<Point> points, Color firstColorRegion, int x, int y)
        {
            Color pixel = bitmap.GetPixel(x, y);
            if (pixel.ToArgb() == firstColorRegion.ToArgb() && pixel.ToArgb() != currentColor.ToArgb())
            {
                points.Push(new Point(x, y));
            }
        }

        /// <summary>
        /// Проверить границы картинки
        /// </summary>
        private void CheckBorderImage(ref int x, ref int y)
        {
            if (x >= bitmap.Width)
                x = bitmap.Width - 1;
            else if (x < 0)
                x = 1;
            if (y >= bitmap.Height)
                y = bitmap.Height - 1;
            else if (y < 0)
                y = 1;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Рисуем гистограмму цвета
        /// </summary>
        private void Drawing(Dictionary<int, List<int>> colors, Pen pen)
        {
            for (int x = 0; x < colors.Keys.Count; x++)
            {
                for (int y = 0; y < colors[x].Count; y++)
                {
                    int color = colors[x][y];
                    graphColored.DrawLine(pen, new Point(x, 0), new Point(x, color));
                }
            }
        }
    }
}
