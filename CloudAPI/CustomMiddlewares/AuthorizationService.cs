using CloudAPI.AL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudAPI.CustomMiddlewares
{
    public class AuthorizationService
    {
        ConfigurationModel _config;

        public AuthorizationService(ConfigurationModel config) {
            _config = config;
        }

        public void DisableRouteOnPublicBuild() {
            if(_config.IsPublicBuild){
                throw new UnauthorizedAccessException("Data changing operations are disabled in public build. Thank you 👍");
            }
        }
    }
}
