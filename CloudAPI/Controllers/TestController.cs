using CloudAPI.AL.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudAPI.Controllers
{
    [ApiController]
    [Route("")]
    public class TestController : ControllerBase
    {
        public ConfigurationModel _config;

        public TestController(ConfigurationModel config) {
            _config = config;
        }

        [HttpGet]
        public IActionResult GetJson() {
            return Ok(new { 
                Message= "API is online",
                BuildType = _config.BuildType,
                LibraryPath = _config.LibraryPath
            });
        }
    }
}
