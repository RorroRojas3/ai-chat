using Microsoft.Azure.Cosmos;

namespace RR.AI_Chat.Service
{
    /// <summary>
    /// Defines operations for interacting with Azure Cosmos DB.
    /// </summary>
    public interface IChatCosmosService
    {
        /// <summary>
        /// Retrieves a single item from the Cosmos DB container.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="id">The unique identifier of the item.</param>
        /// <param name="partitionKey">The partition key value of the item.</param>
        /// <returns>The item if found; otherwise, null.</returns>
        Task<T?> GetItemAsync<T>(string id, string partitionKey);

        /// <summary>
        /// Retrieves multiple items from the Cosmos DB container based on a query.
        /// </summary>
        /// <typeparam name="T">The type of the items to retrieve.</typeparam>
        /// <param name="query">The SQL query string to execute.</param>
        /// <returns>A collection of items matching the query.</returns>
        Task<IEnumerable<T>> GetItemsAsync<T>(string query);

        /// <summary>
        /// Creates a new item in the Cosmos DB container.
        /// </summary>
        /// <typeparam name="T">The type of the item to create.</typeparam>
        /// <param name="item">The item to create.</param>
        /// <param name="partitionKey">The partition key value for the item.</param>
        /// <returns>The created item.</returns>
        Task<T> CreateItemAsync<T>(T item, string partitionKey);

        /// <summary>
        /// Updates an existing item in the Cosmos DB container.
        /// </summary>
        /// <typeparam name="T">The type of the item to update.</typeparam>
        /// <param name="item">The updated item.</param>
        /// <param name="id">The unique identifier of the item to update.</param>
        /// <param name="partitionKey">The partition key value of the item.</param>
        /// <returns>The updated item.</returns>
        Task<T> UpdateItemAsync<T>(T item, string id, string partitionKey);

        /// <summary>
        /// Deletes an item from the Cosmos DB container.
        /// </summary>
        /// <param name="id">The unique identifier of the item to delete.</param>
        /// <param name="partitionKey">The partition key value of the item.</param>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        Task DeleteItemAsync(string id, string partitionKey);
    }

    /// <summary>
    /// Provides implementation for Azure Cosmos DB operations.
    /// </summary>
    public class ChatCosmosService : IChatCosmosService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatCosmosService"/> class.
        /// </summary>
        /// <param name="cosmosClient">The Cosmos DB client.</param>
        /// <param name="databaseId">The database identifier.</param>
        /// <param name="containerId">The container identifier.</param>
        public ChatCosmosService(CosmosClient cosmosClient, string databaseId, string containerId)
        {
            _cosmosClient = cosmosClient;
            var database = _cosmosClient.GetDatabase(databaseId);
            _container = database.GetContainer(containerId);
        }

        /// <inheritdoc/>
        public async Task<T?> GetItemAsync<T>(string id, string partitionKey)
        {
            try
            {
                var response = await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
                return response;
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return default;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetItemsAsync<T>(string query)
        {
            var items = new List<T>();
            var iterator = _container.GetItemQueryIterator<T>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                items.AddRange(response);
            }

            return items;
        }

        /// <inheritdoc/>
        public async Task<T> CreateItemAsync<T>(T item, string partitionKey)
        {
            var response = await _container.CreateItemAsync(item, new PartitionKey(partitionKey));
            return response;
        }

        /// <inheritdoc/>
        public async Task<T> UpdateItemAsync<T>(T item, string id, string partitionKey)
        {
            var response = await _container.ReplaceItemAsync(item, id, new PartitionKey(partitionKey));
            return response;
        }

        /// <inheritdoc/>
        public async Task DeleteItemAsync(string id, string partitionKey)
        {
            await _container.DeleteItemAsync<dynamic>(id, new PartitionKey(partitionKey));
        }
    }
}
