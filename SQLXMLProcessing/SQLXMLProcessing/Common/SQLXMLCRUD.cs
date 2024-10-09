using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SQLXMLProcessing.Common
{
    internal class SQLXMLCRUD
    {

        public static void InsertXMLData(string sConn, string updategramXML)
        {
            ProcessXmlData(updategramXML, sConn);
        }


        static void ProcessXmlData(string xmlData, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // Load the XML
                XDocument xdoc = XDocument.Parse(xmlData);


                // Process each relevant element
                foreach (var element in xdoc.Descendants().Where(e => e.Name.LocalName != "Header"))
                {
                    // Generate and execute the insert command for each element
                    ProcessChildNodes(element, connection, "", true);
                }

            }
        }


        public static void ProcessChildNodes(XElement xmlDoc, SqlConnection connection, string actionNodeName, bool isInsert)
        {

            foreach (var attribute in xmlDoc.Descendants("Record"))
            {
                var columns = new List<string>();
                var values = new List<string>();
                var parameters = new List<SqlParameter>();

                // Build the column and value lists
                var tableName = xmlDoc.Name.LocalName;
                string columnName = xmlDoc.Name.ToString();
                string value = (xmlDoc.Value == "NULL" ? xmlDoc.Value : string.Format("'{0}'", (columnName.ToLower() == xmlDoc.Value)));


                // Determine the appropriate data type
                SqlDbType sqlType = GetSqlDbType(xmlDoc);
                SqlParameter param = new SqlParameter($"@{columnName}", sqlType);


                if (sqlType == SqlDbType.DateTime)
                {
                    DateTime dValue = DateTime.Now;
                    if (DateTime.TryParseExact(dValue.ToString(), "yyyy-MM-dd 00:00:00",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime dateValue))
                    {
                        param.Value = string.Format("'{0}'", dateValue.ToString("yyyy-MM-dd 00:00:00"));
                    }
                    else
                    {
                        param.Value = string.Format("'{0}'", DateTime.Now.ToString("yyyy-MM-dd 00:00:00")); // Assign DBNull for invalid dates
                    }
                }
                else if (sqlType == SqlDbType.NVarChar || sqlType == SqlDbType.VarChar)
                {
                    param.Value = string.IsNullOrEmpty(value) ? (object)DBNull.Value : value;
                }
                else
                {
                    param.Value = value; // Handle other types
                }

                columns.Add(columnName);
                parameters.Add(param);



                if (isInsert)
                {
                    // Insert command
                    string insertQuery = $"INSERT INTO {tableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters.Select(p => p.SqlValue))})";

                    Console.WriteLine($"Table Name : {tableName}");
                    Console.WriteLine($"Insert Query for Each Table: {insertQuery}");
                    ExecuteSqlCommand(connection, insertQuery, parameters);
                }
                else
                {
                    // Update command (assuming you have a primary key or some condition to match)
                    string updateQuery = $"UPDATE {tableName} SET {string.Join(", ", columns)} WHERE your_condition_here;";
                    ExecuteSqlCommand(connection, updateQuery, parameters);
                }
            }
        }


        static void ExecuteSqlCommand(SqlConnection connection, string query, List<SqlParameter> parameters)
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddRange(parameters.ToArray());
                try
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine($"Executed: {query}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing command: {ex.Message}");
                }
            }
        }

        public static SqlDbType GetSqlDbType(XElement attribute)
        {
            // Basic type inference based on attribute name or value
            // You can expand this logic as needed
            if (DateTime.TryParse(attribute.Value, out _))
            {
                return SqlDbType.DateTime;
            }
            if (int.TryParse(attribute.Value, out _))
            {
                return SqlDbType.Int;
            }
            // Add more type checks as needed

            return SqlDbType.NVarChar; // Default to string type
        }

    }
}
