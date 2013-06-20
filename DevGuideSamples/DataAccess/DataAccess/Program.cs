//===============================================================================
// Microsoft patterns & practices
// Enterprise Library 6 Samples
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.ComponentModel;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Xml;
using System.Transactions;

using DevGuideExample.MenuSystem;

// references to application block namespace(s) for these examples
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Threading.Tasks;

namespace DataAccessExample
{
    class Program
    {
        static Database defaultDB = null;
        static SqlDatabase sqlServerDB = null;
        static Database asyncDB = null;

        static void Main(string[] args)
        {
            #region Create the required objects

            // Configure the DatabaseFactory to read its configuration from the config file
            DatabaseProviderFactory factory = new DatabaseProviderFactory();

            // Create a Database object from the factory using the default database defined in the configuration file:
            // <dataConfiguration defaultDatabase="ExampleDatabase" />
            defaultDB = factory.CreateDefault();

            // Or create a Database object from the factory using the connection string name.
            // defaultDB = factory.Create("ExampleDatabase");

            // Create a SqlDatabase object from the factory using the default database.
            sqlServerDB = factory.Create("ExampleDatabase") as SqlDatabase;

            // Crate a Database object that has "Asynchronous Processing=true" in the connection string.
            asyncDB = factory.Create("AsyncExampleDatabase");

            // An alternative approach to building the database objects using the
            // static facade is:
            //DatabaseFactory.SetDatabaseProviderFactory(factory, false);
            //defaultDB = DatabaseFactory.CreateDatabase("ExampleDatabase");
            //sqlServerDB = DatabaseFactory.CreateDatabase() as SqlDatabase;
            //asyncDB = DatabaseFactory.CreateDatabase("AsyncExampleDatabase");

            #endregion

            new MenuDrivenApplication("Data Access Block Developer's Guide Examples",
                ReadSQLStatement,
                ReadStoredProcWithParams,
                ReadSQLOrSprocWithNamedParams,
                ReadDataAsObjects,
                ReadDataAsXML,
                ReadScalarValue,
                ReadDataAsynchronously,
                ReadDataAsynchronouslyTask,
                ReadObjectsAsynchronously,
                UpdateWithCommand,
                FillAndUpdateDataset,
                LoadAndUpdateTypedDataset,
                UseConnectionTransaction,
                UseTransactionScope).Run();
        }

        [Description("Return rows using a SQL statement with no parameters")]
        static void ReadSQLStatement()
        {
            // Call the ExecuteReader method by specifying the command type
            // as a SQL statement, and passing in the SQL statement
          using (IDataReader reader = defaultDB.ExecuteReader(CommandType.Text, "SELECT TOP 1 * FROM OrderList"))
            {
                DisplayRowValues(reader);
            }
        }
        [Description("Return rows using a stored procedure with parameters")]
        static void ReadStoredProcWithParams()
        {
            // Call the ExecuteReader method with the stored procedure
            // name and an Object array containing the parameter values
            using (IDataReader reader = defaultDB.ExecuteReader("ListOrdersByState", new object[] { "Colorado" }))
            {
                DisplayRowValues(reader);
            }
        }

        [Description("Return rows using a SQL statement or stored procedure with named parameters")]
        static void ReadSQLOrSprocWithNamedParams()
        {
            // Read data with a SQL statement that accepts one parameter
            string sqlStatement = "SELECT TOP 1 * FROM OrderList WHERE State LIKE @state";
            // Create a suitable command type and add the required parameter
            using (DbCommand sqlCmd = defaultDB.GetSqlStringCommand(sqlStatement))
            {
                defaultDB.AddInParameter(sqlCmd, "state", DbType.String, "New York");
                // Call the ExecuteReader method with the command
                using (IDataReader sqlReader = defaultDB.ExecuteReader(sqlCmd))
                {
                    Console.WriteLine("Results from executing SQL statement:");
                    DisplayRowValues(sqlReader);
                }
            }
            // Read data with a stored procedure that accepts one parameter
            string storedProcName = "ListOrdersByState";
            // Create a suitable command type and add the required parameter
            using (DbCommand sprocCmd = defaultDB.GetStoredProcCommand(storedProcName))
            {
                defaultDB.AddInParameter(sprocCmd, "state", DbType.String, "New York");
                // Call the ExecuteReader method with the command
                using (IDataReader sprocReader = defaultDB.ExecuteReader(sprocCmd))
                {
                    Console.WriteLine("Results from executing stored procedure:");
                    DisplayRowValues(sprocReader);
                }
            }
        }

