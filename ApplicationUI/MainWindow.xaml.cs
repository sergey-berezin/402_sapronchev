using LiveCharts;
using System.Windows;
using TravellingTaskGeneticSolver;

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

        public MainWindow()
        {
            InitializeComponent();

            EmptyRouteTable();

            YAxis.LabelFormatter = (value) => value.ToString("F2");
            XAxis.LabelFormatter = (value) => value.ToString("F0");
        }

        private void EmptyRouteTable()
        {
            ShortestPathDataGrid.ItemsSource = new List<City> { };

        }
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            EmptyRouteTable();

            int numCities = int.Parse(NumCitiesInput.Text);
            int populationSize = int.Parse(PopulationInput.Text);
            double mutationRate = double.Parse(MutationRateInput.Text);

            var distanceMatrix = TravellingDistances.GenerateRandom(numCities);

            _solver = new TravellingSolver(distanceMatrix, populationSize, mutationRate);
            _solver.OnNewGenIteration += Solver_OnNewGenIteration;

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;

            var best = await Task.Run(() =>
            {
                double bestDistance = 0.0;
                Route bestRoute = _solver?.RunInfinitly(out bestDistance)!;
                return bestRoute;
            });

            GenerateOutput(best, distanceMatrix);

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;

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
    }
}