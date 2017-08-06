using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer {
    class Program {

        public static ChatService _server;
        private static ConcurrentDictionary<string, string> FirstRunUserDB; //Holds users logins and passwords from database file.

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
            if (!wasFileCreated)
            {
                try
                {
                    List<string> tempUsersArray = File.ReadAllLines(userDBPath).ToList<string>();
                    foreach (string line in tempUsersArray)
                    {
                        FirstRunUserDB.TryAdd(line.Split(' ').First(), line.Split(' ').Last());
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine("Error while reading database file and adding users to dictionary. Application exit.");
                }
            }

            _server = new ChatService(FirstRunUserDB, userDBPath);

            using(ServiceHost host = new ServiceHost(_server)) {
                host.Open();
                Console.WriteLine("Server is running...");
                Console.ReadLine();
            }
        }
    }
}
