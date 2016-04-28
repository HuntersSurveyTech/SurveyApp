using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HuntersWP.Db;
using HuntersWP.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace HuntersWP.Services
{
    public class InternalSyncEngine
    {
        public static async Task Execute(bool isQA)
        {
            var notSuccessAddresses = new List<Address>();
            if (!isQA)
            {



                var addresses = await new DbService().GetNotSyncedEntities<Address>();

                foreach (var ad in addresses)
                {
                    var addressesResult = await new DataLoaderService().SaveAddresses(new List<Address>() { ad });

                    if (!addressesResult[0].IsSuccess)
                    {
                        notSuccessAddresses.Add(ad);
                        var pars = new Dictionary<string,string>();
                        pars.Add("UPRN", ad.UPRN);
                        pars.Add("Id", ad.Id.ToString());
                        Helpers.LogEvent("AddressSaveFail",pars);
                    }
                }


                //
                var addressStatuses = await new DbService().GetNotSyncedEntities<AddressStatus>();
                await new DataLoaderService().SaveAddressStatuses(addressStatuses.Where(x => notSuccessAddresses.All(y => y.Id != x.AddressId)).ToList());
                //
                var addressQuestionGroupStatuses = await new DbService().GetNotSyncedEntities<AddressQuestionGroupStatus>();
                await new DataLoaderService().SaveAddressQuestionGroupStatus(addressQuestionGroupStatuses.Where(x => notSuccessAddresses.All(y => y.Id != x.AddressId)).ToList());
                //

                var notSyncedSurvelems = await new DbService().GetNotSyncedEntities<Survelem>();
                var syncResult = await new DataLoaderService().SaveSurvelems(notSyncedSurvelems.Where(x => notSuccessAddresses.All(y => string.Compare(y.UPRN, x.UPRN, StringComparison.InvariantCultureIgnoreCase) != 0)).ToList());

                var syncFailedSurvelemCount = syncResult.Count(x => !x.IsSuccess);

                Helpers.DebugMessage(string.Format("Total survelems failed: {0}-{1}", syncFailedSurvelemCount, notSyncedSurvelems.Count));





                var addressesToRemove = await new DbService().GetAddressesToRemove();

                foreach (var ad in addressesToRemove)
                {
                    await new DbService().Delete(ad);

                }

                var survelemsToRemove = await new DbService().GetSurvelemsToRemove();

                foreach (var s in survelemsToRemove)
                {
                    await new DbService().Delete(s);
                }
            }

            var medias = await new DbService().GetNotSyncedEntities<RichMedia>();

            if (!isQA)
            {
                medias = medias.Where(x => notSuccessAddresses.All(y => string.Compare(y.UPRN, x.UPRN, StringComparison.InvariantCultureIgnoreCase) != 0)).ToList();
            }


            foreach (var media in medias)
            {
                Helpers.DebugMessage(string.Format("Media: {0}-{1}", medias.IndexOf(media), medias.Count));
                await UploadMediaFile(media);
            }



            var qaaddresses = await new DbService().GetNotSyncedEntities<QAAddress>();

            await new DataLoaderService().SaveQAAddresses(qaaddresses);


            var qaaddressesComments = await new DbService().GetNotSyncedEntities<QAAddressComment>();

            await new DataLoaderService().SaveQAAddressComments(qaaddressesComments);

            ///Remove isLoadToPhoneAddresses = false

            var addressesToRemoveIsNotLoadToPhone = await new DbService().GetAddressesToRemoveIsNotLoadToPhone();

            foreach (var ad in addressesToRemoveIsNotLoadToPhone)
            {
                var survelems = await new DbService().GetSyncedSurvelemsByAddressUPRN(ad.UPRN);

                foreach (var s in survelems)
                {
                    await new DbService().Delete(s);
                }


                await new DbService().Delete(ad);
            }
            ///
        }

        static async Task UploadMediaFile(RichMedia media)
        {
#if IMPORT
            await new DataLoaderService().SaveRichMedias(new List<RichMedia>() { media });
            return;
#endif

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var file = iso.OpenFile(media.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    var data = new byte[file.Length];
                    await file.ReadAsync(data, 0, data.Length);


                    var uploadResult = await
                           new DataLoaderService().UploadFile(data, media.FileName, media.IsCreatePDF,
                               media.PDFFileName, media.WatermarkText);

                    if (uploadResult.IsSuccess)
                    {
                        media.CloudPath = uploadResult.Data;
                        await new DataLoaderService().SaveRichMedias(new List<RichMedia>() { media });
                    }
                    else
                    {
                        media.HasError = true;
                        media.ErrorMessage = uploadResult.Data;

                        await UploadToCloud(data, media.UPRN, media.Question_Ref);

                        await new DbService().Save(media, ESyncStatus.NotSynced);
                    }

                }
            }


        }

        private static async Task<string> UploadToCloud(byte[] data, string uprn, string questionRef)
        {
            var account =
                new CloudStorageAccount(
                    new StorageCredentials("hunterscollect",
                        "kgn29dcfazCtxIT8UZpPIUuFX6MiLaFSl11Vu4HPa2V42j5Gk7WtZA/KV4gdrvPvU+evRaRLXDJ6r7Eva4CIXQ=="),
                    true);

            var client = account.CreateCloudBlobClient();

            var container = client.GetContainerReference("hunters-photos-failed");
            await container.CreateIfNotExistsAsync();

            await container.SetPermissionsAsync(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Off });

            var qaText = "";
            if (StateService.IsQA) qaText = "QA";
            var blob = container.GetBlockBlobReference(uprn + "_" + questionRef + "_" + qaText + "_" + Guid.NewGuid().ToString());

            await blob.UploadFromByteArrayAsync(data, 0, data.Length);

            return blob.Uri.ToString();
        }
    }
}
