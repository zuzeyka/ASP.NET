using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Data.Entity;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatesController : ControllerBase
    {
        private readonly DataContext _dataContext;

        public RatesController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public object Get([FromQuery] String data)
        {
            return new { result = $"Request is operated with method GET and recived data {data}" };
        }

        [HttpPost]
        public object Post([FromBody] BodyData bodyData)
        {
            int statusCode;
            String result;
            if (bodyData == null
                || bodyData.Data == null
                || bodyData.ItemId == null
                || bodyData.UserId == null)
            {
                statusCode = StatusCodes.Status400BadRequest;
                result= $"error, not all data recived: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
            }
            else
            {
                try
                {
                    Guid itemId = Guid.Parse(bodyData.ItemId);
                    Guid userId = Guid.Parse(bodyData.UserId);
                    int rating = Convert.ToInt32(bodyData.Data);

                    Rate? rate = _dataContext.Rates.FirstOrDefault(r => r.UserId == userId && r.ItemId == itemId);
                    if (rate is not null)
                    {
                        if(rate.Rating == rating)
                        {
                            statusCode = StatusCodes.Status406NotAcceptable;
                            result = $"error, data already operated: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                        }
                        else
                        {
                            rate.Rating = rating;
                            _dataContext.SaveChanges();
                            statusCode = StatusCodes.Status202Accepted;
                            result = $"Data updated: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                        }
                    }
                    else
                    {
                        _dataContext.Rates.Add(new()
                        {
                            ItemId = itemId,
                            UserId = userId,
                            Rating = rating
                        });
                        _dataContext.SaveChanges();
                        statusCode = StatusCodes.Status201Created;
                        result = $"Data saved: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                    }
                }
                catch
                {
                    statusCode = StatusCodes.Status400BadRequest;
                    result = $"error, data not operated: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                }
            }
            result = $"Request is operated with method POST and recived data {bodyData}";

            HttpContext.Response.StatusCode = statusCode;
            return new { result };
        }

        [HttpDelete]
        public object Delete([FromBody] BodyData bodyData)
        {
            int statusCode;
            String result;

            if (bodyData == null
                || bodyData.Data == null
                || bodyData.ItemId == null
                || bodyData.UserId == null)
            {
                statusCode = StatusCodes.Status400BadRequest;
                result = $"Not all data recived: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
            }
            else
            {
                try
                {
                    Guid itemId = Guid.Parse(bodyData.ItemId);
                    Guid userId = Guid.Parse(bodyData.UserId);
                    int rating = Convert.ToInt32(bodyData.Data);

                    Rate? rate = _dataContext.Rates.FirstOrDefault(r => r.UserId == userId && r.ItemId == itemId);
                    if (rate is not null)
                    {
                        _dataContext.Rates.Remove(rate);
                        _dataContext.SaveChanges();
                        statusCode = StatusCodes.Status202Accepted;
                        result = $"Data deleted: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                    }
                    else
                    {
                        statusCode = StatusCodes.Status406NotAcceptable;
                        result = $"Data is not exist (не можуть бути видалені): ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                    }
                }
                catch
                {
                    statusCode = StatusCodes.Status400BadRequest;
                    result = $"Data not operated: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                }
            }

            HttpContext.Response.StatusCode = statusCode;
            return new { result };
        }

        public object Default()
        {
            switch(HttpContext.Request.Method)
            {
                case "LINK": return Link();
                case "UNLINK": return Unlink();
                case "PATCH": return Patch();
                //case "POST": return Post();
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

        /*private object Post()
        {
            return new { result = $"Request is operated with method POST and recived data --" };
        }*/
    }
    public class BodyData
    {
        public String? Data { get; set; } = null!;
        public String? ItemId { get; set; } = null!;
        public String? UserId { get; set; } = null!;
    }
}
