using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors("AllowAll");

// Serve static files (our HTML frontend) - must come before routing
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.MapControllers();

app.Run();

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    [HttpGet]
    public IActionResult GetFiles([FromQuery] string folder1Path = "", [FromQuery] string folder2Path = "")
    {
        try
        {
            var result = new
            {
                Folder1 = GetFolderFiles(folder1Path, "Folder 1"),
                Folder2 = GetFolderFiles(folder2Path, "Folder 2")
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private object GetFolderFiles(string folderPath, string folderName)
    {
        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
        {
            return new
            {
                FolderName = folderName,
                Path = folderPath,
                Exists = false,
                Files = new List<object>()
            };
        }

        var files = Directory.GetFiles(folderPath)
            .Select(filePath => new
            {
                Name = Path.GetFileName(filePath),
                FullPath = filePath,
                Extension = Path.GetExtension(filePath),
                Size = new FileInfo(filePath).Length,
                SizeFormatted = FormatFileSize(new FileInfo(filePath).Length),
                Type = DetermineFileType(filePath),
                LastModified = System.IO.File.GetLastWriteTime(filePath).ToString("yyyy-MM-dd HH:mm:ss")
            })
            .OrderBy(f => f.Name)
            .ToList();

        return new
        {
            FolderName = folderName,
            Path = folderPath,
            Exists = true,
            Files = files
        };
    }

    private string DetermineFileType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();

        return extension switch
        {
            ".txt" => "Text",
            ".md" => "Markdown",
            ".json" => "JSON",
            ".xml" => "XML",
            ".csv" => "CSV",
            ".log" => "Log",
            ".html" => "HTML",
            ".css" => "CSS",
            ".js" => "JavaScript",
            ".cs" => "C# Code",
            ".py" => "Python",
            ".java" => "Java",
            ".cpp" or ".cc" or ".cxx" => "C++",
            ".c" => "C Code",
            ".h" => "Header",
            ".sql" => "SQL",
            ".pdf" => "PDF",
            ".doc" or ".docx" => "Word Document",
            ".xls" or ".xlsx" => "Excel",
            ".ppt" or ".pptx" => "PowerPoint",
            ".jpg" or ".jpeg" => "JPEG Image",
            ".png" => "PNG Image",
            ".gif" => "GIF Image",
            ".bmp" => "Bitmap Image",
            ".svg" => "SVG Image",
            ".zip" => "ZIP Archive",
            ".rar" => "RAR Archive",
            ".7z" => "7-Zip Archive",
            ".exe" => "Executable",
            ".dll" => "Library",
            ".bin" => "Binary",
            ".hex" => "Hex File",
            ".md5" => "MD5 Hash",
            ".sha1" => "SHA1 Hash",
            ".sha256" => "SHA256 Hash",
            _ when IsTextFile(filePath) => "Text",
            _ when IsBinaryFile(filePath) => "Binary",
            _ => "Unknown"
        };
    }

    private bool IsTextFile(string filePath)
    {
        try
        {
            using var reader = new StreamReader(filePath);
            var buffer = new char[512];
            var charsRead = reader.Read(buffer, 0, buffer.Length);

            for (int i = 0; i < charsRead; i++)
            {
                if (char.IsControl(buffer[i]) && buffer[i] != '\r' && buffer[i] != '\n' && buffer[i] != '\t')
                    return false;
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool IsBinaryFile(string filePath)
    {
        return !IsTextFile(filePath);
    }

    private string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return string.Format("{0:n1} {1}", number, suffixes[counter]);
    }
}