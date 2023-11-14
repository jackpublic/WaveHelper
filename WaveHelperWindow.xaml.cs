using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace WaveHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();
        }


        public ObservableCollection<Signal> sigs { get; set; } = new();
        public ListCollectionView Groupedsignals { get; set; }
        public string WaveJson { get; set; }    

        public string JsonFile = "wavedrom.json";

        public async void InitializeAsync()
        {

            sigs.Add(new Signal() { name = "clk", wave = "p.........", node = "0...." });
            sigs.Add(new Signal() { name = "rst", wave = "01........", node = ".a...." });
            sigs.Add(new Signal() { name = "req", wave = "1..0......", node = "...b....", edge = "a~>b t1"});
            sigs.Add(new Signal() { name = "bus", wave = "x.==.=.=....", data = "head body tail something_else" });

            Groupedsignals = new ListCollectionView(sigs);
            //leave it ungrouped at first
            //Groupedsignals.GroupDescriptions.Add(new PropertyGroupDescription("group"));

            this.DataContext = this;

            await webView.EnsureCoreWebView2Async(null);

        }

        private void ToggleGroup(object sender, RoutedEventArgs e)
        {
            if (Groupedsignals.GroupDescriptions.Count > 0)
            {
                Groupedsignals.GroupDescriptions.Clear();

            }
            else
            {
                Groupedsignals.GroupDescriptions.Add(new PropertyGroupDescription("group"));

            }
        }

        private void MoveUp(object sender, RoutedEventArgs e)
        {
            //move selected row in datagrid up
            //this does not work with grouped items... 
            int n = dg.SelectedIndex;
            if (n > 0 && n < dg.Items.Count - 1)
            {
                sigs.Move(n, n - 1);
                //var item = sigs[n];
                //sigs.RemoveAt(n);
                //sigs.Insert(n - 1, item);
                dg.SelectedIndex = n - 1;
            }


        }

        private void MoveDown(object sender, RoutedEventArgs e)
        {

        }

        private void Copy(object sender, RoutedEventArgs e)
        {

        }
        private void ReorderByGroup(object sender, RoutedEventArgs e)
        {

        }
        private void Delete(object sender, RoutedEventArgs e)
        {
            int n = dg.SelectedIndex;
            if (n < dg.Items.Count - 1)
            {
                sigs.RemoveAt(n);
            }
        }
        
        private void DataGridToWaveJson()
        {
            try {
                if (tabs.SelectedIndex == 0)
                {
                    if (sigs.Count > 0)
                    {
                        List<string> edges = new List<string>();
                        //create json string from sigs
                        StringBuilder sb = new StringBuilder();
                        sb.Append("{ signal : [\r\n");
                        foreach (var item in sigs)
                        {
                            sb.Append("    { name: '" + item.name + "',  wave: '" + item.wave + "'" + ", node: '" + item.node + "'");

                            if (item.data != null && item.data.Trim().Length > 0)
                            {
                                //sb.Append(",   data: [\'" + item.data.Replace(",", "','") + "\']");  //data: ['head', 'body', 'tail', 'data']
                                sb.Append(",   data: \'" + item.data + "\'");  //data: 'head body tail'    
                            }
                            sb.Append(" },\r\n");

                            if (item.edge.Trim().Length > 3)
                            {
                                edges.AddRange(item.edge.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
                            }   
                        }
                        sb.AppendLine("],");
                        sb.AppendLine("edge: [");
                        //create eddge string from sigs
                        var edge_string = "'" + string.Join("', '", edges) + "'";
                        sb.AppendLine(edge_string);
                        sb.AppendLine("]}");
                        WaveJson = sb.ToString();
                        tb.Text = WaveJson;
                    }
                }
                else
                {
                    WaveJson = tb.Text;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            File.WriteAllText(JsonFile, WaveJson);
        }

        private void GenerateWaveForm(object sender, RoutedEventArgs e)
        {
            DataGridToWaveJson();

            string fileName = "editor_local.html";
            var lines = File.ReadAllLines(fileName);

            string outFile = "wavedrom.html";
            using (StreamWriter writer = new StreamWriter(outFile))
            {
                for (int i = 0; i < 14; i++)
                {
                    writer.WriteLine(lines[i]);
                }

                DataGridToWaveJson();
                if (WaveJson.Trim().Length > 0)
                {
                    var x = WaveJson.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in x)
                    {
                        writer.WriteLine(item);
                    }
                }
                else
                {
                    writer.WriteLine("Invalid data");
                }
                for (int i = 17; i < lines.Length; i++)
                {
                    writer.WriteLine(lines[i]);
                }

            }

            string path = Path.Combine(Directory.GetCurrentDirectory(), outFile);
            //change '\' to '/'
            path = "file:///" + path.Replace('\\', '/');
            Debug.WriteLine(path);
            webView.CoreWebView2.Navigate(path);
        }

        private void Cleanup(object sender, RoutedEventArgs e)
        {
            //use first signal's length to set all signal length
           // var waves_max = sigs.Select(x => x.wave.Trim()).Max(y => y.Length);              
           // var nodes_max = sigs.Select(x => x.node.Trim()).Max(y => y.Length);
            //var max = waves_max > nodes_max ? waves_max : nodes_max;
            var len = sigs[0].wave.Trim().Length;   
            foreach (var item in sigs)
            {
                if (item.wave.Length < len)
                {
                    item.wave = item.wave.PadRight(len, '.');
                }
                else if (item.wave.Length > len)
                {
                    int x = item.wave.Length;
                    for (int i=item.wave.Length-1; i >= len; i--)
                    {
                        if (item.wave.Substring(i,1)==".")
                        {
                            x--;
                        }
                        else { break; }
                    }
                    item.wave = item.wave.Substring(0, x);
                }
                
                if (item.node.Length < len)
                {
                    item.node = item.node.PadRight(len, '.');
                }
                else if (item.node.Length > len)
                {
                    item.node = item.node.Substring(0, len);    
                }
            }

        }

        private void ImportFile(object sender, RoutedEventArgs e)
        {
            var fsetting = "settings.txt";
            var initDir = Directory.GetCurrentDirectory();
            if (File.Exists(fsetting))
            {
                initDir = File.ReadAllText(fsetting);
            }
            //browse for json file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = initDir;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                JsonFile = openFileDialog.FileName;
                tb.Text = File.ReadAllText(JsonFile);
                //tabs.SelectedIndex = 1;

                //deserilize JsonFile to JSonImportRoot class; System.Text.Json does not work with this json file (no "" around property names). But Newtonsoft.Json does.
                var root = JsonConvert.DeserializeObject<JSonImportRoot>(tb.Text);
                if (root != null && root.signal != null)
                {
                    sigs.Clear();
                    foreach (var item in root.signal)
                    {
                        sigs.Add(new Signal() { name = item.name, wave = item.wave, data = item.data, node = item.node });
                        //if item.node contains the first character of any edge in the edge array, then add the edge to the edge property of the signal
                        if (item.node != null && item.node.Trim().Length > 0)
                        {
                            foreach (var edge in root.edge)
                            {
                                if (item.node.Trim().Contains(edge[0]))
                                {
                                    sigs.Last().edge += edge + ",";
                                }
                            }
                        }
                    }
                     

                }
                else
                {
                    tabs.SelectedIndex = 1;
                    //msgbox to user
                    MessageBox.Show("Check input json file format");
                }
                File.WriteAllText(fsetting, Path.GetDirectoryName(JsonFile));
            }

        }

        private void InsertDot(object sender, RoutedEventArgs e)
        {
            if (txtIdx.Text.Trim().Length > 0)
            {
                int x;
                var b = int.TryParse(txtIdx.Text, out x);
                if (b)
                {
                    foreach (var item in sigs)
                    {
                        item.node = item.node.Insert(x, ".");
                        item.wave = item.wave.Insert(x, ".");
                    }
                }
            }
        }

        private void RemoveDot(object sender, RoutedEventArgs e)
        {
            if (txtIdx.Text.Trim().Length>0)
            {
                int x;
                var b = int.TryParse(txtIdx.Text, out x);
                if (b)
                {
                    foreach(var item in sigs)
                    {
                        item.node = item.node.Remove(x, 1);
                        item.wave = item.wave.Remove(x, 1);
                    }
                }
            }
        }
    }
}
