using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace AspNetCore.WebApi.ViewModels
{
    public class RegisterViewModel
    {
        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [JsonProperty("confirm_password")]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}