﻿using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer3.Contrib.Store.AzureTableStorage.Serialization;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace IdentityServer3.Contrib.Store.AzureTableStorage
{
    public class AzureTableStorageClientStore: IClientStore
    {
        private readonly CloudTable _table;

        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> {new ClaimConverter()}
        };

        public AzureTableStorageClientStore(string connectionString, string tableName = "Clients")
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudTableClient();
            _table = client.GetTableReference(tableName);
            _table.CreateIfNotExists();
        }

        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            var op = TableOperation.Retrieve<ClientEntity>(clientId.GetParitionKey(), clientId);
            var result = await _table.ExecuteAsync(op);
            var client = result.Result as ClientEntity;
            return client != null ? JsonConvert.DeserializeObject<Client>(client.Json, _settings) : null;
        }
    }
}
