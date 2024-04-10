using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;

namespace Interpolation
{
    public partial class Form1 : Form
    {
        List<float> dividedDiif;
        float[] a;

        int X1 = 8, X2 = 18; //Границы графика(левая и правая)


        public delegate float Function(float x);
        public Form1()
        {
            InitializeComponent();
            chart.ChartAreas[0].AxisX.LabelStyle.Format = "{#.##}";

            checkedListBox.CheckOnClick = true;
            checkedListBox.ItemCheck += checkedListBox_SelectedIndexChanged;


            float[] X = new float[] { 8, 10, 12, 14, 16 };
            float[] Y = new float[] { 4, 9, 5, 1, 16 };
            DrawAllFunc(X, Y);

        }

        //Рисует полином 4ой степени по заданным коэф
        Function GivenCoeffF4(float a, float b, float c, float d, float k)
        {
            Function Func = delegate (float x)
            {
                return (float)(a * Math.Pow(x, 4.0) + b * Math.Pow(x, 3.0) + c * Math.Pow(x, 2.0) + d * x + k);
            };

            return Func;
        }


        //Метод минимальных квадратов
        Function MinimalSquares(float[] X, float[] Y, int k)
        {
            Function Func = delegate (float x)
            {
                if (a == null || a.Length == 0 || a.Length != k + 1)
                {
                    CreateCoeffA(X, Y, k);
                }

                float returnValue = a[0];

                for (int i = 1; i <= k; i++)
                {
                    returnValue += a[i] * (float)Math.Pow(x, i);
                }

                return returnValue;
            };

            return Func;
        }

        //Метод Ньютона
        Function NewtonPolynomial(float[] X, float[] Y)
        {
            Function Func = delegate (float x)
            {
                if (dividedDiif == null || dividedDiif.Count == 0)
                    CalculateDividedDiif(X, Y);


                float returnValue = dividedDiif[0], term;


                for (int i = 1; i < dividedDiif.Count; i++)
                {
                    term = dividedDiif[i];

                    for (int j = 0; j < i; j++)
                    {
                        term *= x - X[j];
                    }
                    returnValue += term;
                }

                return returnValue;
            };

            return Func;
        }


        //Метод Лагранжа
        Function LagrangePolynomial(float[] X, float[] Y)
        {
            Function Func = delegate (float x)
            {
                float returnValue1, returnValue2 = 0;
                for (int i = 0; i < Y.Length; i++)
                {
                    returnValue1 = Y[i];
                    for (int j = 0; j < X.Length; j++)
                    {
                        if (j != i)
                        {
                            returnValue1 *= (x - X[j]) / (X[i] - X[j]);
                        }
                    }
                    returnValue2 += returnValue1;

                }
                return returnValue2;
            };

            return Func;
        }

        //Расчитывает коэф для метода наименьших квадратов 
        void CreateCoeffA(float[] X, float[] Y, int k)
        {
            k++;
            //Ax = b
            //Создадим массив b
            float[] b = new float[k];
            for (int j = 0; j < k; j++)
            {
                b[j] = 0;
                for (int i = 0; i < X.Length; i++)
                {
                    b[j] += Y[i] * (float)Math.Pow(X[i], j);
                }
            }
            //Создадим массив c
            float[] c = new float[k * 2];
            for (int m = 0; m < k * 2; m++)
            {
                c[m] = 0;
                for (int i = 0; i < X.Length; i++)
                {
                    c[m] += (float)Math.Pow(X[i], m);
                }
            }

            //Заполним матрицу A
            int iShift = 0; //Сдвиг по индексу
            float[,] A = new float[k, k];

            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < k; j++)
                {
                    A[i, j] = c[j + iShift];
                }
                iShift++;
            }

