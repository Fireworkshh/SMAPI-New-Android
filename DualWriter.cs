using System;
using System.IO;
using System.Text;

public class DualWriter : TextWriter
{
    private readonly TextWriter _originalConsoleOut;
    private readonly StreamWriter _fileWriter;
    private readonly string _logFilePath;

    public DualWriter(string logFilePath)
    {
        _logFilePath = logFilePath;

        // 删除之前的日志文件
        if (File.Exists(_logFilePath))
        {
            File.Delete(_logFilePath);
        }

        _originalConsoleOut = Console.Out;
        _fileWriter = new StreamWriter(_logFilePath, true)
        {
            AutoFlush = true // 确保每次写入后立即刷新
        };
    }

    public override Encoding Encoding => _originalConsoleOut.Encoding;

    public override void Write(char value)
    {
        _originalConsoleOut.Write(value);
        WriteToFile(value.ToString());
    }

    public override void Write(string value)
    {
        _originalConsoleOut.Write(value);
        WriteToFile(value);
    }

    public override void WriteLine(string value)
    {
        _originalConsoleOut.WriteLine(value);
        WriteToFile(value + Environment.NewLine);
    }

    private void WriteToFile(string text)
    {
        try
        {
            _fileWriter.Write(text);
        }
        catch (Exception ex)
        {
            _originalConsoleOut.WriteLine($"写入日志文件时发生错误: {ex.Message}");
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _fileWriter?.Dispose();
        }
        base.Dispose(disposing);
    }
}
