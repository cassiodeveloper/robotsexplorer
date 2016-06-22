using NDesk.Options;
using RobotsExplorer.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;
using RobotsExplorer.Util;
using System.Xml;
using System.IO;

namespace RobotsExplorer
{
    public class Program
    {
        #region Private Objects

        private static string _urlTarget = null;
        private static string _proxy = null;
        private static string _domainList = null;
        private static string _userAgent = null;
        private static bool _showHelp = false;
        private static bool _showVersion = false;
        private static int _requestQuantity = int.MaxValue;
        private static int _requestTimeInterval = 0;
        private static int _requestTimeout = 0;

        private static HttpManager.HttpManager httpManager = null;
        private static Robot robot = null;
        private static OptionSet options;
        private static Domain domainNames;
        private static int controlRequestQuantity = 0;

        #endregion

        #region Public Methods

        public static void Main(string[] args)
        {
            WriteMessageAndSkipLine(ConfigManager.ConfigManager.artAscii, 1);

            ParseOptionsInput(args);

            if (domainNames != null && domainNames.DomainNames.IsNotNullAndHasCount())
            {
                foreach (var domain in domainNames.DomainNames)
                    PreExecution(domain);
            }
            else
            {
                if (ValidadeRequiredOptionsInput())
                    PreExecution(string.Empty);
            }

            FinishExecution();
        }

        #endregion

        #region Private Methods

        #region Flow

        private static void PreExecution(string domain)
        {
            if (!string.IsNullOrEmpty(domain))
                _urlTarget = domain;

            FormatDomainInput();

            Execute();
        }

        private static void Execute()
        {
            Util.Util.ChangeConsoleColorToDefault();

            WriteMessageAndSkipLine("-------------------------------------------", 0);
            WriteMessageAndSkipLine("<<< PROCESSING >>> " + _urlTarget, 0);
            WriteMessageAndSkipLine("-------------------------------------------", 1);
            WriteMessageAndSkipLine("Localizing target host...", 1);

            httpManager = new HttpManager.HttpManager();

            if (CanMakeMoreRequest())
            {
                var request = httpManager.WebRequestFactory(_urlTarget, _proxy, _userAgent, _requestTimeout);
                var response = request.GetResponse();

                controlRequestQuantity++;

                if (((HttpWebResponse)response).StatusCode == HttpStatusCode.OK)
                    ProcessResponse((HttpWebResponse)response);
                else
                {
                    Util.Util.ChangeConsoleColorToRed();
                    WriteMessageAndSkipLine("Sorry, I failed when I try to access the target :(", 0);
                }
            }
            else
                WriteMessageAndSkipLine("Sorry, you have reached the maximum number of requests that you informed as a parameter {r=" + _requestQuantity + "}.", 0);
        }

        private static void FinishExecution()
        {
            Util.Util.ChangeConsoleColorToDefault();

            WriteMessageAndSkipLine(string.Empty, 0);
            WriteMessageAndSkipLine("Thank you and happy hacking ;)", 1);
        }

        private static void ProceedToAttack()
        {
            WriteMessageAndSkipLine("... PROCESSING DIRECTORY ATTACK ...", 1);

            httpManager = new HttpManager.HttpManager();

            foreach (var directory in robot.Disallows)
            {
                Util.Util.ChangeConsoleColorToDefault();

                WriteMessageAndSkipLine(string.Empty, 1);
                WriteMessageAndSkipLine("Localizing target for directory: " + directory + " ...", 0);

                Attack(robot.Domain, directory);
            }

            FinishExecution();

            Console.Read();
        }

        private static void Attack(string domain, string directory)
        {
            if (CanMakeMoreRequest())
            {
                try
                {
                    PauseExecution(_requestTimeInterval);

                    var request = httpManager.WebRequestFactory(domain + directory, _proxy, _userAgent, _requestTimeout);
                    var response = request.GetResponse();

                    controlRequestQuantity++;

                    if (((HttpWebResponse)response).StatusCode == HttpStatusCode.OK)
                    {
                        Util.Util.ChangeConsoleColorToGreen();
                        WriteMessageAndSkipLine("Directory Listing enabled for directory: " + directory + " ;)", 0);
                    }
                    else
                    {
                        Util.Util.ChangeConsoleColorToRed();
                        WriteMessageAndSkipLine("Sorry, I failed when I try to access the target directory or the directory is pretty safe :(", 0);
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Message.Contains("(404)"))
                    {
                        Util.Util.ChangeConsoleColorToRed();
                        WriteMessageAndSkipLine("Sorry, this directory is no longer part of target domain. Response.HttpStatusCode = 404", 0);
                    }
                    else if (ex.Message.Contains("(403)"))
                    {
                        Util.Util.ChangeConsoleColorToRed();
                        WriteMessageAndSkipLine("This directory is preety safe, I got a (403) response code status :(", 0);
                    }
                    else
                    {
                        Util.Util.ChangeConsoleColorToRed();
                        WriteMessageAndSkipLine("Sorry, I failed when I try to access the target directory :(", 0);
                    }
                }
                catch (Exception ex)
                {
                    Util.Util.ChangeConsoleColorToRed();
                    WriteMessageAndSkipLine("Sorry, I failed when I try to access the target directory or the directory is pretty safe :(", 0);
                }
            }
            else
                WriteMessageAndSkipLine("Sorry, you have reached the maximum number of requests that you informed as a parameter {r=" + _requestQuantity + "}.", 0);
        }

