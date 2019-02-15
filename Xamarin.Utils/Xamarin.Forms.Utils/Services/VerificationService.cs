using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Utils.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly AuthMobileServiceClient _mobileServiceClient;
        private readonly string _verificationControllerName;

        public VerificationService(AuthMobileServiceClient mobileServiceClient, string verificationControllerName)
        {
            _mobileServiceClient = mobileServiceClient;
            _verificationControllerName = verificationControllerName;
        }

        public async Task Verify()
        {
            await _mobileServiceClient.InvokeApiAsync(_verificationControllerName);
        }
    }
}
