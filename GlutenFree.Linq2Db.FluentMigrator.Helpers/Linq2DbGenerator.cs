using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Generic;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.SQLite;
using LinqToDB.DataProvider.SqlServer;
using LinqToDB.Mapping;

namespace GlutenFree.Linq2Db.FluentMigrator.Helpers
{
    public class Linq2DbGenerator
    {
        private readonly GenericGenerator _generator;
        private readonly DataConnection _dataConnection;

        public Linq2DbGenerator(GenericGenerator generator, DataConnection dataConnection)
        {
            _generator = generator;
            _dataConnection = dataConnection;
        }

        public string CreateTableFor<T>(string tableName = "", bool nonClusteredPrimaryKey = false)
        {
            var create = new CreateTableExpression();
            var ed = _dataConnection.MappingSchema.GetEntityDescriptor(typeof(T));

            create.TableName = string.IsNullOrWhiteSpace(tableName) ? ed.Name.Name : tableName;
            create.SchemaName = ed.Name.Schema;
            create.Columns = ed.Columns.Select(c => GetColumnInfo(create.TableName, c, nonClusteredPrimaryKey)).ToList();
            return _generator.Generate(create);
        }

        private static string BuildSqlType(ColumnDescriptor col)
        {
            if (!string.IsNullOrWhiteSpace(col.DbType))
                return col.DbType;

            var dt = col.DataType;
            var len = col.Length;
            var prec = col.Precision;
            var scale = col.Scale;

            switch (dt)
            {
                case LinqToDB.DataType.Int64: return "BIGINT";
                case LinqToDB.DataType.Int32: return "INT";
                case LinqToDB.DataType.Int16: return "SMALLINT";
                case LinqToDB.DataType.Byte: return "TINYINT";
                case LinqToDB.DataType.Boolean: return "BIT";
                case LinqToDB.DataType.Guid: return "UNIQUEIDENTIFIER";
                case LinqToDB.DataType.Decimal:
                case LinqToDB.DataType.Money:
                case LinqToDB.DataType.SmallMoney:
                    return prec > 0 ? $"DECIMAL({prec},{(scale > 0 ? scale : 0)})" : "DECIMAL";
                case LinqToDB.DataType.Double: return "FLOAT";
                case LinqToDB.DataType.Single: return "REAL";
                case LinqToDB.DataType.NVarChar: return len > 0 ? $"NVARCHAR({len})" : "NVARCHAR(MAX)";
                case LinqToDB.DataType.VarChar: return len > 0 ? $"VARCHAR({len})" : "VARCHAR(MAX)";
                case LinqToDB.DataType.NChar: return len > 0 ? $"NCHAR({len})" : "NCHAR";
                case LinqToDB.DataType.Char: return len > 0 ? $"CHAR({len})" : "CHAR";
                case LinqToDB.DataType.VarBinary: return len > 0 ? $"VARBINARY({len})" : "VARBINARY(MAX)";
                case LinqToDB.DataType.Binary: return len > 0 ? $"BINARY({len})" : "BINARY";
                case LinqToDB.DataType.Date: return "DATE";
                case LinqToDB.DataType.Time: return "TIME";
                case LinqToDB.DataType.DateTime:
                case LinqToDB.DataType.DateTime2: return "DATETIME2";
                case LinqToDB.DataType.DateTimeOffset: return "DATETIMEOFFSET";
                case LinqToDB.DataType.Text: return "TEXT";
                case LinqToDB.DataType.NText: return "NTEXT";
                case LinqToDB.DataType.Xml: return "XML";
                case LinqToDB.DataType.Undefined:
                default:
                    var t = col.MemberType;
                    if (t == typeof(string)) return len > 0 ? $"NVARCHAR({len})" : "NVARCHAR(MAX)";
                    if (t == typeof(byte[])) return len > 0 ? $"VARBINARY({len})" : "VARBINARY(MAX)";
                    if (t == typeof(long)) return "BIGINT";
                    if (t == typeof(int)) return "INT";
                    if (t == typeof(short)) return "SMALLINT";
                    if (t == typeof(bool)) return "BIT";
                    if (t == typeof(System.Guid)) return "UNIQUEIDENTIFIER";
                    if (t == typeof(decimal)) return prec > 0 ? $"DECIMAL({prec},{(scale > 0 ? scale : 0)})" : "DECIMAL";
                    if (t == typeof(double)) return "FLOAT";
                    if (t == typeof(float)) return "REAL";
                    return "NVARCHAR(MAX)";
            }
        }

