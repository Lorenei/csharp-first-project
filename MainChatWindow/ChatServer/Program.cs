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
        private static ConcurrentDictionary<string, string> FirstRunUserDB;

        static void Main(string[] args) {
            string userDBPath = "userDB.txt";

            if(!File.Exists(userDBPath))
            {
                File.Create(userDBPath);
                Console.WriteLine("Created users database file.");
            }

            List<string> tempUsersArray = File.ReadAllLines(userDBPath).ToList<string>();
            FirstRunUserDB = new ConcurrentDictionary<string, string>();
            foreach(string line in tempUsersArray)
            {
                FirstRunUserDB.TryAdd(line.Split(' ').First(), line.Split(' ').Last());
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
