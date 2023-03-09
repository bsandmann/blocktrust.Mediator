namespace Blocktrust.Mediator.Server.Models;

using System.Text.RegularExpressions;
using Commands.DatabaseCommands.GetConnection;

public static class ShortenedUrlGenerator
{
   public static string Get(string? requestedPartialSlug, Guid shortenedUrlEntityId)
   {
       if (requestedPartialSlug is null)
       {
           requestedPartialSlug = string.Empty;
       }
       else
       {
           requestedPartialSlug = string.Concat("/", requestedPartialSlug);
       }
       return string.Concat("/qr", requestedPartialSlug, "?_oobid=", shortenedUrlEntityId);
   } 
   
   public static string GenerateSlug(this string phrase) 
   { 
       string str = phrase.RemoveAccent().ToLower(); 
       // invalid chars           
       str = Regex.Replace(str, @"[^a-z0-9\s-]", ""); 
       // convert multiple spaces into one space   
       str = Regex.Replace(str, @"\s+", " ").Trim(); 
       // cut and trim 
       str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();   
       str = Regex.Replace(str, @"\s", "-"); // hyphens   
       return str; 
   } 
   
   private static string RemoveAccent(this string txt) 
   { 
       byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(txt); 
       return System.Text.Encoding.ASCII.GetString(bytes); 
   }
}