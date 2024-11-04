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

namespace ApplicationUI
{
    public partial class NameInputDialog : Window
    {
        public string ExperimentName { get; private set; }

        public NameInputDialog(string DefaultName)
        {
            InitializeComponent();

            NameTextBox.Text = DefaultName;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ExperimentName = NameTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
