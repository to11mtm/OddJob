using System;
using Xunit;

namespace GlutenFree.OddJob.Tests
{
    public class UnitTest1
    {

        [Fact]
        public void Can_Run_Job_And_pass_parameters()
        {
            var myValue = new ClassTest() { classTestValue = TestConstants.classTestValue };
            var next = JobCreator.Create<SampleJob>((j) => j.DoThing(TestConstants.derp, TestConstants.herp, myValue));
            var jobEx = new OldDefaultJobExecutor(new DefaultContainerFactory());
            jobEx.ExecuteJob(next);
        }

        [Fact]
        public void Can_Run_Job_With_Simple_Method_Calls()
        {
            var myValue = new ClassTest() { classTestValue = TestConstants.classTestValue };
            var next = JobCreator.Create<SampleJob>((j) => j.DoThing(TestConstants.derp, int.Parse(TestConstants.herp.ToString()), myValue));
            var jobEx = new OldDefaultJobExecutor(new DefaultContainerFactory());
            jobEx.ExecuteJob(next);
        }
        [Fact]
        public void Can_Run_Jobs_With_Param_Type_Matching()
        {
            var myValue = new ClassTest() { classTestValue = TestConstants.classTestValue };
            var next = JobCreator.Create<SampleJob>((j) => j.DoThing(TestConstants.derp, TestConstants.herp, myValue));
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            jobEx.ExecuteJob(next);
        }

        [Fact]
        public void Can_Run_Jobs_With_Param_Type_Matching_And_Overloads()
        {
            var myValue = new ClassTest() { classTestValue = TestConstants.classTestValue };
            var next = JobCreator.Create<SampleJob2>((j) => j.DoThing(TestConstants.derp, TestConstants.herp, myValue));
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            jobEx.ExecuteJob(next);
        }

        [Fact]
        public void Can_Run_Jobs_With_No_Params()
        {
            var next = JobCreator.Create<SampleJobNoParam>((j) => j.DoThing());
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            jobEx.ExecuteJob(next);
            Xunit.Assert.True(SampleJobNoParam.Called);
        }

    }

    public class ClassTest
    {
        public int classTestValue { get; set; }
    }
    public class TestConstants
    {
        public const string derp = "lol";
        public const int herp = 9001;
        public const int classTestValue = 9002;
    }

    public class SampleJobNoParam
    {
        public static bool Called = false;
        public void DoThing()
        {
            Called = true;
        }
    }

    public class SampleJob
    {
        public void DoThing(string derp, long herp, ClassTest aClass)
        {
            Xunit.Assert.Equal(derp, TestConstants.derp);
            Xunit.Assert.Equal(herp, TestConstants.herp);
            Xunit.Assert.Equal(aClass.classTestValue, TestConstants.classTestValue);
            System.Console.WriteLine("derp");
        }
    }

    public class SampleJob2
    {
        public void DoThing(string derp, long herp, ClassTest aClass)
        {
            Xunit.Assert.Equal(derp, TestConstants.derp);
            Xunit.Assert.Equal(herp, TestConstants.herp);
            Xunit.Assert.Equal(aClass.classTestValue, TestConstants.classTestValue);
            System.Console.WriteLine("derp");
        }

        public void DoThing(object derp, object herp, object aClass)
        {
            throw new Exception("Should not be called!");
        }
    }
}
