using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet.Emit;
using dnlib.DotNet;
using System.Reflection;
using System.IO;
using dnlib.DotNet.Writer;

namespace Anti_Invoke_Detection
{
    class Program
    {
        public static void WriteTitle()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@"   ____              __         ___       __      __          ");
            Console.WriteLine(@"  /  _/__ _  _____  / /_____   / _ \___ _/ /_____/ /  ___ ____");
            Console.WriteLine(@" _/ // _ \ |/ / _ \/  '_/ -_) / ___/ _ `/ __/ __/ _ \/ -_) __/");
            Console.WriteLine(@"/___/_//_/___/\___/_/\_\\__/ /_/   \_,_/\__/\__/_//_/\__/_/   ");
            Console.WriteLine(@"                                                             ");
        }
        static void Main(string[] args)
        {
            WriteTitle();
            Console.Title = "Anti Invoke Patcher Fixed for Beds 1.4.1 - By Tulip#6359";
            if (asArgs(args))
            {
                try
                {
                    int count = 0;
                    ModuleDefMD md = ModuleDefMD.Load(args[0]);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"[+] Loaded: {args[0]}\n");
                    foreach (TypeDef type in md.GetTypes())
                    {
                        foreach (MethodDef method in type.Methods)
                        {
                            try
                            {
                                if (!method.HasBody && !method.Body.HasInstructions) continue;
                                for (int i = 0; i < method.Body.Instructions.Count; i++)
                                {
                                    if (method.Body.Instructions[i].OpCode == OpCodes.Call && method.Body.Instructions[i].Operand.ToString().Contains("CallingAssembly"))
                                    {
                                        method.Body.Instructions[i].Operand = (method.Body.Instructions[i].Operand = md.Import(typeof(Assembly).GetMethod("GetExecutingAssembly")));
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Console.WriteLine($"[-] Detected CallingAssembly - Method: {method.Name} - Token: {method.MDToken.Raw}");
                                        count++;
                                    }
                                    if (method.Body.Instructions[i].OpCode == OpCodes.Call && method.Body.Instructions[i].Operand.ToString().Contains("EntryAssembly"))
                                    {
                                        method.Body.Instructions[i].Operand = (method.Body.Instructions[i].Operand = md.Import(typeof(Assembly).GetMethod("GetExecutingAssembly")));
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Console.WriteLine($"[-] Detected EntryAssembly - Method: {method.Name} - Token: {method.MDToken.Raw}");
                                        count++;
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[+] Patched {count} invokes");
                    ModuleWriterOptions writerOptions = new ModuleWriterOptions(md);
                    writerOptions.MetaDataOptions.Flags |= MetaDataFlags.PreserveAll;
                    writerOptions.Logger = DummyLogger.NoThrowInstance;
                    NativeModuleWriterOptions NativewriterOptions = new NativeModuleWriterOptions(md);
                    NativewriterOptions.MetaDataOptions.Flags |= MetaDataFlags.PreserveAll;
                    NativewriterOptions.Logger = DummyLogger.NoThrowInstance;
                    string filepath = Path.GetDirectoryName(args[0]) + ((!Path.GetDirectoryName(args[0]).EndsWith("\\")) ? "\\" : null) + Path.GetFileNameWithoutExtension(args[0]) + "_NoAntiInvoke" + Path.GetExtension(args[0]);
                    if (md.IsILOnly)
                        md.Write(filepath, writerOptions);

                    md.NativeWrite(filepath, NativewriterOptions);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[+] Saved at {Path.GetFileNameWithoutExtension(args[0]) + "_NoAntiInvoke" + Path.GetExtension(args[0])}");
                    Console.ReadKey();
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[-] File isn't a valid .NET assembly!");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[-] Please drag and drop a file like De4Dot!");
                Console.ReadKey();
            }
        }
        static bool asArgs(string[] args)
        {
            try
            {
                if (args[0] != "2xx1452851xx821x")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }

}