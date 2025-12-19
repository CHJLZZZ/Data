using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMeasure
{
    public static class MatrixAssistant
    {
        /// <summary>
        /// 判斷一個二維陣列是否為矩陣
        /// </summary>
        /// <param name="matrix">二維陣列</param>
        /// <returns>true:是矩陣 false:不是矩陣</returns>
        public static bool isMatrix(double[][] matrix)
        {
            //空矩陣是矩陣
            if (matrix.Length < 1) return true;
            //不同行列數如果不相等，則不是矩陣
            int count = matrix[0].Length;
            for (int i = 1; i < matrix.Length; i++)
            {
                if (matrix[i].Length != count)
                {
                    return false;
                }
            }
            //各行列數相等，則是矩陣
            return true;
        }

        /// <summary>
        /// 計算一個矩陣的行數和列數
        /// </summary>
        /// <param name="matrix">矩陣</param>
        /// <returns>陣列：行數、列數</returns>
        private static int[] MatrixCR(double[][] matrix)
        {
            //接收到的引數不是矩陣則報異常
            if (!isMatrix(matrix))
            {
                throw new Exception("接收到的引數不是矩陣");
            }
            //空矩陣行數列數都為0
            if (!isMatrix(matrix) || matrix.Length == 0)
            {
                return new int[2] { 0, 0 };
            }
            return new int[2] { matrix.Length, matrix[0].Length };
        }

        /// <summary>
        /// 列印矩陣
        /// </summary>
        /// <param name="matrix">待列印矩陣</param>
        public static void PrintMatrix(double[][] matrix)
        {
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < matrix[i].Length; j++)
                {
                    Console.Write(matrix[i][j] + "\t");
                    //注意不能寫為：Console.Write(matrix[i][j] + '\t');
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// 矩陣加法
        /// </summary>
        /// <param name="matrix1">矩陣1</param>
        /// <param name="matrix2">矩陣2</param>
        /// <returns>和</returns>
        public static double[][] MatrixAdd(double[][] matrix1, double[][] matrix2)
        {
            //矩陣1和矩陣2須為同型矩陣
            if (MatrixCR(matrix1)[0] != MatrixCR(matrix2)[0] ||
             MatrixCR(matrix1)[1] != MatrixCR(matrix2)[1])
            {
                throw new Exception("不同型矩陣無法進行加法運算");
            }
            //生成一個與matrix1同型的空矩陣
            double[][] result = new double[matrix1.Length][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new double[matrix1[i].Length];
            }
            //矩陣加法：把矩陣2各元素值加到矩陣1上，返回矩陣1
            for (int i = 0; i < result.Length; i++)
            {
                for (int j = 0; j < result[i].Length; j++)
                {
                    result[i][j] = matrix1[i][j] + matrix2[i][j];
                }
            }
            return result;
        }

        /// <summary>
        /// 矩陣取負
        /// </summary>
        /// <param name="matrix">矩陣</param>
        /// <returns>負矩陣</returns>
        public static double[][] NegtMatrix(double[][] matrix)
        {
            //合法性檢查
            if (!isMatrix(matrix))
            {
                throw new Exception("傳入的引數並不是一個矩陣");
            }
            //引數為空矩陣則返回空矩陣
            if (matrix.Length == 0)
            {
                return new double[][] { };
            }
            //生成一個與matrix同型的空矩陣
            double[][] result = new double[matrix.Length][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new double[matrix[i].Length];
            }
            //矩陣取負：各元素取相反數
            for (int i = 0; i < result.Length; i++)
            {
                for (int j = 0; j < result[0].Length; j++)
                {
                    result[i][j] = -matrix[i][j];
                }
            }
            return result;
        }

        /// <summary>
        /// 求矩陣的逆矩陣
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[][] InverseMatrix(double[][] matrix)
        {
            //matrix必須為非空
            if (matrix == null || matrix.Length == 0)
            {
                return new double[][] { };
            }
            //matrix 必須為方陣
            int len = matrix.Length;
            for (int counter = 0; counter < matrix.Length; counter++)
            {
                if (matrix[counter].Length != len)
                {
                    throw new Exception("matrix 必須為方陣");
                }
            }
            //計算矩陣行列式的值
            double dDeterminant = Determinant(matrix);
            if (Math.Abs(dDeterminant) <= 1E-6)
            {
                throw new Exception("矩陣不可逆");
            }
            //制作一個伴隨矩陣大小的矩陣
            double[][] result = AdjointMatrix(matrix);
            //矩陣的每項除以矩陣行列式的值，即為所求
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < matrix.Length; j++)
                {
                    result[i][j] = result[i][j] / dDeterminant;
                }
            }
            return result;
        }
 
        /// <summary>
        /// 遞歸計算行列式的值
        /// </summary>
        /// <param name="matrix">矩陣</param>
        /// <returns></returns>
        public static double Determinant(double[][] matrix)
        {
            //二階及以下行列式直接計算
            if (matrix.Length == 0) return 0;
            else if (matrix.Length == 1) return matrix[0][0];
            else if (matrix.Length == 2)
            {
                return matrix[0][0] * matrix[1][1] - matrix[0][1] * matrix[1][0];
            }
            //對第一行使用“加邊法”遞歸計算行列式的值
            double dSum = 0, dSign = 1;
            for (int i = 0; i < matrix.Length; i++)
            {
                double[][] matrixTemp = new double[matrix.Length - 1][];
                for (int count = 0; count < matrix.Length - 1; count++)
                {
                    matrixTemp[count] = new double[matrix.Length - 1];
                }
                for (int j = 0; j < matrixTemp.Length; j++)
                {
                    for (int k = 0; k < matrixTemp.Length; k++)
                    {
                        matrixTemp[j][k] = matrix[j + 1][k >= i ? k + 1 : k];
                    }
                }
                dSum += (matrix[0][i] * dSign * Determinant(matrixTemp));
                dSign = dSign * -1;
            }
            return dSum;
        }

        /// <summary>
        /// 計算方陣的伴隨矩陣
        /// </summary>
        /// <param name="matrix">方陣</param>
        /// <returns></returns>
        public static double[][] AdjointMatrix(double[][] matrix)
        {
            //制作一個伴隨矩陣大小的矩陣
            double[][] result = new double[matrix.Length][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new double[matrix[i].Length];
            }
            //生成伴隨矩陣
            for (int i = 0; i < result.Length; i++)
            {
                for (int j = 0; j < result.Length; j++)
                {
                    //存儲代數余子式的矩陣（行、列數都比原矩陣少1）
                    double[][] temp = new double[result.Length - 1][];
                    for (int k = 0; k < result.Length - 1; k++)
                    {
                        temp[k] = new double[result[k].Length - 1];
                    }
                    //生成代數余子式
                    for (int x = 0; x < temp.Length; x++)
                    {
                        for (int y = 0; y < temp.Length; y++)
                        {
                            temp[x][y] = matrix[x < i ? x : x + 1][y < j ? y : y + 1];
                        }
                    }
                    //Console.WriteLine("代數余子式:");
                    //PrintMatrix(temp);
                    result[j][i] = ((i + j) % 2 == 0 ? 1 : -1) * Determinant(temp);
                }
            }
            //Console.WriteLine("伴隨矩陣：");
            //PrintMatrix(result);
            return result;
        }

        /// <summary>
        /// 打印矩陣
        /// </summary>
        /// <param name="matrix">待打印矩陣</param>
        private static void PrintMatrix(double[][] matrix, string title = "")
        {
            //1.標題值為空則不顯示標題
            if (!String.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine(title);
            }
            //2.打印矩陣
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < matrix[i].Length; j++)
                {
                    Console.Write(matrix[i][j] + "\t");
                    //注意不能寫為：Console.Write(matrix[i][j] + '\t');
                }
                Console.WriteLine();
            }
            //3.空行
            Console.WriteLine();
        }

        /// <summary>
        /// 矩陣數乘
        /// </summary>
        /// <param name="matrix">矩陣</param>
        /// <param name="num">常數</param>
        /// <returns>積</returns>
        public static double[][] MatrixMult(double[][] matrix, double num)
        {
            //合法性檢查
            if (!isMatrix(matrix))
            {
                throw new Exception("傳入的引數並不是一個矩陣");
            }
            //引數為空矩陣則返回空矩陣
            if (matrix.Length == 0)
            {
                return new double[][] { };
            }
            //生成一個與matrix同型的空矩陣
            double[][] result = new double[matrix.Length][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new double[matrix[i].Length];
            }
            //矩陣數乘：用常數依次乘以矩陣各元素
            for (int i = 0; i < result.Length; i++)
            {
                for (int j = 0; j < result[0].Length; j++)
                {
                    result[i][j] = matrix[i][j] * num;
                }
            }
            return result;
        }

        /// <summary>
        /// 矩陣乘法
        /// </summary>
        /// <param name="matrix1">矩陣1</param>
        /// <param name="matrix2">矩陣2</param>
        /// <returns>積</returns>
        public static double[][] MatrixMult(double[][] matrix1, double[][] matrix2)
        {
            //合法性檢查
            if (MatrixCR(matrix1)[1] != MatrixCR(matrix2)[0])
            {
                throw new Exception("matrix1 的列數與 matrix2 的行數不想等");
            }
            //矩陣中沒有元素的情況
            if (matrix1.Length == 0 || matrix2.Length == 0)
            {
                return new double[][] { };
            }
            //matrix1是m*n矩陣，matrix2是n*p矩陣，則result是m*p矩陣
            int m = matrix1.Length, n = matrix2.Length, p = matrix2[0].Length;
            double[][] result = new double[m][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new double[p];
            }
            //矩陣乘法：c[i,j]=Sigma(k=1→n,a[i,k]*b[k,j])
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < p; j++)
                {
                    //對乘加法則
                    for (int k = 0; k < n; k++)
                    {
                        result[i][j] += (matrix1[i][k] * matrix2[k][j]);
                    }
                }
            }
            return result;
        }

        public static void TestMatrix()
        {
            //範例矩陣
            double[][] matrix1 = new double[][]
            {
              new double[] { 0.623837529, 0.33249778,  0.211041927 },
              new double[] { 0.339487334, 0.575821342, 0.047152666 },
              new double[] { 0.036675138, 0.091680878, 0.741805407 }
            };

            double[][] matrix2 = new double[][]
            {
              new double[] { 2, 3, 4 },
              new double[] { 5, 6, 7 },
              new double[] { 8, 9, 10 }
            };

            double[][] matrix3 = new double[][]
            {
              new double[] { 0.332784356 },
              new double[] { 0.307771687 },
              new double[] { 0.359443956 }
            };

            //矩陣加法
            Console.WriteLine("矩陣加法");
            PrintMatrix(MatrixAdd(matrix1, matrix2));
            Console.WriteLine();

            //矩陣取負
            Console.WriteLine("矩陣取負");
            PrintMatrix(NegtMatrix(matrix1));
            Console.WriteLine();

            //矩陣數乘
            Console.WriteLine("矩陣數乘");
            PrintMatrix(MatrixMult(matrix1, 3));
            Console.WriteLine();

            //矩陣乘法 (3x3 * 3x1 = 3x1)
            Console.WriteLine("矩陣乘法");
            PrintMatrix(MatrixMult(matrix1, matrix3));
            Console.WriteLine();

            //取逆矩陣
            Console.WriteLine("取逆矩陣");
            PrintMatrix(InverseMatrix(matrix1));
            Console.WriteLine();
        }

    }
}
