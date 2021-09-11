using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace RockPaperScissors
{
    internal class Helper
    {
        public static string GetLowerHexFromBytes(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();
        }
    }
    
    internal class ConsoleTable
    {
    }

    internal class Rules
    {
        private string[] moves;
        
        public Rules(string[] moves)
        {
            this.moves = moves;
        }

        public bool CheckIsPersonWinner(string personMove, int computerMove)
        {
            
        }
    }

    internal class HMACHash
    {
        private static byte[] salt;
        private static byte[] hmac;

        public HMACHash(string text, int saltLength = 128)
        {
            salt = GenSalt(saltLength);
            hmac = GenHMAC(text);
        }

        public string GetSalt()
        {
            return Helper.GetLowerHexFromBytes(salt);
        }

        public string GetHMAC()
        {
            return Helper.GetLowerHexFromBytes(hmac);
        }

        private static byte[] GenSalt(int length)
        {
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            var salt = new byte[length];
            crypto.GetBytes(salt);
            return salt;
        }

        private static byte[] GenHMAC(string str)
        {
            var hmac = new HMACSHA256(salt);
            var bstr = Encoding.UTF8.GetBytes(str);
            return hmac.ComputeHash(bstr);;
        }
    }

    internal static class Program
    {
        public static void Main(string[] args)
        {
            if (CheckMoves(args))
            {
                StartGame(args);
            }
            else
            {
                PrintMessageOfWrong();
            }
        }

        private static void StartGame(IList<string> moves)
        {
            var hmac = new HMACHash(moves[0]);
            var hmachash = hmac.GetHMAC();
            var hmacSalt = hmac.GetSalt();

            Console.WriteLine($"HMAC: {hmachash}");
            Console.WriteLine($"HMAC key: {hmacSalt}");
        }

        private static void PrintMessageOfWrong()
        {
            string[] errorMessages =
            {
                "",
                "Wrong moves.",
                "the number of moves is less than one || an even number of moves || the moves are repeated",
                "Correctly: rock paper scissors || rock paper scissors lizard Spock || 1 2 3 4 5",
            };

            foreach (var message in errorMessages)
            {
                Console.WriteLine(message);
            }
        }

        private static bool CheckMoves(ICollection<string> moves)
        {
            var isOddNumberMoreOne = moves.Count > 1 && moves.Count % 2 == 1;
            var isRepeat = moves.Distinct().Count() != moves.Count();

            return isOddNumberMoreOne && !isRepeat;
        }
    }
}