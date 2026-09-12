using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Services;

namespace StrmAssistant.Web.Api
{
    [Route("/StrmAssistant/ExternalPlayer/Config", "GET", IsHidden = true)]
    [Unauthenticated]
    public class GetExternalPlayerConfig
    {
    }

    public class ExternalPlayerConfigResponse
    {
        public bool Enabled { get; set; }

        public bool StrmDirect { get; set; }
    }
}
