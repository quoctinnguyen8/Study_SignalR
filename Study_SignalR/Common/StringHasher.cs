using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Study_SignalR.Common
{
	public static class StringHasher
	{
		// LUÔN GIỮ ĐỘ DÀI CHUỖI = 256
		// KHÔNG ĐƯỢC XÓA/THÊM GÌ VÀO DÒNG NÀY
		// KHÔNG ĐƯỢC ĐỂ XUẤT HIỆN 2 KÝ TỰ GIỐNG NHAU
		const string COLLECTION_CHARS = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789`~!@#$%^&*()_-+={[}]\|;:""\'<,>.?/abcdefghijklmnopqrstuvwxyzđáàảãạăắằẵẳặâấầẩẫậíìỉĩịúùủũụéèẻẽẹêếềễểệóòỏõọôốồổỗộơớờởỡợưứừửữựýỳỷỹỵĐÁÀẢÃẠĂẮẰẴẲẶÂẤẦẨẪẬÍÌỈĨỊÚÙỦŨỤÉÈẺẼẸÊẾỀỄỂỆÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢƯỨỪỬỮỰÝỲỶỸỴƵƶẐẑĎďĆćĈĉČčḨḩḤḥḪḫṰṱṮṯŦŧȾⱦț";

		public static string CreateSalt(int minLength = 20, int maxLength = 50)
		{
			Random rand = new Random();
			char[] salt = new char[rand.Next(minLength, maxLength)];
			for (int i = 0; i < salt.Length; i++)
			{
				salt[i] = COLLECTION_CHARS[rand.Next(COLLECTION_CHARS.Length)];
			}
			return new String(salt);
		}

		/// <summary>
		/// Hash chuỗi & mã hóa dựa theo thuật toán SHA512
		/// </summary>
		/// <returns>Chuỗi sau khi mã hóa, độ dài cố định 64 ký tự</returns>
		public static string HashWith(this string plainText, object salt = null)
		{
			var sha512Hash = new SHA512Managed();
			string strSalt = salt == null ? "" : salt.ToString();
			string mixedString = plainText + strSalt + strSalt.Length.ToString();
			byte[] crypto = sha512Hash.ComputeHash(Encoding.UTF8.GetBytes(mixedString));
			string encodedString = string.Empty;
			foreach (byte theByte in crypto)
			{
				encodedString += COLLECTION_CHARS[(int)theByte % COLLECTION_CHARS.Length];
			}
			return encodedString;
		}
	}
}
