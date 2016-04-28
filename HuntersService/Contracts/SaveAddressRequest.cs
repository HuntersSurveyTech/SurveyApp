using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HuntersService.Contracts.Base;
using HuntersService.Entities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Omu.ValueInjecter;

namespace HuntersService.Contracts
{
    //public class OperationItemResult
    //{
    //    public Guid Id { get; set; }
    //    public bool IsSuccess { get; set; }
    //    public string Message { get; set; }
    //}


    public class UploadFileRequest : BaseAuthRequest
    {
        public byte[] File { get; set; }

        public string FileName { get; set; }

        public bool IsCreatePDF { get; set; }

        public string WatermarkText { get; set; }

        public string PDFFileName { get; set; }
    }

    public class SaveQAAddressRequest : BaseAuthRequest
    {
        public List<QAAddress> Items { get; set; }
    }

    public class SaveAddressQuestionGroupStatusRequest : BaseAuthRequest
    {
        public List<AddressQuestionGroupStatus> Items { get; set; }
    }

    public class SaveQAAddressCommentsRequest : BaseAuthRequest
    {
        public List<QAAddressComment> Items { get; set; }
    }



    public class SaveAddressStatusRequest : BaseAuthRequest
    {
        public List<AddressStatus> Items { get; set; }
    }

    public class SaveAddressRequest:BaseAuthRequest
    {
        public List<Address> Items { get; set; }
    }

    public class SaveSurvelemRequest : BaseAuthRequest
    {
        public List<Survelem> Items { get; set; }
    }

    public class SaveRichMediaRequest : BaseAuthRequest
    {
        public List<RichMedia> Items { get; set; }
    }

    public class SaveAddressQuestionGroupStatusRequestHandler : RequestHandler<SaveAddressQuestionGroupStatusRequest, BaseReply>
    {
        protected override BaseReply Execute(SaveAddressQuestionGroupStatusRequest request)
        {
            var reply = CheckUser<BaseReply>(request);

            if (reply != null) return reply;


            var forSave = new List<Entity>();

            foreach(var e in request.Items)
            {
                var dbEntity =
                    DbContext.AddressQuestionGroupStatuses.Where(x => x.AddressId == e.AddressId && x.Group == e.Group)
                        .FirstOrDefault();

                if (dbEntity == null)
                {
                   forSave.Add(e);
                }
            }


            reply = SaveEntities<AddressQuestionGroupStatus>(forSave, new List<string>() { "Address" });

            return reply;
        }
    }


    public class SaveQAAddressRequestHandler : RequestHandler<SaveQAAddressRequest, BaseReply>
    {
        protected override BaseReply Execute(SaveQAAddressRequest request)
        {
            var reply = CheckUser<BaseReply>(request);

            if (reply != null) return reply;



            reply = SaveEntities<QAAddress>(request.Items.Cast<Entity>().ToList(), new List<string>() { "Address", "Surveyor" });
            
            return reply;
        }
    }

    public class SaveQAAddressCommentRequestHandler : RequestHandler<SaveQAAddressCommentsRequest, BaseReply>
    {
        protected override BaseReply Execute(SaveQAAddressCommentsRequest request)
        {
            var reply = CheckUser<BaseReply>(request);

            if (reply != null) return reply;

            reply = SaveEntities<QAAddressComment>(request.Items.Cast<Entity>().ToList(), new List<string>() { "Address" });
            
            return reply;
        }
    }

