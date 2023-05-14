using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatesController : ControllerBase
    {
        [HttpGet]
        public object Get([FromQuery] String data)
        {
            return new { result = $"Request is operated with method GET and recived data {data}" };
        }
        /*
        [HttpPost]
        public object Post([FromBody] BodyData bodyData)
        {
            return new { result = $"Request is operated with method POST and recived data {bodyData}" };
        }*/

        public object Default()
        {
            switch(HttpContext.Request.Method)
            {
                case "LINK": return Link();
                case "UNLINK": return Unlink();
                case "PATCH": return Patch();
                case "POST": return Post();
                default: throw new NotImplementedException();
            }
        }

        private object Link()
        {
            return new { result = $"Request is operated with method LINK and recived data --" };
        }

        private object Unlink()
        {
            return new { result = $"Request is operated with method UNLINK and recived data --" };
        }

        private object Patch()
        {
            return new { result = $"Request is operated with method PATCH and recived data --" };
        }

        private object Post()
        {
            return new { result = $"Request is operated with method POST and recived data --" };
        }
    }
    public class BodyData
    {
        public String Data { get; set; } = null!;
    }
}
