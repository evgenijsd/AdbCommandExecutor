using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AdbCommandExecutor.Helpers.ProcessHelpers
{
#nullable enable
    public class AOResult
    {
        private readonly DateTime _creationUtcTime;

        public AOResult(
            [CallerMemberName] string? callerName = null,
            [CallerFilePath] string? callerFile = null,
            [CallerLineNumber] int callerLineNumber = 0)
        {
            _creationUtcTime = DateTime.UtcNow;
            CallerName = callerName;
            CallerFile = callerFile;
            CallerLineNumber = callerLineNumber;
        }

        #region -- Public properties --

        public TimeSpan OperationTime { get; private set; }

        public bool IsSuccess { get; private set; }

        public Exception? Exception { get; private set; }

        public string? ErrorId { get; private set; }

        public string? Message { get; private set; }

        public string? CallerName { get; private set; }

        public string? CallerFile { get; private set; }

        public int CallerLineNumber { get; private set; }

        public bool TrackingResult { get; set; } = true;

        public IDictionary<int, string>? AdditionalMessages { get; private set; }

        #endregion

        #region -- Public helpers --

        public void MergeResult(AOResult res)
        {
            CallerName = res.CallerName;
            CallerLineNumber = res.CallerLineNumber;
            CallerFile = res.CallerFile;

            SetResult(res.IsSuccess, res.ErrorId, res.Message, res.Exception);
        }

        public void SetSuccess()
        {
            SetResult(true, null, null, null);
        }

        public void SetAdditionalMessages(IDictionary<int, string> additionalMessages)
        {
            AdditionalMessages = additionalMessages;
        }

        public void SetFailure()
        {
            SetResult(false, null, null, null);
        }

        public void SetFailure(string message)
        {
            SetResult(false, null, message, null);
        }

        public void SetFailure(Exception ex)
        {
            SetResult(false, null, null, ex);
        }

        public void ArgumentException(string argumentName, string message)
        {
            SetError("ArgumentException", $"argumentName: {argumentName}, message: {message}");
        }

        public void ArgumentNullException(string argumentName)
        {
            SetError("ArgumentNullException", $"argumentName: {argumentName}");
        }

        public void SetError(string errorId, string message, Exception? ex = null)
        {
            SetResult(false, errorId, message, ex);
        }

        public void SetResult(bool isSuccess, string? errorId, string? message, Exception? ex)
        {
            var finishTime = DateTime.UtcNow;
            OperationTime = finishTime - _creationUtcTime;
            IsSuccess = isSuccess;
            ErrorId = errorId;
            Exception = ex;
            Message = message;

            if (TrackingResult)
            {
                TrackResult();
            }
        }

        #endregion

        #region -- Protected helpers --

        protected virtual void TrackResult()
        {
            if (!IsSuccess)
            {
#if DEBUGGER_BREAK
                //Debugger.Break();
#endif
                // Use analytics service when installed
                //var analyticsService = App.Resolve<IAnalyticsService>();
                var param = new Dictionary<string, string?>();
                param[nameof(CallerName)] = CallerName;
                param[nameof(CallerFile)] = CallerFile;
                param[nameof(CallerLineNumber)] = CallerLineNumber.ToString();

                if (!string.IsNullOrEmpty(ErrorId))
                {
                    param[nameof(ErrorId)] = ErrorId;
                }

                if (!string.IsNullOrEmpty(Message))
                {
                    param[nameof(Message)] = Message;
                }

                if (Exception != null)
                {
                    param["ExceptionType"] = Exception.GetType().Name;
                    param["ExceptionMessage"] = Exception.Message;
                }

                param[nameof(OperationTime)] = OperationTime.TotalMilliseconds.ToString();

                // Use analytics service when installed
                //analyticsService.Track($"{CallerName}_AsyncOperation_Failure", param);
            }
        }

        #endregion
    }

#nullable enable
    public class AOResult<T> : AOResult
    {
        public AOResult([CallerMemberName] string? callerName = null, [CallerFilePath] string? callerFile = null, [CallerLineNumber] int callerLineNumber = 0)
            : base(callerName, callerFile, callerLineNumber)
        {
        }

        #region -- Public properties --

        public T? Result { get; private set; }

        #endregion

        #region -- Public helpers --

        public void MergeResult(T result, AOResult res)
        {
            Result = result;
            MergeResult(res);
        }

        public void SetSuccess(T result)
        {
            Result = result;
            SetSuccess();
        }

        public void SetResult(T result, bool isSuccess, string errorId, string message, Exception? ex = null)
        {
            Result = result;
            SetResult(isSuccess, errorId, message, ex);
        }

        public void SetFailure(T result)
        {
            Result = result;
            SetFailure();
        }

        #endregion
    }

    public class AOResult<TResult, TStatus> : AOResult<TResult>
    {
        #region -- Public properties --

        public TStatus? Status { get; private set; }

        #endregion

        #region -- Public helpers --

        public void SetFailure(Exception ex, TStatus status)
        {
            Status = status;
            SetFailure(ex);
        }

        public void SetFailure(TStatus status)
        {
            Status = status;
            SetFailure();
        }

        #endregion
    }
}
