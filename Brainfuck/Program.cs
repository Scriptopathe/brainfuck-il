using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace Brainfuck
{
    /// <summary>
    /// Classe qui contient le point d'entrée du programme.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Point d'entrée.
        /// </summary>
        static void Main(string[] args)
        {
            if (false)
            {
                Interactive();
            }
            else if (false)
            {
                BenchMarkFibbo();
            }
            else
            {
                string str = @">,>,<<
>[<+>-]>[<<+>>-]<<
[>+>+<<-]
>[----------<+>]<	
>>>++++++[<<<++++++++>>>-]<<<
>>>++++++[<<++++++++>>-]<<<
.>.";
                Compiler i = new Compiler();
                while (true)
                {
                    Console.WriteLine("Enter 2 numbers :");
                    i.Start();
                    i.ExecuteString(str);
                    Console.ReadLine();
                    Console.WriteLine();
                }

            }
        }
        static void Interactive()
        {
            Console.Write("Brainfuck -> ");
            Compiler inter = new Compiler();
            bool printState = true;
            while (true)
            {
                string str = Console.ReadLine();
                if (str == "#reset")
                {
                    inter.Start();
                    Console.Write("out: OK (reset)");
                }
                else if (str == "#printstate on")
                {
                    printState = true;
                    Console.Write("out: OK (printstate on)");
                }
                else if (str == "#printstate off")
                {
                    printState = false;
                    Console.Write("out: OK (printstate off)");
                }
                else
                {
                    Console.Write("out: ");
                    inter.Index = (byte)inter.ExecuteString(str);
                    // State
                    if (printState)
                        Console.Write("\nState : index = {0}, arr[index] = {1}",
                                        inter.Index, inter.Array[inter.Index]);
                }

                // Prompt
                Console.Write("\nBrainfuck -> ");

            }
        }
        static void BenchMarkFibbo()
        {
            Compiler inter = new Compiler();
            // Fibbo à 100 sans l'affichage plein de Console.Write() ça ralentit.
            string evalStr = @"+++++++++++>+>>>>++++++++++++++++++++++++++++++++++++++++++++>++++++++++++++++++++++++++++++++<<<<<<[>[>>>>>>+>+<<<<<<<-]>>>>>>>[<<<<<<<+>>>>>>>-]<[>++++++++++[-<-[>>+>+<<<-]>>>[<<<+>>>-]+<[>[-]<[-]]>[<<[>>>+<<<-]>>[-]]<<]>>>[>>+>+<<<-]>>>[<<<+>>>-]+<[>[-]<[-]]>[<<+>>[-]]<<<<<<<]>>>>>[++++++++++++++++++++++++++++++++++++++++++++++++[-]]++++++++++<[->-<]>++++++++++++++++++++++++++++++++++++++++++++++++[-]<<<<<<<<<<<<[>>>+>+<<<<-]>>>>[<<<<+>>>>-]<-[>>><<<[-]]<<[>>+>+<<<-]>>>[<<<+>>>-]<<[<+>-]>[<+>-]<<<-]";

            // Ou : string evalStr = Console.ReadLine();
            DynamicMethod method;
            try
            {
                // On compile notre code
                DateTime t1 = DateTime.Now;
                method = inter.CompileString(evalStr);
                TimeSpan elapsed1 = DateTime.Now - t1;
                Console.WriteLine("Compilation time : {0} ms", elapsed1.Milliseconds);
            }
            catch (BrainfuckSyntaxErrorException e)
            {
                // Erreur de syntaxe à cause des crochets.
                Console.WriteLine(e.Message);
                Console.WriteLine("\n-----\nExecution end");
                Console.ReadLine();
                return;
            }
            // Chtite benchmark.
            int loops = 1000;
            // Initialization of the interpreter's state.
            byte[][] arrs = new byte[loops][];
            for (int j = 0; j < loops; j++)
            {
                arrs[j] = new byte[byte.MaxValue];
            }

            DateTime t = DateTime.Now;
            for (int i = 0; i < loops; i++)
            {
                inter.ExecuteMethod(method, arrs[i], 0);
            }
            TimeSpan elapsed = DateTime.Now - t;
            // Et là, cay la fin.
            Console.WriteLine("\n-----\nExecution time : {0} ms for {1} loops", elapsed.Milliseconds, loops);
            Console.ReadLine();
        }
    }
}
