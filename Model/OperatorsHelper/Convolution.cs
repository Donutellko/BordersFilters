using System;
using System.Collections;

namespace Model.OperatorsHelper
{
    public static class Convolution
    {
        /// <summary>
        /// Применяет формулу свёртки.
        /// </summary>
        /// <param name="pix">Массив участка изображения.</param>
        /// <param name="i">Первая координата центрального элемента.</param>
        /// <param name="j">Вторая координата центрального элемента.</param>
        /// <param name="oper">Оператор свёртки.</param>
        /// <returns></returns>
        public static int Process(this byte[,] pix, int i, int j, int[,] oper)
        {
            int result = 0;
            int size = oper.GetLength(0);
            oper.ForEach((k, l) => result += oper[k, l] * pix.GetPoint(i + k - size / 2, j + l - size / 2));
            return result;
        }
        /// <summary>
        /// Применяет формулу свёртки.
        /// </summary>
        /// <param name="pix">Массив участка изображения.</param>
        /// <param name="i">Первая координата центрального элемента.</param>
        /// <param name="j">Вторая координата центрального элемента.</param>
        /// <param name="oper">Оператор свёртки.</param>
        /// <returns></returns>
        public static byte Process(this byte[,] pix, int i, int j, double[,] oper)
        {
            double result = 0;
            int size = oper.GetLength(0);
            oper.ForEach((k, l) => result += oper[k, l] * pix.GetPoint(i + k - size / 2, j + l - size / 2));
            return result.ToByte();
        }
        private static byte GetPoint(this byte[,] arr, int i, int j)
        {
            return arr[arr.GetUpperBound(0) - i < 0 ? arr.GetUpperBound(0) : (i > 0 ? i : 0),
                arr.GetUpperBound(1) - j < 0 ? arr.GetUpperBound(1) : (j > 0 ? j : 0)];
        }
        private static int GetPoint(this int[,] arr, int i, int j)
        {
            return arr[arr.GetUpperBound(0) - i < 0 ? arr.GetUpperBound(0) : (i > 0 ? i : 0),
                arr.GetUpperBound(1) - j < 0 ? arr.GetUpperBound(1) : (j > 0 ? j : 0)];
        }
        /// <summary>
        /// Применяет формулу свёртки для двух операторов, вычисляя приближённое значение производной.
        /// </summary>
        /// <param name="pix">Массив участка изображения.</param>
        /// <param name="i">Первая координата центрального элемента.</param>
        /// <param name="j">Вторая координата центрального элемента.</param>
        /// <param name="operX">Оператор свёртки по X.</param>
        /// <param name="operY">Оператор свёртки по Y.</param>
        public static byte Process(this byte[,] pix, int i, int j, int[,] operX, int[,] operY)
        {
            double gx = pix.Process(i, j, operX);
            double gy = pix.Process(i, j, operY);

            return Math.Sqrt(gx * gx + gy * gy).ToByte();
        }
        /// <summary>
        /// Применяет формулу свёртки для двух операторов, вычисляя приближённое значение производной и возвращая угол градиента.
        /// </summary>
        /// <param name="pix">Массив участка изображения.</param>
        /// <param name="i">Первая координата центрального элемента.</param>
        /// <param name="j">Вторая координата центрального элемента.</param>
        /// <param name="operX">Оператор свёртки по X.</param>
        /// <param name="operY">Оператор свёртки по Y.</param>
        /// <param name="grad">Угол градиента.</param>
        public static byte Process(this byte[,] pix, int i, int j, int[,] operX, int[,] operY, out int grad)
        {
            double gx = pix.Process(i, j, operX);
            double gy = pix.Process(i, j, operY);
            byte g = Math.Sqrt(gx * gx + gy * gy).ToByte(); //(Math.Abs(gx) + Math.Abs(gy)).ToByte();
            grad = g != 0? (byte)(gy < Double.Epsilon ? 90 : Math.Round((90 - Math.Atan(gy/gx) / Math.PI * 180)/45)*45) : -1;  //TODO можно оптимизировать через Enum
            return g;
        }

        /// <summary>
        /// Подавление немаксимумов.
        /// </summary>
        /// <param name="src">Исходная матрица.</param>
        /// <param name="grads">Матрица градиентов.</param>
        /// <param name="i">Первая координата центрального элемента.</param>
        /// <param name="j">Вторая координата центрального элемента.</param>
        /// <param name="borderMin">Нижнаяя граница.</param>
        /// <param name="borderMax">Верхняя граница.</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte NonMaximum(this byte[,] src, int[,] grads, int i, int j, byte borderMin, byte borderMax, byte value)
        {
            switch (grads[i, j])
            {
                case 0:
                    if(src.GetPoint(i, j-1) < src[i, j] &&  src.GetPoint(i, j+1) < src[i, j])
                        return (byte)(src[i, j] < borderMin ? 0 : borderMax < src[i, j] ? borderMax : value);
                    else return 0;
                case 45:
                    if(src.GetPoint(i-1, j-1) < src[i, j] && src.GetPoint(i+1, j+1) < src[i, j])
                        return (byte)(src[i, j] < borderMin ? 0 : borderMax < src[i, j] ? borderMax : value);
                    else return 0;
                case 90:
                    if(src.GetPoint(i-1, j) < src[i, j] && src.GetPoint(i+1, j) < src[i, j])
                        return (byte)(src[i, j] < borderMin ? 0 : borderMax < src[i, j] ? borderMax : value);
                    else return 0;
                case 135:
                    if(src.GetPoint(i-1, j+1) < src[i, j] && src.GetPoint(i+1, j-1) < src[i, j])
                        return (byte)(src[i, j] < borderMin ? 0 : borderMax < src[i, j] ? borderMax : value);
                    else return 0;
                default:
                    return 0;
            }
        }

        private const int Size = 1;
        /// <summary>
        /// Проверка соприкосновения промежуточного элемента с границей.
        /// </summary>
        /// <param name="src">Исходаня матрица.</param>
        /// <param name="grads">Матрица градиентов.</param>
        /// <param name="i">Первая координата центрального элемента.</param>
        /// <param name="j">Вторая координата центрального элемента.</param>
        /// <param name="borderMax">Верхняя граница.</param>
        /// <returns></returns>
        public static byte UncertaintyTracing(this byte[,] src, int[,] grads, int i, int j, byte borderMax)
        {
            int angle = grads[i, j];
            for (int k = -Size; k <= Size; k++)
            {
                for (int l = -Size; l <= Size; l++)
                {
                    if(k == 0 && l == 0) continue;
                    if (src.GetPoint(i + k, j + l) >= borderMax && grads.GetPoint(i + k, j + l) == angle) return byte.MaxValue;
                }
            }
            return 0;
        }
    }
}
