namespace posConsole.Types;


enum IOStatus
{
    Success = 0,
    FileNotFound,
    DirectoryNotFound,
    AccessDenied,
    IoError,
    InvalidJson,
    NullList,
    InvalidSave
}