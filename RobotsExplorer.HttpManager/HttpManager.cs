using System.Net;
using System.Net.Cache;

namespace RobotsExplorer.HttpManager
{
    public class HttpManager
    {
        public WebRequest WebRequestFactory(string Url, string proxy, string userAgent, int timeout)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            request.Credentials = CredentialCache.DefaultCredentials;
            
            if(!string.IsNullOrEmpty(userAgent))
                request.UserAgent = userAgent;
            
            if(timeout > 0)
                request.Timeout = timeout;

            if (!string.IsNullOrEmpty(proxy))
                request.Proxy = Util.Util.FormatProxyStringToProxyObject(proxy);

            return request;
        }
    }
}