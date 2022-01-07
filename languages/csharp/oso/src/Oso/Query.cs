using Oso.Ffi;

namespace Oso;

public class Query : IDisposable
{

    private readonly QueryHandle _handle;

    internal Query(QueryHandle handle)
    {
        _handle = handle;
    }

    // struct polar_CResult_c_char *polar_next_query_event(struct polar_Query *query_ptr);
    // TODO: 
    /*
    public IEnumerator<string> QueryEvents
    {
        get
        {
            // Add error handling to check for error and throw PolarException
        }
    }
    */
    public string? NextEvent()
    {
        return Native.NextQueryEvent(_handle);
    }

    /**
     * Execute one debugger command for the given query.
     *
     * ## Returns
     * - `0` on error.
     * - `1` on success.
     *
     * ## Errors
     * - Provided value is NULL.
     * - Provided value contains malformed JSON.
     * - Provided value cannot be parsed to a Term wrapping a Value::String.
     * - Query.debug_command returns an error.
     * - Anything panics during the parsing/execution of the provided command.
     */
    // struct polar_CResult_c_void *polar_debug_command(struct polar_Query *query_ptr, const char *value);
    public void DebugCommand(string value)
    {
        Native.DebugCommand(_handle, value);
    }

    public bool TryDebugCommand(string value)
    {
        try
        {
            DebugCommand(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /*
    struct polar_CResult_c_void *polar_call_result(struct polar_Query *query_ptr,
                                                   uint64_t call_id,
                                                   const char *term);
   */
    public void CallResult(ulong callId, string term)
    {
        Native.CallResult(_handle, callId, term);
    }

    /*
    struct polar_CResult_c_void *polar_question_result(struct polar_Query *query_ptr,
                                                       uint64_t call_id,
                                                       int32_t result);
    */
    public void QuestionResult(ulong callId, int result)
    {
        Native.QuestionResult(_handle, callId, result);
    }

    // struct polar_CResult_c_void *polar_application_error(struct polar_Query *query_ptr, char *message);
    public void ReturnApplicationError(string message)
    {
        Native.ReturnApplicationError(_handle, message);
    }

    // struct polar_CResult_c_char *polar_next_query_message(struct polar_Query *query_ptr);
    // TODO: Turn this into an iterator?
    /*
    public IEnumerator<string> Messages
    {
        get
        {
            // Add error handling to check for error and throw PolarException
        }
    }
    */
    public string? NextMessage()
    {
        return Native.NextQueryMessage(_handle);
    }

    // struct polar_CResult_c_char *polar_query_source_info(struct polar_Query *query_ptr);
    public string? SourceInfo
    {
        get => Native.QuerySourceInfo(_handle);
    }

    /*
    struct polar_CResult_c_void *polar_bind(struct polar_Query *query_ptr,
                                            const char *name,
                                            const char *value);
    */
    public void Bind(string name, string value)
    {
        Native.QueryBind(_handle, name, value);
    }

    public void Dispose()
    {
        _handle.Dispose();
    }
}