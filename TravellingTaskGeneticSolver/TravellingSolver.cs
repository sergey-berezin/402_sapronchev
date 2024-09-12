namespace TravellingTaskGeneticSolver
{
    public class TravellingSolver
    {
        private Random _random = new Random();

        private readonly int _populationCount;
        private readonly double _mutationFrequency;

        private readonly TravellingDistances _distMatrix;
        private readonly int _citiesCount;

        private Route? _bestRoute = null;
        private double? _bestDistance = null;

        public bool isRunning = true;
        public event Action<int, double>? OnNewGenIteration;


        public TravellingSolver(TravellingDistances distances, int populationCount, double mutationFrequency)
        {
            _distMatrix = distances;
            _citiesCount = distances.Size;

            _populationCount = populationCount;
            _mutationFrequency = mutationFrequency;
        }

        private List<Route> InitializePopulation()
        {
            var population = new List<Route>();

            for (int i = 0; i < _populationCount; i++)
            {
                Route newRoute = new(_citiesCount);
                population.Add(newRoute);
            }

            return population;
        }

        private Route TournamentSelection(List<Route> population, int tournamentSize)
        {
            var tournament = new List<Route>();
            for (int i = 0; i < tournamentSize; i++)
            {
                int randomIndex = _random.Next(_populationCount);
                tournament.Add(population[randomIndex]);
            }

            return tournament.OrderBy(route => route.CalculateTotalDistance(_distMatrix)).First();
        }

        private List<Route> MakeIteration(List<Route> population)
        {
            var newPopulation = new List<Route>();

            var bestInGeneration = population.OrderBy(route => route.CalculateTotalDistance(_distMatrix)).First();
            newPopulation.Add(bestInGeneration);

            for (int i = 0; i < _populationCount; i++)
            {
                Route parent1 = TournamentSelection(population, 5);
                Route parent2 = TournamentSelection(population, 5);
                Route child = Route.Crossover(parent1, parent2);
                child.Mutate(_mutationFrequency);

                newPopulation.Add(child);
            }

            population = newPopulation;

            foreach (var route in population)
            {
                double routeDistance = route.CalculateTotalDistance(_distMatrix);
                if (routeDistance < _bestDistance)
                {
                    _bestRoute = route.Clone();
                    _bestDistance = routeDistance;
                }
            }

            return newPopulation;
        }
        public Route Run(int maxGenerations, out double bestDistance)
        {
            _bestRoute = null;
            _bestDistance = double.MaxValue;

            var population = InitializePopulation();

            for (int generation = 0; generation < maxGenerations; generation++)
            {
                population = MakeIteration(population);

                OnNewGenIteration?.Invoke(generation, _bestDistance.Value);
            }

            bestDistance = _bestDistance.Value;
            return _bestRoute!;
        }

        public Route? RunInfinitly(out double bestDistance)
        {
            _bestRoute = null;
            _bestDistance = double.MaxValue;

            var generation = 0;
            var population = InitializePopulation();

            while (true)
            {
                population = MakeIteration(population);

                if (!isRunning) break;

                generation++;
                OnNewGenIteration?.Invoke(generation, _bestDistance.Value);
            }

            bestDistance = _bestDistance.Value;
            return _bestRoute;
        }
    }
}
