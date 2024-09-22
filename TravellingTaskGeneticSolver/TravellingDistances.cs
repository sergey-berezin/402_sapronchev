namespace TravellingTaskGeneticSolver
{
    public class TravellingDistances
    {
        private double[][] _matrix;
        private readonly int _size;
        public int Size
        {
            get { return _size; }
        }

        public TravellingDistances(int numCities)
        {
            _matrix = new double[numCities][];
            _size = numCities;
        }

        public static TravellingDistances GenerateRandom(int numCities)
        {
            var distanceMatrix = new TravellingDistances(numCities);

            Random random = new Random();
            for (int i = 0; i < numCities - 1; i++)
            {
                double[] newRow = new double[numCities - i - 1];
                for (int j = 0; j < numCities - i - 1; j++)
                {
                    var dist = random.NextDouble() * 100;
                    for (int k = 0; k < i; k++)
                    {
                        // Ensure triangle inequality holds
                        dist = Math.Min(dist, distanceMatrix.GetDistance(i, k) + distanceMatrix.GetDistance(k, j));
                    }
                    newRow[j] = dist;

                }
                distanceMatrix.SetDistanceRow(i, newRow);
            }
            return distanceMatrix;
        }

        public void SetDistanceRow(int index, double[] costs)
        {
            if (costs.Length != (_size - index - 1))
            {
                throw new InvalidOperationException("Inappropriate size of a row");
            }
            _matrix[index] = costs;
        }

        public double GetDistance(int i, int j)
        {
            if (i == j)
                return 0.0;
            else if (i > j)
                return _matrix[j][i - j - 1];
            return _matrix[i][j - i - 1];
        }

        public override string ToString()
        {
            string result = "";
            for (int i = 0; i < Size - 1; i++)
            {
                result += string.Concat(Enumerable.Repeat("\t", i));
                for (int j = 0; j < _matrix[i].Length; j++)
                {
                    result += _matrix[i][j].ToString("N2") + "\t";
                }
                result += "\n";
            }
            return result;
        }
    }
}
