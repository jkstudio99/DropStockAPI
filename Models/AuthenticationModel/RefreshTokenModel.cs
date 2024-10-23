using System;

namespace DropStockAPI.Models;

public class RefreshTokenModel
{
    public int Id { get; set; } // เพิ่ม Id หรือ Primary Key สำหรับ RefreshToken
    public string Token { get; set; } = string.Empty; // Refresh Token
    public DateTime ExpiryDate { get; set; } // วันหมดอายุ
    public string Username { get; set; } = string.Empty; // รหัสผู้ใช้ที่เกี่ยวข้อง
}

