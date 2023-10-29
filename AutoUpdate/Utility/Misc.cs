using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using AutoUpdate.Structs;

namespace AutoUpdate.Utility;

public class Misc
{
    
    public static void RunProcess(string? filename, string arguments)
    {
        if (!File.Exists(filename))
        {
            throw new FileNotFoundException($"File not found: {filename}");
        }
        
        Process cmd = new Process();
        cmd.StartInfo.FileName = "cmd.exe";
        cmd.StartInfo.Arguments = arguments;
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = false;
        cmd.StartInfo.UseShellExecute = false;
        cmd.Start();

        cmd.StandardInput.WriteLine($"\"{filename}\" {arguments}");
        cmd.StandardInput.Flush();
        cmd.StandardInput.Close();
        
        cmd.WaitForExit();
    }

    public static string RunProcessAndGetOutput(string filename, string arguments)
    {
        if (!File.Exists(filename))
        {
            throw new FileNotFoundException($"File not found: {filename}");
        }
        
        Process cmd = new Process();
        cmd.StartInfo.FileName = "cmd.exe";
        cmd.StartInfo.Arguments = arguments;
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = false;
        cmd.StartInfo.UseShellExecute = false;
        cmd.Start();
        
        cmd.StandardInput.WriteLine($"\"{filename}\" {arguments}");
        cmd.StandardInput.Flush();
        cmd.StandardInput.Close();
        string output = cmd.StandardOutput.ReadToEnd();
        cmd.WaitForExit();
        return output;
    }

    public static async Task<string> DownloadLatest()
    {
        try
        {
            
            string edition = "win";

            string url = $"https://www.minecraft.net/en-us/download/server/bedrock/";
            HttpClient client = new();
            // Set the timeout for the http client to 5 seconds
            client.Timeout = TimeSpan.FromSeconds(5);

            HttpRequestMessage request = new(HttpMethod.Get, url);

            // Add the user agent header to look like a normal browser
            request.Headers.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/119.0");
            request.Headers.Add("Accept", "*/*");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Accept-Language", "en-US,en;q=0.9");

            HttpResponseMessage response = await client.SendAsync(request);

            // This content is likely compressed, so we will need to decompress it
            // We can do this by using the GZipStream class
            // We will also need to use a StreamReader to read the decompressed data
            
            // Check if the response was successful first
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return "Unknown";
            }
            
            // Decompress the response
            // Create a new GZipStream
            GZipStream gzipStream = new(response.Content.ReadAsStream(), CompressionMode.Decompress);
            // Create a new StreamReader
            StreamReader reader = new(gzipStream, Encoding.UTF8);
            // Read the response
            string decompressedResponse = await reader.ReadToEndAsync();
            // Close the reader
            reader.Close();
            // Close the GZipStream
            gzipStream.Close();
            

            if (edition is not "win" and not "linux" and not "win-preview" and not "linux-preview") 
            {
                Console.WriteLine("Invalid edition.");
                return "Unknown";
            }
            
            string link = Regex.Match(decompressedResponse, $@"https://minecraft.azureedge.net/bin-{edition.ToLower()}/bedrock-server-\d+\.\d+\.\d+\.\d+\.zip").Value;
                
            // Check if the link is empty
            if (link.Length == 0)
            {
                Console.WriteLine("Unable to find download link.");
                return "Unknown";
            }
                
            // Download the file
            Console.WriteLine("Downloading...");
            client = new();
            // Set the timeout for the http client to 5 seconds
            client.Timeout = TimeSpan.FromSeconds(5);
            
            // Create a new request
            request = new(HttpMethod.Get, link);
            
            // Add the user agent header to look like a normal browser
            request.Headers.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/119.0");
            
            // Send the request
            response = await client.SendAsync(request);
            
            // Check if the response was successful first
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return "Unknown";
            }
            
            // Get the file name
            string fileName = Regex.Match(link, @"bedrock-server-\d+\.\d+\.\d+\.\d+").Value;

            if (Directory.Exists(Program.Config.WorkingDirectory + "\\" + fileName))
                return fileName;
            
            // Create a new file stream
            FileStream fileStream = new(Program.Config.WorkingDirectory + "\\" + fileName + ".zip", FileMode.Create);
            
            // Copy the response stream to the file stream
            await response.Content.CopyToAsync(fileStream);
            
            // Close the file stream
            
            fileStream.Close();
            Console.WriteLine("Download complete.");
            
            // Unzip
            Console.WriteLine("Unzipping...");
            ZipFile.ExtractToDirectory(Program.Config.WorkingDirectory + "\\" + fileName + ".zip", Program.Config.WorkingDirectory + "\\" + fileName);
            Console.WriteLine("Unzipped.");
            
            // Delete the zip file
            File.Delete(Program.Config.WorkingDirectory + "\\" + fileName + ".zip");
            
            return fileName;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }

        return "Unknown";
    }
    
    public static Function GetFunctionBytes(Section section, PublicSymbol symbol)
    {
        var nextFunctionAddress = FindNextLowestAddress(symbol)!.Value.Address;
            
        // Load the exe into a byte array
        byte[] exeBytes = File.ReadAllBytes(Program.Config.ExeFile);
            
        // Get the offset of the section
        int sectionOffset = int.Parse(section.FilePointerToRawData, System.Globalization.NumberStyles.HexNumber);
            
        // Get the offset of the public symbol in the exe
        int publicOffsetInExe = sectionOffset + symbol.Address;
            
        var functionSize = nextFunctionAddress - symbol.Address;
            
        // Read first 4 bytes of the public symbol
        byte[] publicFunctionBytes = new byte[functionSize];
        Array.Copy(exeBytes, publicOffsetInExe, publicFunctionBytes, 0, publicFunctionBytes.Length);
        
        return new Function()
        {
            Bytes = publicFunctionBytes,
            Address = (ulong) publicOffsetInExe
        };
    }
    
    
    
    public static PublicSymbol? FindNextLowestAddress(PublicSymbol currentSymbol)
    {
        // Go through ALL the public symbols (Since its not sorted at all) and find the lowest address more than the current symbol's address

        PublicSymbol currentSmallest = new PublicSymbol()
        {
            Address = int.MaxValue
        };
            
        foreach (var symbol in Program.PublicSymbolsDict)
        {
            if (symbol.Value.Address > currentSymbol.Address)
            {
                if (symbol.Value.Address < currentSmallest.Address)
                    currentSmallest = symbol.Value;
            }
        }

        return currentSmallest;
    }

    public static string GetVersion()
    {
        return "Unknown";
    }
}