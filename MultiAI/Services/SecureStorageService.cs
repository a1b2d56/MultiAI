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
            if (string.IsNullOrWhiteSpace(key))
            {
                RemoveKey(provider);
                return;
            }

            var vault = new PasswordVault();
            // Remove existing key if present to avoid duplication in credential vault
            RemoveKey(provider);
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
                    if (string.Equals(cred.UserName, provider, StringComparison.OrdinalIgnoreCase))
                    {
                        cred.RetrievePassword();
                        return cred.Password;
                    }
                }
            }
            catch
            {
                // Key not found in Credential Locker
            }
            return null;
        }

        public bool HasKey(string provider)
        {
            return !string.IsNullOrEmpty(GetKey(provider));
        }

        public void RemoveKey(string provider)
        {
            var vault = new PasswordVault();
            try
            {
                var credentials = vault.FindAllByResource(VaultResource);
                foreach (var cred in credentials)
                {
                    if (string.Equals(cred.UserName, provider, StringComparison.OrdinalIgnoreCase))
                    {
                        vault.Remove(cred);
                    }
                }
            }
            catch
            {
                // Ignored if no credentials exist
            }
        }
    }
}
