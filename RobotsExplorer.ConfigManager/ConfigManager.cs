namespace RobotsExplorer.ConfigManager
{
    public static class ConfigManager
    {
        #region Public Constants

        public const string robotPath = "/robots.txt";
        public const string urlHelpText = "The {URL} to scan.";
        public const string proxyHelpText = "The {PROXY} pattern if you are behind one. Ex: http:user:password:domain:port";
        public const string fileListHelpText = "A full {FILE PATH} with a list of domains to scan.";
        public const string versionHelpText = "The {VERSION} of Robots Explorer.";
        public const string userAgentHelpText = "The string name of a {USER AGENT} to make the request, default is null. List of User Agents updated at: \n useragentstring.com/pages/Browserlist/";
        public const string requestQuantityHelpText = "The max {QUANTITY} of total requests, default is no limit.";
        public const string requestTimeIntervalHelpText = "The {INTERVAL TIME} in miliseconds between each request, default is 0 miliseconds.";
        public const string requestTimeoutHelpText = "The {TIMEOUT} in miliseconds for each request, default is 100.000 miliseconds (100 seconds).";
        public const string helpText = "Need help?";
        public const string artAscii = @"
____   ___   ____    ___   ______  _____       ___  __ __  ____  _       ___   ____     ___  ____  
|    \ /   \ |    \  /   \ |      |/ ___/      /  _]|  |  ||    \| |     /   \ |    \   /  _]|    \ 
|  D  )     ||  o  )|     ||      (   \_      /  [_ |  |  ||  o  ) |    |     ||  D  ) /  [_ |  D  )
|    /|  O  ||     ||  O  ||_|  |_|\__  |    |    _]|_   _||   _/| |___ |  O  ||    / |    _]|    / 
|    \|     ||  O  ||     |  |  |  /  \ |    |   [_ |     ||  |  |     ||     ||    \ |   [_ |    \ 
|  .  \     ||     ||     |  |  |  \    |    |     ||  |  ||  |  |     ||     ||  .  \|     ||  .  \
|__|\_|\___/ |_____| \___/   |__|   \___|    |_____||__|__||__|  |_____| \___/ |__|\_||_____||__|\_|";

        #endregion
    }
}