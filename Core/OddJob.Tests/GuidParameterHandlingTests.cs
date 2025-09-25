using System;
using System.Threading.Tasks;
using Xunit;

namespace GlutenFree.OddJob.Tests
{
    public class GuidParameterHandlingTests
    {
        [Fact]
        public async Task Guid_Parameter_Should_Inject_JobGuid()
        {
            Guid? captured = Guid.NewGuid();
            var next = JobCreator.Create<GuidJob>((i,j) => j.DoGuid(i, captured));
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            GuidJob.CapturedGuid = null;
            await jobEx.ExecuteJobAsync(next);
            Assert.NotNull(GuidJob.CapturedGuid);
            Assert.NotEqual(Guid.Empty, GuidJob.CapturedGuid.Value);
        }

        [Fact]
        public async Task NullableGuid_Parameter_Should_Not_Inject_JobGuid()
        {
            var next = JobCreator.Create<NullableGuidJob>(j => j.DoGuid(null));
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            NullableGuidJob.CapturedGuid = null;
            await jobEx.ExecuteJobAsync(next);
            Assert.Null(NullableGuidJob.CapturedGuid);
        }

        [Fact]
        public async Task Object_Parameter_Should_Not_Inject_JobGuid()
        {
            var next = JobCreator.Create<ObjectJob>(j => j.DoObject(null));
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            ObjectJob.CapturedObject = null;
            await jobEx.ExecuteJobAsync(next);
            Assert.Null(ObjectJob.CapturedObject);
        }

        [Fact]
        public async Task Guid_Parameter_With_Conversion_Should_Not_Inject_JobGuid()
        {
            var next = JobCreator.Create<GuidJob>((gid, j) => j.DoGuid(gid, Guid.NewGuid()));
            var jobEx = new DefaultJobExecutor(new DefaultContainerFactory());
            GuidJob.CapturedGuid = null;
            await jobEx.ExecuteJobAsync(next); 
        }
    }

    public class GuidJob
    {
        public static Guid? CapturedGuid;
        public Guid JobGuid { get; set; }
        public void DoGuid(Guid g, Guid? captured)
        {
            CapturedGuid = g;
            Assert.True(captured.HasValue);
            Assert.NotEqual(g, captured.Value);
        }
    }
    public class NullableGuidJob
    {
        public static Guid? CapturedGuid;
        public void DoGuid(Guid? g)
        {
            CapturedGuid = g;
        }
    }
    public class ObjectJob
    {
        public static object CapturedObject;
        public void DoObject(object o)
        {
            CapturedObject = o;
        }
    }
}

