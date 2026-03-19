using System;
namespace SecurityDashboard.Models {
    public class User {
        static int _id = 1;

        public int ID { get; init; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }

        public User() {
            ID = _id++;
            IsAdmin = false;
        }

        public User(string username,
                    string password,
                    bool isAdmin = false) : this() {
            Username = username;
            Password = password;
            IsAdmin = isAdmin;
        }
    }
}

