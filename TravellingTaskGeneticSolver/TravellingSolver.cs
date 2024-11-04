using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace TravellingTaskGeneticSolver
{
    public class TravellingSolver
    {
        [JsonProperty]
        public readonly int PopulationCount;

        [JsonProperty]
        public readonly double MutationFrequency;

        [JsonProperty]
        public readonly TravellingDistances DistMatrix;

        [JsonProperty]
        public readonly int CitiesCount;

        [JsonProperty]
        public List<Route>? LastPopulation;

        public Route? BestRoute = null;
        
        public double? BestDistance = null;

        public int Generation = 0;

        [JsonIgnore]
        public bool isRunning = true;
        public event Action<int, double>? OnNewGenIteration;

        public TravellingSolver() { }
        
        public TravellingSolver(TravellingDistances distances, int populationCount, double mutationFrequency)
        {
            DistMatrix = distances;
            CitiesCount = distances.Size;

            PopulationCount = populationCount;
            MutationFrequency = mutationFrequency;

            LastPopulation = new List<Route>();
        }

        private List<Route> InitializePopulation()
        {
            var population = new List<Route>();

            for (int i = 0; i < PopulationCount; i++)
            {
                Route newRoute = new(CitiesCount);
                population.Add(newRoute);
            }

            return population;
        }

        private Route TournamentSelection(List<Route> population, int tournamentSize)
        {
            Random _random = new Random();

            var tournament = new List<Route>();
            for (int i = 0; i < tournamentSize; i++)
            {
                int randomIndex = _random.Next(PopulationCount);
                tournament.Add(population[randomIndex]);
            }

            return tournament.OrderBy(route => route.CalculateTotalDistance(DistMatrix)).First();
        }

        private List<Route> MakeIteration(List<Route> population)
        {
            var newPopulation = new ConcurrentBag<Route>();

            var bestInGeneration = population.OrderBy(route => route.CalculateTotalDistance(DistMatrix)).First();
            newPopulation.Add(bestInGeneration);

            Parallel.For(0, PopulationCount, i =>
            {
                Route parent1 = TournamentSelection(population, 5);
                Route parent2 = TournamentSelection(population, 5);
                Route child = Route.Crossover(parent1, parent2);
                Route newRoute = Route.Mutate(child, MutationFrequency);

                newPopulation.Add(newRoute);
            });
            
            population = newPopulation.ToList();

            foreach (var route in population)
            {
                double routeDistance = route.CalculateTotalDistance(DistMatrix);
                if (routeDistance < BestDistance)
                {
                    BestRoute = route.Clone();
                    BestDistance = routeDistance;
                }
            }

            return population;
        }

        public Route Run(int maxGenerations, out double bestDistance)
        {
            BestRoute = null;
            BestDistance = double.MaxValue;

            var population = InitializePopulation();

            for (int generation = 0; generation < maxGenerations; generation++)
            {
                population = MakeIteration(population);

                OnNewGenIteration?.Invoke(generation, BestDistance.Value);
            }

            bestDistance = BestDistance.Value;
            return BestRoute!;
        }

        public Route? RunInfinitly(out double bestDistance)
        {
            BestRoute = null;
            BestDistance = double.MaxValue;

            if (LastPopulation == null || LastPopulation.Count == 0) {
                LastPopulation = InitializePopulation();
            }

            while (true)
            {
                LastPopulation = MakeIteration(LastPopulation);

                if (!isRunning) break;

                Generation++;
                OnNewGenIteration?.Invoke(Generation, BestDistance.Value);
            }

            bestDistance = BestDistance.Value;
            return BestRoute;
        }
    }
}
