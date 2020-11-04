namespace AspNetCore.WebApi.ViewModels
{
    public class BaseResponse<T> where T : class
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}