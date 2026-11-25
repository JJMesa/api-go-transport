using GoTransport.Domain.Common;

namespace GoTransport.Application.Commons;

public static class Constants
{
    public const string ApplicationUser = AuditDefaults.SystemUser;
    public const int MaximumRecordsPaged = 50;
    public const string PaginationHeader = "Pagination";
}