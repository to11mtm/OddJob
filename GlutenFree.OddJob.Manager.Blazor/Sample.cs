namespace GlutenFree.OddJob.Manager.Blazor;

public class SampleData
{
    public int Id { get; set; }
}

public class MoreSampleData
{
    public string Name { get; set; }
}

public class SampleJob
{
    public void DoThing(SampleData data)
    {
        // Do something with data
    }
}

public class SampleJob2
{
    public void DoThing(MoreSampleData data)
    {
        
    }
    
    public void DoOtherThing(SampleData data, MoreSampleData data2)
    {
        
    }
}