#nullable enable
using System;
using Windows.Security.Credentials;

namespace MultiAI.Services
{
    public class SecureStorageService
    {
        private const string VaultResource = "MultiAI_API_Keys";

        public void SaveKey(string provider, string key)
        {
            var vault = new PasswordVault();
            vault.Add(new PasswordCredential(VaultResource, provider, key));
        }

        public string? GetKey(string provider)
        {
            var vault = new PasswordVault();
            try
            {
                var credentials = vault.FindAllByResource(VaultResource);
                foreach (var cred in credentials)
                {
                    if (cred.UserName == provider)
                    {
                        cred.RetrievePassword();
                        return cred.Password;
                    }
                }
            }
            catch (Exception)
            {
                // Key not found
            }
            return null;
        }
    }
}
