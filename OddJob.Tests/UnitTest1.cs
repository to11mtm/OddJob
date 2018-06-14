using System;
using Xunit;

namespace OddJob.Tests
{
    public class UnitTest1
    {

        [Fact]
        public void Can_Run_Job_And_pass_parameters()
        {
            var myValue = new ClassTest() { classTestValue = TestConstants.classTestValue };
            var next = JobCreator.Create<SampleJob>((j) => j.DoThing(TestConstants.derp, TestConstants.herp, myValue));
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            jobEx.ExecuteJob(next);
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

    public class SampleJob
    {
        public void DoThing(string derp, int herp, ClassTest aClass)
        {
            Xunit.Assert.Equal(derp, TestConstants.derp);
            Xunit.Assert.Equal(herp, TestConstants.herp);
            Xunit.Assert.Equal(aClass.classTestValue, TestConstants.classTestValue);
            System.Console.WriteLine("derp");
        }
    }
}
