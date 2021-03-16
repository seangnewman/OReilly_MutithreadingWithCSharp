using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

using static System.Text.Encoding;


namespace Chapter9_UsingAsynchronousIO
{
    class Examples
    {
        const int BUFFER_SIZE = 4096;

        private static async Task ProcessAsynchronousIO()
        {
            using (var stream = new FileStream("test1.txt", FileMode.Create, FileAccess.ReadWrite, FileShare.None, BUFFER_SIZE))
            {
                Console.WriteLine($"1. Uses I/O Threads: {stream.IsAsync}");

                byte[] buffer = UTF8.GetBytes(CreateFileContent());

                var writeTask = Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, buffer, 0, buffer.Length, null);
                await writeTask;
            }
            using (var stream = new FileStream("test2.txt", FileMode.Create, FileAccess.ReadWrite, FileShare.None, BUFFER_SIZE, FileOptions.Asynchronous))
            {
                Console.WriteLine($"2. Uses I/O Threads: {stream.IsAsync}");

                byte[] buffer = UTF8.GetBytes(CreateFileContent());
                var writeTask = Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, buffer, 0, buffer.Length, null);
                await writeTask;
            }

            using (var stream = File.Create("test3.txt", BUFFER_SIZE, FileOptions.Asynchronous))
                using(var sw = new StreamWriter(stream))
                {
                    Console.WriteLine($"3. Uses I/O Threads: {stream.IsAsync}");
                    await sw.WriteAsync(CreateFileContent());
                }

            using (var sw = new StreamWriter("test4.txt", true))
            {
                Console.WriteLine($"4. Uses I/O Threads: { ((FileStream)sw.BaseStream).IsAsync}");
                await sw.WriteAsync(CreateFileContent());
            }

            Console.WriteLine("Starting parsing files in parallel");

            var readTasks = new Task<long>[4];
            for (int i = 0; i < 4; i++)
            {
                string fileName = $"test{i + 1}.txt";
                readTasks[i] = SumFileContent(fileName);
            }

            long[] sums = await Task.WhenAll(readTasks);

            Console.WriteLine($"Sum in all files: {sums.Sum()}");

            Console.WriteLine("Deleting files....");

            Task[] deleteTasks = new Task[4];

            for (int i = 0; i < 4; i++)
            {
                string fileName = $"test[i + 1].txt";
                deleteTasks[i] = SimulateAsynchronousDelete(fileName);
            }

