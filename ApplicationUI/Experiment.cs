using TravellingTaskGeneticSolver;


namespace ApplicationUI
{
    public class Experiment
    {
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }
        public string FilePath { get; set; }

        public Experiment(string newName, string newPath)
        {
            Name = newName;
            CreationDate = DateTime.Now;
            FilePath = newPath;
        }

        public Experiment() { }

    }
}
