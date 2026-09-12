using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Services;

namespace StrmAssistant.Web.Api
{
    [Route("/{Web}/components/strmassistant/externalplayer.js", "GET", IsHidden = true)]
    [Unauthenticated]
    public class GetExternalPlayerJs
    {
        public string Web { get; set; }
    }
}