            await Task.WhenAll(deleteTasks);
            Console.WriteLine("Deleting complete.");


        }

        private static async Task ProcessAsynchronousIO(string dbName)
        {
            try
            {
                const string connectionString = @"Data source = (LocalDB)\MSSQLLocalDB; Initial Catalog=master; Integrated Security = True";
                string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                string dbFileName = Path.Combine(outputFolder, $"{dbName}.mdf");
                string dbLogFileName = Path.Combine(outputFolder, $"{dbName}_log.ldf");

                string dbConnectionString = $@"Data source = (LocalDB)\MSSQLLocalDB; AttachDBFileName={dbFileName}; Integrated Security = True";

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    if (File.Exists(dbFileName))
                    {
                        Console.WriteLine("Detaching the database....");

                        var detachCommand = new SqlCommand("sp_detach_db", connection);
                        detachCommand.CommandType = CommandType.StoredProcedure;
                        detachCommand.Parameters.AddWithValue("@dbname", dbName);

                        await detachCommand.ExecuteNonQueryAsync();

                        Console.WriteLine("The database was detached successfully.");
                        Console.WriteLine("Deleting the database....");

                        if (File.Exists(dbLogFileName))
                        {
                            File.Delete(dbLogFileName);
                        }
                        File.Delete(dbFileName);

                        Console.WriteLine("The database was deleted successfully. ");

                    }
                    Console.WriteLine("Creating the database");
                    string createComment = $"CREATE DATABASE {dbName} ON (NAME = N'{dbName}', FILENAME = '{dbFileName}')";
                    var cmd = new SqlCommand(createComment, connection);

                    await cmd.ExecuteNonQueryAsync();
                    Console.WriteLine("The database was created successfully");
                }

                using (var connection = new SqlConnection(dbConnectionString))
                {
                    await connection.OpenAsync();

                    var cmd = new SqlCommand("SELECT newid()", connection);
                    var result = await cmd.ExecuteScalarAsync();
                    Console.WriteLine($"New GUID from DataBase: {result}");

                    cmd = new SqlCommand(@"CREATE TABLE [dbo].[CustomTable]( [ID] [int] IDENTITY(1,1) NOT NULL, [Name] [nvarchar](50) NOT NULL, 
                                                                        CONSTRAINT [PK_ID] PRIMARY KEY CLUSTERED  ([ID] ASC) ON [PRIMARY]) ON [PRIMARY]", connection);

                    await cmd.ExecuteNonQueryAsync();

                    Console.WriteLine("Table was created successfully");

                    cmd = new SqlCommand(@"INSERT INTO [dbo].[CustomTable] (Name) VALUES ('John');
                                                                    INSERT INTO [dbo].[CustomTable] (Name) VALUES ('Peter');
                                                                    INSERT INTO [dbo].[CustomTable] (Name) VALUES ('James');
                                                                    INSERT INTO [dbo].[CustomTable] (Name) VALUES ('Eugene');", connection);
                    await cmd.ExecuteNonQueryAsync();

                    Console.WriteLine("Inserted data successfully");
                    Console.WriteLine("Reading data from table....");

                    cmd = new SqlCommand(@"SELECT * FROM [dbo].[CustomTable]", connection);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var id = reader.GetFieldValue<int>(0);
                            var name = reader.GetFieldValue<string>(1);
                            Console.WriteLine($"Table row: Id {id}, Name {name}");
                        }
                    }
                        

                }



            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static Task SimulateAsynchronousDelete(string fileName)
        {

            // There is no asynchronous delete method, so we simulate by wrapping in a task
            return Task.Run( () => File.Delete(fileName));
        }

        private static async Task<long> SumFileContent(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, BUFFER_SIZE, FileOptions.Asynchronous))
            using (var sr = new StreamReader(stream))
            {
                long sum = 0;
                while (sr.Peek() > 1)
                {
                    string line = await sr.ReadLineAsync();
                    sum += long.Parse(line);
                }
                return sum;
            }
        }

        private static string CreateFileContent()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 100000; i++)
            {
                sb.Append($"{new Random(i).Next(0, 9999)}");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        static async Task GetResponseAsync(string url)
        {
            using (var client = new HttpClient())
            {
                HttpResponseMessage responseMessage = await client.GetAsync(url);
                string responseHeaders = responseMessage.Headers.ToString();
                string response = await responseMessage.Content.ReadAsStringAsync();

                Console.WriteLine("Respone headers");
                Console.WriteLine(responseHeaders);
                Console.WriteLine("Response body");
                Console.WriteLine(response);
            }
        }

        static async Task RunServiceClient()
        {
            var endpoint = new EndpointAddress(SERVICE_URL);
            var channel = ChannelFactory<IHelloWorldServiceClient>
                  .CreateChannel(new BasicHttpBinding(), endpoint);

            var greeting = await channel.GreetAsync("Eugene");
            Console.WriteLine(greeting);
        }

        // ******************************************************
        public void WorkingWithFilesAsynchronously()
        {
            var t = ProcessAsynchronousIO();
            t.GetAwaiter().GetResult();
        }

        public void WritingAsynchronousHTTPServerClient()
        {
            var server = new AsyncHttpServer(1234);
            var t = Task.Run( () => server.Start());
            Console.WriteLine("Listening on port 1234.  Open http://localhost:1234 in your browser");
            Console.WriteLine("Trying to connect");
            Console.WriteLine();

            GetResponseAsync("http://localhost:1234").GetAwaiter().GetResult();

            Console.WriteLine();
            Console.WriteLine("Press Enter to stop the server.");
            Console.ReadLine();

            server.Stop().GetAwaiter().GetResult();

        }

        public void WorkingWithDatabaseAsynchronously()
        {
            const string dataBaseName = "CustomDatabase";
            var t = ProcessAsynchronousIO(dataBaseName);
            t.GetAwaiter().GetResult();
            Console.WriteLine("Press Enter to Exit");
            Console.ReadLine();
        }


        const string SERVICE_URL = "http://localhost:1234/HelloWorld";
        public void CallingAWCFServiceAsynchronously()
        {
            ServiceHost host = null;

            try
            {
                host = new ServiceHost(typeof(HelloWorldService), new Uri(SERVICE_URL));

                var metadata = host.Description.Behaviors.Find<ServiceMetadataBehavior>() ?? new ServiceMetadataBehavior();
                metadata.HttpGetEnabled = true;
                metadata.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;

                host.Description.Behaviors.Add(metadata);
                host.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexHttpBinding(), "mex");

                var endpoint = host.AddServiceEndpoint(typeof(IHelloWorldService), new BasicHttpBinding(), SERVICE_URL);

                host.Faulted += (sender, e) => Console.WriteLine("Error!");

                host.Open();

                Console.WriteLine("Greetings!  service is running and listening on:");
                Console.WriteLine($"{endpoint.Address} {endpoint.Binding.Name}");

                var client = RunServiceClient();
                client.GetAwaiter().GetResult();

                Console.WriteLine("Press Enter to exit");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in catch block: {ex}");
            }
            finally
            {
                if(null != host)
                {
                    if(host.State == CommunicationState.Faulted)
                    {
                        host.Abort();
                    }
                    else
                    {
                        host.Close();
                    }
                }
            }

        }

       
    }
}
