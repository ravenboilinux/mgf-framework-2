﻿using Servers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameCommon;
using MGF.Mappers;
using System.Security.Cryptography;
using MGF.Domain;

namespace Servers.AuthorizationServices
{
    public class UserSaltedPassAuthorizationService : IAuthorizationService
    {
        public ReturnCode IsAuthorized(out User user, params string[] authorizationParameters)
        {
            user = null;
            if(authorizationParameters.Length != 2)
            {
                return ReturnCode.OperationInvalid;
            }

            user = UserMapper.LoadByUserName(authorizationParameters[0]);
            if(null == user)
            {
                return ReturnCode.InvalidUserPass;
            }
            // valid user, check password
            // create a hash object with SHA2-512
            var sha512 = SHA512Managed.Create();
            // Get the salt from the user and add it to the password passed in.
            // calculate a hash and check it against the password hash in the database
            var hashedpw = sha512.ComputeHash(Encoding.UTF8.GetBytes(authorizationParameters[1]).Concat(Convert.FromBase64String(user.Salt ?? "")).ToArray());

            if(user.PasswordHash.Equals(Convert.ToBase64String(hashedpw), StringComparison.OrdinalIgnoreCase))
            {
                return ReturnCode.OK;
            }
            else
            {
                return ReturnCode.InvalidUserPass;
            }
        }
        public ReturnCode CreateAccount(params string[] authorizationParameters)
        {
            if (authorizationParameters.Length != 2)
            {
                return ReturnCode.OperationInvalid;
            }
            UserMapper userMapper = new UserMapper();
            User user = UserMapper.LoadByUserName(authorizationParameters[0]);
            if (null == user)
            {
                // Create the user
                var sha512 = SHA512Managed.Create();
                // calculate a hash and check it against the password hash in the database
                Guid salt = Guid.NewGuid();
                var hashedpw = sha512.ComputeHash(Encoding.UTF8.GetBytes(authorizationParameters[1]).Concat(salt.ToByteArray()).ToArray());
                user = new User() { LoginName = authorizationParameters[0], PasswordHash = Convert.ToBase64String(hashedpw), Salt = Convert.ToBase64String(salt.ToByteArray()) };
                userMapper.Save(user);
                return ReturnCode.OK;
            }
            else
            {
                return ReturnCode.InvalidUserPass;
            }
        }

    }
}
