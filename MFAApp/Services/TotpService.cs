
using OtpNet;
using System.Collections.Concurrent;

namespace MFAApp.Services
{
   
        public class TotpService
        {
            private static readonly ConcurrentDictionary<string, byte[]> UserSecrets = new();

            public string GenerateSetupCode(string username)
            {
                if (username== null || !UserSecrets.ContainsKey(username))
                {

                //Temp code
                username = "test-user";
                var key = KeyGeneration.GenerateRandomKey(20);
                    UserSecrets[username] = key;
                }

                var totp = new Totp(UserSecrets[username]);
                var base32Secret = Base32Encoding.ToString(UserSecrets[username]);

                var uri = new OtpUri(OtpType.Totp, base32Secret, username, "MyKerberosApp").ToString();
                return uri;
            }

            public bool VerifyCode(string username, string code)
            {
                if (!UserSecrets.TryGetValue(username, out var key)) return false;

                var totp = new Totp(key);
                return totp.VerifyTotp(code, out _);
            }
        }
   

}