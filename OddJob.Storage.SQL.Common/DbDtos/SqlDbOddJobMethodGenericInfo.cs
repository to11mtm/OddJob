using System;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.Sql.Common.DbDtos
{
    public class SqlDbOddJobMethodGenericInfo
    {
        [Identity]
        [PrimaryKey]
        public long Id { get; set; }
        public Guid JobGuid { get; set; }
        public int ParamOrder { get; set; }
        public string ParamTypeName { get; set; }
    }
}
