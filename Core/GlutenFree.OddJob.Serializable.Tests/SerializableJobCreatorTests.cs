using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace GlutenFree.OddJob.Serializable.Tests
{
    public class SerializableJobCreatorTests
    {
        [ExcludeFromCodeCoverage]
        public class Test
        {
            public void Derp(string val)
            {
                Console.WriteLine(val);
            }
        }

        [Fact]
        public void GetExecutableJobDefinition_Gets_Proper_Data_Back()
        {
            var jobDef = SerializableJobCreator.CreateJobDefinition((Test d) =>
                d.Derp("Hello"));
            var deser =
                SerializableJobCreator.GetExecutableJobDefinition(jobDef);
            Assert.Equal(nameof(Test.Derp), deser.MethodName);
            Assert.Equal("Hello", deser.JobArgs.First().Value);
        }
    }
}