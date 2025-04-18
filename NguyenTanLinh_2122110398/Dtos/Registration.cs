namespace NguyenTanLinh_2122110398.Dtos
{
    public class Registration
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public required string Password { get; set; }
        public string? Address { get; set; }
        public required string Role { get; set; }
    }
}