namespace posConsole.StaticMethods;

using System;
using System.IO;
using System.Security.Cryptography;
using posConsole.Types;

static class IOMethods
{
    public static async Task<IOStatus> TryCopyFile(string from, string to)
    {
        try
        {
            File.Copy(from, to); //Tuataj wstawic try/catch do File.Copy
        }
        catch (UnauthorizedAccessException)
        {
            return IOStatus.AccessDenied;

        }
        catch (FileNotFoundException)
        {
            return IOStatus.FileNotFound;

        }
        catch (DirectoryNotFoundException)
        {
            return IOStatus.DirectoryNotFound;

        }
        catch (IOException)
        {
            return IOStatus.IoError;

        }

        return IOStatus.Success;

    }

    public static async Task<(byte[]?,IOStatus)> TryCalculateSHA256(string file)
    {
        byte[]? hash = null;
        try
        {
            await using FileStream stream = File.OpenRead(file);
            hash = await SHA256.HashDataAsync(stream);
        }
        catch (UnauthorizedAccessException)
        {
            return (hash, IOStatus.AccessDenied);

        }
        catch (FileNotFoundException)
        {
            return (hash, IOStatus.FileNotFound);

        }
        catch (DirectoryNotFoundException)
        {
            return (hash, IOStatus.DirectoryNotFound);

        }
        catch (IOException)
        {
            return (hash, IOStatus.IoError);
        }

        return (hash, IOStatus.Success);

    }

    public static bool HashCalculate(byte[] a, byte[] b)
    {
        return a.AsSpan().SequenceEqual(b);
    }

    public static async Task<IOStatus> TrySaveHash(string file, byte[] hash)
    {
        try
        {
            await using FileStream streamBackup = File.Create(file);
            streamBackup.Write(hash);
        }
        catch (UnauthorizedAccessException)
        {
            return IOStatus.AccessDenied;

        }
        catch (FileNotFoundException)
        {
            return IOStatus.FileNotFound;

        }
        catch (DirectoryNotFoundException)
        {
            return IOStatus.DirectoryNotFound;

        }
        catch (IOException)
        {
            return IOStatus.IoError;
        }
        return IOStatus.Success;
    }
    public static IOStatus TryDelete(string file)
    {
        try {File.Delete(file);}
        catch(UnauthorizedAccessException)     {return IOStatus.AccessDenied; }
        catch(DirectoryNotFoundException)       {return IOStatus.DirectoryNotFound; }
        catch(IOException)                      {return IOStatus.IoError; }
        return IOStatus.Success;
    }
}
