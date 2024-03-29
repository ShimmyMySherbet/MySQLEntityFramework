﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using ShimmyMySherbet.MySQL.EF.Models;
using ShimmyMySherbet.MySQL.EF.Models.Interfaces;

namespace ShimmyMySherbet.MySQL.EF.Core
{
    /// <summary>
    /// Used to create database Managers/Brides
    /// To use, inherit this class, and add fields/properties of DatabaseTable<T>, or any class that inherits IDatabaseTable
    /// This class will supply each instance with a datasdatabase connection.
    /// Connect() will open the databse connection and CheckSchema() will create any missing database tables
    /// TIP: To provide table specific operations, in another class inherit DatabaseTable<T> and include it in your database manager
    /// </summary>
    /// <seealso cref="DatabaseTable{T}"/>
    public abstract class DatabaseClient
    {
        public MySQLEntityClient Client { get; private set; }
        public bool SingleConnectionMode { get; private set; }
        private bool m_AutoInit = true;
        private Type m_Class => GetType();

        public bool Connected => Client.Connected;

        private bool m_Inited = false;

        /// <summary>
        /// When enabled, numeric auto-increment primary key fields will be updated with the ID assigned to the new row when inserting.
        /// This is still an experimental feature
        /// </summary>
        public bool AutoUpdateInstanceKey
        {
            get => Client.AutoUpdateInstanceKey;
            set => Client.AutoUpdateInstanceKey = value;
        }

        private List<IDatabaseTableInitializer> m_Initializers = new List<IDatabaseTableInitializer>();

        public DatabaseClient(DatabaseSettings settings, bool singleConnectionMode = true, bool autoInit = true)
        {
            Client = new MySQLEntityClient(settings, singleConnectionMode);
            m_AutoInit = autoInit;
            FinaliseConstructor();
        }

        public DatabaseClient(string connectionString, bool singleConnectionMode = true, bool autoInit = true)
        {
            Client = new MySQLEntityClient(connectionString, singleConnectionMode);
            m_AutoInit = autoInit;
            FinaliseConstructor();
        }

        public DatabaseClient(IConnectionProvider connectionProvider, bool autoInit = true)
        {
            Client = new MySQLEntityClient(connectionProvider);
            m_AutoInit = autoInit;
            FinaliseConstructor();
        }

        public DatabaseClient(string address, string username, string password, string database, ushort port = 3306, bool singleConnectionMode = true, bool autoInit = true)
        {
            Client = new MySQLEntityClient(address, username, password, database, port, singleConnectionMode);
            m_AutoInit = autoInit;
            FinaliseConstructor();
        }

        private void FinaliseConstructor()
        {
            if (m_AutoInit)
            {
                Init();
            }
        }

        public void CheckSchema()
        {
            if (!m_Inited)
            {
                throw new InvalidOperationException("Table instances has not been initialized yet. Call Init() or enable auto init.");
            }

            foreach (var init in m_Initializers)
            {
                init.CheckSchema();
            }
        }

        protected void Init()
        {
            if (m_Inited)
            {
                throw new InvalidOperationException("Table instances have already been initialized.");
            }
            m_Inited = true;
            foreach (var field in m_Class.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (typeof(IDatabaseTableInitializer).IsAssignableFrom(field.FieldType))
                {
                    var inst = field.GetValue(this);
                    if (inst != null && inst is IDatabaseTableInitializer init)
                    {
                        m_Initializers.Add(init);
                    }
                }
            }

            foreach (var property in m_Class.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (property.CanRead && typeof(IDatabaseTableInitializer).IsAssignableFrom(property.PropertyType))
                {
                    var inst = property.GetValue(this);
                    if (inst != null && inst is IDatabaseTableInitializer init)
                    {
                        m_Initializers.Add(init);
                    }
                }
            }

            SendClientInstances();
        }

        public bool Connect()
        {
            return Client.Connect();
        }

        public bool Connect(out string errorMessage)
        {
            return Client.Connect(out errorMessage);
        }

        public async Task<bool> ConnectAsync()
        {
            return await Client.ConnectAsync();
        }

        private void SendClientInstances()
        {
            foreach (var init in m_Initializers)
            {
                init.SendClient(Client);
            }
        }
    }
}