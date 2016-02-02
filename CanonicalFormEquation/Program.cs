using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// Represents an equation
/// </summary>
public class Equation
{
    #region Fields
    private string strLeftPart;
    private string strRightPart;
    private List<EquationSummand> expandedEquation;
    // Right part of equation
    private EquationExpression leftPart;
    // Left part of the equation
    private EquationExpression rightPart;
    private List<EquationSummand> canonicalEquation;

    // Gets list of tokens

    #endregion

    #region Properties

    /// <summary>
    /// Count of elements in canonical equation
    /// </summary>
    public int CanonicalCount
    {
        get { return canonicalEquation.Count; }
    }

    #endregion

    #region Methods

    public void Parse(string toParse)
    {
        try
        {
            var equation = toParse.Split('=');
            leftPart = new EquationExpression(); 
            rightPart = new EquationExpression();
            strLeftPart = equation[0];
            strRightPart = equation[1];
            Parse(strLeftPart, leftPart);
            Parse(strRightPart, rightPart);
            ToCanonical();
        }
        catch
        {
            Console.WriteLine("Invalid input format");
        }
    }

    public override string ToString()
    {
        return leftPart.ToString() + '=' + rightPart.ToString();
    }
    


    public string Output(EquationExpression exp, bool isRoot = false)
    {
        string ret = isRoot ? "(" : string.Empty;
        foreach (var part in exp.EquationParts)
        {
            var expression = part as EquationExpression;
            var summand = part as EquationSummand;

            if (expression != null)
                ret += Output(expression);
            else
                ret += summand.ToString();
        }
        if (isRoot)
            ret += ")";
        return ret;
    }
    

    /// <summary>
    /// Transforms an expanded equation to canonical form
    /// </summary>
    private void ToCanonical()
    {
        expandedEquation = ExpandEquation();
        canonicalEquation = expandedEquation.GroupBy(e1 => e1.Type)
            .Select(
            summand => new EquationSummand(summand.Sum(s=> s.Amount),summand.Key)
            
            ).Where(s=> s.Amount != 0).OrderBy(s => s.Type, new VariableTypeComparer()).ToList();

        if (canonicalEquation.Count == 0)
            return;

        var first = canonicalEquation.First();
        first.SetFirst(true);

        if (first.Amount != 1)
        {
            var amount = first.Amount;
            foreach (var record in canonicalEquation)
            {
                record.SetAmount(record.Amount / amount);
            }
        }
    }

    /// <summary>
    /// Outputs an equation in canonical form
    /// </summary>
    /// <returns></returns>
    public string OutputCanonical()
    {
        var ret = new StringBuilder();

        foreach (var record in canonicalEquation)
        {
            ret.Append(string.Format("{0} ", record));
        }

        if (canonicalEquation.Count == 0)
            ret.Append("0 ");

        ret.Append("= 0");
        return ret.ToString();
    }
    
