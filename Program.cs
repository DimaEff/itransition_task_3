using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ConsoleTableExt;


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
        private static List<List<object>> _tableData;

        public ConsoleTable(string[] moves)
        {
            _tableData = new List<List<object>>();
            AddFirstRow(moves);
            AddRowsFromSecond(moves);
        }

        public void PrintTable()
        {
            ConsoleTableBuilder
                .From(_tableData)
                .WithFormat(ConsoleTableBuilderFormat.Alternative)
                .ExportAndWriteLine();
        }

        private static void AddFirstRow(string[] moves)
        {
            var firstRow = new List<object> {"Person↓ | Computer→"};
            foreach (var move in moves)
            {
                firstRow.Add(move);
            }

            _tableData.Add(firstRow);
        }

        private static void AddRowsFromSecond(string[] moves)
        {
            for (int i = 0; i < moves.Length; i++)
            {
                var row = new List<object> {$"{moves[i]}"};
                for (var n = 0; n < moves.Length; n++)
                {
                    var winnerIndex = Rules.GetWinnerIndex(i, n, moves.Length);
                    row.Add($"{Rules.GetPersonGameResult(i, n, winnerIndex)}");
                }

                _tableData.Add(row);
            }
        }
    }

    internal class Rules
    {
        private static string[] _moves;
        private static int _personMove;
        private static int _computerMove;
        private static int _winnerIndex;

        public Rules(string[] moves)
        {
            _moves = moves;
        }

        public int GetComputerMove()
        {
            return _computerMove;
        }

        public int GetPersonMove()
        {
            return _personMove;
        }

        public void DoComputerMove()
        {
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            byte[] randomByte = new byte[1];
            rngCsp.GetBytes(randomByte);
            var randomNum = randomByte[0] % _moves.Length;

            _computerMove = randomNum;
        }

        public void DoPersonMove()
        {
            while (true)
            {
                PrintMoves();
                var personSelectedMove = SelectPersonMove();

                if (personSelectedMove == 0)
                {
                    ShowRulesInfo();
                }

                var personMoveIndex = personSelectedMove - 1;
                if (personMoveIndex < _moves.Length && personMoveIndex >= 0)
                {
                    _personMove = personMoveIndex;
                    break;
                }
            }
        }

        public void CheckWinner()
        {
            _winnerIndex = GetWinnerIndex(_personMove, _computerMove, _moves.Length);
        }

        public static int GetWinnerIndex(int personMove, int computerMove, int movesLength)
        {
            var fromPersToComp = (movesLength + computerMove - personMove) % movesLength;
            var fromCompToPers = (movesLength + personMove - computerMove) % movesLength;

            return fromPersToComp > fromCompToPers ? personMove : computerMove;
        }

        public void PrintSelectedMoves()
        {
            Console.WriteLine($"Computer`s move: {_moves[_computerMove]}");
            Console.WriteLine($"Your move: {_moves[_personMove]}");
        }

        public static string GetPersonGameResult(int personMove, int computerMove, int winnerIndex)
        {
            if (personMove == computerMove)
            {
                return "draw";
            }

            return personMove == winnerIndex ? "win" : "lose";
        }
        
        public void PrintWinner()
        {
            var result = GetPersonGameResult(_personMove, _computerMove, _winnerIndex);
            if (result == "draw")
            {
                Console.WriteLine("Draw!");
            }
            else
            {
                Console.WriteLine($"You {result}!");
            }
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

        private static int SelectPersonMove()
        {
            int personMove;

            do
            {
                Console.WriteLine("Select a move");
            } while (!int.TryParse(Console.ReadLine(), out personMove));

            _personMove = personMove;
            return personMove;
        }

        private static void ShowRulesInfo()
        {
            var table = new ConsoleTable(_moves);
            table.PrintTable();
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
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            var salt = new byte[length];
            rngCsp.GetBytes(salt);
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

            rules.DoComputerMove();
            var computerMove = rules.GetComputerMove();
            var hmac = new HMACHash(moves[computerMove]);
            var hmachash = hmac.GetHMAC();
            var salt = hmac.GetSalt();
            Console.WriteLine($"HMAC(hex): {hmachash}");
            
            rules.DoPersonMove();

            rules.CheckWinner();
            rules.PrintSelectedMoves();
            rules.PrintWinner();

            Console.WriteLine($"HMAC key: {salt}");
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