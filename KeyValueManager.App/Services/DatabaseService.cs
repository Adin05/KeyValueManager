using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using KeyValueManager.App.Models;

namespace KeyValueManager.App.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly EncryptionService _encryptionService;

        public DatabaseService(string dbPath, EncryptionService encryptionService)
        {
            _connectionString = $"Data Source={dbPath}";
            _encryptionService = encryptionService;
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS KeyValueEntries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Key TEXT NOT NULL UNIQUE,
                    Value1 TEXT,
                    Value2 TEXT,
                    Value3 TEXT,
                    Description TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT
                );
                CREATE TABLE IF NOT EXISTS AppSettings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT NOT NULL
                );";
            command.ExecuteNonQuery();
        }

        public async Task<List<KeyValueEntry>> GetAllEntriesAsync()
        {
            var entries = new List<KeyValueEntry>();
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM KeyValueEntries ORDER BY Key";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                entries.Add(new KeyValueEntry
                {
                    Id = reader.GetInt32(0),
                    Key = reader.GetString(1),
                    Value1 = _encryptionService.Decrypt(reader.GetString(2)),
                    Value2 = _encryptionService.Decrypt(reader.GetString(3)),
                    Value3 = _encryptionService.Decrypt(reader.GetString(4)),
                    Description = reader.GetString(5),
                    CreatedAt = DateTime.Parse(reader.GetString(6)),
                    UpdatedAt = reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7))
                });
            }

            return entries;
        }

        public async Task<KeyValueEntry> GetEntryByKeyAsync(string key)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM KeyValueEntries WHERE Key = @Key";
            command.Parameters.AddWithValue("@Key", key);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new KeyValueEntry
                {
                    Id = reader.GetInt32(0),
                    Key = reader.GetString(1),
                    Value1 = _encryptionService.Decrypt(reader.GetString(2)),
                    Value2 = _encryptionService.Decrypt(reader.GetString(3)),
                    Value3 = _encryptionService.Decrypt(reader.GetString(4)),
                    Description = reader.GetString(5),
                    CreatedAt = DateTime.Parse(reader.GetString(6)),
                    UpdatedAt = reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7))
                };
            }

            return null;
        }

        public async Task AddEntryAsync(KeyValueEntry entry)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO KeyValueEntries (Key, Value1, Value2, Value3, Description, CreatedAt)
                VALUES (@Key, @Value1, @Value2, @Value3, @Description, @CreatedAt)";

            command.Parameters.AddWithValue("@Key", entry.Key);
            command.Parameters.AddWithValue("@Value1", _encryptionService.Encrypt(entry.Value1));
            command.Parameters.AddWithValue("@Value2", _encryptionService.Encrypt(entry.Value2));
            command.Parameters.AddWithValue("@Value3", _encryptionService.Encrypt(entry.Value3));
            command.Parameters.AddWithValue("@Description", entry.Description);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("o"));

            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateEntryAsync(KeyValueEntry entry)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE KeyValueEntries 
                SET Value1 = @Value1, Value2 = @Value2, Value3 = @Value3, 
                    Description = @Description, UpdatedAt = @UpdatedAt
                WHERE Key = @Key";

            command.Parameters.AddWithValue("@Key", entry.Key);
            command.Parameters.AddWithValue("@Value1", _encryptionService.Encrypt(entry.Value1));
            command.Parameters.AddWithValue("@Value2", _encryptionService.Encrypt(entry.Value2));
            command.Parameters.AddWithValue("@Value3", _encryptionService.Encrypt(entry.Value3));
            command.Parameters.AddWithValue("@Description", entry.Description);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow.ToString("o"));

            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteEntryAsync(string key)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM KeyValueEntries WHERE Key = @Key";
            command.Parameters.AddWithValue("@Key", key);

            await command.ExecuteNonQueryAsync();
        }

        public async Task SaveSettingAsync(string key, string value)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO AppSettings (Key, Value)
                VALUES (@Key, @Value)";

            command.Parameters.AddWithValue("@Key", key);
            command.Parameters.AddWithValue("@Value", value);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<string> GetSettingAsync(string key)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Value FROM AppSettings WHERE Key = @Key";
            command.Parameters.AddWithValue("@Key", key);

            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }
    }
} 