﻿using Microsoft.Azure.Mobile.Server;

namespace Azure.Server.Utils.CustomAuthentication
{
    public abstract class AccountBase : EntityData
    {
        public string Sid { get; set; }

        public byte[] Salt { get; set; }

        public byte[] Hash { get; set; }

        public Provider Provider { get; set; }

        public bool Verified { get; set; }

        public byte[] ConfirmationHash { get; set; }

        public string RefreshToken { get; set; }
    }
}
