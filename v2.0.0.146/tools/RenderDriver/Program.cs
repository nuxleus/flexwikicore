using System;
using System.Diagnostics; 
using System.IO;
using System.Xml;
using System.Xml.Serialization; 

namespace FlexWiki.Tools.RenderDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            string configPath = Path.Combine(Environment.CurrentDirectory, args[0]);
            string topic = args[1]; 
            int iterations = 1;
            if (args.Length > 2)
            {
                iterations = int.Parse(args[2]);
            }
            
            FederationConfiguration federationConfiguration = LoadFederationConfiguration(configPath);
            Console.WriteLine("Loaded configuration"); 

            RenderDriverApplication application = new RenderDriverApplication(federationConfiguration, 
                new LinkMaker("http://localhost/wiki")); 

            Federation federation = new Federation(application);

            Console.WriteLine("Created federation");

            Console.WriteLine("Rendering content...");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); 
            string content = federation.GetTopicFormattedContent(new QualifiedTopicRevision(topic), null);
            stopwatch.Stop(); 

            Console.WriteLine("Rendered {0} bytes in {1} seconds", content.Length, stopwatch.ElapsedMilliseconds / 1000.0F);

            Console.Write("Hit <enter> to begin secondary iterations.");
            Console.ReadLine();

            stopwatch.Reset();

            stopwatch.Start();
            for (int i = 0; i < iterations; ++i)
            {
                content = federation.GetTopicFormattedContent(new QualifiedTopicRevision(topic), null);
            }
            stopwatch.Stop();

            Console.WriteLine("Rendered {0} times in {1} seconds for an average of {2}", iterations, stopwatch.ElapsedMilliseconds / 1000.0F, 
                stopwatch.ElapsedMilliseconds / 1000.0F / (float) iterations);
            Console.WriteLine("Hit <enter> to exit"); 
            Console.ReadLine(); 
        }

        private static FederationConfiguration LoadFederationConfiguration(string configPath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(RenderDriverConfiguration));

            RenderDriverConfiguration configuration; 

            XmlReader xmlReader = XmlReader.Create(configPath);
            try
            {
                configuration = (RenderDriverConfiguration) serializer.Deserialize(xmlReader);
            }
            finally
            {
                xmlReader.Close(); 
            }

            return configuration.FederationConfiguration; 
        }
    }
}
