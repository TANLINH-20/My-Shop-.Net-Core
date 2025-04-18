namespace NguyenTanLinh_2122110398.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public string? Image { get; set; }
        public string? Address { get; set; }
        public required string Role { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}