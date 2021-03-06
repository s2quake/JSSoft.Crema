﻿// Released under the MIT License.
// 
// Copyright (c) 2018 Ntreev Soft co., Ltd.
// Copyright (c) 2020 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Forked from https://github.com/NtreevSoft/Crema
// Namespaces and files starting with "Ntreev" have been renamed to "JSSoft".

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Users.Serializations;
using JSSoft.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Users
{
    class UserContextSet
    {
        private readonly Dictionary<string, UserSerializationInfo> users = new();
        private readonly Dictionary<string, UserSerializationInfo> usersToCreate = new();
        private readonly Dictionary<string, UserSerializationInfo> usersToDelete = new();
        private readonly SignatureDateProvider signatureDateProvider;
        private readonly string basePath;
        private readonly UserRepositoryHost Repository;
        private readonly IObjectSerializer serializer;

        public UserContextSet(UserContext userContext, UserSet userSet, bool userCreation)
        {
            this.basePath = userContext.BasePath;
            this.Repository = userContext.Repository;
            this.serializer = userContext.Serializer;
            foreach (var item in userSet.Infos)
            {
                if (userCreation == true)
                {
                    this.usersToCreate.Add(item.Path, item);
                }
                else
                {
                    this.users.Add(item.Path, item);
                }
            }
            this.signatureDateProvider = userSet.SignatureDateProvider;
        }

        public static UserContextSet CreateEmpty(Authentication authentication, UserContext userContext, string[] itemPaths)
        {
            var userSet = new UserSet
            {
                ItemPaths = itemPaths,
                Infos = new UserSerializationInfo[] { },
                SignatureDateProvider = new SignatureDateProvider(authentication.ID),
            };
            return new UserContextSet(userContext, userSet, false);
        }

        public void SetUserCategoryPath(string categoryPath, string newCategoryPath)
        {
            var itemPath1 = new RepositoryPath(this.basePath, categoryPath);
            var itemPath2 = new RepositoryPath(this.basePath, newCategoryPath);

            if (itemPath1.IsExists == false)
                throw new DirectoryNotFoundException();
            if (itemPath2.IsExists == true)
                throw new IOException();

            var signatureDate = this.signatureDateProvider.Provide();
            foreach (var item in this.users.ToArray())
            {
                var path = item.Key;
                var userInfo = item.Value;
                if (userInfo.CategoryPath.StartsWith(categoryPath) == false)
                    continue;

                var userInfoCategoryPath = Regex.Replace(userInfo.CategoryPath, "^" + categoryPath, newCategoryPath);
                var repositoryPath1 = new RepositoryPath(this.basePath, path);
                var repositoryPath2 = new RepositoryPath(this.basePath, userInfoCategoryPath + userInfo.ID);
                repositoryPath1.ValidateExists(this.serializer, typeof(UserSerializationInfo));
                repositoryPath2.ValidateNotExists(this.serializer, typeof(UserSerializationInfo));
                userInfo.CategoryPath = userInfoCategoryPath;
                userInfo.ModificationInfo = signatureDate;
                this.users[path] = userInfo;
            }

            this.Serialize();
            this.Repository.Move(itemPath1, itemPath2);
        }

        public void DeleteUserCategory(string categoryPath)
        {
            var itemPath = new RepositoryPath(this.basePath, categoryPath);
            this.Repository.Delete(itemPath);
        }

        public void CreateUser()
        {
            foreach (var item in this.usersToCreate.ToArray())
            {
                var path = item.Key;
                var userInfo = item.Value;
                userInfo.CreationInfo = userInfo.ModificationInfo = this.signatureDateProvider.Provide();
                this.usersToCreate[path] = userInfo;
            }
            this.Serialize();
            foreach (var item in this.usersToCreate)
            {
                var repositoryPath = new RepositoryPath(this.basePath, item.Key);
                this.Repository.Add(repositoryPath);
            }
        }

        public void RenameUser(string typePath, string typeName)
        {
            throw new NotImplementedException();
        }

        public void MoveUser(string userPath, string categoryPath)
        {
            var userInfo = this.users[userPath];
            var repositoryPath1 = new RepositoryPath(this.basePath, userInfo.Path);
            var repositoryPath2 = new RepositoryPath(this.basePath, categoryPath + userInfo.ID);
            repositoryPath1.ValidateExists(this.serializer, typeof(UserSerializationInfo));
            repositoryPath2.ValidateNotExists(this.serializer, typeof(UserSerializationInfo));
            userInfo.CategoryPath = categoryPath;
            userInfo.ModificationInfo = this.signatureDateProvider.Provide();
            this.users[userPath] = userInfo;
            this.Serialize();
            this.Repository.Move(repositoryPath1, repositoryPath2);
        }

        public void DeleteUser(string userPath)
        {
            var userInfo = this.users[userPath];
            this.users.Remove(userPath);
            this.usersToDelete.Add(userPath, userInfo);
            var repositoryPaths = this.usersToDelete.Keys.Select(item => new RepositoryPath(this.basePath, item)).ToArray();
            this.Repository.DeleteRange(repositoryPaths);
        }

        [Obsolete]
        public void ModifyUser(string userPath, SecureString password, SecureString newPassword, string userName, Authority? authority)
        {
            var userInfo = this.users[userPath];
            if (newPassword != null)
                userInfo.Password = UserContext.SecureStringToString(newPassword).Encrypt();
            if (userName != null)
                userInfo.Name = userName;
            if (authority.HasValue)
                userInfo.Authority = authority.Value;
            userInfo.ModificationInfo = this.signatureDateProvider.Provide();
            this.users[userPath] = userInfo;
            this.Serialize();
        }

        public void ModifyUser(string userPath, SecureString password)
        {
            var userInfo = this.users[userPath];
            userInfo.Password = UserContext.SecureStringToString(password).Encrypt();
            userInfo.ModificationInfo = this.signatureDateProvider.Provide();
            this.users[userPath] = userInfo;
            this.Serialize();
        }

        public void ModifyUser(string userPath, string userName)
        {
            var userInfo = this.users[userPath];
            userInfo.Name = userName;
            userInfo.ModificationInfo = this.signatureDateProvider.Provide();
            this.users[userPath] = userInfo;
            this.Serialize();
        }

        public void ModifyUser(string userPath, Authority authority)
        {
            var userInfo = this.users[userPath];
            userInfo.Authority = authority;
            userInfo.ModificationInfo = this.signatureDateProvider.Provide();
            this.users[userPath] = userInfo;
            this.Serialize();
        }

        public void BanUser(string userPath, string comment)
        {
            var userInfo = this.users[userPath];
            var banInfo = new BanSerializationInfo()
            {
                Path = userInfo.Path,
                Comment = comment,
                SignatureDate = this.signatureDateProvider.Provide(),
            };
            userInfo.BanInfo = banInfo;
            this.users[userPath] = userInfo;
            this.Serialize();
        }

        public void UnbanUser(string userPath)
        {
            var userInfo = this.users[userPath];
            userInfo.BanInfo = BanSerializationInfo.Empty;
            this.users[userPath] = userInfo;
            this.Serialize();
        }

        public UserSerializationInfo GetUserInfo(string path)
        {
            if (this.users.ContainsKey(path) == true)
                return this.users[path];
            if (this.usersToCreate.ContainsKey(path) == true)
                return this.usersToCreate[path];
            throw new NotImplementedException();
        }

        private void Serialize()
        {
            foreach (var item in this.users)
            {
                var path = item.Key;
                var userInfo = item.Value;
                var itemPath1 = new RepositoryPath(this.basePath, path);
                var itemPath2 = new RepositoryPath(this.basePath, userInfo.Path);

                itemPath1.ValidateExists(this.serializer, typeof(UserSerializationInfo));
                if (itemPath1 != itemPath2)
                {
                    itemPath2.ValidateNotExists(this.serializer, typeof(UserSerializationInfo));
                }
                this.Repository.Write(path, userInfo, false);
            }

            foreach (var item in this.usersToCreate)
            {
                var path = item.Key;
                var userInfo = item.Value;
                this.Repository.Write(path, userInfo, true);
            }
        }

    }
}
