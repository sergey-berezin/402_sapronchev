using Newtonsoft.Json;
using System;
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
using System.Windows.Shapes;

namespace ApplicationUI
{
    public partial class SelectionWindow : Window
    {
        public Experiment SelectedExperiment { get; private set; }
        private List<Experiment> _experiments;
        private string runsFilePath;

        public SelectionWindow(List<Experiment> experiments, string runspath)
        {
            InitializeComponent();
            _experiments = experiments;
            ExperimentListBox.ItemsSource = _experiments;
            runsFilePath = runspath;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedExperiment = (Experiment) ExperimentListBox.SelectedItem;
            if (SelectedExperiment != null)
            {
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please select an experiment before proceeding.");
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedExperiment = (Experiment) ExperimentListBox.SelectedItem;
            if (selectedExperiment != null)
            {

                try { 
                    var experiments = JsonConvert.DeserializeObject<List<Experiment>>(File.ReadAllText(runsFilePath));
                    var clearedExperiments = experiments.RemoveAll(exp => exp.Name == selectedExperiment!.Name);

                    var handler = new SafeJsonFileHandler<List<Experiment>>(runsFilePath);
                    handler.SaveData(experiments);

                    File.Delete(selectedExperiment.FilePath);

                    _experiments.Remove(selectedExperiment);
                    ExperimentListBox.Items.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete experiment.");
                }
            }
            else
            {
                MessageBox.Show("Please select an experiment to delete.");
            }
        }
    }
}
