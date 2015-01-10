/* --------------------------------------------------------------------------------------------
 * Compilateur Brainfuck de Scriptopathe
 * Fait pour FunkyWork !
 * ------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
namespace Brainfuck
{


    /// <summary>
    /// Classe d'exception lancée lors d'une erreur de syntaxe brainfuck.
    /// </summary>
    public class BrainfuckSyntaxErrorException : Exception
    {
        public BrainfuckSyntaxErrorException(string message)
            : base(message)
        {

        }
    }

    /// <summary>
    /// Compilateur brainfuck.
    /// </summary>
    public class Compiler
    {
        #region Variables
        /// <summary>
        /// Le tableau de valeurs
        /// </summary>
        byte[] m_arr;
        /// <summary>
        /// "Pointeur" sur le tableau.
        /// </summary>
        byte m_index = 0;
        /// <summary>
        /// Method info correspondant à Console.Write(char c)
        /// </summary>
        MethodInfo m_writeChar;
        /// <summary>
        /// Method info correspondant à Console.Read()
        /// </summary>
        MethodInfo m_readLine;
        #endregion

        #region Properties
        public byte Index
        {
            get { return m_index; }
            set { m_index = value; }
        }
        public byte[] Array
        {
            get { return m_arr; }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Constructeur. Initialise des MethodInfo utilisés 
        /// par le code IL généré par CompileString.
        /// </summary>
        public Compiler()
        {
            m_writeChar = typeof(Compiler).GetMethod("WriteChar");
            m_readLine = typeof(Compiler).GetMethod("ReadLine");
            m_arr = new byte[byte.MaxValue];
        }
        /// <summary>
        /// Prépare/Reset les valeurs pour l'interprétation.
        /// </summary>
        public void Start()
        {
            for (int i = 0; i < m_arr.Count(); i++)
            {
                m_arr[i] = 0;
            }
            m_index = 0;
        }
        /// <summary>
        /// Compile et exécute une chaine de code brainfuck.
        /// </summary>
        /// <param name="str"></param>
        /// <returns>l'index final retourné par la méthode</returns>
        public int ExecuteString(string str)
        {
            DynamicMethod method = CompileString(str);
            return ExecuteMethod(method);
        }
        /// <summary>
        /// Exécute une dynamic method compilée.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public int ExecuteMethod(DynamicMethod method)
        {
            int ret = (int)method.Invoke(null, new object[] { m_arr, m_index });
            return ret;
        }
        /// <summary>
        /// Exécute une dynamic method compilée, avec le tableau de bytes donné comme tableau, et
        /// l'index spécifié.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public int ExecuteMethod(DynamicMethod method, byte[] arr, byte index)
        {
            int ret = (int)method.Invoke(null, new object[] { arr, index });
            return ret;
        }
        /// <summary>
        /// Compile une chaine de caractère entière.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public DynamicMethod CompileString(string str)
        {
            return CompileString(str, 0, str.Count());
        }
        /// <summary>
        /// Compile du code brainfuck  (str), à partir de la position de départ start
        /// jusqu'à la position de fin end.
        /// </summary>
        public DynamicMethod CompileString(string str, int start, int end)
        {
            // La méthode dynamique prendra comme argument l'array et l'index, et
            // retournera le nouvel index.
            DynamicMethod method = new DynamicMethod("lol", typeof(int), new Type[] { typeof(byte[]), typeof(byte) });
            // on a au max 10 instructions par caractère, compter le nombre
            // de '+', etc... nuirait gravement à la perf.
            ILGenerator il = method.GetILGenerator(25*str.Count()); 
            
            // L'index est stocké dans la locale 0
            il.DeclareLocal(typeof(int)); // contiendra l'index
            il.DeclareLocal(typeof(int)); // je ne m'en sers plus, mais j'ai la flemme de le supprimer
            il.DeclareLocal(typeof(bool)); // pour les conditions

            // Ici on met l'index de base dans la variable locale 0
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stloc_0);

            // Variable locale : index de la pile
            for (int i = start; i < end; i++)
            {
                int indexAcc;
                switch (str[i])
                {
                    case '>':
                        indexAcc = 1;
                        goto IndexAccFinalize;
                    case '<':
                        indexAcc = -1;
                        // Check for others > or <
                        int l;
                        for (l = i + 1; l < end; l++)
                        {
                            if (str[l] == '>')
                                indexAcc++;
                            else if (str[l] == '<')
                                indexAcc--;
                            else if (str[l] == ']' || str[l] == '[' || str[l] == '-' ||
                                str[l] == '+' || str[l] == '.' || str[l] == ',')
                                break;
                        }
                        // Go to the first instruction after all the consecutive '+' and '-'
                        i = l - 1;
                        // If the total '>' and '<' are 0 then break the loop.
                        if (indexAcc == 0)
                            break;
                    IndexAccFinalize:
                        il.Emit(OpCodes.Ldloc_0);  // index dans pile
                        il.Emit(OpCodes.Ldc_I4, Math.Abs(indexAcc)); // 1 dans pile
                        if (indexAcc < 0)
                            il.Emit(OpCodes.Sub);  // Différence des deux
                        else
                            il.Emit(OpCodes.Add);  // Ajout
                        il.Emit(OpCodes.Stloc_0);  // Resultat dans locale 0 (index)
                        break;
                    case '+':
                        indexAcc = 1;
                        goto ArrayOpFinalize;
                    case '-':
                        indexAcc = -1;

                    ArrayOpFinalize:
                        // Check for others + or -
                        int k;
                        for (k = i+1; k < end; k++)
                        {
                            if (str[k] == '+')
                                indexAcc++;
                            else if (str[k] == '-')
                                indexAcc--;
                            else if (str[k] == ']' || str[k] == '[' || str[k] == '<' ||
                                str[k] == '>' || str[k] == '.' || str[k] == ',')
                                break;
                        }
                        // On va à la première instruction après tous les '+' et '-' consécutifs.
                        i = k-1;

                        // Si le nombre de '+' et '-' total est null, on casse la boucle.
                        if (indexAcc == 0)
                            break;

                        // Allez on s'accroche c'est partit !
                        il.Emit(OpCodes.Ldarg_0); // array
                        il.Emit(OpCodes.Ldloc_0); // index
                        il.Emit(OpCodes.Ldelema, typeof(byte)); // index en tant qu'adresse du tableau.
                        // duplicata de la valeur actuellement sur la pile (adresse de index) car on en aura besoin après.
                        il.Emit(OpCodes.Dup);   
                        il.Emit(OpCodes.Ldobj, typeof(byte)); // copie de la valeur -> pile
                        il.Emit(OpCodes.Ldc_I4, Math.Abs(indexAcc)); // 1 -> pile
                        if (indexAcc > 0)
                            il.Emit(OpCodes.Add); // on ajoute les 2 -> pile
                        else
                            il.Emit(OpCodes.Sub);
                        il.Emit(OpCodes.Conv_U1); // on converti ce byte en int
                        // Les dépassement de capacité sont donc générés dans la limite de l'int32
                        // on fait la copie notre valeur (on a bien les bons arguments sur la pile à cause du duplicata
                        // de l'adresse du tableau)
                        il.Emit(OpCodes.Stobj, typeof(byte)); 
                        break;
                    case '[':
                        // On cherche le ']' de fermeture
                        int depth = 0;
                        int nextBracket = -1;
                        for (int j = i+1; j < end; j++)
                        {
                            if (str[j] == '[')
                                depth++;
                            else if (str[j] == ']')
                                if (depth == 0)
                                {
                                    nextBracket = j;
                                    break;
                                }
                                else
                                    depth--;
                        }
                        if (nextBracket == -1)
                        {
                            throw new BrainfuckSyntaxErrorException("Erreur de syntaxe : apprends a brainfucker pauv' tache");
                        }

                        // La method qui sera appelée : on compile le bout de code qui est à l'intérieur
                        // du coup on ne l'évalue les caractères qu'une seule fois pour tous les tours de boucle.
                        // En gros on compile chaque boucle et ensuite on lance le tout !
                        DynamicMethod d = CompileString(str, i+1, nextBracket);
                        // On skippe les autres caractères jusqu'à la prochaine parenthèse.
                        i = nextBracket;

                        // Définition des étiquettes
                        Label methodCall = il.DefineLabel();
                        Label condition = il.DefineLabel();

                        il.Emit(OpCodes.Br_S, condition); // saute à la vérification de la condition
                        // La on fait l'appel de la méthode.
                        il.MarkLabel(methodCall);
                        il.Emit(OpCodes.Ldarg_0); // array
                        il.Emit(OpCodes.Ldloc_0); // index
                        il.Emit(OpCodes.Call, d); // on appelle la méthode, le résultat est le nouvel index et va dans la pile
                        il.Emit(OpCodes.Stloc_0); // on met l'index dans la loc 0
                        // A partir de là, c'est la condition.
                        il.MarkLabel(condition);
                        il.Emit(OpCodes.Ldarg_0); // array sur la pile
                        il.Emit(OpCodes.Ldloc_0); // index sur la pile (faut les mettre dans l'ordre c'pour ça)
                        il.Emit(OpCodes.Ldelem_U1); // on prend l'elem index de l'array et on le fout sur la pile
                        il.Emit(OpCodes.Ldc_I4_0); // on met 0 sur la pile
                        il.Emit(OpCodes.Ceq); // comparaison de l'index avec 0
                        il.Emit(OpCodes.Brfalse_S, methodCall); // si index = 0, on termine, sinon, on revient à l'appel de la méthode imbriquée
                        break;
                    case ']':
                        // Normalement on doit pas rencontrer ça.
                        throw new BrainfuckSyntaxErrorException("Erreur de syntaxe : apprends a brainfucker pauv' tache");
                    case ',':
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Call, m_readLine);
                        il.Emit(OpCodes.Stelem_I1); // remplace la valeur tableau[index] par la valeur lue dans read line
                        break;
                    case '.':
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Call, m_writeChar);
                        break;
                }
            }
            // Pof, on fout l'"index" sur la pile et on le retourne et l'enfer est fini.
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            return method;
        }
        #endregion

        #region Public Methods pour IL
        /// <summary>
        /// Méthode permettant d'écrire un caractère à partir du tableau de bytes et
        /// de l'index.
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="index"></param>
        public static void WriteChar(byte[] arr, byte index)
        {
            Console.Write(char.ConvertFromUtf32(arr[index]));
        }
        /// <summary>
        /// Méthode qui fait directement la conversion pour le readline.
        /// Pas de vérification sinon c'trop lent.
        /// </summary>
        /// <returns></returns>
        public static int ReadLine()
        {
            return byte.Parse(Console.ReadLine());
        }
        #endregion
    }
}