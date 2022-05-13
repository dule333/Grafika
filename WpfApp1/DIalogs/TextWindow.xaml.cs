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
    public partial class TextWindow : Window
    {
        private bool modify = false;
        public double NewSize { get; set; }
        public string NewText { get; set; }
        public SolidColorBrush NewTextColor { get; set; }
        public TextWindow()
        {
            InitializeComponent();
            DataContext = this;
            textColor.ItemsSource = typeof(Colors).GetProperties();
        }
        public TextWindow(bool modify)
        {
            InitializeComponent();
            DataContext = this;
            textColor.ItemsSource = typeof(Colors).GetProperties();
            text.IsReadOnly = modify;
            this.modify = modify;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(!modify)
                NewText = text.Text;
            NewSize = Convert.ToDouble(size.Text);
            string[] splitted;
            splitted = textColor.SelectedItem.ToString().Split(' ');
            NewTextColor = (SolidColorBrush)new BrushConverter().ConvertFromString(splitted[1]);
            this.DialogResult = true;
            this.Close();
        }
    }
}
