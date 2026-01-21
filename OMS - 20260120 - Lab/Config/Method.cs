using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BaseTool
{
    #region --- TimeManager ---

    public class TimeManager
    {
        private DateTime SetTime;
        private int DelayTime = 1000;

        public TimeManager()
        {

        }

        public TimeManager(int Time)
        {
            SetTime = DateTime.Now;
            DelayTime = Time;
        }

        public void SetDelay(int Time)
        {
            SetTime = DateTime.Now;
            DelayTime = Time;
        }

        public void Reset()
        {
            SetTime = DateTime.Now;
        }


        public bool IsTimeOut()
        {
            DateTime NowTime = DateTime.Now;
            return (NowTime - SetTime).TotalMilliseconds >= DelayTime;
        }
    }

    #endregion --- Class [2] : TimeManager ---

    #region -- DesktopManager --
    public static class DesktopManager
    {
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern int SystemParametersInfo(int uAction, int uPara, string lpvParam, int fuWinIni);

        public static bool SetBackgroud(string fileName)
        {
            if (File.Exists(fileName))
            {
                Bitmap BMP = new Bitmap(fileName);
                BMP.Save("D:\\Temp.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                SystemParametersInfo(20, 0, "D:\\Temp.bmp", 0x2);
                return true;
            }
            else
			{
                return false;
			}
        }
    }
    #endregion -- DesktopManager --

    #region -- DesktopManager --
    public static class FolderManager
    {
        public static void Delete_All_Files(string dirPath)
        {
            DirectoryInfo di = new DirectoryInfo(dirPath);
            FileInfo[] files = di.GetFiles();
            foreach (FileInfo file in files)
            {
                file.Delete();
            }
        }
    }
    #endregion -- DesktopManager --

    public static class ZipHelper
    {
        public static string DefaultPassword = "ThisIsMAMAA2_Tzuying+2206052";

        #region --- CreateZip ---
        /// <summary>  
        /// 壓縮整包資料夾  
        /// </summary>  
        /// <param name="outputPath">壓縮檔的完整路徑, 包含副檔名</param>  
        /// <param name="inputFolder">要壓縮的資料夾</param>  
        /// <param name="password">解壓縮密碼</param>  
        /// <returns></returns>  
        public static void CreateZip(string outputPath, string inputFolder)
        {
            FileStream fsOut = File.Create(outputPath);
            ZipOutputStream zipStream = new ZipOutputStream(fsOut);
            zipStream.SetLevel(0);
            zipStream.Password = DefaultPassword;
            //zipStream.Password = password;  // optional. Null is the same as not setting. Required if using AES.
            int folderOffset = inputFolder.Length + (inputFolder.EndsWith("\\") ? 0 : 1);
            CompressFolder(inputFolder, zipStream, folderOffset);
            zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
            zipStream.Close();
        }
        #endregion

        #region --- CompressFolder ---
        private static void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {

            string[] files = Directory.GetFiles(path);

            foreach (string filename in files)
            {
                FileInfo fi = new FileInfo(filename);
                string entryName = filename.Substring(folderOffset); // Makes the name in zip based on the folder
                entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
                ZipEntry newEntry = new ZipEntry(entryName);
                newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity
                newEntry.Size = fi.Length;
                zipStream.PutNextEntry(newEntry);
                zipStream.Password = DefaultPassword;

                byte[] buffer = new byte[4096];
                using (FileStream streamReader = File.OpenRead(filename))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }

            string[] folders = Directory.GetDirectories(path);

            foreach (string folder in folders)
            {
                CompressFolder(folder, zipStream, folderOffset);
            }
        }
        #endregion

        #region --- DeCompressionZip ---
        /// <summary>  
        /// 解壓縮
        /// </summary>  
        /// <param name="depositPath">壓縮檔路徑</param>  
        /// <param name="floderPath">解壓縮後存放路徑</param>  
        /// <returns></returns>  
        public static bool DeCompressionZip(string depositPath, string floderPath)
        {
            bool result = true;
            FileStream fileStream = null;

            try
            {
                ZipInputStream InpStream = new ZipInputStream(File.OpenRead(depositPath));
                ZipEntry zipEntry = InpStream.GetNextEntry(); // 取得資料夾中每一個檔案
                InpStream.Password = DefaultPassword;

                while (zipEntry != null)  // 做到沒有檔案為止
                {
                    if (zipEntry.IsFile)  // 處理子資料夾
                    {
                        string[] strs = zipEntry.Name.Split('\\');
                        if (strs.Length > 1)
                        {
                            for (int i = 0; i < strs.Length - 1; i++)
                            {
                                string fullPath = floderPath;
                                for (int j = 0; j < i; j++)
                                {
                                    fullPath = fullPath + "\\" + strs[j];
                                }
                                fullPath = fullPath + "\\" + strs[i];
                                Directory.CreateDirectory(floderPath);
                            }
                        }

                        fileStream = new FileStream(floderPath + "\\" + zipEntry.Name, FileMode.OpenOrCreate, FileAccess.Write);

                        while (true)
                        {
                            byte[] bts = new byte[4096];
                            int i = InpStream.Read(bts, 0, bts.Length);
                            if (i > 0)
                            {
                                fileStream.Write(bts, 0, i);
                            }
                            else
                            {
                                fileStream.Flush();
                                fileStream.Close();
                                break;
                            }
                        }
                    }
                    zipEntry = InpStream.GetNextEntry();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                if (fileStream != null)
                {
                    fileStream.Close();
                }

                result = false;
            }
            return result;
        }
        #endregion

        #region --- CheckFolder ---
        /// <summary>
        /// 檢查資料夾
        /// 存在->清除目錄下所有檔案; 不存在->創建資料夾
        /// 將資料夾設為隱藏
        /// </summary>
        /// <param name="folder"></param>
        public static void CheckFolder(string folder)
        {
            if (Directory.Exists(folder))
            {
                DirectoryInfo dir = new DirectoryInfo(folder);
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    file.Delete();
                }

                Directory.Delete(folder, true);
            }

            Directory.CreateDirectory(folder);
            DirectoryInfo diMyDir = new DirectoryInfo(folder);  // 隱藏資料夾
            diMyDir.Attributes = FileAttributes.Hidden;
        }
        #endregion



    }

    #region --- PropertyManager
    public static class PropertyManager
    {
        public static string GetDisplayName(PropertyInfo prop)
        {
            var attribute = prop.GetCustomAttributes(typeof(DisplayNameAttribute), true)
          .Cast<DisplayNameAttribute>().Single();
            string displayName = attribute.DisplayName;

            return displayName;
        }

        public static string GetValue(Object obj, PropertyInfo prop)
        {
            if (prop.PropertyType == typeof(int[]))
            {
                int[] values = (int[])prop.GetValue(obj);

                string Value = string.Join(" , ", values);

                return Value;
            }
            else
            {
                string Value = prop.GetValue(obj).ToString();

                return Value;
            }
        }
    }

    #endregion

    public static class CrossThread
    {
        public static void TbxEdit(string Txt, TextBox Ctrl)
        {
            if (Ctrl?.InvokeRequired == true)
            {
                Ctrl.Invoke(new Action(() => TbxEdit(Txt, Ctrl)));
            }
            else if (Ctrl != null)
            {
                Ctrl.Text = Txt;
            }
        }

        public static void LabelEdit(string Txt, Label Ctrl)
        {
            if (Ctrl?.InvokeRequired == true)
            {
                Ctrl.Invoke(new Action(() => LabelEdit(Txt, Ctrl)));
            }
            else if (Ctrl != null)
            {
                Ctrl.Text = Txt;
            }
        }

        public static void Set_Label_Message(Label Ctrl, string sMessage)
        {
            if (Ctrl?.InvokeRequired == true)
            {
                Ctrl.Invoke(new Action(() => Set_Label_Message(Ctrl, sMessage)));
            }
            else if (Ctrl != null)
            {
                Ctrl.Text = sMessage;
            }
        }

        public static void Set_Label_BGColor(Label Ctrl, System.Drawing.Color color)
        {
            if (Ctrl?.InvokeRequired == true)
            {
                Ctrl.Invoke(new Action(() => Set_Label_BGColor(Ctrl, color)));
            }
            else if (Ctrl != null)
            {
                Ctrl.BackColor = color;
            }
        }

        public static void Set_ListView_Message(ListView Ctrl, string sMessage)
        {
            if (Ctrl?.InvokeRequired == true)
            {
                Ctrl.Invoke(new Action(() => Set_ListView_Message(Ctrl, sMessage)));
            }
            else if (Ctrl != null)
            {
                Ctrl.Items.Add(sMessage);
            }
        }

        public static void Set_Numeric_Message(NumericUpDown Ctrl, double dMessage)
        {
            if (Ctrl?.InvokeRequired == true)
            {
                Ctrl.Invoke(new Action(() => Set_Numeric_Message(Ctrl, dMessage)));
            }
            else if (Ctrl != null)
            {
                if (dMessage != double.NaN && Math.Abs(dMessage) < 10000)
                    Ctrl.Value = Convert.ToDecimal(dMessage);
            }
        }
    }

    public static class Calculate
    {
        public static double CalculateSlope(List<double> y, List<double> x)
        {
            if (y.Count == 0 || x.Count == 0 || y.Count != x.Count)
            {
                // Handle error: data points are empty or do not match in count
                throw new ArgumentException("Known Ys and Known Xs must not be empty and must have the same number of data points.");
            }

            double xBar = x.Average();
            double yBar = y.Average();

            double numerator = 0;
            double denominator = 0;

            for (int i = 0; i < y.Count; i++)
            {
                numerator += (x[i] - xBar) * (y[i] - yBar);
                denominator += Math.Pow(x[i] - xBar, 2);
            }

            if (denominator == 0)
            {
                // Handle error: division by zero (e.g., all x-values are the same)
                throw new DivideByZeroException("Cannot calculate slope: all X values are identical.");
            }

            return numerator / denominator;
        }

        public static double CalculateIntercept(List<double> y, List<double> x)
        {
            if (x.Count != y.Count || x.Count == 0)
            {
                throw new ArgumentException("輸入的 x 和 y 數組長度必須相同且不為空");
            }

            int n = x.Count;

            // 計算 x 和 y 的平均值
            double xMean = x.Average();
            double yMean = y.Average();

            // 計算斜率 b
            double numerator = 0; // 分子
            double denominator = 0; // 分母

            for (int i = 0; i < n; i++)
            {
                numerator += (x[i] - xMean) * (y[i] - yMean);
                denominator += Math.Pow(x[i] - xMean, 2);
            }

            double slope = numerator / denominator;

            // 計算截距
            double intercept = yMean - slope * xMean;

            return intercept;
        }

        public static double CalculateRSQ(List<double> y, List<double> x)
        {
            if (x.Count != y.Count || x.Count == 0)
            {
                throw new ArgumentException("輸入的 x 和 y 數組長度必須相同且不為空");
            }

            int n = x.Count;

            // 計算 x 和 y 的平均值
            double xMean = x.Average();
            double yMean = y.Average();

            // 計算分子和分母
            double numerator = 0; // 分子
            double denominatorX = 0; // 分母中的 x 部分
            double denominatorY = 0; // 分母中的 y 部分

            for (int i = 0; i < n; i++)
            {
                numerator += (x[i] - xMean) * (y[i] - yMean);
                denominatorX += Math.Pow(x[i] - xMean, 2);
                denominatorY += Math.Pow(y[i] - yMean, 2);
            }

            // 計算相關係數 R
            double correlation = numerator / Math.Sqrt(denominatorX * denominatorY);

            // 計算 R^2
            double rsq = Math.Pow(correlation, 2);

            return rsq;
        }

      
        public static double CalculateStandard(List<double> data)
        {
            double Mean = data.Average();
            double sumOfSquares = data.Sum(d => Math.Pow(d - Mean, 2));

            return Math.Sqrt(sumOfSquares / (data.Count - 1));

          
        }

    }
}
