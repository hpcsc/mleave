using Newtonsoft.Json;
using System;
using System.Web.Mvc;

namespace MLeave.Controllers
{
    public class ControllerBase : Controller
    {
        protected ActionResult JsonNet(object data)
        {
            return new JsonNetResult { Data = data };
        }
    }

    public class JsonNetResult : JsonResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var response = context.HttpContext.Response;

            response.ContentType = !String.IsNullOrEmpty(ContentType)
                ? ContentType
                : "application/json";

            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }

            var formatter = new JsonSerializerSettings();
            //camelCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
            formatter.Formatting = Formatting.Indented;

            var serializedObject = JsonConvert.SerializeObject(Data, formatter);
            response.Write(serializedObject);
        }
    }
}