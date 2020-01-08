using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace WhitelistDiscordBot.Services
{
    class DBConnect
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        //Constructor
        public DBConnect()
        {
            Initialize();
        }

        //Initialize values
        private void Initialize()
        {
            server = "127.0.0.1";
            database = "essentialsmode";
            uid = "root";
            password = "orebro123";
            string connectionString = "SERVER=" + server + "; " + "DATABASE=" +
            database + "; " + "UID=" + uid + "; " + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
        }

        //open connection to database
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        //0: Cannot connect to server.
                        break;

                    case 1045:
                        //1045: Invalid user name and/or password.
                        break;
                }
                return false;
            }
        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                
                return false;
            }
        }


        //Whitelisting
        public bool Whitelist(string hexID)
        {
            string query = $"INSERT INTO user_whitelist (identifier, whitelisted) VALUES('steam:{hexID}', '1')";

            //open connection
            if (OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                cmd.ExecuteNonQuery();

                //close connection
                CloseConnection();

                return true;
            }
            return false;
        }

        public bool InWhitelist(string hexID)
        {
            string query = $"SELECT * FROM user_whitelist WHERE identifier='steam:{hexID}'";

            //open connection
            if (OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    dataReader.Close();

                    CloseConnection();

                    return true;
                }
                dataReader.Close();

                CloseConnection();
                return false;
            }
            return false;
        }

        public bool UpdateWhitelist(string hexID, short wl)
        {
            string query = $"UPDATE user_whitelist SET whitelisted='{wl}' WHERE identifier='steam:{hexID}'";

            //open connection
            if (OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                cmd.ExecuteNonQuery();

                //close connection
                CloseConnection();

                return true;
            }
            return false;
        }

        //List Weapons
        private List<Weapon> GetWeapons(string json)
        {
            List<Weapon> weapons = new List<Weapon>();

            JArray jArray = JArray.Parse(json);
            foreach (JObject root in jArray)
            {
                if ((string)root["name"] == "WEAPON_PETROLCAN")
                    continue;

                Weapon w1 = new Weapon
                {
                    Ammo = (int)root["ammo"],
                    Name = (string)root["name"],
                    Label = (string)root["label"]
                };
                //Console.WriteLine(w1.Ammo + ", " + w1.Name + ", " + w1.Label);
                weapons.Add(w1);
            }
            return weapons;
        }


        public List<User> GetUsersWithWeapon(string weapon, string filterJob = "")
        {
            List<User> users = new List<User>();
            string query = $"SELECT identifier, name, loadout, job, firstname, lastname FROM users WHERE loadout LIKE '%{weapon}%'";

            //open connection
            if (OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    if (dataReader["job"].ToString() != filterJob)
                    {
                        User user = new User
                        {
                            SteamName = dataReader["name"].ToString(),
                            SteamID64 = dataReader["identifier"].ToString(),
                            Name = $"{dataReader["firstname"].ToString()} {dataReader["lastname"].ToString()}",
                            Job = dataReader["job"].ToString(),
                            Weapons = GetWeapons(dataReader["loadout"].ToString())
                        };

                        users.Add(user);
                    }
                }
                dataReader.Close();
                CloseConnection();

                return users;
            }

            return users;
        }

        public List<User> GetUsersWithWeapons(string filterJob = "")
        {
            List<User> users = new List<User>();
            string query = $"SELECT identifier, name, loadout, job, firstname, lastname FROM users WHERE loadout != '[]'";

            //open connection
            if (OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    if (dataReader["job"].ToString() != filterJob)
                    {
                        User user = new User
                        {
                            SteamName = dataReader["name"].ToString(),
                            SteamID64 = dataReader["identifier"].ToString(),
                            Name = $"{dataReader["firstname"].ToString()} {dataReader["lastname"].ToString()}",
                            Job = dataReader["job"].ToString(),
                            Weapons = GetWeapons(dataReader["loadout"].ToString())
                        };

                        users.Add(user);
                    }
                }
                dataReader.Close();
                CloseConnection();

                return users;
            }

            return users;
        }
    }

    public struct Weapon
    {
        public int Ammo { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
    }

    public struct User
    {
        public string SteamName { get; set; }
        public string SteamID64 { get; set; }
        public string Name { get; set; }
        public string Job { get; set; }

        public List<Weapon> Weapons { get; set; }
    }
}
