using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace ManagedCode.Repository.CosmosDB
{
    public class CosmosDbAdapter<T> where T : class, new()
    {
        private readonly bool _allowTableCreation = true;
        private readonly string _collectionName;
        private readonly CosmosClient _cosmosClient;
        private readonly string _databaseName;
        private readonly object _sync = new();
        private bool _tableClientInitialized;

        public CosmosDbAdapter(string connectionString, string databaseName = null, string collectionName = null)
        {
            _databaseName = databaseName ?? "database";
            _collectionName = collectionName ?? "containerTest";
            _cosmosClient = new CosmosClient(connectionString);
        }

        public CosmosDbAdapter(string connectionString, CosmosClientOptions cosmosClientOptions, string databaseName = null, string collectionName = null)
        {
            _databaseName = databaseName ?? "database";
            _collectionName = collectionName ?? "containerTest";
            _cosmosClient = new CosmosClient(connectionString, cosmosClientOptions);
        }

        public async Task<Container> GetContainer()
        {
            if (!_tableClientInitialized)
            {
                if (_allowTableCreation)
                {
                    var response = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseName);
                    var database = response.Database;
                    Container container = await database.CreateContainerIfNotExistsAsync(_collectionName, "/id").ConfigureAwait(false);
                }
                else
                {
                    var database = _cosmosClient.GetDatabase(_databaseName);
                    if (database == null)
                    {
                        throw new Exception($"Database '{_databaseName}' does not exist.");
                    }

                    var container = database.GetContainer(_collectionName);
                    if (container == null)
                    {
                        throw new Exception($"Container '{_collectionName}' does not exist.");
                    }
                }

                _tableClientInitialized = true;
            }

            return _cosmosClient.GetContainer(_databaseName, _collectionName);
        }
    }
}