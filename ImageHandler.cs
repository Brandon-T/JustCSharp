<%@ WebHandler Language="C#" Class="ImageHandler" %>

using System;
using System.Web;

public class ImageHandler : IHttpHandler {
    
    public void ProcessRequest (HttpContext context)
    {
        string Info = context.Request.QueryString["ID"];
		string StoredProcedure = context.Request.QueryString["SProc"];
		string StoredParameter = context.Request.QueryString["SParam"];
		
		if (string.IsNullOrEmpty(Info))
		{
			Info = context.Request.QueryString["Name"];
		}
		
        if (!string.IsNullOrEmpty(Info) && !string.IsNullOrEmpty(StoredProcedure) && !string.IsNullOrEmpty(StoredParameter))
        {
            string ConnectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            System.Data.SqlClient.SqlConnection Connect = new System.Data.SqlClient.SqlConnection(ConnectionString);
            System.Data.SqlClient.SqlCommand Command = new System.Data.SqlClient.SqlCommand(StoredProcedure, Connect);
            Command.CommandType = System.Data.CommandType.StoredProcedure;
            Command.Parameters.AddWithValue(StoredParameter, Info);

            byte[] ImageBuffer = null;
            try
            {
                Connect.Open();
                ImageBuffer = (byte[])Command.ExecuteScalar();
            }
            catch (Exception Ex)
            {
                Ex.ToString();
            }
			finally
			{
				Connect.Close();
			}

            context.Response.ContentType = "Image/Png";
            context.Response.OutputStream.Write(ImageBuffer, 0, ImageBuffer.Length);
        }
    }
 
    public bool IsReusable {
        get {
            return true;
        }
    }

}