        public ColumnDefinition GetColumnInfo(string tableName, ColumnDescriptor col, bool nonclusteredPrimaryKey = false, string pkName = "")
        {
            var cd = new ColumnDefinition
            {
                Name = col.ColumnName,
                TableName = tableName,
                IsNullable = col.CanBeNull,
                IsPrimaryKey = col.IsPrimaryKey,
                PrimaryKeyName = string.IsNullOrEmpty(pkName) ? $"PK_{tableName}_{col.ColumnName}" : pkName,
                IsIdentity = col.IsIdentity,
                CustomType = BuildSqlType(col)
            };

            if (col.Precision > 0)
                cd.Precision = col.Precision;
            if (col.Scale > 0)
                cd.Size = col.Scale;

            if ((col.DataType == LinqToDB.DataType.Int64 || col.MemberType == typeof(long)) && _dataConnection.DataProvider is SQLiteDataProvider)
            {
                cd.CustomType = "INTEGER";
            }

            // If targeting SQL Server and caller requests nonclustered PKs, we don't embed it in type; clustering handled at index/constraint level.
            return cd;
        }

        public string IndexFor<T>(Index<T> indexDef, string indexName, bool unique = false, string tableName = "", bool isClustered = false)
        {
            var cols = indexDef.IndexColExprs.Select(r => IndexCol<T>.Create(r.expr, r.dir));
            return IndexForImpl(cols, indexName, unique, tableName, isClustered);
        }

        public string DropIndexFor<T>(Index<T> indexDef, string indexName, string tableName = "")
        {
            var ed = _dataConnection.MappingSchema.GetEntityDescriptor(typeof(T));
            var delete = new DeleteIndexExpression();
            var def = new IndexDefinition
            {
                TableName = string.IsNullOrWhiteSpace(tableName) ? ed.Name.Name : tableName,
                Name = indexName
            };
            delete.Index = def;
            return _generator.Generate(delete);
        }

        public string DropPrimaryKey<T>(string constraintName, string tableName = "")
        {
            var ed = _dataConnection.MappingSchema.GetEntityDescriptor(typeof(T));
            var delete = new DeleteConstraintExpression(ConstraintType.PrimaryKey);
            var def = new ConstraintDefinition(ConstraintType.PrimaryKey)
            {
                TableName = string.IsNullOrWhiteSpace(tableName) ? ed.Name.Name : tableName,
                ConstraintName = constraintName
            };
            delete.Constraint = def;
            return _generator.Generate(delete);
        }

        public string PrimaryKeyFor<T>(Index<T> indexDef, string constraintName, string tableName = "")
        {
            return PrimaryKeyForImpl(indexDef.IndexColExprs.Select(c => IndexCol<T>.Create(c.expr, c.dir)), constraintName, tableName);
        }

        public string PrimaryKeyForImpl<T>(IEnumerable<IndexCol<T>> indexDef, string constraintName, string tableName = "")
        {
            var ed = _dataConnection.MappingSchema.GetEntityDescriptor(typeof(T));

            var cd = new ConstraintDefinition(ConstraintType.PrimaryKey)
            {
                Columns = indexDef.Select(r => r.ColumnName).ToArray(),
                TableName = string.IsNullOrWhiteSpace(tableName) ? ed.Name.Name : tableName,
                ConstraintName = constraintName
            };
            var cce = new CreateConstraintExpression(ConstraintType.PrimaryKey) { Constraint = cd };
            return _generator.Generate(cce);
        }

        private string IndexForImpl<T>(IEnumerable<IndexCol<T>> indexDef, string indexName, bool unique, string tableName = "", bool isClustered = false)
        {
            var ed = _dataConnection.MappingSchema.GetEntityDescriptor(typeof(T));

            var colDefs = indexDef.Select(d =>
            {
                var icd = new IndexColumnDefinition();
                var mapped = ed.Columns.FirstOrDefault(c => c.MemberName == d.ColumnName);
                icd.Name = mapped?.ColumnName ?? d.ColumnName;
                if (d.Direction != DirectionEnum.unspecified)
                {
                    icd.Direction = d.Direction == DirectionEnum.asc ? Direction.Ascending : Direction.Descending;
                }
                return icd;
            }).ToList();

            var index = new CreateIndexExpression();
            var def = index.Index = new IndexDefinition
            {
                TableName = string.IsNullOrWhiteSpace(tableName) ? ed.Name.Name : tableName,
                Columns = colDefs,
                IsUnique = unique,
                Name = indexName,
                IsClustered = isClustered
            };
            return _generator.Generate(index);
        }
    }
}