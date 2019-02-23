﻿using Dapper;
using DevChatter.DevStreams.Core.Data;
using DevChatter.DevStreams.Core.Model;
using DevChatter.DevStreams.Core.Settings;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DevChatter.DevStreams.Infra.Dapper
{
    public class DapperCrudRepository : ICrudRepository
    {
        private readonly DatabaseSettings _dbSettings;

        public DapperCrudRepository(IOptions<DatabaseSettings> databaseSettings)
        {
            _dbSettings = databaseSettings.Value;
        }

        public async Task<int?> Insert<T>(T model) where T : DataEntity
        {
            using (IDbConnection connection = new SqlConnection(_dbSettings.DefaultConnection))
            {
                return await connection.InsertAsync(model);
            }
        }

        public async Task<T> Get<T>(int id) where T : DataEntity
        {
            string tableName = GetTableName<T>();
            string sql = $"SELECT * FROM {tableName} WHERE Id = @id";
            using (IDbConnection connection = new SqlConnection(_dbSettings.DefaultConnection))
            {
                return await connection.QuerySingleAsync<T>(sql, new {id});
            }
        }

        public async Task<List<T>> GetAll<T>() where T : DataEntity
        {
            string tableName = GetTableName<T>();
            string sql = $"SELECT * FROM {tableName}";

            using (IDbConnection connection = new SqlConnection(_dbSettings.DefaultConnection))
            {
                return (await connection.QueryAsync<T>(sql)).ToList();
            }
        }

        public async Task<int> Update<T>(T model) where T : DataEntity
        {
            using (IDbConnection connection = new SqlConnection(_dbSettings.DefaultConnection))
            {
                return await connection.UpdateAsync(model);
            }
        }

        public async Task<int> Delete<T>(int id) where T : DataEntity
        {
            using (IDbConnection connection = new SqlConnection(_dbSettings.DefaultConnection))
            {
                return await connection.DeleteAsync(id);
            }
        }

        public async Task<int> Delete<T>(T model) where T : DataEntity
        {
            using (IDbConnection connection = new SqlConnection(_dbSettings.DefaultConnection))
            {
                return await connection.DeleteAsync(model);
            }
        }

        public async Task<bool> Exists<T>(int id) where T : DataEntity
        {
            using (IDbConnection connection = new SqlConnection(_dbSettings.DefaultConnection))
            {
                return await connection.RecordCountAsync<T>("WHERE Id=@id", new {id}) > 0;
            }
        }

        private static string GetTableName<T>() where T : DataEntity
        {
            var tableAttrib = typeof(T).GetCustomAttributes(true)
                .SingleOrDefault(attr => attr.GetType().Name == typeof(TableAttribute).Name) as dynamic;
            string tableName = tableAttrib?.Name ?? typeof(T).Name + "s";
            return tableName;
        }
    }
}