    /// <summary>
    /// Expands the equation
    /// </summary>
    /// <returns></returns>
    private List<EquationSummand> ExpandEquation()
    {
        var leftSummands = leftPart.Expand();
        var rightSummands = rightPart.Expand();
        foreach (var summand in rightSummands)
        {
            summand.Sign = -summand.Sign;
        }
        return leftSummands.Concat(rightSummands).ToList();

    }
    /// <summary>
    /// Parses equation string
    /// </summary>
    /// <param name="toParse"></param>
    /// <param name="exp"></param>
    private void Parse(string toParse, EquationExpression exp)
    {
        // Remove all whitespaces from equation string
        var copy = toParse.Replace(" ", string.Empty).Trim();
        int i = 0;
        int currentSign = 1;
        // Next Expression or summand will be first in sequence
        bool first = true;

        while (i < copy.Length)
        {
            var item = copy[i];
            if (item == '(')
            {
                var newExpression = new EquationExpression(currentSign,first);
                first = false;
                exp.AddEquationPart(newExpression);
                int begin = i + 1;
                int bracketsCounter = 1;
                while (bracketsCounter != 0 && i < copy.Length)
                {
                    i++;
                    switch (copy[i])
                    {
                        case '(':
                            bracketsCounter++;
                            break;
                        case ')':
                            bracketsCounter--;
                            break;
                    }
                    
                }
                int shift = i - begin;
                Parse(copy.Substring(begin,shift), newExpression);
                i++;
            }
            else if (IsSign(item))
            {
                currentSign = (item == '+') ? 1 : -1;
                i++;
            }
            else
            {
                var summand = new EquationSummand(currentSign, isFirst: first);
                first = false;
                exp.AddEquationPart(summand);
                i =  ParseEquationSummand(i, copy, summand);
            }
                

        }
    }
    /// <summary>
    /// Initializes EquationSummand object from given string
    /// </summary>
    /// <param name="begin">Start index of summand</param>
    /// <param name="toParse">Expression string to parse</param>
    /// <param name="summand">New summand</param>
    /// <returns></returns>
    private int ParseEquationSummand(int begin, string toParse, EquationSummand summand)
    {
        var i = begin;
        string amount = string.Empty;
        
        Variable currentVariable = null;
        
        while (i < toParse.Length && !IsBracket(toParse[i]) && !IsSign(toParse[i]))
        {
            int shift = 1;
            if (IsFloat(toParse[i]))
            {
                amount += toParse[i];
            }

            else if (IsAlpha(toParse[i]))
            {
                currentVariable = summand.Type.AddVariable(new Variable(toParse[i], 1));
            }
            else if (IsPower(toParse[i]))
            {
                currentVariable.SetPower(ParsePower(toParse.Substring(i + 1), out shift));
            }
            i += shift;
        }
        summand.SetAmount(amount);
        return i;
    }

    private bool IsBracket(char chr)
    {
        return chr == '(' || chr == ')';
    }
    private bool IsInt(char chr)
    {
        return chr >= '0' && chr <= '9';
    }
    private bool IsFloat(char chr)
    {
        return chr >= '0' && chr <= '9' || chr == '.' || chr == ',';
    }
    private bool IsAlpha(char chr)
    {
        return chr >= 'a' && chr <= 'z';
    }
    private bool IsPower(char chr)
    {
        return chr == '^';
    }
    private bool IsSign(char chr)
    {
        return chr == '-' || chr == '+';
    }

    private int ParsePower(string toParse, out int shift)
    {
        var power = string.Empty;
        int i = 0;
        var item = toParse;
        // if power is negative
        if (IsSign(item[i]))
        {
            power += item[i];
            i++;
        }
        while ( i < toParse.Length && IsInt(item[i]))
        {
            power += toParse[i];
            i++;
        }
        shift = i + 1;
        return int.Parse(power);
    }
    #endregion

}


/// <summary>
/// Represents an expression 
/// </summary>
public class EquationExpression : EquationPart
{
    #region Constructor
    public EquationExpression(int sign = 1, bool isFirst = false) :base(sign, isFirst)
    {
        equationParts = new List<EquationPart>();
    }
    #endregion
    
    #region Fields
    private List<EquationPart> equationParts;
    #endregion

    #region Properties
    public List<EquationPart> EquationParts
    {
        get { return equationParts; }
    }
    #endregion

    #region Methods
    public void AddEquationPart(EquationPart part)
    {
        equationParts.Add(part);
    }

    public override string ToString()
    {
        return Output(this, false);
    }


    /// <summary>
    /// Expands given equation expression
    /// </summary>
    /// <param name="exp">expression</param>
    /// <returns></returns>
    public List<EquationSummand> Expand(EquationExpression exp = null)
    {
        // Root expression
        if (exp == null)
            exp = this;
        var ret = new List<EquationSummand>();
        foreach (var part in exp.equationParts)
        {
            var expression = part as EquationExpression;
            var summand = part as EquationSummand;
            if (expression != null)
                ret.AddRange(Expand(expression));
            else
            {
                if (exp.Sign < 0)
                   summand.Sign = -summand.Sign;
               ret.Add(summand);
            }
        }
        return ret;
    }