        [Description("Return data as a sequence of objects using a stored procedure")]
        static void ReadDataAsObjects()
        {
            // Create an object array and populate it with the required parameter values
            object[] paramArray = new object[] { "%bike%" };
            // Create and execute a sproc accessor that uses default parameter and output mappings
            var productData = defaultDB.ExecuteSprocAccessor<Product>("GetProductList", paramArray);
            // Perform a client-side query on the returned data
            // Be aware that the orderby and filtering is happening on the client, not the database
            var results = from productItem in productData
                          where productItem.Description != null
                          orderby productItem.Name
                          select new { productItem.Name, productItem.Description };
            // Display the results
            foreach (var item in results)
            {
                Console.WriteLine("Product Name: {0}", item.Name);
                Console.WriteLine("Description: {0}", item.Description);
                Console.WriteLine();
            }
        }

        [Description("Return data as an XML fragment using a SQL Server XML query")]
        static void ReadDataAsXML()
        {
            // Specify a SQL query that returns XML data
            string xmlQuery = "SELECT * FROM OrderList WHERE State = @state FOR XML AUTO";
            // Create a suitable command type and add the required parameter
            // NB: ExecuteXmlReader is only available for SQL Server databases
            using (DbCommand xmlCmd = sqlServerDB.GetSqlStringCommand(xmlQuery))
            {
                xmlCmd.Parameters.Add(new SqlParameter("state", "Colorado"));
                using (XmlReader reader = sqlServerDB.ExecuteXmlReader(xmlCmd))
                {
                    // Iterate through the elements in the XmlReader
                    while (!reader.EOF)
                    {
                        if (reader.IsStartElement())
                        {
                            Console.WriteLine(reader.ReadOuterXml());
                        }
                    }
                }
            }
        }

        [Description("Return a single scalar value from a SQL statement or stored procedure")]
        static void ReadScalarValue()
        {
            // Create a suitable command type for a SQL statement
            using (DbCommand sqlCmd = defaultDB.GetSqlStringCommand("SELECT [Name] FROM States"))
            {
                // Call the ExecuteScalar method of the command
                Console.WriteLine("Result using a SQL statement: {0}",
                                   defaultDB.ExecuteScalar(sqlCmd).ToString());
            }
            // Create a suitable command type for a stored procedure
            using (DbCommand sprocCmd = defaultDB.GetStoredProcCommand("GetStatesList"))
            {
                // Call the ExecuteScalar method of the command
                Console.WriteLine("Result using a stored procedure: {0}",
                                   defaultDB.ExecuteScalar(sprocCmd).ToString());
            }
        }

