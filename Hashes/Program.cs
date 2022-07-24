
using System.Security.Cryptography;
using System.Text;

string str = "qwerty";
byte[] source = ASCIIEncoding.ASCII.GetBytes(str);
byte[] hashedPassword = new MD5CryptoServiceProvider().ComputeHash(source);
string hashedPasswordString = Convert.ToBase64String(hashedPassword);
Console.WriteLine(hashedPasswordString);