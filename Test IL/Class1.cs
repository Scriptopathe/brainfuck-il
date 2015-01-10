using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test_IL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    namespace Interpreter
    {
        class Program
        {                                       // Définition des variables :
            private ushort pointer = 0;         // Le pointeur
            private byte loop = 0;              // Le nombre de boucles imbriquées
            private int reader = 0;             // Le caractère en cours de lecture
            private string answer = "";         // La chaîne de caractères réponse
            byte[] field = new byte[65536];     // Le tableau (autant de cases que le pointeur a de valeurs possibles)
            string code = Console.ReadLine();   // Le code à lire

            private Program()                   // Méthode principale
            {
                while (reader < code.Length)
                {
                    Interpreter(code[reader]);
                    reader++;
                }
                Console.WriteLine(answer);
            }

            private void Interpreter(char charRead) // L'interpréteur
            {
                switch (charRead)
                {
                    case '>':                       // Avancer le pointeur
                        pointer++;
                        break;
                    case '<':                       // Reculer le pointeur
                        pointer--;
                        break;
                    case '-':                       // Diminuer la valeur pointée
                        field[pointer]--;
                        break;
                    case '+':                       // Augmenter la valeur pointée
                        field[pointer]++;
                        break;
                    case '[':                       // Début d'une boucle
                        if (field[pointer] == 0)
                            GoTo(loop);
                        else
                            loop++;
                        break;
                    case ']':                       // Fin d'une boucle
                        if (field[pointer] == 0)
                            loop--;
                        else
                            GoBack(loop);
                        break;
                    case '.':                       // Ajouter un caractère à la chaîne réponse
                        answer += char.ConvertFromUtf32(field[pointer]);
                        break;
                    case ',':                       // Demander une valeur ASCII
                        char asking = Console.ReadKey(true).KeyChar;
                        field[pointer] = (byte)asking;
                        break;
                }
            }

            private void GoTo(byte loopNumber)      // Ignore une boucle si l'octet pointé est nul
            {
                reader++;
                while (loop != loopNumber || code[reader] != ']')
                {
                    if (code[reader] == '[')
                        loop++;
                    else if (code[reader] == ']')
                        loop--;
                    reader++;
                }

            }

            private void GoBack(byte loopNumber)    // Retourne au début de la boucle si l'octet pointé est non nul
            {
                reader--;
                while (loop != loopNumber || code[reader] != '[')
                {
                    if (code[reader] == '[')
                        loop--;
                    else if (code[reader] == ']')
                        loop++;
                    reader--;
                }
            }

            static void Main(string[] args)         // Génère une instance de la classe pour démarrer le programme
            {
                Program programme = new Program();
            }
        }
    }
}
