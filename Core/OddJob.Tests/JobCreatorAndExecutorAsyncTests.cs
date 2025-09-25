using System;
using System.Threading.Tasks;
using Xunit;

namespace GlutenFree.OddJob.Tests
{
    public class JobCreatorAndExecutorAsyncTests
    {
        [Fact]
        public async Task Can_Run_Async_Job_With_Arity_in_class_With_Arity()
        {
            var myvalue = new ClassTest() { classTestValue = TestConstants.classTestValue };
            var next = JobCreator.Create<AsyncSampleJobInGenericClass<string>>(j =>
                j.DoThingAsync(TestConstants.derp, TestConstants.herp, myvalue));
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            await jobEx.ExecuteJobAsync(next);
        }

        [Fact]
        public async Task Can_Run_Async_Job_With_Arity()
        {
            var myvalue = new ClassTest() { classTestValue = TestConstants.classTestValue };
            var next = JobCreator.Create<AsyncSampleJobWithGenericType>(j =>
                j.DoThingAsync(TestConstants.derp, TestConstants.herp, myvalue));
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            await jobEx.ExecuteJobAsync(next);
        }

        [Fact]
        public async Task Can_Run_Async_Job_And_pass_parameters()
        {
            var myValue = new ClassTest() { classTestValue = TestConstants.classTestValue };
            var next = JobCreator.Create<AsyncSampleJob>((j) => j.DoThingAsync(TestConstants.derp, TestConstants.herp, myValue));
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            await jobEx.ExecuteJobAsync(next);
        }

        [Fact]
        public async Task Can_Run_Async_Job_With_Simple_Method_Calls()
        {
            var myValue = new ClassTest() { classTestValue = TestConstants.classTestValue };
            var next = JobCreator.Create<AsyncSampleJob>((j) => j.DoThingAsync(TestConstants.derp, int.Parse(TestConstants.herp.ToString()), myValue));
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            await jobEx.ExecuteJobAsync(next);
        }

        [Fact]
        public async Task Can_Run_Async_Jobs_With_Param_Type_Matching()
        {
            var myValue = new ClassTest() { classTestValue = TestConstants.classTestValue };
            var next = JobCreator.Create<AsyncSampleJob>((j) => j.DoThingAsync(TestConstants.derp, TestConstants.herp, myValue));
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            await jobEx.ExecuteJobAsync(next);
        }

        [Fact]
        public async Task Can_Run_Async_Jobs_With_Param_Type_Matching_And_Overloads()
        {
            var myValue = new ClassTest() { classTestValue = TestConstants.classTestValue };
            var next = JobCreator.Create<AsyncSampleJob2>((j) => j.DoThingAsync(TestConstants.derp, TestConstants.herp, myValue));
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            await jobEx.ExecuteJobAsync(next);
        }

        [Fact]
        public async Task Can_Run_Async_Jobs_With_No_Params()
        {
            var next = JobCreator.Create<AsyncSampleJobNoParam>((j) => j.DoThingAsync());
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            await jobEx.ExecuteJobAsync(next);
            Assert.True(AsyncSampleJobNoParam.Called);
        }

        [Fact]
        public async Task Can_Run_Async_Jobs_With_Static_Method()
        {
            var next = JobCreator.Create<AsyncSampleJobStaticMethod>((j) => AsyncSampleJobStaticMethod.DoThingAsync());
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            await jobEx.ExecuteJobAsync(next);
            Assert.True(AsyncSampleJobStaticMethod.Called);
        }

        [Fact]
        public async Task Can_Run_Async_Static_Method_On_Static_Class()
        {
            var next = JobCreator.Create<object>((j) => AsyncSampleJobStaticClass.DoThingAsync());
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            await jobEx.ExecuteJobAsync(next);
            Assert.True(AsyncSampleJobStaticClass.Called);
        }

        [Fact]
        public async Task Can_Get_Guid_From_Async_Job_In_Params()
        {
            var next = JobCreator.Create<AsyncSampleJobWithGuid>((j, g) => g.DoThingAsync(j, TestConstants.derp));
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            await jobEx.ExecuteJobAsync(next);
        }

        [Fact]
        public async Task Can_Use_Async_Struct_Param()
        {
            var next = JobCreator.Create<AsyncStructJob>((j) => j.DoThingAsync(new StructParam(4)));
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            await jobEx.ExecuteJobAsync(next);
        }

        [Fact]
        public async Task Creator_Respects_Async_StaticJob_Handling()
        {
            var next = JobCreator.Create<object>((j) => AsyncSampleJobStaticClass.DoThingAsync());
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            await jobEx.ExecuteJobAsync(next);
            Assert.True(AsyncSampleJobStaticClass.Called);
        }

        [Fact]
        public async Task Can_Run_Async_Job_That_Returns_Int()
        {
            var next = JobCreator.Create<AsyncReturnIntJob>(j => j.GetNumberAsync(21));
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            var result = await jobEx.ExecuteJobAsync(next);
            Assert.NotNull(result);
            Assert.Equal(typeof(int), result.ReturnType);
            Assert.IsType<int>(result.Result);
            Assert.Equal(42, (int)result.Result);
        }

