using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HuntersService.Contracts.Base;
using HuntersService.Entities;

namespace HuntersService.Contracts
{
    public class LoginRequest:BaseRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class LoginReply : BaseReply
    {
        public Guid? UserId { get; set; }
        public int Timestamp { get; set; }

        public List<AddressMove> AddressMoves { get; set; }

        public int Type { get; set; }

        public List<QAAddress> QaAddresses { get; set; }
        public List<QAAddressComment> QaAddressComments { get; set; }

    }


    public class LoginRequestHandler : RequestHandler<LoginRequest, LoginReply>
    {
        protected override LoginReply Execute(LoginRequest request)
        {
            var user =
                DbContext.Surveyors.FirstOrDefault(x => x.Username == request.Login && x.Password == request.Password);
            var r =  new LoginReply()
            {
                UserId = user != null ? user.Id : (Guid?)null,
                IsSuccess = user != null,
                Timestamp =  user != null ? user.SyncTimestampForQuestionsOptions:0,
                Type = user != null ? user.Type : 0
            };

            if(user != null)
                r.AddressMoves = DbContext.AddressMoves.Where(x => (!x.IsProcessedFrom && x.FromSurveyorId == user.Id) || (!x.IsProcessedTo && x.ToSurveyorId == user.Id)).ToList();

            if (user != null && user.Type == (int) ESurveyorType.QA)
            {
                r.QaAddresses = DbContext.QAAddresses.Where(x => !x.IsCompleted && x.SurveyorId == user.Id).ToList();

                var ids = r.QaAddresses.Select(x => x.Id).ToList();

                r.QaAddressComments = DbContext.QAAddressComments.Where(x => ids.Contains(x.AddressId)).ToList();
            }
 
            return r;
        }
    }

    public class ProcessAddressMoveRequestHandler : RequestHandler<ProcessAddressMoveRequest, BaseReply>
    {
        protected override BaseReply Execute(ProcessAddressMoveRequest request)
        {
            var r = CheckUser<BaseReply>(request);

            if (r != null) return r;

            var move = DbContext.AddressMoves.Find(request.Id);

            if (move != null)
            {
                move.IsProcessedFrom = request.IsFrom;
                move.IsProcessedTo = request.IsTo;
                move.UpdateDate = DateTime.UtcNow;

                var address = DbContext.Addresses.Find(move.AddressId);
                if (address != null && move.ToSurveyorId != null)
                {
                    address.SurveyorId = move.ToSurveyorId.Value;
                }
             
            }

            DbContext.SaveChanges();

            return new BaseReply();
        }
    }

    public class ProcessAddressMoveRequest : BaseAuthRequest
    {
        public bool IsTo { get; set; }
        public bool IsFrom { get; set; }
        public Guid Id { get; set; }
    }
}