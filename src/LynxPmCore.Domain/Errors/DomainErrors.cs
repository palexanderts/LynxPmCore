using LynxPmCore.Shared.Common;

namespace LynxPmCore.Domain.Errors;

public static class DomainErrors
{
    public static class Notice
    {
        public static readonly Error NotFound = new("Notice.NotFound", "Notice not found.");
        public static readonly Error AlreadyClosed = new("Notice.AlreadyClosed", "Notice is already closed.");
        public static readonly Error AlreadyApproved = new("Notice.AlreadyApproved", "Notice is already approved.");
        public static readonly Error CannotModifyWhenClosed = new("Notice.CannotModify", "Notice cannot be modified when closed.");
        public static readonly Error InvalidStatus = new("Notice.InvalidStatus", "Invalid notice status transition.");
    }

    public static class Operation
    {
        public static readonly Error NotFound = new("Operation.NotFound", "Operation not found.");
        public static readonly Error AlreadyStarted = new("Operation.AlreadyStarted", "Operation already started.");
        public static readonly Error AlreadyCompleted = new("Operation.AlreadyCompleted", "Operation already completed.");
        public static readonly Error CannotPauseRelocate = new("Operation.CannotPause", "Relocate operations cannot be paused.");
        public static readonly Error RequiresEquipmentScan = new("Operation.RequiresScan", "This operation requires equipment scan confirmation.");
        public static readonly Error RequiresPhotoConfirmation = new("Operation.RequiresPhoto", "Relocate operations require photo confirmation.");
    }

    public static class Equipment
    {
        public static readonly Error NotFound = new("Equipment.NotFound", "Equipment not found.");
        public static readonly Error InvalidCode = new("Equipment.InvalidCode", "Equipment code is invalid.");
    }

    public static class Customer
    {
        public static readonly Error NotFound = new("Customer.NotFound", "Customer not found.");
    }

    public static class User
    {
        public static readonly Error NotFound = new("User.NotFound", "User not found.");
        public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "Invalid credentials.");
        public static readonly Error Unauthorized = new("User.Unauthorized", "User is not authorized.");
    }
}
