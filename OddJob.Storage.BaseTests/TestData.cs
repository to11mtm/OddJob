namespace GlutenFree.OddJob.Storage.BaseTests
{
    public class TestConstants
    {
        public const string SimpleParam = "simple";
        public const string OddParam1 = "odd1";
        public const string OddParam2 = "odd2";
        public const string NestedOddParam1 = "n1";
        public const string NestedOddParam2 = "n2";
    }

    public class MockJob
    {
        public void DoThing(string simpleParam, OddParam oddParam)
        {

        }
    }

    public class OddParam
    {
        public string Param1 { get; set; }
        public string Param2 { get; set; }
        public NestedOddParam Nested { get; set; }
    }

    public class NestedOddParam
    {
        public string NestedParam1 { get; set; }
        public string NestedParam2 { get; set; }
    }
}

