﻿namespace GlutenFree.OddJob.Storage.BaseTests
{
    public static class TestConstants
    {
        public static readonly string SimpleParam = "simple";
        public static readonly string OddParam1 = "odd1";
        public static readonly string OddParam2 = "odd2";
        public static readonly string NestedOddParam1 = "n1";
        public static readonly  string NestedOddParam2 = "n2";
    }

    public class MockJobNoParam
    {
        public void DoThing()
        {
            //Intentionally empty, storage test.
        }
    }

    public class MockJobInClassWithArity<TString>
    {
        public void DoThing(TString simpleParam, OddParam oddParam)
        {
            //Intentionally Empty since this is testing storage.
        }
    }

    public class MockJobWithArity
    {
        public void DoThing<TString, TOddParam>(TString simpleParam, TOddParam oddParam)
        {
            //Intentionally empty since this is testing storage.
        }
    }

    public class MockJob
    {
        public void DoThing(string simpleParam, OddParam oddParam)
        {
            //Intentionally empty since this is testing storage.
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

