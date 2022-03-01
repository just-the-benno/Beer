using Beer.DaAPI.Core.Tracing;
using Beer.DaAPI.Infrastructure.StorageEngine;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.Infrastructure.StorageEngine
{
    public class DapperBasedTracingStore : ITracingStore
    {
        private readonly string _connenctionString;
        private readonly ILogger<DapperBasedTracingStore> logger;

        public DapperBasedTracingStore(string connenctionString, ILogger<DapperBasedTracingStore> logger)
        {
            this._connenctionString = connenctionString;
            this.logger = logger;
        }

        public async Task<bool> AddTracingRecord(TracingRecord record)
        {
            try
            {
                using (IDbConnection connection = new NpgsqlConnection(_connenctionString))
                {
                    String recordSql = @"INSERT INTO public.""TracingStreamEntries"" 
                        (""Id"",""Identifier"",""Timestamp"",""EntityId"",""AddtionalData"",""StreamId"",""ResultType"") 
                            VALUES 
                        (:id,:identifier,:timestamp,:entityId,:addtionalData::json,:streamId,:resultType)";

                    var rowsAffected = await connection.ExecuteAsync(recordSql,
                        new
                        {
                            id = Guid.NewGuid(),
                            identifier = record.Identifier,
                            timestamp = record.Timestamp,
                            entityId = record.EntityId,
                            addtionalData = JsonSerializer.Serialize(new Dictionary<string, string>(record.Data ?? new Dictionary<string, string>())),
                            streamId = record.StreamId,
                            resultType = (Int32)record.Status,
                        });

                    String updateCountSql = $@"UPDATE public.""TracingStreams"" 
                            SET ""RecordCount"" = ""RecordCount"" + 1
                            WHERE ""Id"" = :id";

                    await connection.ExecuteAsync(updateCountSql,
                    new
                    {
                        id = record.StreamId,
                    });

                    Int32? resultStatus = null;

                    if (record.Status == TracingRecordStatus.Error)
                    {

                    }
                    else if (record.Status == TracingRecordStatus.Success)
                    {
                        resultStatus = (Int32)TracingRecordStatus.Success;
                    }

                    if (resultStatus.HasValue == true)
                    {
                        String statusUpdateCountSql = $@"UPDATE public.""TracingStreams"" 
                            SET ""ResultType"" = :result
                            WHERE ""Id"" = :id";

                        await connection.ExecuteAsync(statusUpdateCountSql,
                            new
                            {
                                id = record.StreamId,
                                result = resultStatus.Value,
                            });
                    }

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AddTracingRecord Failed");
                return false;
            }
        }

        public async Task<bool> AddTracingStream(TracingStream stream)
        {
            try
            {
                using (IDbConnection connection = new NpgsqlConnection(_connenctionString))
                {
                    String sql = @"INSERT INTO public.""TracingStreams"" 
                        (""Id"",""CreatedAt"",""SystemIdentifier"",""ProcedureIdentifier"",""FirstEntryData"",""RecordCount"") 
                            VALUES 
                        (:id,:timestamp,:systemIdentifier,:procedureIdentifier,:firstEntryData::json,:recordCount)";

                    var rowsAffected = await connection.ExecuteAsync(sql,
                        new
                        {
                            id = stream.Id,
                            timestamp = stream.CreatedAt,
                            systemIdentifier = stream.SystemIdentifier,
                            procedureIdentifier = stream.ProcedureIdentifier,
                            firstEntryData = JsonSerializer.Serialize(new Dictionary<string, string>(stream.Record.FirstOrDefault()?.Data ?? new Dictionary<string, string>())),
                            recordCount = stream.Record.Count(),
                        });

                    return rowsAffected > 0;

                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AddTracingStream Failed");
                return false;
            }
        }

        public async Task<bool> CloseTracingStream(Guid streamId)
        {
            try
            {
                using (IDbConnection connection = new NpgsqlConnection(_connenctionString))
                {
                    String closeSql = $@"UPDATE public.""TracingStreams"" 
                            SET ""ClosedAt"" = :timestamp
                            WHERE ""Id"" = :id";

                    var rowsAffected = await connection.ExecuteAsync(closeSql,
                        new
                        {
                            id = streamId,
                            timestamp = DateTime.UtcNow,
                        });

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CloseTracingStream Failed");
                return false;
            }

        }
    }
}
