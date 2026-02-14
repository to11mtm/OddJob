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
        [Column(CanBeNull = false, Length = 1024)]
        public string ParamTypeName { get; set; }
    }
}