        private static void ProcessResponse(HttpWebResponse response)
        {
            Util.Util.ChangeConsoleColorToGreen();

            WriteMessageAndSkipLine("TARGET FOUND :)", 1);

            Util.Util.ChangeConsoleColorToDefault();

            WriteMessageAndSkipLine("Getting Robots.txt...", 1);

            var robotsTxt = Util.Util.ParseResponseStreamToText(response);

            Util.Util.ChangeConsoleColorToGreen();

            WriteMessageAndSkipLine("VOILÀ!", 1);

            Util.Util.ChangeConsoleColorToDefault();

            WriteMessageAndSkipLine("Begin of file", 0);
            WriteMessageAndSkipLine("-------------------------------------------", 1);
            WriteMessageAndSkipLine(robotsTxt, 1);
            WriteMessageAndSkipLine("-------------------------------------------", 0);
            WriteMessageAndSkipLine("End of file", 1);

            robot = Util.Util.ParseRobotTxtToRobotObject(robotsTxt, _urlTarget);

            SiteMapXmlFile(robot);

            DisallowDirectories(robot);
        }

        private static void DisallowDirectories(Robot robot)
        {
            if (robot.Disallows != null && robot.Disallows.Count > 0)
            {
                WriteMessageAndSkipLine("Listing 'disallow' directories...", 1);

                foreach (var disallowDirectory in robot.Disallows)
                    WriteMessageAndSkipLine(disallowDirectory, 0);

                WriteMessageAndSkipLine(string.Empty, 0);

                if (AskUser("Do you want to exploit those directories? I am going to tell you if, they are or not listing directories! (Y/N)"))
                    ProceedToAttack();
                else
                    FinishExecution();
            }
            else
                WriteMessageAndSkipLine("There is no 'disallow' directories on the target.", 1);
        }

        private static void SiteMapXmlFile(Robot robot)
        {
            if (robot.SiteMaps.IsNotNullAndHasCount())
            {
                if (AskUser("I've found a Sitemap XML file, do you want to download it? (Y/N)"))
                {
                    robot.SiteMap = new SiteMap();

                    var request = httpManager.WebRequestFactory(robot.SiteMaps[0], _proxy, _userAgent, _requestTimeout);
                    var response = request.GetResponse();

                    controlRequestQuantity++;

                    if (((HttpWebResponse)response).StatusCode == HttpStatusCode.OK)
                    {
                        robot.SiteMap.SiteMapData = Util.Util.LoadSiteMapXml(Util.Util.ParseResponseStreamToText(request.GetResponse()));

                        Util.Util.ChangeConsoleColorToGreen();

                        WriteMessageAndSkipLine("VOILÀ!", 1);

                        Util.Util.ChangeConsoleColorToDefault();

                        var fullPath = Util.Util.SaveFile(robot.SiteMap.SiteMapData.InnerXml, _urlTarget);

                        WriteMessageAndSkipLine("-------------------------------------------", 1);
                        WriteMessageAndSkipLine("Sitemap file downloaded at: " + fullPath, 1);
                        WriteMessageAndSkipLine("-------------------------------------------", 0);
                    }
                    else
                    {
                        Util.Util.ChangeConsoleColorToRed();
                        WriteMessageAndSkipLine("Sorry, I failed when I try to access the target directory or the directory is pretty safe :(", 0);
                    }
                }
            }
        }

        private static bool AskUser(string questionText)
        {
            var answer = string.Empty;

            do
            {
                WriteMessageAndSkipLine(questionText, 1);

                answer = Console.ReadLine();

                if ((string.IsNullOrEmpty(answer)) || (answer.ToUpper() != "Y" && answer.ToUpper() != "N"))
                {
                    WriteMessageAndSkipLine(string.Empty, 0);
                    WriteMessageAndSkipLine("Wrong answer ¬¬", 0);
                }

            } while (answer.ToUpper() != "Y" && answer.ToUpper() != "N");

            return answer.ToUpper() == "Y" ? true : false;
        }

