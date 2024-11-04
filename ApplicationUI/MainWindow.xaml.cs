using LiveCharts;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Windows;
using TravellingTaskGeneticSolver;
using System.Reflection;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Input;
using System.IO;
using Microsoft.VisualBasic;


public class PrivateFieldsContractResolver : DefaultContractResolver
{
    protected override List<MemberInfo> GetSerializableMembers(Type objectType)
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var members = objectType.GetFields(flags);

        return new List<MemberInfo>(members);
    }
}


namespace ApplicationUI
{
    public class City
    {
        public int Index { get; set; }
        public double Price { get; set; }
    }

    public partial class MainWindow : Window
    {
        private TravellingSolver? _solver;
        public ChartValues<double> MetricValues { get; set; } = new ChartValues<double>();
        private const int iterationsPerDot = 200;
        private JsonSerializerSettings JsonSettings;

        private String openedExperimentName = "Default";
        const String runsFilePath = "runs.json";

        public MainWindow()
        {
            InitializeComponent();

            EmptyRouteTable();

            YAxis.LabelFormatter = (value) => value.ToString("F2");
            XAxis.LabelFormatter = (value) => value.ToString("F0");

            JsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new PrivateFieldsContractResolver()
            };
        }

        private void EmptyRouteTable()
        {
            ShortestPathDataGrid.ItemsSource = new List<City> { };

        }
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            EmptyRouteTable();
            SaveButton.IsEnabled = true;

            int numCities = int.Parse(NumCitiesInput.Text);
            int populationSize = int.Parse(PopulationInput.Text);
            double mutationRate = double.Parse(MutationRateInput.Text);

            var distanceMatrix = TravellingDistances.GenerateRandom(numCities);

            _solver = new TravellingSolver(distanceMatrix, populationSize, mutationRate);
            _solver.OnNewGenIteration += Solver_OnNewGenIteration;

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            ContinueButton.IsEnabled = false;

            var best = await Task.Factory.StartNew(() =>
            {
                double bestDistance = 0.0;
                Route bestRoute = _solver?.RunInfinitly(out bestDistance)!;
                return bestRoute;
            }, TaskCreationOptions.LongRunning);

            string json = JsonConvert.SerializeObject(_solver, JsonSettings);
            
            GenerateOutput(best, distanceMatrix);

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            ContinueButton.IsEnabled = false;