            //Решим Ax = b
            a = Matrix.GaussWithMainElement(A, b);
        }

        //Расчитывает разделенные разности для метода Ньютона
        public void CalculateDividedDiif(float[] X, float[] Y)
        {
            List<float> dividedDiif = new List<float> { }; //Разделенные разности
            int k = 1, N = X.Length, lastIndex = 0; //Порядок разности, количество точек, индекс с которого начинается добавление в список

            for (int i = 0; i < X.Length; i++) dividedDiif.Add(Y[i]);

            //Заполняем массив разностей
            while (k != X.Length)
            {
                for (int i = 0; i < X.Length - k; i++)
                {
                    dividedDiif.Add((dividedDiif[i + lastIndex] - dividedDiif[i + 1 + lastIndex]) / (X[i] - X[i + k]));
                }
                lastIndex += N - k + 1;
                k += 1;

            }

            //Удаляем из него ненужные элмементы
            int index = 0; //Индекс элемента который нужно оставить, текущий индекс
            k = N; //Через сколько след элемент который нужно оставить
            List<float> reqDividedDiif = new List<float> { };//Нужные разделенные разности

            for (int i = 0; i < dividedDiif.Count; i++)
            {
                if (i == index)
                {
                    index += k;
                    k--;
                    reqDividedDiif.Add(dividedDiif[i]);
                }

            }
            this.dividedDiif = reqDividedDiif;
        }

        //Рисует функцию
        void DrawFunction(Function function, float x1, float x2, string name, int seriesNum)
        {
            chart.Series[seriesNum].Points.Clear();
            chart.Series[seriesNum].Name = name;
            chart.Series[seriesNum].ChartType = SeriesChartType.Line;
            chart.Series[seriesNum].BorderWidth = 2;

            for (float x = x1; x <= x2; x += 0.02f)
            {
                chart.Series[seriesNum].Points.AddXY(x, function(x));
            }

        }

        //Рисует точки
        void DrawPoints(float[] X, float[] Y, string name, int seriesNum)
        {
            chart.Series[seriesNum].Points.Clear();
            chart.Series[seriesNum].Name = name;
            chart.Series[seriesNum].ChartType = SeriesChartType.Point;
            chart.Series[seriesNum].MarkerSize = 7;


            for (int i = 0; i < X.Length; i++)
            {
                chart.Series[seriesNum].Points.AddXY(X[i], Y[i]);
            }

        }

        //Скрывает графики если она не отмечены
        private void checkedListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

            for (int i = 0; i < checkedListBox.Items.Count; i++)
            {
                chart.Series[i].Enabled = false;
            }

            foreach (int i in checkedListBox.CheckedIndices)
            {
                chart.Series[i].Enabled = true;
            }
        }


        //Отрисовка всех графиков
        void DrawAllFunc(float[] X, float[] Y)
        {
            Function function1 = LagrangePolynomial(X, Y);
            Function function2 = NewtonPolynomial(X, Y);
            Function function3 = MinimalSquares(X, Y, 1);
            Function function4 = MinimalSquares(X, Y, 2);
            Function function5 = MinimalSquares(X, Y, 3);

            Function function6 = GivenCoeffF4(0, 0, 0, 0.8f, -2.6f);
            Function function7 = GivenCoeffF4(0, 0, 0.3571f, -7.7714f, 45.9714f);
            Function function8 = GivenCoeffF4(0, 0.2917f, -10.1429f, 114.2619f, -410.4286f);
            Function function9 = GivenCoeffF4(5f / 192f, -23f / 24f, 571f / 48f, -164f / 3f, 64f);
            DrawPoints(X, Y, "Изначальные точки   ", 0);
            DrawFunction(function1, X1, X2, "Формула Лагранжа    ", 1);
            DrawFunction(function2, X1, X2, "Формула Ньютона     ", 2);
            DrawFunction(function3, X1, X2, "Сглаж мн-н 1-ой степ", 3);
            DrawFunction(function4, X1, X2, "Сглаж мн-н 2-ой степ", 4);
            DrawFunction(function5, X1, X2, "Сглаж мн-н 3-ой степ", 5);
            DrawFunction(function6, X1, X2, "1.Задан мн-н 4-ой степ", 6);
            DrawFunction(function7, X1, X2, "2.Задан мн-н 4-ой степ", 7);
            DrawFunction(function8, X1, X2, "3.Задан мн-н 4-ой степ", 8);
            DrawFunction(function9, X1, X2, "4.Задан мн-н 4-ой степ", 9);

        }
    }
}