        #endregion

        #region Auxiliary

        private static void FormatDomainInput()
        {
            if (_urlTarget.ToLower().EndsWith(".txt") || _urlTarget.ToLower().EndsWith(".txt/"))
            {
                _urlTarget.Replace("robots.txt", string.Empty);
                _urlTarget.Replace("robots.txt/", string.Empty);
                _urlTarget.Replace("/robots.txt/", string.Empty);
                _urlTarget.Replace("/robots.txt", string.Empty);
            }
            else
            {
                if (_urlTarget.Substring(_urlTarget.Length - 1, 1) == "/")
                    _urlTarget += ConfigManager.ConfigManager.robotPath.Replace("/", string.Empty);
                else
                    _urlTarget += ConfigManager.ConfigManager.robotPath;
            }
        }

        private static void ParseOptionsInput(string[] args)
        {
            options = new OptionSet()
                .Add("u|urlTarget=", ConfigManager.ConfigManager.urlHelpText, u => _urlTarget = u)
                .Add("p|proxy=", ConfigManager.ConfigManager.proxyHelpText, p => _proxy = p)
                .Add("l|list=", ConfigManager.ConfigManager.fileListHelpText, l => _domainList = l)
                .Add("a|userAgent=", ConfigManager.ConfigManager.userAgentHelpText, a => _userAgent = a)
                .Add("r|requestQuantity=", ConfigManager.ConfigManager.requestQuantityHelpText, r => _requestQuantity = Convert.ToInt32(r))
                .Add("i|requestTimeInterval=", ConfigManager.ConfigManager.requestTimeIntervalHelpText, i => _requestTimeInterval = Convert.ToInt32(i))
                .Add("t|requestTimeout=", ConfigManager.ConfigManager.requestTimeoutHelpText, i => _requestTimeout = Convert.ToInt32(i))
                .Add("v|version=", ConfigManager.ConfigManager.versionHelpText, v => _showVersion = v != null)
                .Add("?|h|help", ConfigManager.ConfigManager.helpText, h => _showHelp = h != null);

            options.Parse(args);

            ValidadeOptionsInput();
        }

        private static void ValidadeOptionsInput()
        {
            if (_showVersion)
            {
                DisplayVersion();
                Environment.Exit(0);
            }

            if (_showHelp)
                DisplayHelp(options);

            if (!string.IsNullOrEmpty(_domainList))
            {
                domainNames = new Domain
                {
                    DomainNames = new List<string>()
                };

                try
                {
                    Util.Util.ReadTextFileAndFillDomainsList(_domainList, domainNames);
                }
                catch (Exception ex)
                {
                    Util.Util.ChangeConsoleColorToRed();
                    WriteMessageAndSkipLine("Sorry, I failed when I try to access the domain list file :(", 0);
                }
            }
            else
                ValidadeRequiredOptionsInput();
        }

        private static bool ValidadeRequiredOptionsInput()
        {
            var valid = false;

            if (string.IsNullOrEmpty(_urlTarget))
                DisplayHelp(options);
            else
                valid = true;

            return valid;
        }

        private static void DisplayHelp(OptionSet options)
        {
            WriteMessageAndSkipLine("Robots Explorer --- Version: " + GetRobotsExplorerVersion() + " Released: June, 2016", 0);
            WriteMessageAndSkipLine("Copyright (C) 2016 Batista Pereira, Cássio (http://github.com/cassiodeveloper)", 0);
            WriteMessageAndSkipLine("http://cassiodeveloper.github.io/robotsexplorer", 1);
            WriteMessageAndSkipLine("Options:", 0);
            options.WriteOptionDescriptions(Console.Out);
        }

        private static void DisplayVersion()
        {
            WriteMessageAndSkipLine(GetRobotsExplorerVersion(), 0);
        }

        private static bool CanMakeMoreRequest()
        {
            return controlRequestQuantity <= _requestQuantity ? true : false;
        }

        private static string GetRobotsExplorerVersion()
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        }

        private static void WriteMessageAndSkipLine(string message, int linesToSkip)
        {
            Console.WriteLine(message);

            for (int i = 0; i < linesToSkip; i++)
                Console.WriteLine();
        }

        private static void PauseExecution(int miliseconds)
        {
            try
            {
                if (miliseconds > 0)
                    Thread.Sleep(miliseconds);
            }
            catch (Exception ex)
            {
                Util.Util.ChangeConsoleColorToRed();
                WriteMessageAndSkipLine("Sorry, I failed when I try to set Time Interval between requests :(", 0);
            }
        }

        #endregion

        #endregion
    }
}