using System;
using System.Diagnostics;

namespace GlutenFree.OddJob._PerfTests
{
    public class Class1
    {
        [Fact]
        public void PerfTest()
        {
            var sw1  = Stopwatch.StartNew();
            for (int i = 0; i < 100000; i++)
            {
                var myvalue = new ClassTest() { classTestValue = TestConstants.classTestValue };
                var next = JobCreator.Create<SampleJobInGenericClass<string>>(j =>
                    j.DoThing(TestConstants.derp, TestConstants.herp, myvalue));
            }
            sw1.Stop();
            Assert.True(sw1.Elapsed.TotalSeconds<10);
            
        }
    }
}