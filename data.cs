using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace WaveHelper
{
    public class Signal : ObservableObject
    {

        private string _name; 
        public string name { get => _name; set => SetProperty(ref _name, value); }

        private string _wave;
        public string wave { get => _wave; set => SetProperty(ref _wave, value); }
        
        private string _data;
        public string data { get => _data ; set => SetProperty(ref _data, value); } 
        
        private string _node;
        public string node { get => _node; set => SetProperty(ref _node, value); }

        private bool _plot;
        public bool plot { get => _plot; set => SetProperty(ref _plot, value); }

        private string _group;
        public string group { get => _group; set => SetProperty(ref _group, value); }

        private string _edge;
        public string edge { get => _edge; set => SetProperty(ref _edge, value); }

        public Signal()
        {
            _node = "";
            _name = "";
            _wave = "";
            _data = "";
            _plot = true;
            _group = "";
            _edge = "";
        }
    }

    public class Edge
    {
        public string n1 { get; set; } = "";
        public string n2 { get; set; } = "";
        public string desc { get; set; } = "";
        public bool plot { get; set; } = true;
        public Edge()
        {
        }
    }
    public class JSonImportRoot
    {
        public List<Signal> signal { get; set; }
        public List<string> edge { get; set; }
    }
}
