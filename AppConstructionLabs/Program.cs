using TravellingTaskGeneticSolver;

class Program
{
    static void Main()
    {
        int numCities = 200;

        var distanceMatrix = TravellingDistances.GenerateRandom(numCities); // Console.WriteLine(distanceMatrix);

        var solver = new TravellingSolver(distanceMatrix, 20, 0.2);
        solver.OnNewGenIteration += Solver_OnNewGenIteration;

        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("Cancelling calculations...");
            e.Cancel = true;

            solver.isRunning = false;
        };

        double bestResult;
        Route bestRoute = solver.RunInfinitly(out bestResult)!; // solver.Run(2 << 10, out bestResult);

        Console.WriteLine(bestRoute);
    }

    private static void Solver_OnNewGenIteration(int iteration, double bestDistance)
    {
        Console.WriteLine("iters: {0}, best distance: {1:N3}", iteration, bestDistance);
    }
}