namespace BootstrapBlazor.Server.Data;
public class ResponseHttpBase<T>
    {
        public T Data { get; set; } = default(T);
        public int Total { get; set; } = 0;
        public bool Status { get; set; } = true;
        public string? Message  { get; set; } = null;
    }

    public class ResponseHttpBaseBool : ResponseHttpBase<bool> { }
    public class ResponseHttpBaseInt : ResponseHttpBase<int> { }
    public class ResponseHttpBaseString : ResponseHttpBase<string> { }
    public class ResponseHttpBaseObject : ResponseHttpBase<object> { }
    public class ResponseHttpBaseList<T> : ResponseHttpBase<List<T>> { }