        [Fact]
        public async Task Can_Run_Async_Job_That_Returns_String()
        {
            var next = JobCreator.Create<AsyncReturnStringJob>(j => j.GetStringAsync("nyan"));
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            var result = await jobEx.ExecuteJobAsync(next);
            Assert.NotNull(result);
            Assert.Equal(typeof(string), result.ReturnType);
            Assert.IsType<string>(result.Result);
            Assert.Equal("nyan-nyan", (string)result.Result);
        }

        [Fact]
        public async Task Can_Run_Async_Static_Job_That_Returns_Value()
        {
            var next = JobCreator.Create<object>(j => AsyncReturnStaticJob.GetStaticValueAsync());
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            var result = await jobEx.ExecuteJobAsync(next);
            Assert.NotNull(result);
            Assert.Equal(typeof(int), result.ReturnType);
            Assert.IsType<int>(result.Result);
            Assert.Equal(9001, (int)result.Result);
        }

        [Fact]
        public async Task Can_Run_Async_Job_That_Returns_Object()
        {
            var next = JobCreator.Create<AsyncReturnObjectJob>(j => j.GetObjectAsync("uwu", 7));
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            var result = await jobEx.ExecuteJobAsync(next);
            Assert.NotNull(result);
            Assert.Equal(typeof(ReturnTestObj), result.ReturnType);
            var obj = Assert.IsType<ReturnTestObj>(result.Result);
            Assert.Equal("uwu", obj.Name);
            Assert.Equal(7, obj.Count);
        }
    }

    // Async job classes
    public class AsyncSampleJob
    {
        public async Task DoThingAsync(string derp, long herp, ClassTest aClass)
        {
            await Task.Delay(1); // Simulate async work
            Assert.Equal(derp, TestConstants.derp);
            Assert.Equal(herp, TestConstants.herp);
            Assert.Equal(aClass.classTestValue, TestConstants.classTestValue);
        }
    }

    public class AsyncSampleJobInGenericClass<TString>
    {
        public async Task DoThingAsync<TLong, TClass>(TString derp, TLong herp, TClass aClass)
        {
            await Task.Delay(1);
            Assert.Equal(TestConstants.derp, derp as string);
            Assert.Equal(TestConstants.herp, Convert.ToInt64(herp));
            Assert.Equal(TestConstants.classTestValue, (aClass as ClassTest).classTestValue);
        }
    }

    public class AsyncSampleJobWithGenericType
    {
        public async Task DoThingAsync<TString, TLong, TClass>(TString derp, TLong herp, TClass aClass)
        {
            await Task.Delay(1);
            Assert.Equal(TestConstants.derp, derp as string);
            Assert.Equal(TestConstants.herp, Convert.ToInt64(herp));
            Assert.Equal(TestConstants.classTestValue, (aClass as ClassTest).classTestValue);
        }
    }

    public class AsyncSampleJob2
    {
        public async Task DoThingAsync(string derp, long herp, ClassTest aClass)
        {
            await Task.Delay(1);
            Assert.Equal(TestConstants.derp, derp);
            Assert.Equal(TestConstants.herp, herp);
            Assert.Equal(TestConstants.classTestValue, aClass.classTestValue);
        }

        public Task DoThingAsync(object derp, object herp, object aClass)
        {
            throw new Exception("Should not be called!");
        }
    }

    public class AsyncSampleJobNoParam
    {
        public static bool Called = false;
        public async Task DoThingAsync()
        {
            await Task.Delay(1);
            Called = true;
        }
    }

    public class AsyncSampleJobStaticMethod
    {
        public static bool Called = false;
        public static async Task DoThingAsync()
        {
            await Task.Delay(1);
            Called = true;
        }
    }

    public static class AsyncSampleJobStaticClass
    {
        public static bool Called = false;
        public static async Task DoThingAsync()
        {
            await Task.Delay(1);
            Called = true;
        }
    }

    public class AsyncSampleJobWithGuid
    {
        public async Task DoThingAsync(Guid g, string p)
        {
            await Task.Delay(1);
            Assert.NotEqual(Guid.Empty, g);
            Assert.NotNull(p);
            Assert.Equal(TestConstants.derp, p);
        }
    }
    public class AsyncStructJob
    {
        public async Task DoThingAsync(StructParam t)
        {
            await Task.Delay(1);
            Assert.Equal(4, t.Val);
        }
    }
    
    public class AsyncReturnIntJob
    {
        public async Task<int> GetNumberAsync(int x)
        {
            await Task.Delay(1);
            return x * 2;
        }
    }
    public class AsyncReturnStringJob
    {
        public async Task<string> GetStringAsync(string s)
        {
            await Task.Delay(1);
            return s + "-nyan";
        }
    }
    public class AsyncReturnStaticJob
    {
        public static async Task<int> GetStaticValueAsync()
        {
            await Task.Delay(1);
            return 9001;
        }
    }
    public class AsyncReturnObjectJob
    {
        public async Task<ReturnTestObj> GetObjectAsync(string name, int count)
        {
            await Task.Delay(1);
            return new ReturnTestObj { Name = name, Count = count };
        }
    }
    public class ReturnTestObj
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }
}
