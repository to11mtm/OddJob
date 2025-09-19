namespace GlutenFree.OddJob.Manager.Blazor;

public class SampleData
{
    public int Id { get; set; }
}

public class SampleJob
{
    public void DoThing(SampleData data)
    {
        // Do something with data
    }
}