namespace posConsole.Types;


enum IOStatus
{
    NotInit = -1,
    Success = 0,
    FileNotFound,
    DirectoryNotFound,
    AccessDenied,
    IoError,
    InvalidJson,
    NullList,
    InvalidSave,
    InvalidHash,
}