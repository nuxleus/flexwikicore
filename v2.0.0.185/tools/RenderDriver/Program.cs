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

            Warmup(topic, federation);
            Run(topic, iterations, federation);
            RunWithContext(topic, iterations, federation); 
        }

        private static FederationConfiguration LoadFederationConfiguration(string configPath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(RenderDriverConfiguration));

            RenderDriverConfiguration configuration;

            XmlReader xmlReader = XmlReader.Create(configPath);
            try
            {
                configuration = (RenderDriverConfiguration)serializer.Deserialize(xmlReader);
            }
            finally
            {
                xmlReader.Close();
            }

            return configuration.FederationConfiguration;
        }
        private static void Run(string topic, int iterations, Federation federation)
        {
            Stopwatch stopwatch = new Stopwatch(); 
            stopwatch.Start();
            for (int i = 0; i < iterations; ++i)
            {
                string content = federation.GetTopicFormattedContent(new QualifiedTopicRevision(topic), null);
            }
            stopwatch.Stop();

            Console.WriteLine("Rendered {0} times in {1} seconds for an average of {2}", 
                iterations, stopwatch.ElapsedMilliseconds / 1000.0F,
                stopwatch.ElapsedMilliseconds / 1000.0F / (float)iterations);
        }
        private static void RunWithContext(string topic, int iterations, Federation federation)
        {
            using (RequestContext.Create())
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                string content = federation.GetTopicFormattedContent(new QualifiedTopicRevision(topic), null);
                stopwatch.Stop();

                Console.WriteLine("Rendered first times in {0} seconds", stopwatch.ElapsedMilliseconds / 1000.0F);

                stopwatch.Reset(); 
                stopwatch.Start();
                content = federation.GetTopicFormattedContent(new QualifiedTopicRevision(topic), null);
                stopwatch.Stop();

                Console.WriteLine("Rendered second time in {0} seconds", stopwatch.ElapsedMilliseconds / 1000.0F);

                stopwatch.Reset();
                stopwatch.Start();
                content = federation.GetTopicFormattedContent(new QualifiedTopicRevision(topic), null);
                stopwatch.Stop();

                Console.WriteLine("Rendered third time in {0} seconds", stopwatch.ElapsedMilliseconds / 1000.0F);
            }
        }
        private static void Warmup(string topic, Federation federation)
        {
            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine("Rendering warmup content...");
    
            stopwatch = new Stopwatch();
            stopwatch.Start();
            string content = federation.GetTopicFormattedContent(new QualifiedTopicRevision(topic), null);
            stopwatch.Stop();

            Console.WriteLine("Rendered {0} bytes in {1} seconds", content.Length, stopwatch.ElapsedMilliseconds / 1000.0F);

        }

    }
}
