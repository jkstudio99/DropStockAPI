using System;

namespace DropStockAPI.Models.AuthenticationModel;

public class RefreshToken
{
    public int Id { get; set; } // Primary Key
    public string Token { get; set; } = string.Empty; // Refresh Token
    public DateTime ExpiryDate { get; set; } // วันหมดอายุ
    public string Username { get; set; } = string.Empty; // รหัสผู้ใช้ที่เกี่ยวข้อง
}
