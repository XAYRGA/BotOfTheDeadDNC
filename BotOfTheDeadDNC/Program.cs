using System;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using xayrga.binutils;
using WindowsInput;
using WindowsInput.Native;

namespace BotOfTheDeadDNC
{
    class Program
    {
        private const int WORDPOINTERS_BASE = 0x008395c4;
        private const int WORDPOINTERS_LENG = 0x008395fC;

        private static Process ttotdProcess;
        private static ProcessMemoryInterface todInterface;

        private static InputSimulator keySim = new InputSimulator();
        

        static void Main(string[] args)
        {
            Console.Write("Waiting for process....");
            while ((ttotdProcess = getTTODProcess()) == null)
                Thread.Sleep(10);
            Console.WriteLine($"Gotcha! {ttotdProcess.Id:X6}");
            todInterface = new ProcessMemoryInterface(ttotdProcess);
            while (true)
            {
                var words = getWordList();
                for (int i = 0; i < words.Length; i++)
                    Console.WriteLine($"\t{words[i].word}\t0x{words[i].MemoryAddress:X}\t{words[i].proximity}");
                if (words.Length > 0)
                {
                    var bestWord = getBestWord(words);
                    if (bestWord==null)
                    {
                        Thread.Sleep(3);
                        continue;
                    }
                   
                    try
                    {
                        Console.WriteLine($"Selected best word 0x{bestWord.MemoryAddress:X}\t{bestWord.word}");
                        typeWord(bestWord);
                    } catch (Exception E)
                    {
                        Console.WriteLine($"!{E.Message}");
                    }
                }
                Thread.Sleep(3);
            }
        }

        static void typeWord(TTOTDWordObject word)
        {
            Console.Write($"typeWord {word.word} : ");
            var lastWordLen = 0;
            var anythingTyped = false;
            while (true)
            {
                if (word == null)
                    break;
                word.update();
                if (word.typedLength == word.wordLength)
                    break;
                if (word.typedLength < lastWordLen)
                    break;
                lastWordLen = word.typedLength;

                if (!DoesTTODHaveFocus() || !word.enabled)
                {
                    Thread.Sleep(3);
                    continue;
                }
                var wordString = word.word;
                var currentInput = wordString[word.typedLength];
                simulateKeyPress(currentInput);
                Console.Write($"{currentInput}");
                if (word.typedLength + 1 == word.wordLength)
                    break;
                anythingTyped = true;
                word.update();
                Thread.Sleep(35);
            }
            if (anythingTyped==true)
                Console.WriteLine();

        }

        static void simulateKeyPress(char key)
        {
            switch (key)
            {
                case ',':
                    keySim.Keyboard.KeyPress(VirtualKeyCode.OEM_COMMA);
                    break;
                case '.':
                    keySim.Keyboard.KeyPress(VirtualKeyCode.OEM_PERIOD);
                    break;
                case '?':
                    keySim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.LSHIFT, VirtualKeyCode.VK_2);
                    break;
                case '!':
                    keySim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.LSHIFT, VirtualKeyCode.VK_1);
                    break;
                case '-':
                    keySim.Keyboard.KeyPress(VirtualKeyCode.OEM_MINUS);
                    break;
                case '\'':
                    keySim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.SHIFT, VirtualKeyCode.OEM_7);
                    break;
                default:
                    keySim.Keyboard.KeyPress((VirtualKeyCode)key);
                    break;
            }

        }

        static Process getTTODProcess()
        {
            var processes = Process.GetProcesses();
            for (int I = 0; I < processes.Length; I++)
                if (processes[I].ProcessName.Contains("Tod_e"))
                    return processes[I];
            return null;
        }

        static TTOTDWordObject getBestWord(TTOTDWordObject[] words)
        {
            // Find the closest word that we can type on and return it
            TTOTDWordObject bestWord = null;
            for (int i = 0; i < words.Length; i++)
                if (words[i].enabled)
                    if (words[i].proximity < (bestWord == null ? 100000 : bestWord.proximity)) // N: Proximity is never over 1
                        bestWord = words[i];
            return bestWord;
        }

        static TTOTDWordObject[] getWordList()
        {
            var count = todInterface.getU32(WORDPOINTERS_LENG);
            int[] wordOPointers = new int[count];
            TTOTDWordObject[] objects = new TTOTDWordObject[count];

            for (int i = 0; i < count; i++)
                wordOPointers[i] = todInterface.getI32(WORDPOINTERS_BASE + (4 * i));
            for (int i=0; i < count; i++)
                objects[i] = TTOTDWordObject.load(wordOPointers[i], todInterface);
            return objects;
        }

        public static bool DoesTTODHaveFocus()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
                return false;       // No window is currently activated
            var procId = ttotdProcess.Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);
            return activeProcId == procId;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
    }
}
