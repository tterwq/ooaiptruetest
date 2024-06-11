// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Net;
using CoreWCF;
using CoreWCF.OpenApi.Attributes;
using CoreWCF.Web;

namespace WebHttp
{
    [ServiceContract]
    [OpenApiBasePath("/ioc")]
    public interface IIoCApi
    {
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/init", RequestFormat = WebMessageFormat.Json)]
        [OpenApiTag("Tag")]
        [OpenApiResponse(ContentTypes = new[] { "application/json" }, Description = "Success", StatusCode = HttpStatusCode.OK, Type = typeof(string))]
        void Init();

    }
}