        [Description("Execute a command that retrieves data asynchronously")]
        static void ReadDataAsynchronously()
        {
            if (!SupportsAsync(asyncDB)) return;

            using (var doneWaitingEvent = new ManualResetEvent(false))
            using (var readCompleteEvent = new ManualResetEvent(false))
            {
                try
                {
                    // Create command to execute stored procedure and add parameters
                    DbCommand cmd = asyncDB.GetStoredProcCommand("ListOrdersSlowly");
                    asyncDB.AddInParameter(cmd, "state", DbType.String, "Colorado");
                    asyncDB.AddInParameter(cmd, "status", DbType.String, "DRAFT");
                    // Execute the query asynchronously specifying the command and the
                    // expression to execute when the data access process completes.
                    asyncDB.BeginExecuteReader(cmd,
                        asyncResult =>
                        {
                            // Lambda expression executed when the data access completes.
                            doneWaitingEvent.Set();
                            try
                            {
                                using (IDataReader reader = asyncDB.EndExecuteReader(asyncResult))
                                {
                                    Console.WriteLine();
                                    Console.WriteLine();
                                    DisplayRowValues(reader);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error after data access completed: {0}", ex.Message);
                            }
                            finally
                            {
                                readCompleteEvent.Set();
                            }
                        }, null);

                    // Display waiting messages to indicate executing asynchronouly
                    while (!doneWaitingEvent.WaitOne(1000))
                    {
                        Console.Write("Waiting... ");
                    }

                    // Allow async thread to write results before displaying "continue" prompt
                    readCompleteEvent.WaitOne();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while starting data access: {0}", ex.Message);
                }
            }
        }

        [Description("Execute a command that retrieves data asynchronously using a Task")]
        static void ReadDataAsynchronouslyTask()
        {
          if (!SupportsAsync(asyncDB)) return;
            DoReadDataAsynchronouslyTask().Wait();
        }

        private static async Task DoReadDataAsynchronouslyTask()
        {
          try
          {
            // Create command to execute stored procedure and add parameters
            DbCommand cmd = asyncDB.GetStoredProcCommand("ListOrdersSlowly");
            asyncDB.AddInParameter(cmd, "state", DbType.String, "Colorado");
            asyncDB.AddInParameter(cmd, "status", DbType.String, "DRAFT");

            using (var timer = new Timer(_ => Console.Write("Waiting... ")))
            {
              timer.Change(0, 1000);

              using (var reader = await Task<IDataReader>.Factory.FromAsync<DbCommand>(asyncDB.BeginExecuteReader, asyncDB.EndExecuteReader, cmd, null))
              {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                Console.WriteLine();
                Console.WriteLine();
                DisplayRowValues(reader);
              }
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("Error while starting data access: {0}", ex.Message);
          }
        }


        [Description("Execute a command that retrieves data as objects asynchronously")]
        static void ReadObjectsAsynchronously()
        {
            if (!SupportsAsync(asyncDB)) return;

            using (var doneWaitingEvent = new ManualResetEvent(false))
            using (var readCompleteEvent = new ManualResetEvent(false))
            {
                try
                {
                    // Create an object array and populate it with the required parameter values.
                    object[] paramArray = new object[] { "%bike%", 20 };

                    // Create the accessor. This example uses the simplest overload.
                    var accessor = asyncDB.CreateSprocAccessor<Product>("GetProductsSlowly");

                    // Execute the accessor asynchronously specifying the callback expression,
                    // the existing accessor as the AsyncState, and the parameter values array.
                    accessor.BeginExecute(
                        asyncResult =>
                        {
                            // Lambda expression executed when the data access completes.
                            doneWaitingEvent.Set();
                            try
                            {
                                // Accessor is available via the asyncResult parameter
                                var acc = (DataAccessor<Product>)asyncResult.AsyncState;

                                // Obtain the results from the accessor.
                                var productData = acc.EndExecute(asyncResult);

                                // Perform a client-side query on the returned data.
                                // Be aware that the orderby and filtering is happening 
                                // on the client, not inside the database.
                                var results = from productItem in productData
                                              where productItem.Description != null
                                              orderby productItem.Name
                                              select new { productItem.Name, productItem.Description };

                                // Display the results
                                Console.WriteLine();
                                Console.WriteLine();
                                foreach (var item in results)
                                {
                                    Console.WriteLine("Product Name: {0}", item.Name);
                                    Console.WriteLine("Description: {0}", item.Description);
                                    Console.WriteLine();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error after data access completed: {0}", ex.Message);
                            }
                            finally
                            {
                                readCompleteEvent.Set();
                            }
                        }, accessor, paramArray);

                    // Display waiting messages to indicate executing asynchronously.
                    while (!doneWaitingEvent.WaitOne(1000))
                    {
                        Console.Write("Waiting... ");
                    }
                    // Allow async thread to write results before displaying "continue" prompt.
                    readCompleteEvent.WaitOne();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while starting data access: {0}", ex.Message);
                }
            }
        }

        [Description("Update data using a Command object")]
        static void UpdateWithCommand()
        {
            string oldDescription = "Carries 4 bikes securely; steel construction, fits 2\" receiver hitch.";
            string newDescription = "Bikes tend to fall off after a few miles.";
            Console.WriteLine("Contents of row before update:");
            DisplayRowValues(defaultDB.ExecuteReader(CommandType.Text, "SELECT * FROM Products WHERE [Id] = 84"));
            // Create command to execute stored procedure and add parameters
            DbCommand cmd = defaultDB.GetStoredProcCommand("UpdateProductsTable");
            defaultDB.AddInParameter(cmd, "productID", DbType.Int32, 84);
            defaultDB.AddInParameter(cmd, "description", DbType.String, newDescription);
            // Execute query and check if one row was updated
            if (defaultDB.ExecuteNonQuery(cmd) == 1)
            {
                Console.WriteLine("Contents of row after first update:");
                DisplayRowValues(defaultDB.ExecuteReader(CommandType.Text, "SELECT * FROM Products WHERE [Id] = 84"));
            }
            else
            {
                Console.WriteLine("ERROR: Could not update just one row.");
            }
            // Change the value of the second parameter
            defaultDB.SetParameterValue(cmd, "description", oldDescription);
            // Execute query and check if one row was updated
            if (defaultDB.ExecuteNonQuery(cmd) == 1)
            {
                Console.WriteLine("Contents of row after second update:");
                DisplayRowValues(defaultDB.ExecuteReader(CommandType.Text, "SELECT * FROM Products WHERE [Id] = 84"));
            }
            else
            {
                Console.WriteLine("ERROR: Could not update just one row.");
            }
        }

        [Description("Fill a DataSet and update the source data")]
        static void FillAndUpdateDataset()
        {
            string selectSQL = "SELECT Id, Name, Description FROM Products WHERE Id > 90";
            string addSQL = "INSERT INTO Products (Name, Description) VALUES (@name, @description);";
            string updateSQL = "UPDATE Products SET Name = @name, Description = @description WHERE Id = @id";
            string deleteSQL = "DELETE FROM Products WHERE Id = @id";

            // Fill a DataSet from the Products table using the simple approach
            DataSet simpleDS = defaultDB.ExecuteDataSet(CommandType.Text, selectSQL);
            DisplayTableNames(simpleDS, "ExecuteDataSet");
            simpleDS = null;
            // Fill a DataSet from the Products table using the LoadDataSet method
            // This allows you to specify the name(s) for the table(s) in the DataSet
            DataSet loadedDS = new DataSet("ProductsDataSet");
            defaultDB.LoadDataSet(CommandType.Text, selectSQL, loadedDS, new string[] { "Products" });
            DisplayTableNames(loadedDS, "LoadDataSet");
            // Update some data in the rows of the DataSet table
            DataTable dt = loadedDS.Tables["Products"];
            dt.Rows[0].Delete();
            object[] rowData = new object[] { -1, "A New Row", "Added to the table at " + DateTime.Now.ToShortTimeString() };
            dt.Rows.Add(rowData);
            rowData = dt.Rows[1].ItemArray;
            rowData[2] = "A new description at " + DateTime.Now.ToShortTimeString();
            dt.Rows[1].ItemArray = rowData;
            DisplayRowValues(dt);
            // Create the commands to update the original table in the database
            DbCommand insertCommand = defaultDB.GetSqlStringCommand(addSQL);
            defaultDB.AddInParameter(insertCommand, "name", DbType.String, "Name", DataRowVersion.Current);
            defaultDB.AddInParameter(insertCommand, "description", DbType.String, "Description", DataRowVersion.Current);
            DbCommand updateCommand = defaultDB.GetSqlStringCommand(updateSQL);
            defaultDB.AddInParameter(updateCommand, "name", DbType.String, "Name", DataRowVersion.Current);
            defaultDB.AddInParameter(updateCommand, "description", DbType.String, "Description", DataRowVersion.Current);
            defaultDB.AddInParameter(updateCommand, "id", DbType.String, "Id", DataRowVersion.Original);
            DbCommand deleteCommand = defaultDB.GetSqlStringCommand(deleteSQL);
            defaultDB.AddInParameter(deleteCommand, "id", DbType.Int32, "Id", DataRowVersion.Original);
            // Apply the updates in the DataSet to the original table in the database
            int rowsAffected = defaultDB.UpdateDataSet(loadedDS, "Products",
                               insertCommand, updateCommand, deleteCommand,
                               UpdateBehavior.Standard);
            Console.WriteLine("Updated a total of {0} rows in the database.", rowsAffected);
        }

        [Description("Load a Typed DataSet and update the source data")]
        static void LoadAndUpdateTypedDataset()
        {
            string selectSQL = "SELECT Id, Name, Description FROM Products WHERE Id > 90";
            string addSQL = "INSERT INTO Products (Name, Description) VALUES (@name, @description);";
            string updateSQL = "UPDATE Products SET Name = @name, Description = @description WHERE Id = @id";
            string deleteSQL = "DELETE FROM Products WHERE Id = @id";

          ProductsDataSet prodDataSet = new ProductsDataSet();

          // Fill a DataSet from the Products table using the LoadDataSet method
          // This allows you to specify the name(s) for the table(s) in the DataSet
          //
          // Alternatively, use the ProductsTableAdapter class and the Fill method.
          // ProductsDataSetTableAdapters.ProductsTableAdapter adapter = new ProductsDataSetTableAdapters.ProductsTableAdapter();
          // adapter.Fill(prodDataSet.Products);
          defaultDB.LoadDataSet(CommandType.Text, selectSQL, prodDataSet, new string[] { "Products" });

          DisplayTableNames(prodDataSet, "LoadDataSet");
          Console.WriteLine("Loaded ProductsDataSet");
          DisplayRowValues(prodDataSet.Products);
          // Update some data in the rows of the DataSet table
          prodDataSet.Products.AddProductsRow("A New Row", "Added to the table at " + DateTime.Now.ToShortTimeString());
          prodDataSet.Products[2].Description = "A new description at " + DateTime.Now.ToShortTimeString();
          prodDataSet.Products.AddProductsRow("Another New Row", "Added to the table at " + DateTime.Now.ToShortTimeString());
          prodDataSet.Products[prodDataSet.Products.Count-1].Delete();
          Console.WriteLine("Updated ProductsDataSet");
          DisplayRowValues(prodDataSet.Products);

          
          
          // Create the commands to update the original table in the database
          //
          // Alternatively, use the ProductsTableAdapter class and the Fill method.
          // ProductsDataSetTableAdapters.ProductsTableAdapter adapter = new ProductsDataSetTableAdapters.ProductsTableAdapter();
          // adapter.Update(prodDataSet.Products);
          DbCommand insertCommand = defaultDB.GetSqlStringCommand(addSQL);
          defaultDB.AddInParameter(insertCommand, "name", DbType.String, "Name", DataRowVersion.Current);
          defaultDB.AddInParameter(insertCommand, "description", DbType.String, "Description", DataRowVersion.Current);
          DbCommand updateCommand = defaultDB.GetSqlStringCommand(updateSQL);
          defaultDB.AddInParameter(updateCommand, "name", DbType.String, "Name", DataRowVersion.Current);
          defaultDB.AddInParameter(updateCommand, "description", DbType.String, "Description", DataRowVersion.Current);
          defaultDB.AddInParameter(updateCommand, "id", DbType.String, "Id", DataRowVersion.Original);
          DbCommand deleteCommand = defaultDB.GetSqlStringCommand(deleteSQL);
          defaultDB.AddInParameter(deleteCommand, "id", DbType.Int32, "Id", DataRowVersion.Original);
          // Apply the updates in the DataSet to the original table in the database
          int rowsAffected = defaultDB.UpdateDataSet(prodDataSet, "Products",
                             insertCommand, updateCommand, deleteCommand,
                             UpdateBehavior.Standard);
          Console.WriteLine("Written ProductsDataSet Changes to Database");
          DisplayRowValues(prodDataSet.Products);


          Console.WriteLine("Updated a total of {0} rows in the database.", rowsAffected);
        }

        [Description("Use a connection-based transaction")]
        static void UseConnectionTransaction()
        {
            Console.WriteLine("Contents of rows before update:");
            Console.WriteLine();
            DisplayRowValues(defaultDB.ExecuteReader(CommandType.Text, "SELECT * FROM Products WHERE [Id] = 53"));
            DisplayRowValues(defaultDB.ExecuteReader(CommandType.Text, "SELECT * FROM Products WHERE [Id] = 84"));
            Console.WriteLine(MenuDrivenApplication.Underline);
            string newRow53Value = "Third and little fingers tend to get cold.";
            string newRow84Value = "Bikes tend to fall off after a few miles.";
            // Must specifically create the connection for the command
            // to be able to create a transaction for the operations.
            using (DbConnection con = defaultDB.CreateConnection())
            {
                // By default, data access methods will create and manage the connection.
                // When the connection is created manually, it must be managed in the code.
                con.Open();
                // Create a transaction on the open connection for the operations
                using (DbTransaction trans = con.BeginTransaction())
                {
                    // Create command to execute stored procedure and add parameters
                    DbCommand cmd = defaultDB.GetStoredProcCommand("UpdateProductsTable");
                    defaultDB.AddInParameter(cmd, "productID", DbType.Int32, 53);
                    defaultDB.AddInParameter(cmd, "description", DbType.String, newRow53Value);
                    // Execute two updates and check if each updated one row as expected,
                    // specifying that they execute within the current transaction
                    if (defaultDB.ExecuteNonQuery(cmd, trans) == 1)
                    {
                        Console.WriteLine("Updated row with ID = 53 to '{0}'.", newRow53Value);
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Could not update just one row.");
                    }
                    // Change the values in the command parameters
                    defaultDB.SetParameterValue(cmd, "productID", 84);
                    defaultDB.SetParameterValue(cmd, "description", newRow84Value);
                    if (defaultDB.ExecuteNonQuery(cmd, trans) == 1)
                    {
                        Console.WriteLine("Updated row with ID = 84 to '{0}'.", newRow84Value);
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Could not update just one row.");
                    }
                    // Roll back the transaction
                    trans.Rollback();
                }
                Console.WriteLine(MenuDrivenApplication.Underline);
                Console.WriteLine("Contents of row after rolling back transaction:");
                Console.WriteLine();
                DisplayRowValues(defaultDB.ExecuteReader(CommandType.Text, "SELECT * FROM Products WHERE [Id] = 53"));
                DisplayRowValues(defaultDB.ExecuteReader(CommandType.Text, "SELECT * FROM Products WHERE [Id] = 84"));
            }
        }

        [Description("Use a TransactionScope for a distributed transaction")]
        static void UseTransactionScope()
        {
            Console.WriteLine("Contents of rows before update:");
            Console.WriteLine();
            DisplayRowValues(defaultDB.ExecuteReader(CommandType.Text, "SELECT * FROM Products WHERE [Id] = 53"));
            DisplayRowValues(defaultDB.ExecuteReader(CommandType.Text, "SELECT * FROM Products WHERE [Id] = 84"));
            Console.WriteLine(MenuDrivenApplication.Underline);
            string newRow53Value = "Third and little fingers tend to get cold.";
            string newRow84Value = "Bikes tend to fall off after a few miles.";
            // Create a transaction scope for the distributed operations
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                // Create command to execute stored procedure and add parameters
                DbCommand cmdA = defaultDB.GetStoredProcCommand("UpdateProductsTable");
                defaultDB.AddInParameter(cmdA, "productID", DbType.Int32, 53);
                defaultDB.AddInParameter(cmdA, "description", DbType.String, newRow53Value);
                // Execute first update and check if it updated one row as expected.
                // No distributed transaction will be created for this operation.
                if (defaultDB.ExecuteNonQuery(cmdA) == 1)
                {
                    Console.WriteLine("Updated row with ID = 53 to '{0}'.", newRow53Value);
                }
                else
                {
                    Console.WriteLine("ERROR: Could not update just one row.");
                }
                // Wait for a keypress. At this point there is no distributed transaction.
                // Open Component Services in MMC and view Local DTC Transaction List.
                Console.WriteLine("No distributed transaction. Press any key to continue...");
                Console.WriteLine();
                Console.ReadKey(true);
                // Create second command to execute stored procedure and add parameters.
                // Must use a different connection string to force creation of a new connection.
                DbCommand cmdB = asyncDB.GetStoredProcCommand("UpdateProductsTable");
                asyncDB.AddInParameter(cmdB, "productID", DbType.Int32, 84);
                asyncDB.AddInParameter(cmdB, "description", DbType.String, newRow84Value);
                // Execute second update and check if it updated one row as expected.
                // This will automatically create a new distributed transaction and
                // enrol both this and the original command.
                if (asyncDB.ExecuteNonQuery(cmdB) == 1)
                {
                    Console.WriteLine("Updated row with ID = 84 to '{0}'.", newRow84Value);
                }
                else
                {
                    Console.WriteLine("ERROR: Could not update just one row.");
                }
                // Wait for a keypress. At this point there is a new distributed transaction.
                Console.WriteLine("New distributed transaction created. Press any key to continue...");
                Console.ReadKey(true);
            }
            // TransactionScope now disposed without executing TransactionScope.Complete method
            // to commit changes. Therefore changes will be rolled back automatically.
            Console.WriteLine(MenuDrivenApplication.Underline);
            Console.WriteLine("Contents of row after disposing TransactionScope:");
            Console.WriteLine();
            DisplayRowValues(defaultDB.ExecuteReader(CommandType.Text, "SELECT * FROM Products WHERE [Id] = 53"));
            DisplayRowValues(defaultDB.ExecuteReader(CommandType.Text, "SELECT * FROM Products WHERE [Id] = 84"));
        }

        #region Auxiliary routines

        private static bool SupportsAsync(Database db)
        {
            if (db.SupportsAsync)
            {
                Console.WriteLine("Database supports asynchronous operations");
                return true;
            }
            Console.WriteLine("Database does not support asynchronous operations");
            return false;
        }

        private static void DisplayRowValues(DataTable table)
        {
            Console.WriteLine(MenuDrivenApplication.Underline);
            Console.WriteLine("Rows in the table named '{0}':", table.TableName);
            Console.WriteLine();
            DisplayRowValues(table.CreateDataReader());
        }

        private static void DisplayRowValues(IDataReader reader)
        {
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Console.WriteLine("{0} = {1}", reader.GetName(i), reader[i].ToString());
                }
                Console.WriteLine();
            }
        }

        private static void DisplayTableNames(DataSet ds, string methodName)
        {
            Console.WriteLine("Tables in the DataSet obtained using the {0} method:", methodName);
            foreach (DataTable t in ds.Tables)
            {
                Console.WriteLine(" - Table named '{0}' contains {1} rows.", t.TableName, t.Rows.Count);
            }
            Console.WriteLine();
        }

        #endregion
    }
}
