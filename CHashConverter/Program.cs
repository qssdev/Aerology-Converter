using System.Text;
using System.Text.RegularExpressions;


string filePath = "";
string convertedData = "";
List<string> fileData;
string Process = "";
string YNSave = "N";

do
{
    Console.WriteLine("Please choose \n 1 - To convert .cs file \n 2 - To convert the code directly");
    Process = Console.ReadLine();

    Console.WriteLine("Do you want to save the file \n Y - Yes \n N - No to print on the screen only");
    YNSave = Console.ReadLine();

    if (Process == "1")
    {
        Console.WriteLine("Please enter the File path to be converted.");
        filePath = Console.ReadLine();

        //Validate the file exists
        if (filePath.Length == 0 || !File.Exists(filePath))
        {
            Console.WriteLine("File does not exist or invalid file location.");
            Console.ReadLine();
            return;
        }

        //Validate the file if its correct c# object
        if (Path.GetExtension(filePath) != ".cs")
        {
            Console.WriteLine("File is not a c# file.");
            Console.ReadLine();
            return;
        }

        //Process the file data
        fileData = File.ReadAllLines(filePath).ToList();
        if (fileData.Count == 0)
        {
            Console.WriteLine("File is empty.");
            Console.ReadLine();
            return;
        }
        convertedData = Convert(fileData);
    }
    else
    {
        filePath = AppContext.BaseDirectory + "converted.ts";
        Console.WriteLine("Please paste code to convert then press ENTER.");
        fileData = new List<string>();
        string line;
        ConsoleKeyInfo cki;
        do
        {
            fileData.Add(Console.ReadLine());
            cki = Console.ReadKey();
            fileData.Add(cki.KeyChar.ToString());

        } while (cki.Key != ConsoleKey.Enter);
       
        convertedData = Convert(fileData);
    }
    
    if (convertedData.Length > 0)
    {
        if (YNSave.ToUpper() == "Y")
        {
            using (FileStream fs = File.Create(filePath.Replace(".cs", ".ts")))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(convertedData);
                fs.Write(info, 0, info.Length);
            }
        }
        else
        {
            Console.WriteLine(convertedData);
        }
    }

} while (true);

//Return type of the property
static string ConvertType(string Type)
{
    Dictionary<string, string> typeDictionary = new Dictionary<string, string>
    {
        { "STRING", "string" },
        { "INT?", "number" },
        { "INT", "number" },
        { "LONG?", "number" },
        { "LONG", "number" },
        { "BOOl", "boolean" },
        { "LIST", "[]" }
    };

    string newDataType="";

    typeDictionary.TryGetValue(Type.ToUpper(), out newDataType);

    return newDataType;
}

//Remove some characters
static string RemoveChar(string sLine)
{
    return sLine.Replace("{", "").Replace("}", "").Replace(";", "");
}

//Replace class with interface
static string ConvertClass(string Class)
{ 
    return Class.Replace("class", "interface");
}


static string ConvertAccessModifier(string AccessModifier)
{
    return AccessModifier.Replace("public", "export").Replace("private", "").Replace("seald", "").Replace("abstract", "");
}

//Get the class name using regex
static string GetClassName(string sLine)
{
    string propertyName = "";
    // Regular expression to match property declarations
    Regex propertyRegex = new Regex(@"(((internal)|(public)|(private)|(protected)|(sealed)|(abstract)|(static))?[\s\r\n\t]+){0,2}class[\s\S]+?(\w+)");

    // Match all property declarations in the code
    MatchCollection propertyMatches = propertyRegex.Matches(sLine.Trim());

    if (propertyMatches.Any())
    {
        propertyName = propertyMatches[0].Groups[10].Value;
    }
    return propertyName;
}

//Get the property name using regex
static string GetPropertyName(string sLine)
{
    string propertyName = "";
    // Regular expression to match property declarationspublic\s+\w+\s+(\w+)\s{+\w+;\s+\w+;}
    Regex propertyRegex = new Regex(@"((public)|(protected))\s+\w+\s+(\w+)\s+{\s+get;\s+set;\s+}");

    // Match all property declarations in the code
    MatchCollection propertyMatches = propertyRegex.Matches(sLine);

    if (propertyMatches.Any())
    {
        propertyName = propertyMatches[0].Groups[4].Value;
    }
    return propertyName;
}

