using System;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using ProductScanner.Core.Scanning.Storage;

namespace York.Metadata
{
    public class YorkInventoryFileDownloader
    {
        private string _inventoryFilename = "York Inventory.CSV";
        private readonly IStorageProvider<YorkVendor> _storageProvider;

        public YorkInventoryFileDownloader(IStorageProvider<YorkVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public void Download()
        {
            var staticFolder = _storageProvider.GetStaticFolder();
            var applicationName = "Gmail API .NET Quickstart";
            var credential = Authenticate(Path.Combine(staticFolder, "EmailInfo"));
            var service = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
            });

            var request = service.Users.Messages.List("me");
            var messageIds = request.Execute().Messages;

            var allMessages = messageIds.Select(x => GetMessage(service, x.Id));
            var yorkMessages = allMessages.Where(x => x.Payload.Parts.Any(p => p.Filename == _inventoryFilename));
            var sortedMessages = yorkMessages.OrderByDescending(x => x.InternalDate).ToList();
            if (sortedMessages.Count > 1)
                SaveInventoryFile(sortedMessages.First(), service);
        }

        private UserCredential Authenticate(string authFolder)
        {
            string[] scopes = { GmailService.Scope.GmailReadonly };
            using (var stream = new FileStream(Path.Combine(authFolder, "client_id.json"), FileMode.Open, FileAccess.Read))
            {
                return GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(Path.Combine(authFolder, "gmail-dotnet-quickstart.json"), true)).Result;
            }
        }

        private Message GetMessage(GmailService service, string messageId)
        {
            return service.Users.Messages.Get("me", messageId).Execute();
        }

        private void SaveInventoryFile(Message message, GmailService service)
        {
            var parts = message.Payload.Parts;
            foreach (var part in parts)
            {
                if (part.Filename != _inventoryFilename) continue;

                var attId = part.Body.AttachmentId;
                var attachPart = service.Users.Messages.Attachments.Get("me", message.Id, attId).Execute();

                // Converting from RFC 4648 base64 to base64url encoding
                // see http://en.wikipedia.org/wiki/Base64#Implementations_and_history
                var attachData = attachPart.Data.Replace('-', '+');
                attachData = attachData.Replace('_', '/');

                var data = Convert.FromBase64String(attachData);
                _storageProvider.SaveFile(CacheFolder.Files, part.Filename, data);
            }
        }
    }
}