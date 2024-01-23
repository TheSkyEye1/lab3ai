using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace lab3ai
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<DataSet> dataSet = new List<DataSet>();
        List<DataSet> trainData = new List<DataSet>();
        WriteableBitmap wb;
        MLP Perceptron;
        bool isTrained = false;
        public MainWindow()
        {
            InitializeComponent();

            wb = new WriteableBitmap(28, 28, 96, 96, PixelFormats.Gray8, null);
            img.Source = wb;
        }

        private void Button_Click(object sende, RoutedEventArgs e)
        {
            wb = new WriteableBitmap(28, 28, 96, 96, PixelFormats.Gray8, null);
            img.Source = wb;
        }

        private void can_MouseMove(object sender, MouseEventArgs e)
        {
            if(Mouse.LeftButton == MouseButtonState.Pressed)
            {
                Point pos = Mouse.GetPosition(can);
                int column = (int)pos.X/4;
                int row = (int)pos.Y/4;

                if (row > 26 || column > 26) return;

                try
                {
                    wb.Lock();

                    unsafe
                    {
                        IntPtr pBackBuffer = wb.BackBuffer;
                        pBackBuffer += row * wb.BackBufferStride;
                        pBackBuffer += column;

                        byte color_data = 255;

                        *((byte*)pBackBuffer) = color_data;
                        pBackBuffer = wb.BackBuffer;
                        pBackBuffer += (row + 1) * wb.BackBufferStride;
                        pBackBuffer += column;
                        *((byte*)pBackBuffer) = color_data;
                        pBackBuffer = wb.BackBuffer;
                        pBackBuffer += (row) * wb.BackBufferStride;
                        pBackBuffer += column + 1;
                        *((byte*)pBackBuffer) = color_data;
                        pBackBuffer = wb.BackBuffer;
                        pBackBuffer += (row + 1) * wb.BackBufferStride;
                        pBackBuffer += column + 1;
                        *((byte*)pBackBuffer) = color_data;

                    }

                    wb.AddDirtyRect(new Int32Rect(column, row, 2, 2));
                }
                finally { wb.Unlock(); }
                img.Source = wb;
            }
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            lb.Items.Clear();
            double[] pixels = new double[wb.PixelWidth * wb.PixelHeight];

            unsafe
            {
                IntPtr pBackBuffer = wb.BackBuffer;
                int k = 0;
                for(int i = 0; i<wb.PixelHeight; i++)
                {
                    string str = "";
                    for(int j = 0; j < wb.PixelWidth; j++)
                    {
                        str += *((byte*)pBackBuffer) + "\t";
                        pixels[k] = *((byte*)pBackBuffer);
                        k++;
                        pBackBuffer += 1;
                    }
                    lb.Items.Add(str);
                }
            }

            if (isTrained)
            {
                double[] newpixels = pixels.Skip(1).ToArray();
                OutDataLB.Content = "";
                Perceptron.setInput(newpixels);
                Perceptron.forwardPass();
                double[] answer = Perceptron.getOutput();
                double maxValue = answer.Max();
                int maxIndex = Array.IndexOf(answer, maxValue);
                NumberLB.Content = maxIndex + " " + answer[maxIndex];
                for (int i = 0; i < 10; i++)
                {
                    OutDataLB.Content += i.ToString() + ":" + Math.Round(answer[i], 3) + " ";
                }
            }
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            TBDlb.Items.Clear();
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.ShowDialog();
            using (var reader = new StreamReader(dlg.FileName))
            {
                List<string> headers = reader.ReadLine().Split(',').ToList <string>();
                while(!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    List<string> values = line.Split(',').ToList<string>();
                    DataSet entry = new DataSet(values);
                    dataSet.Add(entry);
                    TBDlb.Items.Add (entry.number);
                }
            }
        }

        private void TDBlb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TBDlb.SelectedIndex > -1)
            {
                lb.Items.Clear();
                string str = "";

                try
                {
                    wb.Lock();

                    unsafe
                    {
                        IntPtr pBackBuffer = wb.BackBuffer;
                        for (int i = 0; i < dataSet[TBDlb.SelectedIndex].image_data.Length; i++)
                        {
                            str += dataSet[TBDlb.SelectedIndex].image_data[i] + "\t";
                            if(i%28 == 0)
                            {
                                lb.Items.Add(str);
                                str = "";
                            }

                            byte color_data = dataSet[TBDlb.SelectedIndex].image_data[i];
                            *((byte*)pBackBuffer) = color_data;

                            pBackBuffer += 1;
                        }
                    }

                    wb.AddDirtyRect(new Int32Rect(0, 0, 28, 28));
                }
                finally
                {
                    wb.Unlock();
                }
                img.Source = wb;

                if(isTrained)
                {
                    OutDataLB.Content = "";
                    Perceptron.setInput(createinput(dataSet[TBDlb.SelectedIndex]));
                    Perceptron.forwardPass();
                    double[] answer = Perceptron.getOutput();
                    double maxValue = answer.Max();
                    int maxIndex = Array.IndexOf(answer, maxValue);
                    NumberLB.Content = maxIndex + " " + answer[maxIndex];
                    for(int i = 0; i<10; i++)
                    {
                        OutDataLB.Content += i.ToString() + ":" + Math.Round(answer[i],3) + " ";  
                    }
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.ShowDialog();
            using (var reader = new StreamReader(dlg.FileName))
            {
                List<string> headers = reader.ReadLine().Split(',').ToList<string>();
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    List<string> values = line.Split(',').ToList<string>();
                    DataSet entry = new DataSet(values);
                    trainData.Add(entry);
                }
            }

            Perceptron = new MLP(783, 30, 10);

            int curpos = 0;
            int maxval = dataSet.Count * 10;
            pb.Maximum = maxval;

            for(int i = 0; i<2; i++)
            {
                foreach(DataSet dataSet in trainData)
                {
                    Perceptron.setInput(createinput(dataSet));
                    double[] output = new double[10];
                    output[dataSet.number] = 1;
                    Perceptron.forwardPass();
                    Perceptron.backwardPass(output, 0.01);
                    curpos++;
                }
            }
            isTrained = true;
            MessageBox.Show("Mission Complete");
        }

        public double[] createinput(DataSet data)
        {
            double[] input = new double[28 * 28 - 1];

            for(int i = 1; i<28*28-1; i++)
            {
                input[i] = toBinary(data.image_data[i + 1]);
            }

            return input;
        }

        public double toBinary(byte a)
        {
            if (a > 0) return 1;
            else return 0;
        }

        private async Task UpdateProgressBarAsync(int value)
        {
            // Обновляем ProgressBar через Dispatcher
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                pb.Value = value;
            });
        }

    }
}