    /// <summary>
    /// Output an equation expression
    /// </summary>
    /// <param name="exp">Current expression</param>
    /// <param name="needBrackets"></param>
    /// <returns></returns>
    private string Output(EquationExpression exp, bool needBrackets)
    {
        var outputStr = string.Empty;
        if (needBrackets)
            outputStr += "(";
        foreach (var equationPart in exp.equationParts)
        {
            var expression = equationPart as EquationExpression;
            if (expression != null)
            {
                if (!expression.isFirst)
                    outputStr += expression.Sign == 1 ? "+" : "-";
                outputStr += Output(expression,true);
            }
            else
            {
                var summand = equationPart as EquationSummand;
                outputStr += summand.ToString();
            }
        }
        if (needBrackets)
            outputStr += ")";
        return outputStr;
    }
    
    #endregion
}


/// <summary>
/// Represents an equation summand
/// </summary>
public class EquationSummand : EquationPart
{
    #region Constructors
    public EquationSummand(float amount, int sign = 1, bool isFirst = false,SummandType type = null) : base(sign,isFirst)
    {
        this.amount = amount;
    }

    public EquationSummand(float amount, SummandType type)
    {
        sign = Math.Sign(amount);
        this.amount = Math.Abs(amount);
        this.type = type;
    }

    public EquationSummand(int sign = 1, bool isFirst = false) : base(sign,isFirst)
    {
        this.amount = 1.0f;
    }
    #endregion

    #region Fields
    // The amount
    private float amount = 1.0f;
    // Variables present in the summand
    private SummandType type = new SummandType();
    #endregion

    #region Properties
    public float Amount
    {
        get { return amount * sign; }
    }

    public SummandType Type
    {
        get { return type; }
    }
    #endregion

    #region Methods

    public override string ToString()
    {
        string output = string.Empty;
        if (amount != 0)
            output += sign == 1 ? (!isFirst) ? "+ " : "" : "- ";
        output += amount == 1 ? "" : amount.ToString();
        foreach (var variable in Type.Variables)
        {
            output += variable.ToString();
        }
        return output;
    }

    public void SetAmount(string amount)
    {
        float result;
        // if amount string contains dot  
        amount = amount.Replace('.', ',');
        this.amount = (amount == "") ? 1 : float.TryParse(amount,out result) ? result: float.NaN;
    }

    public void SetAmount(float amount)
    {
        this.amount = Math.Abs(amount);
        this.sign = Math.Sign(amount);
    }
    #endregion

    #region Operators

    public static EquationSummand operator +(EquationSummand s1,EquationSummand s2)
    {
        var amount = s1.Amount + s2.Amount;
        var sign = Math.Sign(amount);
        return  new EquationSummand(s1.Amount + s2.Amount, sign, s1.isFirst || s2.isFirst,s1.Type);
    }

    #endregion



}

/// <summary>
/// Represents an equation part (can be a single summand or an expression in brackets)
/// </summary>
public class EquationPart
{
    #region Constructors
    public EquationPart(int sign = 1, bool isFirst = false)
    {
        this.sign = sign;
        this.isFirst = isFirst;
    }
    #endregion

    #region Fields
    // Sign of equation part
    protected int sign;
    // If the part is first in the sequence
    protected bool isFirst;
    #endregion

    #region Properties
    public int Sign
    {
        get { return sign; }
        set { sign = value; }
    }

    public bool IsFirst
    {
        get { return isFirst; }
    }

    #endregion

    #region Methods

    public void SetFirst(bool isFirst)
    {
        this.isFirst = isFirst;
    }
    #endregion
}

/// <summary>
/// Represents a type of a summand
/// </summary>
public class SummandType : IComparable
{
    #region Constructor
    public SummandType()
    {
        variables = new List<Variable>();
    }
    #endregion

    #region Fields
    private List<Variable> variables;
    #endregion

    #region Properties
    public List<Variable> Variables
    {
        get { return variables; }
    }
    #endregion

    #region Methods

    public string GetVariableNames()
    {
        var allNames = string.Empty;
        foreach (var variable in Variables)
        {
            allNames += variable.Name;
        }
        return allNames;
    }

