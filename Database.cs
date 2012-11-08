using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

public class Database
{
    SqlConnection Connection;
    string ConnectionString;
    List<string> Procedures;
    List<string[]> Tables;
    List<object[]> TableParameters;
    List<SqlCommand> Commands;

    public Database(string ConnectionString)
    {
        Procedures = new List<string>();
        Tables = new List<string[]>();
        TableParameters = new List<object[]>();
        Commands = new List<SqlCommand>();
        this.ConnectionString = ConnectionString;
        Connection = new SqlConnection(ConnectionString);
    }

    ~Database()
    {
        for (int I = 0; I < Commands.Count; ++I)
        {
            Commands[I].Dispose();
        }
    }

    public void StoredProcedures(params string[] Parameters)
    {
        if (Parameters != null)
        {
            foreach (string Parameter in Parameters)
            {
                Procedures.Add(Parameter);
            }
        }
    }

    public void ProcedureLayouts(params string[][] Parameters)
    {
        if (Parameters != null)
        {
            foreach (string[] Parameter in Parameters)
            {
                Tables.Add(Parameter);
            }
        }
    }

    public void ProcedureParameters(params object[][] Parameters)
    {
        if (Parameters != null)
        {
            foreach (object[] Parameter in Parameters)
            {
                TableParameters.Add(Parameter);
            }
        }
    }

    public void BuildCommands()
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
    }

    public void SingleTable(string StoredProcedure, string[] Layout, object[] Parameters)
    {
        StoredProcedures(new string[] { StoredProcedure });
        ProcedureLayouts(Layout != null ? new string[][] { Layout } : null);
        ProcedureParameters(Parameters != null ? new object[][] { Parameters } : null);
    }

    public DataTable SingleDownload()
    {
        List<DataTable> Tables = Download();
        return (Tables.Count > 0 ? Tables[0] : null);
    }

    public List<DataTable> Download()
    {
        SqlDataAdapter Adapter = new SqlDataAdapter();
        List<DataTable> DataTables = new List<DataTable>();
        try
        {
            Connection.Open();
            foreach (SqlCommand Command in Commands)
            {
                try
                {
                    DataTable Table = new DataTable();
                    Adapter.SelectCommand = Command;
                    Adapter.Fill(Table);
                    DataTables.Add(Table);
                }
                catch (Exception Ex)
                {
                    throw new Exception("Error Executing SQLDataAdapter Commands.", Ex);
                }
            }
        }
        catch (Exception Ex)
        {
            throw new Exception("Error Opening Connection/Executing Commands.", Ex);
        }
        finally
        {
            Connection.Close();
        }
        return DataTables;
    }

    public void Upload()
    {
        try
        {
            Connection.Open();
            foreach (SqlCommand Command in Commands)
            {
                try
                {
                    Command.ExecuteNonQuery();
                }
                catch (Exception Ex)
                {
                    throw new Exception("Error Executing SQL Non-Query Commands.", Ex);
                }
            }
        }
        catch (Exception Ex)
        {
            throw new Exception("Error Opening Connection/Executing Commands.", Ex);
        }
        finally
        {
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
    }
}