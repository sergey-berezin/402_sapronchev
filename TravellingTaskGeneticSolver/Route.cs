namespace TravellingTaskGeneticSolver
{
    public class Route
    {
        private int[] _cities;
        public int[] Cities
        {
            get { return (int[])_cities.Clone(); }
        }

        private Route(int[] cities)
        {
            _cities = (int[])cities.Clone();
        }

        internal Route(int numCities)
        {
            Random _random = new Random();
            _cities = Enumerable.Range(0, numCities).ToArray();
            _cities = _cities.OrderBy(x => _random.Next()).ToArray();
        }

        internal double CalculateTotalDistance(TravellingDistances distanceMatrix)
        {
            double totalDistance = 0.0;
            for (int i = 0; i < _cities.Length - 1; i++)
            {
                totalDistance += distanceMatrix.GetDistance(_cities[i], _cities[i + 1]);
            }
            totalDistance += distanceMatrix.GetDistance(_cities[_cities.Length - 1], _cities[0]);
            return totalDistance;
        }

        internal static Route Mutate(Route route, double mutationRate)
        {
            Route mutatedRoute = route.Clone();

            Random _random = new Random();
            if (_random.NextDouble() < mutationRate)
            {
                int firstIndex = _random.Next(route._cities.Length);
                int secondIndex = _random.Next(route._cities.Length);

                mutatedRoute._cities[firstIndex] = route._cities[secondIndex];
                mutatedRoute._cities[secondIndex] = route._cities[firstIndex];
            }

            return mutatedRoute;
        }

        internal static Route Crossover(Route parent1, Route parent2)
        {
            int[] firstParentCities = parent1.Cities;
            int[] secondParentCities = parent2.Cities;

            Random _random = new Random();
            int start = _random.Next(0, firstParentCities.Length / 2);
            int end =_random.Next(start + 1, firstParentCities.Length);

            var childCities = new int[firstParentCities.Length];
            Array.Fill(childCities, -1);

            for (int i = start; i <= end; i++)
            {
                childCities[i] = firstParentCities[i];
            }

            int currentPos = 0;
            for (int i = 0; i < secondParentCities.Length; i++)
            {
                if (!childCities.Contains(secondParentCities[i]))
                {
                    while (childCities[currentPos] != -1) currentPos++;
                    childCities[currentPos] = secondParentCities[i];
                }
            }

            return new Route(childCities);
        }

        public Route Clone()
        {
            return new Route(_cities);
        }

        public override string ToString()
        {
            string result = "Best route by cities ids: ";
            foreach (var city in _cities)
            {
                result += city + " - ";
            }
            result += $"{_cities[0]}";
            return result;
        }
    }
}
