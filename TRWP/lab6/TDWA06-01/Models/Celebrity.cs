namespace TDWA06_01.Models;

public sealed class Celebrity
{
    public int Id { get; set; }
    public string Fullname { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string? ReqPhotoPath { get; set; }
}
