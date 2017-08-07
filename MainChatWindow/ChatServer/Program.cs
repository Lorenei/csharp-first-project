using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer {
    /// <summary>
    /// Main console application of WCF service.
    /// </summary>
    class Program {

        /// <summary>
        /// The server
        /// </summary>
        public static ChatService _server;
        /// <summary>
        /// Holds users logins and passwords from database file.
        /// </summary>
        private static ConcurrentDictionary<string, string> FirstRunUserDB;

        /// <summary>
        /// Mains the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args) {
            string userDBPath = "userDB.txt";
            bool wasFileCreated = false;

            //Checks if database file exists already. Creates it if not.
            if(!File.Exists(userDBPath))
            {
                File.Create(userDBPath);
                Console.WriteLine("Created users database file.");
                wasFileCreated = true;
            }
            FirstRunUserDB = new ConcurrentDictionary<string, string>();
            //Read from file to dictionary currently registered users only if file wasn't just created with this app run, since it's useless because file is empty.
            //Used this mainly to avoid File.IO exceptions, that showed up if file HAD to be created for first time, which was blocking file after creation thus making it impossible to read from empty file.
            //But still can be used as a feature instead of a workaround right :)
            if (!wasFileCreated)
            {
                //Read users logins and passwords from file to dictionary
                try
                {
                    List<string> tempUsersArray = File.ReadAllLines(userDBPath).ToList<string>();
                    foreach (string line in tempUsersArray)
                    {
                        FirstRunUserDB.TryAdd(line.Split(' ').First(), line.Split(' ').Last());
                    }
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception err)
#pragma warning restore CS0168 // Variable is declared but never used
                {
                    Console.WriteLine("Error while reading database file and adding users to dictionary. Application exit.");
                }
            }

            _server = new ChatService(FirstRunUserDB, userDBPath);

            //Start the server!
            using(ServiceHost host = new ServiceHost(_server)) {
                host.Open();
                Console.WriteLine("Server is running...");
                Console.ReadLine();
            }
        }
    }
}
