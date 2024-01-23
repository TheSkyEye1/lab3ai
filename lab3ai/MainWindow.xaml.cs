using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
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
        string dbPath = "Perc.db";
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

            Perceptron = new MLP(783, 65, 10);

            int curpos = 0;
            int maxval = dataSet.Count * 10;

            for(int i = 0; i<15; i++)
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

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            int count = dataSet.Count;
            int correct = 0;

            foreach(DataSet dataSet in dataSet)
            {
                Perceptron.setInput(createinput(dataSet));
                Perceptron.forwardPass();
                double[] answer = Perceptron.getOutput();
                double maxValue = answer.Max();
                int maxIndex = Array.IndexOf(answer, maxValue);

                if(maxIndex == dataSet.number)
                {
                    correct++;
                }

            }

            double perc = ((double)correct / (double)count) * 100;
            perc = Math.Round(perc, 2);

            labl.Content = perc + "%     " + correct + "/" + count;

        }

        private void savePerc()
        {
            int counter = 0;
            foreach (Hidden h in Perceptron.hiddens)
            {
                string tableName = "hidden" + counter;
                using (SQLiteConnection connection = new SQLiteConnection($"Data Source={dbPath}; Version=3;"))
                {
                    connection.Open();
                    string insertHiddenLayerQuery = $"INSERT INTO {tableName} (Value, Bias) VALUES (@Value, @Bias); SELECT last_insert_rowid();";
                    using (SQLiteCommand insertHiddenLayerCommand = new SQLiteCommand(insertHiddenLayerQuery, connection))
                    {
                        InsertWeights(h.inputW, $"InputW_{tableName}", counter, connection);
                        InsertWeights(h.outputW, $"OutputW_{tableName}", counter, connection);
                    }
                }
                counter++;
            }
        }
        static void InsertWeights(List<double> weights, string columnName, int hiddenLayerId, SQLiteConnection connection)
        {
            string createTableQuery = $"CREATE TABLE IF NOT EXISTS {columnName} (Id INTEGER PRIMARY KEY AUTOINCREMENT, Weight REAL, HiddenLayerId INTEGER);";
            using (SQLiteCommand createTableCommand = new SQLiteCommand(createTableQuery, connection))
            {
                createTableCommand.ExecuteNonQuery();
            }
            string insertWeightsQuery = $"INSERT INTO {columnName} (Weight, HiddenLayerId) VALUES (@Weight, @HiddenLayerId);";
            using (SQLiteCommand insertWeightsCommand = new SQLiteCommand(insertWeightsQuery, connection))
            {
                foreach (double weight in weights)
                {
                    insertWeightsCommand.Parameters.AddWithValue("@Weight", weight);
                    insertWeightsCommand.Parameters.AddWithValue("@HiddenLayerId", hiddenLayerId);
                    insertWeightsCommand.ExecuteNonQuery();
                }
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            savePerc();
            MessageBox.Show("Saving Complete");
        }

        private List<Hidden> LoadHiddenLayers(string path)
        {
            List<Hidden> hiddenLayers = new List<Hidden>();
            int counter = 0;

            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={path}; Version=3;"))
            {
                connection.Open();
                while (true)
                {
                    string tableName = "InputW_hidden" + counter;
                    string checkTableQuery = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';";
                    using (SQLiteCommand checkTableCommand = new SQLiteCommand(checkTableQuery, connection))
                    {
                        object result = checkTableCommand.ExecuteScalar();

                        if (result == null || result == DBNull.Value)
                        {
                            break;
                        }
                    }
                    Hidden hiddenLayer = new Hidden();
                    hiddenLayer.inputW = LoadWeights($"InputW_hidden{counter}", connection);
                    hiddenLayer.outputW = LoadWeights($"OutputW_hidden{counter}", connection);
                    hiddenLayers.Add(hiddenLayer);
                    counter++;
                }
            }

            return hiddenLayers;
        }

        private List<double> LoadWeights(string columnName, SQLiteConnection connection)
        {
            List<double> weights = new List<double>();
            string checkTableQuery = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{columnName}';";
            using (SQLiteCommand checkTableCommand = new SQLiteCommand(checkTableQuery, connection))
            {
                object result = checkTableCommand.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    // Таблица весов существует, загружаем данные
                    string loadWeightsQuery = $"SELECT Weight FROM {columnName};";
                    using (SQLiteCommand loadWeightsCommand = new SQLiteCommand(loadWeightsQuery, connection))
                    {
                        using (SQLiteDataReader reader = loadWeightsCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                double weight = Convert.ToDouble(reader["Weight"]);
                                weights.Add(weight);
                            }
                        }
                    }
                }
            }

            return weights;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.ShowDialog();
            List<Hidden> hiddens = LoadHiddenLayers(dlg.FileName);
            Perceptron = new MLP(783, hiddens.Count, 10);
            Perceptron.setHiddens(hiddens);
            isTrained = true;
            MessageBox.Show("Loading Complete");
        }
    }
}
