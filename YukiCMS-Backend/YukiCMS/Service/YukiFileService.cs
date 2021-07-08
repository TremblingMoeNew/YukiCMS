using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using YukiCMS.Models;

namespace YukiCMS.Service
{
    public class YukiFileService : YukiServiceBase
    {
        private readonly IMongoCollection<YukiFile> _file;
        private readonly YukiGlobalService _globalService;
        public YukiFileService(IYukiDatabaseSettings settings,YukiGlobalService globalService)
        {
            _file = getMongoCollection<YukiFile>(settings.fileColleName, settings);
            _globalService = globalService;
        }
        public void create(YukiFile file)
        {
            _file.InsertOne(file);
        }
        public YukiFile get(Expression<Func<YukiFile, bool>> filter) =>
            _file.Find(filter).FirstOrDefault();
        public bool update<TResult>(Expression<Func<YukiFile, bool>> filter,
                    Expression<Func<YukiFile, TResult>> updatedProperty,
                    TResult value
            ) =>
            _file.UpdateOne(
                filter,
                Builders<YukiFile>.Update.Set(updatedProperty, value)
            ).ModifiedCount > 0;

        public void delete(Expression<Func<YukiFile, bool>> filter) =>
            _file.DeleteOne(filter);

        public string create(IFormFile file)
        {
            BinaryReader reader = new BinaryReader(file.OpenReadStream());
            byte[] blob = reader.ReadBytes((int)reader.BaseStream.Length);
            string fileContentType = file.ContentType;
            string fileOriginalName = Path.GetFileName(file.FileName);
            string fileStorageName;
            using (SHA1 sha = SHA1.Create())
            {
                byte[] hmb = sha.ComputeHash(blob);
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < hmb.Length; i++)
                {
                    sBuilder.Append(hmb[i].ToString("x2"));
                }
                fileStorageName = sBuilder.ToString();
            }
            if (getFile(fileStorageName) != null) return fileStorageName;
            YukiFile f = new YukiFile
            {
                blob = blob,
                fileOriginalName = fileOriginalName,
                fileStorageName = fileStorageName,
                fileClaimedName = fileOriginalName,
                fileContentType = fileContentType,
                fileQuoteCount = 0
            };
            create(f);
            return fileStorageName;
        }
        public bool increaseQuote(string storageName)
        {
            var f = getFile(storageName);
            if (f == null) return false;
            return update(file => file.fileStorageName == storageName, fi => fi.fileQuoteCount, f.fileQuoteCount + 1);
        }
        public bool decreaseQuote(string storageName)
        {
            var f = getFile(storageName);
            if (f == null) return false;
            if (f.fileQuoteCount == 0) return false;
            return update(file => file.fileStorageName == storageName, fi => fi.fileQuoteCount, f.fileQuoteCount - 1);
        }
        public YukiFile getFile(string storageName) =>
            get(file => file.fileStorageName == storageName);
        public void deleteFile(string storageName) =>
            delete(file => file.fileStorageName == storageName);
    }
}
