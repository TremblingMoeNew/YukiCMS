using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YukiCMS.Models;

namespace YukiCMS.Service
{
    public class YukiServiceBase
    {
        protected IMongoCollection<TDocument>getMongoCollection<TDocument>(string name,IYukiDatabaseSettings settings)
        {
            var mongoUrl = new MongoUrlBuilder(settings.dbConnectionUrl);
            mongoUrl.Username = settings.username;
            mongoUrl.Password = settings.password;
            var client = new MongoClient(mongoUrl.ToMongoUrl());
            var database = client.GetDatabase(settings.databaseName);
            return database.GetCollection<TDocument>(name);
        }
    }
}
