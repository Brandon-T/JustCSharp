//Examples:


/**		The following code will teach you how to use the Database class and the Image class for uploading and downloading anything.
  *		By: Brandon-T.
  **/
  
private void DownloadFromDatabase()
{
	Database DB = new Database(ConnectionString);
	DB.SingleTable("NameOfStoredProcedure", "@NameOfStoredProcedureParameter", "ValueToPassAsParameter");
	DataTable Table = DB.SingleDownload();

	foreach (DataRow Row in Table.Rows)
	{
		Console.WriteLine(Row["Table Row Name"]);	//The name of the row the store procedure selects.
		Console.WriteLine(Row[0]);				//Row zero.
	}
}


											//Practical Example..
private void DownloadFromDatabase2()
{
	int ProductID = 1;	//Product with an ID of 1.
	
	Database DB = new Database(ConnectionString);
	DB.SingleTable("GetProductImage", "@ProductID", ProductID);  //StoredProcedure, ParameterName, Parameter Value.
	DataTable Table = DB.SingleDownload();
	
	foreach (DataRow Row in Table.Rows)
	{
		Console.WriteLine(Row["Product_Image"]);		//The store procedure selects Product_Image FROM Product_T.
	}
}

private void DownloadFromDatabase3()
{
	Database DB = new Database(ConnectionString);
	DB.SingleTable("GetProductID", "@ProductName", "Sweet Apple");	//Sweet apple is the name of a product.
	DataTable Table = DB.SingleDownload();
	
	//IF we know that our table only returns one item..
	Console.WriteLine(Table.Rows[0][0]);		    //Get Item 0 in the table.
	Console.WriteLine(Table.Rows[0]["ProductID"]);	//Same as above..
}

private void DownloadFromDatabase4()
{
	Dictionary<string, object> Layout = new Dictionary<string, object>();
	Layout.Add("@ProductName", "Sweet Apple");	//Sweet apple is the name of a product.
	
	Database DB = new Database(ConnectionString);
	DB.SingleTable("GetProductID", Layout);
	DataTable Table = DB.SingleDownload();
	
	Console.WriteLine(Table.Rows[0][0]);	//Get the Item ID and print it to the screen.
}


/**EXAMPLE: Sending multiple parameters to the stored procedure.. Uploading and Downloading*/

private void DownloadWithMultipleParameters()
{
	string[] ProcedureParameterNames = {"@UserName", "@Password"};
	object[] Parameters = {"Brandon", "MyPassword"};
	
	Database DB = new Database(ConnectionString);
	DB.SingleTable("LoginUser", ProcedureParameterNames, Parameters);
	DataTable Table = DB.SingleDownload();
	
	if (Table.Rows[0][0] == "TRUE")
	{
		Console.WriteLine("Logged In Successfully");
	}
	else
	{
		Console.WriteLine("Error Logging in. Please Check your credentions again");
	}
}

private void DownloadWithMultipleParameters2()
{
	Dictionary<string, object> StoredProcedureParameters = new Dictionary<string, object>();
	StoredProcedureParameters.Add("@Username", "Brandon");
	StoredProcedureParameters.Add("@Password", "MyPassword");

	Database DB = new Database(ConnectionString);
	DB.SingleTable("LoginUser", StoredProcedureParameters);
	DataTable Table = DB.SingleDownload();
	
	if (Table.Rows[0][0] == "TRUE")
	{
		Console.WriteLine("Logged In Successfully");
	}
	else
	{
		Console.WriteLine("Error Logging in. Please Check your credentions again");
	}
}

private void UploadToDatabase()
{
	if (FileUpload1.HasFile)
	{
		byte[] ImageBytes = FileUpload1.FileBytes;
		
		string[] ParameterNames = {"@ImageID", "@ImageBytes"};	//Stored Procedure Parameters.
		object[] Parameters = {200, ImageBytes};				//Values to pass to those parameters.. ImageID = 200. ImageBytes = Bytes..
		
		Database DB = new Database(ConnectionString);
		DB.SingleTable("UploadImage", ParameterNames, Parameters);
		DB.Upload();												//Upload..
	}
}











/** Using the Image Handler to GET Images from the database and display them.. 3 Examples. **/

private void GetImageFromDatabaseByName()
{
	string ProductName = "Laptop 3000";
	string StoredProcedure = "GetImage";
	string StoredProcedureParameter = "@ProductName";
	
	string QueryString = "Name=" + ProductName + "&SProc=" + StoredProcedure + "&SParam=" + StoredProcedureParameter;
	
	string Image = "<img src='ImageHandler.ashx?" + QueryString + "' alt='" + ProductName + "' />";
	Label1.Text = Image;		//The Label will now turn into an Image and display the image on screen.
}

private void GetImageFromDatabaseByID()
{
	int ProductID = 127;
	string StoredProcedure = "GetImage";
	string StoredProcedureParameter = "@ProductID";
	
	string QueryString = "ID=" + ProductID.ToString() + "&SProc=" + StoredProcedure + "&SParam=" + StoredProcedureParameter;
	
	string Image = "<img src='ImageHandler.ashx?" + QueryString + "' alt='" + ProductID.ToString() + "' />";
	Label1.Text = Image;		//The Label will now turn into an Image and display the image on screen.
}

private void GetImageFromDatabaseByIDString()
{
	int ProductID = "127";
	string StoredProcedure = "GetImage";
	string StoredProcedureParameter = "@ProductID";
	
	string QueryString = "ID=" + ProductID + "&SProc=" + StoredProcedure + "&SParam=" + StoredProcedureParameter;
	
	string Image = "<img src='ImageHandler.ashx?" + QueryString + "' alt='" + ProductID + "' />";
	Label1.Text = Image;		//The Label will now turn into an Image and display the image on screen.
}