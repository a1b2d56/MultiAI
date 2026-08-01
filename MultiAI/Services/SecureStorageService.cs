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
            RemoveKey(provider);
            vault.Add(new PasswordCredential(VaultResource, provider, key));

            // Also save under canonical alias if provider is Gemini / Google Gemini
            if (provider == "Google Gemini")
            {
                RemoveKey("Gemini");
                vault.Add(new PasswordCredential(VaultResource, "Gemini", key));
            }
            else if (provider == "Gemini")
            {
                RemoveKey("Google Gemini");
                vault.Add(new PasswordCredential(VaultResource, "Google Gemini", key));
            }
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

                    // Alias fallback for Gemini vs Google Gemini
                    if ((provider == "Google Gemini" || provider == "Gemini") &&
                        (cred.UserName == "Google Gemini" || cred.UserName == "Gemini"))
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
                    bool isMatch = string.Equals(cred.UserName, provider, StringComparison.OrdinalIgnoreCase);
                    if (!isMatch && (provider == "Google Gemini" || provider == "Gemini"))
                    {
                        isMatch = (cred.UserName == "Google Gemini" || cred.UserName == "Gemini");
                    }

                    if (isMatch)
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