    public class SaveAddressStatusRequestHandler : RequestHandler<SaveAddressStatusRequest, BaseReply>
    {
        protected override BaseReply Execute(SaveAddressStatusRequest request)
        {
            var reply = CheckUser<BaseReply>(request);

            if (reply != null) return reply;

            reply = new BaseReply();


                foreach (var i in request.Items)
                {
                    var db = DbContext.AddressStatus.FirstOrDefault(x => x.AddressId == i.AddressId);

                    if (db != null && i.IsDeletedOnClient)
                    {
                        DbContext.AddressStatus.Remove(db);
                        DbContext.SaveChanges();
                    }

                    if (i.IsDeletedOnClient) continue;

                    if (db != null)
                    {
                        db.CompletedQuestionsCount = i.CompletedQuestionsCount;
                        db.IsCompleted = i.IsCompleted;
                        db.UpdateDate = DateTime.UtcNow;
                        DbContext.SaveChanges();
                    }
                    else
                    {
                        db = new AddressStatus();
                        db.AddressId = i.AddressId;
                        db.CompletedQuestionsCount = i.CompletedQuestionsCount;
                        db.IsCompleted = i.IsCompleted;
                        db.AppVersion = i.AppVersion;
                        db.UpdateDate = DateTime.UtcNow;

                        DbContext.AddressStatus.Add(db);
                        DbContext.SaveChanges();
                    }
                }


            return reply;
        }
    }

    public class SaveSurvelemRequestHandler : RequestHandler<SaveSurvelemRequest, BaseReply>
    {
        protected override BaseReply Execute(SaveSurvelemRequest request)
        {
            var reply = CheckUser<BaseReply>(request);

            if (reply != null) return reply;


            reply = SaveEntities<Survelem>(request.Items.Cast<Entity>().ToList(), new List<string>() { "Option", "Option2ndry" });

            //foreach (var i in request.Items)
            //{
            //    var r = SaveEntities<SurvelemSecond>(i.SurvelemSeconds.Cast<Entity>().ToList(), new List<string>() { "Survelem" });

            //    if (!r.IsSuccess) return r;
            //}

            return reply;
        }
    }



    public class SaveRichMediasRequestHandler : RequestHandler<SaveRichMediaRequest, BaseReply>
    {
        protected override BaseReply Execute(SaveRichMediaRequest request)
        {
            var reply = CheckUser<BaseReply>(request);

            if (reply != null) return reply;

            return SaveEntities<RichMedia>(request.Items.Cast<Entity>().ToList(), new List<string>() { "Option" });
        }
    }

    public class UploadFileRequestHandler : RequestHandler<UploadFileRequest, BaseReply>
    {
        protected override BaseReply Execute(UploadFileRequest request)
        {
            var reply = CheckUser<BaseReply>(request);

            if (reply != null) return reply;

            var image = ImageService.Watermark(request.File, request.WatermarkText);

            reply = new BaseReply();

            reply.Data =  UploadToCloud(image, request.FileName);

            if (request.IsCreatePDF)
            {
                var pdf = ImageService.ConvertToPDF(image);
                UploadToCloud(pdf, request.PDFFileName);
            }
            return reply;

        }

        private string UploadToCloud(byte[] data, string filename)
        {
            var account =
                new CloudStorageAccount(
                    new StorageCredentials("hunterscollect",
                        "kgn29dcfazCtxIT8UZpPIUuFX6MiLaFSl11Vu4HPa2V42j5Gk7WtZA/KV4gdrvPvU+evRaRLXDJ6r7Eva4CIXQ=="),
                    true);

            var client = account.CreateCloudBlobClient();

            var container = client.GetContainerReference("hunters-photos");
            container.CreateIfNotExists();

           container.SetPermissions(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Blob });

           var blob = container.GetBlockBlobReference(filename);

            blob.UploadFromByteArray(data, 0, data.Length);


            return blob.Uri.ToString();
        }
    }

    public class SaveAddressRequestHandler : RequestHandler<SaveAddressRequest, BaseReply>
    {
        protected override BaseReply Execute(SaveAddressRequest request)
        {
            var reply = CheckUser<BaseReply>(request);

            if (reply != null) return reply;

            return SaveEntities<Address>(request.Items.Cast<Entity>().ToList(), new List<string>() { "Surveyor", "IsLoadToPhone" });
        }
    }




}