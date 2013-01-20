using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Zeta.Common;
using Zeta.Internals.Actors;

namespace GilesTrinity
{
    public static class HashGenerator
    {
        /*
         * Reference implimentation: http://msdn.microsoft.com/en-us/library/s02tk69a.aspx
         */

        /// <summary>
        /// Generates an SHA1 Hash given the dropped item parameters
        /// </summary>
        /// <param name="position">The Vector3 position of hte item</param>
        /// <param name="actorSNO">The ActorSNO of the item</param>
        /// <param name="name">The Name of the item</param>
        /// <param name="worldID">The current World ID</param>
        /// <param name="itemQuality">The ItemQuality of the item</param>
        /// <param name="itemLevel">The Item Level</param>
        public static string GenerateItemHash(Vector3 position, int actorSNO, string name, int worldID, ItemQuality itemQuality, int itemLevel)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                string itemHashBase = String.Format("{0}{1}{2}{3}{4}{5}", position, actorSNO, name, worldID, itemQuality, itemLevel);
                string itemHash = GetSha1Hash(sha1, itemHashBase);
                return itemHash;
            }
        }

        /// <summary>
        /// Generates an SHA1 hash of a particular GilesObject
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GenerateGilesObjecthash(GilesObject obj)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                string objHashBase = String.Format("{0}{1}{2}{3}", obj.ActorSNO, obj.Position, obj.Type, GilesTrinity.iCurrentWorldID);
                string objHash = GetSha1Hash(sha1, objHashBase);
                return objHash;
            }
        }
        static string GetSha1Hash(SHA1 sha1Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash. 
            byte[] data = sha1Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }
        // Verify a hash against a string. 
        static bool VerifySha1Hash(SHA1 sha1Hash, string input, string hash)
        {
            // Hash the input. 
            string hashOfInput = GetSha1Hash(sha1Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
