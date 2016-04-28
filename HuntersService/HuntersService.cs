using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using HuntersService.Contracts;
using HuntersService.Contracts.Base;

namespace HuntersService
{

    [ServiceKnownType(typeof(LoginRequest))]
    [ServiceKnownType(typeof(LoginReply))]


    [ServiceKnownType(typeof(GetCustomersRequest))]
    [ServiceKnownType(typeof(GetCustomersReply))]

    [ServiceKnownType(typeof(GetAddressesRequest))]
    [ServiceKnownType(typeof(GetAddressesReply))]


    [ServiceKnownType(typeof(GetOptionsReply))]
    [ServiceKnownType(typeof(GetOptionsRequest))]

    [ServiceKnownType(typeof(GetQuestionsReply))]
    [ServiceKnownType(typeof(GetQuestionsRequest))]

    [ServiceKnownType(typeof(GetSurveyTypesRequest))]
    [ServiceKnownType(typeof(GetSurveyTypesReply))]

    [ServiceKnownType(typeof(GetRichMediasReply))]
    [ServiceKnownType(typeof(GetRichMediasRequest))]

    [ServiceKnownType(typeof(GetSurvelemMapsReply))]
    [ServiceKnownType(typeof(GetSurvelemMapsRequest))]

    [ServiceKnownType(typeof(GetSurvelemsReply))]
    [ServiceKnownType(typeof(GetSurvelemsRequest))]

    [ServiceKnownType(typeof(CheckItemsReply))]
    [ServiceKnownType(typeof(CheckItemsRequest))]

    [ServiceKnownType(typeof(SaveSurvelemRequest))]
    [ServiceKnownType(typeof(SaveAddressRequest))]
    [ServiceKnownType(typeof(SaveRichMediaRequest))]
    [ServiceKnownType(typeof(SaveAddressStatusRequest))]

    [ServiceKnownType(typeof(ImportRequest))]
    [ServiceKnownType(typeof(ImportSurveyorDataRequest))]

    [ServiceKnownType(typeof(ProcessAddressMoveRequest))]

    [ServiceKnownType(typeof(SaveQAAddressRequest))]
    [ServiceKnownType(typeof(SaveQAAddressCommentsRequest))]
    [ServiceKnownType(typeof(SaveAddressQuestionGroupStatusRequest))]

    [ServiceKnownType(typeof(GetAddressQuestionGroupStatusRequest))]
    [ServiceKnownType(typeof(GetAddressQuestionGroupStatusReply))]

    [ServiceKnownType(typeof(UploadFileRequest))]
    
    
    [ServiceContract]

    public interface IHuntersService
    {
        [OperationContract]
       // [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "getData/{value}")]
        BaseReply ProcessRequest(BaseRequest request);

    }


   
}
