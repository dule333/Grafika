using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace WpfApp1.DIalogs
{
    /// <summary>
    /// Interaction logic for Polygon.xaml
    /// </summary>
    public partial class PolygonWindow : Window
    {
        private bool modify = false;
        public double NewThickness { get; set; }
        public SolidColorBrush NewFill { get; set; }
        public SolidColorBrush NewStroke { get; set; }
        public string NewText { get; set; }
        public SolidColorBrush NewTextColor { get; set; }
        public PolygonWindow()
        {
            InitializeComponent();
            DataContext = this;
            fill.ItemsSource = typeof(Colors).GetProperties();
            stroke.ItemsSource = typeof(Colors).GetProperties();
            textColor.ItemsSource = typeof(Colors).GetProperties();
        }
        public PolygonWindow(bool modify)
        {
            InitializeComponent();
            DataContext = this;
            fill.ItemsSource = typeof(Colors).GetProperties();
            stroke.ItemsSource = typeof(Colors).GetProperties();
            textColor.ItemsSource = typeof(Colors).GetProperties();
            text.IsReadOnly = modify;
            textColor.IsReadOnly = modify;
            this.modify = modify;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string[] splitted;
            if (!modify)
            {
                NewText = text.Text;
                splitted = textColor.SelectedItem.ToString().Split(' ');
                NewTextColor = (SolidColorBrush)new BrushConverter().ConvertFromString(splitted[1]);
            }
            NewThickness = Convert.ToDouble(thickness.Text);
            splitted = fill.SelectedItem.ToString().Split(' ');
            Color color = ((SolidColorBrush)new BrushConverter().ConvertFromString(splitted[1])).Color;
            color.A = Byte.Parse(((int)(polygonOpacity.Value * 255)).ToString());
            NewFill = new SolidColorBrush(color);
            splitted = stroke.SelectedItem.ToString().Split(' ');
            NewStroke = (SolidColorBrush)new BrushConverter().ConvertFromString(splitted[1]);
            this.DialogResult = true;
            this.Close();
        }
    }
}
