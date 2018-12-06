﻿using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;

namespace Xamarin.Forms.Utils.Services
{
    public interface IAuthenticationService
    {
        Task<bool> Authenticate();

        Task Logout();
    }
}
