using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web;

namespace WebTestConsole
{
    class Program
    {
        private string _baseUrl;
        private int _iterations;
        private string _outputFile;
        private string _tag; 
        private string _urlList;

        static void Main(string[] args)
        {
            try
            {
                Program program = ParseOptions(args);
                program.Run();
            }
            catch (Exception x)
            {
                Console.WriteLine(x);
            }
        }

        public void Run()
        {
            string urlListPath = Path.Combine(Environment.CurrentDirectory, _urlList);

            List<string> urls = new List<string>(); 
            StreamReader streamReader = File.OpenText(urlListPath);
            try
            {
                string line; 
                while ((line = streamReader.ReadLine()) != null)
                {
                    urls.Add(line.Trim()); 
                }
            }
            finally
            {
                streamReader.Close(); 
            }

            Stopwatch stopwatch = new Stopwatch();

            string outputPath = Path.Combine(Environment.CurrentDirectory, _outputFile); 

            DateTime startTime = DateTime.Now; 
            string timecode = startTime.ToString("yyyy-MM-dd-HH-mm-ss"); 

            outputPath = Path.Combine(Path.GetDirectoryName(outputPath), 
                Path.GetFileNameWithoutExtension(outputPath) + "-" + timecode + 
                Path.GetExtension(outputPath));

            string errorPath = outputPath + ".errors"; 

            StreamWriter outputWriter = new StreamWriter(outputPath);
            StreamWriter errorWriter = new StreamWriter(errorPath);

            outputWriter.WriteLine("tag, starttime, time, iteration, exception, statuscode, ttfb, ttlb, bytes, url"); 
            for (int iteration = 0; iteration < _iterations; ++iteration)
            {
                foreach (string relativeUrl in urls)
                {
                    bool succeeded = false;
                    string absoluteUrl = string.Empty; 
                    long ttfb = 0; 
                    long ttlb = 0; 
                    long bytes = 0; 
                    int status = 0;
                    string statusDescription = string.Empty; 
                    try
                    {
                        absoluteUrl = UrlCombine(_baseUrl, relativeUrl);
                        stopwatch.Stop();
                        stopwatch.Reset();
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(absoluteUrl);
                        request.AllowAutoRedirect = false; 
                        stopwatch.Start();
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        ttfb = stopwatch.ElapsedMilliseconds;
                        bytes = ConsumeStream(response.GetResponseStream());
                        stopwatch.Stop();
                        ttlb = stopwatch.ElapsedMilliseconds;
                        status = (int) response.StatusCode;
                        statusDescription = response.StatusDescription; 
                        response.Close();
                        succeeded = true; 
                    }
                    catch (Exception x)
                    {
                        errorWriter.WriteLine("URL: ({0}) {1}", iteration, absoluteUrl);
                        errorWriter.WriteLine(x);
                        errorWriter.WriteLine(); 
                    }

                    outputWriter.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                        _tag,
                        startTime, 
                        DateTime.Now,
                        iteration,
                        !succeeded,
                        status,
                        ttfb,
                        ttlb,
                        bytes,
                        absoluteUrl
                    ); 

                    Console.WriteLine("Iteration: {0} {1}", iteration, absoluteUrl);
                    Console.WriteLine("Exception? {0}", !succeeded); 
                    Console.WriteLine("TTFB/TTLB: {0}/{1}", ttfb, ttlb);
                    Console.WriteLine("Bytes: {0}", bytes); 
                    Console.WriteLine("Status: {0} {1}", status, statusDescription);
                    Console.WriteLine(); 
                }
            }

            outputWriter.Flush(); 
            outputWriter.Close(); 
        }

        private static long ConsumeStream(Stream stream)
        {
            long bytesRead = 0;

            byte[] buffer = new byte[1024];

            int read;
            while ((read = stream.Read(buffer, 0, 1024)) != 0)
            {
                bytesRead += read;
            }

            return bytesRead;
        }
        private static Program ParseOptions(string[] args)
        {
            Program program = new Program();
            for (int i = 0; i < args.Length; ++i)
            {
                string arg = args[i];

                if (arg.StartsWith("-") || arg.StartsWith("/"))
                {
                    string command = arg.Substring(1);

                    if (command.Equals("debug", StringComparison.CurrentCultureIgnoreCase))
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                    else if (command.StartsWith("baseurl=", StringComparison.CurrentCultureIgnoreCase))
                    {
                        program._baseUrl = ParseValue(command);
                    }
                    else if (command.StartsWith("urllist=", StringComparison.CurrentCultureIgnoreCase))
                    {
                        program._urlList = ParseValue(command);
                    }
                    else if (command.StartsWith("iterations=", StringComparison.CurrentCultureIgnoreCase))
                    {
                        program._iterations = int.Parse(ParseValue(command));
                    }
                    else if (command.StartsWith("output=", StringComparison.CurrentCultureIgnoreCase))
                    {
                        program._outputFile = ParseValue(command);
                    }
                    else if (command.StartsWith("tag=", StringComparison.CurrentCultureIgnoreCase))
                    {
                        program._tag = ParseValue(command); 
                    }
                    else
                    {
                        throw new ParseOptionsException("Unrecognized command switch : " + args[i]);
                    }
                }
                else
                {
                    throw new ParseOptionsException("Illegal command line argument : " + args[i]);
                }
            }
            return program;
        }
        private static string ParseValue(string command)
        {
            return command.Substring(command.IndexOf("=") + 1);
        }
        private string UrlCombine(string baseUrl, string relativeUrl)
        {
            baseUrl = VirtualPathUtility.AppendTrailingSlash(baseUrl);

            if (relativeUrl.StartsWith("/"))
            {
                relativeUrl = relativeUrl.Substring(1);
            }

            return baseUrl + relativeUrl;
        }
    }
}
