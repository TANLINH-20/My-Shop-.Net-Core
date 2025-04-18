namespace NguyenTanLinh_2122110398.Dtos
{
    public class UserRegisterDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FullName { get; set; }
        public string? Role { get; set; }
        public string? Address { get; set; }
    }
}