            MetricValues = new ChartValues<double>();
        }

        private async void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            EmptyRouteTable();
            SaveButton.IsEnabled = true;

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            ContinueButton.IsEnabled = false;

            var best = await Task.Factory.StartNew(() =>
            {
                double bestDistance = 0.0;
                Route bestRoute = _solver?.RunInfinitly(out bestDistance)!;
                return bestRoute;
            }, TaskCreationOptions.LongRunning);

            GenerateOutput(best, _solver.DistMatrix);

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            ContinueButton.IsEnabled = false;

            MetricValues = new ChartValues<double>();
        }

        private void GenerateOutput(Route bestRoute, TravellingDistances distances)
        {
            var cityList = new List<City> { };
            var originalCities = bestRoute.Cities;
            var citiesCount = originalCities.Length;
            for (int i = 0; i < citiesCount - 1; i++)
            {
                var cityOut = originalCities[i];
                var cityTo = originalCities[i + 1];

                cityList.Add(new City { Index = cityOut, Price = distances.GetDistance(cityOut, cityTo) });
            }

            cityList.Add(new City { Index = bestRoute.Cities[citiesCount - 1],
                Price = distances.GetDistance(bestRoute.Cities[citiesCount - 1], bestRoute.Cities[0]) });

            ShortestPathDataGrid.ItemsSource = cityList;

        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_solver != null)
            {
                _solver.isRunning = false;
            }
        }

        private void Solver_OnNewGenIteration(int iteration, double bestDistance)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressText.Text = $"Generation: {iteration}";
                BestDistanceText.Text = $"Best distance: {bestDistance:N3}";

                if (iteration % iterationsPerDot == 0)
                {
                    MetricValues.Add(bestDistance);
                    UpdateChart();
                }
            });
        }

        private void UpdateChart()
        {
            var new_labels = new List<string> { };
            for (int i = 0; i < MetricValues.Count + 1; i++)
            {
                new_labels.Add($"{i * iterationsPerDot}");
            }

            XAxis.Labels = new_labels;

            MyLineSeries.Values = MetricValues;
            MetricsChart.Update();
        }

        public void SaveState_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;

            if (_solver != null)
            {
                _solver.isRunning = false;
            }

            var experimentName = "";
            var nameInputDialog = new NameInputDialog(openedExperimentName);
            if (nameInputDialog.ShowDialog() == true)
            {
                experimentName = nameInputDialog.ExperimentName;
                if (LoadExperimentData(experimentName) != null)
                {
                    MessageBox.Show($"Existing experiment would be rewritten");
                }
            }
            else
            {
                return;
            }

            List<Experiment> experiments;
            if (File.Exists(runsFilePath))
            {
                experiments = JsonConvert.DeserializeObject<List<Experiment>>(File.ReadAllText(runsFilePath));
            }
            else
            {
                experiments = new List<Experiment>();
            }

            var solverFileName = $"{experimentName}_solver.json";
            var handler = new SafeJsonFileHandler<TravellingSolver>(solverFileName);
            handler.SaveData(_solver);

            Experiment? newExper = LoadExperimentData(experimentName);
            if (newExper != null) return;

            newExper = new Experiment(experimentName, solverFileName);
            experiments.Add(new Experiment(experimentName, solverFileName));

            var experhandler = new SafeJsonFileHandler<List<Experiment>>(runsFilePath);
            experhandler.SaveData(experiments);
        }

        public Experiment? LoadExperimentData(string experimentName)
        {

            if (!File.Exists(runsFilePath)) return null;

            var experiments = JsonConvert.DeserializeObject<List<Experiment>>(File.ReadAllText(runsFilePath));
            var selectedExperiment = experiments.FirstOrDefault(exp => exp.Name == experimentName);

            return selectedExperiment;
        }

        public List<Experiment> LoadAllExperiments()
        {
            if (!File.Exists(runsFilePath)) { 
                return new List<Experiment>();
            };

            var experiments = JsonConvert.DeserializeObject<List<Experiment>>(File.ReadAllText(runsFilePath))!;

            return experiments;
        }

        private bool LoadSolver(string filePath)
        {
            if (!File.Exists(filePath)) return false;

            TravellingSolver loadedSolver = JsonConvert.DeserializeObject<TravellingSolver>(File.ReadAllText(filePath));
            if (loadedSolver == null)
            {
                return false;
            }

            _solver = loadedSolver;
            _solver.OnNewGenIteration += Solver_OnNewGenIteration;
            
            NumCitiesInput.Text = _solver.CitiesCount.ToString();
            PopulationInput.Text = _solver.PopulationCount.ToString();
            MutationRateInput.Text = _solver.MutationFrequency.ToString();

            ProgressText.Text = $"Generation: {_solver.Generation}";
            BestDistanceText.Text = $"Best distance: {_solver.BestDistance:N3}";

            if (_solver.BestRoute != null) { 
                GenerateOutput(_solver.BestRoute, _solver.DistMatrix);
            }

            return true;
        }

        private void LoadState_Click(object sender, RoutedEventArgs e)
        {
            List<Experiment> experiments = LoadAllExperiments();
            if (experiments.Count == 0)
            {
                MessageBox.Show($"No experiments are created yet.");
                return;
            }

            SelectionWindow selectionWindow = new SelectionWindow(experiments, runsFilePath);

            if (selectionWindow.ShowDialog() == false) return; 
            
            Experiment selectedExperiment = selectionWindow.SelectedExperiment;
            if (!LoadSolver(selectedExperiment.FilePath))
            {
                MessageBox.Show($"Failed to load experiment.");
                return;
            };

            openedExperimentName = selectedExperiment.Name;

            ContinueButton.IsEnabled = true;
        }

    }
}