    public int GetMaxDegree()
    {
        var degree = 1;
        foreach (var variable in Variables)
        {
            if (variable.Power > degree)
                degree = variable.Power;
        }
        return degree;
    }

    public override bool Equals(object obj)
    {
        var type = obj as SummandType;
        if (type == null)
            return false;
        var sorted = Variables.OrderByDescending(v => v.Name);
        var objSorted = type.Variables.OrderByDescending(v => v.Name);
        return sorted.SequenceEqual(objSorted);
    }

    public override int GetHashCode()
    {
        int hash = 0;
        foreach (var variable in Variables)
        {
            hash += variable.Name;
        }
        return hash;
    }

    public override string ToString()
    {
        var ret = string.Empty;
        foreach (var variable in Variables)
        {
            ret += variable.ToString();
        }
        return ret;
    }

    public Variable AddVariable(Variable var)
    {
        variables.Add(var);
        return var;
    }

    public int CompareTo(object obj)
    {
        var type = obj as SummandType;
        if (GetMaxDegree() < type.GetMaxDegree())
            return 1;
        else if (GetMaxDegree() > type.GetMaxDegree())
            return -1;
        else
        {
            return GetVariableNames().CompareTo(type.GetVariableNames());
        }
            
    }
    #endregion

}

/// <summary>
/// Represents a variable
/// </summary>
public class Variable
{
    #region Constructor
    public Variable(char type, int power)
    {
        this.name = type;
        this.power = power;
    }
    #endregion

    #region Fields
    private char name;
    private int power;
    #endregion
    
    #region Properties
    public int Power
    {
        get { return power; }
    }

    public char Name
    {
        get { return name; }
    }
    #endregion

    #region Methods
    public override string ToString()
    {
        return name + ((power != 1) ? "^" + power.ToString() : "");
    }

    public override bool Equals(object obj)
    {
        var variable = obj as Variable;
        return name == variable.name && power == variable.power;
    }

    public override int GetHashCode()
    {
        var hash = name.GetHashCode();
        return hash;
    }

    public void SetPower(int power)
    {
        this.power = power;
    }
    #endregion


}

/// <summary>
/// Summand type comparer
/// </summary>
public class VariableTypeComparer : IComparer<SummandType>
{
    public int Compare(SummandType x, SummandType y)
    {
        return x.CompareTo(y);
    }
}

public class Program
{
    private const string InputString = "Please select input method: from (F)ile or (I)nteractive?";
    private const string InputFilename = "Please input filename:";
    private const string InvalidInput = "Invalid input";
    private const string InputEquation = "Please input your equation";
    private const string Example = "Example: \"x^2 + y^2 + 2xy = 0\"";
    private const string FileNotExist = "Specified file does not exist";
    private const string Result = "Result";
    static void Main()
    {
        while(true)
        {
            Console.WriteLine(InputString);
            var key = Console.ReadLine();
            switch (key.ToString())
            {
                case "F":
                case "f":
                    FromFile();
                    break;
                case "I":
                case "i":
                    FromConsole();
                    break;
                default:
                    Console.WriteLine(InvalidInput);
                    break;
            }
        }
    }

    

    private static void FromFile()
    {
        Console.WriteLine(InputFilename);
        var input = Console.ReadLine();
        var equation = new Equation();
        if (File.Exists(input))
        {
            using (var streamWriter = new StreamWriter("output.out"))
            {
                using (var reader = new StreamReader(input))
                {
                    while (reader.Peek() > 0)
                    {
                        equation.Parse(reader.ReadLine());
                        streamWriter.WriteLine(equation.OutputCanonical());
                    }
                }
            }
            Console.WriteLine("Saved to output.out");
        }
        else
            Console.WriteLine(FileNotExist);
    }

    private static void FromConsole()
    {
        Console.WriteLine("{0}\n{1}",InputEquation,Example);
        string str = Console.ReadLine();
        var equation = new Equation();
        equation.Parse(str);
        Console.WriteLine("{0}:\n{1}",Result, equation.OutputCanonical());
    }
}

