﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common.DbDtos;

namespace GlutenFree.OddJob.Storage.Sql.Common
{
    public interface IJobSearchProvider
    {
        bool UpdateJobMetadataValues(
            IDictionary<Expression<Func<SqlCommonDbOddJobMetaData, object>>, object> setters, Guid jobGuid,
            string oldStatusIfRequired);

        bool UpdateJobMetadataAndParameters(JobUpdateCommand updateCommand);

        bool UpdateJobMetadataFull(SqlCommonDbOddJobMetaData metaDataToUpdate, string oldStatusIfRequired);
        bool UpdateJobParameterValues(IEnumerable<SqlCommonOddJobParamMetaData> metaDatas);
        IEnumerable<T> GetJobParamCriteriaValues<T>(Expression<Func<SqlCommonOddJobParamMetaData, T>> selector);
        IEnumerable<T> GetJobCriteriaValues<T>(Expression<Func<SqlCommonDbOddJobMetaData, T>> selector);

        IEnumerable<T> GetJobCriteriaByCriteria<T>(Expression<Func<SqlCommonDbOddJobMetaData, bool>> criteria,
            Expression<Func<SqlCommonDbOddJobMetaData, T>> selector);
        IEnumerable<IOddJobWithMetadata> GetJobsByParameterAndMainCriteria(
            Expression<Func<SqlCommonDbOddJobMetaData, bool>> jobQueryable,
            Expression<Func<SqlCommonOddJobParamMetaData, bool>> paramQueryable);
        IEnumerable<SerializableOddJob> GetSerializableJobsByCriteria(
            Expression<Func<SqlCommonDbOddJobMetaData, bool>> criteria);
        IEnumerable<IOddJobWithMetadata> GetJobsByCriteria(
            Expression<Func<SqlCommonDbOddJobMetaData, bool>> criteria);
    }
}
