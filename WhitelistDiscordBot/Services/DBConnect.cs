using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.IO;

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
    }
}
