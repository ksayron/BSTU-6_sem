using Microsoft.AspNetCore.Mvc;
using TDWA_01_01_2.Models;
using TDWA_01_01_2.Services;

namespace TDWA_01_01_2.Controllers
{
    [ApiController]
    [Route("NGINX-test")]
    public class RequestController : ControllerBase
    {
        private double Calculate(string op, double x, double y)
        {
            return op switch
            {
                "add" => x + y,
                "sub" => x - y,
                "mul" => x * y,
                "div" when y != 0 => x / y,
                "div" => throw new Exception("division by zero"),
                _ => throw new Exception("unknown operation")
            };
        }

        // GET
        [HttpGet]
        public IActionResult Get()
        {
            var data = StoreService.Get();

            if (data == null)
                return NotFound("JSON request not found");

            return Ok(data);
        }

        // POST
        [HttpPost]
        public IActionResult Post([FromBody] Request req)
        {
            try
            {
                var result = Calculate(req.Op, req.X, req.Y);

                var resp = new Response
                {
                    Op = req.Op,
                    X = req.X,
                    Y = req.Y,
                    Result = result
                };

                StoreService.Set(resp);

                return Ok(resp);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT
        [HttpPut]
        public IActionResult Put([FromBody] Request req)
        {
            if (!StoreService.Exists())
                return NotFound("JSON request not found");

            try
            {
                var result = Calculate(req.Op, req.X, req.Y);

                var resp = new Response
                {
                    Op = req.Op,
                    X = req.X,
                    Y = req.Y,
                    Result = result
                };

                StoreService.Set(resp);

                return Ok(resp);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE
        [HttpDelete]
        public IActionResult Delete()
        {
            var deleted = StoreService.Delete();

            if (!deleted)
                return NotFound("JSON request not found");

            return Ok(new { message = "Deleted successfully" });
        }
    }
}
