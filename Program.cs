using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ConsoleApp1
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
        private static string[] _moves;

        public Rules(string[] moves)
        {
            _moves = moves;
        }

        public int StartPersonMove()
        {
            while (true)
            {
                PrintMoves();
                var personMove = SelectMove();

                if (personMove == 0)
                {
                    ShowRulesInfo();
                }

                var personMoveIndex = personMove - 1;
                if (personMoveIndex < _moves.Length && personMoveIndex >= 0)
                {
                    return personMoveIndex;
                }
            }
        }

        public bool CheckIsPersonWinner(int personMove, int computerMove)
        {
            var movesCont = _moves.Length;
            var fromPersToComp = (movesCont + computerMove - personMove) % movesCont;
            var fromCompToPers = (movesCont + personMove - computerMove) % movesCont;

            return fromPersToComp > fromCompToPers;
        }

        private static void PrintMoves()
        {
            for (var i = 0; i < _moves.Length; i++)
            {
                Console.WriteLine($"  {i + 1} {_moves[i]}");
            }

            Console.WriteLine("");
            Console.WriteLine("  0 help");
        }

        private static int SelectMove()
        {
            int personMove;

            do
            {
                Console.WriteLine("Select a move");
            } while (!int.TryParse(Console.ReadLine(), out personMove));

            return personMove;
        }

        private static void ShowRulesInfo()
        {
            Console.WriteLine("rules help!!!!");
            Console.WriteLine("Press enter to continue");
            Console.ReadLine();
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
            RNGCryptoServiceProvider rngCsp  = new RNGCryptoServiceProvider();
            var salt = new byte[length];
            rngCsp .GetBytes(salt);
            return salt;
        }

        private static byte[] GenHMAC(string str)
        {
            var hmac = new HMACSHA256(salt);
            var bstr = Encoding.UTF8.GetBytes(str);
            return hmac.ComputeHash(bstr);
            ;
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

        private static void StartGame(string[] moves)
        {
            var rules = new Rules(moves);

            // var hmac = new HMACHash(moves[0]);
            // var hmachash = hmac.GetHMAC();
            // var hmacSalt = hmac.GetSalt();
            //
            // Console.WriteLine($"HMAC: {hmachash}");
            // Console.WriteLine($"HMAC key: {hmacSalt}");

            // var pm = 5;
            // var cm = 0;
            // var isWin = rules.CheckIsPersonWinner(pm, cm);
            // Console.WriteLine(isWin);

            // var personMoveIndex = rules.StartPersonMove();
            // Console.WriteLine(personMoveIndex);
            
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            byte[] randomByte = new byte[1];
            rngCsp.GetBytes(randomByte);
            var randomNum = randomByte[0] % 5;
            Console.WriteLine(randomNum);
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
            var isRepeat = moves.Distinct().Count() != moves.Count;

            return isOddNumberMoreOne && !isRepeat;
        }
    }
}