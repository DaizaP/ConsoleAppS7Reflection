using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ConsoleAppS7Reflection
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //изи мод он
            //string easyString = JsonSerializer.Serialize(new TestClass(1, "str", 2.0m, ['A', 'B', 'C']));
            //TestClass easyClass = JsonSerializer.Deserialize<TestClass>(easyString);
            //Console.WriteLine(easyClass.S);


            string stringClass = CustomClassToString(new TestClass(1, "str", 2.0m, ['A', 'B', 'C']));
            Console.WriteLine(stringClass);

            TestClass obj = (TestClass)StringToClass(stringClass);
            Console.WriteLine(obj.C);
            Console.WriteLine(obj.D);
            Console.WriteLine(obj.I);
            Console.WriteLine(obj.S);

        }
        public static string CustomClassToString(object obj)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(obj.GetType().FullName).Append('|');
            sb.Append(obj.GetType().GetConstructors().LastOrDefault()).Append('|');

            var val = JsonSerializer.Serialize(obj);
            Dictionary<string, object> dict = JsonSerializer.Deserialize<Dictionary<string, object>>(val);
            foreach (var value in dict)
            {
                sb.Append(value.Value);
                if (value.Value != dict.Last().Value)
                {
                    sb.Append('/');
                }
            }
            return sb.ToString();
        }

        public static object StringToClass(string input)
        {
            string[] info = input.Split('|');
            Type myClass = Type.GetType(info[0]);
            List<Type> typeParamList = myGetTypeParamInConstructor(info[1]);
            List<string> paramValueList = myGetValuesInProperties(info[2]);
            List<object> resObject = myGetResObject(typeParamList, paramValueList);

            ConstructorInfo[] constructor = myClass.GetConstructors();

            object userInstance = constructor.LastOrDefault().Invoke(resObject.ToArray());


            return userInstance;
        }

        public static List<Type> myGetTypeParamInConstructor(string input)
        {

            List<Type> types = new List<Type>();
            input = input.Remove(0, input.IndexOf('(') + 1);
            input = input.Remove(input.IndexOf(')'), 1);
            foreach (string type in input.Split(", "))
            {
                if (type.Contains("System.") == false)
                {
                    string newType = "System." + type;
                    types.Add(Type.GetType(newType));
                }
                else
                {

                    types.Add(Type.GetType(type));
                }

            }
            return types;
        }
        public static List<string> myGetValuesInProperties(string input)
        {
            List<string> result = [.. input.Split('/')];
            return result;
        }
        public static List<object> myGetResObject(List<Type> type, List<string> values)
        {
            //Два дня пытался разобраться как сделать по человески приведение любой строки в любой объект на основе типа. 
            //Сделал как есть. Универсальный метод у меня не вышел/
            List<object> res = new List<object>();
            for (int i = 0; i < values.Count; i++)
            {
                if (type[i] == Type.GetType("System.Char[]"))
                {
                    List<char> chars = new List<char>();
                    string str = values[i];
                    str = str.Remove(0, str.IndexOf('[') + 1);
                    str = str.Remove(str.IndexOf(']'), 1);
                    string[] valArray = str.Split(",");
                    foreach (var val in valArray)
                    {
                        char[] newChars = val.ToCharArray();

                        chars.Add(newChars[1]);
                    }
                    res.Add(chars.ToArray());
                }
                else if (type[i] == Type.GetType("System.Int32"))
                {
                    res.Add(int.Parse(values[i]));
                }
                else if (type[i] == Type.GetType("System.Decimal"))
                {
                    decimal d = Convert.ToDecimal(values[i], CultureInfo.InvariantCulture);
                    res.Add(d);
                }
                else if (type[i] == Type.GetType("System.String"))
                {
                    res.Add(values[i]);
                }
            }
            return res;
        }

    }
}
