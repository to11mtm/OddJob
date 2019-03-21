using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GlutenFree.OddJob.Storage.Sql.Common.DbDtos;

namespace GlutenFree.OddJob.Storage.Sql.Common
{
    public class JobUpdateCommand
    {
        public Dictionary<Expression<Func<SqlCommonDbOddJobMetaData, object>>, object> SetJobMetadata { get; set; }
        public Dictionary<int, Dictionary<Expression<Func<SqlCommonOddJobParamMetaData, object>>, object>> SetJobParameters
        {
            get;
            set;
        }
        public Guid JobGuid { get; set; }
        public string OldStatusIfRequired { get; set; }
    }
}