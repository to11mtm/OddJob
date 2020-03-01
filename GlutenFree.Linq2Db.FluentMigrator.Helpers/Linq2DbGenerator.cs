using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Generic;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.SQLite;
using LinqToDB.DataProvider.SqlServer;
using LinqToDB.SqlQuery;

namespace GlutenFree.Linq2Db.FluentMigrator.Helpers
{
    public class Linq2DbGenerator
    {

        private GenericGenerator _generator;
        private DataConnection _dataConnection;
        public Linq2DbGenerator(GenericGenerator generator, DataConnection dataConnection)
        {
            _generator = generator;
            _dataConnection = dataConnection;
        }

        
        public string CreateTableFor<T>(string tableName = "", bool nonClusteredPrimaryKey = false)
        {
            var create = new CreateTableExpression();
            var t = new SqlTable(_dataConnection.MappingSchema, typeof(T));

            create.TableName = string.IsNullOrWhiteSpace(tableName) ? t.PhysicalName : tableName;
            create.SchemaName = t.Schema;
            create.Columns = t.Fields.Select(sqlf => GetColumnInfo(create.TableName, sqlf.Value, nonClusteredPrimaryKey)).ToList();
            return _generator.Generate(create);
        }
        public string GetL2DbCol(SqlField field)
        {
            var sb = _dataConnection.DataProvider.CreateSqlBuilder(_dataConnection.DataProvider.MappingSchema);
            var type = sb.GetType();
            var bdt = type.GetMethod("BuildCreateTableFieldType", BindingFlags.Instance | BindingFlags.NonPublic);
            var sbfld = type.GetField("StringBuilder", BindingFlags.NonPublic | BindingFlags.Instance);
            sbfld.SetValue(sb, new StringBuilder());
            var innerSb = sbfld.GetValue(sb) as StringBuilder;

            innerSb.Clear();
            bdt.Invoke(sb, new object[] { field });

            return innerSb.ToString();
        }
        public ColumnDefinition GetColumnInfo(string tableName, SqlField field, bool nonclusteredPrimaryKey = false, string pkName = "")
        {
            var cd = new ColumnDefinition();
            cd.Name = field.PhysicalName;
            cd.TableName = tableName;
            cd.IsNullable = field.CanBeNull;
            cd.IsPrimaryKey = field.IsPrimaryKey;
            cd.PrimaryKeyName = string.IsNullOrEmpty(pkName)
                ? "PK_" + tableName + "_" + field.PhysicalName : pkName;
            cd.IsIdentity = field.IsIdentity;
            cd.CustomType = GetL2DbCol(field);
            cd.Precision = field.Precision;
            cd.Size = field.Scale;
            
            if (field.DataType == DataType.Int64 && _dataConnection.DataProvider is SQLiteDataProvider)
            {
                cd.CustomType = "INTEGER"; //in SQLite this is still 64 bit, and SQLite won't like BIGINT PRIMARY KEY AUTOINCREMENT.
            }

            if (field.IsIdentity &&
                _dataConnection.DataProvider is SqlServerDataProvider && nonclusteredPrimaryKey)
            {
                cd.CustomType = cd.CustomType + " NONCLUSTERED";
            }


            return cd;
        }
        public string IndexFor<T>(Index<T> indexDef, string indexName, bool unique = false, string tableName = "", bool isClustered = false)
        {
            var cols = indexDef.IndexColExprs.Select(r => IndexCol<T>.Create(r.expr, r.dir));
            return IndexForImpl(cols, indexName, unique, tableName, isClustered);
        }

        public string DropIndexFor<T>(Index<T> indexDef, string indexName, string tableName = "")
        {
            var mappingColDef = new SqlTable<T>(_dataConnection.MappingSchema);
            var delete = new DeleteIndexExpression();
            var def = new IndexDefinition();
            def.TableName = string.IsNullOrWhiteSpace(tableName) ? mappingColDef.PhysicalName : tableName;
            def.Name = indexName;
            delete.Index = def;
            return _generator.Generate(delete);
        }
        public string DropPrimaryKey<T>(string constraintName, string tableName = "")
        {
            var mappingColDef = new SqlTable<T>(_dataConnection.MappingSchema);
            var delete = new DeleteConstraintExpression(ConstraintType.PrimaryKey);
            var def = new ConstraintDefinition(ConstraintType.PrimaryKey);
            def.TableName = string.IsNullOrWhiteSpace(tableName) ? mappingColDef.PhysicalName : tableName;
            def.ConstraintName = constraintName;
            delete.Constraint = def;
            return _generator.Generate(delete);
        }
        public string PrimaryKeyFor<T>(Index<T> indexDef, string constraintName,
            string tableName = "")
        {
            return PrimaryKeyForImpl<T>(indexDef.IndexColExprs.Select(c=>IndexCol<T>.Create(c.expr,c.dir)), constraintName,
                tableName);
        }

        public string PrimaryKeyForImpl<T>(IEnumerable<IndexCol<T>> indexDef, string constraintName, string tableName = "")
        {
            var mappingColDef = new SqlTable<T>(_dataConnection.MappingSchema);
            
            var cd = new ConstraintDefinition(ConstraintType.PrimaryKey);
            cd.Columns = indexDef.Select(r => r.ColumnName).ToArray();
            cd.TableName = string.IsNullOrWhiteSpace(tableName) ? mappingColDef.PhysicalName : tableName;
            cd.ConstraintName = constraintName;
            var cce = new CreateConstraintExpression(ConstraintType.PrimaryKey);
            cce.Constraint = cd;
            return _generator.Generate(cce);
        }
        private string IndexForImpl<T>(IEnumerable<IndexCol<T>> indexDef, string indexName, bool unique, string tableName = "", bool isClustered = false) 
        {
            var mappingColDef = new SqlTable<T>(_dataConnection.MappingSchema);

            var colDefs = indexDef.Select(d =>
                {
                    var icd = new IndexColumnDefinition();
                
                    icd.Name = mappingColDef.Fields[d.ColumnName]?.PhysicalName ?? d.ColumnName;
                    if (d.Direction != DirectionEnum.unspecified)
                    {
                        icd.Direction = icd.Direction == Direction.Ascending ? Direction.Ascending : Direction.Descending;
                    }
                    return icd;
                }
            ).ToList();
            
            var index = new CreateIndexExpression();
            
            var def = index.Index = new IndexDefinition();

            def.TableName = string.IsNullOrWhiteSpace(tableName) ? mappingColDef.PhysicalName : tableName;
            def.Columns = colDefs;
            def.IsUnique = unique;
            def.Name = indexName;
            def.IsClustered = isClustered;
            return _generator.Generate(index);
        }
    }
}