using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustCSharp
{
    public class Database
    {
        SqlConnection Connection;
        string ConnectionString;
        List<string> Procedures;
        List<string[]> Tables;
        List<object[]> TableParameters;
        List<SqlCommand> Commands;
        bool CommandsBuilt = false;
        List<ParameterDirection[]> DirectionParameters;

        public Database(string ConnectionString)
        {
            Procedures = new List<string>();
            Tables = new List<string[]>();
            DirectionParameters = new List<ParameterDirection[]>();
            TableParameters = new List<object[]>();
            Commands = new List<SqlCommand>();
            this.ConnectionString = ConnectionString;
            Connection = new SqlConnection(ConnectionString);
        }

        ~Database()
        {
            Connection.Close();
            for (int I = 0; I < Commands.Count; ++I)
            {
                Commands[I].Dispose();
            }
        }

        private void StoredProcedures(params string[] Parameters)
        {
            if (Parameters != null)
            {
                foreach (string Parameter in Parameters)
                {
                    Procedures.Add(Parameter);
                }
            }
        }

        private void ProcedureLayouts(params string[][] Parameters)
        {
            if (Parameters != null)
            {
                foreach (string[] Parameter in Parameters)
                {
                    Tables.Add(Parameter);
                }
            }
        }

        private void ProcedureParameters(params object[][] Parameters)
        {
            if (Parameters != null)
            {
                foreach (object[] Parameter in Parameters)
                {
                    TableParameters.Add(Parameter);
                }
            }
        }

        private void ParameterDirections(params ParameterDirection[][] Parameters)
        {
            if (Parameters != null)
            {
                foreach (ParameterDirection[] Parameter in Parameters)
                {
                    DirectionParameters.Add(Parameter);
                }
            }
        }

        private void BuildCommands()
        {
            if (!CommandsBuilt)
            {
                int CommandCount = 0;
                Commands.Clear();
                foreach (string StoredProcedure in Procedures)
                {
                    Commands.Add(new SqlCommand(StoredProcedure, Connection));
                    Commands[CommandCount++].CommandType = CommandType.StoredProcedure;
                }

                for (int I = 0, K = 0; I < Tables.Count; ++I, ++K)
                {
                    for (int J = 0; J < Tables[I].Length; ++J)
                    {
                        Commands[K].Parameters.AddWithValue(Tables[I][J], TableParameters[I][J]);
                    }
                }

                if (DirectionParameters.Count > 0)
                {
                    for (int I = 0, K = 0, L = 0; I < Tables.Count; ++I, ++K)
                    {
                        for (int J = 0; J < Tables[I].Length; ++J)
                        {
                            Commands[K].Parameters[L++].Direction = DirectionParameters[I][J];
                        }
                    }
                }
            }
        }

        public void SingleTable(string StoredProcedure, Dictionary<string, object> LayoutParameters = null, bool Build = true, ParameterDirection[] Directions = null)
        {
            if (Build)
            {
                CommandsBuilt = false;
                if (LayoutParameters != null)
                {
                    object[] Parameters = LayoutParameters.Values.ToArray<object>();

                    StoredProcedures(new string[] { StoredProcedure });
                    ProcedureLayouts(new string[][] { LayoutParameters.Keys.ToArray<string>() });
                    ProcedureParameters(Parameters != null ? new object[][] { Parameters } : null);
                }
                else
                {
                    StoredProcedures(new string[] { StoredProcedure });
                    ProcedureLayouts(null);
                    ProcedureParameters(null);
                }
            }
        }

        public void SingleTable(string StoredProcedure, string Layout, object Parameters, bool Build = true, ParameterDirection[] Directions = null)
        {
            if (Build)
            {
                CommandsBuilt = false;
                SingleTable(StoredProcedure, new string[] { Layout }, new object[] { Parameters }, Build, Directions);
            }
        }

        public void SingleTable(string StoredProcedure, string[] Layout, object[] Parameters, bool Build = true, ParameterDirection[] Directions = null)
        {
            if (Build)
            {
                CommandsBuilt = false;
                StoredProcedures(new string[] { StoredProcedure });
                ProcedureLayouts(Layout != null ? new string[][] { Layout } : null);
                ProcedureParameters(Parameters != null ? new object[][] { Parameters } : null);
                ParameterDirections(Directions != null ? new ParameterDirection[][] { Directions } : null);
            }
        }

        public void MultiTable(string[] StoredProcedures, Dictionary<string[], object[]> LayoutParameters = null, bool Build = true, ParameterDirection[] Directions = null)
        {
            if (Build)
            {
                CommandsBuilt = false;
                if (LayoutParameters != null)
                {
                    this.StoredProcedures(StoredProcedures);
                    ProcedureLayouts(LayoutParameters.Keys.ToArray());
                    ProcedureParameters(LayoutParameters.Values.ToArray());
                }
                else
                {
                    this.StoredProcedures(StoredProcedures);
                    ProcedureLayouts(null);
                    ProcedureParameters(null);
                    ParameterDirections(Directions != null ? new ParameterDirection[][] { Directions } : null);
                }
            }
        }

        public void MultiTable(string[] StoredProcedures, string[][] Layout, object[][] Parameters, bool Build = true, ParameterDirection[][] Directions = null)
        {
            if (Build)
            {
                CommandsBuilt = false;
                this.StoredProcedures(StoredProcedures);
                ProcedureLayouts(Layout);
                ProcedureParameters(Parameters);
                ParameterDirections(Directions);
            }
        }

        public DataTable SingleDownload()
        {
            List<DataTable> Tables = Download();
            return (Tables.Count > 0 ? Tables[0].Rows.Count > 0 ? Tables[0] : null : null);
        }

        public DataTable SingleDownload(ref List<object> OutputValues)
        {
            List<List<object>> Values = new List<List<object>>();
            List<DataTable> Tables = Download(ref Values);
            OutputValues = Values.Count > 0 ? Values[0].Count > 0 ? Values[0] : null : null;
            return (Tables.Count > 0 ? Tables[0].Rows.Count > 0 ? Tables[0] : null : null);
        }

        public List<DataTable> Download()
        {
            BuildCommands();
            SqlDataAdapter Adapter = new SqlDataAdapter();
            List<DataTable> DataTables = new List<DataTable>();
            using (Connection)
            {
                Connection.Open();
                foreach (SqlCommand Command in Commands)
                {
                    DataTable Table = new DataTable();
                    Adapter.SelectCommand = Command;
                    Adapter.Fill(Table);
                    DataTables.Add(Table);
                }
                Connection.Close();
            }

            return DataTables;
        }

        public List<DataTable> Download(ref List<List<object>> OutputValues)
        {
            BuildCommands();
            SqlDataAdapter Adapter = new SqlDataAdapter();
            List<DataTable> DataTables = new List<DataTable>();
            using (Connection)
            {
                Connection.Open();
                foreach (SqlCommand Command in Commands)
                {
                    DataTable Table = new DataTable();
                    Adapter.SelectCommand = Command;
                    Adapter.Fill(Table);
                    DataTables.Add(Table);
                }

                if (DirectionParameters.Count > 0)
                {
                    foreach (SqlCommand Command in Commands)
                    {
                        List<object> Values = new List<object>();
                        for (int I = 0; I < Command.Parameters.Count; ++I)
                        {
                            Values.Add(Command.Parameters[I].Value);
                        }
                        OutputValues.Add(Values);
                    }
                }
                Connection.Close();
            }
           
            return DataTables;
        }

        public void Upload()
        {
            BuildCommands();
            using (Connection)
            {
                Connection.Open();
                foreach (SqlCommand Command in Commands)
                {
                    Command.ExecuteNonQuery();
                }
                Connection.Close();
            }
        }

        public void Clear()
        {
            for (int I = 0; I < Commands.Count; ++I)
            {
                Commands[I].Dispose();
            }

            Commands.Clear();
            Procedures.Clear();
            Tables.Clear();
            TableParameters.Clear();
            DirectionParameters.Clear();
        }
    }
}