//Get the property name using regex
static string GetPropertyType(string sLine)
{
    string propertyName = "";
    // Regular expression to match property declarationspublic\s+\w+\s+(\w+)\s{+\w+;\s+\w+;}
    Regex propertyRegex = null;
    if (sLine.Trim().Contains("List<") || (sLine.Trim().Contains("IEnumerable<")))
    {
        propertyRegex = new Regex(@"((public)|(protected))\s(\w+)(<\w+>)");
        // Match all property declarations in the code
        MatchCollection propertyMatches = propertyRegex.Matches(sLine);
        if (propertyMatches.Any())
        {
            propertyName = propertyMatches[0].Groups[4].Value +";" + propertyMatches[0].Groups[5].Value.Trim();
        }
    }
    else
    {
        // Match all property declarations in the code
        propertyRegex = new Regex(@"((public)|(protected))\s+((\w+)|\w+\?)\s+(\w+)\s+(?:{)\s+");
        MatchCollection propertyMatches = propertyRegex.Matches(sLine);
        if (propertyMatches.Any())
        {
            propertyName = propertyMatches[0].Groups[4].Value;
        }
    }

    return propertyName;
}

//Remove and extract name and type per line
static string Convert(List<string> fileData)
{
    bool isDoneWithClass = false;
    bool isDoneWithNamespace = false;
    string className = "";
    string propName = "";
    string typeName = "";
    int classCount = 0;
    StringBuilder newData = new StringBuilder();
    fileData = fileData.Where(f => !f.Contains("using") && !(f.Length == 0) && !(f.Trim().Length == 0)).ToList();
    foreach (string sData in fileData)
    {
        string data = "";

        //get the namespace and starting {
        if (sData.Contains("namespace") || isDoneWithNamespace && sData.StartsWith("{"))
        {
            isDoneWithNamespace = true;
            data = sData.Trim() + "\n";
            newData.Append(data);
            continue;
        }

        if (sData.Trim().Length <= 1 || (sData.Trim() == "{" || sData.Trim() == "}"))
        {
            data = sData.Trim() + "\n";
            newData.Append(data);
            continue;
        }

        //if there is another class inside the class
        if (sData.Contains("class") && isDoneWithClass)
        {
            isDoneWithClass = false;
            newData.Append("} \n");
            classCount++;
        }
        
        if (isDoneWithClass)
        {
            
            string dataType = GetPropertyType(sData.Trim());

            if (sData.Trim().StartsWith("public") && isDoneWithClass)
                data = sData.Replace("public", "");
            data = RemoveChar(data).Replace("get", "").Replace("set", "");
            if (sData.Trim().Contains("List<") || (sData.Trim().Contains("IEnumerable<")))
            {
                dataType = dataType.Substring(dataType.IndexOf(";") + 1).Replace("<", "").Replace(">", "");
                var regex = new Regex(Regex.Escape(dataType));
                data = regex.Replace(data, "", 1).Replace("List<", "").Replace(">", "");

                data = data.Trim() + ":" + dataType + "[];\n";
            }
            else
            { 
                data = data.Replace(dataType, "").Trim()+":" + ConvertType(dataType) + ";\n";
      
            }
            data = "\t" + data.Substring(0, 1).ToLower() + data.Remove(0,1);
        }
        else
        {
            className = GetClassName(sData);

            if (sData.Trim().StartsWith("public"))
                data = ConvertAccessModifier(sData);

            if (data.Contains("class"))
            {
                isDoneWithClass = true;
                data = ConvertClass(data).Trim();
            }
        }
        newData.Append(data);
    }
    if (classCount >= 1)
    { 
        string newDataString = newData.ToString().TrimEnd().Remove(newData.ToString().TrimEnd().Length-1, 1);
        newData.Clear();
        newData.Append(newDataString);
    }
    return newData.ToString();
}
